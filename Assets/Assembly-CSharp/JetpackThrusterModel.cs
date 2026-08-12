using UnityEngine;

public class JetpackThrusterModel : ThrusterModel
{
	private const float MIN_CHARGE = 0.1f;

	[SerializeField]
	private float _boostGroundVelocity = 2f;

	[SerializeField]
	private float _boostThrust = 23f;

	[SerializeField]
	private float _boostSeconds = 1f;

	[SerializeField]
	private float _chargeSecondsGround = 1f;

	[SerializeField]
	private float _chargeSecondsAir = 8f;

	private bool _boostActivated;

	private float _boostChargeFraction;

	private float _chargeSeconds;

	private float _lastJumpTime;

	private OWRigidbody _attachBody;

	protected override void Awake()
	{
		_chargeSeconds = _chargeSecondsAir;
		PlayerCharacterController component = GetComponent<PlayerCharacterController>();
		if (component != null)
		{
			component.OnBecomeGrounded += OnBecomeGrounded;
			component.OnBecomeUngrounded += OnBecomeUngrounded;
			component.OnJump += OnJump;
		}
		GlobalMessenger<OWRigidbody>.AddListener("AttachPlayerToPoint", OnAttachPlayerToPoint);
		GlobalMessenger.AddListener("DetachPlayerFromPoint", OnDetachPlayerFromPoint);
		base.Awake();
	}

	protected override void OnDestroy()
	{
		PlayerCharacterController component = GetComponent<PlayerCharacterController>();
		if (component != null)
		{
			component.OnBecomeGrounded -= OnBecomeGrounded;
			component.OnBecomeUngrounded -= OnBecomeUngrounded;
			component.OnJump -= OnJump;
		}
		GlobalMessenger<OWRigidbody>.RemoveListener("AttachPlayerToPoint", OnAttachPlayerToPoint);
		GlobalMessenger.RemoveListener("DetachPlayerFromPoint", OnDetachPlayerFromPoint);
	}

	public float GetBoostMaxThrust()
	{
		return _boostThrust;
	}

	public float GetBoostChargeFraction()
	{
		return _boostChargeFraction;
	}

	public bool IsBoosterFiring()
	{
		return _boostActivated;
	}

	public bool IsBoosterCharged()
	{
		return _boostChargeFraction >= 0.1f;
	}

	public bool IsBoosterReadyToFire()
	{
		if (!_boostActivated)
		{
			return _boostChargeFraction >= 0.1f;
		}
		return false;
	}

	public void DebugResetBoostCharge()
	{
		_boostChargeFraction = 1f;
	}

	public override void ActivateBoost()
	{
		if (IsBoosterReadyToFire())
		{
			_boostActivated = true;
			if (Locator.GetPlayerController().IsGrounded() && Time.time > _lastJumpTime + 0.1f)
			{
				_owRigidbody.AddVelocityChange(base.transform.up * _boostGroundVelocity);
			}
			RumbleManager.StartJetpackBoost();
		}
	}

	public override void DeactivateBoost()
	{
		_boostActivated = false;
		RumbleManager.StopJetpackBoost();
	}

	protected override void FireTranslationalThrusters()
	{
		float y = _translationalInput.y * _maxTranslationalThrust;
		if (_boostActivated)
		{
			float num = 1f / 15f - Time.fixedDeltaTime * 0.5f;
			float nerfDuration = 0f;
			if (Time.time < _lastJumpTime + num)
			{
				y = 0f;
			}
			else if (Locator.GetPlayerRulesetDetector().IsJetpackBoosterNerfed(out nerfDuration))
			{
				float num2 = Mathf.InverseLerp(_lastJumpTime + num, _lastJumpTime + nerfDuration, Time.time);
				y = _boostThrust * num2;
			}
			else
			{
				y = _boostThrust;
			}
			_boostChargeFraction -= Time.deltaTime / _boostSeconds;
			_boostChargeFraction = Mathf.Clamp01(_boostChargeFraction);
			if (_boostChargeFraction == 0f)
			{
				_boostActivated = false;
				RumbleManager.StopJetpackBoost();
			}
		}
		else
		{
			_boostChargeFraction += Time.deltaTime / _chargeSeconds;
			_boostChargeFraction = Mathf.Clamp01(_boostChargeFraction);
		}
		_localAcceleration = new Vector3(_translationalInput.x * _maxTranslationalThrust, y, _translationalInput.z * _maxTranslationalThrust);
		_isTranslationalFiring = _localAcceleration.magnitude > 0f;
		if (_isTranslationalFiring)
		{
			_owRigidbody.AddLocalAcceleration(_localAcceleration);
			if (Locator.GetPlayerRulesetDetector().GetRingRiverRuleset() != null)
			{
				Vector3 localAcceleration = Locator.GetPlayerRulesetDetector().GetRingRiverRuleset().CalculateJetpackCounterAcceleration(_localAcceleration, base.transform, _owRigidbody);
				_owRigidbody.AddLocalAcceleration(localAcceleration);
			}
		}
	}

	protected override void OnInitAlignment()
	{
		base.OnInitAlignment();
	}

	protected override void OnBreakAlignment()
	{
		base.OnBreakAlignment();
		_boostActivated = false;
		_boostChargeFraction = 0f;
		RumbleManager.StopJetpackBoost();
	}

	private void OnBecomeUngrounded()
	{
		_chargeSeconds = _chargeSecondsAir;
	}

	private void OnBecomeGrounded()
	{
		_chargeSeconds = _chargeSecondsGround;
	}

	private void OnJump()
	{
		_lastJumpTime = Time.time;
	}

	private void OnAttachPlayerToPoint(OWRigidbody attachBody)
	{
		_attachBody = attachBody;
		_manualAngularVelocity = Vector3.zero;
		_updateManualAngularVelocity = false;
	}

	private void OnDetachPlayerFromPoint()
	{
		if (_attachBody != null)
		{
			_manualAngularVelocity = _attachBody.GetAngularVelocity();
		}
		else
		{
			Debug.LogError("No attach body!", this);
		}
		_updateManualAngularVelocity = true;
	}
}
