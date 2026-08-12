using UnityEngine;

public class ToolModeSwapper : MonoBehaviour
{
	private ToolGroup _currentToolGroup;

	private ToolMode _currentToolMode;

	private ToolMode _nextToolMode;

	private bool _isSwitchingToolMode;

	private PlayerTool _equippedTool;

	private PlayerTool _nextTool;

	private ProbeLauncher _probeLauncher;

	private Signalscope _signalscope;

	private NomaiTranslator _translator;

	private ItemTool _itemCarryTool;

	private ShipCockpitController _shipSystemsCtrlr;

	private FirstPersonManipulator _firstPersonManipulator;

	private NomaiAudioDetector _nomaiAudioDetector;

	private bool _waitForLoseNomaiTextFocus;

	private bool _shipDestroyed;

	private void Awake()
	{
		_probeLauncher = GetComponentInChildren<ProbeLauncher>();
		_signalscope = GetComponentInChildren<Signalscope>();
		_translator = GetComponentInChildren<NomaiTranslator>();
		_itemCarryTool = GetComponentInChildren<ItemTool>();
		GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.AddListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger.AddListener("ShipSystemFailure", OnShipSystemFailure);
	}

	private void Start()
	{
		OWRigidbody shipBody = Locator.GetShipBody();
		if (shipBody != null)
		{
			_shipSystemsCtrlr = shipBody.GetComponentInChildren<ShipCockpitController>();
		}
		_firstPersonManipulator = Locator.GetPlayerTransform().GetComponentInChildren<FirstPersonManipulator>();
		_nomaiAudioDetector = Locator.GetPlayerTransform().GetComponentInChildren<NomaiAudioDetector>();
	}

	private void OnDestroy()
	{
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.RemoveListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipSystemFailure);
	}

	public ToolGroup GetToolGroup()
	{
		return _currentToolGroup;
	}

	public ToolMode GetToolMode()
	{
		return _currentToolMode;
	}

	public bool IsInToolMode(ToolMode mode)
	{
		return mode == _currentToolMode;
	}

	public bool IsInToolMode(ToolMode mode, ToolGroup group)
	{
		if (IsInToolMode(mode))
		{
			return _currentToolGroup == group;
		}
		return false;
	}

	public ProbeLauncher GetProbeLauncher()
	{
		return _probeLauncher;
	}

	public Signalscope GetSignalScope()
	{
		return _signalscope;
	}

	public NomaiTranslator GetTranslator()
	{
		return _translator;
	}

	public ItemTool GetItemCarryTool()
	{
		return _itemCarryTool;
	}

	public bool IsNomaiTextInFocus()
	{
		if (OWInput.IsInputMode(InputMode.Character) && (_firstPersonManipulator.GetFocusedNomaiText() != null || _nomaiAudioDetector.GetFocusedNomaiAudioVolume() != null))
		{
			return !_firstPersonManipulator.HasFocusedInteractible();
		}
		return false;
	}

	private bool IsItemToolBlocked()
	{
		if (!_firstPersonManipulator.HasFocusedInteractible() && !(_firstPersonManipulator.GetFocusedNomaiText() != null))
		{
			return _nomaiAudioDetector.GetFocusedNomaiAudioVolume() != null;
		}
		return true;
	}

	public bool IsSuitPatchingBlocked()
	{
		if (!IsItemToolBlocked())
		{
			return _itemCarryTool.IsAnyPromptVisible();
		}
		return true;
	}

	public OWRigidbody GetFocusedTextParentBody()
	{
		NomaiText focusedNomaiText = _firstPersonManipulator.GetFocusedNomaiText();
		NomaiAudioVolume focusedNomaiAudioVolume = _nomaiAudioDetector.GetFocusedNomaiAudioVolume();
		if (focusedNomaiText != null)
		{
			return focusedNomaiText.gameObject.GetAttachedOWRigidbody();
		}
		if (focusedNomaiAudioVolume != null)
		{
			return focusedNomaiAudioVolume.gameObject.GetAttachedOWRigidbody();
		}
		return null;
	}

	public bool AllowTranslatorFactRevealNotification()
	{
		if (_currentToolMode == ToolMode.Translator)
		{
			return !IsNomaiTextInFocus();
		}
		return true;
	}

	public bool IsTranslatorEquipPromptAllowed()
	{
		if (IsNomaiTextInFocus() && _currentToolMode != ToolMode.Translator && (!GetAutoEquipTranslator() || _waitForLoseNomaiTextFocus))
		{
			return OWInput.IsInputMode(InputMode.Character);
		}
		return false;
	}

	private bool GetAutoEquipTranslator()
	{
		if (PlayerData.GetAutoEquipTranslator())
		{
			return _currentToolMode != ToolMode.Item;
		}
		return false;
	}

	private void Update()
	{
		if (_isSwitchingToolMode && !_equippedTool.IsEquipped())
		{
			_equippedTool = _nextTool;
			_nextTool = null;
			if (_equippedTool != null)
			{
				_equippedTool.EquipTool();
			}
			_currentToolMode = _nextToolMode;
			_nextToolMode = ToolMode.None;
			_isSwitchingToolMode = false;
		}
		InputMode inputMode = InputMode.Character | InputMode.ShipCockpit;
		if (!IsNomaiTextInFocus())
		{
			_waitForLoseNomaiTextFocus = false;
		}
		if (_shipDestroyed && _currentToolGroup == ToolGroup.Ship)
		{
			return;
		}
		if (_currentToolMode != 0 && _currentToolMode != ToolMode.Item && (OWInput.IsNewlyPressed(InputLibrary.cancel, inputMode | InputMode.ScopeZoom) || PlayerState.InConversation()))
		{
			InputLibrary.cancel.ConsumeInput();
			if (GetAutoEquipTranslator() && _currentToolMode == ToolMode.Translator)
			{
				_waitForLoseNomaiTextFocus = true;
			}
			UnequipTool();
		}
		else if (IsNomaiTextInFocus() && _currentToolMode != ToolMode.Translator && ((GetAutoEquipTranslator() && !_waitForLoseNomaiTextFocus) || OWInput.IsNewlyPressed(InputLibrary.interact, inputMode)))
		{
			EquipToolMode(ToolMode.Translator);
			if (_firstPersonManipulator.GetFocusedNomaiText() != null && _firstPersonManipulator.GetFocusedNomaiText().CheckTurnOffFlashlight())
			{
				Locator.GetFlashlight().TurnOff(playAudio: false);
			}
		}
		else if (_currentToolMode == ToolMode.Translator && !IsNomaiTextInFocus() && GetAutoEquipTranslator())
		{
			UnequipTool();
		}
		else if (OWInput.IsNewlyPressed(InputLibrary.probeLaunch, inputMode))
		{
			if (_currentToolGroup == ToolGroup.Suit && _itemCarryTool.GetHeldItemType() == ItemType.DreamLantern)
			{
				return;
			}
			if (((_currentToolMode == ToolMode.None || _currentToolMode == ToolMode.Item) && Locator.GetPlayerSuit().IsWearingSuit(includeTrainingSuit: false)) || ((_currentToolMode == ToolMode.None || _currentToolMode == ToolMode.SignalScope) && OWInput.IsInputMode(InputMode.ShipCockpit)))
			{
				EquipToolMode(ToolMode.Probe);
			}
		}
		else if (OWInput.IsNewlyPressed(InputLibrary.signalscope, inputMode | InputMode.ScopeZoom))
		{
			if (PlayerState.InDreamWorld())
			{
				return;
			}
			if (_currentToolMode == ToolMode.SignalScope)
			{
				UnequipTool();
			}
			else
			{
				EquipToolMode(ToolMode.SignalScope);
			}
		}
		bool flag = _itemCarryTool.UpdateInteract(_firstPersonManipulator, IsItemToolBlocked());
		if (!_itemCarryTool.IsEquipped() && flag)
		{
			EquipToolMode(ToolMode.Item);
		}
		else if (_itemCarryTool.GetHeldItem() != null && _currentToolMode == ToolMode.None && OWInput.IsInputMode(InputMode.Character) && !OWInput.IsChangePending())
		{
			EquipToolMode(ToolMode.Item);
		}
	}

	public void EquipToolMode(ToolMode mode)
	{
		PlayerTool playerTool = null;
		switch (mode)
		{
		case ToolMode.Probe:
			playerTool = ((_currentToolGroup == ToolGroup.Ship) ? _shipSystemsCtrlr.GetShipProbeLauncher() : _probeLauncher);
			break;
		case ToolMode.SignalScope:
			playerTool = _signalscope;
			break;
		case ToolMode.Translator:
			playerTool = ((_currentToolGroup == ToolGroup.Ship) ? null : _translator);
			break;
		case ToolMode.Item:
			playerTool = _itemCarryTool;
			break;
		}
		if (_equippedTool != playerTool)
		{
			if (_equippedTool != null)
			{
				_equippedTool.UnequipTool();
				_nextToolMode = mode;
				_nextTool = playerTool;
				_isSwitchingToolMode = true;
			}
			else
			{
				playerTool.EquipTool();
				_equippedTool = playerTool;
				_currentToolMode = mode;
				_nextToolMode = ToolMode.None;
			}
		}
	}

	public void UnequipTool()
	{
		EquipToolMode(ToolMode.None);
	}

	private void OnEnterFlightConsole(OWRigidbody shipBody)
	{
		_currentToolGroup = ToolGroup.Ship;
	}

	private void OnExitFlightConsole()
	{
		_currentToolGroup = ToolGroup.Suit;
	}

	private void OnShipSystemFailure()
	{
		_shipDestroyed = true;
		if (_currentToolGroup == ToolGroup.Ship)
		{
			UnequipTool();
		}
	}
}
