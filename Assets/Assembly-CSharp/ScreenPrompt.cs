using System.Collections.Generic;
using UnityEngine;

public class ScreenPrompt
{
	public enum MultiCommandType
	{
		NONE = 0,
		CUSTOM_BOTH = 1,
		HOLD_BOTH = 2,
		HOLD_ONE_AND_PRESS_2ND = 3,
		POS_NEG = 4,
		WASD = 5
	}

	public enum DisplayState
	{
		Normal = 0,
		Attention = 1,
		GrayedOut = 2
	}

	private List<IInputCommands> _commandList = new List<IInputCommands>();

	private List<InputConsts.InputCommandType> _commandIdList = new List<InputConsts.InputCommandType>();

	private MultiCommandType _multiCommandType;

	private int _priority;

	private string _text;

	private Color _textColor;

	private bool _isVisible = true;

	private DisplayState _displayState;

	private bool _hasVisibilityController;

	private bool _scrollOnVisible = true;

	private bool _isInteractPrompt;

	private Sprite _customSprite;

	public ScreenPrompt(string prompt, int priority = 0)
	{
		Init(prompt, priority, MultiCommandType.NONE, DisplayState.Normal, overridePriority: false);
	}

	public ScreenPrompt(string prompt, Sprite customSprite, int priority = 0)
	{
		Init(prompt, priority, MultiCommandType.NONE, DisplayState.Normal, overridePriority: false, customSprite);
	}

	public ScreenPrompt(IInputCommands cmd, string prompt, int priority = 0, DisplayState displayState = DisplayState.Normal, bool overridePriority = false)
	{
		_isInteractPrompt = cmd.CommandType == InputConsts.InputCommandType.INTERACT || cmd.CommandType == InputConsts.InputCommandType.INTERACT_SECONDARY;
		_commandIdList = new List<InputConsts.InputCommandType>();
		_commandIdList.Add(cmd.CommandType);
		Init(prompt, priority, MultiCommandType.NONE, displayState, overridePriority);
	}

	public ScreenPrompt(IInputCommands cmd, IInputCommands cmd2, string prompt, MultiCommandType multiCmdType, int priority = 0, DisplayState displayState = DisplayState.Normal, bool overridePriority = false)
	{
		_commandIdList = new List<InputConsts.InputCommandType>();
		_commandIdList.Add(cmd.CommandType);
		_commandIdList.Add(cmd2.CommandType);
		Init(prompt, priority, multiCmdType, displayState, overridePriority);
	}

	public ScreenPrompt(List<IInputCommands> cmdList, string prompt, MultiCommandType multiCmdType, int priority = 0, DisplayState displayState = DisplayState.Normal, bool overridePriority = false)
	{
		_commandIdList = new List<InputConsts.InputCommandType>();
		for (int i = 0; i < cmdList.Count; i++)
		{
			_commandIdList.Add(cmdList[i].CommandType);
		}
		Init(prompt, priority, multiCmdType, displayState, overridePriority);
	}

	~ScreenPrompt()
	{
	}

	private void Init(string prompt, int priority, MultiCommandType multiCmdType, DisplayState displayState, bool overridePriority, Sprite customSprite = null)
	{
		_textColor = GetDefaultColor();
		_priority = priority;
		_customSprite = customSprite;
		_text = prompt;
		_multiCommandType = multiCmdType;
		_displayState = displayState;
	}

	public Color GetDefaultColor()
	{
		return Color.white;
	}

	public int GetPriority()
	{
		return _priority;
	}

	public MultiCommandType GetMultiCommandType()
	{
		return _multiCommandType;
	}

	private void RefreshCommandList()
	{
		_commandList.Clear();
		for (int i = 0; i < _commandIdList.Count; i++)
		{
			if (InputCommandManager.MappedInputActions.TryGetValue(_commandIdList[i], out var value))
			{
				_commandList.Add(value);
			}
		}
	}

	public List<IInputCommands> GetInputCommandList()
	{
		RefreshCommandList();
		return _commandList;
	}

	public void SetVisibilityController(IPromptVisibilityController visibilityController)
	{
		_hasVisibilityController = true;
		visibilityController.OnPromptVisibilityChange += VisibilityControllerSetVisibility;
	}

	private void VisibilityControllerSetVisibility(ScreenPrompt prompt, bool visible)
	{
		if (_isVisible != visible && prompt == this)
		{
			_isVisible = visible;
			Locator.GetPromptManager().TriggerVisibilityRefresh(this);
		}
	}

	public void SetVisibility(bool isVisible)
	{
		if (_hasVisibilityController)
		{
			Debug.LogWarning("This ScreenPrompt already has a VisibilityController that controls Visibility");
		}
		if (_isVisible != isVisible)
		{
			_isVisible = isVisible;
			Locator.GetPromptManager().TriggerVisibilityRefresh(this);
		}
	}

	public bool IsVisible()
	{
		if (_isVisible && (PlayerData.GetPromptsEnabled() || _isInteractPrompt || _commandList.Count == 0))
		{
			return GUIMode.AreScreenPromptsVisible();
		}
		return false;
	}

	public void SetText(string text)
	{
		if (_text != text)
		{
			Locator.GetPromptManager().UpdateText(this, text);
			_text = text;
		}
	}

	public string GetText()
	{
		return _text;
	}

	public void SetTextColor(Color textColor)
	{
		if (_textColor != textColor)
		{
			_textColor = textColor;
			Locator.GetPromptManager().TriggerRefresh(this);
		}
	}

	public Color GetTextColor()
	{
		return _textColor;
	}

	public List<Sprite> GetSpriteList(bool forceRefresh = false)
	{
		if (_customSprite != null)
		{
			return new List<Sprite> { _customSprite };
		}
		List<Texture2D> textureList = new List<Texture2D>();
		RefreshCommandList();
		for (int i = 0; i < _commandList.Count; i++)
		{
			List<Texture2D> uITextures = _commandList[i].GetUITextures(OWInput.UsingGamepad(), forceRefresh);
			if (uITextures != null)
			{
				for (int j = 0; j < uITextures.Count; j++)
				{
					textureList.Add(uITextures[j]);
				}
			}
		}
		ButtonPromptLibrary.SharedInstance.RefineUITextureListForDisplay(in textureList);
		List<Sprite> list = new List<Sprite>();
		for (int k = 0; k < textureList.Count; k++)
		{
			Texture2D texture2D = textureList[k];
			Sprite item = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect, Vector4.zero, generateFallbackPhysicsShape: false);
			list.Add(item);
		}
		return list;
	}

	public void SetDisplayState(DisplayState state)
	{
		if (_displayState != state)
		{
			_displayState = state;
			Locator.GetPromptManager().TriggerRefresh(this);
		}
	}

	public bool IsDisplayState(DisplayState state)
	{
		return _displayState == state;
	}

	public bool GetScrollsOnVisble()
	{
		return _scrollOnVisible;
	}

	public void SetScrollOnVisible(bool value)
	{
		_scrollOnVisible = value;
	}
}
