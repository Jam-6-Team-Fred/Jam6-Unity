using UnityEngine;

public class RingWorldAudioVolume : AudioVolume
{
	[SerializeField]
	private AudioModifier _riverModifier;

	[SerializeField]
	private bool _indoors;

	private RiverPathAudioController _riverPathAudioController;

	protected override void Start()
	{
		base.Start();
		_riverPathAudioController = Locator.GetRingWorldController().GetRiverPathAudioController();
	}

	public override void Activate()
	{
		base.Activate();
		_riverPathAudioController.AddAudioModifier(_riverModifier);
		if (_indoors)
		{
			Locator.GetAudioMixer().OnEnterIndoorVolume();
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		_riverPathAudioController.RemoveAudioModifier(_riverModifier);
		if (_indoors)
		{
			Locator.GetAudioMixer().OnExitIndoorVolume();
		}
	}

	public override void Deactivate(float fadeSeconds)
	{
		_isActive = false;
		_owAudioSrc.FadeOut(fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		OnAudioStop.Invoke();
		_riverPathAudioController.RemoveAudioModifier(_riverModifier);
		if (_indoors)
		{
			Locator.GetAudioMixer().OnExitIndoorVolume();
		}
	}
}
