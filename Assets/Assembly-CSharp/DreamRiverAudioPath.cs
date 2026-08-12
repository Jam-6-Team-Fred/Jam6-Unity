using System.Collections.Generic;
using Delaunay;
using UnityEngine;

public class DreamRiverAudioPath : BaseRiverAudioPath
{
	[SerializeField]
	private float _minX;

	[SerializeField]
	private float _maxX;

	protected override float GetWaterLevelAtPointForBake(Vector3 point, Triangle[] workedOnTriArray = null)
	{
		return base.transform.position.y;
	}

	protected override float GetRuntimeWaterLevelAtPoint(Vector3 point)
	{
		return base.transform.position.y;
	}

	protected override float PointHeight(Vector3 point)
	{
		return point.y;
	}

	protected override void FindDeepSearchIndexes(int pivot, Triangle[] triArray, out int startSearchIdx, out int endSearchIdx)
	{
		startSearchIdx = 0;
		endSearchIdx = triArray.Length - 1;
		float cachedDegree = triArray[pivot].cachedDegree;
		float num = cachedDegree - 20f;
		startSearchIdx = pivot;
		while (triArray[startSearchIdx].cachedDegree > num && startSearchIdx > 0)
		{
			startSearchIdx--;
		}
		float num2 = cachedDegree + 20f;
		endSearchIdx = pivot;
		while (triArray[endSearchIdx].cachedDegree < num2 && endSearchIdx < triArray.Length - 1)
		{
			endSearchIdx++;
		}
	}

	public override float OrderedPosition(Vector3 localPoint)
	{
		return localPoint.z;
	}

	public override Vector3 DesiredPointOnWater(Vector3 localPos)
	{
		return new Vector3(localPos.x, 0f, localPos.z);
	}

	protected override void AddPairIfIntersect(List<VertexPair> vertexPairs, Triangle[] workedOnTriArray, Vector3 v1, Vector3 v2)
	{
		float y = base.transform.position.y;
		if (v1.y < y && v2.y > y)
		{
			vertexPairs.Add(new VertexPair
			{
				v0 = v1,
				v1 = v2
			});
		}
		else if (v1.y > y && v2.y < y)
		{
			vertexPairs.Add(new VertexPair
			{
				v0 = v2,
				v1 = v1
			});
		}
	}

	protected override Rect GetVoronoiBounds()
	{
		return new Rect(0f, minY, 2000f, maxY - minY);
	}

	protected override Vector2 WorldToVoronoi(Vector3 point)
	{
		Vector3 vector = base.transform.InverseTransformPoint(point);
		return new Vector2(vector.z, 0f - vector.x);
	}

	protected override Vector3 VoronoiToLocal(Vector2 point, Triangle[] workedOnTriArray = null)
	{
		return new Vector3(0f - point.y, 0f, point.x);
	}

	protected override void PadVoronoiPoints(Vector3[] midPoints, List<Vector2> mids2D, List<uint> colors)
	{
		colors.Add(0u);
		mids2D.Add(new Vector2(_minX, minY));
		colors.Add(0u);
		mids2D.Add(new Vector2(_minX, maxY));
		colors.Add(0u);
		mids2D.Add(new Vector2(_maxX, minY));
		colors.Add(0u);
		mids2D.Add(new Vector2(_maxX, maxY));
	}

	protected override float GetTriRaycastLimit(Triangle tri, Triangle[] workedOnTriArray = null)
	{
		return float.PositiveInfinity;
	}

	protected override float GetPaddedLookupTriangleHorizontalPosition(float inLookupHorizontalPosition, float inTriHorizontalPosition)
	{
		return inTriHorizontalPosition;
	}

	protected override void RemovePaddingTris(List<Delaunay.Triangle> delTris)
	{
		int num = 0;
		while (num < delTris.Count)
		{
			Vector2 vector = (delTris[num].sites[0].Coord + delTris[num].sites[0].Coord + delTris[num].sites[0].Coord) / 3f;
			if (vector.x < _minX || vector.x > _maxX)
			{
				delTris.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public void Generate()
	{
		Generate(ref _baseFloodTriangles, ref _cachedTriangleSections);
	}

	public Mesh GenerateMesh()
	{
		return GenerateMesh(_baseFloodTriangles);
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			DrawPath(_baseFloodTriangles);
		}
	}
}
