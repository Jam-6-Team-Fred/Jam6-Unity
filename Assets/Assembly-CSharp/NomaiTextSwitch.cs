using UnityEngine;

public class NomaiTextSwitch : MonoBehaviour
{
	[SerializeField]
	private NomaiInterfaceSlot _onSlot;

	[SerializeField]
	private NomaiInterfaceSlot _offSlot;

	[SerializeField]
	private NomaiWallText[] _wallText;

	private void Awake()
	{
		_onSlot.OnSlotActivated += OnPowerOn;
		_offSlot.OnSlotActivated += OnPowerOff;
		for (int i = 0; i < _wallText.Length; i++)
		{
			_wallText[i].HideTextOnStart();
		}
	}

	private void OnDestroy()
	{
		_onSlot.OnSlotActivated -= OnPowerOn;
		_offSlot.OnSlotActivated -= OnPowerOff;
	}

	private void OnPowerOn(NomaiInterfaceSlot slot)
	{
		for (int i = 0; i < _wallText.Length; i++)
		{
			_wallText[i].Show();
		}
	}

	private void OnPowerOff(NomaiInterfaceSlot slot)
	{
		for (int i = 0; i < _wallText.Length; i++)
		{
			_wallText[i].Hide();
		}
	}
}
