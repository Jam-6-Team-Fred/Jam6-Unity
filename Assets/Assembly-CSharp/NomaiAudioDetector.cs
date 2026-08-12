using UnityEngine;

public class NomaiAudioDetector : Detector
{
	private NomaiTranslator _translator;

	private Equalizer _equalizer;

	private NomaiAudioVolume _currentAudioVolume;

	private Transform _cameraTransform;

	protected void Start()
	{
		_translator = Locator.GetPlayerTransform().GetComponentInChildren<NomaiTranslator>();
		_equalizer = Locator.GetPlayerTransform().GetComponentInChildren<Equalizer>(includeInactive: true);
		if (base.gameObject.CompareTag("PlayerDetector"))
		{
			_cameraTransform = Locator.GetPlayerCamera().transform;
		}
		else
		{
			Debug.LogError("Only the player should have a NomaiAudioDetector");
		}
	}

	protected void Update()
	{
		if (_currentAudioVolume != null)
		{
			float normalizedDistToOrigin = _currentAudioVolume.GetNormalizedDistToOrigin(base.transform.position);
			if (_equalizer != null)
			{
				_equalizer.SetVolume(1f - normalizedDistToOrigin * 0.5f);
				_equalizer.SetSignal(1f - normalizedDistToOrigin);
			}
		}
	}

	public NomaiAudioVolume GetFocusedNomaiAudioVolume()
	{
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			if (Vector3.Angle(_activeVolumes[i].transform.position - _cameraTransform.position, _cameraTransform.forward) < 45f)
			{
				return (NomaiAudioVolume)_activeVolumes[i];
			}
		}
		return null;
	}

	protected override void OnVolumeAdded(EffectVolume effectVol)
	{
		NomaiAudioVolume nomaiAudioVolume = effectVol as NomaiAudioVolume;
		if (!(nomaiAudioVolume == null))
		{
			_currentAudioVolume = nomaiAudioVolume;
			_translator.SetNomaiAudio(nomaiAudioVolume);
		}
	}

	protected override void OnVolumeRemoved(EffectVolume effectVol)
	{
		if (!(effectVol as NomaiAudioVolume == null))
		{
			_currentAudioVolume = null;
			_translator.ClearNomaiAudio();
			if (_equalizer != null)
			{
				_equalizer.SetVolume(0f);
				_equalizer.SetSignal(0f);
			}
		}
	}
}
