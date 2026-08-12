using System.Collections.Generic;
using Delaunay;
using UnityEngine;
using UnityEngine.Serialization;

public class RingRiverAudioPath : BaseRiverAudioPath
{
	[SerializeField]
	private OWRingRiverCollider _ringRiverCollider;

	[HideInInspector]
	[SerializeField]
	protected Triangle[] _postFloodTriangles;

	[FormerlySerializedAs("_cachedPostFloodDegreeSections")]
	[HideInInspector]
	[SerializeField]
	protected float[] _cachedPostFloodTriangleSections;

	public bool hasPostFloodData => _postFloodTriangles != null;

	protected override Triangle[] CurrentTriangleArrayForPoint(Vector3 point)
	{
		if (!IsPostFlood(point))
		{
			return _baseFloodTriangles;
		}
		return _postFloodTriangles;
	}

	protected override float[] CurrentCachedTriangleSectionsForPoint(Vector3 point)
	{
		if (!IsPostFlood(point))
		{
			return _cachedTriangleSections;
		}
		return _cachedPostFloodTriangleSections;
	}

	private void Start()
	{
		if (_ringRiverCollider == null)
		{
			_ringRiverCollider = Locator.GetRingRiverFluidVolume().GetComponent<OWRingRiverCollider>();
		}
	}

	public void GenerateAll()
	{
		Generate(postFlood: false);
		Generate(postFlood: true);
		_importedMesh = false;
	}

	private void Generate(bool postFlood)
	{
		if (postFlood)
		{
			Generate(ref _postFloodTriangles, ref _cachedPostFloodTriangleSections);
		}
		else
		{
			Generate(ref _baseFloodTriangles, ref _cachedTriangleSections);
		}
	}

	private bool IsPostFlood(Vector3 worldPos)
	{
		return _ringRiverCollider.HasFloodReachedPosition(worldPos, 0.7f);
	}

	private float GetWaterRadiusAtPoint(Vector3 point, bool postFlood)
	{
		if (!postFlood)
		{
			return _ringRiverCollider.GetPreFloodInnerRadiusAtLocalPosition(_ringRiverCollider.transform.InverseTransformPoint(point));
		}
		return _ringRiverCollider.GetPostFloodInnerRadiusAtLocalPosition(_ringRiverCollider.transform.InverseTransformPoint(point));
	}

	protected override float GetWaterLevelAtPointForBake(Vector3 point, Triangle[] workedOnTriArray = null)
	{
		bool postFlood = false;
		if (Application.isPlaying)
		{
			postFlood = IsPostFlood(point);
		}
		else if (workedOnTriArray == _postFloodTriangles)
		{
			postFlood = true;
		}
		return GetWaterRadiusAtPoint(point, postFlood);
	}

	protected override float GetRuntimeWaterLevelAtPoint(Vector3 point)
	{
		return _ringRiverCollider.GetInnerRadiusAtLocalPosition(_ringRiverCollider.transform.InverseTransformPoint(point));
	}

	public override Vector3 DesiredPointOnWater(Vector3 localPos)
	{
		Vector3 vector = Vector3.ProjectOnPlane(localPos, Vector3.up);
		float y = localPos.y;
		vector = vector.normalized * GetWaterLevelAtPointForBake(base.transform.TransformPoint(localPos));
		vector.y = y;
		return vector;
	}

	protected override void AddPairIfIntersect(List<VertexPair> vertexPairs, Triangle[] workedOnTriArray, Vector3 v1, Vector3 v2)
	{
		float num = PointHeight(v1);
		float num2 = PointHeight(v2);
		float waterLevelAtPointForBake = GetWaterLevelAtPointForBake(v1, workedOnTriArray);
		float waterLevelAtPointForBake2 = GetWaterLevelAtPointForBake(v2, workedOnTriArray);
		if (num < waterLevelAtPointForBake && num2 > waterLevelAtPointForBake2)
		{
			vertexPairs.Add(new VertexPair
			{
				v0 = v1,
				v1 = v2
			});
		}
		else if (num > waterLevelAtPointForBake && num2 < waterLevelAtPointForBake2)
		{
			vertexPairs.Add(new VertexPair
			{
				v0 = v2,
				v1 = v1
			});
		}
	}

	private float CylinderDistance(Vector3 point)
	{
		return Vector3.ProjectOnPlane(point, Vector3.up).magnitude;
	}

	protected override float PointHeight(Vector3 point)
	{
		return CylinderDistance(point);
	}

	protected override Rect GetVoronoiBounds()
	{
		return new Rect(-30f, minY, 1470f, maxY - minY);
	}

	protected override void PadVoronoiPoints(Vector3[] midPoints, List<Vector2> mids2D, List<uint> colors)
	{
		int num = midPoints.Length - 1;
		while (PointAngle(midPoints[num]) > 350f)
		{
			colors.Add(0u);
			Vector2 item = WorldToVoronoi(midPoints[num]);
			item.x -= 1440f;
			mids2D.Add(item);
			num--;
		}
		for (num = 0; PointAngle(midPoints[num]) < 10f; num++)
		{
			colors.Add(0u);
			Vector2 item2 = WorldToVoronoi(midPoints[num]);
			item2.x += 1440f;
			mids2D.Add(item2);
		}
	}

	protected override void RemovePaddingTris(List<Delaunay.Triangle> delTris)
	{
		int num = 0;
		while (num < delTris.Count)
		{
			Vector2 vector = (delTris[num].sites[0].Coord + delTris[num].sites[0].Coord + delTris[num].sites[0].Coord) / 3f;
			if (vector.x < 0f || vector.x > 1440f)
			{
				delTris.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public float PointAngle(Vector3 localPoint)
	{
		Vector3 vector = Vector3.ProjectOnPlane(localPoint, Vector3.up);
		float num = Vector3.Angle(Vector3.forward, vector.normalized);
		if (localPoint.x < 0f)
		{
			num = 360f - num;
		}
		return num;
	}

	public override float OrderedPosition(Vector3 localPoint)
	{
		return PointAngle(localPoint);
	}

	protected override void FindDeepSearchIndexes(int pivot, Triangle[] triArray, out int startSearchIdx, out int endSearchIdx)
	{
		startSearchIdx = 0;
		endSearchIdx = triArray.Length - 1;
		float cachedDegree = triArray[pivot].cachedDegree;
		float num = cachedDegree - 20f;
		startSearchIdx = pivot;
		float num2 = 0f;
		while (triArray[startSearchIdx].cachedDegree + num2 > num)
		{
			startSearchIdx--;
			if (startSearchIdx < 0)
			{
				startSearchIdx += triArray.Length;
				num2 = -360f;
			}
		}
		if (num2 != 0f)
		{
			startSearchIdx -= triArray.Length;
		}
		float num3 = cachedDegree + 20f;
		endSearchIdx = pivot;
		num2 = 0f;
		while (triArray[endSearchIdx].cachedDegree + num2 < num3)
		{
			endSearchIdx++;
			if (endSearchIdx > triArray.Length - 1)
			{
				endSearchIdx = 0;
				num2 = 360f;
			}
		}
		if (num2 != 0f)
		{
			endSearchIdx += triArray.Length;
		}
	}

	private Vector3 CylinderToPlane(Vector3 point)
	{
		return new Vector3(PointAngle(point) * 4f, point.y, 0f);
	}

	private Vector2 CylinderTo2DPlane(Vector3 point)
	{
		return new Vector2(PointAngle(point) * 4f, point.y);
	}

	protected override Vector2 WorldToVoronoi(Vector3 point)
	{
		return CylinderTo2DPlane(point);
	}

	private Vector3 PlaneToCylinder(Vector2 point, Triangle[] workedOnTriArray)
	{
		Vector3 vector = Quaternion.AngleAxis(point.x / 4f, Vector3.up) * Vector3.forward;
		vector *= GetWaterLevelAtPointForBake(vector, workedOnTriArray);
		vector.y = point.y;
		return vector;
	}

	protected override Vector3 VoronoiToLocal(Vector2 point, Triangle[] workedOnTriArray = null)
	{
		return PlaneToCylinder(point, workedOnTriArray);
	}

	protected override float GetTriRaycastLimit(Triangle tri, Triangle[] workedOnTriArray = null)
	{
		return GetWaterLevelAtPointForBake(tri.centroid, workedOnTriArray);
	}

	protected override float GetPaddedLookupTriangleHorizontalPosition(float inLookupHorizontalPosition, float inTriHorizontalPosition)
	{
		if (!(inLookupHorizontalPosition > 355f) || !(inTriHorizontalPosition < 180f))
		{
			return inTriHorizontalPosition;
		}
		return inTriHorizontalPosition + 360f;
	}

	public override Vector3 GetWaterPoint(Vector3 worldPos, ref int currentTriIdx, ref int currentEdgeIdx, out Triangle currentTriangle, out Vector3 normal, out Vector3 desiredPoint, out bool inside)
	{
		Vector3 waterPoint = base.GetWaterPoint(worldPos, ref currentTriIdx, ref currentEdgeIdx, out currentTriangle, out normal, out desiredPoint, out inside);
		Vector3 vector = base.transform.InverseTransformPoint(waterPoint);
		Vector3 vector2 = Vector3.Project(vector, Vector3.up);
		Vector3 vector3 = (vector - vector2).normalized * GetRuntimeWaterLevelAtPoint(waterPoint);
		return base.transform.TransformPoint(vector2 + vector3);
	}

	public Mesh GenerateMesh(bool postFlood)
	{
		if (postFlood)
		{
			return GenerateMesh(_postFloodTriangles);
		}
		return GenerateMesh(_baseFloodTriangles);
	}

	public void ImportMeshPostFlood()
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
					break;
				case 6:
					b += 2;
					break;
				case 5:
					b += 4;
					break;
				}
			}
			Triangle value = new Triangle(list[j]);
			value.allowedEdgeFlags = b;
			list[j] = value;
		}
		list.Sort((Triangle t1, Triangle t2) => Mathf.RoundToInt(Mathf.Sign(t1.cachedDegree - t2.cachedDegree)));
		_postFloodTriangles = list.ToArray();
		_cachedPostFloodTriangleSections = new float[3];
		_cachedPostFloodTriangleSections[0] = _postFloodTriangles[_postFloodTriangles.Length / 4].cachedDegree;
		_cachedPostFloodTriangleSections[1] = _postFloodTriangles[_postFloodTriangles.Length / 2].cachedDegree;
		_cachedPostFloodTriangleSections[2] = _postFloodTriangles[_postFloodTriangles.Length / 4 * 3].cachedDegree;
		_importedMesh = true;
		_meshToImport = null;
		Debug.Log("tri count = " + list.Count);
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			if (showPostFlood)
			{
				DrawPath(_postFloodTriangles);
			}
			else
			{
				DrawPath(_baseFloodTriangles);
			}
		}
	}
}
