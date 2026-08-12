using UnityEngine;

public class NomaiPowerSwitch : MonoBehaviour
{
	[SerializeField]
	private NomaiInterfaceSlot _powerSourceSlot;

	[Space]
	[SerializeField]
	private bool _startOn;

	[SerializeField]
	private NomaiInterfaceSlot _slot;

	[SerializeField]
	private float _fadeDuration = 3f;

	[SerializeField]
	private NomaiLamp[] _onLamps;

	[SerializeField]
	private OWLightController[] _onLightControllers;

	[SerializeField]
	private TractorBeamController[] _onTractorBeams;

	[SerializeField]
	private NomaiLamp[] _offLamps;

	[SerializeField]
	private TractorBeamController[] _offTractorBeams;

	[SerializeField]
	private NomaiEnergyCable _energyCable;

	[Space]
	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private AudioType _onClip;

	[SerializeField]
	private AudioType _offClip;

	private bool _powerOn;

	private bool _receivingPower = true;

	private void Awake()
	{
		_powerOn = _startOn;
		_slot.OnSlotActivated += OnSlotActivated;
		_slot.OnSlotDeactivated += OnSlotDeactivated;
		if (_powerSourceSlot != null)
		{
			_powerSourceSlot.OnSlotActivated += OnPowerSourceActivated;
			_powerSourceSlot.OnSlotDeactivated += OnPowerSourceDeactivated;
		}
	}

	private void Start()
	{
		for (int i = 0; i < _onLightControllers.Length; i++)
		{
			_onLightControllers[i].SetIntensity(_startOn ? 1f : 0f);
		}
	}

	private void OnDestroy()
	{
		_slot.OnSlotActivated -= OnSlotActivated;
		_slot.OnSlotDeactivated -= OnSlotDeactivated;
		if (_powerSourceSlot != null)
		{
			_powerSourceSlot.OnSlotActivated -= OnPowerSourceActivated;
			_powerSourceSlot.OnSlotDeactivated -= OnPowerSourceDeactivated;
		}
	}

	public void PowerOn()
	{
		if (_receivingPower && !_powerOn)
		{
			_powerOn = true;
			for (int i = 0; i < _onLamps.Length; i++)
			{
				_onLamps[i].FadeTo(1f, _fadeDuration);
			}
			for (int j = 0; j < _offLamps.Length; j++)
			{
				_offLamps[j].FadeTo(0f, _fadeDuration);
			}
			for (int k = 0; k < _onLightControllers.Length; k++)
			{
				_onLightControllers[k].FadeTo(1f, _fadeDuration);
			}
			for (int l = 0; l < _onTractorBeams.Length; l++)
			{
				_onTractorBeams[l].SetActivation(active: true);
			}
			for (int m = 0; m < _offTractorBeams.Length; m++)
			{
				_offTractorBeams[m].SetActivation(active: false);
			}
			if (_energyCable != null)
			{
				_energyCable.SetPowered(powered: true);
			}
			if (_audioSource != null)
			{
				_audioSource.PlayOneShot(_onClip);
			}
		}
	}

	public void PowerOff()
	{
		if (_powerOn)
		{
			_powerOn = false;
			for (int i = 0; i < _onLamps.Length; i++)
			{
				_onLamps[i].FadeTo(0f, _fadeDuration);
			}
			for (int j = 0; j < _onLightControllers.Length; j++)
			{
				_onLightControllers[j].FadeTo(0f, _fadeDuration);
			}
			for (int k = 0; k < _onTractorBeams.Length; k++)
			{
				_onTractorBeams[k].SetActivation(active: false);
			}
			if (_energyCable != null)
			{
				_energyCable.SetPowered(powered: false);
			}
			if (_audioSource != null)
			{
				_audioSource.PlayOneShot(_offClip);
			}
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		PowerOn();
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		PowerOff();
	}

	private void OnPowerSourceActivated(NomaiInterfaceSlot slot)
	{
		_receivingPower = true;
	}

	private void OnPowerSourceDeactivated(NomaiInterfaceSlot slot)
	{
		_receivingPower = false;
		PowerOff();
	}
}
