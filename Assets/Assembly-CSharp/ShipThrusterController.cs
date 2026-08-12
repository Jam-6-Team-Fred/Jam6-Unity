using UnityEngine;

public class ShipThrusterController : ThrusterController
{
	private RulesetDetector _rulesetDetector;

	private LandingPadManager _landingManager;

	private Vector3 _lastTranslationalInput = Vector3.zero;

	private bool _requireIgnition;

	private bool _isIgniting;

	private float _ignitionTime;

	private float _ignitionDuration = 1f;

	private OWRigidbody _shipBody;

	private ShipResources _shipResources;

	private ReferenceFrame _landingRF;

	private Autopilot _autopilot;

	private AlignShipWithReferenceFrame _shipAlignment;

	private bool _limitOrbitSpeed;

	protected override void Awake()
	{
		_landingManager = this.GetRequiredComponentInChildren<LandingPadManager>();
		_shipResources = this.GetRequiredComponent<ShipResources>();
		_shipBody = this.GetRequiredComponent<OWRigidbody>();
		_autopilot = this.GetRequiredComponent<Autopilot>();
		_shipAlignment = this.GetRequiredComponent<AlignShipWithReferenceFrame>();
		GlobalMessenger<ReferenceFrame>.AddListener("EnterLandingMode", OnEnterLandingMode);
		GlobalMessenger.AddListener("ExitLandingMode", OnExitLandingMode);
		base.Awake();
	}

	private void Start()
	{
		_rulesetDetector = Locator.GetShipDetector().GetComponent<RulesetDetector>();
	}

	private void OnEnable()
	{
		_requireIgnition = _landingManager.IsLanded();
	}

	protected override void OnDestroy()
	{
		GlobalMessenger<ReferenceFrame>.RemoveListener("EnterLandingMode", OnEnterLandingMode);
		GlobalMessenger.RemoveListener("ExitLandingMode", OnExitLandingMode);
		base.OnDestroy();
	}

	public bool RequiresIgnition()
	{
		return _requireIgnition;
	}

	protected override Vector3 ReadTranslationalInput()
	{
		float value = OWInput.GetValue(InputLibrary.thrustX);
		float value2 = OWInput.GetValue(InputLibrary.thrustZ);
		float value3 = OWInput.GetValue(InputLibrary.thrustUp);
		float value4 = OWInput.GetValue(InputLibrary.thrustDown);
		if (!OWInput.IsInputMode(InputMode.ShipCockpit | InputMode.LandingCam))
		{
			return Vector3.zero;
		}
		if (!_shipResources.AreThrustersUsable())
		{
			return Vector3.zero;
		}
		if (_autopilot.IsFlyingToDestination())
		{
			return Vector3.zero;
		}
		Vector3 vector = new Vector3(value, 0f, value2);
		if (vector.sqrMagnitude > 1f)
		{
			vector.Normalize();
		}
		vector.y = value3 - value4;
		if (_requireIgnition && _landingManager.IsLanded())
		{
			vector.x = 0f;
			vector.z = 0f;
			vector.y = Mathf.Clamp01(vector.y);
			if (!_isIgniting && _lastTranslationalInput.y <= 0f && vector.y > 0f)
			{
				_isIgniting = true;
				_ignitionTime = Time.time;
				GlobalMessenger.FireEvent("StartShipIgnition");
			}
			if (_isIgniting)
			{
				if (vector.y <= 0f)
				{
					_isIgniting = false;
					GlobalMessenger.FireEvent("CancelShipIgnition");
				}
				if (Time.time < _ignitionTime + _ignitionDuration)
				{
					vector.y = 0f;
				}
				else
				{
					_isIgniting = false;
					_requireIgnition = false;
					GlobalMessenger.FireEvent("CompleteShipIgnition");
					RumbleManager.PlayShipIgnition();
					RumbleManager.SetShipThrottleNormal();
				}
			}
		}
		float num = 1f;
		num = Mathf.Min(_rulesetDetector.GetThrustLimit(), _thrusterModel.GetMaxTranslationalThrust()) / _thrusterModel.GetMaxTranslationalThrust();
		Vector3 vector2 = vector * num;
		if (_limitOrbitSpeed && _shipAlignment.IsAligning() && vector2.magnitude > 0f)
		{
			Vector3 vector3 = _landingRF.GetOWRigidBody().GetWorldCenterOfMass() - _shipBody.GetWorldCenterOfMass();
			Vector3 vector4 = _shipBody.GetVelocity() - _landingRF.GetVelocity();
			Vector3 vector5 = vector4 - Vector3.Project(vector4, vector3);
			Vector3 vector6 = Quaternion.FromToRotation(-_shipBody.transform.up, vector3) * _shipBody.transform.TransformDirection(vector2 * _thrusterModel.GetMaxTranslationalThrust());
			Vector3 vector7 = Vector3.Project(vector6, vector3);
			Vector3 vector8 = vector6 - vector7;
			Vector3 vector9 = vector5 + vector8 * Time.deltaTime;
			float magnitude = vector9.magnitude;
			float orbitSpeed = _landingRF.GetOrbitSpeed(vector3.magnitude);
			if (magnitude > orbitSpeed)
			{
				vector9 = vector9.normalized * orbitSpeed;
				vector8 = (vector9 - vector5) / Time.deltaTime;
				vector6 = vector7 + vector8;
				vector2 = _shipBody.transform.InverseTransformDirection(vector6 / _thrusterModel.GetMaxTranslationalThrust());
				if (vector2.sqrMagnitude > 1f)
				{
					vector2.Normalize();
				}
			}
		}
		_lastTranslationalInput = vector;
		return vector2;
	}

	protected override Vector3 ReadRotationalInput()
	{
		bool flag = OWInput.IsInputMode(InputMode.ShipCockpit) && OWInput.IsPressed(InputLibrary.freeLook);
		if (!OWInput.IsInputMode(InputMode.ShipCockpit | InputMode.LandingCam) || !_shipResources.AreThrustersUsable() || flag)
		{
			return Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		if (!_landingManager.IsLanded())
		{
			if (_isRollMode)
			{
				zero.z -= OWInput.GetValue(InputLibrary.yaw) * (float)_rollScalar;
			}
			else
			{
				zero.y += OWInput.GetValue(InputLibrary.yaw);
			}
			zero.x -= OWInput.GetValue(InputLibrary.pitch);
		}
		return zero;
	}

	private void OnEnterLandingMode(ReferenceFrame referenceFrame)
	{
		_limitOrbitSpeed = true;
		_landingRF = referenceFrame;
	}

	private void OnExitLandingMode()
	{
		_limitOrbitSpeed = false;
		_landingRF = null;
	}
}
