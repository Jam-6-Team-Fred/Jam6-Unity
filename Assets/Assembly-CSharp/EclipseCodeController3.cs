using UnityEngine;

public class EclipseCodeController3 : SectoredMonoBehaviour
{
	[SerializeField]
	private MultiInteractReceiver _interactReceiver;

	[SerializeField]
	private GearInterfaceEffects _gearInterface;

	[SerializeField]
	private RotaryDial[] _dials;

	[Space]
	[SerializeField]
	private SingleLightSensor _lightSensor;

	[SerializeField]
	private Transform _lightSensorRoot;

	[Space]
	[SerializeField]
	private int[] _code;

	[Space]
	[SerializeField]
	private AbstractDoor _frontDoor;

	private Material _origMaterial;

	private int _goUpCommandIndex;

	private int _goDownCommandIndex;

	private int _currentFromIdx;

	public bool isAtBottom => _currentFromIdx <= 0;

	public bool isAtTop => _currentFromIdx >= _dials.Length - 1;

	protected override void Awake()
	{
		base.Awake();
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		if (_dials.Length != _code.Length)
		{
			Debug.LogError("The code length does not match the number of dials.");
		}
	}

	private void Start()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract += OnPressInteract;
			_goUpCommandIndex = _interactReceiver.AddInteraction(InputLibrary.interact, InputMode.Character, UITextType.RotateGearUpPrompt, !isAtTop, displayCommandIcon: true);
			_goDownCommandIndex = _interactReceiver.AddInteraction(InputLibrary.interactSecondary, InputMode.Character, UITextType.RotateGearDownPrompt, !isAtBottom, displayCommandIcon: true);
		}
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_interactReceiver.OnPressInteract -= OnPressInteract;
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void OnPressInteract(IInputCommands command)
	{
		if (command == _interactReceiver.GetInteractionAt(_goUpCommandIndex).inputCommand)
		{
			if (_gearInterface != null)
			{
				_gearInterface.AddRotation(-90f);
				GoUp();
			}
		}
		else if (command == _interactReceiver.GetInteractionAt(_goDownCommandIndex).inputCommand && _gearInterface != null)
		{
			_gearInterface.AddRotation(90f);
			GoDown();
		}
	}

	private void GoUp()
	{
		_currentFromIdx = (_currentFromIdx + _dials.Length - 1) % _dials.Length;
		_lightSensorRoot.position = _dials[_currentFromIdx].GetCenterTransform().position;
	}

	private void GoDown()
	{
		_currentFromIdx = (_currentFromIdx + 1) % _dials.Length;
		_lightSensorRoot.position = _dials[_currentFromIdx].GetCenterTransform().position;
	}

	private void OnDetectLight()
	{
		_dials[_currentFromIdx].StartRotation();
	}

	private void OnDetectDarkness()
	{
		_dials[_currentFromIdx].StopRotation();
		GoDown();
		CheckForCode();
	}

	private void CheckForCode()
	{
		bool flag = true;
		for (int i = 0; i < _dials.Length; i++)
		{
			flag = flag && _dials[i].GetSymbolSelected() == _code[i];
		}
		if (flag && _frontDoor != null)
		{
			_frontDoor.Open();
		}
	}
}
