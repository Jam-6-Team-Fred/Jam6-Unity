using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class MatchInitialMotion : MonoBehaviour
{
	[SerializeField]
	private bool _ignoreAngularVelocity;

	[SerializeField]
	private bool _printMatchVelocity;

	private OWRigidbody _bodyToMatch;

	private OWRigidbody _owRigidbody;

	private Vector3 _initVel;

	private void Awake()
	{
		_owRigidbody = base.gameObject.GetRequiredComponent<OWRigidbody>();
		_bodyToMatch = _owRigidbody.GetOrigParentBody();
	}

	private void Start()
	{
		_initVel = CalculateMatchVelocity();
		_owRigidbody.AddVelocityChange(_initVel);
		if (_printMatchVelocity)
		{
			MonoBehaviour.print(base.gameObject.name + " Match Velocity: " + _initVel);
		}
	}

	public Vector3 CalculateMatchVelocity()
	{
		Vector3 zero = Vector3.zero;
		_owRigidbody.UpdateCenterOfMass();
		if (_bodyToMatch != null)
		{
			_bodyToMatch.UpdateCenterOfMass();
			InitialMotion component = _bodyToMatch.GetComponent<InitialMotion>();
			if (component != null)
			{
				zero += component.GetInitVelocity();
				if (!_ignoreAngularVelocity)
				{
					Vector3 worldCenterOfMass = _owRigidbody.GetWorldCenterOfMass();
					Vector3 worldCenterOfMass2 = _bodyToMatch.GetWorldCenterOfMass();
					Vector3 initAngularVelocity = component.GetInitAngularVelocity();
					zero += OWPhysics.PointTangentialVelocity(worldCenterOfMass, worldCenterOfMass2, initAngularVelocity);
				}
			}
			else
			{
				MatchInitialMotion component2 = _bodyToMatch.GetComponent<MatchInitialMotion>();
				if (component2 != null)
				{
					zero += component2.CalculateMatchVelocity();
				}
			}
		}
		return zero;
	}

	private void FixedUpdate()
	{
		Object.Destroy(this);
	}

	public OWRigidbody GetBodyToMatch()
	{
		return _bodyToMatch;
	}

	public void SetBodyToMatch(OWRigidbody bodyToMatch)
	{
		_bodyToMatch = bodyToMatch;
	}
}
