using UnityEngine;

public class GlitchedCodeDoorInterface : PictureFrameDoorInterface
{
	[Space]
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private OWAudioSource _loopingAudio;

	[SerializeField]
	private Transform[] _simulationTransforms;

	private bool _outsideLanternBounds;

	protected override void Awake()
	{
		base.Awake();
		_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
	}

	protected override void Start()
	{
		base.Start();
		_loopingAudio.SetLocalVolume(0f);
		DreamWorldController dreamWorldController = Locator.GetDreamWorldController();
		if (dreamWorldController != null)
		{
			dreamWorldController.OnEnterLanternBounds += new OWEvent.OWCallback(OnEnterLanternBounds);
			dreamWorldController.OnExitLanternBounds += new OWEvent.OWCallback(OnExitLanternBounds);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		DreamWorldController dreamWorldController = Locator.GetDreamWorldController();
		if (dreamWorldController != null)
		{
			dreamWorldController.OnEnterLanternBounds -= new OWEvent.OWCallback(OnEnterLanternBounds);
			dreamWorldController.OnExitLanternBounds -= new OWEvent.OWCallback(OnExitLanternBounds);
		}
	}

	protected override void ToggleOpenState()
	{
		base.ToggleOpenState();
		CheckPlayGlitchAudio();
	}

	private void OnSectorOccupantsUpdated()
	{
		CheckPlayGlitchAudio();
	}

	private void OnEnterLanternBounds()
	{
		_outsideLanternBounds = false;
		CheckPlayGlitchAudio();
	}

	private void OnExitLanternBounds()
	{
		_outsideLanternBounds = true;
		CheckPlayGlitchAudio();
	}

	private void CheckPlayGlitchAudio()
	{
		bool flag = _door.IsOpen() && _outsideLanternBounds && _sector.ContainsAnyOccupants(DynamicOccupant.Player);
		if (flag && (!_loopingAudio.isPlaying || _loopingAudio.IsFadingOut()))
		{
			_loopingAudio.FadeIn(1f);
		}
		else if (!flag && _loopingAudio.isPlaying && !_loopingAudio.IsFadingOut())
		{
			_loopingAudio.FadeOut(1f);
		}
	}
}
