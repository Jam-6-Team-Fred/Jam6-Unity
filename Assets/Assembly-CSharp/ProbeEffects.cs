using UnityEngine;

public class ProbeEffects : MonoBehaviour
{
	private SurveyorProbe _probe;

	private FluidDetector _fluidDetector;

	[SerializeField]
	private OWAudioSource _flightLoopAudio;

	[SerializeField]
	private OWAudioSource _anchorAudio;

	[SerializeField]
	private ParticleSystem _anchorParticles;

	[SerializeField]
	private ParticleSystem _underwaterAnchorParticles;

	private void Awake()
	{
		_probe = this.GetAttachedOWRigidbody().GetRequiredComponent<SurveyorProbe>();
		_fluidDetector = _probe.GetRequiredComponentInChildren<FluidDetector>();
		_probe.OnLaunchProbe += OnLaunch;
		_probe.OnAnchorProbe += OnAnchor;
		_probe.OnUnanchorProbe += OnUnanchor;
		_probe.OnStartRetrieveProbe += OnStartRetrieve;
	}

	private void OnDestroy()
	{
		_probe.OnLaunchProbe -= OnLaunch;
		_probe.OnAnchorProbe -= OnAnchor;
		_probe.OnUnanchorProbe -= OnUnanchor;
		_probe.OnStartRetrieveProbe -= OnStartRetrieve;
	}

	private void OnLaunch()
	{
		_flightLoopAudio.FadeIn(0.1f, fadeFromNothing: true, randomizePlayhead: true);
	}

	private void OnAnchor()
	{
		if (_fluidDetector.InFluidType(FluidVolume.Type.WATER))
		{
			_underwaterAnchorParticles.Play();
		}
		else
		{
			_anchorParticles.Play();
		}
		_flightLoopAudio.FadeOut(0.5f);
		_anchorAudio.PlayOneShot(AudioType.ToolProbeAttach);
	}

	private void OnUnanchor()
	{
		_flightLoopAudio.FadeIn(0.5f);
	}

	private void OnStartRetrieve(float retrieveLength)
	{
		_flightLoopAudio.FadeOut(retrieveLength);
	}
}
