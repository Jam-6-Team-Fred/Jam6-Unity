using UnityEngine;

public class AlignPlayerWithForce : AlignWithForce
{
	[SerializeField]
	private DampedSpring2D _airWobbleSpring2D = new DampedSpring2D();

	private OWCamera _playerCam;

	private PlayerCameraController _playerCameraController;

	private bool _updateSteadyCam;

	private bool _updateDiscreteRotation;

	private bool _forceSteadyCam;

	private bool _skipFrame;

	private Quaternion _fromRotation;

	private Quaternion _toRotation;

	private Quaternion _betweenRotation;

	private Quaternion _lastBetweenRotation;

	private float _initDiscreteRotationTime;

	private float _discreteRotationDuration;

	private Vector3 _currentAlignmentDirection;

	private Vector3 _wobbleAxis;

	private bool _waitForAlignment;

	private float _degreesPerSecond;

	private float _initDegreesToTarget;

	private Vector2 _airWobbleDegrees2D;

	protected override void Awake()
	{
		base.Awake();
		GetComponent<ImpactSensor>().OnImpact += OnImpact;
		GlobalMessenger.AddListener("SuitUp", OnSuitUp);
	}

	protected override void Start()
	{
		base.Start();
		_playerCam = Locator.GetPlayerCamera();
		_playerCameraController = _playerCam.GetRequiredComponent<PlayerCameraController>();
	}

	protected virtual void OnDestroy()
	{
		GetComponent<ImpactSensor>().OnImpact -= OnImpact;
		GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_updateDiscreteRotation = false;
		_updateSteadyCam = false;
	}

	public void ForceSteadyCam()
	{
		_forceSteadyCam = true;
	}

	public void SkipNextFrame()
	{
		_skipFrame = true;
	}

	public override void ManagedFixedUpdate()
	{
		if (OWTime.IsPaused())
		{
			return;
		}
		if (_skipFrame)
		{
			_skipFrame = false;
			return;
		}
		UpdateAlignment();
		if (_doAlignment && !PlayerState.IsAttached())
		{
			if (!Locator.GetPlayerController().AllowMidairWobble())
			{
				_airWobbleSpring2D.ResetVelocity();
				_airWobbleDegrees2D = Vector2.zero;
			}
			if (_airWobbleSpring2D.velocity.magnitude == 0f)
			{
				_currentAlignmentDirection = -base.transform.up;
			}
			_airWobbleDegrees2D = _airWobbleSpring2D.Update(_airWobbleDegrees2D, Vector2.zero, Time.deltaTime);
			Quaternion quaternion = Quaternion.AngleAxis(_airWobbleDegrees2D.x, base.transform.forward) * Quaternion.AngleAxis(_airWobbleDegrees2D.y, base.transform.right);
			_alignmentDirection = GetAlignmentDirection();
			_degreesToTarget = Vector3.Angle(_currentAlignmentDirection, _alignmentDirection);
			if (_degreesToTarget == 0f)
			{
				_degreesToTarget = 0.0001f;
			}
			bool flag = _degreesToTarget < 0.5f;
			if (!_waitForAlignment)
			{
				if (flag)
				{
					_degreesPerSecond = 1000f;
				}
				else
				{
					StartAlignment();
				}
			}
			else if (flag)
			{
				_waitForAlignment = false;
			}
			else if (_degreesToTarget > _initDegreesToTarget)
			{
				StartAlignment();
			}
			_adjustedSlerpRate = _degreesPerSecond / _degreesToTarget * Time.fixedDeltaTime;
			_adjustedSlerpRate = Mathf.Clamp01(_adjustedSlerpRate);
			UpdateCurrentAlignment();
			UpdateRotation(-base.transform.up, quaternion * _currentAlignmentDirection, 1f, usePhysics: true);
		}
		if (_updateDiscreteRotation)
		{
			UpdateDiscreteRotation();
		}
	}

	private void UpdateCurrentAlignment()
	{
		Vector3 currentAlignmentDirection = _currentAlignmentDirection;
		if (Vector3.Angle(_alignmentDirection, -_currentAlignmentDirection) < 45f)
		{
			Vector3 vector = -_currentAlignmentDirection;
			Vector3 vector2 = Vector3.Cross(base.transform.right, vector);
			Vector3 vector3 = Vector3.ProjectOnPlane(_alignmentDirection, vector2);
			Quaternion quaternion = Quaternion.AngleAxis(OWMath.Angle(-vector, vector3, vector2), vector2);
			Quaternion quaternion2 = Quaternion.FromToRotation(vector3, _alignmentDirection);
			Quaternion quaternion3 = Quaternion.LookRotation(vector2, vector);
			Quaternion quaternion4 = quaternion * quaternion3;
			quaternion4 = quaternion2 * quaternion4;
			Quaternion quaternion5 = Quaternion.Slerp(quaternion3, quaternion4, _adjustedSlerpRate);
			_currentAlignmentDirection = quaternion5 * -Vector3.up;
		}
		else
		{
			_currentAlignmentDirection = Vector3.Slerp(_currentAlignmentDirection, _alignmentDirection, _adjustedSlerpRate);
		}
		if (_updateSteadyCam)
		{
			Vector3 from = Vector3.ProjectOnPlane(currentAlignmentDirection, base.transform.right);
			Vector3 to = Vector3.ProjectOnPlane(_currentAlignmentDirection, base.transform.right);
			float deltaDegreesY = OWMath.Angle(from, to, base.transform.right);
			_playerCameraController.AddDegreesY(deltaDegreesY);
			if (_degreesToTarget < 1f)
			{
				_updateSteadyCam = false;
			}
		}
	}

	protected override void UpdateRotation(Vector3 currentDirection, Vector3 targetDirection, float slerpRate, bool usePhysics)
	{
		_owRigidbody.SetAngularVelocity(Vector3.zero);
		Vector3 vector = OWPhysics.FromToAngularVelocity(currentDirection, targetDirection);
		_owRigidbody.AddAngularVelocityChange(vector * slerpRate);
	}

	protected override void InitAlignment()
	{
		base.InitAlignment();
		_updateDiscreteRotation = false;
		_currentAlignmentDirection = -base.transform.up;
		GlobalMessenger.FireEvent("InitPlayerForceAlignment");
	}

	protected override void BreakAlignment()
	{
		base.BreakAlignment();
		_updateSteadyCam = false;
		if (PlayerState.IsWearingSuit() && !PlayerState.IsAttached())
		{
			float degreesPerSecond = 50f;
			_playerCameraController.CenterCamera(degreesPerSecond, smoothStep: true);
			StartDiscreteRotation(base.transform.rotation, _playerCam.transform.rotation, degreesPerSecond);
		}
		_owRigidbody.SetAngularVelocity(Vector3.zero);
		GlobalMessenger.FireEvent("BreakPlayerForceAlignment");
	}

	private void StartAlignment()
	{
		if (_degreesToTarget > 30f)
		{
			float num = Vector3.Angle(_playerCam.transform.forward, GetAlignmentDirection());
			float num2 = Vector3.Angle(_playerCam.transform.forward, Quaternion.AngleAxis(-90f, base.transform.right) * _alignmentDirection);
			_updateSteadyCam = _forceSteadyCam || num < 90f || num2 < 45f;
			_forceSteadyCam = false;
		}
		float t = Mathf.InverseLerp(90f, 0f, _degreesToTarget);
		_degreesPerSecond = Mathf.Lerp(150f, 5f, t);
		_waitForAlignment = true;
		_initDegreesToTarget = _degreesToTarget;
		if (_forceDetector.GetAlignmentAngularVelocity().magnitude > 0.5f)
		{
			_degreesPerSecond = 150f;
		}
	}

	private void StartDiscreteRotation(Quaternion fromRotation, Quaternion toRotation, float degreesPerSecond)
	{
		_fromRotation = (_betweenRotation = fromRotation);
		_toRotation = toRotation;
		_initDiscreteRotationTime = Time.time;
		_discreteRotationDuration = Quaternion.Angle(_fromRotation, _toRotation) / degreesPerSecond;
		if (_discreteRotationDuration > 0f)
		{
			_updateDiscreteRotation = true;
		}
	}

	private void UpdateDiscreteRotation()
	{
		float t = Mathf.InverseLerp(_initDiscreteRotationTime, _initDiscreteRotationTime + _discreteRotationDuration, Time.time);
		t = Mathf.SmoothStep(0f, 1f, t);
		_lastBetweenRotation = _betweenRotation;
		_betweenRotation = Quaternion.Slerp(_fromRotation, _toRotation, t);
		Quaternion deltaRotation = _betweenRotation * Quaternion.Inverse(_lastBetweenRotation);
		_owRigidbody.AddRotation(deltaRotation);
		if (t >= 1f)
		{
			_updateDiscreteRotation = false;
		}
	}

	private void OnImpact(ImpactData impactData)
	{
		if (Locator.GetPlayerController().AllowMidairWobble() && _doAlignment)
		{
			Vector3 vector = base.transform.InverseTransformPoint(impactData.point);
			Vector3 vector2 = base.transform.InverseTransformDirection(impactData.velocity);
			_airWobbleSpring2D.velocity += new Vector2(0f - vector2.x, vector2.z) * Mathf.Sign(vector.y) * 10f;
		}
	}

	private void OnSuitUp()
	{
		if (!_doAlignment && !PlayerState.IsAttached() && LoadManager.GetCurrentScene() != OWScene.EyeOfTheUniverse)
		{
			float degreesPerSecond = 50f;
			_playerCameraController.CenterCamera(degreesPerSecond, smoothStep: true);
			StartDiscreteRotation(base.transform.rotation, _playerCam.transform.rotation, degreesPerSecond);
		}
	}
}
