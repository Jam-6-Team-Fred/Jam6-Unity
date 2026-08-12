using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DecalProjector : MonoBehaviour
{
	public class ReverseFloatComparer : IComparer<float>
	{
		public int Compare(float x, float y)
		{
			if (x < y)
			{
				return 1;
			}
			if (x == y)
			{
				return 0;
			}
			return -1;
		}
	}

	public enum UVSplittingDimensions
	{
		TWO_BY_TWO = 0,
		FOUR_BY_FOUR = 1
	}

	protected const float EPSILON = 0.001f;

	protected static Vector3 NINF_VECTOR3 = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

	[SerializeField]
	public bool _drawMeshTriangles;

	[SerializeField]
	public bool _drawTrimmedTriangles;

	[SerializeField]
	public bool _staggerTrimmedTriangles;

	[SerializeField]
	public bool _drawProjectorCube = true;

	[SerializeField]
	public bool _drawMeshTangents;

	[SerializeField]
	protected Shader _shader;

	[SerializeField]
	private Texture2D _texture;

	[SerializeField]
	private Vector2 _textureUVMin = Vector2.zero;

	[SerializeField]
	private Vector2 _textureUVMax = new Vector2(1f, 1f);

	[SerializeField]
	public UVSplittingDimensions _tilingDimensions = UVSplittingDimensions.FOUR_BY_FOUR;

	[HideInInspector]
	[SerializeField]
	protected GameObject _projectedMesh;

	[HideInInspector]
	[SerializeField]
	protected string _projectionAssetGuid = string.Empty;

	protected Bounds _projectorBounds;

	protected Vector3 _minExtent;

	protected Vector3 _maxExtent;

	protected Vector3 _xyzPos;

	protected Vector3 _xzPos;

	protected Vector3 _yzPos;

	protected Vector3 _xyPos;

	protected Vector3 _z1Pos;

	protected Vector3 _y1Pos;

	protected Vector3 _x1Pos;

	protected Vector3 _zeroPos;

	protected Vector3[] _listCubeVectors;

	protected Plane _pXYAtZMin;

	protected Plane _pXYAtZMax;

	protected Plane _pXZAtYMin;

	protected Plane _pXZAtYMax;

	protected Plane _pYZAtXMin;

	protected Plane _pYZAtXMax;

	protected IDictionary<Plane, Vector3> _planeDict;

	protected List<Vector3> _meshTrianglePointList;

	protected List<List<Vector3>> _trimmedTrianglePointList;

	protected bool _buildMeshInUpdate;

	protected bool _saveMeshAsAsset;

	public Shader MyShader
	{
		get
		{
			return _shader;
		}
		set
		{
			_shader = value;
		}
	}

	protected Texture2D MyTexture => _texture;

	public Vector2 MyUVMin
	{
		get
		{
			return _textureUVMin;
		}
		set
		{
			_textureUVMin = value;
		}
	}

	public Vector2 MyUVMax
	{
		get
		{
			return _textureUVMax;
		}
		set
		{
			_textureUVMax = value;
		}
	}

	public virtual Texture2D GetInspectorTexture()
	{
		return null;
	}

	private void Start()
	{
		_projectorBounds = default(Bounds);
		_pXYAtZMin = default(Plane);
		_pXYAtZMax = default(Plane);
		_pXZAtYMin = default(Plane);
		_pXZAtYMax = default(Plane);
		_pYZAtXMin = default(Plane);
		_pYZAtXMax = default(Plane);
		_planeDict = new Dictionary<Plane, Vector3>();
		Gizmos.color = Color.white;
		_meshTrianglePointList = new List<Vector3>();
		_trimmedTrianglePointList = new List<List<Vector3>>();
	}

	protected virtual void SetSizeParameters(int x = -1, int z = -1)
	{
		_zeroPos = Vector3.zero - new Vector3(0.5f * base.transform.lossyScale.x, 0f, 0.5f * base.transform.lossyScale.z);
		_x1Pos = _zeroPos + new Vector3(base.transform.lossyScale.x, 0f, 0f);
		_y1Pos = _zeroPos - new Vector3(0f, base.transform.lossyScale.y, 0f);
		_z1Pos = _zeroPos + new Vector3(0f, 0f, base.transform.lossyScale.z);
		_xyPos = _x1Pos - new Vector3(0f, base.transform.lossyScale.y, 0f);
		_yzPos = _y1Pos + new Vector3(0f, 0f, base.transform.lossyScale.z);
		_xzPos = _z1Pos + new Vector3(base.transform.lossyScale.x, 0f, 0f);
		_xyzPos = _xzPos - new Vector3(0f, base.transform.lossyScale.y, 0f);
		Matrix4x4 matrix4x = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
		_zeroPos = matrix4x.MultiplyPoint3x4(_zeroPos);
		_x1Pos = matrix4x.MultiplyPoint3x4(_x1Pos);
		_y1Pos = matrix4x.MultiplyPoint3x4(_y1Pos);
		_z1Pos = matrix4x.MultiplyPoint3x4(_z1Pos);
		_xyPos = matrix4x.MultiplyPoint3x4(_xyPos);
		_yzPos = matrix4x.MultiplyPoint3x4(_yzPos);
		_xzPos = matrix4x.MultiplyPoint3x4(_xzPos);
		_xyzPos = matrix4x.MultiplyPoint3x4(_xyzPos);
		_listCubeVectors = new Vector3[8] { _zeroPos, _x1Pos, _y1Pos, _z1Pos, _xyPos, _xzPos, _yzPos, _xyzPos };
		float x2 = _zeroPos.x;
		float x3 = _zeroPos.x;
		float y = _zeroPos.y;
		float y2 = _zeroPos.y;
		float z2 = _zeroPos.z;
		float z3 = _zeroPos.z;
		Vector3[] listCubeVectors = _listCubeVectors;
		for (int i = 0; i < listCubeVectors.Length; i++)
		{
			Vector3 vector = listCubeVectors[i];
			if (vector.x > x3)
			{
				x3 = vector.x;
			}
			if (vector.x < x2)
			{
				x2 = vector.x;
			}
			if (vector.y > y2)
			{
				y2 = vector.y;
			}
			if (vector.y < y)
			{
				y = vector.y;
			}
			if (vector.z > z3)
			{
				z3 = vector.z;
			}
			if (vector.z < z2)
			{
				z2 = vector.z;
			}
		}
		_pXYAtZMin.Set3Points(_zeroPos, _x1Pos, _y1Pos);
		_pXYAtZMax.Set3Points(_yzPos, _xzPos, _z1Pos);
		_pXZAtYMin.Set3Points(_z1Pos, _x1Pos, _zeroPos);
		_pXZAtYMax.Set3Points(_y1Pos, _xyPos, _yzPos);
		_pYZAtXMin.Set3Points(_zeroPos, _y1Pos, _z1Pos);
		_pYZAtXMax.Set3Points(_xzPos, _xyPos, _x1Pos);
		if (_planeDict == null)
		{
			_planeDict = new Dictionary<Plane, Vector3>();
		}
		if (_planeDict.Count == 0)
		{
			_planeDict.Add(_pXYAtZMin, _zeroPos);
			_planeDict.Add(_pXYAtZMax, _xyzPos);
			_planeDict.Add(_pXZAtYMin, _zeroPos);
			_planeDict.Add(_pXZAtYMax, _xyzPos);
			_planeDict.Add(_pYZAtXMin, _zeroPos);
			_planeDict.Add(_pYZAtXMax, _xyzPos);
		}
		else if (_planeDict.Count == 6)
		{
			_planeDict.Clear();
			_planeDict.Add(_pXYAtZMin, _zeroPos);
			_planeDict.Add(_pXYAtZMax, _xyzPos);
			_planeDict.Add(_pXZAtYMin, _zeroPos);
			_planeDict.Add(_pXZAtYMax, _xyzPos);
			_planeDict.Add(_pYZAtXMin, _zeroPos);
			_planeDict.Add(_pYZAtXMax, _xyzPos);
		}
		_minExtent = new Vector3(x2, y, z2);
		_maxExtent = new Vector3(x3, y2, z3);
		_projectorBounds.SetMinMax(_minExtent, _maxExtent);
	}

	protected void DrawGuides()
	{
		Debug.DrawLine(_zeroPos, _x1Pos, Color.white);
		Debug.DrawLine(_zeroPos, _y1Pos, Color.white);
		Debug.DrawLine(_zeroPos, _z1Pos, Color.white);
		Debug.DrawLine(_x1Pos, _xyPos, Color.white);
		Debug.DrawLine(_y1Pos, _yzPos, Color.white);
		Debug.DrawLine(_z1Pos, _xzPos, Color.white);
		Debug.DrawLine(_y1Pos, _xyPos, Color.white);
		Debug.DrawLine(_z1Pos, _yzPos, Color.white);
		Debug.DrawLine(_x1Pos, _xzPos, Color.white);
		Debug.DrawLine(_xyzPos, _xyPos, Color.white);
		Debug.DrawLine(_xyzPos, _yzPos, Color.white);
		Debug.DrawLine(_xyzPos, _xzPos, Color.white);
		if (_drawMeshTriangles)
		{
			for (int i = 0; i < _meshTrianglePointList.Count; i += 3)
			{
				Debug.DrawLine(_meshTrianglePointList[i], _meshTrianglePointList[i + 1], Color.blue);
				Debug.DrawLine(_meshTrianglePointList[i + 2], _meshTrianglePointList[i + 1], Color.blue);
				Debug.DrawLine(_meshTrianglePointList[i], _meshTrianglePointList[i + 2], Color.blue);
			}
		}
		if (_drawTrimmedTriangles)
		{
			Vector3 zero = Vector3.zero;
			Vector3 vector = new Vector3(0f, 0.1f, 0f);
			foreach (List<Vector3> trimmedTrianglePoint in _trimmedTrianglePointList)
			{
				for (int j = 0; j < trimmedTrianglePoint.Count; j++)
				{
					if (j + 1 != trimmedTrianglePoint.Count)
					{
						Debug.DrawLine(trimmedTrianglePoint[j] + zero, trimmedTrianglePoint[j + 1] + zero, Color.red);
					}
					else
					{
						Debug.DrawLine(trimmedTrianglePoint[j] + zero, trimmedTrianglePoint[0] + zero, Color.red);
					}
				}
				if (_staggerTrimmedTriangles)
				{
					zero += vector;
				}
			}
		}
		if (_projectedMesh != null && _drawMeshTangents)
		{
			Mesh sharedMesh = _projectedMesh.GetRequiredComponent<MeshFilter>().sharedMesh;
			Vector3[] vertices = sharedMesh.vertices;
			Vector4[] tangents = sharedMesh.tangents;
			int vertexCount = sharedMesh.vertexCount;
			Matrix4x4 matrix4x = Matrix4x4.TRS(_projectedMesh.transform.position, _projectedMesh.transform.rotation, _projectedMesh.transform.lossyScale);
			for (int k = 0; k < vertexCount; k++)
			{
				Vector3 vector2 = matrix4x.MultiplyPoint3x4(vertices[k]);
				Vector3 vector3 = new Vector3(tangents[k].x, tangents[k].y, tangents[k].z);
				vector3.Scale(new Vector3(0.1f, 0.1f, 0.1f));
				Debug.DrawLine(vector2, vector2 + vector3, Color.green);
			}
		}
	}

	public void BuildMeshInNextUpdate(bool saveAsset)
	{
		_buildMeshInUpdate = true;
		_saveMeshAsAsset = saveAsset;
	}

	public MeshFilter[] BuildProjectedMeshes()
	{
		List<MeshFilter> list = new List<MeshFilter>();
		MeshFilter[] componentsInChildren = base.transform.root.GetComponentsInChildren<MeshFilter>();
		foreach (MeshFilter meshFilter in componentsInChildren)
		{
			if (!(meshFilter == null) && !meshFilter.transform.IsChildOf(base.transform))
			{
				Matrix4x4 sourceMeshRotationMatrix = Matrix4x4.TRS(meshFilter.transform.position, meshFilter.transform.rotation, meshFilter.transform.lossyScale);
				Mesh mesh = BuildProjectionMesh(meshFilter);
				if (mesh != null)
				{
					GameObject obj = new GameObject(meshFilter.transform.name + "_Decal");
					obj.transform.SetParent(meshFilter.transform);
					obj.transform.localPosition = Vector3.zero;
					obj.transform.localRotation = Quaternion.identity;
					obj.transform.localScale = Vector3.one;
					MeshFilter meshFilter2 = obj.gameObject.AddComponent<MeshFilter>();
					SetMeshFilter(meshFilter2, mesh, sourceMeshRotationMatrix);
					list.Add(meshFilter2);
				}
			}
		}
		return list.ToArray();
	}

	protected void SetMeshFilter(MeshFilter f, Mesh m, Matrix4x4 sourceMeshRotationMatrix)
	{
		Vector2[] array = new Vector2[m.vertices.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 point = m.vertices[i];
			Vector3 position = sourceMeshRotationMatrix.MultiplyPoint3x4(point);
			Vector3 vector = base.transform.InverseTransformPoint(position);
			float num = base.transform.lossyScale.x * 0.5f;
			float num2 = base.transform.lossyScale.z * 0.5f;
			Vector2 vector2 = new Vector2(Mathf.InverseLerp(num * -1f, num, vector.x), Mathf.InverseLerp(num2 * -1f, num2, vector.z));
			float x = OWMath.LerpUnclamped(_textureUVMin.x, _textureUVMax.x, vector2.x);
			float y = OWMath.LerpUnclamped(_textureUVMin.y, _textureUVMax.y, vector2.y);
			array[i] = new Vector2(x, y);
		}
		f.sharedMesh = m;
		f.sharedMesh.uv = array;
	}

	protected Mesh BuildProjectionMesh(MeshFilter f)
	{
		Mesh mesh = null;
		Matrix4x4 meshRotationMatrix = Matrix4x4.TRS(f.transform.position, f.transform.rotation, f.transform.lossyScale);
		if (f.GetComponent<MeshRenderer>() == null)
		{
			return mesh;
		}
		Vector3[] array = null;
		int[] array2 = null;
		List<int> outputTriangleList = new List<int>();
		List<Vector3> outputVertList = new List<Vector3>();
		List<Vector3> srcVertList = new List<Vector3>();
		outputVertList.Clear();
		outputTriangleList.Clear();
		srcVertList.Clear();
		if (_projectorBounds.Intersects(f.GetComponent<Renderer>().bounds))
		{
			srcVertList.AddRange(f.sharedMesh.vertices);
			array = f.sharedMesh.vertices;
			array2 = f.sharedMesh.triangles;
			for (int i = 0; i < array2.Length; i += 3)
			{
				if (IsInBounds(_projectorBounds, meshRotationMatrix.MultiplyPoint3x4(array[array2[i]]), meshRotationMatrix.MultiplyPoint3x4(array[array2[i + 1]]), meshRotationMatrix.MultiplyPoint3x4(array[array2[i + 2]])))
				{
					AddMeshTriangle(i, i + 1, i + 2, meshRotationMatrix, array2, ref srcVertList, ref outputVertList, ref outputTriangleList);
				}
			}
			if (outputTriangleList.Count > 0)
			{
				mesh = new Mesh();
				mesh.vertices = outputVertList.ToArray();
				mesh.triangles = outputTriangleList.ToArray();
				mesh.RecalculateNormals();
			}
		}
		return mesh;
	}

	private void AddMeshTriangle(int triIndex1, int triIndex2, int triIndex3, Matrix4x4 meshRotationMatrix, int[] triangleList, ref List<Vector3> srcVertList, ref List<Vector3> outputVertList, ref List<int> outputTriangleList)
	{
		int index = triangleList[triIndex1];
		int index2 = triangleList[triIndex2];
		int index3 = triangleList[triIndex3];
		Vector3 vector = meshRotationMatrix.MultiplyPoint3x4(srcVertList[index]);
		Vector3 vector2 = meshRotationMatrix.MultiplyPoint3x4(srcVertList[index2]);
		Vector3 vector3 = meshRotationMatrix.MultiplyPoint3x4(srcVertList[index3]);
		Matrix4x4 inverse = meshRotationMatrix.inverse;
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		bool flag = false;
		Vector3[] array = FindIntersectionPoints(vector, vector2);
		if (array.Length == 0)
		{
			flag = true;
		}
		else
		{
			Vector3[] array2 = array;
			foreach (Vector3 vector4 in array2)
			{
				if (!list.ApproxContains(vector4))
				{
					list.Add(vector4);
				}
			}
		}
		array = FindIntersectionPoints(vector2, vector3);
		if (array.Length == 0)
		{
			flag = true;
		}
		else
		{
			Vector3[] array2 = array;
			foreach (Vector3 vector5 in array2)
			{
				if (!list.ApproxContains(vector5))
				{
					list.Add(vector5);
				}
			}
		}
		array = FindIntersectionPoints(vector3, vector);
		if (array.Length == 0)
		{
			flag = true;
		}
		else
		{
			Vector3[] array2 = array;
			foreach (Vector3 vector6 in array2)
			{
				if (!list.ApproxContains(vector6))
				{
					list.Add(vector6);
				}
			}
		}
		if (flag && FindProjectorEdgeIntersectionPoint(vector, vector2, vector3, vector3, vector, out var intersectVectors))
		{
			foreach (Vector3 item2 in intersectVectors)
			{
				if (!list.ApproxContains(item2))
				{
					list.Add(item2);
				}
			}
		}
		if (list.Count > 2)
		{
			list = OrderPoints(list);
			list = MatchWinding(vector, vector2, vector3, list);
		}
		foreach (Vector3 item3 in list)
		{
			Vector3 item = inverse.MultiplyPoint3x4(item3);
			if (!outputVertList.Contains(item))
			{
				outputVertList.Add(item);
			}
			list2.Add(outputVertList.IndexOf(item));
		}
		if (list2.Count > 2)
		{
			int j = 0;
			int num = 0;
			for (; j < list2.Count - 2; j++)
			{
				outputTriangleList.Add(list2[0]);
				num++;
				outputTriangleList.Add(list2[num]);
				outputTriangleList.Add(list2[num + 1]);
			}
		}
		if (list.Count > 0)
		{
			_trimmedTrianglePointList.Add(list);
		}
		_meshTrianglePointList.Add(vector);
		_meshTrianglePointList.Add(vector2);
		_meshTrianglePointList.Add(vector3);
	}

	protected List<Vector3> OrderPoints(List<Vector3> points)
	{
		List<Vector3> list = new List<Vector3>();
		SortedDictionary<float, Vector3> sortedDictionary = new SortedDictionary<float, Vector3>(new ReverseFloatComparer());
		Vector3 normal = new Plane(points[0], points[1], points[2]).normal;
		Vector3 zero = Vector3.zero;
		foreach (Vector3 point in points)
		{
			zero += point;
		}
		zero /= (float)points.Count;
		Vector3 vector = points[0] - zero;
		sortedDictionary.Add(0f, points[0]);
		for (int i = 1; i < points.Count; i++)
		{
			Vector3 vector2 = points[i] - zero;
			Vector3 rhs = Vector3.Cross(vector2, vector);
			float num = Mathf.Sign(Vector3.Dot(normal, rhs));
			float key = Vector3.Angle(vector, vector2) * num;
			sortedDictionary.Add(key, points[i]);
		}
		list.AddRange(sortedDictionary.Values);
		return list;
	}

	protected List<Vector3> MatchWinding(Vector3 triangleV1, Vector3 triangleV2, Vector3 triangleV3, List<Vector3> points)
	{
		if (points.Count < 3)
		{
			return points;
		}
		Vector3 v = Vector3.Cross(triangleV1 - triangleV2, triangleV1 - triangleV3);
		Vector3 vector = Vector3.zero;
		bool flag = false;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (!flag)
		{
			num2 = num + 1;
			num3 = num + 2;
			if (num2 >= points.Count)
			{
				num2 -= points.Count;
			}
			if (num3 >= points.Count)
			{
				num3 -= points.Count;
			}
			vector = Vector3.Cross(points[num] - points[num2], points[num] - points[num3]);
			if (points.Count == 3)
			{
				flag = true;
			}
			else if (vector.ApproxEquals(Vector3.zero, 0.0001f))
			{
				num++;
				if (num >= points.Count)
				{
					new List<Vector3>().AddRange(points);
					Debug.LogError("Unable to determine winding of mesh polygon");
					break;
				}
			}
			else
			{
				flag = true;
			}
		}
		v.Normalize();
		vector.Normalize();
		if (!v.ApproxEquals(vector, 0.1f))
		{
			points.Reverse();
		}
		return points;
	}

	protected bool IsInBounds(Bounds box, Vector3 triangleV1, Vector3 triangleV2, Vector3 triangleV3)
	{
		float[] values = new float[3] { triangleV1.x, triangleV2.x, triangleV3.x };
		float[] values2 = new float[3] { triangleV1.y, triangleV2.y, triangleV3.y };
		float[] values3 = new float[3] { triangleV1.z, triangleV2.z, triangleV3.z };
		Vector3 max = new Vector3(Mathf.Max(values), Mathf.Max(values2), Mathf.Max(values3));
		Vector3 min = new Vector3(Mathf.Min(values), Mathf.Min(values2), Mathf.Min(values3));
		Bounds bounds = default(Bounds);
		bounds.SetMinMax(min, max);
		if (bounds.Intersects(box))
		{
			return true;
		}
		return false;
	}

	protected bool IsPointInsideProjector(Vector3 v)
	{
		bool result = true;
		List<Plane> list = new List<Plane>();
		list.AddRange(_planeDict.Keys);
		foreach (Plane item in list)
		{
			if (item.GetDistanceToPoint(v) > 0.001f)
			{
				result = false;
			}
		}
		return result;
	}

	protected Vector3[] FindIntersectionPoints(Vector3 v1, Vector3 v2)
	{
		SortedDictionary<float, Vector3> sortedDictionary = new SortedDictionary<float, Vector3>();
		SortedDictionary<float, Vector3> sortedDictionary2 = new SortedDictionary<float, Vector3>();
		bool flag = false;
		foreach (KeyValuePair<Plane, Vector3> item in _planeDict)
		{
			Plane key = item.Key;
			Vector3 intersection;
			switch (DoesSegmentPlaneIntersect(v1, v2, key, item.Value, out intersection))
			{
			case 1:
			{
				float key2 = Vector3.Magnitude(intersection - v1);
				if (!sortedDictionary.ContainsKey(key2))
				{
					sortedDictionary.Add(key2, intersection);
				}
				key2 = Vector3.Magnitude(intersection - v2);
				if (!sortedDictionary2.ContainsKey(key2))
				{
					sortedDictionary2.Add(key2, intersection);
				}
				break;
			}
			case 2:
				flag = true;
				break;
			}
		}
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		list.AddRange(sortedDictionary.Values);
		list2.AddRange(sortedDictionary2.Values);
		Vector3 vector = v1;
		Vector3 vector2 = v2;
		bool flag2 = IsPointInsideProjector(vector);
		if (!flag2)
		{
			foreach (Vector3 item2 in list)
			{
				vector = item2;
				flag2 = IsPointInsideProjector(vector);
				if (flag2)
				{
					break;
				}
			}
		}
		bool flag3 = IsPointInsideProjector(vector2);
		if (!flag3)
		{
			foreach (Vector3 item3 in list2)
			{
				vector2 = item3;
				flag3 = IsPointInsideProjector(vector2);
				if (flag3)
				{
					break;
				}
			}
		}
		if ((!flag2 && !flag3) || flag)
		{
			return new Vector3[0];
		}
		List<Vector3> list3 = new List<Vector3>();
		if (vector == vector2)
		{
			list3.Add(vector);
		}
		else
		{
			list3.Add(vector);
			list3.Add(vector2);
		}
		return list3.ToArray();
	}

	private bool FindProjectorEdgeIntersectionPoint(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 segmentV1, Vector3 segmentV2, out List<Vector3> intersectVectors)
	{
		intersectVectors = new List<Vector3>();
		SortedDictionary<float, Vector3> sortedDictionary = new SortedDictionary<float, Vector3>();
		List<Vector3> list = new List<Vector3>();
		Vector3 result = Vector3.zero;
		for (int i = 0; i < 12; i++)
		{
			Vector3 v4;
			Vector3 v5;
			switch (i)
			{
			case 0:
				v4 = _zeroPos;
				v5 = _x1Pos;
				break;
			case 1:
				v4 = _zeroPos;
				v5 = _y1Pos;
				break;
			case 2:
				v4 = _zeroPos;
				v5 = _z1Pos;
				break;
			case 3:
				v4 = _x1Pos;
				v5 = _xyPos;
				break;
			case 4:
				v4 = _x1Pos;
				v5 = _xzPos;
				break;
			case 5:
				v4 = _y1Pos;
				v5 = _xyPos;
				break;
			case 6:
				v4 = _y1Pos;
				v5 = _yzPos;
				break;
			case 7:
				v4 = _z1Pos;
				v5 = _xzPos;
				break;
			case 8:
				v4 = _z1Pos;
				v5 = _yzPos;
				break;
			case 9:
				v4 = _xyzPos;
				v5 = _xyPos;
				break;
			case 10:
				v4 = _xyzPos;
				v5 = _xzPos;
				break;
			case 11:
				v4 = _xyzPos;
				v5 = _yzPos;
				break;
			default:
				v4 = NINF_VECTOR3;
				v5 = NINF_VECTOR3;
				Debug.LogError("Well, that didn't work.");
				break;
			}
			if (!FindSegmentTriangleIntersection(v1, v2, v3, v4, v5, out result))
			{
				continue;
			}
			float num = ShortestDistanceToEitherPoint(segmentV1, segmentV2, result);
			if (sortedDictionary.ContainsKey(num))
			{
				list.Clear();
				list.AddRange(sortedDictionary.Values);
				if (!list.ApproxContains(result))
				{
					num += 0.001f;
					sortedDictionary.Add(num, result);
				}
			}
			else
			{
				sortedDictionary.Add(num, result);
			}
		}
		bool result2 = false;
		if (sortedDictionary.Count > 0)
		{
			result2 = true;
			intersectVectors.AddRange(sortedDictionary.Values);
		}
		return result2;
	}

	private float ShortestDistanceToEitherPoint(Vector3 v1, Vector3 v2, Vector3 v)
	{
		float magnitude = (v1 - v).magnitude;
		float magnitude2 = (v2 - v).magnitude;
		if (magnitude > magnitude2)
		{
			return magnitude;
		}
		return magnitude2;
	}

	private bool FindSegmentTriangleIntersection(Vector3 triV1, Vector3 triV2, Vector3 triV3, Vector3 v1, Vector3 v2, out Vector3 result)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		result = NINF_VECTOR3;
		Plane plane = new Plane(triV1, triV2, triV3);
		float distanceToPoint = plane.GetDistanceToPoint(v1);
		float distanceToPoint2 = plane.GetDistanceToPoint(v2);
		if (Mathf.Abs(distanceToPoint) <= 0.001f)
		{
			result = v1;
			flag3 = true;
		}
		if (Mathf.Abs(distanceToPoint2) <= 0.001f)
		{
			result = v2;
			flag4 = true;
		}
		bool flag5 = flag3 || flag4;
		bool num = flag3 && flag4;
		if (!flag5 && !plane.SameSide(v1, v2))
		{
			flag2 = true;
			Vector3 direction = Vector3.Normalize(v1 - v2);
			Ray ray = new Ray(v1, direction);
			if (!plane.Raycast(ray, out var enter) && Mathf.Abs(enter) <= 0.001f)
			{
				flag2 = false;
			}
			else
			{
				result = ray.GetPoint(enter);
			}
		}
		if (!num || flag2)
		{
			Vector3 vector = result - triV1;
			Vector3 vector2 = result - triV2;
			Vector3 vector3 = result - triV3;
			vector.Normalize();
			vector2.Normalize();
			vector3.Normalize();
			float f = Vector3.Dot(vector, vector2);
			float f2 = Vector3.Dot(vector2, vector3);
			float f3 = Vector3.Dot(vector3, vector);
			float num2 = Mathf.Acos(f) + Mathf.Acos(f2) + Mathf.Acos(f3);
			if (num2 >= 6.2821856f && num2 <= 6.2841854f)
			{
				flag = true;
			}
			if (!flag && result != NINF_VECTOR3)
			{
				Vector3 vector4 = NINF_VECTOR3;
				Vector3 vector5 = NINF_VECTOR3;
				Vector3 nINF_VECTOR = NINF_VECTOR3;
				Vector3 nINF_VECTOR2 = NINF_VECTOR3;
				for (int i = 0; i < 3; i++)
				{
					switch (i)
					{
					case 0:
						vector4 = triV2;
						vector5 = triV1;
						break;
					case 1:
						vector4 = triV3;
						vector5 = triV2;
						break;
					case 2:
						vector4 = triV1;
						vector5 = triV3;
						break;
					default:
						Debug.LogError("Something went horribly wrong. y u do dis");
						break;
					}
					nINF_VECTOR = vector4 - vector5;
					nINF_VECTOR2 = vector4 - result;
					if (!Vector3.Cross(nINF_VECTOR, nINF_VECTOR2).ApproxEquals(Vector3.zero))
					{
						continue;
					}
					float num3 = OWMath.InverseLerpUnclamped(vector4.x, vector5.x, result.x);
					bool flag6 = false;
					if (num3 >= -0.001f && num3 <= 1.001f)
					{
						flag6 = true;
					}
					else if (num3 == float.PositiveInfinity && Mathf.Abs(vector4.x - result.x) < 0.001f)
					{
						flag6 = true;
					}
					if (!flag6)
					{
						continue;
					}
					num3 = OWMath.InverseLerpUnclamped(vector4.y, vector5.y, result.y);
					flag6 = false;
					if (num3 >= -0.001f && num3 <= 1.001f)
					{
						flag6 = true;
					}
					else if (num3 == float.PositiveInfinity && Mathf.Abs(vector4.y - result.y) < 0.001f)
					{
						flag6 = true;
					}
					if (flag6)
					{
						num3 = OWMath.InverseLerpUnclamped(vector4.z, vector5.z, result.z);
						flag6 = false;
						if (num3 >= -0.001f && num3 <= 1.001f)
						{
							flag6 = true;
						}
						else if (num3 == float.PositiveInfinity && Mathf.Abs(vector4.z - result.z) < 0.001f)
						{
							flag6 = true;
						}
						if (flag6)
						{
							flag = true;
						}
					}
				}
			}
		}
		return flag;
	}

	private int DoesSegmentPlaneIntersect(Vector3 v1, Vector3 v2, Plane p, Vector3 vOnPlane, out Vector3 intersection)
	{
		intersection = Vector3.zero;
		Vector3 vector = v2 - v1;
		Vector3 rhs = v1 - vOnPlane;
		float num = Vector3.Dot(p.normal, vector);
		float num2 = -1f * Vector3.Dot(p.normal, rhs);
		if (Mathf.Abs(num) < 0.001f)
		{
			if (num2 <= 0.001f)
			{
				return 2;
			}
			return 0;
		}
		float num3 = num2 / num;
		if (num3 < 0f || num3 > 1f)
		{
			return 0;
		}
		intersection = v1 + num3 * vector;
		return 1;
	}
}
