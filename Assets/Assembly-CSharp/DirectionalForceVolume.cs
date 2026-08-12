using UnityEngine;

public class DirectionalForceVolume : ForceVolume
{
	[SerializeField]
	private Vector3 _fieldDirection;

	[SerializeField]
	private float _fieldMagnitude;

	[SerializeField]
	private bool _affectsAlignment = true;

	[SerializeField]
	private bool _offsetCentripetalForce;

	[SerializeField]
	private bool _playGravityCrystalAudio;

	public override bool GetPlayGravityCrystalAudio()
	{
		return _playGravityCrystalAudio;
	}

	public override Vector3 CalculateForceAccelerationAtPoint(Vector3 worldPos)
	{
		Vector3 result = base.transform.TransformDirection(_fieldDirection * _fieldMagnitude);
		if (_offsetCentripetalForce)
		{
			result += _attachedBody.GetPointCentripetalAcceleration(worldPos);
		}
		return result;
	}

	public override bool GetAffectsAlignment(OWRigidbody targetBody)
	{
		return _affectsAlignment;
	}

	public Vector3 GetLocalForceDirection()
	{
		return _fieldDirection;
	}

	public void SetLocalForceDirection(Vector3 direction)
	{
		_fieldDirection = direction;
	}

	public Vector3 GetFieldDirection()
	{
		return base.transform.TransformDirection(_fieldDirection);
	}

	public float GetFieldMagnitude()
	{
		return _fieldMagnitude;
	}

	public void SetFieldMagnitude(float magnitude)
	{
		_fieldMagnitude = magnitude;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.TransformDirection(_fieldDirection * _fieldMagnitude));
			Gizmos.color = Color.cyan;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			// CHANGED
			if (GetComponent<BoxCollider>() != null)
			{
				Gizmos.DrawWireCube(Vector3.zero + GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
			}
			else if (GetComponent<SphereCollider>() != null)
			{
				Gizmos.DrawWireSphere(Vector3.zero + GetComponent<SphereCollider>().center, GetComponent<SphereCollider>().radius);
			}
		}
	}
}
