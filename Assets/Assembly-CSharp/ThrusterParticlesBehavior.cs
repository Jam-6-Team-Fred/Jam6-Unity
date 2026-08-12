using UnityEngine;

public class ThrusterParticlesBehavior : MonoBehaviour
{
	private ThrusterModel _thrusterModel;

	private FluidDetector _fluidDetector;

	private ParticleSystem _thrustingParticles;

	private RelativisticParticleSystem _rpsController;

	[SerializeField]
	private Thruster _thruster;

	[SerializeField]
	private bool _underwaterParticles;

	private Vector3 _thrusterFilter;

	private bool _underwater;

	private void Awake()
	{
		_thrusterModel = base.gameObject.GetAttachedOWRigidbody().GetRequiredComponent<ThrusterModel>();
		_fluidDetector = base.gameObject.GetAttachedOWRigidbody().GetComponentInChildren<FluidDetector>();
		_thrustingParticles = base.gameObject.GetComponent<ParticleSystem>();
		_rpsController = _thrustingParticles.GetComponent<RelativisticParticleSystem>();
		_thrusterFilter = OWUtilities.GetShipThrusterFilter(_thruster);
		_underwater = false;
		_thrustingParticles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
		if (_rpsController != null)
		{
			_rpsController.enabled = false;
		}
		_thrusterModel.OnStartTranslationalThrust += OnStartTranslationalThrust;
		_thrusterModel.OnStopTranslationalThrust += OnStopTranslationalThrust;
		if (_fluidDetector != null)
		{
			_fluidDetector.OnEnterFluidType += OnEnterExitFluidType;
			_fluidDetector.OnExitFluidType += OnEnterExitFluidType;
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_thrusterModel.OnStartTranslationalThrust -= OnStartTranslationalThrust;
		_thrusterModel.OnStopTranslationalThrust -= OnStopTranslationalThrust;
		if (_fluidDetector != null)
		{
			_fluidDetector.OnEnterFluidType -= OnEnterExitFluidType;
			_fluidDetector.OnExitFluidType -= OnEnterExitFluidType;
		}
	}

	private void Update()
	{
		if (((_underwater != _underwaterParticles) ? 0f : Vector3.Dot(_thrusterModel.GetLocalAcceleration(), _thrusterFilter)) > 1f)
		{
			if (!_thrustingParticles.isPlaying)
			{
				_thrustingParticles.Play();
			}
		}
		else if (_thrustingParticles.isPlaying)
		{
			_thrustingParticles.Stop();
		}
	}

	private void OnStartTranslationalThrust()
	{
		if (_rpsController != null)
		{
			_rpsController.enabled = true;
		}
		base.enabled = true;
	}

	private void OnStopTranslationalThrust()
	{
		_thrustingParticles.Stop();
		if (_rpsController != null)
		{
			_rpsController.enabled = false;
		}
		base.enabled = false;
	}

	private void OnEnterExitFluidType(FluidVolume.Type type)
	{
		_underwater = _fluidDetector.InFluidType(FluidVolume.Type.WATER);
	}
}
