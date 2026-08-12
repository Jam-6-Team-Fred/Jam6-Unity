using UnityEngine;

public class GearInterface : AbstractGhostDoorInterface
{
	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private GearInterfaceEffects _gearEffects;

	private void Start()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract += OnPressInteract;
			_interactReceiver.SetPromptText(UITextType.RotateGearPrompt);
		}
	}

	public override void SetStartingPosition(bool isUp)
	{
	}

	private void OnDestroy()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract -= OnPressInteract;
		}
	}

	private void OnPressInteract()
	{
		_interactReceiver.ResetInteraction();
		CallOpenEvent();
		if (_gearEffects != null)
		{
			_gearEffects.AddRotation(90f);
		}
	}
}
