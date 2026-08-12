using UnityEngine;

public class ReducedFrightsPopup : MonoBehaviour
{
	[SerializeField]
	private ToggleElement _toggleElement;

	[SerializeField]
	private GameObject _popupPrefab;

	private PopupMenu _popup;

	private UITextType _textID = UITextType.MenuMessage_ConfirmReducedFright1;

	private void Awake()
	{
		_toggleElement.OnMenuOptionValueChanged += OnOptionValueChanged;
	}

	private void OnDestroy()
	{
		_toggleElement.OnMenuOptionValueChanged -= OnOptionValueChanged;
		if ((bool)_popup)
		{
			_popup.OnPopupCancel -= OnPopupCancel;
		}
	}

	private void OnOptionValueChanged(SettingsID settingsID, MenuValueOption menuValueOption)
	{
		if (menuValueOption.GetValueAsBool())
		{
			if (_popup == null)
			{
				GameObject gameObject = Object.Instantiate(_popupPrefab);
				_popup = gameObject.GetComponentInChildren<PopupMenu>(includeInactive: true);
				_popup.OnPopupCancel += OnPopupCancel;
			}
			ScreenPrompt okPrompt = new ScreenPrompt(InputLibrary.menuConfirm, UITextLibrary.GetString(UITextType.MenuConfirm));
			ScreenPrompt cancelPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.MenuCancel));
			_popup.EnableMenu(value: true);
			_popup.SetUpPopup(UITextLibrary.GetString(_textID), InputLibrary.menuConfirm, InputLibrary.cancel, okPrompt, cancelPrompt);
		}
	}

	private void OnPopupCancel()
	{
		_toggleElement.Toggle();
	}
}
