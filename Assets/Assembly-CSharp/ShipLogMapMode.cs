using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipLogMapMode : ShipLogMode
{
	private const int ENTRY_LIST_POOL_SIZE = 32;

	private const int ENTRY_LIST_DISPLAY_LIMIT = 4;

	[SerializeField]
	private ShipLogAstroObject[] _topRow;

	[SerializeField]
	private ShipLogAstroObject[] _midRow;

	[SerializeField]
	private ShipLogAstroObject[] _bottomRow;

	[SerializeField]
	private ShipLogSandFunnel _sandFunnel;

	[Space]
	[SerializeField]
	private RectTransform _scaleRoot;

	[SerializeField]
	private RectTransform _panRoot;

	[Space]
	[SerializeField]
	private RectTransform _entryListRoot;

	[SerializeField]
	private GameObject _entryTemplate;

	[SerializeField]
	private ShipLogEntryDescriptionField _descriptionField;

	[SerializeField]
	private ShipLogSlideProjector _slideProjector;

	[SerializeField]
	private Text _nameField;

	[SerializeField]
	private RectTransform _photoRoot;

	[SerializeField]
	private GameObject _photoObject;

	[SerializeField]
	private Image _photo;

	[SerializeField]
	private Text _questionMark;

	[SerializeField]
	private RectTransform _entrySelectArrow;

	[Space]
	[SerializeField]
	private CanvasGroupAnimator _mapModeAnimator;

	[SerializeField]
	private CanvasGroupAnimator _solarSystemAnimator;

	[SerializeField]
	private CanvasGroupAnimator _entryMenuAnimator;

	[SerializeField]
	private CanvasGroupAnimator _reticleAnimator;

	[Space]
	[SerializeField]
	private ShipLogEntryHUDMarker _entryHUDMarker;

	[SerializeField]
	private GameObject _markOnHUDPromptRoot;

	[SerializeField]
	private ScreenPromptList _markHUDPromptList;

	[SerializeField]
	private UiSizeSetterMarkHUDPrompt _markHUDSizeSetter;

	private Text _markOnHUDPromptText;

	private ShipLogManager _manager;

	private ScreenPromptList _centerPromptList;

	private ScreenPromptList _upperRightPromptList;

	private string _startingAstroObjectID;

	private ShipLogAstroObject[][] _astroObjects;

	private int _objIndex;

	private int _rowIndex;

	private float _startPanTime;

	private Vector2 _startPanPos;

	private float _panDuration;

	private ShipLogEntryListItem[] _listItems;

	private int _entryIndex = -1;

	private int _maxIndex;

	private Vector2 _origEntryListPos;

	private bool _isEntryMenuOpen;

	private float _pressedUpTimer;

	private float _pressedDownTimer;

	private float _nextHoldUpTime;

	private float _nextHoldDownTime;

	private ScreenPrompt _closeMenuPrompt;

	private ScreenPrompt _detectiveModePrompt;

	private ScreenPrompt _viewEntryPrompt;

	private ScreenPrompt _markOnHUDPrompt;

	private OWAudioSource _oneShotSource;

	public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
	{
		_nameField.font = Locator.GetUIStyleManager().GetShipLogCardFont();
		_oneShotSource = oneShotSource;
		_centerPromptList = centerPromptList;
		_upperRightPromptList = upperRightPromptList;
		_markHUDPromptList.Init();
		float layoutMinHeight = Mathf.Ceil(25f * TextTranslation.GetFontSizeModifier());
		_markHUDPromptList.SetMinElementDimensionsAndFontSize(layoutMinHeight, 0f, 25);
		_markHUDPromptList.SetMinElementDimensionsAndFontSize(layoutMinHeight, 0f, 25);
		_closeMenuPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.LogBackToMapPrompt));
		_detectiveModePrompt = new ScreenPrompt(InputLibrary.swapShipLogMode, "");
		_viewEntryPrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.LogViewEntriesPrompt));
		_markOnHUDPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, "");
		_manager = Locator.GetShipLogManager();
		_startingAstroObjectID = "TIMBER_HEARTH";
		_astroObjects = new ShipLogAstroObject[3][];
		_astroObjects[0] = _bottomRow;
		_astroObjects[1] = _midRow;
		_astroObjects[2] = _topRow;
		_origEntryListPos = _entryListRoot.anchoredPosition;
		_listItems = new ShipLogEntryListItem[32];
		for (int i = 0; i < _listItems.Length; i++)
		{
			GameObject gameObject = Object.Instantiate(_entryTemplate, _entryTemplate.transform.parent);
			gameObject.name = "EntryListItem_" + i;
			_listItems[i] = gameObject.GetComponent<ShipLogEntryListItem>();
			_listItems[i].Init();
		}
		Object.Destroy(_entryTemplate);
		_markHUDSizeSetter.SetInitReferences(_markOnHUDPrompt);
		_markHUDSizeSetter.MarkReadyForInitialization();
		ResetCanvasGroups();
	}

	public override bool AllowModeSwap()
	{
		return PlayerData.GetDetectiveModeEnabled();
	}

	public override bool AllowCancelInput()
	{
		return !_isEntryMenuOpen;
	}

	public override string GetFocusedEntryID()
	{
		return _descriptionField.GetDisplayedEntryID();
	}

	public override void OnEnterComputer()
	{
		for (int i = 0; i < _astroObjects.Length; i++)
		{
			for (int j = 0; j < _astroObjects[i].Length; j++)
			{
				_astroObjects[i][j].OnEnterComputer();
			}
		}
	}

	public override void OnExitComputer()
	{
	}

	public override void EnterMode(string entryID, List<ShipLogFact> revealQueue)
	{
		_detectiveModePrompt.SetText(UITextLibrary.GetString(UITextType.LogRumorModePrompt));
		int[] astroObjectIndex = GetAstroObjectIndex(_startingAstroObjectID);
		FocusOnAstroObjectImmediate(astroObjectIndex[0], astroObjectIndex[1]);
		_mapModeAnimator.AnimateTo(1f, Vector3.one, 0.5f);
		for (int i = 0; i < _astroObjects.Length; i++)
		{
			for (int j = 0; j < _astroObjects[i].Length; j++)
			{
				_astroObjects[i][j].UpdateState();
			}
		}
		_sandFunnel.UpdateState();
		if (entryID.Length > 0)
		{
			ShipLogEntry entry = _manager.GetEntry(entryID);
			int[] astroObjectIndex2 = GetAstroObjectIndex(entry.GetAstroObjectID());
			FocusOnAstroObjectImmediate(astroObjectIndex2[0], astroObjectIndex2[1]);
			OpenEntryMenu(entryID);
		}
		_slideProjector.OnEnterMapMode();
		_markOnHUDPromptRoot.SetActive(value: true);
		Locator.GetPromptManager().AddScreenPrompt(_closeMenuPrompt, _upperRightPromptList, TextAnchor.MiddleRight);
		Locator.GetPromptManager().AddScreenPrompt(_detectiveModePrompt, _upperRightPromptList, TextAnchor.MiddleRight);
		Locator.GetPromptManager().AddScreenPrompt(_viewEntryPrompt, _centerPromptList, TextAnchor.MiddleCenter);
		Locator.GetPromptManager().AddScreenPrompt(_markOnHUDPrompt, _markHUDPromptList, TextAnchor.MiddleCenter);
	}

	public override void ExitMode()
	{
		CloseEntryMenu();
		_mapModeAnimator.AnimateTo(0f, Vector3.one * 0.5f, 0.5f);
		_startingAstroObjectID = _astroObjects[_rowIndex][_objIndex].GetID();
		Locator.GetPromptManager().RemoveScreenPrompt(_closeMenuPrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_detectiveModePrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_viewEntryPrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_markOnHUDPrompt);
	}

	public override void UpdateMode()
	{
		UpdateMapCamera();
		UpdatePrompts();
		if (!_isEntryMenuOpen)
		{
			UpdateMapNavigation();
			if (OWInput.IsNewlyPressed(InputLibrary.interact))
			{
				OpenEntryMenu();
				_oneShotSource.PlayOneShot(AudioType.ShipLogSelectPlanet);
			}
		}
		else
		{
			if (!_isEntryMenuOpen)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			if (_entryIndex >= 0)
			{
				ShipLogEntry entry = _listItems[_entryIndex].GetEntry();
				flag = entry.GetID().Equals(_entryHUDMarker.GetMarkedEntryID());
				flag2 = !flag && entry.GetState() == ShipLogEntry.State.Explored && Locator.GetEntryLocation(entry.GetID()) != null;
				flag4 = flag2 || flag;
			}
			_markOnHUDPromptRoot.SetActive(flag4);
			_markOnHUDPrompt.SetVisibility(flag4);
			_markOnHUDPrompt.SetDisplayState(_manager.GetMarkOnHUDDisplayState());
			if (flag4)
			{
				string text = (flag ? UITextLibrary.GetString(UITextType.LogRemoveMarkerPrompt) : UITextLibrary.GetString(UITextType.LogMarkLocationPrompt));
				_markOnHUDPrompt.SetText(text);
			}
			_markHUDSizeSetter.DoResizeAction(PlayerData.GetTextSize());
			if (OWInput.IsPressed(InputLibrary.up) || OWInput.IsPressed(InputLibrary.up2))
			{
				_pressedUpTimer += Time.unscaledDeltaTime;
			}
			else
			{
				_nextHoldUpTime = 0f;
				_pressedUpTimer = 0f;
			}
			if (OWInput.IsPressed(InputLibrary.down) || OWInput.IsPressed(InputLibrary.down2))
			{
				_pressedDownTimer += Time.unscaledDeltaTime;
			}
			else
			{
				_nextHoldDownTime = 0f;
				_pressedDownTimer = 0f;
			}
			if (_pressedUpTimer > _nextHoldUpTime)
			{
				flag3 = SetEntryFocus(_entryIndex - 1);
				_nextHoldUpTime += ((_nextHoldUpTime < 0.1f) ? 0.4f : 0.15f);
			}
			else if (_pressedDownTimer > _nextHoldDownTime)
			{
				flag3 = SetEntryFocus(_entryIndex + 1);
				_nextHoldDownTime += ((_nextHoldDownTime < 0.1f) ? 0.4f : 0.15f);
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD) && _entryIndex >= 0)
			{
				if (flag)
				{
					_listItems[_entryIndex].SetMarkedOnHUD(markedOnHUD: false);
					_entryHUDMarker.SetEntryLocation(null);
					_oneShotSource.PlayOneShot(AudioType.ShipLogUnmarkLocation);
				}
				else if (flag2)
				{
					for (int i = 0; i <= _maxIndex; i++)
					{
						if (_listItems[i].GetEntry().GetID().Equals(_entryHUDMarker.GetMarkedEntryID()))
						{
							_listItems[i].SetMarkedOnHUD(markedOnHUD: false);
						}
					}
					_listItems[_entryIndex].SetMarkedOnHUD(markedOnHUD: true);
					_entryHUDMarker.SetEntryLocation(Locator.GetEntryLocation(_listItems[_entryIndex].GetEntry().GetID()));
					_oneShotSource.PlayOneShot(AudioType.ShipLogMarkLocation);
					_manager.OnPressMarkOnHUD();
				}
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.cancel))
			{
				CloseEntryMenu();
				_oneShotSource.PlayOneShot(AudioType.ShipLogDeselectPlanet);
			}
			if (flag3)
			{
				_oneShotSource.PlayOneShot(AudioType.ShipLogMoveBetweenEntries);
			}
		}
	}

	private void UpdatePrompts()
	{
		_closeMenuPrompt.SetVisibility(_isEntryMenuOpen);
		_detectiveModePrompt.SetVisibility(PlayerData.GetDetectiveModeEnabled());
		_viewEntryPrompt.SetVisibility(!_isEntryMenuOpen && _astroObjects[_rowIndex][_objIndex].GetState() != ShipLogEntry.State.Hidden);
	}

	private void OpenEntryMenu(string focusedEntryID = "")
	{
		List<ShipLogEntry> list = new List<ShipLogEntry>(_astroObjects[_rowIndex][_objIndex].GetEntries());
		if (list.Count > _listItems.Length)
		{
			Debug.LogError("Not enough list items for all entries!");
			Debug.Break();
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].GetState() == ShipLogEntry.State.Hidden)
			{
				list.RemoveAt(num);
			}
		}
		_maxIndex = list.Count - 1;
		for (int i = 0; i < _listItems.Length; i++)
		{
			if (i <= _maxIndex)
			{
				_listItems[i].Setup(list[i], (float)i * 0.1f);
				_listItems[i].SetMarkedOnHUD(_listItems[i].GetEntry().GetID().Equals(_entryHUDMarker.GetMarkedEntryID()));
			}
			else
			{
				_listItems[i].Reset();
			}
		}
		_isEntryMenuOpen = true;
		_solarSystemAnimator.AnimateTo(0f, Vector3.one * 5f, 0.5f);
		_entryMenuAnimator.AnimateTo(1f, Vector3.one, 0.3f);
		_reticleAnimator.AnimateTo(0f, Vector3.one, 0.2f);
		_entryIndex = -1;
		_entrySelectArrow.gameObject.SetActive(list.Count > 0);
		_photo.enabled = list.Count > 0;
		if (list.Count > 0)
		{
			int entryFocus = 0;
			if (focusedEntryID.Length > 0)
			{
				for (int j = 0; j < _listItems.Length; j++)
				{
					if (_listItems[j].GetEntry().GetID().Equals(focusedEntryID))
					{
						entryFocus = j;
						break;
					}
				}
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(_entryListRoot);
			SetEntryFocus(entryFocus);
		}
		else
		{
			_descriptionField.SetText(UITextLibrary.GetString(UITextType.LogNoDiscoveriesPrompt));
			_questionMark.gameObject.SetActive(value: true);
			_photoObject.SetActive(value: false);
		}
		_detectiveModePrompt.SetText(UITextLibrary.GetString(UITextType.LogFindInRumorsPrompt));
		_descriptionField.SetVisible(visible: true);
	}

	private void CloseEntryMenu()
	{
		if (_entryIndex >= 0 && _entryIndex <= _maxIndex && _listItems[_entryIndex].GetEntry() != null)
		{
			_listItems[_entryIndex].MarkAsRead();
		}
		_astroObjects[_rowIndex][_objIndex].UpdateState();
		_isEntryMenuOpen = false;
		_solarSystemAnimator.AnimateTo(1f, Vector3.one, 0.5f);
		_entryMenuAnimator.AnimateTo(0f, new Vector3(1f, 0.01f, 1f), 0.3f);
		_descriptionField.SetVisible(visible: false);
		_reticleAnimator.AnimateTo(1f, Vector3.one, 0.2f);
		_entrySelectArrow.gameObject.SetActive(value: false);
		_detectiveModePrompt.SetText(UITextLibrary.GetString(UITextType.LogRumorModePrompt));
		_markOnHUDPromptRoot.SetActive(value: false);
		_markOnHUDPrompt.SetVisibility(isVisible: false);
	}

	private bool SetEntryFocus(int index)
	{
		int entryIndex = _entryIndex;
		if (index < 0)
		{
			index = _maxIndex;
		}
		else if (index > _maxIndex)
		{
			index = 0;
		}
		if (entryIndex != index && _listItems[index].GetEntry() != null)
		{
			if (entryIndex >= 0 && entryIndex <= _maxIndex && _listItems[entryIndex].GetEntry() != null)
			{
				_listItems[entryIndex].MarkAsRead();
			}
			_photoObject.SetActive(_listItems[index].GetEntry().GetState() == ShipLogEntry.State.Explored);
			_photo.sprite = _listItems[index].GetEntry().GetSprite();
			_descriptionField.SetEntry(_listItems[index].GetEntry());
			_questionMark.gameObject.SetActive(_listItems[index].GetEntry().GetState() == ShipLogEntry.State.Rumored);
			int num = Mathf.Max(0, index - 4);
			float num2 = _listItems[1].GetAnchoredPosition().y - _listItems[0].GetAnchoredPosition().y;
			_entryListRoot.anchoredPosition = _origEntryListPos - new Vector2(0f, (float)num * num2);
			Vector3 localPosition = _entrySelectArrow.localPosition;
			Vector3 vector = _entrySelectArrow.parent.InverseTransformPoint(_listItems[index].GetSelectionArrowPosition());
			_entrySelectArrow.localPosition = new Vector3(localPosition.x, vector.y, localPosition.z);
			_markOnHUDPromptRoot.SetActive(value: false);
			_markOnHUDPrompt.SetVisibility(_markOnHUDPromptRoot.activeSelf);
			_entryIndex = index;
			UpdateListItemVisuals();
			return true;
		}
		return false;
	}

	private void UpdateListItemVisuals()
	{
		for (int i = 0; i < _listItems.Length && _listItems[i].GetEntry() != null; i++)
		{
			bool focus = i == _entryIndex;
			_listItems[i].SetFocus(focus);
			int num = Mathf.Max(0, _entryIndex - 4);
			int num2 = num;
			int num3 = 4 + num;
			if (i < num2)
			{
				_listItems[i].SetListAlpha(0f);
			}
			else if (i <= num3 - 1)
			{
				_listItems[i].SetListAlpha(1f);
			}
			else if (i == num3)
			{
				_listItems[i].SetListAlpha(0.5f);
			}
			else if (i == num3 + 1)
			{
				_listItems[i].SetListAlpha(0.2f);
			}
			else if (i == num3 + 2)
			{
				_listItems[i].SetListAlpha(0.05f);
			}
			else
			{
				_listItems[i].SetListAlpha(0f);
			}
		}
	}

	private void ResetCanvasGroups()
	{
		_mapModeAnimator.SetImmediate(0f, Vector3.one * 0.5f);
		_solarSystemAnimator.SetImmediate(1f, Vector3.one);
		_entryMenuAnimator.SetImmediate(0f, new Vector3(1f, 0.01f, 1f));
	}

	private void UpdateMapNavigation()
	{
		int rowIndex = _rowIndex;
		int objIndex = _objIndex;
		float x = _astroObjects[_rowIndex][objIndex].GetAnchoredPosition().x;
		if (OWInput.IsNewlyPressed(InputLibrary.right) || OWInput.IsNewlyPressed(InputLibrary.right2))
		{
			IncrementObjIndex(1);
		}
		else if (OWInput.IsNewlyPressed(InputLibrary.left) || OWInput.IsNewlyPressed(InputLibrary.left2))
		{
			IncrementObjIndex(-1);
		}
		else if (OWInput.IsNewlyPressed(InputLibrary.up) || OWInput.IsNewlyPressed(InputLibrary.up2))
		{
			if (_rowIndex < _astroObjects.Length - 1 && AreAnyObjectsVisibleInRow(_rowIndex + 1))
			{
				_rowIndex++;
				_objIndex = GetClosestIndexInRow(x, _rowIndex, requireVisible: true);
			}
		}
		else if ((OWInput.IsNewlyPressed(InputLibrary.down) || OWInput.IsNewlyPressed(InputLibrary.down2)) && _rowIndex > 0 && AreAnyObjectsVisibleInRow(_rowIndex - 1))
		{
			_rowIndex--;
			_objIndex = GetClosestIndexInRow(x, _rowIndex, requireVisible: true);
		}
		if (objIndex != _objIndex || rowIndex != _rowIndex)
		{
			FocusOnAstroObject(_rowIndex, _objIndex);
			_oneShotSource.PlayOneShot(AudioType.ShipLogMoveBetweenPlanets);
		}
	}

	private int GetClosestIndexInRow(float xPos, int rowIndex, bool requireVisible = false)
	{
		float num = float.PositiveInfinity;
		int result = -1;
		for (int i = 0; i < _astroObjects[rowIndex].Length; i++)
		{
			float num2 = Mathf.Abs(xPos - _astroObjects[rowIndex][i].GetAnchoredPosition().x);
			if (num2 < num && (!requireVisible || _astroObjects[rowIndex][i].IsVisible()))
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	private void IncrementObjIndex(int direction)
	{
		float x = _astroObjects[_rowIndex][_objIndex].GetAnchoredPosition().x;
		do
		{
			_objIndex += direction;
			if (_rowIndex == 1)
			{
				if (_objIndex < 0)
				{
					_objIndex = _astroObjects[_rowIndex].Length - 1;
				}
				else if (_objIndex > _astroObjects[_rowIndex].Length - 1)
				{
					_objIndex = 0;
				}
			}
			else if (_objIndex < 0 || _objIndex > _astroObjects[_rowIndex].Length - 1)
			{
				_rowIndex = 1;
				_objIndex = GetClosestIndexInRow(x, _rowIndex) + direction;
			}
		}
		while (_objIndex < 0 || _objIndex > _astroObjects[_rowIndex].Length - 1 || !_astroObjects[_rowIndex][_objIndex].IsVisible());
	}

	private bool AreAnyObjectsVisibleInRow(int row)
	{
		for (int i = 0; i < _astroObjects[row].Length; i++)
		{
			if (_astroObjects[row][i].IsVisible())
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateMapCamera()
	{
		Vector2 b = -_astroObjects[_rowIndex][_objIndex].GetAnchoredPosition();
		float num = Mathf.InverseLerp(_startPanTime, _startPanTime + _panDuration, Time.unscaledTime);
		num = 1f - (num - 1f) * (num - 1f);
		_panRoot.anchoredPosition = Vector2.Lerp(_startPanPos, b, num);
	}

	private void FocusOnAstroObjectImmediate(int rowIndex, int objIndex)
	{
		if (DoesAstroObjectExist(rowIndex, objIndex))
		{
			_rowIndex = rowIndex;
			_objIndex = objIndex;
			_nameField.text = _astroObjects[_rowIndex][_objIndex].GetName();
			_panRoot.anchoredPosition = (_startPanPos = -_astroObjects[_rowIndex][_objIndex].GetAnchoredPosition());
		}
	}

	private void FocusOnAstroObject(int rowIndex, int objIndex)
	{
		if (DoesAstroObjectExist(rowIndex, objIndex))
		{
			_rowIndex = rowIndex;
			_objIndex = objIndex;
			_startPanTime = Time.unscaledTime;
			_startPanPos = _panRoot.anchoredPosition;
			_panDuration = 0.25f;
			_nameField.text = _astroObjects[_rowIndex][_objIndex].GetName();
		}
	}

	private int[] GetAstroObjectIndex(string astroObjectID)
	{
		for (int i = 0; i < _astroObjects.Length; i++)
		{
			for (int j = 0; j < _astroObjects[i].Length; j++)
			{
				if (_astroObjects[i][j].GetID().Equals(astroObjectID))
				{
					return new int[2] { i, j };
				}
			}
		}
		return new int[2] { -1, -1 };
	}

	private bool DoesAstroObjectExist(int rowIndex, int objIndex)
	{
		if (rowIndex >= 0 && rowIndex < _astroObjects.Length && objIndex >= 0 && objIndex < _astroObjects[rowIndex].Length)
		{
			return true;
		}
		Debug.LogError("Indices out of bounds: " + rowIndex + ", " + objIndex);
		Debug.Break();
		return false;
	}
}
