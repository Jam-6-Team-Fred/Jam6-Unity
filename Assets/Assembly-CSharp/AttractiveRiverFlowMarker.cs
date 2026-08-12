using UnityEngine;

public class AttractiveRiverFlowMarker : MonoBehaviour
{
	public float magnitude;

	public float attractMagnitude;

	public float curveDistance;

	private void OnValidate()
	{
		if (magnitude < 0f)
		{
			magnitude = 0f;
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(base.transform.position, 2f);
	}
}
