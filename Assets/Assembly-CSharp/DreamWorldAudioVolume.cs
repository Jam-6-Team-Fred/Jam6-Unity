using UnityEngine;

public class DreamWorldAudioVolume : AudioVolume
{
	[SerializeField]
	private AudioModifier _riverModifier;

	[SerializeField]
	private bool _undergroundLake;

	private RiverPathAudioController _riverPathAudioController;

	protected override void Start()
	{
		base.Start();
		DreamWorldAudioController dreamWorldAudioController = Locator.GetDreamWorldAudioController();
		_riverPathAudioController = (_undergroundLake ? dreamWorldAudioController.GetLakePathAudioController() : dreamWorldAudioController.GetRiverPathAudioController());
	}

	public override void Activate()
	{
		base.Activate();
		_riverPathAudioController.AddAudioModifier(_riverModifier);
	}

	public override void Deactivate()
	{
		base.Deactivate();
		_riverPathAudioController.RemoveAudioModifier(_riverModifier);
	}

	public override void Deactivate(float fadeSeconds)
	{
		_isActive = false;
		_owAudioSrc.FadeOut(fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		OnAudioStop.Invoke();
		_riverPathAudioController.RemoveAudioModifier(_riverModifier);
	}
}
