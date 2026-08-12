using UnityEngine;

public class OWRail : MonoBehaviour
{
	[SerializeField]
	private Vector3[] _railPoints;

	[SerializeField]
	private bool _generateFromChildren;

	private void OnValidate()
	{
		if (_generateFromChildren)
		{
			_railPoints = new Vector3[base.transform.childCount];
			for (int i = 0; i < base.transform.childCount; i++)
			{
				_railPoints[i] = base.transform.InverseTransformPoint(base.transform.GetChild(i).position);
			}
			_generateFromChildren = false;
		}
	}

	public float FindClosestPointOnRail(Vector3 worldPosition, out Vector3 closestPoint)
	{
		Vector3 vector = base.transform.InverseTransformPoint(worldPosition);
		closestPoint = Vector3.zero;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _railPoints.Length - 1; i++)
		{
			Vector3 vector2 = OWMath.ClosestPointOnSegment(vector, _railPoints[i], _railPoints[i + 1]);
			float num2 = Vector3.Distance(vector, vector2);
			if (num2 < num)
			{
				num = num2;
				closestPoint = vector2;
			}
		}
		closestPoint = base.transform.TransformPoint(closestPoint);
		return num;
	}

	private void OnDrawGizmosSelected()
	{
		if (_railPoints == null) return; // CHANGED
		
		Gizmos.color = Color.yellow;
		for (int i = 0; i < _railPoints.Length; i++)
		{
			Gizmos.DrawSphere(base.transform.TransformPoint(_railPoints[i]), 0.05f);
			if (i > 0)
			{
				Gizmos.DrawLine(base.transform.TransformPoint(_railPoints[i - 1]), base.transform.TransformPoint(_railPoints[i]));
			}
		}
	}
}
