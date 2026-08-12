using System;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class PlanetaryFogImageEffect : MonoBehaviour
{
	private Camera _camera;

	public Shader fogShader;

	private Material fogMaterial;

	private static readonly int _propID_FogParams = Shader.PropertyToID("_FogParams");

	private static readonly int _propID_FrustumCornersWS = Shader.PropertyToID("_FrustumCornersWS");

	private static readonly int _propID_RingworldFogClipPlane1 = Shader.PropertyToID("_RingworldFogClipPlane1");

	private static readonly int _propID_RingworldFogClipPlane2 = Shader.PropertyToID("_RingworldFogClipPlane2");

	private static readonly int _propID_MainTex = Shader.PropertyToID("_MainTex");

	private void Awake()
	{
		Shader.SetGlobalVector(_propID_FogParams, new Vector4(0f, 0f, 1f, 0f));
	}

	private void OnDestroy()
	{
		if (fogMaterial != null)
		{
#if UNITY_EDITOR
			UnityEngine.Object.DestroyImmediate(fogMaterial);
#else
			UnityEngine.Object.Destroy(fogMaterial);
#endif
		}
		fogMaterial = null;
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_camera == null)
		{
			_camera = GetComponent<Camera>();
		}
		if (fogMaterial == null && fogShader != null)
		{
			fogMaterial = new Material(fogShader);
		}
		if (fogMaterial != null)
		{
			Transform obj = _camera.transform;
			float nearClipPlane = _camera.nearClipPlane;
			float farClipPlane = _camera.farClipPlane;
			float fieldOfView = _camera.fieldOfView;
			float aspect = _camera.aspect;
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
			fogMaterial.SetMatrix(_propID_FrustumCornersWS, identity);
			PlanetaryFogController activeFogSphere = PlanetaryFogController.GetActiveFogSphere();
			if (activeFogSphere != null && activeFogSphere.isRingworldFog)
			{
				Vector3 position = activeFogSphere.transform.position;
				Vector3 up = activeFogSphere.transform.up;
				Plane plane = new Plane(up, position - up * activeFogSphere.ringworldPlaneDist1);
				Plane plane2 = new Plane(-up, position + up * activeFogSphere.ringworldPlaneDist2);
				fogMaterial.EnableKeyword("USE_RINGWORLD_LIGHTING");
				fogMaterial.SetVector(_propID_RingworldFogClipPlane1, new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance));
				fogMaterial.SetVector(_propID_RingworldFogClipPlane2, new Vector4(plane2.normal.x, plane2.normal.y, plane2.normal.z, plane2.distance));
			}
			else
			{
				fogMaterial.DisableKeyword("USE_RINGWORLD_LIGHTING");
			}
			CustomGraphicsBlit(source, destination, fogMaterial);
		}
	}

	private void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material mat)
	{
		RenderTexture.active = dest;
		mat.SetTexture(_propID_MainTex, source);
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(0);
		GL.Begin(7);
		GL.MultiTexCoord2(0, 0f, 0f);
		GL.Vertex3(0f, 0f, 3f);
		GL.MultiTexCoord2(0, 1f, 0f);
		GL.Vertex3(1f, 0f, 2f);
		GL.MultiTexCoord2(0, 1f, 1f);
		GL.Vertex3(1f, 1f, 1f);
		GL.MultiTexCoord2(0, 0f, 1f);
		GL.Vertex3(0f, 1f, 0f);
		GL.End();
		GL.PopMatrix();
	}
}
