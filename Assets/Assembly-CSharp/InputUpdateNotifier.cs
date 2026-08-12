using UnityEngine;

public class InputUpdateNotifier : MonoBehaviour
{
	public delegate void PlayerRectificationCompleteEvent();

	[SerializeField]
	private KeyRebindingDisplayElement[] _keyRebindingDisplayList;

	[SerializeField]
	private PopupTwoOptionMenu _popupMenu;

	[SerializeField]
	private Menu _inputBindingMenu;

	private ScreenPrompt _openMenuPrompt;

	private ScreenPrompt _closePopupPrompt;

	public event PlayerRectificationCompleteEvent OnPlayerRectificationComplete;

	public void DisplayUserRebindables(IInputCommands[] listRebindables)
	{
		for (int i = 0; i < _keyRebindingDisplayList.Length; i++)
		{
			_keyRebindingDisplayList[i].Initialize(listRebindables[i]);
		}
		_openMenuPrompt = new ScreenPrompt(InputLibrary.setDefaults, UITextLibrary.GetString(UITextType.KeyRebindingUpdatePopupContinueBtn));
		_closePopupPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.KeyRebindingUpdateConfirmPopupMsg));
		_popupMenu.SetButtonPrompts(_openMenuPrompt, InputLibrary.setDefaults, _closePopupPrompt, InputLibrary.cancel);
		_popupMenu.CloseMenuOnAction(value: true);
		_popupMenu.OnPopupActionOne += OnOpenInputRebinding;
		_popupMenu.OnPopupActionTwo += OnContinue;
		_popupMenu.EnableMenu(value: true);
	}

	private void OnContinue()
	{
		if (this.OnPlayerRectificationComplete != null)
		{
			this.OnPlayerRectificationComplete();
		}
	}

	private void OnOpenInputRebinding()
	{
		_inputBindingMenu.OnDeactivateMenu += OnCloseInputRebinding;
	}

	private void OnCloseInputRebinding()
	{
		_inputBindingMenu.OnDeactivateMenu -= OnCloseInputRebinding;
		if (this.OnPlayerRectificationComplete != null)
		{
			this.OnPlayerRectificationComplete();
		}
	}
}
