using UnityEngine;

public class IslandController : MonoBehaviour
{
	public delegate void IslandSplashEvent();

	public delegate void IslandEnteredTornadoEvent();

	public delegate void IslandApexEvent();

	public bool debug;

	[SerializeField]
	private FluidVolume _inheritanceFluid;

	[SerializeField]
	private FluidVolume[] _barrierRepelFluids;

	[SerializeField]
	private ForceVolume _zeroGVolume;

	[SerializeField]
	private FluidDetector _fluidDetector;

	[SerializeField]
	private SafetyTractorBeamController[] _safetyTractorBeams;

	[SerializeField]
	private Campfire _campfire;

	private OWRigidbody _islandBody;

	private Transform _transform;

	private Transform _planetTransform;

	private bool _tractorBeamsActive;

	private bool _repelFluidsActive = true;

	private float _splashdownTime;

	public event IslandSplashEvent OnIslandSplashEvent;

	public event IslandEnteredTornadoEvent OnIslandEnteredTornadoEvent;

	public event IslandApexEvent OnIslandApexEvent;

	private void Awake()
	{
		_transform = base.transform;
		_islandBody = GetComponent<OWRigidbody>();
		if (_fluidDetector != null)
		{
			_fluidDetector.OnSpawnSplashEvent += OnSpawnSplash;
		}
	}

	private void Start()
	{
		_planetTransform = GetComponent<OWRigidbody>().GetOrigParentBody().transform;
		_zeroGVolume.SetVolumeActivation(active: false);
		_inheritanceFluid.SetVolumeActivation(active: false);
		SetSafetyBeamActivation(active: false);
	}

	private void OnDestroy()
	{
		if (_fluidDetector != null)
		{
			_fluidDetector.OnSpawnSplashEvent -= OnSpawnSplash;
		}
	}

	private void SetSafetyBeamActivation(bool active)
	{
		if (_safetyTractorBeams != null)
		{
			_tractorBeamsActive = active;
			for (int i = 0; i < _safetyTractorBeams.Length; i++)
			{
				_safetyTractorBeams[i].SetActivation(active);
			}
		}
	}

	private void OnSpawnSplash(FluidVolume fluidVol)
	{
		if (fluidVol.GetFluidType() == FluidVolume.Type.WATER && this.OnIslandSplashEvent != null)
		{
			this.OnIslandSplashEvent();
		}
	}

	private void FixedUpdate()
	{
		Vector3 vector = _transform.position - _planetTransform.position;
		float num = Vector3.Distance(_planetTransform.position, _transform.position);
		Vector3 lhs = Vector3.Project(_islandBody.GetRelativeVelocity(_islandBody.GetOrigParentBody()), vector);
		float num2 = lhs.magnitude * (0f - Mathf.Sign(Vector3.Dot(lhs, vector)));
		if (_safetyTractorBeams != null)
		{
			if (!_tractorBeamsActive && _fluidDetector.InFluidType(FluidVolume.Type.CLOUD))
			{
				SetSafetyBeamActivation(active: true);
			}
			else if (_tractorBeamsActive && !_fluidDetector.InFluidType(FluidVolume.Type.CLOUD) && _fluidDetector.InFluidType(FluidVolume.Type.WATER) && num2 > 0f)
			{
				SetSafetyBeamActivation(active: false);
			}
		}
		if (_fluidDetector.InFluidType(FluidVolume.Type.CLOUD) && !_inheritanceFluid.IsVolumeActive())
		{
			_inheritanceFluid.SetVolumeActivation(active: true);
			if (this.OnIslandEnteredTornadoEvent != null)
			{
				this.OnIslandEnteredTornadoEvent();
			}
		}
		else if (num < 900f && !_fluidDetector.InFluidType(FluidVolume.Type.CLOUD) && _inheritanceFluid.IsVolumeActive())
		{
			_inheritanceFluid.SetVolumeActivation(active: false);
		}
		if (num >= 900f && !_zeroGVolume.IsVolumeActive())
		{
			if (this.OnIslandApexEvent != null)
			{
				this.OnIslandApexEvent();
			}
			_zeroGVolume.SetVolumeActivation(active: true);
			if (_campfire != null)
			{
				_campfire.StopRoasting();
				_campfire.StopSleeping(sudden: true);
				_campfire.SetInteractionEnabled(enabled: false);
			}
		}
		else if (num < 900f && _zeroGVolume.IsVolumeActive())
		{
			_zeroGVolume.SetVolumeActivation(active: false);
			if (_campfire != null)
			{
				_campfire.SetInteractionEnabled(enabled: true);
			}
		}
		if (_repelFluidsActive && _fluidDetector.InFluidType(FluidVolume.Type.CLOUD))
		{
			for (int i = 0; i < _barrierRepelFluids.Length; i++)
			{
				_barrierRepelFluids[i].SetVolumeActivation(active: false);
			}
			_repelFluidsActive = false;
		}
		else if (!_repelFluidsActive && _fluidDetector.InFluidType(FluidVolume.Type.WATER) && !_fluidDetector.InFluidType(FluidVolume.Type.CLOUD))
		{
			for (int j = 0; j < _barrierRepelFluids.Length; j++)
			{
				_barrierRepelFluids[j].SetVolumeActivation(active: true);
			}
			_repelFluidsActive = true;
		}
	}
}
