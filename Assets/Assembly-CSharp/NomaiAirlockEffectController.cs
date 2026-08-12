using UnityEngine;

public class NomaiAirlockEffectController : MonoBehaviour
{
	[SerializeField]
	private NomaiAirlock _airlock;

	[SerializeField]
	private ParticleSystem _airPourInParticles;

	[SerializeField]
	private ParticleSystem _airPourOutParticles;

	[SerializeField]
	private FluidVolume _pouringFluid;

	[SerializeField]
	private float _fluidPourDuration = 3f;

	private float _airPourInTime;

	private void Awake()
	{
		_airlock.OnAirPourIn += OnAirPourIn;
		_airlock.OnAirPourOut += OnAirPourOut;
	}

	private void Start()
	{
		_pouringFluid.SetVolumeActivation(active: false);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_airlock.OnAirPourIn -= OnAirPourIn;
		_airlock.OnAirPourOut -= OnAirPourOut;
	}

	private void Update()
	{
		if (Time.time > _airPourInTime + _fluidPourDuration)
		{
			_pouringFluid.SetVolumeActivation(active: false);
		}
	}

	private void OnAirPourIn()
	{
		_airPourInParticles.Play();
		_pouringFluid.SetVolumeActivation(active: true);
		_airPourInTime = Time.time;
		base.enabled = true;
	}

	private void OnAirPourOut()
	{
		_airPourOutParticles.Play();
		_pouringFluid.SetVolumeActivation(active: true);
		_airPourInTime = Time.time;
		base.enabled = true;
	}
}
