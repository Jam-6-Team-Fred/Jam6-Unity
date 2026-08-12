using UnityEngine;

public class DreamCandle : MonoBehaviour
{
	[SerializeField]
	private bool _startLit;

	[Space]
	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private LightSensor _lightSensor;

	[SerializeField]
	private InteractReceiver _interactReceiver;

	[Header("Audio")]
	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private AudioType _lightClip = AudioType.Candle_Light_Small;

	private bool _lit;

	private float _detectLightTime;

	private float _litTime;

	public OWEvent OnLitStateChanged = new OWEvent(1);

	private void Awake()
	{
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		}
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract += OnPressInteract;
		}
	}

	private void Start()
	{
		base.enabled = false;
		_lit = _startLit;
		_lightController.SetIntensity(_lit ? 1f : 0f);
		if (_interactReceiver != null)
		{
			_interactReceiver.SetPromptText(_lit ? UITextType.RoastingExtinguishPrompt : UITextType.LightCampfirePrompt);
			if (!_lit)
			{
				_interactReceiver.DisableInteraction();
			}
		}
	}

	private void OnDestroy()
	{
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		}
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract -= OnPressInteract;
		}
	}

	public bool StartsLit()
	{
		return _startLit;
	}

	public bool IsLit()
	{
		return _lit;
	}

	public void SetPulseIntensity(float intensity)
	{
		_lightController.SetIntensity(intensity);
	}

	private void OnPressInteract()
	{
		base.enabled = false;
		SetLit(!_lit);
		if (!_lit)
		{
			Locator.GetPlayerAudioController().PlayMarshmallowBlowOut();
		}
		if (_interactReceiver != null)
		{
			_interactReceiver.ResetInteraction();
			if (!_lit)
			{
				_interactReceiver.DisableInteraction();
			}
		}
	}

	public void SetLit(bool lit, bool playAudio = true, bool instant = false)
	{
		if (lit != _lit)
		{
			_lit = lit;
			if (instant)
			{
				_lightController.SetIntensity(lit ? 1f : 0f);
			}
			else
			{
				_lightController.FadeTo(lit ? 1f : 0f, lit ? 1f : 0.5f);
			}
			if (_interactReceiver != null)
			{
				_interactReceiver.SetPromptText(_lit ? UITextType.RoastingExtinguishPrompt : UITextType.LightCampfirePrompt);
				_interactReceiver.SetInteractionEnabled(_lit);
			}
			if (playAudio && _audioSource != null)
			{
				_audioSource.PlayOneShot(_lit ? _lightClip : AudioType.Candle_Extinguish);
			}
			OnLitStateChanged.Invoke();
		}
	}

	private void Update()
	{
		if (!_lit)
		{
			if (!_lightSensor.IsIlluminated())
			{
				base.enabled = false;
			}
			else if (Time.time > _detectLightTime + 0.2f)
			{
				SetLit(lit: true);
				base.enabled = false;
			}
		}
	}

	private void OnDetectLight()
	{
		base.enabled = true;
		_detectLightTime = Time.time;
	}
}
