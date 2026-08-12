using UnityEngine;

public class ThrusterFlameController : MonoBehaviour
{
	[SerializeField]
	private Thruster _thruster;

	[SerializeField]
	private Light _light;

	[SerializeField]
	private AnimationCurve _scaleByThrust = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private DampedSpring _scaleSpring = new DampedSpring();

	[SerializeField]
	private float _belowMaxThrustScalar = 1f;

	private MeshRenderer _thrusterRenderer;

	private ThrusterModel _thrusterModel;

	private FluidDetector _fluidDetector;

	private RulesetDetector _rulesetDetector;

	private Vector3 _thrusterFilter;

	private bool _thrustersFiring;

	private bool _underwater;

	private float _baseLightRadius;

	private float _currentScale;

	private void Awake()
	{
		_thrusterRenderer = GetComponent<MeshRenderer>();
		_thrusterModel = base.gameObject.GetAttachedOWRigidbody().GetRequiredComponent<ThrusterModel>();
		_fluidDetector = base.gameObject.GetAttachedOWRigidbody().GetComponentInChildren<FluidDetector>();
		_rulesetDetector = base.gameObject.GetAttachedOWRigidbody().GetComponentInChildren<RulesetDetector>();
		_thrusterFilter = OWUtilities.GetShipThrusterFilter(_thruster);
		_thrustersFiring = false;
		_underwater = false;
		_baseLightRadius = _light.range;
		_currentScale = 0f;
		_thrusterRenderer.enabled = false;
		_light.enabled = false;
		base.enabled = false;
		_thrusterModel.OnStartTranslationalThrust += OnStartTranslationalThrust;
		_thrusterModel.OnStopTranslationalThrust += OnStopTranslationalThrust;
		if (_fluidDetector != null)
		{
			_fluidDetector.OnEnterFluidType += OnEnterExitFluidType;
			_fluidDetector.OnExitFluidType += OnEnterExitFluidType;
		}
	}

	private void OnDestroy()
	{
		if (_thrusterModel != null)
		{
			_thrusterModel.OnStartTranslationalThrust -= OnStartTranslationalThrust;
			_thrusterModel.OnStopTranslationalThrust -= OnStopTranslationalThrust;
		}
		if (_fluidDetector != null)
		{
			_fluidDetector.OnEnterFluidType -= OnEnterExitFluidType;
			_fluidDetector.OnExitFluidType -= OnEnterExitFluidType;
		}
	}

	private void Update()
	{
		float num = (_underwater ? 0f : _scaleByThrust.Evaluate(GetThrustFraction()));
		if (_belowMaxThrustScalar < 1f && _rulesetDetector.GetThrustLimit() < _thrusterModel.GetMaxTranslationalThrust() - 1f)
		{
			num *= _belowMaxThrustScalar;
		}
		_currentScale = _scaleSpring.Update(_currentScale, num, Time.deltaTime);
		if (_currentScale < 0f)
		{
			_currentScale = 0f;
			_scaleSpring.ResetVelocity();
		}
		if ((!_thrustersFiring || _underwater) && _currentScale <= 0.001f)
		{
			_currentScale = 0f;
			_scaleSpring.ResetVelocity();
			base.enabled = false;
		}
		base.transform.localScale = Vector3.one * _currentScale;
		_light.range = _baseLightRadius * _currentScale;
		_thrusterRenderer.enabled = _currentScale > 0f;
		_light.enabled = _currentScale > 0f;
	}

	private void OnStartTranslationalThrust()
	{
		_thrustersFiring = true;
		if (!_underwater)
		{
			base.enabled = true;
		}
	}

	private void OnStopTranslationalThrust()
	{
		_thrustersFiring = false;
	}

	private void OnEnterExitFluidType(FluidVolume.Type type)
	{
		_underwater = _fluidDetector.InFluidType(FluidVolume.Type.WATER);
		if (!_underwater && _thrustersFiring)
		{
			base.enabled = true;
		}
	}

	private float GetThrustFraction()
	{
		if (!_thrusterModel.IsThrusterBankEnabled(OWUtilities.GetShipThrusterBank(_thruster)))
		{
			return 0f;
		}
		return Vector3.Dot(_thrusterModel.GetLocalAcceleration(), _thrusterFilter);
	}
}
