using UnityEngine;

[RequireComponent(typeof(OWAudioSource))]
public class MenuAudioController : MonoBehaviour
{
	private OWAudioSource _audioSource;

	private void Awake()
	{
		_audioSource = GetComponent<OWAudioSource>();
	}

	public void PlayRebindKey()
	{
		_audioSource.PlayOneShot(AudioType.Menu_RebindKey);
	}

	public void PlayResetDefaults()
	{
		_audioSource.PlayOneShot(AudioType.Menu_ResetDefaults);
	}

	public void PlayButtonFocus()
	{
		_audioSource.PlayOneShot(AudioType.Menu_UpDown);
	}

	public void PlayOptionToggle()
	{
		_audioSource.PlayOneShot(AudioType.Menu_LeftRight);
	}

	public void PlaySliderIncrement()
	{
		_audioSource.PlayOneShot(AudioType.Menu_SliderIncrement);
	}

	public void PlayChangeTab()
	{
		_audioSource.PlayOneShot(AudioType.Menu_ChangeTab);
	}

	public void PlayOpenPauseMenu()
	{
		_audioSource.PlayOneShot(AudioType.Menu_Pause);
	}

	public void PlayClosePauseMenu()
	{
		_audioSource.PlayOneShot(AudioType.Menu_Unpause);
	}

	public void PlayNegativeUISound()
	{
		_audioSource.PlayOneShot(AudioType.NonDiaUINegativeSFX);
	}

	public void PlayKonamiCode()
	{
		_audioSource.PlayOneShot(AudioType.Menu_KonamiCode);
	}
}
