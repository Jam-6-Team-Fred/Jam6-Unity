using UnityEngine;

[AddComponentMenu("Streaming/Streaming Mesh Handle", 300)]
public class StreamingMeshHandle : MonoBehaviour
{
	public delegate void StreamingMeshEvent();

	[SerializeField]
	public string assetBundle;

	[SerializeField]
	public int meshIndex;

	[SerializeField]
	public Mesh proxyMesh;

	public event StreamingMeshEvent OnMeshLoaded;

	public event StreamingMeshEvent OnMeshUnloaded;

	protected virtual void Awake()
	{
		if (!string.IsNullOrEmpty(assetBundle))
		{
			StreamingManager.RegisterStreamingMeshHandle(this);
		}
	}

	private void OnDestroy()
	{
		if (!string.IsNullOrEmpty(assetBundle))
		{
			StreamingManager.UnregisterStreamingMeshHandle(this);
		}
	}

	protected void InvokeOnMeshLoaded()
	{
		if (this.OnMeshLoaded != null)
		{
			this.OnMeshLoaded();
		}
	}

	protected void InvokeOnMeshUnloaded()
	{
		if (this.OnMeshUnloaded != null)
		{
			this.OnMeshUnloaded();
		}
	}

	public virtual void LoadMesh(Mesh mesh)
	{
		InvokeOnMeshLoaded();
	}

	public virtual void UnloadMesh()
	{
		InvokeOnMeshUnloaded();
	}
}
