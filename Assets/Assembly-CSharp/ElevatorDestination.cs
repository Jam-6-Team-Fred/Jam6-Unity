using UnityEngine;

public class ElevatorDestination : MonoBehaviour
{
	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private GearInterfaceEffects _gearInterface;

	private int _floorIndex;

	private CageElevator _elevator;

	public OWEvent<int> OnElevatorCalled = new OWEvent<int>(1);

	public bool hasControls => _interactReceiver != null;

	private void Start()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract += OnPressInteract;
			_interactReceiver.SetPromptText(UITextType.RotateGearPrompt);
		}
	}

	private void OnDestroy()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract -= OnPressInteract;
		}
	}

	public void SetElevator(CageElevator elev)
	{
		_elevator = elev;
	}

	public void SetFloorIndex(int inIndex)
	{
		_floorIndex = inIndex;
	}

	private void OnPressInteract()
	{
		if (_gearInterface != null)
		{
			if (_elevator.currentLevelIdx != _floorIndex || _elevator.isMoving)
			{
				_gearInterface.AddRotation(90f);
			}
			else
			{
				_gearInterface.PlayFailure();
			}
		}
		OnElevatorCalled.Invoke(_floorIndex);
		_interactReceiver.ResetInteraction();
	}
}
