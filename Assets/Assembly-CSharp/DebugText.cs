using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
	private static Text _textComponent;

	private void Awake()
	{
		if (_textComponent == null)
		{
			_textComponent = GetComponent<Text>();
			_textComponent.text = string.Empty;
		}
		else
		{
			Debug.LogError("Cannot have multiple DebugText");
		}
	}

	public static string GetText()
	{
		if (_textComponent == null)
		{
			Debug.LogError("DebugText not yet initialized");
			return string.Empty;
		}
		return _textComponent.text;
	}

	public static void SetText(string s)
	{
		if (_textComponent == null)
		{
			Debug.LogError("DebugText not yet initialized");
		}
		else
		{
			_textComponent.text = s;
		}
	}

	public static void AppendText(string s, bool newLine)
	{
		if (_textComponent == null)
		{
			Debug.LogError("DebugText not yet initialized");
		}
		else if (newLine)
		{
			if (_textComponent.text == string.Empty)
			{
				_textComponent.text = s;
			}
			else
			{
				_textComponent.text = _textComponent.text + "\n" + s;
			}
		}
		else
		{
			_textComponent.text += s;
		}
	}
}
