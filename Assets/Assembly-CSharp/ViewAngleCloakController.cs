using UnityEngine;

public class ViewAngleCloakController : MonoBehaviour
{
	[SerializeField]
	private float _viewAngle;

	[SerializeField]
	private float _viewRadius;

	[SerializeField]
	private Transform _focalPoint;

	[SerializeField]
	private DitheringAnimator _fireDitherAnimator;

	[SerializeField]
	private DitheringAnimator _structureDitherAnimator;

	private void Reset()
	{
		_focalPoint = base.transform;
	}

	private void Awake()
	{
	}

	private void Start()
	{
		_fireDitherAnimator.SetVisibleImmediate(visible: false);
		_structureDitherAnimator.SetVisibleImmediate(visible: false);
	}

	private void Update()
	{
		Vector3 vector = Locator.GetPlayerCamera().transform.position - _focalPoint.position;
		vector = Vector3.ProjectOnPlane(vector, _focalPoint.up);
		float num = Vector3.Angle(vector, _focalPoint.forward);
		_fireDitherAnimator.SetVisible(num < _viewAngle, 3f);
		_structureDitherAnimator.SetVisible(vector.sqrMagnitude < _viewRadius * _viewRadius, 3f);
	}

	private void OnDrawGizmosSelected()
	{
		if (_focalPoint != null)
		{
			Gizmos.color = Color.yellow;
			OWGizmos.DrawWireCircle(_focalPoint.position, _focalPoint.up, _viewRadius);
			Vector3 vector = Quaternion.AngleAxis(_viewAngle * 0.5f, _focalPoint.up) * _focalPoint.forward;
			Vector3 vector2 = Quaternion.AngleAxis((0f - _viewAngle) * 0.5f, _focalPoint.up) * _focalPoint.forward;
			Gizmos.DrawLine(_focalPoint.position, _focalPoint.position + vector * 10f);
			Gizmos.DrawLine(_focalPoint.position, _focalPoint.position + vector2 * 10f);
		}
	}
}
