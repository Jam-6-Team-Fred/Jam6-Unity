using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TypeEffectText : MonoBehaviour
{
	protected struct TextInfo
	{
		public string text;

		public bool isTag;

		public float pauseTime;

		public int tagLocation;
	}

	public delegate void TypingTextCompleteEvent();

	public const float c_defaultPauseTime = 1f;

	protected Text _textComponent;

	protected TextAnchor _textAnchor;

	protected string _strToDisplay;

	protected List<TextInfo> _toDisplayStringParts;

	protected string[] _currentlyDisplayingStrings;

	protected int _totalNumVisibleChar;

	private int _iCurrentlyVisibleChar;

	protected bool _bTypeFromRightSide;

	protected float _typingTime;

	protected float _typingStartTime;

	protected bool _isHolding;

	protected float _holdStartTime;

	protected float _holdDuration;

	protected float _totalHoldTime;

	private StringBuilder _updateStringBuilder;

	protected float _percentageVisibleText = 1f;

	protected OWAudioSource _audioSource;

	protected bool _effectInProgress;

	protected bool _setDisabledInNextFrame;

	protected LayoutGroup _parentLayoutGroup;

	protected bool _linkedToWillRenderCanvas;

	public event TypingTextCompleteEvent OnTypingComplete;

	private void OnDestroy()
	{
		EnableOnWillRenderCanvases(value: false);
	}

	public virtual void Init(string displayString, TextAnchor alignment, TextSpeed overrideTextSpeed)
	{
		EnableOnWillRenderCanvases(value: false);
		_toDisplayStringParts = new List<TextInfo>();
		_textComponent = this.GetRequiredComponent<Text>();
		_textComponent.text = string.Empty;
		SetTextAlignment(_textComponent.alignment);
		_iCurrentlyVisibleChar = 0;
		_percentageVisibleText = 0f;
		SetTextAlignment(alignment);
		SetDisplayString(displayString);
		SetTypingTime(overrideTextSpeed);
		_effectInProgress = false;
		_updateStringBuilder = new StringBuilder();
		_parentLayoutGroup = GetComponentInParent<LayoutGroup>();
	}

	public virtual void Init(string displayString, TextAnchor alignment)
	{
		EnableOnWillRenderCanvases(value: false);
		_toDisplayStringParts = new List<TextInfo>();
		_textComponent = this.GetRequiredComponent<Text>();
		_textComponent.text = string.Empty;
		SetTextAlignment(_textComponent.alignment);
		_iCurrentlyVisibleChar = 0;
		_percentageVisibleText = 0f;
		SetTextAlignment(alignment);
		SetDisplayString(displayString);
		SetTypingTime();
		_effectInProgress = false;
		_updateStringBuilder = new StringBuilder();
		_parentLayoutGroup = GetComponentInParent<LayoutGroup>();
	}

	protected virtual void EnableOnWillRenderCanvases(bool value)
	{
		if (_linkedToWillRenderCanvas != value)
		{
			_linkedToWillRenderCanvas = value;
			if (_linkedToWillRenderCanvas)
			{
				Canvas.willRenderCanvases += OnWillRenderCanvases;
			}
			else
			{
				Canvas.willRenderCanvases -= OnWillRenderCanvases;
			}
		}
	}

	public virtual void SetAudioSource(OWAudioSource audioSource)
	{
		_audioSource = audioSource;
	}

	protected virtual void SetTypingTime()
	{
		switch (PlayerData.LoadTextSpeed())
		{
		case TextSpeed.Slow:
			_typingTime = (float)_totalNumVisibleChar * 0.05f;
			break;
		case TextSpeed.Normal:
			_typingTime = (float)_totalNumVisibleChar * 0.01f;
			break;
		case TextSpeed.Fast:
			_typingTime = (float)_totalNumVisibleChar * 0.005f;
			break;
		case TextSpeed.Instant:
			_typingTime = 0f;
			break;
		}
	}

	protected virtual void SetTypingTime(TextSpeed overrideDefaultSpeed)
	{
		switch (overrideDefaultSpeed)
		{
		case TextSpeed.Slow:
			_typingTime = (float)_totalNumVisibleChar * 0.05f;
			break;
		case TextSpeed.Normal:
			_typingTime = (float)_totalNumVisibleChar * 0.01f;
			break;
		case TextSpeed.Fast:
			_typingTime = (float)_totalNumVisibleChar * 0.005f;
			break;
		case TextSpeed.Instant:
			_typingTime = 0f;
			break;
		}
	}

	public virtual void SetTextAlignment(TextAnchor ta)
	{
		_textAnchor = ta;
		_textComponent.alignment = _textAnchor;
		if (_textAnchor == TextAnchor.UpperRight || _textAnchor == TextAnchor.MiddleRight || _textAnchor == TextAnchor.LowerRight)
		{
			_bTypeFromRightSide = true;
		}
		else
		{
			_bTypeFromRightSide = false;
		}
	}

	public virtual void SetDisplayString(string str)
	{
		_strToDisplay = str;
		ParseDisplayString();
	}

	protected void ParseDisplayString()
	{
		List<string> list = new List<string>();
		string[] array = new string[2] { "<", ">" };
		int[] array2 = Enumerable.ToArray(Enumerable.Select(array, (string d) => _strToDisplay.IndexOf(d)));
		int num = 0;
		int num2;
		do
		{
			string empty = string.Empty;
			num2 = int.MaxValue;
			string text = null;
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i] != -1 && array2[i] < num2)
				{
					num2 = array2[i];
					text = array[i];
				}
			}
			if (num2 != int.MaxValue)
			{
				empty = _strToDisplay.Substring(num, num2 - num);
				num = num2 + text.Length;
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j] != -1 && array2[j] < num)
					{
						array2[j] = _strToDisplay.IndexOf(array[j], num);
					}
				}
				list.Add(empty);
				list.Add(text);
			}
			else
			{
				empty = _strToDisplay.Substring(num);
				list.Add(empty);
			}
		}
		while (num2 != int.MaxValue);
		_toDisplayStringParts.Clear();
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		_totalNumVisibleChar = 0;
		for (int k = 0; k < list.Count; k++)
		{
			if (flag)
			{
				stringBuilder.Append(list[k]);
				if (!(list[k] == ">"))
				{
					continue;
				}
				flag = false;
				TextInfo item = default(TextInfo);
				string text2 = (item.text = stringBuilder.ToString());
				item.isTag = true;
				if (text2.Contains("Pause"))
				{
					text2 = text2.TrimStart('<');
					text2 = text2.TrimEnd('>');
					text2 = text2.Replace("Pause", string.Empty);
					text2 = text2.Trim();
					if (text2.Contains("="))
					{
						text2 = text2.TrimStart('=');
						if (!float.TryParse(text2, NumberStyles.Any, OWUtilities.owFormatProvider, out item.pauseTime))
						{
							item.pauseTime = 1f;
						}
					}
					else
					{
						item.pauseTime = 1f;
					}
					item.tagLocation = _totalNumVisibleChar;
				}
				else
				{
					item.pauseTime = 0f;
					item.tagLocation = -1;
				}
				_toDisplayStringParts.Add(item);
			}
			else if (list[k] == "<")
			{
				stringBuilder.Length = 0;
				flag = true;
				stringBuilder.Append(list[k]);
			}
			else
			{
				TextInfo item = default(TextInfo);
				item.text = list[k];
				item.isTag = false;
				_toDisplayStringParts.Add(item);
				_totalNumVisibleChar += item.text.Length;
			}
		}
		if (flag)
		{
			Debug.LogError("Tag not closed. Use &lt; and &gt; instead of < and > if necessary");
		}
		_currentlyDisplayingStrings = new string[_toDisplayStringParts.Count];
	}

	public virtual void StopAndClearText()
	{
		if (_linkedToWillRenderCanvas)
		{
			EnableOnWillRenderCanvases(value: false);
			if (_audioSource != null)
			{
				_audioSource.Stop();
			}
		}
		if (_textComponent != null)
		{
			_textComponent.text = string.Empty;
		}
		_percentageVisibleText = 0f;
		_iCurrentlyVisibleChar = 0;
		_effectInProgress = false;
	}

	public virtual void SetTextWithNoEffect()
	{
		_textComponent.text = _strToDisplay;
		_percentageVisibleText = 1f;
		_iCurrentlyVisibleChar = _totalNumVisibleChar;
	}

	public virtual void StartTextEffect()
	{
		if (_strToDisplay == "")
		{
			SetTextWithNoEffect();
			return;
		}
		SetTypingTime();
		_typingStartTime = Time.unscaledTime;
		_percentageVisibleText = 0f;
		_iCurrentlyVisibleChar = 0;
		_totalHoldTime = 0f;
		_isHolding = false;
		EnableOnWillRenderCanvases(value: true);
		_effectInProgress = true;
		if (_audioSource != null)
		{
			_audioSource.Play();
		}
	}

	public virtual int GetTotalVisibleTextLength()
	{
		return _totalNumVisibleChar;
	}

	public virtual int GetCurrentlyVisibleTextLength()
	{
		return _iCurrentlyVisibleChar;
	}

	public virtual bool IsTextEffectRunning()
	{
		return _effectInProgress;
	}

	public virtual bool IsTextEffectComplete()
	{
		return _percentageVisibleText >= 1f;
	}

	public virtual void CompleteTextEffect()
	{
		if (_percentageVisibleText < 1f)
		{
			_typingStartTime = Time.unscaledTime - _typingTime - _totalHoldTime;
			_isHolding = false;
		}
	}

	protected virtual void OnCompleteTextEffect()
	{
		_setDisabledInNextFrame = true;
		_percentageVisibleText = 1f;
		_effectInProgress = false;
		if (this.OnTypingComplete != null)
		{
			this.OnTypingComplete();
		}
		if (_audioSource != null)
		{
			_audioSource.Stop();
		}
	}

	private void OnWillRenderCanvases()
	{
		if (!_effectInProgress)
		{
			return;
		}
		if (_setDisabledInNextFrame)
		{
			_setDisabledInNextFrame = false;
			_textComponent.SetLayoutDirty();
		}
		if (_toDisplayStringParts.Count == 0)
		{
			_percentageVisibleText = 1f;
			return;
		}
		if (_isHolding)
		{
			if (Time.unscaledTime - _holdStartTime >= _holdDuration)
			{
				_isHolding = false;
			}
			_totalHoldTime += Time.unscaledDeltaTime;
		}
		if (_typingTime == 0f)
		{
			_percentageVisibleText = 1f;
		}
		else
		{
			_percentageVisibleText = (Time.unscaledTime - _typingStartTime - _totalHoldTime) / _typingTime;
			_percentageVisibleText = Mathf.Min(1f, _percentageVisibleText);
		}
		int num = Convert.ToInt32(Mathf.Floor(_percentageVisibleText * (float)_totalNumVisibleChar));
		int num2 = 0;
		bool flag = false;
		if (_iCurrentlyVisibleChar == num)
		{
			return;
		}
		for (int i = 0; i < _toDisplayStringParts.Count; i++)
		{
			int num3 = ((!_bTypeFromRightSide) ? i : (_toDisplayStringParts.Count - i - 1));
			if (num == num2)
			{
				flag = true;
			}
			if (_toDisplayStringParts[num3].isTag)
			{
				if (_toDisplayStringParts[num3].pauseTime == 0f)
				{
					_currentlyDisplayingStrings[num3] = _toDisplayStringParts[num3].text;
				}
				else if (num == _toDisplayStringParts[num3].tagLocation)
				{
					if (!_isHolding)
					{
						_isHolding = true;
						_holdDuration = _toDisplayStringParts[num3].pauseTime;
						_holdStartTime = Time.unscaledTime;
					}
					_currentlyDisplayingStrings[num3] = string.Empty;
				}
			}
			else if (flag)
			{
				_currentlyDisplayingStrings[num3] = string.Empty;
			}
			else
			{
				string text = ((_toDisplayStringParts[num3].text.Length < num - num2) ? _toDisplayStringParts[num3].text : ((!_bTypeFromRightSide) ? _toDisplayStringParts[num3].text.Substring(0, num - num2) : _toDisplayStringParts[num3].text.Substring(_toDisplayStringParts[num3].text.Length + num2 - num)));
				_currentlyDisplayingStrings[num3] = text;
				num2 += text.Length;
			}
		}
		_updateStringBuilder.Length = 0;
		for (int j = 0; j < _currentlyDisplayingStrings.Length; j++)
		{
			_updateStringBuilder.Append(_currentlyDisplayingStrings[j]);
		}
		_textComponent.text = _updateStringBuilder.ToString();
		_iCurrentlyVisibleChar = num;
		_textComponent.SetLayoutDirty();
		if (_iCurrentlyVisibleChar == _totalNumVisibleChar)
		{
			OnCompleteTextEffect();
		}
	}
}
