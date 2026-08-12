using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyRebindingDisplayElement : MonoBehaviour
{
	[SerializeField]
	private Text _label;

	[Space(10f)]
	[SerializeField]
	private GameObject _gamepadBindingImage1Obj;

	[SerializeField]
	private GameObject _gamepadBindingImage2Obj;

	[SerializeField]
	private Image _gamepadBindingImage1;

	[SerializeField]
	private Image _gamepadBindingImage2;

	[Space(10f)]
	[SerializeField]
	private GameObject _keyboardMouseBindingBlockObj;

	[SerializeField]
	private GameObject _keyboardMouseBindingImage1Obj;

	[SerializeField]
	private GameObject _keyboardMouseBindingImage2Obj;

	[SerializeField]
	private Image _keyboardMouseBindingImage1;

	[SerializeField]
	private Image _keyboardMouseBindingImage2;

	private IInputCommands _inputCommand;

	private Texture2D[] _gamepadBindingTextures;

	private Texture2D[] _keyboardMouseBindingTextures;

	public void Initialize(IInputCommands inputCommand)
	{
		_keyboardMouseBindingTextures = new Texture2D[2];
		_gamepadBindingTextures = new Texture2D[2];
		SetInputCommand(inputCommand);
	}

	public void SetInputCommand(IInputCommands command)
	{
		_label.text = InputTransitionUtil.GetLabel(_inputCommand.CommandType);
		_inputCommand = command;
		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		if (_inputCommand != null)
		{
			SetImageList(_inputCommand);
			if (_gamepadBindingTextures[0] != null)
			{
				_gamepadBindingImage1.sprite = Sprite.Create(_gamepadBindingTextures[0], new Rect(0f, 0f, _gamepadBindingTextures[0].width, _gamepadBindingTextures[0].height), new Vector2(0.5f, 0.5f), _gamepadBindingTextures[0].width);
				_gamepadBindingImage1.enabled = true;
			}
			else
			{
				_gamepadBindingImage1.enabled = false;
			}
			if (_gamepadBindingTextures[1] != null)
			{
				_gamepadBindingImage2Obj.SetActive(value: true);
				_gamepadBindingImage2.sprite = Sprite.Create(_gamepadBindingTextures[1], new Rect(0f, 0f, _gamepadBindingTextures[1].width, _gamepadBindingTextures[1].height), new Vector2(0.5f, 0.5f), _gamepadBindingTextures[1].width);
			}
			else
			{
				_gamepadBindingImage2Obj.SetActive(value: false);
			}
			if (_keyboardMouseBindingTextures[0] != null)
			{
				_keyboardMouseBindingImage1.sprite = Sprite.Create(_keyboardMouseBindingTextures[0], new Rect(0f, 0f, _keyboardMouseBindingTextures[0].width, _keyboardMouseBindingTextures[0].height), new Vector2(0.5f, 0.5f), _keyboardMouseBindingTextures[0].width);
				_keyboardMouseBindingImage1.enabled = true;
			}
			else
			{
				_keyboardMouseBindingImage1.enabled = false;
			}
			if (_keyboardMouseBindingTextures[1] != null)
			{
				_keyboardMouseBindingImage2Obj.SetActive(value: true);
				_keyboardMouseBindingImage2.sprite = Sprite.Create(_keyboardMouseBindingTextures[1], new Rect(0f, 0f, _keyboardMouseBindingTextures[1].width, _keyboardMouseBindingTextures[1].height), new Vector2(0.5f, 0.5f), _keyboardMouseBindingTextures[1].width);
			}
			else
			{
				_keyboardMouseBindingImage2Obj.SetActive(value: false);
			}
		}
	}

	protected void SetImageList(IInputCommands inputCommand)
	{
		int num = 0;
		for (int i = 0; i < _gamepadBindingTextures.Length; i++)
		{
			_gamepadBindingTextures[i] = null;
		}
		AxisIdentifier axisID = inputCommand.AxisID;
		if (axisID != 0)
		{
			List<Texture2D> uITextures = inputCommand.GetUITextures(gamepad: true);
			for (int j = 0; j < uITextures.Count; j++)
			{
				_gamepadBindingTextures[num] = uITextures[j];
				num++;
			}
		}
		else
		{
			List<Texture2D> uITextures2 = inputCommand.GetUITextures(gamepad: true);
			for (int k = 0; k < uITextures2.Count; k++)
			{
				_gamepadBindingTextures[num] = uITextures2[k];
				num++;
			}
		}
		num = 0;
		for (int l = 0; l < _keyboardMouseBindingTextures.Length; l++)
		{
			_keyboardMouseBindingTextures[l] = null;
		}
		if (axisID != 0)
		{
			List<Texture2D> uITextures3 = inputCommand.GetUITextures(gamepad: false);
			for (int m = 0; m < uITextures3.Count; m++)
			{
				_keyboardMouseBindingTextures[num] = uITextures3[m];
				num++;
			}
		}
		else
		{
			List<Texture2D> uITextures4 = inputCommand.GetUITextures(gamepad: false);
			for (int n = 0; n < uITextures4.Count; n++)
			{
				_keyboardMouseBindingTextures[num] = uITextures4[n];
				num++;
			}
		}
	}

	private bool TryGetButtonTexture(JoystickButton button, KeyCode key, out Texture2D texture)
	{
		if (button != 0)
		{
			if (button == JoystickButton.Custom)
			{
				texture = ButtonPromptLibrary.SharedInstance.GetButtonTexture(key);
			}
			else
			{
				texture = ButtonPromptLibrary.SharedInstance.GetButtonTexture(button);
			}
			return true;
		}
		if (key != 0)
		{
			texture = ButtonPromptLibrary.SharedInstance.GetButtonTexture(key);
			return true;
		}
		texture = null;
		return false;
	}
}
