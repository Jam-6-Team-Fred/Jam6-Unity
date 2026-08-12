using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(OWCamera))]
public class PlanetaryFogRenderer : MonoBehaviour
{
	private static Material _fogMaterial;

	private static Mesh _bufferMesh;

	private static CommandBuffer _commandBuffer;

	private OWCamera _owCamera;

	private void Awake()
	{
		_owCamera = GetComponent<OWCamera>();
	}

	private void OnEnable()
	{
		if (_owCamera != null && VerifyResources())
		{
			_owCamera.mainCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, _commandBuffer);
			_owCamera.onThisPreRender += new OWEvent<OWCamera>.OWCallback(UpdateMaterial);
		}
	}

	private void OnDisable()
	{
		if (_owCamera != null && _commandBuffer != null)
		{
			_owCamera.mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, _commandBuffer);
			_owCamera.onThisPreRender -= new OWEvent<OWCamera>.OWCallback(UpdateMaterial);
		}
	}

	private void UpdateMaterial(OWCamera owCamera)
	{
		if (_fogMaterial != null)
		{
			_fogMaterial.SetMatrix("_FrustumCornersWS", CalcFrustumCorners());
		}
	}

	private bool VerifyResources()
	{
		if (_fogMaterial == null)
		{
			CreateMaterial();
		}
		if (_bufferMesh == null)
		{
			CreateMesh();
		}
		if (_commandBuffer == null)
		{
			CreateCommandBuffer();
		}
		if (_fogMaterial != null && _bufferMesh != null)
		{
			return _commandBuffer != null;
		}
		return false;
	}

	private void CreateMaterial()
	{
		Shader shader = Shader.Find("Hidden/PlanetaryFogCommandBuffer");
		if (shader != null)
		{
			_fogMaterial = new Material(shader);
			_fogMaterial.hideFlags = HideFlags.DontSave;
		}
	}

	private void CreateMesh()
	{
		_bufferMesh = new Mesh();
		_bufferMesh.name = "PlanetaryFogFullscreenQuad";
		_bufferMesh.vertices = new Vector3[4]
		{
			new Vector3(-1f, -1f, 0f),
			new Vector3(1f, -1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(-1f, 1f, 0f)
		};
		_bufferMesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
		_bufferMesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		_bufferMesh.uv2 = new Vector2[4]
		{
			new Vector2(3f, 0f),
			new Vector2(2f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 0f)
		};
		_bufferMesh.UploadMeshData(markNoLongerReadable: true);
		_bufferMesh.hideFlags = HideFlags.DontSave;
	}

	private void CreateCommandBuffer()
	{
		if (_fogMaterial != null && _bufferMesh != null)
		{
			_commandBuffer = new CommandBuffer();
			_commandBuffer.name = "Planetary Fog";
			_commandBuffer.DrawMesh(_bufferMesh, Matrix4x4.identity, _fogMaterial);
		}
	}

	private Matrix4x4 CalcFrustumCorners()
	{
		Transform obj = _owCamera.transform;
		float nearClipPlane = _owCamera.nearClipPlane;
		float farClipPlane = _owCamera.farClipPlane;
		float fieldOfView = _owCamera.fieldOfView;
		float aspect = _owCamera.aspect;
		Matrix4x4 identity = Matrix4x4.identity;
		float num = fieldOfView * 0.5f;
		Vector3 vector = obj.right * nearClipPlane * Mathf.Tan(num * ((float)Math.PI / 180f)) * aspect;
		Vector3 vector2 = obj.up * nearClipPlane * Mathf.Tan(num * ((float)Math.PI / 180f));
		Vector3 vector3 = obj.forward * nearClipPlane - vector + vector2;
		float num2 = vector3.magnitude * farClipPlane / nearClipPlane;
		vector3.Normalize();
		vector3 *= num2;
		Vector3 vector4 = obj.forward * nearClipPlane + vector + vector2;
		vector4.Normalize();
		vector4 *= num2;
		Vector3 vector5 = obj.forward * nearClipPlane + vector - vector2;
		vector5.Normalize();
		vector5 *= num2;
		Vector3 vector6 = obj.forward * nearClipPlane - vector - vector2;
		vector6.Normalize();
		vector6 *= num2;
		identity.SetRow(0, vector3);
		identity.SetRow(1, vector4);
		identity.SetRow(2, vector5);
		identity.SetRow(3, vector6);
		return identity;
	}
}
