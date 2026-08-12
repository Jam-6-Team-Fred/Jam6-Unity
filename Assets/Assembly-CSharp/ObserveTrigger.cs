using UnityEngine;

public class ObserveTrigger : MonoBehaviour, IObservable
{
	[SerializeField]
	private float _maxViewDistance = 2f;

	[SerializeField]
	private float _maxViewAngle = 180f;

	public OWEvent OnGainFocus = new OWEvent(1);

	public OWEvent OnLoseFocus = new OWEvent(1);

	private void Reset()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Interactible");
	}

	public void Observe(RaycastHit raycastHit, Vector3 raycastOrigin)
	{
		float num = Vector3.Angle(raycastHit.point - raycastOrigin, -base.transform.forward);
		if (raycastHit.distance < _maxViewDistance && num < _maxViewAngle)
		{
			OnGainFocus.Invoke();
		}
	}

	public void LoseFocus()
	{
		OnLoseFocus.Invoke();
	}

	private void OnDrawGizmosSelected()
	{
		Quaternion quaternion = Quaternion.AngleAxis(_maxViewAngle, base.transform.up);
		Vector3 vector = quaternion * (base.transform.forward * _maxViewDistance);
		Vector3 vector2 = Quaternion.Inverse(quaternion) * (base.transform.forward * _maxViewDistance);
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(base.transform.position, base.transform.position + vector);
		Gizmos.DrawLine(base.transform.position, base.transform.position + vector2);
		Gizmos.color = Color.blue;
		OWGizmos.DrawWireCircle(base.transform.position, base.transform.up, _maxViewDistance);
	}
}
