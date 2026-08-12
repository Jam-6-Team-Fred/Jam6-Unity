using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Light))]
public class ProxyShadowLight : MonoBehaviour
{
	private static List<Camera> s_ignoredCameras = new List<Camera>(8);

	private Light _light;

	private Matrix4x4 _shadowMatrix;

	private Matrix4x4 _shiftRightMatrix;

	private Matrix4x4 _shiftUpMatrix;

	private Matrix4x4[] _worldToShadowMatrixArray = new Matrix4x4[4];

	private Vector2 _cascadeTextureModifier = new Vector2(0.5f, 0.5f);

	private Vector4 _shadowSplitsNear;

	private Vector4 _shadowSplitsFar;

	private Plane[] _frustumPlanes = new Plane[6];

	private RenderTexture _shadowTexture;

	private Material _shadowMaterial;

	private Camera _targetCamera;

	private CommandBuffer _renderCommandBuffer;

	private CommandBuffer _paramsCommandBuffer;

	private int _propID_unity_LightShadowBias;

	private int _propID_OW_WorldToProxyShadowArray;

	private int _propID_LightShadowData;

	private int _propID_proxyLightSplitsNear;

	private int _propID_proxyLightSplitsFar;

	private int _propID_cascadeTexMod;

	private int _propID_cameraPosition;

	private bool _dirty = true;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_shadowMaterial = new Material(Shader.Find("Hidden/ProxyShadowCaster"));
		_targetCamera = null;
		_renderCommandBuffer = new CommandBuffer();
		_renderCommandBuffer.name = "ProxyShadowmap";
		_paramsCommandBuffer = new CommandBuffer();
		_paramsCommandBuffer.name = "ProxyShadowmapParams";
		_propID_proxyLightSplitsNear = Shader.PropertyToID("_proxyLightSplitsNear");
		_propID_proxyLightSplitsFar = Shader.PropertyToID("_proxyLightSplitsFar");
		_propID_cascadeTexMod = Shader.PropertyToID("_proxyTextureModifier");
		_propID_cameraPosition = Shader.PropertyToID("_proxyCameraPosition");
		_propID_unity_LightShadowBias = Shader.PropertyToID("unity_LightShadowBias");
		_propID_OW_WorldToProxyShadowArray = Shader.PropertyToID("OW_WorldToProxyShadowArray");
		_propID_LightShadowData = Shader.PropertyToID("_LightShadowData");
		ProxyShadowSettings.OnShadowDistanceChanged += OnShadowSettingsChanged;
		ProxyShadowSettings.OnShadowTextureSizeChanged += OnShadowSettingsChanged;
		ProxyShadowSettings.OnCascadeDivisionsChanged += OnShadowSettingsChanged;
	}

	private void OnEnable()
	{
		_light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, _renderCommandBuffer);
		_light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, _paramsCommandBuffer);
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(BuildCommandBuffers));
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(ClearCommandBuffers));
	}

	private void OnDisable()
	{
		_light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, _renderCommandBuffer);
		_light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, _paramsCommandBuffer);
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(BuildCommandBuffers));
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(ClearCommandBuffers));
	}

	private void OnDestroy()
	{
		ProxyShadowSettings.OnShadowDistanceChanged -= OnShadowSettingsChanged;
		ProxyShadowSettings.OnShadowTextureSizeChanged -= OnShadowSettingsChanged;
		ProxyShadowSettings.OnCascadeDivisionsChanged -= OnShadowSettingsChanged;
		s_ignoredCameras.Clear();
		if (_shadowTexture != null)
		{
			_shadowTexture.Release();
			UnityEngine.Object.Destroy(_shadowTexture);
			_shadowTexture = null;
		}
	}

	private void OnShadowSettingsChanged()
	{
		_dirty = true;
	}

	private void UpdateCascadeDivisions(int numCascades)
	{
		switch (numCascades)
		{
		case 1:
			_cascadeTextureModifier = new Vector2(1f, 1f);
			break;
		case 2:
			_cascadeTextureModifier = new Vector2(0.5f, 1f);
			break;
		case 3:
			_cascadeTextureModifier = new Vector2(1f / 3f, 1f);
			break;
		default:
			_cascadeTextureModifier = new Vector2(0.5f, 0.5f);
			break;
		}
		_shadowMatrix = new Matrix4x4(new Vector4(0.5f * _cascadeTextureModifier.x, 0f, 0f, 0f), new Vector4(0f, 0.5f * _cascadeTextureModifier.y, 0f, 0f), new Vector4(0f, 0f, 0.5f, 0f), new Vector4(0.5f * _cascadeTextureModifier.x, 0.5f * _cascadeTextureModifier.y, 0.5f, 1f));
		_shiftUpMatrix = new Matrix4x4(new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 1f, 0f, 0f), new Vector4(0f, 0f, 1f, 0f), new Vector4(0f, 0.5f / (0.5f * _cascadeTextureModifier.y), 0f, 1f));
		if (numCascades == 3)
		{
			_shiftRightMatrix = new Matrix4x4(new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 1f, 0f, 0f), new Vector4(0f, 0f, 1f, 0f), new Vector4(1f / 3f / (0.5f * _cascadeTextureModifier.x), 0f, 0f, 1f));
		}
		else
		{
			_shiftRightMatrix = new Matrix4x4(new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 1f, 0f, 0f), new Vector4(0f, 0f, 1f, 0f), new Vector4(0.5f / (0.5f * _cascadeTextureModifier.x), 0f, 0f, 1f));
		}
	}

	private void UpdateLightSplits(ProxyShadowCascade.Division[] cascadeDivisions, float shadowDistance)
	{
		_shadowSplitsNear = new Vector4(0f, 1f, -1f, -1f);
		_shadowSplitsFar = new Vector4(1f, -1f, -1f, -1f);
		for (int i = 0; i < cascadeDivisions.Length; i++)
		{
			_shadowSplitsFar[i] = cascadeDivisions[i].fraction;
			if (i < 3)
			{
				_shadowSplitsNear[i + 1] = cascadeDivisions[i].fraction;
			}
		}
		_shadowSplitsNear *= shadowDistance;
		_shadowSplitsFar *= shadowDistance;
	}

	private void UpdateShadowTexture(int shadowTextureSquareSize)
	{
		int num = Mathf.FloorToInt((float)shadowTextureSquareSize / _cascadeTextureModifier.x);
		int num2 = Mathf.FloorToInt((float)shadowTextureSquareSize / _cascadeTextureModifier.y);
		if (_shadowTexture != null)
		{
			if (_shadowTexture.width == num && _shadowTexture.height == num2)
			{
				return;
			}
			_shadowTexture.Release();
			UnityEngine.Object.Destroy(_shadowTexture);
			_shadowTexture = null;
		}
		_shadowTexture = new RenderTexture(num, num2, 24, RenderTextureFormat.Shadowmap, RenderTextureReadWrite.Linear);
		_shadowTexture.name = "ProxyShadowmap";
		_shadowTexture.hideFlags = HideFlags.HideAndDontSave;
		_shadowTexture.filterMode = FilterMode.Bilinear;
		_shadowTexture.useMipMap = false;
		_shadowTexture.wrapMode = TextureWrapMode.Clamp;
		_shadowTexture.Create();
		_shadowTexture.SetGlobalShaderProperty("_ProxyShadowMapTexture");
	}

	private void BuildCommandBuffers(Camera camera)
	{
		if (!base.enabled || camera.cameraType == CameraType.Preview || QualitySettings.shadows == UnityEngine.ShadowQuality.Disable || ProxyShadowSettings.numCascades == 0 || s_ignoredCameras.Contains(camera))
		{
			return;
		}
		if (_targetCamera != null)
		{
			Debug.LogError("Tried to BuildCommandBuffers for Camera " + camera.name + " but CommandBuffers have already been built for " + _targetCamera.name + "!", this);
			return;
		}
		_targetCamera = camera;
		List<ProxyShadowCasterSuperGroup> groupList = ProxyShadowCasterSuperGroup.GetGroupList();
		if (groupList.Count == 0)
		{
			return;
		}
		float shadowDistance = ProxyShadowSettings.shadowDistance;
		float bias = ProxyShadowSettings.bias;
		int shadowTextureSquareSize = ProxyShadowSettings.shadowTextureSquareSize;
		ProxyShadowCascade.Division[] cascadeDivisions = ProxyShadowSettings.cascadeDivisions;
		int num = cascadeDivisions.Length;
		if (_dirty)
		{
			UpdateCascadeDivisions(num);
			UpdateLightSplits(cascadeDivisions, shadowDistance);
			UpdateShadowTexture(shadowTextureSquareSize);
			_dirty = false;
		}
		Vector2 position = new Vector2(0f, 0f);
		Vector2 size = new Vector2(shadowTextureSquareSize, shadowTextureSquareSize);
		for (int i = 0; i < num; i++)
		{
			Matrix4x4 identity = Matrix4x4.identity;
			switch (i)
			{
			case 1:
				identity *= _shiftRightMatrix;
				position.x = shadowTextureSquareSize;
				position.y = 0f;
				break;
			case 2:
				if (num == 3)
				{
					identity *= _shiftRightMatrix;
					identity *= _shiftRightMatrix;
					position.x = (float)shadowTextureSquareSize * 2f;
					position.y = 0f;
				}
				else
				{
					identity *= _shiftUpMatrix;
					position.x = 0f;
					position.y = shadowTextureSquareSize;
				}
				break;
			case 3:
				identity *= _shiftRightMatrix;
				identity *= _shiftUpMatrix;
				position.x = shadowTextureSquareSize;
				position.y = shadowTextureSquareSize;
				break;
			}
			Matrix4x4 viewMatrix = GetViewMatrix();
			Matrix4x4 projectionMatrix = GetProjectionMatrix(camera.transform.position, shadowDistance, cascadeDivisions[i].fraction);
			Matrix4x4 matrix4x = projectionMatrix * viewMatrix;
			Matrix4x4 matrix4x2 = _shadowMatrix * identity * matrix4x;
			_worldToShadowMatrixArray[i] = matrix4x2;
			GeometryUtility.CalculateFrustumPlanes(matrix4x, _frustumPlanes);
			_renderCommandBuffer.SetRenderTarget(_shadowTexture);
			_renderCommandBuffer.SetViewport(new Rect(position, size));
			_renderCommandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.white);
			_renderCommandBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
			_renderCommandBuffer.SetGlobalVector(_propID_unity_LightShadowBias, GetLightShadowBias(bias, shadowDistance));
			int num2 = ProxyShadowCascade.CascadeFlagToIndex(cascadeDivisions[i].shadowGroup);
			for (int j = 0; j < groupList.Count; j++)
			{
				if (!groupList[j].FrustumCheck(_frustumPlanes))
				{
					continue;
				}
				ProxyShadowCasterSuperGroup.CascadeGroup cascadeGroup = ((num2 < 3) ? groupList[j].GetNearCascade() : groupList[j].GetFarCascade());
				cascadeGroup.PreProcessRenderers();
				for (int k = 0; k < cascadeGroup.shadowCasters.Count; k++)
				{
					ProxyShadowCasterSuperGroup.ShadowCasterData shadowCasterData = cascadeGroup.shadowCasters[k];
					for (int l = 0; l < shadowCasterData.cachedSubMeshCount; l++)
					{
						_renderCommandBuffer.DrawMesh(shadowCasterData.cachedMesh, shadowCasterData.cachedGlobalMatrix, _shadowMaterial, l);
					}
				}
			}
		}
		_renderCommandBuffer.SetGlobalMatrixArray(_propID_OW_WorldToProxyShadowArray, _worldToShadowMatrixArray);
		_renderCommandBuffer.SetGlobalVector(_propID_proxyLightSplitsNear, _shadowSplitsNear);
		_renderCommandBuffer.SetGlobalVector(_propID_proxyLightSplitsFar, _shadowSplitsFar);
		_renderCommandBuffer.SetGlobalVector(_propID_cascadeTexMod, _cascadeTextureModifier);
		_paramsCommandBuffer.SetGlobalVector(_propID_LightShadowData, GetLightShadowData(camera));
		_paramsCommandBuffer.SetGlobalVector(_propID_cameraPosition, camera.transform.position);
	}

	private void ClearCommandBuffers(Camera camera)
	{
		if (camera == _targetCamera)
		{
			_targetCamera = null;
			_renderCommandBuffer.Clear();
			_paramsCommandBuffer.Clear();
		}
	}

	private Matrix4x4 GetViewMatrix()
	{
		Matrix4x4 m = Matrix4x4.TRS(_light.transform.position, _light.transform.rotation, Vector3.one);
		m = Matrix4x4.Inverse(m);
		m.m20 = 0f - m.m20;
		m.m21 = 0f - m.m21;
		m.m22 = 0f - m.m22;
		m.m23 = 0f - m.m23;
		return m;
	}

	private Matrix4x4 GetProjectionMatrix(Vector3 target, float shadowDistance, float cascadeFraction)
	{
		float num = shadowDistance * cascadeFraction;
		float num2 = Mathf.Max(Vector3.Distance(target, _light.transform.position) - shadowDistance, 0.001f);
		float zFar = num2 + shadowDistance * 2f;
		return Matrix4x4.Ortho(0f - num, num, 0f - num, num, num2, zFar);
	}

	private Vector4 GetLightShadowBias(float bias, float shadowDistance)
	{
		Vector4 result = default(Vector4);
		result.x = (0f - bias * 10f) / (shadowDistance * 2f);
		result.y = 1f;
		result.z = 0f;
		result.w = 0f;
		return result;
	}

	private Vector4 GetLightShadowData(Camera camera)
	{
		Vector4 result = default(Vector4);
		result.x = 1f - _light.shadowStrength;
		result.y = camera.farClipPlane / QualitySettings.shadowDistance;
		result.z = 5f / QualitySettings.shadowDistance;
		result.w = -2f * (1f + camera.fieldOfView / 180f);
		return result;
	}

	public static void SetCameraIgnored(Camera camera, bool ignored)
	{
		if (camera == null)
		{
			return;
		}
		if (ignored)
		{
			if (!s_ignoredCameras.Contains(camera))
			{
				s_ignoredCameras.Add(camera);
			}
		}
		else
		{
			s_ignoredCameras.Remove(camera);
		}
	}
}
