using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIStyleApplier : MonoBehaviour, IPointerUpHandler, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[Serializable]
	public struct OnOffGraphic
	{
		public Graphic graphic;

		public bool visibleNormal;

		public bool visibleIntermediate;

		public bool visibleHighlighted;

		public bool visiblePressed;

		public bool visibleDisabled;

		public bool visibleMouseRollover;
	}

	[SerializeField]
	protected bool _secondaryMenuItem;

	[SerializeField]
	protected bool _buttonItem;

	[SerializeField]
	protected bool _preflightItem;

	[SerializeField]
	protected Text[] _textItems;

	[SerializeField]
	protected Graphic[] _foregroundGraphics;

	[SerializeField]
	protected Graphic[] _backgroundGraphics;

	[SerializeField]
	protected Graphic[] _onOffGraphics;

	[SerializeField]
	protected OnOffGraphic[] _onOffGraphicList;

	protected static UIStyleManager s_styleManager;

	protected UIElementState _currentState;

	protected bool _mouseOver;

	protected bool _mousePressed;

	protected bool _selected;

	protected int[] _originalFontSizes;

	protected Vector3[] _originalScales;

	protected bool _enableAutoInputStateChanges = true;

	protected virtual void Start()
	{
		_currentState = EvaluateState(null);
		ChangeState(_currentState, force: true);
	}

	protected virtual void OnDisable()
	{
		_mouseOver = false;
		_mousePressed = false;
		if (_enableAutoInputStateChanges)
		{
			ChangeState(UIElementState.NORMAL);
		}
	}

	protected virtual bool InitStyleManager()
	{
		if (s_styleManager == null)
		{
			s_styleManager = Locator.GetUIStyleManager();
			if (s_styleManager == null)
			{
				return false;
			}
		}
		return true;
	}

	public void SetAutoInputStateChangesEnabled(bool value)
	{
		_enableAutoInputStateChanges = value;
	}

	public virtual void ChangeState(UIElementState state, bool force = false)
	{
		if (_currentState != state)
		{
			_currentState = state;
			ChangeColors(state);
			ChangeVisibility(state);
		}
		else if (force)
		{
			ChangeColors(state);
			ChangeVisibility(state);
		}
	}

	protected virtual void ChangeColors(UIElementState state)
	{
		if (InitStyleManager())
		{
			Color color;
			Color color2;
			if (_preflightItem)
			{
				color = s_styleManager.GetPreflightMenuColor(state);
				color2 = Color.black;
			}
			else if (_buttonItem)
			{
				color = s_styleManager.GetButtonForegroundMenuColor(state);
				color2 = s_styleManager.GetButtonBackgroundMenuColor(state);
			}
			else if (_secondaryMenuItem)
			{
				color = s_styleManager.GetSecondaryForegroundMenuColor(state);
				color2 = s_styleManager.GetSecondaryBackgroundMenuColor(state);
			}
			else
			{
				color = s_styleManager.GetForegroundMenuColor(state);
				color2 = s_styleManager.GetBackgroundMenuColor(state);
			}
			for (int i = 0; i < _foregroundGraphics.Length; i++)
			{
				_foregroundGraphics[i].color = color;
			}
			for (int j = 0; j < _backgroundGraphics.Length; j++)
			{
				_backgroundGraphics[j].color = color2;
			}
		}
	}

	protected virtual void ChangeVisibility(UIElementState state)
	{
		if (_onOffGraphicList.Length != 0)
		{
			for (int i = 0; i < _onOffGraphicList.Length; i++)
			{
				switch (state)
				{
				case UIElementState.NORMAL:
					_onOffGraphicList[i].graphic.enabled = _onOffGraphicList[i].visibleNormal;
					break;
				case UIElementState.INTERMEDIATELY_HIGHLIGHTED:
					_onOffGraphicList[i].graphic.enabled = _onOffGraphicList[i].visibleIntermediate;
					break;
				case UIElementState.HIGHLIGHTED:
					_onOffGraphicList[i].graphic.enabled = _onOffGraphicList[i].visibleHighlighted;
					break;
				case UIElementState.PRESSED:
					_onOffGraphicList[i].graphic.enabled = _onOffGraphicList[i].visiblePressed;
					break;
				case UIElementState.DISABLED:
					_onOffGraphicList[i].graphic.enabled = _onOffGraphicList[i].visibleDisabled;
					break;
				case UIElementState.ROLLOVER_HIGHLIGHT:
					_onOffGraphicList[i].graphic.enabled = _onOffGraphicList[i].visibleMouseRollover;
					break;
				}
			}
		}
		else
		{
			for (int j = 0; j < _onOffGraphics.Length; j++)
			{
				_onOffGraphics[j].enabled = state == UIElementState.HIGHLIGHTED;
			}
		}
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		_selected = true;
		if (_enableAutoInputStateChanges)
		{
			UIElementState state = EvaluateState(eventData);
			ChangeState(state);
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		_selected = false;
		if (_enableAutoInputStateChanges)
		{
			UIElementState state = EvaluateState(eventData);
			ChangeState(state);
		}
	}

	public virtual void OnPointerEnter(PointerEventData pointerEventData)
	{
		_mouseOver = true;
		if (_enableAutoInputStateChanges)
		{
			UIElementState state = EvaluateState(pointerEventData);
			ChangeState(state);
		}
	}

	public virtual void OnPointerExit(PointerEventData pointerEventData)
	{
		_mouseOver = false;
		if (_enableAutoInputStateChanges)
		{
			UIElementState state = EvaluateState(pointerEventData);
			ChangeState(state);
		}
	}

	public virtual void OnPointerUp(PointerEventData pointerEventData)
	{
		_mousePressed = false;
		if (_enableAutoInputStateChanges)
		{
			UIElementState state = EvaluateState(pointerEventData);
			ChangeState(state);
		}
	}

	public virtual void OnPointerDown(PointerEventData pointerEventData)
	{
		_mousePressed = true;
		if (_enableAutoInputStateChanges)
		{
			UIElementState state = EvaluateState(pointerEventData);
			ChangeState(state);
		}
	}

	protected virtual UIElementState EvaluateState(BaseEventData eventData)
	{
		if (_mouseOver && _mousePressed)
		{
			return UIElementState.PRESSED;
		}
		if (_selected)
		{
			return UIElementState.HIGHLIGHTED;
		}
		if (_mouseOver)
		{
			return UIElementState.ROLLOVER_HIGHLIGHT;
		}
		return UIElementState.NORMAL;
	}
}
