using UnityEngine;

public class TravelMusicController : MonoBehaviour
{
	private bool _isTraveling;

	private bool _wasTraveling;

	private OWAudioSource _audioSource;

	private void Awake()
	{
		_audioSource = this.GetRequiredComponent<OWAudioSource>();
	}

	private void Update()
	{
		_isTraveling = PlayerState.AtFlightConsole() && Locator.GetPlayerRulesetDetector().AllowTravelMusic();
		if (_isTraveling && !_wasTraveling)
		{
			_audioSource.FadeIn(5f);
		}
		else if (!_isTraveling && _wasTraveling)
		{
			_audioSource.FadeOut(5f, OWAudioSource.FadeOutCompleteAction.PAUSE);
		}
		_wasTraveling = _isTraveling;
	}
}
