using System.Collections.Generic;
using UnityEngine;

public class RiverFlowRail : MonoBehaviour
{
	public struct RiverFlowSegment
	{
		public AttractiveRiverFlowMarker pt1;

		public AttractiveRiverFlowMarker pt2;

		public Vector3 closestPoint;
	}

	[SerializeField]
	private AttractiveRiverFlowMarker[] _nodes;

	public RiverFlowSegment GetNearestSegment(Vector3 worldPoint)
	{
		float num = float.MaxValue;
		int num2 = -1;
		Vector3 closestPoint = Vector3.zero;
		for (int i = 0; i < _nodes.Length && i + 1 <= _nodes.Length - 1; i++)
		{
			Vector3 vector = OWMath.ClosestPointOnSegment(worldPoint, _nodes[i].GetPosition(), _nodes[i + 1].GetPosition());
			float num3 = Vector3.Distance(worldPoint, vector);
			if (num3 < num)
			{
				num = num3;
				num2 = i;
				closestPoint = vector;
			}
		}
		RiverFlowSegment result = default(RiverFlowSegment);
		result.pt1 = _nodes[num2];
		result.pt2 = _nodes[num2 + 1];
		result.closestPoint = closestPoint;
		return result;
	}

	[ContextMenu("Collect and Rename Marker Children")]
	public void CollectMarkerChildren()
	{
		List<AttractiveRiverFlowMarker> list = new List<AttractiveRiverFlowMarker>();
		int num = 1;
		foreach (Transform item in base.transform)
		{
			AttractiveRiverFlowMarker component = item.GetComponent<AttractiveRiverFlowMarker>();
			if (component != null)
			{
				list.Add(component);
				item.name = "FlowMarker " + num;
				num++;
			}
		}
		_nodes = list.ToArray();
	}
}
