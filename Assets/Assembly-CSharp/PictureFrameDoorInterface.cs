using UnityEngine;

public class PictureFrameDoorInterface : MonoBehaviour
{
	[SerializeField]
	private InteractReceiver[] _interactReceivers;

	[SerializeField]
	protected RotatingDoor _door;

	[SerializeField]
	private string[] _revealFactIDs = new string[0];

	protected virtual void Awake()
	{
		if (_revealFactIDs.Length != 0)
		{
			_door.OnOpenFinish += new OWEvent.OWCallback(OnOpenFinish);
		}
		for (int i = 0; i < _interactReceivers.Length; i++)
		{
			_interactReceivers[i].OnPressInteract += OnPressInteract;
		}
	}

	protected virtual void Start()
	{
		UpdatePrompt();
	}

	protected virtual void OnDestroy()
	{
		if (_revealFactIDs.Length != 0)
		{
			_door.OnOpenFinish -= new OWEvent.OWCallback(OnOpenFinish);
		}
		for (int i = 0; i < _interactReceivers.Length; i++)
		{
			_interactReceivers[i].OnPressInteract -= OnPressInteract;
		}
	}

	protected virtual void ToggleOpenState()
	{
		if (_door.IsOpen())
		{
			_door.Close();
		}
		else
		{
			_door.Open();
		}
		UpdatePrompt();
	}

	private void OnPressInteract()
	{
		ToggleOpenState();
	}

	private void UpdatePrompt()
	{
		for (int i = 0; i < _interactReceivers.Length; i++)
		{
			_interactReceivers[i].SetPromptText(_door.IsOpen() ? UITextType.ClosePrompt : UITextType.OpenPrompt);
			_interactReceivers[i].ResetInteraction();
		}
	}

	private void OnOpenFinish()
	{
		for (int i = 0; i < _revealFactIDs.Length; i++)
		{
			Locator.GetShipLogManager().RevealFact(_revealFactIDs[i]);
		}
		_door.OnOpenFinish -= new OWEvent.OWCallback(OnOpenFinish);
	}
}
