using UnityEngine;

public class NomaiLampSwitch : MonoBehaviour
{
	[SerializeField]
	private bool _startOn;

	[SerializeField]
	private float _fadeDuration = 3f;

	[SerializeField]
	private GameObject _lampRoot;

	[SerializeField]
	private TractorBeamController[] _tractorBeams = new TractorBeamController[0];

	private OWLightController[] _lightControllers = new OWLightController[0];

	private NomaiLamp[] _lamps = new NomaiLamp[0];

	[SerializeField]
	private NomaiInterfaceSlot[] _onSlots;

	[SerializeField]
	private NomaiInterfaceSlot[] _offSlots;

	[SerializeField]
	private NomaiInterfaceSlot[] _toggleSlots;

	private bool _powerOn;

	private void Awake()
	{
		_powerOn = _startOn;
		_lamps = _lampRoot.GetComponentsInChildren<NomaiLamp>();
		_lightControllers = _lampRoot.GetComponentsInChildren<OWLightController>();
		for (int i = 0; i < _onSlots.Length; i++)
		{
			_onSlots[i].OnSlotActivated += OnPowerOn;
		}
		for (int j = 0; j < _offSlots.Length; j++)
		{
			_offSlots[j].OnSlotActivated += OnPowerOff;
		}
		for (int k = 0; k < _toggleSlots.Length; k++)
		{
			_toggleSlots[k].OnSlotActivated += OnPowerToggle;
		}
		SetPower(_powerOn, immediate: true);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _onSlots.Length; i++)
		{
			_onSlots[i].OnSlotActivated -= OnPowerOn;
		}
		for (int j = 0; j < _offSlots.Length; j++)
		{
			_offSlots[j].OnSlotActivated -= OnPowerOff;
		}
		for (int k = 0; k < _toggleSlots.Length; k++)
		{
			_toggleSlots[k].OnSlotActivated -= OnPowerToggle;
		}
	}

	private void SetPower(bool on, bool immediate = false)
	{
		_powerOn = on;
		for (int i = 0; i < _lightControllers.Length; i++)
		{
			_lightControllers[i].FadeTo(on ? 1f : 0f, immediate ? 0f : _fadeDuration);
		}
		for (int j = 0; j < _lamps.Length; j++)
		{
			_lamps[j].FadeTo(on ? 1f : 0f, immediate ? 0f : _fadeDuration);
		}
		for (int k = 0; k < _tractorBeams.Length; k++)
		{
			_tractorBeams[k].SetActivation(on);
		}
	}

	private void OnPowerOn(NomaiInterfaceSlot slot)
	{
		SetPower(on: true);
	}

	private void OnPowerOff(NomaiInterfaceSlot slot)
	{
		SetPower(on: false);
	}

	private void OnPowerToggle(NomaiInterfaceSlot slot)
	{
		SetPower(!_powerOn);
	}
}
