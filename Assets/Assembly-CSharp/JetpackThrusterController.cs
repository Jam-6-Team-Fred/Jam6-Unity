using UnityEngine;

public class JetpackThrusterController : ThrusterController
{
	private Autopilot _autopilot;

	private ReferenceFrameTracker _rfTracker;

	private PlayerResources _resources;

	private Vector3 _liftoffInput;

	private bool _allowMidairHorizontalThrust;

	private PlayerCharacterController _characterController;

	protected override void Awake()
	{
		base.Awake();
		base.enabled = false;
		_autopilot = base.gameObject.AddComponent<Autopilot>();
		base.gameObject.AddComponent<AutopilotGUI>();
		_rfTracker = this.GetRequiredComponent<ReferenceFrameTracker>();
		_resources = this.GetRequiredComponent<PlayerResources>();
		_characterController = GetComponent<PlayerCharacterController>();
		if (_characterController != null)
		{
			_characterController.OnBecomeGrounded += OnBecomeGrounded;
			_characterController.OnBecomeUngrounded += OnBecomeUngrounded;
		}
		GlobalMessenger.AddListener("SuitUp", OnSuitUp);
		GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger.AddListener("EquipTranslator", OnEquipTranslator);
		GlobalMessenger.AddListener("UnequipTranslator", OnUnequipTranslator);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_characterController != null)
		{
			_characterController.OnBecomeGrounded -= OnBecomeGrounded;
			_characterController.OnBecomeUngrounded -= OnBecomeUngrounded;
		}
		GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
		GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger.RemoveListener("EquipTranslator", OnEquipTranslator);
		GlobalMessenger.RemoveListener("UnequipTranslator", OnUnequipTranslator);
	}

	public bool AllowBoostPrompt()
	{
		if (!PlayerData.GetAutoBoost() && OWInput.GetValue(InputLibrary.thrustUp) > 0f && _resources.IsBoosterAllowed())
		{
			return ((JetpackThrusterModel)_thrusterModel).IsBoosterCharged();
		}
		return false;
	}

	public bool IsMatchVelocityAvailable(bool ignorePassiveFrame = false)
	{
		if (PlayerState.InZeroG())
		{
			return _rfTracker.GetReferenceFrame(ignorePassiveFrame) != null;
		}
		return false;
	}

	public Vector3 GetLocalVelocity()
	{
		if (_rfTracker.GetReferenceFrame() != null)
		{
			return _rfTracker.GetReferenceFrame().GetOWRigidBody().GetRelativeVelocity(_thrusterModel.GetAttachedOWRigidbody());
		}
		return _thrusterModel.GetAttachedOWRigidbody().GetVelocity();
	}

	protected override bool AllowHorizontalThrust()
	{
		if (!_characterController.IsGrounded())
		{
			return _allowMidairHorizontalThrust;
		}
		return !_characterController.HasGroundControl();
	}

	protected override Vector3 ReadTranslationalInput()
	{
		if (!_resources.IsJetpackUsable() || _autopilot.IsMatchingVelocity())
		{
			return Vector3.zero;
		}
		Vector3 rawInput = GetRawInput();
		if (!_characterController.IsGrounded())
		{
			Vector2 from = new Vector2(_liftoffInput.x, _liftoffInput.z);
			Vector2 to = new Vector2(rawInput.x, rawInput.z);
			bool flag = Vector2.Angle(from, to) > 10f || from.sqrMagnitude < 0.001f || to.sqrMagnitude < 0.001f;
			if (_isRotationalThrustEnabled || flag || rawInput.y != 0f)
			{
				_allowMidairHorizontalThrust = true;
			}
		}
		return rawInput;
	}

	protected override Vector3 ReadRotationalInput()
	{
		Vector3 zero = Vector3.zero;
		if (_isRollMode)
		{
			zero.z -= OWInput.GetValue(InputLibrary.yaw, InputMode.Character | InputMode.ScopeZoom | InputMode.NomaiRemoteCam);
		}
		else
		{
			zero.y += OWInput.GetValue(InputLibrary.yaw, InputMode.Character | InputMode.ScopeZoom | InputMode.NomaiRemoteCam);
		}
		zero.x -= OWInput.GetValue(InputLibrary.pitch, InputMode.Character | InputMode.ScopeZoom | InputMode.NomaiRemoteCam);
		if (OWInput.IsInputMode(InputMode.ScopeZoom))
		{
			zero *= Locator.GetPlayerCameraController().GetZoomFOVRatio();
		}
		return zero;
	}

	protected override void OnInitAlignment()
	{
		base.OnInitAlignment();
		_autopilot.Abort();
	}

	protected override void OnBreakAlignment()
	{
		base.OnBreakAlignment();
	}

	protected override void Update()
	{
		base.Update();
		if (!_resources.IsJetpackUsable() || !OWInput.IsInputMode(InputMode.Character | InputMode.NomaiRemoteCam))
		{
			if (_autopilot.enabled && !OWInput.IsInputMode(InputMode.Dialogue))
			{
				_autopilot.Abort();
			}
			if (((JetpackThrusterModel)_thrusterModel).IsBoosterFiring())
			{
				_thrusterModel.DeactivateBoost();
			}
			return;
		}
		if (IsMatchVelocityAvailable())
		{
			if (OWInput.IsNewlyPressed(InputLibrary.matchVelocity) && !_autopilot.IsMatchingVelocity())
			{
				_autopilot.StartMatchVelocity(_rfTracker.GetReferenceFrame(ignorePassiveFrame: false));
			}
			if (OWInput.IsNewlyReleased(InputLibrary.matchVelocity, InputMode.Character) && _autopilot.IsMatchingVelocity())
			{
				_autopilot.StopMatchVelocity();
			}
		}
		if (_resources.IsBoosterAllowed() && ((JetpackThrusterModel)_thrusterModel).IsBoosterReadyToFire())
		{
			bool num = PlayerData.GetAutoBoost() && OWInput.IsNewlyPressed(InputLibrary.thrustUp);
			bool flag = OWInput.GetValue(InputLibrary.thrustUp) > 0f && OWInput.IsPressed(InputLibrary.boost) && InputLibrary.boost.PressDuration < 0.1f;
			if (num || flag)
			{
				_thrusterModel.ActivateBoost();
			}
		}
		else if (OWInput.IsNewlyReleased(InputLibrary.thrustUp) || OWInput.IsNewlyReleased(InputLibrary.boost))
		{
			_thrusterModel.DeactivateBoost();
		}
	}

	private Vector3 GetRawInput()
	{
		if (!OWInput.IsInputMode(InputMode.Character | InputMode.NomaiRemoteCam))
		{
			return Vector3.zero;
		}
		Vector3 result = new Vector3(OWInput.GetValue(InputLibrary.thrustX), 0f, OWInput.GetValue(InputLibrary.thrustZ));
		if (result.sqrMagnitude > 1f)
		{
			result.Normalize();
		}
		result.y = OWInput.GetValue(InputLibrary.thrustUp) - OWInput.GetValue(InputLibrary.thrustDown);
		return result;
	}

	private void OnSuitUp()
	{
		base.enabled = true;
	}

	private void OnRemoveSuit()
	{
		base.enabled = false;
	}

	private void OnBecomeGrounded()
	{
		_allowMidairHorizontalThrust = false;
	}

	private void OnBecomeUngrounded()
	{
		_liftoffInput = GetRawInput();
	}

	private void OnEquipTranslator()
	{
		if (PlayerState.InZeroG() && !_autopilot.IsMatchingVelocity())
		{
			OWRigidbody focusedTextParentBody = Locator.GetToolModeSwapper().GetFocusedTextParentBody();
			if (focusedTextParentBody != null)
			{
				_autopilot.StartMatchVelocity(focusedTextParentBody.GetReferenceFrame());
			}
		}
	}

	private void OnUnequipTranslator()
	{
		if (_autopilot.enabled)
		{
			_autopilot.Abort();
		}
	}
}
