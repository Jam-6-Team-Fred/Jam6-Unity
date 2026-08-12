using UnityEngine;

public class EscapePodSignalSwitch : MonoBehaviour
{
	[SerializeField]
	private NomaiInterfaceSlot _onSlot;

	private AudioSignal _audioSignal;

	private void Awake()
	{
		_audioSignal = GetComponent<AudioSignal>();
		if (_onSlot != null)
		{
			_onSlot.OnSlotActivated += OnSlotActivated;
			_onSlot.OnSlotDeactivated += OnSlotDeactivated;
		}
	}

	private void OnDestroy()
	{
		if (_onSlot != null)
		{
			_onSlot.OnSlotActivated -= OnSlotActivated;
			_onSlot.OnSlotDeactivated -= OnSlotDeactivated;
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		_audioSignal.SetSignalActivation(active: true);
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		_audioSignal.SetSignalActivation(active: false);
	}
}
