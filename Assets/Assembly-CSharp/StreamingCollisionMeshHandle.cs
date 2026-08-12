using UnityEngine;

[AddComponentMenu("Streaming/Streaming Collision Mesh Handle", 300)]
public class StreamingCollisionMeshHandle : StreamingMeshHandle
{
	private const string k_sharedMeshSerializedPropertyName = "m_Mesh";

	private MeshCollider _meshCollider;

	private bool _isInitialized;

	protected override void Awake()
	{
		Initialize();
		base.Awake();
	}

	private void Initialize()
	{
		_meshCollider = GetComponent<MeshCollider>();
		if (_meshCollider == null)
		{
			Debug.LogError("StreamingCollision.Awake() found no attached collider");
		}
		_isInitialized = true;
	}

	public override void LoadMesh(Mesh mesh)
	{
		if (!_isInitialized)
		{
			Initialize();
		}
		if (_meshCollider != null)
		{
			_meshCollider.sharedMesh = mesh;
			if (mesh == null)
			{
				Debug.LogError("StreamingCollision has a MeshCollider to put the mesh in, BUT the mesh == null");
			}
			else if (mesh.isReadable)
			{
				Debug.LogError("Mesh (" + mesh.name + ") being streamed in from Asset Bundle (" + assetBundle + ") is RW enabled, but shouldn't be.");
			}
		}
		base.LoadMesh(mesh);
	}

	public override void UnloadMesh()
	{
		if (_meshCollider != null)
		{
			_meshCollider.sharedMesh = proxyMesh;
		}
		base.UnloadMesh();
	}

	public int GetMeshID()
	{
		return _meshCollider.sharedMesh.GetInstanceID();
	}
}
