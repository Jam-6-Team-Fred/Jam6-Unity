using UnityEngine;

public class Autopilot : ThrusterController
{
	public delegate void FireRetroRocketsEvent();

	public delegate void InitMatchVelocityEvent();

	public delegate void InitFlyToDestinationEvent();

	public delegate void AbortAutopilotEvent();

	public delegate void AlreadyAtDestinationEvent();

	public delegate void ArriveAtDestinationEvent(float arrivalError);

	public delegate void MatchedVelocityEvent();

	private OWRigidbody _owRigidbody;

	private ReferenceFrame _referenceFrame;

	private ShipResources _shipResources;

	private ForceDetector _forceDetector;

	private RulesetDetector _rulesetDetector;

	private bool _isDamaged;

	private bool _isShipAutopilot;

	private bool _isMatchingVelocity;

	private bool _stopMatchingNextFrame;

	private bool _ignoreThrustLimits;

	private bool _isFlyingToDestination;

	private bool _isLiningUpDestination;

	private bool _isApproachingDestination;

	public event FireRetroRocketsEvent OnFireRetroRockets;

	public event InitMatchVelocityEvent OnInitMatchVelocity;

	public event InitFlyToDestinationEvent OnInitFlyToDestination;

	public event AbortAutopilotEvent OnAbortAutopilot;

	public event AlreadyAtDestinationEvent OnAlreadyAtDestination;

	public event ArriveAtDestinationEvent OnArriveAtDestination;

	public event MatchedVelocityEvent OnMatchedVelocity;

	protected override void Awake()
	{
		base.Awake();
		base.enabled = false;
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
		_shipResources = GetComponent<ShipResources>();
		_isShipAutopilot = _shipResources != null;
		_forceDetector = this.GetRequiredComponentInChildren<ForceDetector>();
		_rulesetDetector = this.GetRequiredComponentInChildren<RulesetDetector>();
	}

	private void OnDisable()
	{
		_isMatchingVelocity = false;
		_isFlyingToDestination = false;
		_isLiningUpDestination = false;
		_isApproachingDestination = false;
	}

	public bool IsDamaged()
	{
		return _isDamaged;
	}

	public bool IsMatchingVelocity()
	{
		return _isMatchingVelocity;
	}

	public bool IsFlyingToDestination()
	{
		return _isFlyingToDestination;
	}

	public bool IsLiningUpDestination()
	{
		return _isLiningUpDestination;
	}

	public bool IsApproachingDestination()
	{
		return _isApproachingDestination;
	}

	public void SetDamaged(bool damaged)
	{
		if (damaged)
		{
			Abort();
			StopMatchVelocity();
		}
		_isDamaged = damaged;
	}

	public void Abort()
	{
		if (base.enabled)
		{
			if (this.OnAbortAutopilot != null)
			{
				this.OnAbortAutopilot();
			}
			_isMatchingVelocity = false;
			_ignoreThrustLimits = false;
			_isFlyingToDestination = false;
			_isLiningUpDestination = false;
			_isApproachingDestination = false;
			base.enabled = false;
		}
	}

	public bool FlyToDestination(ReferenceFrame referenceFrame)
	{
		if (_isDamaged || (_isShipAutopilot && !_shipResources.AreThrustersUsable()))
		{
			return false;
		}
		if (referenceFrame.GetAllowAutopilot())
		{
			if ((referenceFrame.GetPosition() - _owRigidbody.GetWorldCenterOfMass()).magnitude < referenceFrame.GetAutopilotArrivalDistance())
			{
				if (this.OnAlreadyAtDestination != null)
				{
					this.OnAlreadyAtDestination();
				}
				base.enabled = false;
				return false;
			}
			base.enabled = true;
			_referenceFrame = referenceFrame;
			_isFlyingToDestination = true;
			_isMatchingVelocity = false;
			_ignoreThrustLimits = false;
			if (this.OnInitFlyToDestination != null)
			{
				this.OnInitFlyToDestination();
			}
			return true;
		}
		return false;
	}

	public void StartMatchVelocity(ReferenceFrame referenceFrame, bool ignoreThrustLimits = false)
	{
		if (!_isDamaged && (!_isShipAutopilot || _shipResources.AreThrustersUsable()) && !_isMatchingVelocity)
		{
			base.enabled = true;
			_referenceFrame = referenceFrame;
			_isMatchingVelocity = true;
			_ignoreThrustLimits = ignoreThrustLimits;
			if (this.OnInitMatchVelocity != null)
			{
				this.OnInitMatchVelocity();
			}
		}
	}

	public void StopMatchVelocity()
	{
		_isMatchingVelocity = false;
		_ignoreThrustLimits = false;
		base.enabled = false;
	}

	protected override Vector3 ReadTranslationalInput()
	{
		if (_isShipAutopilot && !_shipResources.AreThrustersUsable())
		{
			if (_isMatchingVelocity)
			{
				StopMatchVelocity();
			}
			else if (_isFlyingToDestination)
			{
				Abort();
			}
			return Vector3.zero;
		}
		if (_isMatchingVelocity && _referenceFrame != null)
		{
			float num = Vector3.Distance(_owRigidbody.GetWorldCenterOfMass(), _referenceFrame.GetPosition());
			Vector3 vector = ((!_referenceFrame.GetAllowMatchAngularVelocity(num)) ? _owRigidbody.GetRelativeVelocity(_referenceFrame) : (_referenceFrame.GetPointVelocity(_owRigidbody.GetCenterOfMass()) - _owRigidbody.GetVelocity()));
			float magnitude = vector.magnitude;
			float num2 = (_ignoreThrustLimits ? _thrusterModel.GetMaxTranslationalThrust() : Mathf.Min(_rulesetDetector.GetThrustLimit(), _thrusterModel.GetMaxTranslationalThrust()));
			float num3 = num2 * Time.fixedDeltaTime;
			float num4 = num2 / _thrusterModel.GetMaxTranslationalThrust();
			if (magnitude < num3)
			{
				num4 *= magnitude / num3;
			}
			if (_stopMatchingNextFrame)
			{
				_stopMatchingNextFrame = false;
				if (_isFlyingToDestination && this.OnArriveAtDestination != null)
				{
					float arrivalError = num - _referenceFrame.GetAutopilotArrivalDistance();
					this.OnArriveAtDestination(arrivalError);
					_isFlyingToDestination = false;
				}
				else if (this.OnMatchedVelocity != null)
				{
					this.OnMatchedVelocity();
				}
				StopMatchVelocity();
			}
			else
			{
				float num5 = magnitude - num3 * num4;
				float num6 = 0.01f;
				ForceDetector attachedForceDetector = _referenceFrame.GetOWRigidBody().GetAttachedForceDetector();
				if (attachedForceDetector != null)
				{
					num6 = (Vector3.Project(attachedForceDetector.GetForceAcceleration() - _forceDetector.GetForceAcceleration(), vector.normalized) * Time.deltaTime).magnitude;
				}
				if (num5 <= num6)
				{
					_stopMatchingNextFrame = true;
					return Vector3.zero;
				}
			}
			return base.transform.InverseTransformDirection(vector.normalized * num4);
		}
		if (_isFlyingToDestination && _referenceFrame != null)
		{
			_isLiningUpDestination = false;
			_isApproachingDestination = false;
			Vector3 vector2 = _referenceFrame.GetPosition() - _owRigidbody.GetWorldCenterOfMass();
			float magnitude2 = vector2.magnitude;
			Vector3 relativeVelocity = _owRigidbody.GetRelativeVelocity(_referenceFrame);
			Vector3 vector3 = Vector3.Project(relativeVelocity, vector2);
			float num7 = vector3.magnitude * (0f - Mathf.Sign(Vector3.Dot(vector2, vector3)));
			if (num7 < -1f)
			{
				_isLiningUpDestination = true;
				return base.transform.InverseTransformDirection(relativeVelocity.normalized);
			}
			Vector3 rhs = Vector3.Project(_forceDetector.GetForceAcceleration(), vector2);
			float num8 = rhs.magnitude * (0f - Mathf.Sign(Vector3.Dot(vector2, rhs)));
			Vector3 rhs2 = Vector3.Project(_referenceFrame.GetAcceleration(), vector2);
			float num9 = rhs2.magnitude * Mathf.Sign(Vector3.Dot(vector2, rhs2)) + num8 + _thrusterModel.GetMaxTranslationalThrust();
			if (Mathf.Pow(Mathf.Abs(num7), 2f) / (2f * num9) > magnitude2 - _referenceFrame.GetAutopilotArrivalDistance())
			{
				if (this.OnFireRetroRockets != null)
				{
					this.OnFireRetroRockets();
				}
				StartMatchVelocity(_referenceFrame, ignoreThrustLimits: true);
				return Vector3.zero;
			}
			Vector3 vector4 = relativeVelocity - vector3;
			if (vector4.magnitude > _thrusterModel.GetMaxTranslationalThrust() / 10f)
			{
				_isLiningUpDestination = true;
				if (num7 > 10f)
				{
					vector3 = Vector3.zero;
				}
			}
			else
			{
				_isApproachingDestination = true;
				vector4 *= vector3.magnitude / 2f;
				if (num7 > 0f)
				{
					vector3 *= -1f;
				}
				else if (num7 <= 0f)
				{
					vector4 = Vector3.zero;
					vector3 = vector2;
				}
			}
			return base.transform.InverseTransformDirection((vector4 + vector3).normalized);
		}
		return Vector3.zero;
	}

	protected override Vector3 ReadRotationalInput()
	{
		return Vector3.zero;
	}
}
