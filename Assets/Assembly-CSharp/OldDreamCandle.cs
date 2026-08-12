using UnityEngine;

public class OldDreamCandle : MonoBehaviour
{
	[SerializeField]
	private bool _startLit = true;

	[Space]
	[SerializeField]
	private GameObject _objectsVisibleInLight;

	[SerializeField]
	private OWLightController[] _linkedLightControllers;

	[SerializeField]
	private GhostController[] _ghostsToWake;

	[SerializeField]
	private DarkZone _darkZone;

	[Space]
	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private OWLightController _candleLightController;

	[SerializeField]
	private ParticleSystem _candleParticles;

	[SerializeField]
	private LensFlare _lensFlare;

	[SerializeField]
	private AudioVolume _audioVolume;

	public OWEvent OnOldDreamCandleLit = new OWEvent(16);

	private float _lastFadeTime;

	private int _fadeIndex = -1;

	private bool _lit;

	private void Awake()
	{
		_interactReceiver.OnPressInteract += OnPressInteract;
	}

	private void Start()
	{
		base.enabled = false;
		_lit = _startLit;
		_objectsVisibleInLight.SetActive(_lit);
		_candleLightController.FadeTo(_lit ? 1f : 0f, 0f);
		_audioVolume.SetVolumeActivation(active: false);
		if (_lit)
		{
			_candleParticles.Play();
		}
		UpdatePrompt();
	}

	private void OnDestroy()
	{
		_interactReceiver.OnPressInteract -= OnPressInteract;
	}

	private void UpdatePrompt()
	{
		_interactReceiver.SetPromptText(_lit ? UITextType.RoastingExtinguishPrompt : UITextType.LightCampfirePrompt);
	}

	private void OnPressInteract()
	{
		base.enabled = true;
		_lastFadeTime = Time.time;
		_lit = !_lit;
		UpdatePrompt();
		if (_linkedLightControllers.Length != 0)
		{
			_fadeIndex = 0;
		}
		if (_lit)
		{
			_candleLightController.FadeTo(1f, 1f);
			_candleParticles.Play();
			_lensFlare.enabled = true;
			_darkZone.RemovePlayerFromZone();
		}
		else
		{
			_candleLightController.FadeTo(0f, 1f);
			_candleParticles.Stop();
			_lensFlare.enabled = false;
			_interactReceiver.DisableInteraction();
			_darkZone.AddPlayerToZone();
			Locator.GetPlayerAudioController().PlayMarshmallowBlowOut();
		}
	}

	private void Update()
	{
		if (!(Time.time > _lastFadeTime + 1f))
		{
			return;
		}
		_lastFadeTime = Time.time;
		if (_fadeIndex >= _linkedLightControllers.Length || _fadeIndex < 0)
		{
			if (_objectsVisibleInLight != null)
			{
				_objectsVisibleInLight.SetActive(_lit);
			}
			_audioVolume.SetVolumeActivation(active: true);
			OnOldDreamCandleLit.Invoke();
			base.enabled = false;
		}
		else if (_linkedLightControllers.Length != 0)
		{
			_linkedLightControllers[_fadeIndex].FadeTo(_lit ? 1f : 0f, 1f);
			_fadeIndex++;
		}
	}
}
