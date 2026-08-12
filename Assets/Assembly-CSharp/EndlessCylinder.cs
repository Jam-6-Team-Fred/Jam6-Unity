using UnityEngine;

[RequireComponent(typeof(CylinderShape))]
public class EndlessCylinder : EndlessTriggerVolume
{
	private CylinderShape _cylinder;

	protected override void Awake()
	{
		base.Awake();
		_cylinder = base.gameObject.GetRequiredComponent<CylinderShape>();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void WarpBody(OWRigidbody body)
	{
		Vector3 vector = base.transform.InverseTransformPoint(body.GetPosition());
		Vector3 vector2 = base.transform.InverseTransformDirection(body.GetVelocity());
		Vector3 position = vector;
		float num = Mathf.Min(vector2.magnitude, 100f);
		if (vector.y > _cylinder.center.y + _cylinder.height * 0.5f)
		{
			position.y = _cylinder.center.y + _cylinder.height * 0.5f - 1f;
			vector2.y *= -1f;
			Vector3 forward = body.transform.forward;
			Vector3 up = body.transform.up;
			forward.y *= -1f;
			up.y *= -1f;
			Quaternion rotation = Quaternion.LookRotation(forward, up);
			body.SetRotation(rotation);
		}
		else if (vector.y < _cylinder.center.y - _cylinder.height * 0.5f)
		{
			position.y = _cylinder.center.y + _cylinder.height * 0.5f - 1f;
		}
		if (new Vector3(vector.x, 0f, vector.z).magnitude > _cylinder.radius)
		{
			Vector3 vector3 = -new Vector3(vector.x, 0f, vector.z).normalized * (_cylinder.radius - 1f);
			position.x = vector3.x;
			position.z = vector3.z;
		}
		body.SetVelocity(base.transform.TransformDirection(vector2.normalized * num));
		body.SetPosition(base.transform.TransformPoint(position));
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
	}
}
