using UnityEngine;

public class MeshRendererBoundsOverride : MonoBehaviour
{
	[SerializeField]
	private Vector3 _boundsSize = Vector3.one;

	[SerializeField]
	private MeshFilter[] _meshFilters = new MeshFilter[0];

	private void Reset()
	{
		_meshFilters = GetComponentsInChildren<MeshFilter>();
	}

	private void Awake()
	{
		for (int i = 0; i < _meshFilters.Length; i++)
		{
			if (_meshFilters[i].sharedMesh != null)
			{
				Bounds bounds = _meshFilters[i].sharedMesh.bounds;
				bounds.size = _boundsSize;
				_meshFilters[i].sharedMesh.bounds = bounds;
			}
		}
	}
}
