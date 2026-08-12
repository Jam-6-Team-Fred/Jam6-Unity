using System;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class BaseRiverAudioPath : MonoBehaviour
{
	protected struct MeshGlobal
	{
		public Mesh mesh;

		public Transform globalTransform;
	}

	[Serializable]
	protected struct VertexPair
	{
		public Vector3 v0;

		public Vector3 v1;
	}

	[Serializable]
	public struct Triangle
	{
		public Vector3 v0;

		public Vector3 v1;

		public Vector3 v2;

		public Vector3 centroid;

		public Vector3 normal;

		public float cachedDegree;

		public byte allowedEdgeFlags;

		public short[] adjacency;

		public Vector3 this[int i]
		{
			get
			{
				switch (i)
				{
				case 0:
					return v0;
				case 1:
					return v1;
				case 2:
					return v2;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		public Triangle(Vector3 p0, Vector3 p1, Vector3 p2)
		{
			v0 = p0;
			v1 = p1;
			v2 = p2;
			centroid = (v0 + v1 + v2) / 3f;
			normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
			cachedDegree = 0f;
			allowedEdgeFlags = 0;
			adjacency = new short[3] { -1, -1, -1 };
		}

		public Triangle(Triangle t)
		{
			v0 = t.v0;
			v1 = t.v1;
			v2 = t.v2;
			centroid = t.centroid;
			normal = t.normal;
			cachedDegree = t.cachedDegree;
			allowedEdgeFlags = t.allowedEdgeFlags;
			adjacency = new short[3] { -1, -1, -1 };
		}

		private Vector3 CalcCentroid()
		{
			return (v0 + v1 + v2) / 3f;
		}

		public bool RayIntersect(Ray ray, float limit = float.PositiveInfinity)
		{
			Plane plane = new Plane(normal, v0);
			float enter = 0f;
			if (plane.Raycast(ray, out enter))
			{
				if (enter > limit)
				{
					return false;
				}
				Vector3 planeIntersectPoint = ray.origin + ray.direction * enter;
				CalcParametric(planeIntersectPoint, out var s, out var t);
				if ((double)s < 0.0 || (double)s > 1.0)
				{
					return false;
				}
				if ((double)t < 0.0 || (double)(s + t) > 1.0)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public bool RayHitEdge(Ray ray, out Vector3 hitPoint, bool testAllowed = true)
		{
			hitPoint = Vector3.zero;
			for (int i = 0; i < 3; i++)
			{
				if ((allowedEdgeFlags & (1 << i)) > 0 && testAllowed)
				{
					continue;
				}
				GetEdge(i, out var p, out var p2);
				Plane plane = new Plane(-Vector3.Cross((p2 - p).normalized, normal), p);
				if (plane.Raycast(ray, out var enter) && !(enter < 0f))
				{
					Vector3 vector = ray.origin + ray.direction * enter;
					if (!(Vector3.Dot(p2 - p, vector - p) < 0f) && !(Vector3.Dot(p - p2, vector - p2) < 0f))
					{
						hitPoint = OWMath.ClosestPointOnSegment(vector, p, p2);
						return true;
					}
				}
			}
			return false;
		}

		public void CalcParametric(Vector3 planeIntersectPoint, out float s, out float t)
		{
			Vector3 vector = v1 - v0;
			Vector3 vector2 = v2 - v0;
			Vector3 lhs = planeIntersectPoint - v0;
			float num = Vector3.Dot(vector, vector);
			float num2 = Vector3.Dot(vector, vector2);
			float num3 = Vector3.Dot(vector2, vector2);
			float num4 = Vector3.Dot(lhs, vector);
			float num5 = Vector3.Dot(lhs, vector2);
			float num6 = num2 * num2 - num * num3;
			s = (num2 * num5 - num3 * num4) / num6;
			t = (num2 * num4 - num * num5) / num6;
		}

		public Vector3 CalcBarycentric(Vector3 point)
		{
			Vector3 zero = Vector3.zero;
			CalcParametric(point, out var s, out var t);
			zero.x = 1f - s - t;
			zero.y = s;
			zero.z = t;
			return zero;
		}

		public static Vector3 ClampBarycentric(Vector3 barycentric)
		{
			int num = 0;
			float num2 = 0f;
			for (int i = 0; i < 3; i++)
			{
				if (barycentric[i] < 0f)
				{
					num++;
					num2 += barycentric[i];
				}
			}
			if (num <= 0)
			{
				return barycentric;
			}
			if (num == 1)
			{
				num2 /= 2f;
			}
			for (int j = 0; j < 3; j++)
			{
				if (barycentric[j] >= 0f)
				{
					barycentric[j] += num2;
				}
				else if (barycentric[j] < 0f)
				{
					barycentric[j] = 0f;
				}
			}
			return barycentric;
		}

		public static bool IsInsideBarycentric(Vector3 barycentric)
		{
			if (barycentric.x >= 0f && barycentric.y >= 0f && barycentric.z >= 0f && barycentric.x <= 1f && barycentric.y <= 1f)
			{
				return barycentric.z <= 1f;
			}
			return false;
		}

		public Vector3 BarycentricToCartesian(Vector3 barycentric)
		{
			return barycentric.x * v0 + barycentric.y * v1 + barycentric.z * v2;
		}

		public bool ContainsPoint(Vector3 point, out Vector3 barycentric)
		{
			barycentric = CalcBarycentric(point);
			if (barycentric.x < 0f || barycentric.x > 1f || barycentric.y < 0f || barycentric.y > 1f || barycentric.z < 0f || barycentric.z > 1f)
			{
				return false;
			}
			return true;
		}

		public Vector3 GetEdge(int edgeIdx, out Vector3 p1, out Vector3 p2)
		{
			switch (edgeIdx)
			{
			case 0:
				p1 = v0;
				p2 = v1;
				return v1 - v0;
			case 1:
				p1 = v1;
				p2 = v2;
				return v2 - v1;
			case 2:
				p1 = v2;
				p2 = v0;
				return v0 - v2;
			default:
				p1 = v0;
				p2 = v1;
				return Vector3.zero;
			}
		}
	}

	protected const float PLANE_X_MULTIPLIER = 4f;

	protected const float LOOKUP_ANGLE_EPSILON = 5f;

	[Tooltip("Roots of meshes to take into account as terrain go here")]
	[SerializeField]
	protected Transform[] _includedMeshRoots;

	[Tooltip("Meshes that are children of the above root but are to be excluded")]
	[SerializeField]
	protected Transform[] _meshExceptions;

	[SerializeField]
	protected int sampledTrianglesCount = 40;

	[SerializeField]
	protected float minY;

	[SerializeField]
	protected float maxY;

	[SerializeField]
	protected bool _loop = true;

	[SerializeField]
	protected Mesh _meshToImport;

	[HideInInspector]
	[SerializeField]
	protected VertexPair[] voronoiLines;

	[FormerlySerializedAs("_preFloodTriangles")]
	[HideInInspector]
	[SerializeField]
	protected Triangle[] _baseFloodTriangles;

	[FormerlySerializedAs("_cachedPreFloodDegreeSections")]
	[HideInInspector]
	[SerializeField]
	protected float[] _cachedTriangleSections;

	[HideInInspector]
	[SerializeField]
	protected bool _importedMesh;

	[Space]
	[Header("Editor Tools")]
	public bool showVoronoi;

	public bool showPostFlood;

	[SerializeField]
	private bool _showCentroidLabels = true;

	[SerializeField]
	private bool _showAdjacency;

	public bool importedMesh => _importedMesh;

	public bool hasBaseData => _baseFloodTriangles != null;

	protected virtual Triangle[] CurrentTriangleArrayForPoint(Vector3 point)
	{
		return _baseFloodTriangles;
	}

	protected virtual float[] CurrentCachedTriangleSectionsForPoint(Vector3 point)
	{
		return _cachedTriangleSections;
	}

	protected abstract float GetWaterLevelAtPointForBake(Vector3 point, Triangle[] workedOnTriArray = null);

	protected abstract float GetRuntimeWaterLevelAtPoint(Vector3 point);

	public virtual Vector3 GetWaterPoint(Vector3 worldPos, ref int currentTriIdx, ref int currentEdgeIdx, out Triangle currentTriangle, out Vector3 normal, out Vector3 desiredPoint, out bool inside)
	{
		Vector3 vector = base.transform.InverseTransformPoint(worldPos);
		desiredPoint = DesiredPointOnWater(vector);
		Triangle[] array = CurrentTriangleArrayForPoint(worldPos);
		float[] cachedSections = CurrentCachedTriangleSectionsForPoint(worldPos);
		bool flag = false;
		inside = false;
		if (currentTriIdx < 0 || currentTriIdx >= array.Length)
		{
			flag = true;
		}
		currentTriangle = default(Triangle);
		Vector3 barycentric = Vector3.zero;
		if (!flag)
		{
			currentTriangle = array[currentTriIdx];
			if (!currentTriangle.ContainsPoint(vector, out barycentric))
			{
				flag = true;
			}
			else
			{
				inside = true;
			}
		}
		if (flag)
		{
			currentTriangle = FindTriangle(vector, currentTriIdx, out currentTriIdx, out currentEdgeIdx, out barycentric, array, cachedSections);
			inside = Triangle.IsInsideBarycentric(barycentric);
			if (currentTriIdx < 0)
			{
				normal = Vector3.up;
				return Vector3.zero;
			}
			barycentric = Triangle.ClampBarycentric(barycentric);
		}
		normal = base.transform.TransformDirection(currentTriangle.normal);
		if (inside)
		{
			return base.transform.TransformPoint(desiredPoint);
		}
		currentTriangle.GetEdge(currentEdgeIdx, out var p, out var p2);
		return base.transform.TransformPoint(p + Vector3.Project(desiredPoint - p, p2 - p));
	}

	public abstract Vector3 DesiredPointOnWater(Vector3 localPos);

	private Triangle FindTriangle(Vector3 localPos, int lastIdx, out int triIdx, out int edgeIdx, out Vector3 barycentric, Triangle[] triArray, float[] cachedSections)
	{
		triIdx = -1;
		edgeIdx = 0;
		if (lastIdx >= 0 && lastIdx < triArray.Length)
		{
			Triangle triangle = triArray[lastIdx];
			for (int i = 0; i < 3; i++)
			{
				if (triangle.adjacency[i] >= 0)
				{
					Triangle result = triArray[triangle.adjacency[i]];
					if (result.ContainsPoint(localPos, out barycentric))
					{
						triIdx = triangle.adjacency[i];
						return result;
					}
				}
			}
		}
		float num = OrderedPosition(localPos);
		int pivot = 0;
		for (int j = 0; j < triArray.Length; j++)
		{
			if (triArray[j].cachedDegree > num)
			{
				pivot = j;
				break;
			}
		}
		FindDeepSearchIndexes(pivot, triArray, out var startSearchIdx, out var endSearchIdx);
		if (startSearchIdx < 0)
		{
			endSearchIdx += endSearchIdx + triArray.Length;
			startSearchIdx = triArray.Length + startSearchIdx;
		}
		int num2 = -1;
		float num3 = float.MaxValue;
		Vector3 vector = Vector3.zero;
		for (int k = startSearchIdx; k <= endSearchIdx; k++)
		{
			int num4 = ((k >= triArray.Length) ? (k - triArray.Length) : k);
			if (triArray[num4].ContainsPoint(localPos, out barycentric))
			{
				triIdx = num4;
				return triArray[num4];
			}
			for (int l = 0; l < 3; l++)
			{
				if ((triArray[num4].allowedEdgeFlags & (1 << l)) > 0)
				{
					continue;
				}
				float num5 = float.MaxValue;
				Vector3 zero = Vector3.zero;
				int i2 = 0;
				int i3 = 1;
				switch (l)
				{
				case 1:
					i2 = 1;
					i3 = 2;
					break;
				case 2:
					i2 = 2;
					i3 = 0;
					break;
				}
				zero = localPos - triArray[num4][i2];
				Vector3 vector2 = triArray[num4][i3] - triArray[num4][i2];
				Vector3 vector3 = Vector3.Project(vector2, zero);
				if (!(Vector3.Dot(vector3, vector2) < 0f) && !(vector3.sqrMagnitude > vector2.sqrMagnitude))
				{
					num5 = (zero - vector3).sqrMagnitude;
					if (num5 < num3)
					{
						edgeIdx = l;
						num2 = num4;
						num3 = num5;
						vector = barycentric;
					}
				}
			}
		}
		triIdx = num2;
		barycentric = vector;
		if (num2 < 0)
		{
			Debug.LogWarning("Failed to find triangle");
			return triArray[0];
		}
		return triArray[num2];
	}

	protected virtual void FindDeepSearchIndexes(int pivot, Triangle[] triArray, out int startSearchIdx, out int endSearchIdx)
	{
		startSearchIdx = 0;
		endSearchIdx = triArray.Length - 1;
	}

	protected abstract float GetPaddedLookupTriangleHorizontalPosition(float inLookupHorizontalPosition, float inTriHorizontalPosition);

	public Triangle TryFindAcross(Vector3 startPtWorld, Vector3 dirWorld, int originTriIdx, out int triIdx, out Vector3 hitPoint)
	{
		Triangle[] array = CurrentTriangleArrayForPoint(startPtWorld);
		triIdx = -1;
		hitPoint = Vector3.zero;
		FindDeepSearchIndexes(originTriIdx, array, out var startSearchIdx, out var endSearchIdx);
		if (startSearchIdx < 0)
		{
			endSearchIdx += endSearchIdx + array.Length;
			startSearchIdx = array.Length + startSearchIdx;
		}
		Vector3 vector = base.transform.InverseTransformPoint(startPtWorld);
		Ray ray = new Ray(vector, base.transform.InverseTransformDirection(dirWorld));
		float num = float.MaxValue;
		for (int i = startSearchIdx; i <= endSearchIdx; i++)
		{
			int num2 = ((i >= array.Length) ? (i - array.Length) : i);
			if (num2 != originTriIdx && array[num2].RayHitEdge(ray, out var hitPoint2))
			{
				float sqrMagnitude = (hitPoint2 - vector).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					triIdx = num2;
					num = sqrMagnitude;
					hitPoint = hitPoint2;
				}
			}
		}
		if (triIdx > 0)
		{
			return array[triIdx];
		}
		return default(Triangle);
	}

	private int EdgeDistance(int triIdxStart, int triIdxEnd)
	{
		if (triIdxStart == triIdxEnd)
		{
			return 1;
		}
		return 0;
	}

	protected void Generate(ref Triangle[] triArray, ref float[] triSectionsCache)
	{
		List<MeshGlobal> meshes = new List<MeshGlobal>();
		CollectMeshes(meshes);
		List<VertexPair> list = new List<VertexPair>();
		GenerateVertexPairs(list, meshes, triArray);
		Debug.Log("found " + list.Count + " pairs");
		Vector3[] midPoints = new Vector3[0];
		MakeMidPoints(ref midPoints, list, triArray);
		Debug.Log("minY " + minY + " maxY " + maxY);
		GenerateTriangles(ref triArray, ref triSectionsCache, midPoints, meshes);
	}

	private void CollectMeshes(List<MeshGlobal> meshes)
	{
		Transform[] includedMeshRoots = _includedMeshRoots;
		for (int i = 0; i < includedMeshRoots.Length; i++)
		{
			MeshCollider[] componentsInChildren = includedMeshRoots[i].GetComponentsInChildren<MeshCollider>();
			foreach (MeshCollider meshCollider in componentsInChildren)
			{
				bool flag = false;
				Transform[] meshExceptions = _meshExceptions;
				foreach (Transform transform in meshExceptions)
				{
					if (meshCollider.transform == transform)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					meshes.Add(new MeshGlobal
					{
						mesh = meshCollider.sharedMesh,
						globalTransform = meshCollider.transform
					});
				}
			}
		}
	}

	private void GenerateVertexPairs(List<VertexPair> vertexPairs, List<MeshGlobal> meshes, Triangle[] workedOnTriArray)
	{
		foreach (MeshGlobal mesh in meshes)
		{
			_ = mesh.mesh.vertexCount;
			Vector3[] vertices = mesh.mesh.vertices;
			for (int i = 0; i < mesh.mesh.subMeshCount; i++)
			{
				int[] indices = mesh.mesh.GetIndices(i);
				for (int j = 0; j < indices.Length; j++)
				{
					int num = j % 3;
					int num2 = j;
					switch (num)
					{
					case 0:
					case 1:
						num2 = j + 1;
						break;
					case 2:
						num2 = j - 2;
						break;
					}
					if (num2 < indices.Length)
					{
						Vector3 v = mesh.globalTransform.TransformPoint(vertices[indices[j]]);
						Vector3 v2 = mesh.globalTransform.TransformPoint(vertices[indices[num2]]);
						AddPairIfIntersect(vertexPairs, workedOnTriArray, v, v2);
					}
				}
			}
		}
	}

	protected abstract void AddPairIfIntersect(List<VertexPair> vertexPairs, Triangle[] workedOnTriArray, Vector3 v1, Vector3 v2);

	protected abstract float PointHeight(Vector3 point);

	private void MakeMidPoints(ref Vector3[] midPoints, List<VertexPair> vertexPairs, Triangle[] workedOnTriArray)
	{
		List<Vector3> list = new List<Vector3>();
		foreach (VertexPair vertexPair in vertexPairs)
		{
			float num = PointHeight(vertexPair.v0);
			float num2 = PointHeight(vertexPair.v1);
			float waterLevelAtPointForBake = GetWaterLevelAtPointForBake(vertexPair.v0, workedOnTriArray);
			float waterLevelAtPointForBake2 = GetWaterLevelAtPointForBake(vertexPair.v1, workedOnTriArray);
			float num3 = ((waterLevelAtPointForBake + waterLevelAtPointForBake2) / 2f - num) / (num2 - num);
			Vector3 item = vertexPair.v0 + (vertexPair.v1 - vertexPair.v0) * num3;
			list.Add(item);
		}
		list.Sort((Vector3 v1, Vector3 v2) => Mathf.RoundToInt(Mathf.Sign(OrderedPosition(v1) - OrderedPosition(v2))));
		midPoints = list.ToArray();
	}

	private void GenerateTriangles(ref Triangle[] triangleArray, ref float[] cachedHorizontal, Vector3[] midPoints, List<MeshGlobal> meshes)
	{
		List<Vector2> list = new List<Vector2>();
		List<uint> list2 = new List<uint>();
		for (int i = 0; i < midPoints.Length; i++)
		{
			list2.Add(0u);
			list.Add(WorldToVoronoi(midPoints[i]));
		}
		if (_loop)
		{
			PadVoronoiPoints(midPoints, list, list2);
		}
		Voronoi voronoi = new Voronoi(list, list2, GetVoronoiBounds());
		List<LineSegment> list3 = voronoi.VoronoiDiagram();
		voronoiLines = new VertexPair[list3.Count];
		for (int j = 0; j < list3.Count; j++)
		{
			voronoiLines[j] = new VertexPair
			{
				v0 = VoronoiToLocal(list3[j].p0.Value, triangleArray),
				v1 = VoronoiToLocal(list3[j].p1.Value, triangleArray)
			};
		}
		voronoi.SpanningTree();
		voronoi.DelaunayTriangulation();
		List<Delaunay.Triangle> triangles = voronoi.GetTriangles();
		if (_loop)
		{
			RemovePaddingTris(triangles);
		}
		List<Triangle> list4 = new List<Triangle>();
		for (int k = 0; k < triangles.Count; k++)
		{
			Triangle triangle = new Triangle(VoronoiToLocal(triangles[k].sites[0].Coord, triangleArray), VoronoiToLocal(triangles[k].sites[1].Coord, triangleArray), VoronoiToLocal(triangles[k].sites[2].Coord, triangleArray));
			bool flag = false;
			foreach (MeshGlobal mesh in meshes)
			{
				if (PointUnderMesh(triangle.centroid, triangle.normal, GetTriRaycastLimit(triangle, triangleArray), mesh.mesh, mesh.globalTransform))
				{
					flag = true;
					break;
				}
			}
			bool flag2 = triangle.centroid.y > maxY || triangle.centroid.y < minY;
			if (!flag && !flag2)
			{
				triangle.cachedDegree = OrderedPosition(triangle.centroid);
				list4.Add(triangle);
			}
		}
		for (int l = 0; l < list4.Count; l++)
		{
			byte b = 0;
			short[] array = new short[3] { -1, -1, -1 };
			for (int m = 0; m < list4.Count; m++)
			{
				if (m == l)
				{
					continue;
				}
				byte b2 = 0;
				for (int n = 0; n < 3; n++)
				{
					for (int num = 0; num < 3; num++)
					{
						if (Vector3.Distance(list4[l][n], list4[m][num]) < 0.001f)
						{
							b2 += (byte)(1 << n);
						}
					}
				}
				switch (b2)
				{
				case 3:
					b++;
					array[0] = (short)m;
					break;
				case 6:
					b += 2;
					array[1] = (short)m;
					break;
				case 5:
					b += 4;
					array[2] = (short)m;
					break;
				}
			}
			Triangle value = new Triangle(list4[l]);
			value.allowedEdgeFlags = b;
			value.adjacency = array;
			list4[l] = value;
		}
		FilterDupeTris(list4);
		list4.Sort((Triangle t1, Triangle t2) => Mathf.RoundToInt(Mathf.Sign(t1.cachedDegree - t2.cachedDegree)));
		triangleArray = list4.ToArray();
		cachedHorizontal = new float[3];
		cachedHorizontal[0] = triangleArray[triangleArray.Length / 4].cachedDegree;
		cachedHorizontal[1] = triangleArray[triangleArray.Length / 2].cachedDegree;
		cachedHorizontal[2] = triangleArray[triangleArray.Length / 4 * 3].cachedDegree;
		Debug.Log("tri count = " + list4.Count);
		voronoi.Dispose();
	}

	protected abstract Rect GetVoronoiBounds();

	protected abstract void PadVoronoiPoints(Vector3[] midPoints, List<Vector2> mids2D, List<uint> colors);

	protected abstract void RemovePaddingTris(List<Delaunay.Triangle> delTris);

	private void FilterDupeTris(List<Triangle> triangles)
	{
		for (int i = 0; i < triangles.Count - 1; i++)
		{
			Vector3 centroid = triangles[i].centroid;
			int num = i + 1;
			while (num < triangles.Count)
			{
				if (Vector3.Dot(triangles[num].CalcBarycentric(centroid), Vector3.one) - 0.9f <= 0.001f)
				{
					triangles.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
	}

	public Mesh GenerateMesh(Triangle[] triArray)
	{
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<List<Vector3>> list3 = new List<List<Vector3>>();
		List<Vector3> list4 = new List<Vector3>();
		int num = 0;
		for (int i = 0; i < triArray.Length; i++)
		{
			Triangle triangle = triArray[i];
			Vector3[] array = null;
			array = ((!(Vector3.Dot(Vector3.Cross(triangle[1] - triangle[0], triangle[2] - triangle[1]), triangle.normal) < 0f)) ? new Vector3[3]
			{
				triangle[2],
				triangle[1],
				triangle[0]
			} : new Vector3[3]
			{
				triangle[0],
				triangle[1],
				triangle[2]
			});
			for (int j = 0; j < 3; j++)
			{
				int num2 = list.IndexOf(array[j]);
				if (num2 < 0)
				{
					list.Add(array[j]);
					num2 = list.Count - 1;
					list3.Add(new List<Vector3>());
					list3[num2].Add(triangle.normal);
				}
				else
				{
					list3[num2].Add(triangle.normal);
				}
				list2.Add(num2);
				num++;
			}
		}
		for (int k = 0; k < list3.Count; k++)
		{
			Vector3 zero = Vector3.zero;
			for (int l = 0; l < list3[k].Count; l++)
			{
				zero += list3[k][l];
			}
			zero /= (float)list3[k].Count;
			list4.Add(zero);
		}
		Mesh mesh = new Mesh();
		mesh.SetVertices(list);
		mesh.SetIndices(list2.ToArray(), MeshTopology.Triangles, 0);
		mesh.SetNormals(list4);
		return mesh;
	}

	public void ImportMesh()
	{
		List<Triangle> list = new List<Triangle>();
		Vector3[] vertices = _meshToImport.vertices;
		int[] indices = _meshToImport.GetIndices(0);
		for (int i = 0; i < indices.Length; i += 3)
		{
			Triangle item = new Triangle(vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]]);
			item.cachedDegree = OrderedPosition(item.centroid);
			list.Add(item);
		}
		for (int j = 0; j < list.Count; j++)
		{
			byte b = 0;
			short[] array = new short[3] { -1, -1, -1 };
			for (int k = 0; k < list.Count; k++)
			{
				if (k == j)
				{
					continue;
				}
				byte b2 = 0;
				for (int l = 0; l < 3; l++)
				{
					for (int m = 0; m < 3; m++)
					{
						if (Vector3.Distance(list[j][l], list[k][m]) < 0.001f)
						{
							b2 += (byte)(1 << l);
						}
					}
				}
				switch (b2)
				{
				case 3:
					b++;
					array[0] = (short)k;
					break;
				case 6:
					b += 2;
					array[1] = (short)k;
					break;
				case 5:
					b += 4;
					array[2] = (short)k;
					break;
				}
			}
			Triangle value = new Triangle(list[j]);
			value.allowedEdgeFlags = b;
			value.adjacency = array;
			list[j] = value;
		}
		list.Sort((Triangle t1, Triangle t2) => Mathf.RoundToInt(Mathf.Sign(t1.cachedDegree - t2.cachedDegree)));
		_baseFloodTriangles = list.ToArray();
		_cachedTriangleSections = new float[3];
		_cachedTriangleSections[0] = _baseFloodTriangles[_baseFloodTriangles.Length / 4].cachedDegree;
		_cachedTriangleSections[1] = _baseFloodTriangles[_baseFloodTriangles.Length / 2].cachedDegree;
		_cachedTriangleSections[2] = _baseFloodTriangles[_baseFloodTriangles.Length / 4 * 3].cachedDegree;
		_importedMesh = true;
		_meshToImport = null;
		Debug.Log("tri count = " + list.Count);
	}

	private bool PointUnderMesh(Vector3 point, Vector3 normal, float limit, Mesh mesh, Transform meshTransform)
	{
		Vector3 origin = base.transform.TransformPoint(point);
		Ray ray = new Ray(origin, normal);
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			int[] indices = mesh.GetIndices(i);
			for (int j = 0; j < indices.Length; j += 3)
			{
				if (new Triangle(meshTransform.TransformPoint(vertices[indices[j]]), meshTransform.TransformPoint(vertices[indices[j + 1]]), meshTransform.TransformPoint(vertices[indices[j + 2]])).RayIntersect(ray, limit))
				{
					return true;
				}
			}
		}
		return false;
	}

	public abstract float OrderedPosition(Vector3 localPoint);

	protected abstract Vector2 WorldToVoronoi(Vector3 point);

	protected abstract Vector3 VoronoiToLocal(Vector2 point, Triangle[] workedOnTriArray = null);

	protected abstract float GetTriRaycastLimit(Triangle tri, Triangle[] workedOnTriArray = null);

	protected void DrawPath(Triangle[] toShow)
	{
		if (voronoiLines != null && showVoronoi)
		{
			Gizmos.color = new Color(1f, 0.647f, 0f);
			VertexPair[] array = voronoiLines;
			for (int i = 0; i < array.Length; i++)
			{
				VertexPair vertexPair = array[i];
				Gizmos.DrawLine(vertexPair.v0, vertexPair.v1);
			}
		}
		if (toShow != null)
		{
			int num = 0;
			for (int i = 0; i < toShow.Length; i++)
			{
				Triangle triangle = toShow[i];
				Vector3 vector = base.transform.TransformPoint(triangle.v0);
				Vector3 vector2 = base.transform.TransformPoint(triangle.v1);
				Vector3 vector3 = base.transform.TransformPoint(triangle.v2);
				Gizmos.color = Color.HSVToRGB((float)num / (float)toShow.Length, 1f, 1f);
				Gizmos.DrawWireSphere(vector, 1f);
				Gizmos.DrawWireSphere(vector2, 1f);
				Gizmos.DrawWireSphere(vector3, 1f);
				Gizmos.color = (((triangle.allowedEdgeFlags & 1) > 0) ? Color.cyan : Color.red);
				Gizmos.DrawLine(vector, vector2);
				Gizmos.color = (((triangle.allowedEdgeFlags & 2) > 0) ? Color.cyan : Color.red);
				Gizmos.DrawLine(vector2, vector3);
				Gizmos.color = (((triangle.allowedEdgeFlags & 4) > 0) ? Color.cyan : Color.red);
				Gizmos.DrawLine(vector3, vector);
				Gizmos.color = new Color(1f, 0.647f, 0f);
				Vector3 vector4 = base.transform.TransformPoint(triangle.centroid);
				Gizmos.DrawWireSphere(vector4, 1f);
				Gizmos.DrawLine(vector4, vector4 + base.transform.TransformDirection(triangle.normal) * 5f);
				num++;
			}
		}
	}
}
