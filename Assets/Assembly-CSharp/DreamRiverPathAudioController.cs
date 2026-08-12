using UnityEngine;

public class DreamRiverPathAudioController : RiverPathAudioController
{
	[SerializeField]
	private RiverPathAudioSource _riverSource;

	private bool _playerBelowSurface;

	protected override void Awake()
	{
		base.Awake();
		_riverAudioSources = new RiverPathAudioSource[1] { _riverSource };
	}

	protected override RiverPathAudioSource GetDefaultRiverSource()
	{
		return _riverSource;
	}

	protected override void OnActivationUpdated(bool active)
	{
		if (!active && _playerBelowSurface)
		{
			RemoveAudioModifier(AudioModifier.Mute);
			_playerBelowSurface = false;
		}
	}

	protected override void FixedUpdate()
	{
		bool flag = base.transform.InverseTransformPoint(Locator.GetPlayerCameraDetector().transform.position).y < 0f;
		if (_playerBelowSurface != flag)
		{
			_playerBelowSurface = flag;
			if (flag)
			{
				AddAudioModifier(AudioModifier.Mute);
			}
			else
			{
				RemoveAudioModifier(AudioModifier.Mute);
			}
		}
		base.FixedUpdate();
	}
}
