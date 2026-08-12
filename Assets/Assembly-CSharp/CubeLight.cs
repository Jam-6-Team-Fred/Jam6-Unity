using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CubeLight : SectoredMonoBehaviour
{
	[Serializable]
	private struct FaceSuperGroup
	{
		public ProxyShadowCasterSuperGroup[] superGroups;
	}

	private readonly string[] kProfilingFaceNames = new string[6]
	{
		"Build command buffer " + CubemapFace.PositiveX,
		"Build command buffer " + CubemapFace.NegativeX,
		"Build command buffer " + CubemapFace.PositiveY,
		"Build command buffer " + CubemapFace.NegativeY,
		"Build command buffer " + CubemapFace.PositiveZ,
		"Build command buffer " + CubemapFace.NegativeZ
	};

	private readonly string[] kCommandBufferRenderFaceNames = new string[6]
	{
		"Render Shadow Cubemap " + CubemapFace.PositiveX,
		"Render Shadow Cubemap " + CubemapFace.NegativeX,
		"Render Shadow Cubemap " + CubemapFace.PositiveY,
		"Render Shadow Cubemap " + CubemapFace.NegativeY,
		"Render Shadow Cubemap " + CubemapFace.PositiveZ,
		"Render Shadow Cubemap " + CubemapFace.NegativeZ
	};

	public bool debugShadowMap;

	public int renderMask;

	[SerializeField]
	private FaceSuperGroup[] _faceSuperGroups;

	[SerializeField]
	private int _neverDrawMask;

	[SerializeField]
	private int _neverDynamicMask;

	[SerializeField]
	private bool _updateFrustums;

	[SerializeField]
	private bool _cameraTest = true;

	[SerializeField]
	private int _renderTextureSize = 1024;

	[SerializeField]
	private float _cascadeBlendRange = 50f;

	[SerializeField]
	private float _range = 50000f;

	[SerializeField]
	private float _receiverPlaneDepthBias;

	[SerializeField]
	private float _distanceBias = 0.985f;

	[SerializeField]
	private float _padOffset = 0.0108f;

	[SerializeField]
	private float _padExponent = 500f;

	[SerializeField]
	private Light _light;

	[SerializeField]
	private RingworldShadowsOverride _ringworldShadowsOverride;

	[SerializeField]
	private int _debugFrustumDraw;

	private Material _objectRenderMaterial;

	private Vector4 _shadowSplitsNear;

	private Vector4 _shadowSplitsFar;

	private Plane[][] _frustums;

	private Vector3[][] _frustumVertices;

	private Plane[] _cameraFrustum = new Plane[6];

	private Quaternion[] _cubeEulers;

	private CommandBuffer[] _faceCommandBuffers = new CommandBuffer[6];

	private CommandBuffer _paramsCommandBuffer;

	private int _propID_CubeLightPositionRange;

	private int _propID_CubeLightParams;

	private int _propID_CubeLightWorldToLocal;

	private int _propID_CubeUseCubeShadows;

	private int _propID_LightShadowData;

	private int _propID_cameraPosition;

	private int _propID_unity_LightShadowBias;

	private int _propID_proxyLightSplitsNear;

	private int _propID_proxyLightSplitsFar;

	private int _propID_faceWorldToShadowMats;

	private int _propID_padSeamOffset;

	private int _propID_padSeamExp;

	private bool _overridingShadowSettings;

	private float _prevShadowDistance;

	private float _prevProxyDistance;

	private ProxyShadowCascade.Division[] _prevCascadeDivisions;

	private bool _dirty = true;

	private ProxyShadowLight _myProxyLight;

	private RenderTexture _2DCubeTexture;

	private CommandBuffer _clear2DTexCommandBuffer;

	private Matrix4x4[] _viewMatrices = new Matrix4x4[6];

	private Matrix4x4 _projectionMatrix;

	private Matrix4x4[] _faceWorldToShadowMatrices = new Matrix4x4[6];

	private Matrix4x4 _shadowSamplingScaleBiasMat;

	private bool _needsFirstDraw;

	private Material objectRenderMaterial
	{
		get
		{
			if (_objectRenderMaterial == null)
			{
				_objectRenderMaterial = new Material(Shader.Find("Hidden/ProxyShadowCaster"));
				_objectRenderMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return _objectRenderMaterial;
		}
	}

	private Camera _mainCamera
	{
		get
		{
			OWCamera activeCamera = Locator.GetActiveCamera();
			if (activeCamera != null)
			{
				return activeCamera.mainCamera;
			}
			return Camera.main;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (_ringworldShadowsOverride == null)
		{
			_ringworldShadowsOverride = base.transform.parent.GetComponentInChildren<RingworldShadowsOverride>();
		}
		_shadowSamplingScaleBiasMat = Matrix4x4.identity;
		_shadowSamplingScaleBiasMat.m00 = 0.5f;
		_shadowSamplingScaleBiasMat.m11 = 0.5f;
		_shadowSamplingScaleBiasMat.m22 = 0.5f;
		_shadowSamplingScaleBiasMat.m03 = 0.5f;
		_shadowSamplingScaleBiasMat.m13 = 0.5f;
		_shadowSamplingScaleBiasMat.m23 = 0.5f;
		_faceCommandBuffers = new CommandBuffer[6];
		for (int i = 0; i < 6; i++)
		{
			_faceCommandBuffers[i] = new CommandBuffer();
		}
		_paramsCommandBuffer = new CommandBuffer();
		_paramsCommandBuffer.name = "CubeProxyShadowmapParams";
		_clear2DTexCommandBuffer = new CommandBuffer();
		_clear2DTexCommandBuffer.name = "Clear 2D Cube Texture";
		_cubeEulers = new Quaternion[6];
	}

	private void Start()
	{
		_propID_CubeLightPositionRange = Shader.PropertyToID("_CubeLightPositionRange");
		_propID_CubeLightParams = Shader.PropertyToID("_CubeLightParams");
		_propID_CubeLightWorldToLocal = Shader.PropertyToID("_CubeLightWorldToLocal");
		_propID_CubeUseCubeShadows = Shader.PropertyToID("_CubeUseCubeShadows");
		_propID_LightShadowData = Shader.PropertyToID("_LightShadowData");
		_propID_cameraPosition = Shader.PropertyToID("_proxyCameraPosition");
		_propID_proxyLightSplitsNear = Shader.PropertyToID("_proxyLightSplitsNear");
		_propID_proxyLightSplitsFar = Shader.PropertyToID("_proxyLightSplitsFar");
		_propID_faceWorldToShadowMats = Shader.PropertyToID("_CubeFaceWorldToShadow");
		_propID_padSeamOffset = Shader.PropertyToID("OWCubePadOffset");
		_propID_padSeamExp = Shader.PropertyToID("OWCubePadExp");
		_propID_unity_LightShadowBias = Shader.PropertyToID("unity_LightShadowBias");
		CreateFrustums();
		ProxyShadowSettings.OnShadowDistanceChanged += OnShadowSettingsChanged;
		ProxyShadowSettings.OnShadowTextureSizeChanged += OnShadowSettingsChanged;
		ProxyShadowSettings.OnCascadeDivisionsChanged += OnShadowSettingsChanged;
		DisableCubeMapShadows();
	}

	private void Update()
	{
		Vector4 value = base.transform.position;
		value.w = 1f / _range;
		Vector4 value2 = default(Vector4);
		value2.x = _receiverPlaneDepthBias;
		value2.y = _distanceBias;
		Shader.SetGlobalVector(_propID_CubeLightPositionRange, value);
		Shader.SetGlobalVector(_propID_CubeLightParams, value2);
		Shader.SetGlobalFloat(_propID_padSeamOffset, _padOffset);
		Shader.SetGlobalFloat(_propID_padSeamExp, _padExponent);
		Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
		Shader.SetGlobalMatrix(_propID_CubeLightWorldToLocal, worldToLocalMatrix);
		renderMask = (_cameraTest ? TestCamera() : 63);
	}

	private void LateUpdate()
	{
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < _faceSuperGroups[i].superGroups.Length; j++)
			{
				if (_faceSuperGroups[i].superGroups[j].GetFarCascade().hasDynamicShadowCasters)
				{
					num += 1 << i;
				}
			}
		}
		ClearCommandBuffers();
		BuildClearCommandBuffer(num);
		BuildCommandBuffers(Locator.GetActiveCamera(), num);
		_needsFirstDraw = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger<GraphicSettings>.RemoveListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
		ProxyShadowSettings.OnShadowDistanceChanged -= OnShadowSettingsChanged;
		ProxyShadowSettings.OnShadowTextureSizeChanged -= OnShadowSettingsChanged;
		ProxyShadowSettings.OnCascadeDivisionsChanged -= OnShadowSettingsChanged;
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			EnableCubeMapShadows();
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			DisableCubeMapShadows();
		}
	}

	private void OnEnable()
	{
		AddCommandBuffers();
	}

	private void OnDisable()
	{
		RemoveCommandBuffers();
	}

	private void OnGraphicSettingsUpdated(GraphicSettings settings)
	{
		if (_overridingShadowSettings)
		{
			StartCoroutine(OverrideSettingsCoroutine());
		}
	}

	private void OnShadowSettingsChanged()
	{
		_dirty = true;
	}

	private IEnumerator OverrideSettingsCoroutine()
	{
		yield return new WaitForEndOfFrame();
	}

	private void SetShadowSettings()
	{
		if (!_overridingShadowSettings)
		{
			_prevShadowDistance = QualitySettings.shadowDistance;
			_prevCascadeDivisions = new ProxyShadowCascade.Division[ProxyShadowSettings.cascadeDivisions.Length];
			Array.Copy(ProxyShadowSettings.cascadeDivisions, _prevCascadeDivisions, ProxyShadowSettings.cascadeDivisions.Length);
			_prevProxyDistance = ProxyShadowSettings.shadowDistance;
		}
		UnityEngine.Object.FindObjectOfType<ProxyShadowLight>().enabled = false;
		QualitySettings.shadowDistance = Mathf.Min(_prevShadowDistance, _cascadeBlendRange);
		ProxyShadowSettings.cascadeDivisions = new ProxyShadowCascade.Division[1]
		{
			new ProxyShadowCascade.Division(ProxyShadowCascade.Flags.Final, 0f)
		};
		ProxyShadowSettings.shadowDistance = Mathf.Min(_prevProxyDistance, _cascadeBlendRange);
		_overridingShadowSettings = true;
	}

	private void RestoreShadowSettings()
	{
		if (_overridingShadowSettings)
		{
			QualitySettings.shadowDistance = _prevShadowDistance;
			ProxyShadowSettings.cascadeDivisions = _prevCascadeDivisions;
			ProxyShadowSettings.shadowDistance = _prevProxyDistance;
			UnityEngine.Object.FindObjectOfType<ProxyShadowLight>().enabled = true;
			_overridingShadowSettings = false;
		}
	}

	private RenderTexture CreateRenderTexture()
	{
		RenderTexture renderTexture = new RenderTexture(_renderTextureSize, _renderTextureSize * 5, 24, RenderTextureFormat.Shadowmap, RenderTextureReadWrite.Linear);
		renderTexture.name = "RingWorldProxyShadow2dCubemap";
		renderTexture.hideFlags = HideFlags.HideAndDontSave;
		renderTexture.useMipMap = false;
		renderTexture.wrapMode = TextureWrapMode.Clamp;
		renderTexture.filterMode = FilterMode.Bilinear;
		renderTexture.Create();
		return renderTexture;
	}

	private void EnableCubeMapShadows()
	{
		Debug.Log("enabling ringworld cube shadows");
		if (_2DCubeTexture == null)
		{
			_2DCubeTexture = CreateRenderTexture();
			Shader.SetGlobalTexture("OW_ProxyShadow2DCubeMap", _2DCubeTexture);
			_needsFirstDraw = true;
		}
		Shader.SetGlobalFloat(_propID_CubeUseCubeShadows, 1f);
		base.enabled = true;
	}

	private void DisableCubeMapShadows()
	{
		Debug.Log("disabling ringworld cube shadows");
		Shader.SetGlobalFloat(_propID_CubeUseCubeShadows, 0f);
		base.enabled = false;
		if (_2DCubeTexture != null)
		{
			_2DCubeTexture.Release();
			_2DCubeTexture = null;
		}
	}

	private void AddCommandBuffers()
	{
		_light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, _clear2DTexCommandBuffer);
		for (int i = 0; i < 6; i++)
		{
			_light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, _faceCommandBuffers[i]);
		}
		_light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, _paramsCommandBuffer);
	}

	private void RemoveCommandBuffers()
	{
		if (_faceCommandBuffers == null)
		{
			return;
		}
		_light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, _clear2DTexCommandBuffer);
		for (int i = 0; i < 6; i++)
		{
			if (_faceCommandBuffers[i] != null)
			{
				_light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, _faceCommandBuffers[i]);
			}
		}
		_light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, _paramsCommandBuffer);
	}

	private Vector3[] CalcFrustumVertices(Plane[] frustum)
	{
		Vector3[] array = new Vector3[8];
		int[][] array2 = new int[8][]
		{
			new int[3] { 0, 2, 4 },
			new int[3] { 1, 2, 4 },
			new int[3] { 0, 3, 4 },
			new int[3] { 1, 3, 4 },
			new int[3] { 0, 2, 5 },
			new int[3] { 1, 2, 5 },
			new int[3] { 0, 3, 5 },
			new int[3] { 1, 3, 5 }
		};
		for (int i = 0; i < 8; i++)
		{
			Matrix4x4 identity = Matrix4x4.identity;
			identity.m00 = frustum[array2[i][0]].normal.x;
			identity.m01 = frustum[array2[i][0]].normal.y;
			identity.m02 = frustum[array2[i][0]].normal.z;
			identity.m10 = frustum[array2[i][1]].normal.x;
			identity.m11 = frustum[array2[i][1]].normal.y;
			identity.m12 = frustum[array2[i][1]].normal.z;
			identity.m20 = frustum[array2[i][2]].normal.x;
			identity.m21 = frustum[array2[i][2]].normal.y;
			identity.m22 = frustum[array2[i][2]].normal.z;
			float determinant = identity.determinant;
			Matrix4x4 identity2 = Matrix4x4.identity;
			identity2.m00 = 0f - frustum[array2[i][0]].distance;
			identity2.m01 = frustum[array2[i][0]].normal.y;
			identity2.m02 = frustum[array2[i][0]].normal.z;
			identity2.m10 = 0f - frustum[array2[i][1]].distance;
			identity2.m11 = frustum[array2[i][1]].normal.y;
			identity2.m12 = frustum[array2[i][1]].normal.z;
			identity2.m20 = 0f - frustum[array2[i][2]].distance;
			identity2.m21 = frustum[array2[i][2]].normal.y;
			identity2.m22 = frustum[array2[i][2]].normal.z;
			Matrix4x4 identity3 = Matrix4x4.identity;
			identity3.m00 = frustum[array2[i][0]].normal.x;
			identity3.m01 = 0f - frustum[array2[i][0]].distance;
			identity3.m02 = frustum[array2[i][0]].normal.z;
			identity3.m10 = frustum[array2[i][1]].normal.x;
			identity3.m11 = 0f - frustum[array2[i][1]].distance;
			identity3.m12 = frustum[array2[i][1]].normal.z;
			identity3.m20 = frustum[array2[i][2]].normal.x;
			identity3.m21 = 0f - frustum[array2[i][2]].distance;
			identity3.m22 = frustum[array2[i][2]].normal.z;
			Matrix4x4 identity4 = Matrix4x4.identity;
			identity4.m00 = frustum[array2[i][0]].normal.x;
			identity4.m01 = frustum[array2[i][0]].normal.y;
			identity4.m02 = 0f - frustum[array2[i][0]].distance;
			identity4.m10 = frustum[array2[i][1]].normal.x;
			identity4.m11 = frustum[array2[i][1]].normal.y;
			identity4.m12 = 0f - frustum[array2[i][1]].distance;
			identity4.m20 = frustum[array2[i][2]].normal.x;
			identity4.m21 = frustum[array2[i][2]].normal.y;
			identity4.m22 = 0f - frustum[array2[i][2]].distance;
			float x = identity2.determinant / determinant;
			float y = identity3.determinant / determinant;
			float z = identity4.determinant / determinant;
			array[i] = new Vector3(x, y, z);
		}
		return array;
	}

	private void CreateFrustums()
	{
		Vector3[] array = new Vector3[6]
		{
			Vector3.right,
			Vector3.left,
			Vector3.up,
			Vector3.down,
			Vector3.forward,
			Vector3.back
		};
		Quaternion[] array2 = new Quaternion[6]
		{
			Quaternion.Euler(0f, 45f, 0f),
			Quaternion.Euler(0f, -45f, 0f),
			Quaternion.Euler(45f, 0f, 0f),
			Quaternion.Euler(-45f, 0f, 0f),
			Quaternion.identity,
			Quaternion.Euler(0f, 180f, 0f)
		};
		_frustums = new Plane[6][];
		_frustumVertices = new Vector3[6][];
		for (int i = 0; i < 6; i++)
		{
			_frustums[i] = new Plane[6];
			for (int j = 0; j < 6; j++)
			{
				Quaternion quaternion = Quaternion.LookRotation(array[i]);
				float d = ((j == 5) ? _range : 0f);
				Vector3 inNormal = quaternion * array2[j] * Vector3.forward;
				_frustums[i][j] = new Plane(inNormal, d);
			}
			_frustumVertices[i] = CalcFrustumVertices(_frustums[i]);
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
		result.y = camera.farClipPlane / _ringworldShadowsOverride.overrideShadowDistance;
		result.z = 5f / _ringworldShadowsOverride.overrideShadowDistance;
		result.w = -2f * (1f + camera.fieldOfView / 180f);
		return result;
	}

	private void BuildClearCommandBuffer(int dynamicMask)
	{
		_clear2DTexCommandBuffer.Clear();
		_clear2DTexCommandBuffer.SetRenderTarget(_2DCubeTexture);
		for (int i = 0; i < 6; i++)
		{
			if ((((dynamicMask & (1 << i) & ~_neverDynamicMask) > 0) | _needsFirstDraw) & ((_neverDrawMask & (1 << i)) <= 0))
			{
				Vector2 vector = Vector2.zero;
				switch (i)
				{
				case 1:
					vector = new Vector2(0f, 1f);
					break;
				case 2:
					vector = new Vector2(0f, 2f);
					break;
				case 3:
					vector = new Vector2(0f, 5f);
					break;
				case 4:
					vector = new Vector2(0f, 3f);
					break;
				case 5:
					vector = new Vector2(0f, 4f);
					break;
				}
				vector *= (float)_renderTextureSize;
				_clear2DTexCommandBuffer.SetViewport(new Rect(vector.x, vector.y, _renderTextureSize, _renderTextureSize));
				_clear2DTexCommandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
			}
		}
	}

	private void BuildCommandBuffers(OWCamera camera, int dynamicMask)
	{
		if (_needsFirstDraw)
		{
			Debug.Log("[CubeLight] first draw");
		}
		float shadowDistance = QualitySettings.shadowDistance;
		float bias = ProxyShadowSettings.bias;
		ProxyShadowCascade.Division[] cascadeDivisions = ProxyShadowSettings.cascadeDivisions;
		if (_dirty)
		{
			UpdateLightSplits(cascadeDivisions, shadowDistance);
			_dirty = false;
		}
		_cubeEulers[0] = Quaternion.AngleAxis(90f, base.transform.up);
		_cubeEulers[1] = Quaternion.AngleAxis(-90f, base.transform.up);
		_cubeEulers[2] = Quaternion.AngleAxis(-90f, base.transform.right);
		_cubeEulers[3] = Quaternion.AngleAxis(-90f, -base.transform.right);
		_cubeEulers[4] = Quaternion.identity;
		_cubeEulers[5] = Quaternion.AngleAxis(180f, base.transform.right);
		_projectionMatrix = Matrix4x4.Perspective(90f, 1f, 25f, _range);
		for (int i = 0; i < 6; i++)
		{
			bool flag = (dynamicMask & (1 << i) & ~_neverDynamicMask) > 0;
			flag &= (renderMask & (1 << i)) > 0;
			flag |= _needsFirstDraw;
			flag &= (_neverDrawMask & (1 << i)) <= 0;
			_viewMatrices[i] = Matrix4x4.TRS(base.transform.position, _cubeEulers[i] * base.transform.rotation, new Vector3((i != 5) ? 1 : (-1), (i != 5) ? 1 : (-1), 1f));
			_viewMatrices[i] = Matrix4x4.Inverse(_viewMatrices[i]);
			_viewMatrices[i].m20 = 0f - _viewMatrices[i].m20;
			_viewMatrices[i].m21 = 0f - _viewMatrices[i].m21;
			_viewMatrices[i].m22 = 0f - _viewMatrices[i].m22;
			_viewMatrices[i].m23 = 0f - _viewMatrices[i].m23;
			_faceCommandBuffers[i].name = kCommandBufferRenderFaceNames[i];
			_faceCommandBuffers[i].SetRenderTarget(_2DCubeTexture, 0);
			Vector2 vector = Vector2.zero;
			switch (i)
			{
			case 1:
				vector = new Vector2(0f, 1f);
				break;
			case 2:
				vector = new Vector2(0f, 2f);
				break;
			case 3:
				vector = new Vector2(0f, 5f);
				break;
			case 4:
				vector = new Vector2(0f, 3f);
				break;
			case 5:
				vector = new Vector2(0f, 4f);
				break;
			}
			vector *= (float)_renderTextureSize;
			_faceWorldToShadowMatrices[i] = _shadowSamplingScaleBiasMat * _projectionMatrix * _viewMatrices[i];
			if (!flag)
			{
				continue;
			}
			_faceCommandBuffers[i].EnableShaderKeyword("SHADOWS_CUBE");
			_faceCommandBuffers[i].SetViewport(new Rect(vector.x, vector.y, _renderTextureSize, _renderTextureSize));
			_faceCommandBuffers[i].SetViewProjectionMatrices(_viewMatrices[i], _projectionMatrix);
			for (int j = 0; j < _faceSuperGroups[i].superGroups.Length; j++)
			{
				ProxyShadowCasterSuperGroup.CascadeGroup farCascade = _faceSuperGroups[i].superGroups[j].GetFarCascade();
				farCascade.PreProcessRenderers();
				for (int k = 0; k < farCascade.shadowCasters.Count; k++)
				{
					ProxyShadowCasterSuperGroup.ShadowCasterData shadowCasterData = farCascade.shadowCasters[k];
					for (int l = 0; l < shadowCasterData.cachedSubMeshCount; l++)
					{
						_faceCommandBuffers[i].DrawMesh(shadowCasterData.cachedMesh, shadowCasterData.cachedGlobalMatrix, objectRenderMaterial, l);
					}
				}
			}
		}
		Vector4 value = base.transform.position;
		value.w = 1f / _range;
		_paramsCommandBuffer.SetGlobalVector(_propID_proxyLightSplitsNear, _shadowSplitsNear);
		_paramsCommandBuffer.SetGlobalVector(_propID_proxyLightSplitsFar, _shadowSplitsFar);
		_paramsCommandBuffer.SetGlobalVector(_propID_unity_LightShadowBias, GetLightShadowBias(bias, shadowDistance));
		_paramsCommandBuffer.SetGlobalVector("_LightPositionRange", value);
		_paramsCommandBuffer.SetGlobalVector("_WorldSpaceLightPos0", new Vector4(base.transform.position.x, base.transform.position.y, base.transform.position.z, 1f));
		_paramsCommandBuffer.SetGlobalMatrixArray(_propID_faceWorldToShadowMats, _faceWorldToShadowMatrices);
		_paramsCommandBuffer.SetGlobalVector(_propID_LightShadowData, GetLightShadowData(camera.mainCamera));
		_paramsCommandBuffer.SetGlobalVector(_propID_cameraPosition, camera.transform.position);
	}

	private void ClearCommandBuffers()
	{
		for (int i = 0; i < 6; i++)
		{
			_faceCommandBuffers[i].Clear();
		}
		_paramsCommandBuffer.Clear();
	}

	private void ExecuteCommandBuffers()
	{
		for (int i = 0; i < 6; i++)
		{
			if ((renderMask & (1 << i)) > 0)
			{
				Graphics.ExecuteCommandBuffer(_faceCommandBuffers[i]);
			}
		}
	}

	private int TestCamera()
	{
		_ = base.transform.localToWorldMatrix;
		Vector3 position = _mainCamera.transform.position;
		Vector3[] cameraVertices = new Vector3[5]
		{
			position + _mainCamera.ViewportPointToRay(new Vector3(0f, 0f, 0f)).direction * _mainCamera.farClipPlane,
			position + _mainCamera.ViewportPointToRay(new Vector3(0f, 1f, 0f)).direction * _mainCamera.farClipPlane,
			position + _mainCamera.ViewportPointToRay(new Vector3(1f, 0f, 0f)).direction * _mainCamera.farClipPlane,
			position + _mainCamera.ViewportPointToRay(new Vector3(1f, 1f, 0f)).direction * _mainCamera.farClipPlane,
			position
		};
		GeometryUtility.CalculateFrustumPlanes(_mainCamera, _cameraFrustum);
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			if (TestFrustumIntersect(cameraVertices, _cameraFrustum, i))
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	private bool TestFrustumIntersect(Vector3[] cameraVertices, Plane[] cameraFrustum, int frustumCacheIdx, bool isPyr = true)
	{
		int num = (isPyr ? 5 : 8);
		Plane[] array = _frustums[frustumCacheIdx];
		Vector3[] array2 = _frustumVertices[frustumCacheIdx];
		bool flag = true;
		for (int i = 0; i < 6; i++)
		{
			bool flag2 = false;
			for (int j = 0; j < num; j++)
			{
				float num2 = Vector3.Dot(array[i].normal, base.transform.InverseTransformPoint(cameraVertices[j])) + array[i].distance;
				flag2 = flag2 || num2 > 0f;
				if (flag2)
				{
					break;
				}
			}
			flag = flag && flag2;
			if (!flag)
			{
				return flag;
			}
		}
		for (int k = 0; k < 6; k++)
		{
			bool flag3 = false;
			for (int l = 0; l < 8; l++)
			{
				float num3 = Vector3.Dot(cameraFrustum[k].normal, base.transform.TransformPoint(array2[l])) + cameraFrustum[k].distance;
				flag3 = flag3 || num3 > 0f;
				if (flag3)
				{
					break;
				}
			}
			flag = flag && flag3;
			if (!flag)
			{
				return flag;
			}
		}
		return flag;
	}
}
