using UnityEngine;

public class TurbulenceAudioController : MonoBehaviour
{
	[SerializeField]
	private TurbulenceAudio[] _turbulenceAudio;

	private DynamicFluidDetector _fluidDetector;

	private void Start()
	{
		_fluidDetector = base.gameObject.GetAttachedOWRigidbody().GetRequiredComponentInChildren<DynamicFluidDetector>();
		for (int i = 0; i < _turbulenceAudio.Length; i++)
		{
			_turbulenceAudio[i].Initialize();
		}
	}

	private void Update()
	{
		float fluidSpeed = _fluidDetector.GetRelativeFluidVelocity(FluidVolume.Type.AIR).magnitude + _fluidDetector.GetRelativeFluidVelocity(FluidVolume.Type.CLOUD).magnitude;
		float fluidDensity = _fluidDetector.GetFluidDensity(FluidVolume.Type.AIR) + _fluidDetector.GetFluidDensity(FluidVolume.Type.CLOUD);
		for (int i = 0; i < _turbulenceAudio.Length; i++)
		{
			_turbulenceAudio[i].Update(fluidSpeed, fluidDensity, _fluidDetector.CompareName(Detector.Name.Ship));
		}
		RumbleManager.UpdateAirTurbulence(fluidSpeed, fluidDensity, _fluidDetector.CompareName(Detector.Name.Ship));
	}
}
