using UnityEngine;

[AddComponentMenu("Streaming/Streaming Skinned Mesh Handle", 300)]
public class StreamingSkinnedMeshHandle : StreamingMeshHandle
{
	private SkinnedMeshRenderer _skinnedMeshRenderer;

	protected override void Awake()
	{
		_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		if (!string.IsNullOrEmpty(assetBundle))
		{
			StreamingManager.RegisterStreamingMeshHandle(this);
		}
	}

	public override void LoadMesh(Mesh mesh)
	{
		if (_skinnedMeshRenderer != null)
		{
			_skinnedMeshRenderer.sharedMesh = mesh;
		}
		InvokeOnMeshLoaded();
	}

	public override void UnloadMesh()
	{
		if (_skinnedMeshRenderer != null)
		{
			_skinnedMeshRenderer.sharedMesh = proxyMesh;
		}
		InvokeOnMeshUnloaded();
	}
}
