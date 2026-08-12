using UnityEngine;

[AddComponentMenu("Streaming/Streaming Render Mesh Handle", 300)]
public class StreamingRenderMeshHandle : StreamingMeshHandle
{
	private MeshFilter _meshFilter;

	protected override void Awake()
	{
		_meshFilter = GetComponent<MeshFilter>();
		base.Awake();
	}

	public override void LoadMesh(Mesh mesh)
	{
		if (_meshFilter != null)
		{
			_meshFilter.sharedMesh = mesh;
		}
		base.LoadMesh(mesh);
	}

	public override void UnloadMesh()
	{
		if (_meshFilter != null && proxyMesh != null)
		{
			_meshFilter.sharedMesh = proxyMesh;
		}
		base.UnloadMesh();
	}
}
