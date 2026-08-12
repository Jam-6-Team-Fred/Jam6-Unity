using System;
using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class AnglerfishController : SectoredMonoBehaviour
{
	public enum AnglerState
	{
		Lurking = 0,
		Investigating = 1,
		Chasing = 2,
		Consuming = 3,
		Stunned = 4
	}

	public delegate void AnglerStateEvent(AnglerState newState);

	public delegate void AnglerAnimationEvent();

	[SerializeField]
	private Vector3 _mouthOffset;

	[SerializeField]
	private float _acceleration = 2f;

	[SerializeField]
	private float _investigateSpeed = 20f;

	[SerializeField]
	private float _chaseSpeed = 42f;

	[SerializeField]
	private float _turnSpeed = 180f;

	[SerializeField]
	private float _quickTurnSpeed = 360f;

	[Space]
	[SerializeField]
	private float _arrivalDistance = 100f;

	[SerializeField]
	private float _pursueDistance = 200f;

	[SerializeField]
	private float _escapeDistance = 500f;

	[Space]
	[SerializeField]
	private float _consumeShipCrushDelay = 3f;

	[SerializeField]
	private float _consumeDeathDelay = 5f;

	private OWRigidbody _brambleBody;

	private OWRigidbody _anglerBody;

	private OWRigidbody _targetBody;

	private ImpactSensor _impactSensor;

	private NoiseSensor _noiseSensor;

	private AnglerfishFluidVolume _anglerfishFluidVolume;

	private AnglerState _currentState;

	private Vector3 _localDisturbancePos;

	private bool _turningInPlace;

	private float _stunTimer;

	private float _consumeStartTime;

	private bool _consumeComplete;

	private Vector3 _targetPos;

	public event AnglerStateEvent OnChangeAnglerState;

	public event AnglerStateEvent OnAnglerSuspended;

	public event AnglerStateEvent OnAnglerUnsuspended;

	public event AnglerAnimationEvent OnAnglerTurn;

	public AnglerState GetAnglerState()
	{
		return _currentState;
	}

	public Vector3 GetTargetPosition()
	{
		if (_targetBody != null)
		{
			return _targetBody.GetPosition();
		}
		return _brambleBody.transform.TransformPoint(_localDisturbancePos);
	}

	protected override void Awake()
	{
		base.Awake();
		_anglerBody = this.GetRequiredComponent<OWRigidbody>();
		_impactSensor = this.GetRequiredComponent<ImpactSensor>();
		_noiseSensor = this.GetRequiredComponentInChildren<NoiseSensor>();
		_anglerfishFluidVolume = this.GetRequiredComponentInChildren<AnglerfishFluidVolume>();
		_currentState = AnglerState.Lurking;
		_turningInPlace = false;
		_stunTimer = 0f;
		_consumeStartTime = -1f;
		_consumeComplete = false;
	}

	private void Start()
	{
		_brambleBody = _anglerBody.GetOrigParentBody();
		if (this.OnChangeAnglerState != null)
		{
			this.OnChangeAnglerState(AnglerState.Lurking);
		}
	}

	private void OnEnable()
	{
		_impactSensor.OnImpact += OnImpact;
		_noiseSensor.OnClosestAudibleNoise += OnClosestAudibleNoise;
		_anglerfishFluidVolume.OnCaughtObject += OnCaughtObject;
	}

	private void OnDisable()
	{
		_impactSensor.OnImpact -= OnImpact;
		_noiseSensor.OnClosestAudibleNoise -= OnClosestAudibleNoise;
		_anglerfishFluidVolume.OnCaughtObject -= OnCaughtObject;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (!base.gameObject.activeSelf && _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
		{
			base.gameObject.SetActive(value: true);
			_anglerBody.Unsuspend();
			if (this.OnAnglerUnsuspended != null)
			{
				this.OnAnglerUnsuspended(_currentState);
			}
		}
		else if (base.gameObject.activeSelf && !_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
		{
			_anglerBody.Suspend();
			base.gameObject.SetActive(value: false);
			if (this.OnAnglerSuspended != null)
			{
				this.OnAnglerSuspended(_currentState);
			}
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (_targetBody != null && sectorDetector.GetAttachedOWRigidbody() == _targetBody)
		{
			_targetBody = null;
		}
	}

	private void FixedUpdate()
	{
		UpdateState();
		UpdateMovement();
	}

	private void UpdateState()
	{
		switch (_currentState)
		{
		case AnglerState.Investigating:
			if ((_brambleBody.transform.TransformPoint(_localDisturbancePos) - _anglerBody.GetPosition()).sqrMagnitude < _arrivalDistance * _arrivalDistance)
			{
				ChangeState(AnglerState.Lurking);
			}
			break;
		case AnglerState.Chasing:
			if (_targetBody == null)
			{
				ChangeState(AnglerState.Lurking);
			}
			else if ((_targetBody.GetPosition() - _anglerBody.GetPosition()).sqrMagnitude > _escapeDistance * _escapeDistance)
			{
				_targetBody = null;
				ChangeState(AnglerState.Lurking);
			}
			break;
		case AnglerState.Consuming:
		{
			if (_consumeComplete)
			{
				break;
			}
			if (_targetBody == null)
			{
				ChangeState(AnglerState.Lurking);
				break;
			}
			float num = Time.time - _consumeStartTime;
			if (_targetBody.CompareTag("Player") && num > _consumeDeathDelay)
			{
				Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
				_consumeComplete = true;
			}
			else
			{
				if (!_targetBody.CompareTag("Ship"))
				{
					break;
				}
				if (num > _consumeShipCrushDelay)
				{
					_targetBody.GetComponentInChildren<ShipDamageController>().TriggerSystemFailure();
				}
				if (num > _consumeDeathDelay)
				{
					if (PlayerState.IsInsideShip())
					{
						Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
					}
					_consumeComplete = true;
				}
			}
			break;
		}
		case AnglerState.Stunned:
			_stunTimer -= Time.deltaTime;
			if (_stunTimer <= 0f)
			{
				if (_targetBody != null)
				{
					ChangeState(AnglerState.Chasing);
				}
				else
				{
					ChangeState(AnglerState.Lurking);
				}
			}
			break;
		}
	}

	private void UpdateMovement()
	{
		if (_anglerBody.GetVelocity().sqrMagnitude > Mathf.Pow(_chaseSpeed * 1.5f, 2f))
		{
			ApplyDrag(10f);
		}
		switch (_currentState)
		{
		case AnglerState.Lurking:
			ApplyDrag(1f);
			break;
		case AnglerState.Investigating:
		{
			Vector3 targetPos = _brambleBody.transform.TransformPoint(_localDisturbancePos);
			RotateTowardsTarget(targetPos, _turnSpeed, _turnSpeed);
			if (!_turningInPlace)
			{
				MoveTowardsTarget(targetPos, _investigateSpeed, _acceleration);
			}
			break;
		}
		case AnglerState.Chasing:
		{
			Vector3 velocity = _targetBody.GetVelocity();
			Vector3 normalized = velocity.normalized;
			Vector3 from = _anglerBody.GetPosition() + base.transform.TransformDirection(_mouthOffset) - _targetBody.GetPosition();
			float magnitude = velocity.magnitude;
			float num = Vector3.Angle(from, normalized);
			float num2 = magnitude * 2f;
			float num3 = num2;
			if (num < 90f)
			{
				float magnitude2 = from.magnitude;
				float num4 = magnitude2 * Mathf.Sin(num * ((float)Math.PI / 180f));
				float num5 = magnitude2 * Mathf.Cos(num * ((float)Math.PI / 180f));
				float magnitude3 = _anglerBody.GetVelocity().magnitude;
				float num6 = num5 / Mathf.Max(magnitude, 0.0001f);
				float num7 = num4 / Mathf.Max(magnitude3, 0.0001f);
				float num8 = num6 / num7;
				if (num8 <= 1f)
				{
					float t = Mathf.Clamp01(num8);
					num3 = Mathf.Lerp(num2, num5, t);
				}
				else
				{
					float num9 = Mathf.InverseLerp(1f, 4f, num8);
					num3 = Mathf.Lerp(num5, 0f, num9 * num9);
				}
			}
			_targetPos = _targetBody.GetPosition() + normalized * num3;
			RotateTowardsTarget(_targetPos, _turnSpeed, _quickTurnSpeed);
			if (!_turningInPlace)
			{
				MoveTowardsTarget(_targetPos, _chaseSpeed, _acceleration);
			}
			break;
		}
		case AnglerState.Consuming:
			ApplyDrag(1f);
			break;
		case AnglerState.Stunned:
			ApplyDrag(0.5f);
			break;
		}
	}

	private void RotateTowardsTarget(Vector3 targetPos, float normalTurnSpeed, float inPlaceTurnSpeed)
	{
		Vector3 from = targetPos - _anglerBody.GetPosition();
		float num = Vector3.Angle(from, base.transform.forward);
		if (!_turningInPlace && num > 90f)
		{
			_turningInPlace = true;
			if (this.OnAnglerTurn != null)
			{
				this.OnAnglerTurn();
			}
		}
		else if (_turningInPlace && num < 30f)
		{
			_turningInPlace = false;
		}
		Vector3 normalized = Vector3.Cross(base.transform.forward, from.normalized).normalized;
		float num2 = Mathf.InverseLerp(0f, 15f, num) * (_turningInPlace ? inPlaceTurnSpeed : normalTurnSpeed);
		_anglerBody.SetAngularVelocity(normalized * num2 * ((float)Math.PI / 180f));
	}

	private void MoveTowardsTarget(Vector3 targetPos, float moveSpeed, float maxAcceleration)
	{
		Vector3 vector = targetPos - (_anglerBody.GetPosition() + base.transform.TransformDirection(_mouthOffset));
		Vector3 relativeVelocity = _brambleBody.GetRelativeVelocity(_anglerBody);
		Vector3 vector2 = vector.normalized * moveSpeed - relativeVelocity;
		vector2 = vector2.normalized * Mathf.Min(vector2.magnitude, maxAcceleration);
		_anglerBody.AddAcceleration(vector2);
	}

	private void ApplyDrag(float dragFactor)
	{
		Vector3 relativeVelocity = _brambleBody.GetRelativeVelocity(_anglerBody);
		Vector3 acceleration = -0.5f * relativeVelocity * dragFactor;
		_anglerBody.AddAcceleration(acceleration);
		Vector3 angularVelocity = _anglerBody.GetAngularVelocity();
		Vector3 angularAcceleration = -0.5f * angularVelocity * dragFactor;
		_anglerBody.AddAngularAcceleration(angularAcceleration);
	}

	private void OnImpact(ImpactData impact)
	{
		OWRigidbody attachedOWRigidbody = impact.otherCollider.GetAttachedOWRigidbody();
		if ((attachedOWRigidbody.CompareTag("Player") || attachedOWRigidbody.CompareTag("Ship")) && _currentState != AnglerState.Consuming && _currentState != AnglerState.Stunned)
		{
			_targetBody = attachedOWRigidbody;
			ChangeState(AnglerState.Chasing);
		}
		else if (impact.speed > (_investigateSpeed + _chaseSpeed) * 0.5f)
		{
			attachedOWRigidbody.GetMass();
			_ = _anglerBody.GetMass() * 2f;
		}
	}

	private void OnClosestAudibleNoise(NoiseMaker noiseMaker)
	{
		if (_currentState == AnglerState.Consuming || _currentState == AnglerState.Stunned)
		{
			return;
		}
		if ((noiseMaker.GetNoiseOrigin() - base.transform.position).sqrMagnitude < _pursueDistance * _pursueDistance)
		{
			if (_targetBody != noiseMaker.GetAttachedBody())
			{
				_targetBody = noiseMaker.GetAttachedBody();
				if (_currentState != AnglerState.Chasing)
				{
					ChangeState(AnglerState.Chasing);
				}
			}
		}
		else if (_currentState == AnglerState.Lurking || _currentState == AnglerState.Investigating)
		{
			_localDisturbancePos = _brambleBody.transform.InverseTransformPoint(noiseMaker.GetNoiseOrigin());
			if (_currentState != AnglerState.Investigating)
			{
				ChangeState(AnglerState.Investigating);
			}
		}
	}

	private void OnCaughtObject(OWRigidbody caughtBody)
	{
		if (_currentState == AnglerState.Consuming)
		{
			if (!_targetBody.CompareTag("Player") && caughtBody.CompareTag("Player"))
			{
				Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
			}
		}
		else if (caughtBody.CompareTag("Player") || caughtBody.CompareTag("Ship"))
		{
			_targetBody = caughtBody;
			_consumeStartTime = Time.time;
			ChangeState(AnglerState.Consuming);
		}
	}

	private void ChangeState(AnglerState newState)
	{
		if (newState != _currentState)
		{
			_currentState = newState;
			if (this.OnChangeAnglerState != null)
			{
				this.OnChangeAnglerState(newState);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(base.transform.position, _arrivalDistance);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, _pursueDistance);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(base.transform.position, _escapeDistance);
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(base.transform.position + base.transform.TransformDirection(_mouthOffset), 3f);
		}
	}

	private void OnDrawGizmos()
	{
		if (_targetBody != null)
		{
			Gizmos.color = Color.gray;
			Gizmos.DrawLine(_targetBody.GetPosition(), _anglerBody.GetPosition());
			Gizmos.color = Color.green;
			Gizmos.DrawLine(_targetBody.GetPosition(), _targetBody.GetPosition() + _targetBody.GetVelocity());
			Gizmos.color = Color.red;
			Gizmos.DrawLine(_anglerBody.GetPosition(), _targetPos);
			Gizmos.DrawSphere(_targetPos, 5f);
		}
	}
}
