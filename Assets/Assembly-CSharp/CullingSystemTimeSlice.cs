using UnityEngine;

public class CullingSystemTimeSlice : MonoBehaviour
{
	private enum PLANET_SIDE
	{
		INTERIOR = 0,
		EXTERIOR = 1
	}

	private struct CULLING
	{
		public MeshRenderer _MeshRenderer;

		public Vector3 _MeshPosition;

		public Vector3 _PlanetCenterPosition;

		public Vector3 _PlayerPosition;

		public Vector3 _vPlayerDirection;

		public Vector3 _vMeshDirection;

		public PLANET_SIDE _SidePlayer;

		public PLANET_SIDE _SideMesh;
	}

	[SerializeField]
	private GameObject _CullingRoot;

	[SerializeField]
	private GameObject _CullingCenter;

	[SerializeField]
	private float _InteriorRadius;

	public bool DEBUG;

	public int _FrameSlice;

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

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 1f, 0f, 0.75f);
		Gizmos.DrawSphere(_CullingCenter.transform.position, _InteriorRadius);
	}

	public void LoadMeshObjects()
	{
		MeshRenderer[] componentsInChildren = _CullingRoot.GetComponentsInChildren<MeshRenderer>();
		_RenderCenters = new Vector3[componentsInChildren.Length];
		_MeshRenderers = new MeshRenderer[componentsInChildren.Length];
		_Transforms = new Transform[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			_MeshRenderers[i] = componentsInChildren[i];
			_Transforms[i] = componentsInChildren[i].transform;
			_RenderCenters[i] = ComputeMeshCenter(componentsInChildren[i]);
		}
	}

	private Vector3 ComputeMeshCenter(MeshRenderer meshRenderer)
	{
		MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
		if (component == null)
		{
			return Vector3.zero;
		}
		Mesh sharedMesh = component.sharedMesh;
		if (sharedMesh == null)
		{
			return Vector3.zero;
		}
		Vector3[] vertices = sharedMesh.vertices;
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
		int num = Time.frameCount % _FrameSlice;
		int num2 = _MeshRenderers.Length;
		int num3 = (int)((float)num2 / (float)_FrameSlice);
		int num4 = num * num3;
		int num5 = (num + 1) % _FrameSlice * num3;
		num5 = ((num5 == 0) ? num2 : num5);
		if (DEBUG)
		{
			MonoBehaviour.print("FrameSlice: " + num + "Start: " + num4 + " End: " + num5);
		}
		CullMeshes(num4, num5);
	}

	private void Initialize()
	{
		if (_Player == null)
		{
			_Player = GameObject.FindGameObjectWithTag("Player");
		}
	}

	private PLANET_SIDE EvaluateSide(Vector3 center, float radius, Vector3 position)
	{
		if (!(Vector3.Distance(center, position) <= radius))
		{
			return PLANET_SIDE.EXTERIOR;
		}
		return PLANET_SIDE.INTERIOR;
	}

	private void CullMeshes(int start, int end)
	{
		if (_MeshRenderers == null || _MeshRenderers.Length == 0)
		{
			return;
		}
		CULLING _Culling = default(CULLING);
		_Culling._PlanetCenterPosition = _CullingCenter.transform.position;
		_Culling._PlayerPosition = _Player.transform.position;
		_Culling._SidePlayer = EvaluateSide(_Culling._PlanetCenterPosition, _InteriorRadius, _Culling._PlayerPosition);
		_Culling._vPlayerDirection = Vector3.Normalize(_Culling._PlayerPosition - _Culling._PlanetCenterPosition);
		Debug.DrawLine(_Culling._PlayerPosition, _Culling._PlanetCenterPosition, Color.blue);
		for (int i = start; i < end; i++)
		{
			_Culling._MeshRenderer = _MeshRenderers[i];
			if (_Culling._MeshRenderer != null)
			{
				_Culling._MeshPosition = _Transforms[i].TransformPoint(_RenderCenters[i]);
				_Culling._SideMesh = EvaluateSide(_Culling._PlanetCenterPosition, _InteriorRadius, _Culling._MeshPosition);
				if (_Culling._SidePlayer == PLANET_SIDE.EXTERIOR && _Culling._SideMesh == PLANET_SIDE.EXTERIOR)
				{
					CullingExterior(ref _Culling);
				}
				if (_Culling._SidePlayer == PLANET_SIDE.INTERIOR && _Culling._SideMesh == PLANET_SIDE.INTERIOR)
				{
					CullingInterior(ref _Culling);
				}
				if (_Culling._SidePlayer != _Culling._SideMesh)
				{
					CullingOpposite(ref _Culling);
				}
			}
		}
	}

	private void CullingExterior(ref CULLING _Culling)
	{
		Vector3 rhs = Vector3.Normalize(_Culling._MeshPosition - _Culling._PlanetCenterPosition);
		if (DEBUG)
		{
			Debug.DrawLine(_Culling._MeshPosition, _Culling._PlanetCenterPosition, Color.red);
		}
		float num = Vector3.Dot(_Culling._vPlayerDirection, rhs);
		_Culling._MeshRenderer.enabled = !(num < DOT);
	}

	private void CullingInterior(ref CULLING _Culling)
	{
		_Culling._MeshRenderer.enabled = true;
	}

	private void CullingOpposite(ref CULLING _Culling)
	{
		Debug.DrawRay(_Culling._PlayerPosition, Vector3.Normalize(_Culling._MeshPosition - _Culling._PlayerPosition) * 512f, Color.green);
		if (Physics.Raycast(_Culling._PlayerPosition, Vector3.Normalize(_Culling._MeshPosition - _Culling._PlayerPosition), out var hitInfo, float.PositiveInfinity))
		{
			if (hitInfo.collider == null)
			{
				_Culling._MeshRenderer.enabled = true;
			}
			else if (Vector3.Distance(_Culling._PlanetCenterPosition, hitInfo.point) < _InteriorRadius)
			{
				_Culling._MeshRenderer.enabled = true;
			}
			else
			{
				_Culling._MeshRenderer.enabled = false;
			}
		}
	}
}
