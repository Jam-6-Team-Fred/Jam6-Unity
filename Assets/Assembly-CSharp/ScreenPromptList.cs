using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenPromptList : MonoBehaviour
{
	private List<ScreenPrompt> _listPrompts;

	private List<ScreenPromptElement> _listPromptUiElements;

	private float _promptElementMinWidth;

	private float _promptElementMinHeight;

	private int _listFontSize;

	private bool _initialized;

	private bool _reverse;

	private void Awake()
	{
		Init();
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.AddListener("PlayerResurrection", OnPlayerResurrection);
	}

	private void OnDestroy()
	{
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.RemoveListener("PlayerResurrection", OnPlayerResurrection);
	}

	public void Init()
	{
		if (!_initialized)
		{
			_listPrompts = new List<ScreenPrompt>();
			_listPromptUiElements = new List<ScreenPromptElement>();
			_initialized = true;
		}
	}

	public void SetReversePromptOrder(bool reverse)
	{
		_reverse = reverse;
	}

	public void SetMinElementDimensionsAndFontSize(float layoutMinHeight, float layoutMinWidth, int fontSize)
	{
		_promptElementMinHeight = layoutMinHeight;
		_promptElementMinWidth = layoutMinWidth;
		_listFontSize = fontSize;
		_listFontSize = Mathf.CeilToInt((float)fontSize * TextTranslation.GetFontSizeModifier());
		if (_listPromptUiElements != null)
		{
			for (int i = 0; i < _listPromptUiElements.Count; i++)
			{
				ScreenPromptElement screenPromptElement = _listPromptUiElements[i];
				LayoutElement component = screenPromptElement.gameObject.GetComponent<LayoutElement>();
				component.minWidth = _promptElementMinWidth;
				component.minHeight = _promptElementMinHeight;
				screenPromptElement.RefreshUIElementSize(layoutMinHeight, _listFontSize);
			}
		}
	}

	public int GetFontSize()
	{
		return _listFontSize;
	}

	public void SetCustomLineSpacing(float lineSpacing)
	{
		for (int i = 0; i < _listPromptUiElements.Count; i++)
		{
			_listPromptUiElements[i].SetCustomLineSpacing(lineSpacing);
		}
	}

	public void ResetCustomLineSpacing()
	{
		for (int i = 0; i < _listPromptUiElements.Count; i++)
		{
			_listPromptUiElements[i].ResetCustomLineSpacing();
		}
	}

	public void AddScreenPrompt(ScreenPromptElement spe)
	{
		LayoutElement layoutElement = spe.gameObject.GetComponent<LayoutElement>();
		if (layoutElement == null)
		{
			layoutElement = spe.gameObject.AddComponent<LayoutElement>();
		}
		spe.transform.SetParent(base.transform);
		if (_reverse)
		{
			spe.transform.SetAsFirstSibling();
		}
		spe.SetPreferredIconHeightAndWidth(_promptElementMinHeight);
		ScreenPrompt promptData = spe.GetPromptData();
		if (promptData != null)
		{
			if (!_listPrompts.Contains(promptData))
			{
				_listPrompts.Add(promptData);
				layoutElement.minWidth = _promptElementMinWidth;
				layoutElement.minHeight = _promptElementMinHeight;
				_listPromptUiElements.Add(spe);
				spe.RefreshUIElement();
			}
			else
			{
				Debug.LogWarning("Adding duplicate screen prompt " + promptData.GetText());
			}
		}
	}

	public void RemoveScreenPrompt(ScreenPrompt sp)
	{
		for (int i = 0; i < _listPromptUiElements.Count; i++)
		{
			ScreenPromptElement screenPromptElement = _listPromptUiElements[i];
			if (screenPromptElement.GetPromptData() == sp)
			{
				_listPrompts.Remove(sp);
				_listPromptUiElements.Remove(screenPromptElement);
				if (screenPromptElement.gameObject != null)
				{
					Object.Destroy(screenPromptElement.gameObject);
				}
				break;
			}
		}
	}

	public void RefreshScreenPromptUIElementVisibility(ScreenPrompt sp)
	{
		for (int i = 0; i < _listPromptUiElements.Count; i++)
		{
			ScreenPromptElement screenPromptElement = _listPromptUiElements[i];
			if (screenPromptElement.GetPromptData() == sp && screenPromptElement.IsDisplayed() != sp.IsVisible())
			{
				screenPromptElement.RefreshUIElement();
			}
		}
	}

	public void RefreshScreenPromptUIElement(ScreenPrompt sp)
	{
		for (int i = 0; i < _listPromptUiElements.Count; i++)
		{
			ScreenPromptElement screenPromptElement = _listPromptUiElements[i];
			if (screenPromptElement.GetPromptData() == sp)
			{
				LayoutElement requiredComponent = screenPromptElement.GetRequiredComponent<LayoutElement>();
				if (screenPromptElement.GetPromptData().IsDisplayState(ScreenPrompt.DisplayState.Attention))
				{
					requiredComponent.minWidth = _promptElementMinWidth * screenPromptElement.GetAttentionStateMaxScale();
					requiredComponent.minHeight = _promptElementMinHeight * screenPromptElement.GetAttentionStateMaxScale();
				}
				else
				{
					requiredComponent.minWidth = _promptElementMinWidth;
					requiredComponent.minHeight = _promptElementMinHeight;
				}
				screenPromptElement.RefreshUIElement();
				break;
			}
		}
	}

	public void OnUpdateScreenPromptText(ScreenPrompt sp, string updatedText)
	{
		for (int i = 0; i < _listPromptUiElements.Count; i++)
		{
			ScreenPromptElement screenPromptElement = _listPromptUiElements[i];
			if (screenPromptElement.GetPromptData() == sp)
			{
				screenPromptElement.UpdateText(updatedText);
				break;
			}
		}
	}

	public void RebuildScreenPromptUIElement(ScreenPrompt sp)
	{
		for (int i = 0; i < _listPromptUiElements.Count; i++)
		{
			ScreenPromptElement screenPromptElement = _listPromptUiElements[i];
			if (screenPromptElement.GetPromptData() == sp)
			{
				screenPromptElement.RebuildUIElementFromPrompt();
				break;
			}
		}
	}

	public void RebuildAllScreenPromptUIElements()
	{
		for (int i = 0; i < _listPromptUiElements.Count; i++)
		{
			_listPromptUiElements[i].RebuildUIElementFromPrompt();
		}
	}

	public bool Contains(ScreenPrompt sp)
	{
		return _listPrompts.Contains(sp);
	}

	public bool IsDisplaying(ScreenPrompt sp)
	{
		if (_listPrompts.Contains(sp) && sp.GetPriority() == GetHighestVisiblePriority() && sp.IsVisible())
		{
			return true;
		}
		return false;
	}

	public bool IsDisplayingAnyPrompt()
	{
		for (int i = 0; i < _listPrompts.Count; i++)
		{
			if (_listPrompts[i].IsVisible())
			{
				return true;
			}
		}
		return false;
	}

	public int GetHighestVisiblePriority()
	{
		int num = int.MinValue;
		for (int i = 0; i < _listPrompts.Count; i++)
		{
			ScreenPrompt screenPrompt = _listPrompts[i];
			if (screenPrompt.GetPriority() > num && screenPrompt.IsVisible())
			{
				num = screenPrompt.GetPriority();
			}
		}
		return num;
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnPlayerResurrection()
	{
		base.gameObject.SetActive(value: true);
	}
}
