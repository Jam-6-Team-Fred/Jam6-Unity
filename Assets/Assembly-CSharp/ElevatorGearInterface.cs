using UnityEngine;

public class ElevatorGearInterface : AbstractGhostElevatorInterface
{
	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private GearInterfaceEffects _gearEffects;

	private bool _goingDown;

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
		_goingDown = isUp;
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
		CallOnActivateEvent();
		if (_goingDown)
		{
			if (_gearEffects != null)
			{
				_gearEffects.AddRotation(90f);
			}
		}
		else if (_gearEffects != null)
		{
			_gearEffects.AddRotation(-90f);
		}
		_goingDown = !_goingDown;
	}

	public override void OnActionComplete()
	{
		_interactReceiver.ResetInteraction();
	}
}
