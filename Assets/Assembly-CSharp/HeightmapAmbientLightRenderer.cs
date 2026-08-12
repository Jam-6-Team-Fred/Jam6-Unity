using System;
using UnityEngine;
using UnityEngine.Rendering;

[ImageEffectAllowedInSceneView]
[ExecuteInEditMode]
[RequireComponent(typeof(OWCamera))]
public class HeightmapAmbientLightRenderer : MonoBehaviour
{
	[SerializeField]
	private Shader _lightShader;

	private Material _ambientLightMaterial;

	private Mesh _fullscreenQuadMesh;

	private CommandBuffer _commandBuffer;

	private int _propID_FrustumCornersWS = Shader.PropertyToID("_FrustumCornersWS");

	private int _propID_SrcBlend = Shader.PropertyToID("_SrcBlend");

	private int _propID_DstBlend = Shader.PropertyToID("_DstBlend");

	private int _propID_AmbientLightDir = Shader.PropertyToID("_AmbientLightDir");

	private int _propID_AmbientLightColor = Shader.PropertyToID("_AmbientLightColor");

	private int _propID_AmbientLightFalloff = Shader.PropertyToID("_AmbientLightFalloff");

	private int _propID_AmbientLightGradient = Shader.PropertyToID("_AmbientLightGradient");

	private int _propID_WorldToAmbientLight = Shader.PropertyToID("_WorldToAmbientLight");

	private int _propID_AmbientLightHeightmap = Shader.PropertyToID("_AmbientLightHeightmap");

	private OWCamera _owCamera;

	private void Awake()
	{
		_owCamera = GetComponent<OWCamera>();
		CreateMaterial();
		CreateMesh();
		CreateCommandBuffer();
	}

	private void OnDestroy()
	{
		if (_ambientLightMaterial != null)
		{
			UnityEngine.Object.Destroy(_ambientLightMaterial);
		}
		if (_fullscreenQuadMesh != null)
		{
			UnityEngine.Object.Destroy(_fullscreenQuadMesh);
		}
		if (_commandBuffer != null)
		{
			_commandBuffer.Dispose();
			_commandBuffer = null;
		}
	}

	private void OnEnable()
	{
		if (_commandBuffer != null)
		{
			_owCamera.mainCamera.AddCommandBuffer(CameraEvent.AfterLighting, _commandBuffer);
			_owCamera.onThisPreRender += new OWEvent<OWCamera>.OWCallback(UpdateMaterial);
		}
	}

	private void OnDisable()
	{
		if (_commandBuffer != null)
		{
			_owCamera.mainCamera.RemoveCommandBuffer(CameraEvent.AfterLighting, _commandBuffer);
			_owCamera.onThisPreRender -= new OWEvent<OWCamera>.OWCallback(UpdateMaterial);
		}
	}

	private void UpdateMaterial(OWCamera owCamera)
	{
		if (_ambientLightMaterial == null)
		{
			return;
		}
		HeightmapAmbientLight activeLight = HeightmapAmbientLight.GetActiveLight();
		if (activeLight == null)
		{
			_ambientLightMaterial.SetInt(_propID_SrcBlend, 1);
			_ambientLightMaterial.SetInt(_propID_DstBlend, 1);
			_ambientLightMaterial.SetVector(_propID_AmbientLightDir, new Vector4(0f, -1f, 0f, 0f));
			_ambientLightMaterial.SetColor(_propID_AmbientLightColor, new Color(0f, 0f, 0f, 0f));
			_ambientLightMaterial.SetFloat(_propID_AmbientLightFalloff, 1f);
			_ambientLightMaterial.SetTexture(_propID_AmbientLightGradient, null);
			_ambientLightMaterial.SetMatrix(_propID_WorldToAmbientLight, Matrix4x4.identity);
			_ambientLightMaterial.SetTexture(_propID_AmbientLightHeightmap, null);
		}
		else
		{
			if (owCamera.mainCamera.allowHDR)
			{
				_ambientLightMaterial.SetInt(_propID_SrcBlend, 1);
				_ambientLightMaterial.SetInt(_propID_DstBlend, 1);
			}
			else
			{
				_ambientLightMaterial.SetInt(_propID_SrcBlend, 2);
				_ambientLightMaterial.SetInt(_propID_DstBlend, 0);
			}
			_ambientLightMaterial.SetVector(_propID_AmbientLightDir, activeLight.transform.forward);
			_ambientLightMaterial.SetColor(_propID_AmbientLightColor, activeLight.color.linear * activeLight.intensity);
			_ambientLightMaterial.SetFloat(_propID_AmbientLightFalloff, Mathf.Clamp01(activeLight.falloff / activeLight.size.z));
			_ambientLightMaterial.SetTexture(_propID_AmbientLightGradient, activeLight.gradient);
			_ambientLightMaterial.SetMatrix(_propID_WorldToAmbientLight, activeLight.CalcWorldToLightMatrix());
			_ambientLightMaterial.SetTexture(_propID_AmbientLightHeightmap, activeLight.heightmap);
		}
		_ambientLightMaterial.SetMatrix(_propID_FrustumCornersWS, CalcFrustumCorners());
	}

	private void CreateMaterial()
	{
		_ambientLightMaterial = new Material(_lightShader);
		_ambientLightMaterial.name = "HeightmapAmbientLightMaterial";
		_ambientLightMaterial.hideFlags = HideFlags.DontSave;
	}

	private void CreateMesh()
	{
		_fullscreenQuadMesh = new Mesh();
		_fullscreenQuadMesh.name = "HeightmapAmbientLightFullscreenQuad";
		_fullscreenQuadMesh.vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, 0f),
			new Vector3(1f, -1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(-1f, 1f, 0f)
		};
		_fullscreenQuadMesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
		_fullscreenQuadMesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		_fullscreenQuadMesh.uv2 = new Vector2[4]
		{
			new Vector2(3f, 0f),
			new Vector2(2f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 0f)
		};
		_fullscreenQuadMesh.UploadMeshData(markNoLongerReadable: true);
		_fullscreenQuadMesh.hideFlags = HideFlags.DontSave;
	}

	private void CreateCommandBuffer()
	{
		_commandBuffer = new CommandBuffer();
		_commandBuffer.name = "HeightmapAmbientLight";
		_commandBuffer.DrawMesh(_fullscreenQuadMesh, Matrix4x4.identity, _ambientLightMaterial);
	}

	private Matrix4x4 CalcFrustumCorners()
	{
		float nearClipPlane = _owCamera.nearClipPlane;
		float farClipPlane = _owCamera.farClipPlane;
		float fieldOfView = _owCamera.fieldOfView;
		float aspect = _owCamera.aspect;
		Transform obj = _owCamera.transform;
		Vector3 vector = obj.forward * nearClipPlane;
		Vector3 vector2 = obj.right * nearClipPlane;
		Vector3 vector3 = obj.up * nearClipPlane;
		float num = Mathf.Tan(fieldOfView * 0.5f * ((float)Math.PI / 180f));
		Vector3 vector4 = vector2 * num * aspect;
		Vector3 vector5 = vector3 * num;
		Vector3 vector6 = vector - vector4 + vector5;
		float num2 = vector6.magnitude * farClipPlane / nearClipPlane;
		vector6.Normalize();
		vector6 *= num2;
		Vector3 vector7 = vector + vector4 + vector5;
		vector7.Normalize();
		vector7 *= num2;
		Vector3 vector8 = vector + vector4 - vector5;
		vector8.Normalize();
		vector8 *= num2;
		Vector3 vector9 = vector - vector4 - vector5;
		vector9.Normalize();
		vector9 *= num2;
		Matrix4x4 result = default(Matrix4x4);
		result.SetRow(0, vector6);
		result.SetRow(1, vector7);
		result.SetRow(2, vector8);
		result.SetRow(3, vector9);
		return result;
	}
}
