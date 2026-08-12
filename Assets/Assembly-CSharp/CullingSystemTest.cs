using UnityEngine;

public class CullingSystemTest : MonoBehaviour
{
	public bool DEBUG;

	[SerializeField]
	private GameObject planetCenter;

	[SerializeField]
	[Range(-1f, 1f)]
	private float DOT;

	[SerializeField]
	private MeshRenderer[] _MeshRenderers;

	[SerializeField]
	private Transform[] _Transforms;

	[SerializeField]
	private Vector3[] _RenderCenters;

	private GameObject _Player;

	public void LoadMeshObjects()
	{
		MeshRenderer[] array = Object.FindObjectsOfType<MeshRenderer>();
		_RenderCenters = new Vector3[array.Length];
		_MeshRenderers = new MeshRenderer[array.Length];
		_Transforms = new Transform[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			_MeshRenderers[i] = array[i];
			_Transforms[i] = array[i].transform;
			_RenderCenters[i] = ComputeMeshCenter(array[i]);
		}
	}

	private Vector3 ComputeMeshCenter(MeshRenderer meshRenderer)
	{
		Vector3[] vertices = meshRenderer.GetComponent<MeshFilter>().sharedMesh.vertices;
		Vector3 vector = Vector3.positiveInfinity;
		Vector3 vector2 = Vector3.negativeInfinity;
		for (int i = 0; i < vertices.Length; i++)
		{
			vector = Vector3.Min(vector, vertices[i]);
			vector2 = Vector3.Max(vector2, vertices[i]);
		}
		return (vector + vector2) * 0.5f;
	}

	private void Update()
	{
		Initialize();
		CullMeshes();
	}

	private void Initialize()
	{
		if (_Player == null)
		{
			_Player = GameObject.FindGameObjectWithTag("Player");
		}
	}

	private void CullMeshes()
	{
		if (_MeshRenderers == null || _MeshRenderers.Length == 0)
		{
			return;
		}
		Vector3 position = planetCenter.transform.position;
		Vector3 position2 = _Player.transform.position;
		Vector3 lhs = Vector3.Normalize(position2 - position);
		Debug.DrawLine(position2, position, Color.blue);
		for (int i = 0; i < _MeshRenderers.Length; i++)
		{
			MeshRenderer meshRenderer = _MeshRenderers[i];
			if (meshRenderer != null)
			{
				Vector3 vector = _Transforms[i].TransformPoint(_RenderCenters[i]);
				Vector3 rhs = Vector3.Normalize(vector - position);
				if (DEBUG)
				{
					Debug.DrawLine(vector, position, Color.red);
				}
				float num = Vector3.Dot(lhs, rhs);
				meshRenderer.enabled = !(num < DOT);
			}
		}
	}
}
