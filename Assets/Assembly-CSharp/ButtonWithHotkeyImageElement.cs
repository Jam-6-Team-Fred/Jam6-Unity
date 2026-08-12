using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(SubmitAction))]
[RequireComponent(typeof(Button))]
public class ButtonWithHotkeyImageElement : MonoBehaviour, IPointerUpHandler, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
	public delegate void PointerPressAndMoveOutEvent();

	[SerializeField]
	private Text _buttonText;

	[SerializeField]
	private Image _buttonBorderImage;

	[SerializeField]
	private Image _hotkeyImageOne;

	[SerializeField]
	private Image _hotkeyImageTwo;

	[SerializeField]
	private GameObject _imageOneLayoutObject;

	[SerializeField]
	private GameObject _imageTwoLayoutObject;

	[Space(10f)]
	[SerializeField]
	private RectTransform _graphicElementsRootTransform;

	[SerializeField]
	private LayoutElement _hotkeyImageOneLayoutElement;

	[SerializeField]
	private LayoutElement _hotkeyImageTwoLayoutElement;

	[Space(10f)]
	[SerializeField]
	private LayoutElement _spacerElement;

	[SerializeField]
	private float _imageToTextElementSpacing = 10f;

	private Button _button;

	private ScreenPrompt _screenPrompt;

	private UIStyleApplier _styleApplier;

	private List<IInputCommands> _commandList;

	private bool _pointerInside;

	private bool _pointerPressed;

	private bool _usingGamepad;

	private static float s_lastButtonImageRefresh;

	private InputMode _inputMode = InputMode.Menu;

	public event PointerPressAndMoveOutEvent OnPointerPressAndMoveOut;

	private void Awake()
	{
		_styleApplier = GetComponent<UIStyleApplier>();
		OWInput.SharedInputManager.OnUpdateInputCommands += ForceRefreshButtonImages;
		ButtonPromptLibrary.OnUpdateButtonPromptConfig += ForceRefreshButtonImages;
	}

	private void OnDestroy()
	{
		OWInput.SharedInputManager.OnUpdateInputCommands -= ForceRefreshButtonImages;
		ButtonPromptLibrary.OnUpdateButtonPromptConfig -= ForceRefreshButtonImages;
	}

	public void SetPrompt(ScreenPrompt prompt, InputMode inputMode = InputMode.Menu)
	{
		_screenPrompt = prompt;
		_inputMode = inputMode;
		if (_screenPrompt == null)
		{
			_buttonText.text = "";
			_imageOneLayoutObject.SetActive(value: false);
			_imageTwoLayoutObject.SetActive(value: false);
		}
		else
		{
			SetTextAndSprite(_screenPrompt.GetText(), _screenPrompt.GetSpriteList());
		}
	}

	private void OnEnable()
	{
		if (_screenPrompt != null)
		{
			if (_styleApplier != null)
			{
				_styleApplier.ChangeState(UIElementState.NORMAL, force: true);
			}
			RefreshTextAndImages();
		}
	}

	private void OnDisable()
	{
		_pointerInside = false;
		_pointerPressed = false;
	}

	public void RefreshTextAndImages(bool forceRefresh = false)
	{
		if (_screenPrompt != null)
		{
			SetTextAndSprite(_screenPrompt.GetText(), _screenPrompt.GetSpriteList(forceRefresh));
		}
	}

	private void SetTextAndSprite(string text, List<Sprite> spriteImgList)
	{
		_buttonText.text = text;
		if (_button == null)
		{
			_button = this.GetRequiredComponent<Button>();
		}
		_button.interactable = true;
		if (_buttonBorderImage != null)
		{
			_buttonBorderImage.gameObject.SetActive(!_usingGamepad);
		}
		if (!_usingGamepad || spriteImgList.Count == 0)
		{
			_imageOneLayoutObject.SetActive(value: false);
			_imageTwoLayoutObject.SetActive(value: false);
			if (_spacerElement != null)
			{
				_spacerElement.preferredWidth = 0f;
			}
		}
		else if (spriteImgList.Count == 1)
		{
			_imageOneLayoutObject.SetActive(value: true);
			_imageTwoLayoutObject.SetActive(value: false);
			if (_spacerElement != null)
			{
				_spacerElement.preferredWidth = _imageToTextElementSpacing;
			}
			_hotkeyImageOne.sprite = spriteImgList[0];
			_hotkeyImageOne.preserveAspect = true;
			if (_graphicElementsRootTransform != null && _hotkeyImageOneLayoutElement != null)
			{
				_hotkeyImageOneLayoutElement.preferredHeight = _graphicElementsRootTransform.sizeDelta.y;
				_hotkeyImageOneLayoutElement.preferredWidth = _graphicElementsRootTransform.sizeDelta.y;
			}
			_hotkeyImageOne.SetAllDirty();
		}
		else if (spriteImgList.Count == 2)
		{
			_imageOneLayoutObject.SetActive(value: true);
			_imageTwoLayoutObject.SetActive(value: true);
			if (_spacerElement != null)
			{
				_spacerElement.preferredWidth = _imageToTextElementSpacing;
			}
			_hotkeyImageOne.sprite = spriteImgList[0];
			_hotkeyImageTwo.sprite = spriteImgList[1];
			_hotkeyImageOne.preserveAspect = true;
			_hotkeyImageTwo.preserveAspect = true;
			if (_graphicElementsRootTransform != null && _hotkeyImageOneLayoutElement != null)
			{
				_hotkeyImageOneLayoutElement.preferredHeight = _graphicElementsRootTransform.sizeDelta.y;
				_hotkeyImageOneLayoutElement.preferredWidth = _graphicElementsRootTransform.sizeDelta.y;
			}
			if (_graphicElementsRootTransform != null && _hotkeyImageTwoLayoutElement != null)
			{
				_hotkeyImageTwoLayoutElement.preferredHeight = _graphicElementsRootTransform.sizeDelta.y;
				_hotkeyImageTwoLayoutElement.preferredWidth = _graphicElementsRootTransform.sizeDelta.y;
			}
			_hotkeyImageOne.SetAllDirty();
			_hotkeyImageTwo.SetAllDirty();
		}
		else
		{
			Debug.LogError("ButtonWithHotkeyImageElement cannot handle more than two images");
		}
	}

	private void Update()
	{
		bool flag = false;
		bool flag2 = OWInput.UsingGamepad();
		if (_usingGamepad != flag2 && Time.unscaledTime - s_lastButtonImageRefresh > ButtonPromptLibrary.BUTTONIMAGE_MIN_REFRESH_TIME)
		{
			s_lastButtonImageRefresh = Time.unscaledTime;
			_usingGamepad = flag2;
			flag = true;
		}
		if (flag)
		{
			RefreshTextAndImages();
		}
		bool flag3 = false;
		if (!(_styleApplier != null) || _screenPrompt == null)
		{
			return;
		}
		_commandList = _screenPrompt.GetInputCommandList();
		for (int i = 0; i < _commandList.Count; i++)
		{
			if (OWInput.IsPressed(_commandList[i], _inputMode))
			{
				flag3 = true;
				break;
			}
		}
		if (flag3 || (_pointerInside && _pointerPressed))
		{
			_styleApplier.ChangeState(UIElementState.PRESSED);
		}
		else if (Locator.GetEventSystem().currentSelectedGameObject == base.gameObject)
		{
			_styleApplier.ChangeState(UIElementState.INTERMEDIATELY_HIGHLIGHTED);
		}
		else if (_pointerInside)
		{
			_styleApplier.ChangeState(UIElementState.ROLLOVER_HIGHLIGHT);
		}
		else
		{
			_styleApplier.ChangeState(UIElementState.NORMAL);
		}
	}

	public virtual void OnPointerEnter(PointerEventData pointerEventData)
	{
		_pointerInside = true;
	}

	public virtual void OnPointerExit(PointerEventData pointerEventData)
	{
		_pointerInside = false;
		if (_pointerPressed && this.OnPointerPressAndMoveOut != null)
		{
			this.OnPointerPressAndMoveOut();
		}
		_pointerPressed = false;
	}

	public virtual void OnPointerUp(PointerEventData pointerEventData)
	{
		_pointerPressed = false;
	}

	public virtual void OnPointerDown(PointerEventData pointerEventData)
	{
		_pointerPressed = true;
	}

	protected virtual void ForceRefreshButtonImages()
	{
		RefreshTextAndImages(forceRefresh: true);
	}
}
