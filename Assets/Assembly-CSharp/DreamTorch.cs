using UnityEngine;

public class DreamTorch : MonoBehaviour
{
	[SerializeField]
	private bool _startLit = true;

	[Space]
	[SerializeField]
	private LightSensor _lightSensor;

	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private ParticleSystem _flameParticles;

	private bool _lit;

	public OWEvent<DreamTorch> OnTorchLit = new OWEvent<DreamTorch>(128);

	public OWEvent<DreamTorch> OnTorchUnlit = new OWEvent<DreamTorch>(128);

	private void Awake()
	{
		_interactReceiver.OnPressInteract += OnPressInteract;
		_lit = _startLit;
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		}
	}

	private void Start()
	{
		base.enabled = false;
		_lightController.FadeTo(_lit ? 1f : 0f, 0f);
		if (_lit)
		{
			_flameParticles.Play();
		}
		UpdatePrompt();
	}

	private void OnDestroy()
	{
		_interactReceiver.OnPressInteract -= OnPressInteract;
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		}
	}

	public bool IsLit()
	{
		return _lit;
	}

	public void SetLit(bool lit)
	{
		if (_lit != lit)
		{
			_lit = lit;
			UpdatePrompt();
			if (_lit)
			{
				_lightController.FadeTo(1f, 1f);
				_flameParticles.Play();
				OnTorchLit.Invoke(this);
			}
			else
			{
				_lightController.FadeTo(0f, 0.2f);
				_flameParticles.Stop();
				Locator.GetPlayerAudioController().PlayMarshmallowBlowOut();
				OnTorchUnlit.Invoke(this);
			}
		}
	}

	private void UpdatePrompt()
	{
		_interactReceiver.SetPromptText(_lit ? UITextType.RoastingExtinguishPrompt : UITextType.LightCampfirePrompt);
	}

	private void OnPressInteract()
	{
		SetLit(!_lit);
		_interactReceiver.ResetInteraction();
	}

	private void OnDetectLight()
	{
	}
}
