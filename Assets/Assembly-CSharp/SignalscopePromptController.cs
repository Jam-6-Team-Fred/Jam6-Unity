using UnityEngine;

public class SignalscopePromptController : MonoBehaviour, ILateInitializer
{
	private bool _screenPromptsInitialized;

	private ScreenPrompt _unequipPrompt;

	private ScreenPrompt _zoomLevelPrompt;

	private ScreenPrompt _zoomModePrompt;

	private ScreenPrompt _changeFrequencyPrompt;

	private ScreenPrompt _centerZoomModePrompt;

	private bool _showCenterZoomPrompt;

	private bool _hasZoomedIn;

	private void Awake()
	{
		_screenPromptsInitialized = false;
		LateInitializerManager.RegisterLateInitializer(this);
		_unequipPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.SignalscopeExitPrompt) + "   <CMD>");
		_zoomModePrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.SignalscopeZoomInPrompt) + "   <CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt));
		_centerZoomModePrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.SignalscopeZoomInPrompt) + "   <CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt));
		_zoomLevelPrompt = new ScreenPrompt(InputLibrary.toolOptionUp, InputLibrary.toolOptionDown, UITextLibrary.GetString(UITextType.SignalscopeZoomLevelPrompt) + "   <CMD1> <CMD2>", ScreenPrompt.MultiCommandType.CUSTOM_BOTH);
		_changeFrequencyPrompt = new ScreenPrompt(InputLibrary.toolOptionLeft, InputLibrary.toolOptionRight, UITextLibrary.GetString(UITextType.SignalscopeFrequencyPrompt) + "   <CMD1> <CMD2>", ScreenPrompt.MultiCommandType.CUSTOM_BOTH);
		GlobalMessenger<Signalscope>.AddListener("EquipSignalscope", OnEquipSignalscope);
		GlobalMessenger.AddListener("UnequipSignalscope", OnUnequipSignalscope);
		GlobalMessenger<bool>.AddListener("EnterSignalscopePromptTrigger", OnEnterSignalscopePromptTrigger);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (!_screenPromptsInitialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
		GlobalMessenger<Signalscope>.RemoveListener("EquipSignalscope", OnEquipSignalscope);
		GlobalMessenger.RemoveListener("UnequipSignalscope", OnUnequipSignalscope);
		GlobalMessenger<bool>.RemoveListener("EnterSignalscopePromptTrigger", OnEnterSignalscopePromptTrigger);
	}

	public void LateInitialize()
	{
		_screenPromptsInitialized = true;
		Locator.GetPromptManager().AddScreenPrompt(_unequipPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_changeFrequencyPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_zoomModePrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_zoomLevelPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_centerZoomModePrompt, PromptPosition.Center);
	}

	private void OnEquipSignalscope(Signalscope signalscope)
	{
		base.enabled = true;
	}

	private void OnUnequipSignalscope()
	{
		base.enabled = false;
		HideAllPrompts();
	}

	private void Update()
	{
		HideAllPrompts();
		if (OWInput.IsInputMode(InputMode.Character | InputMode.ShipCockpit))
		{
			_changeFrequencyPrompt.SetVisibility(Locator.GetToolModeSwapper().GetSignalScope().AllowChangeFrequency());
			bool visibility = !PlayerState.AtFlightConsole();
			if (_showCenterZoomPrompt && !_hasZoomedIn)
			{
				_centerZoomModePrompt.SetVisibility(visibility);
			}
			else
			{
				_zoomModePrompt.SetVisibility(visibility);
			}
			_unequipPrompt.SetVisibility(isVisible: true);
		}
		else if (OWInput.IsInputMode(InputMode.ScopeZoom))
		{
			_zoomLevelPrompt.SetVisibility(isVisible: true);
			if (_showCenterZoomPrompt)
			{
				_hasZoomedIn = true;
			}
		}
	}

	private void HideAllPrompts()
	{
		_unequipPrompt.SetVisibility(isVisible: false);
		_zoomModePrompt.SetVisibility(isVisible: false);
		_centerZoomModePrompt.SetVisibility(isVisible: false);
		_zoomLevelPrompt.SetVisibility(isVisible: false);
		_changeFrequencyPrompt.SetVisibility(isVisible: false);
	}

	private void OnEnterSignalscopePromptTrigger(bool centerZoomPrompt)
	{
		_showCenterZoomPrompt = centerZoomPrompt;
		_hasZoomedIn = false;
	}

	private void OnExitSignalscopePromptTrigger()
	{
		_showCenterZoomPrompt = false;
	}
}
