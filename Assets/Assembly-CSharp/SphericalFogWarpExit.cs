using UnityEngine;

public class SphericalFogWarpExit : MonoBehaviour
{
	[SerializeField]
	private float _upperMinExitSpeed;

	[SerializeField]
	private float _lowerMinExitSpeed;

	protected bool _straightenExitTrajectory = true;

	public Vector3 GetRelativeExitVelocity(Vector3 relativeWorldVelocity)
	{
		if (_straightenExitTrajectory)
		{
			relativeWorldVelocity = Vector3.Project(relativeWorldVelocity, -base.transform.up);
		}
		float num = relativeWorldVelocity.magnitude;
		if (num < _upperMinExitSpeed)
		{
			num = Mathf.Clamp(num * 2f, _lowerMinExitSpeed, _upperMinExitSpeed);
		}
		return num * relativeWorldVelocity.normalized;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			SphericalFogWarpVolume componentInParent = GetComponentInParent<SphericalFogWarpVolume>();
			if (componentInParent != null)
			{
				componentInParent.DrawExitMarkers();
			}
		}
	}
}
