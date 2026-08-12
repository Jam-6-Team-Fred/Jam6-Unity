using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CubeProxyRenderTest : MonoBehaviour
{
	public RenderTexture cube;

	private Camera _camera;

	private CommandBuffer _commandBuffer;

	private Shader _shader;

	private Material _material;

	private Shader shader
	{
		get
		{
			if (!(_shader != null))
			{
				return _shader = Shader.Find("Custom/ObjectDepth");
			}
			return _shader;
		}
	}

	private Material material
	{
		get
		{
			if (_material == null)
			{
				_material = new Material(shader);
				_material.hideFlags = HideFlags.HideAndDontSave;
			}
			return _material;
		}
	}

	private void Start()
	{
		_camera = GetComponent<Camera>();
		BuildCommandBuffer(_camera);
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Update()
	{
		Vector4 value = base.transform.position;
		value.w = 0.001f;
		Shader.SetGlobalVector(Shader.PropertyToID("_CubeLightPositionRange"), value);
		BuildCommandBuffer(_camera);
		Graphics.ExecuteCommandBuffer(_commandBuffer);
	}

	private Matrix4x4 GetViewMatrix()
	{
		Matrix4x4 m = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
		m = Matrix4x4.Inverse(m);
		m.m20 = 0f - m.m20;
		m.m21 = 0f - m.m21;
		m.m22 = 0f - m.m22;
		m.m23 = 0f - m.m23;
		return m;
	}

	private Matrix4x4 GetProjectionMatrix()
	{
		return Matrix4x4.Perspective(90f, 1f, 0.3f, 1000f);
	}

	private void BuildCommandBuffer(Camera camera)
	{
		_commandBuffer = new CommandBuffer();
		_commandBuffer.name = "RenderDepth";
		_commandBuffer.SetRenderTarget(cube, 0, CubemapFace.PositiveZ);
		_commandBuffer.SetViewProjectionMatrices(GetViewMatrix(), GetProjectionMatrix());
		_commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.white);
		List<ProxyShadowCasterSuperGroup> groupList = ProxyShadowCasterSuperGroup.GetGroupList();
		Debug.Log("caster group count = " + groupList.Count);
		for (int i = 0; i < groupList.Count; i++)
		{
			ProxyShadowCasterSuperGroup.CascadeGroup farCascade = groupList[i].GetFarCascade();
			farCascade.PreProcessRenderers();
			for (int j = 0; j < farCascade.shadowCasters.Count; j++)
			{
				ProxyShadowCasterSuperGroup.ShadowCasterData shadowCasterData = farCascade.shadowCasters[j];
				for (int k = 0; k < shadowCasterData.cachedSubMeshCount; k++)
				{
					_commandBuffer.DrawMesh(shadowCasterData.cachedMesh, shadowCasterData.cachedGlobalMatrix, material, k);
				}
			}
		}
	}

	private void ClearCommandBuffer(Camera camera)
	{
		_commandBuffer.Clear();
	}
}
