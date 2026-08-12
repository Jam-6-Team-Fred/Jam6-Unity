using System.Collections.Generic;
using UnityEngine;

public class ShipLogDetectiveMode : ShipLogMode
{
	private struct LinkKey
	{
		public string sourceID;

		public string targetID;

		public LinkKey(string sourceID, string targetID)
		{
			this.sourceID = sourceID;
			this.targetID = targetID;
		}
	}

	[SerializeField]
	private bool _editMode;

	[Space]
	[SerializeField]
	private RectTransform _rootCanvasTransform;

	[SerializeField]
	private CanvasGroupAnimator _canvasAnimator;

	[SerializeField]
	private RectTransform _scaleRoot;

	[SerializeField]
	private RectTransform _panRoot;

	[SerializeField]
	private RectTransform _reticle;

	[SerializeField]
	private ShipLogEntryDescriptionField _descriptionField;

	[SerializeField]
	private ShipLogSlideProjector _slideProjector;

	[Space]
	[SerializeField]
	private GameObject _entryCardTemplate;

	[SerializeField]
	private GameObject _entryLinkTemplate;

	[Space]
	[SerializeField]
	private ShipLogEntryHUDMarker _entryHUDMarker;

	[SerializeField]
	private OWAudioSource _navigateAudioSource;

	[Space]
	[SerializeField]
	private FontAndLanguageController _fontAndLanguageController;

	private ShipLogManager _manager;

	private ScreenPromptList _centerPromptList;

	private ScreenPromptList _upperRightPromptList;

	private List<ShipLogEntryCard> _cardList;

	private List<ShipLogEntryLink> _linkList;

	private List<ShipLogEntryLink> _highlightedLinkList;

	private Dictionary<string, ShipLogEntryCard> _cardDict;

	private Dictionary<LinkKey, ShipLogEntryLink> _linkDict;

	private IShipLogSelectable _focusedSelectable;

	private List<ShipLogFact> _factRevealQueue;

	private ShipLogEntryCard _targetCard;

	private bool _updateRevealAnim;

	private bool _updateFrameAll;

	private float _animWaitSeconds;

	private float _startPanTime;

	private Vector2 _startPanPos;

	private Vector3 _startScale;

	private int _queueIndex;

	private float _panDuration;

	private float _enterModeTime;

	private Vector2 _minBounds;

	private Vector2 _maxBounds;

	private float _minScale;

	private float _maxScale;

	private ScreenPrompt _zoomPrompt;

	private ScreenPrompt _viewEntryPrompt;

	private ScreenPrompt _mapModePrompt;

	private ScreenPrompt _findInMapModePrompt;

	private ScreenPrompt _skipPrompt;

	private ScreenPrompt _markOnHUDPrompt;

	private OWAudioSource _oneShotSource;

	private void OnValidate()
	{
	}

	public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
	{
		_manager = Locator.GetShipLogManager();
		_factRevealQueue = new List<ShipLogFact>();
		_highlightedLinkList = new List<ShipLogEntryLink>();
		_oneShotSource = oneShotSource;
		_navigateAudioSource.SetLocalVolume(0f);
		_centerPromptList = centerPromptList;
		_upperRightPromptList = upperRightPromptList;
		_zoomPrompt = new ScreenPrompt(InputLibrary.mapZoomIn, InputLibrary.mapZoomOut, UITextLibrary.GetString(UITextType.LogZoomPrompt), ScreenPrompt.MultiCommandType.POS_NEG);
		_mapModePrompt = new ScreenPrompt(InputLibrary.swapShipLogMode, UITextLibrary.GetString(UITextType.LogMapModePrompt));
		_findInMapModePrompt = new ScreenPrompt(InputLibrary.swapShipLogMode, UITextLibrary.GetString(UITextType.LogFindInMapPrompt));
		_viewEntryPrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.LogViewPrompt));
		_skipPrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.LogSkipPrompt));
		_markOnHUDPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, "");
		GenerateEntryCards();
		GenerateEntryLinks();
		if (!_editMode)
		{
			_canvasAnimator.SetImmediate(0f, Vector3.one * 0.5f);
		}
		else
		{
			_reticle.gameObject.SetActive(value: false);
		}
	}

	public override bool IsEditMode()
	{
		return _editMode;
	}

	public override bool AllowModeSwap()
	{
		return !_updateRevealAnim;
	}

	public override bool AllowCancelInput()
	{
		if (!_updateRevealAnim)
		{
			return !_descriptionField.IsVisible();
		}
		return false;
	}

	public override string GetFocusedEntryID()
	{
		return _descriptionField.GetDisplayedEntryID(includeLinks: false);
	}

	public override void OnEnterComputer()
	{
		UpdateBounds();
		FrameRevealedCards();
		for (int i = 0; i < _cardList.Count; i++)
		{
			_cardList[i].OnEnterComputer();
		}
		for (int j = 0; j < _linkList.Count; j++)
		{
			_linkList[j].UpdatePosition();
			_linkList[j].UpdateVisibility();
			ShipLogEntryLink link = GetLink(_linkList[j].GetTargetEntryCard().GetEntry().GetID(), _linkList[j].GetSourceEntryCard().GetEntry().GetID());
			if (!(link != null))
			{
				continue;
			}
			link.UpdateVisibility();
			if (link.IsVisible() && _linkList[j].IsVisible())
			{
				if (link.GetRevealOrder() < _linkList[j].GetRevealOrder())
				{
					_linkList[j].Hide();
				}
				else
				{
					link.Hide();
				}
			}
		}
	}

	public override void OnExitComputer()
	{
	}

	public override void EnterMode(string entryID, List<ShipLogFact> revealQueue)
	{
		_enterModeTime = Time.unscaledTime;
		for (int i = 0; i < _cardList.Count; i++)
		{
			if (_cardList[i].GetEntry().GetState() != ShipLogEntry.State.Hidden)
			{
				_cardList[i].OnEnterDetectiveMode();
				_cardList[i].SetMarkedOnHUD(_cardList[i].GetEntry().GetID().Equals(_entryHUDMarker.GetMarkedEntryID()));
			}
		}
		_focusedSelectable = null;
		_canvasAnimator.AnimateTo(1f, Vector3.one * 1f, 0.5f);
		if (revealQueue != null && revealQueue.Count > 0)
		{
			_factRevealQueue.Clear();
			_factRevealQueue = revealQueue;
			_updateRevealAnim = true;
			_updateFrameAll = false;
			_targetCard = null;
			_animWaitSeconds = 0.5f;
			_panDuration = 0.7f;
			_queueIndex = 0;
			_startScale = _scaleRoot.localScale;
			PrepareRevealAnimations();
		}
		else if (entryID.Length > 0)
		{
			ShipLogEntryCard shipLogEntryCard = _cardDict[entryID];
			_panRoot.anchoredPosition = -shipLogEntryCard.GetAnchoredPosition();
			_scaleRoot.localScale = Vector3.one;
			UpdateFocusedSelectable();
			if (_focusedSelectable != null)
			{
				_descriptionField.SetVisible(visible: true);
			}
		}
		else
		{
			FrameRevealedCards();
		}
		_slideProjector.OnEnterDetectiveMode();
		_navigateAudioSource.SetLocalVolume(0f);
		_navigateAudioSource.Play();
		Locator.GetPromptManager().AddScreenPrompt(_zoomPrompt, _upperRightPromptList, TextAnchor.MiddleRight);
		Locator.GetPromptManager().AddScreenPrompt(_mapModePrompt, _upperRightPromptList, TextAnchor.MiddleRight);
		Locator.GetPromptManager().AddScreenPrompt(_findInMapModePrompt, _upperRightPromptList, TextAnchor.MiddleRight);
		Locator.GetPromptManager().AddScreenPrompt(_skipPrompt, _upperRightPromptList, TextAnchor.MiddleRight);
		Locator.GetPromptManager().AddScreenPrompt(_viewEntryPrompt, _centerPromptList, TextAnchor.MiddleCenter);
		Locator.GetPromptManager().AddScreenPrompt(_markOnHUDPrompt, _centerPromptList, TextAnchor.MiddleCenter);
	}

	public override void ExitMode()
	{
		_navigateAudioSource.SetLocalVolume(0f);
		_navigateAudioSource.Stop();
		_updateRevealAnim = false;
		_updateFrameAll = false;
		_descriptionField.SetVisible(visible: false);
		_canvasAnimator.AnimateTo(0f, Vector3.one * 0.05f, 0.5f);
		ChangeFocus(null);
		Locator.GetPromptManager().RemoveScreenPrompt(_zoomPrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_viewEntryPrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_mapModePrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_findInMapModePrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_skipPrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_markOnHUDPrompt);
	}

	public override void UpdateMode()
	{
		UpdatePrompts();
		float x = _scaleRoot.localScale.x;
		if (_updateFrameAll)
		{
			float t = Mathf.InverseLerp(_startPanTime, _startPanTime + _panDuration, Time.unscaledTime);
			t = Mathf.SmoothStep(0f, 1f, t);
			Vector2 vector = _maxBounds - _minBounds;
			Vector2 b = -(_minBounds + vector * 0.5f);
			_panRoot.anchoredPosition = Vector2.Lerp(_startPanPos, b, t);
			_scaleRoot.localScale = Vector2.Lerp(_startScale, Vector2.one * _minScale, t);
			if (t >= 1f)
			{
				_updateFrameAll = false;
			}
		}
		else if (!_updateRevealAnim)
		{
			UpdateNavigationInput();
			UpdateFocusedSelectable();
			if (_focusedSelectable != null)
			{
				if (_focusedSelectable.GetType() == typeof(ShipLogEntryCard))
				{
					ShipLogEntryCard shipLogEntryCard = (ShipLogEntryCard)_focusedSelectable;
					bool flag = shipLogEntryCard.GetEntry().GetID().Equals(_entryHUDMarker.GetMarkedEntryID());
					bool flag2 = !flag && shipLogEntryCard.GetEntry().GetState() == ShipLogEntry.State.Explored && Locator.GetEntryLocation(shipLogEntryCard.GetEntry().GetID()) != null;
					if (!_descriptionField.IsVisible() && (flag2 || flag))
					{
						string text = (flag ? UITextLibrary.GetString(UITextType.LogRemoveMarkerPrompt) : UITextLibrary.GetString(UITextType.LogMarkLocationPrompt));
						text = ((!TextTranslation.Get().IsLanguageCJK()) ? text.Replace('\n', ' ') : text.Replace("\n", ""));
						_markOnHUDPrompt.SetText(text);
						_markOnHUDPrompt.SetVisibility(isVisible: true);
						_markOnHUDPrompt.SetDisplayState(_manager.GetMarkOnHUDDisplayState());
					}
					if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD))
					{
						if (flag2)
						{
							if (_entryHUDMarker.GetMarkedEntryID().Length > 0)
							{
								_cardDict[_entryHUDMarker.GetMarkedEntryID()].SetMarkedOnHUD(markedOnHUD: false);
							}
							shipLogEntryCard.SetMarkedOnHUD(markedOnHUD: true);
							_entryHUDMarker.SetEntryLocation(Locator.GetEntryLocation(shipLogEntryCard.GetEntry().GetID()));
							_oneShotSource.PlayOneShot(AudioType.ShipLogMarkLocation);
							_manager.OnPressMarkOnHUD();
						}
						else if (flag)
						{
							shipLogEntryCard.SetMarkedOnHUD(markedOnHUD: false);
							_entryHUDMarker.SetEntryLocation(null);
							_oneShotSource.PlayOneShot(AudioType.ShipLogUnmarkLocation);
						}
					}
				}
				if (!_descriptionField.IsVisible() && OWInput.IsNewlyPressed(InputLibrary.interact))
				{
					_descriptionField.SetVisible(visible: true);
					_focusedSelectable.MarkAsRead();
					_oneShotSource.PlayOneShot(AudioType.ShipLogSelectEntry);
				}
				else if (_descriptionField.IsVisible() && (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.cancel)))
				{
					_descriptionField.SetVisible(visible: false);
					_oneShotSource.PlayOneShot(AudioType.ShipLogDeselectEntry);
				}
			}
		}
		else if (OWInput.IsNewlyPressed(InputLibrary.interact))
		{
			FinishRevealAnimation();
		}
		else
		{
			UpdateRevealAnimation();
		}
		float target = ((Mathf.Abs(x - _scaleRoot.localScale.x) > 0.001f) ? 1f : 0f);
		_navigateAudioSource.SetLocalVolume(Mathf.MoveTowards(_navigateAudioSource.GetLocalVolume(), target, Time.unscaledDeltaTime * 10f));
	}

	private void UpdateBounds(bool newlyRevealedOnly = false)
	{
		_minBounds = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
		_maxBounds = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
		float num = 275f;
		for (int i = 0; i < _cardList.Count; i++)
		{
			if (_cardList[i].GetEntry().GetState() != ShipLogEntry.State.Hidden && (!newlyRevealedOnly || _cardList[i].IsRevealAnimationReady()))
			{
				_minBounds.x = Mathf.Min(_minBounds.x, _cardList[i].GetAnchoredPosition().x - num);
				_maxBounds.x = Mathf.Max(_maxBounds.x, _cardList[i].GetAnchoredPosition().x + num);
				_minBounds.y = Mathf.Min(_minBounds.y, _cardList[i].GetAnchoredPosition().y - num);
				_maxBounds.y = Mathf.Max(_maxBounds.y, _cardList[i].GetAnchoredPosition().y + num);
			}
		}
		Vector2 sizeDelta = _rootCanvasTransform.sizeDelta;
		float num2 = sizeDelta.y / sizeDelta.x;
		Vector2 vector = _maxBounds - _minBounds;
		_minScale = Mathf.Min(1f, (vector.y / vector.x > num2) ? (sizeDelta.y / vector.y) : (sizeDelta.x / vector.x));
		_maxScale = 5f;
	}

	private void FrameRevealedCards()
	{
		Vector2 vector = _maxBounds - _minBounds;
		Vector2 vector2 = _minBounds + vector * 0.5f;
		_panRoot.anchoredPosition = -vector2;
		_scaleRoot.localScale = Vector3.one * _minScale;
	}

	private void UpdatePrompts()
	{
		_zoomPrompt.SetVisibility(!_updateRevealAnim);
		_viewEntryPrompt.SetVisibility(!_updateRevealAnim && _focusedSelectable != null && !_descriptionField.IsVisible());
		bool flag = _descriptionField.IsVisible() && GetFocusedEntryID().Length > 0;
		_findInMapModePrompt.SetVisibility(!_updateRevealAnim && flag);
		_mapModePrompt.SetVisibility(!_updateRevealAnim && !flag);
		_skipPrompt.SetVisibility(_updateRevealAnim);
		_markOnHUDPrompt.SetVisibility(isVisible: false);
	}

	private void UpdateNavigationInput()
	{
		Vector2 axisValue = OWInput.GetAxisValue(InputLibrary.moveXZ);
		Vector2 vector = -_panRoot.anchoredPosition;
		vector += axisValue * (60f / _scaleRoot.localScale.x) * Time.unscaledDeltaTime * 10f;
		vector.x = Mathf.Clamp(vector.x, _minBounds.x, _maxBounds.x);
		vector.y = Mathf.Clamp(vector.y, _minBounds.y, _maxBounds.y);
		_panRoot.anchoredPosition = -vector;
		float num = OWInput.GetValue(InputLibrary.mapZoomIn) - OWInput.GetValue(InputLibrary.mapZoomOut);
		float x = _scaleRoot.localScale.x;
		x += 3f * num * _scaleRoot.localScale.x * Time.unscaledDeltaTime;
		x = Mathf.Clamp(x, _minScale, _maxScale);
		_scaleRoot.localScale = Vector3.one * x;
	}

	private void UpdateFocusedSelectable()
	{
		if (_focusedSelectable != null && !_focusedSelectable.CheckPointCollision(_reticle.position))
		{
			ChangeFocus(null);
			if (_descriptionField.IsVisible())
			{
				_descriptionField.SetVisible(visible: false);
				_oneShotSource.PlayOneShot(AudioType.ShipLogDeselectEntry);
			}
		}
		for (int num = _cardList.Count - 1; num >= 0; num--)
		{
			if (_cardList[num].IsVisible() && _cardList[num].CheckPointCollision(_reticle.position))
			{
				if (!_cardList[num].Equals(_focusedSelectable))
				{
					ChangeFocus(_cardList[num]);
					if (Time.unscaledTime > _enterModeTime + 0.5f)
					{
						_oneShotSource.PlayOneShot(AudioType.ShipLogHighlightEntry);
					}
					_descriptionField.SetEntry(_cardList[num].GetEntry());
					_descriptionField.SetVisible(visible: false);
				}
				return;
			}
		}
		for (int i = 0; i < _linkList.Count; i++)
		{
			if (!_linkList[i].IsVisible() || !_linkList[i].CheckPointCollision(_reticle.position))
			{
				continue;
			}
			if (!_linkList[i].Equals(_focusedSelectable))
			{
				ChangeFocus(_linkList[i]);
				if (Time.unscaledTime > _enterModeTime + 0.5f)
				{
					_oneShotSource.PlayOneShot(AudioType.ShipLogHighlightEntry);
				}
				_descriptionField.SetLink(_linkList[i]);
				_descriptionField.SetVisible(visible: false);
			}
			break;
		}
	}

	private void ChangeFocus(IShipLogSelectable selectable)
	{
		if (_focusedSelectable != null)
		{
			_focusedSelectable.OnLoseFocus();
			for (int i = 0; i < _highlightedLinkList.Count; i++)
			{
				_highlightedLinkList[i].OnLoseFocus();
			}
			_highlightedLinkList.Clear();
		}
		_focusedSelectable = selectable;
		if (_focusedSelectable == null)
		{
			return;
		}
		_focusedSelectable.OnGainFocus();
		string text = "";
		if (_focusedSelectable.GetType() == typeof(ShipLogEntryCard))
		{
			ShipLogEntryCard shipLogEntryCard = (ShipLogEntryCard)_focusedSelectable;
			text = ((shipLogEntryCard.GetEntry().GetState() == ShipLogEntry.State.Rumored) ? UITextLibrary.GetString(UITextType.LogViewRumoredEntryPrompt) : UITextLibrary.GetString(UITextType.LogViewPrompt));
			if (shipLogEntryCard.GetEntry().GetState() == ShipLogEntry.State.Rumored)
			{
				for (int j = 0; j < _linkList.Count; j++)
				{
					if (_linkList[j].GetTargetEntryCard().Equals(shipLogEntryCard))
					{
						_highlightedLinkList.Add(_linkList[j]);
						_linkList[j].OnGainFocus();
					}
				}
			}
		}
		else
		{
			text = UITextLibrary.GetString(UITextType.LogViewRumorPrompt);
		}
		_viewEntryPrompt.SetText(text);
	}

	private void PrepareRevealAnimations()
	{
		for (int i = 0; i < _factRevealQueue.Count; i++)
		{
			ShipLogEntry entry = _manager.GetEntry(_factRevealQueue[i].GetEntryID());
			if (!_cardDict[entry.GetID()].IsRevealAnimationReady())
			{
				_cardDict[entry.GetID()].PrepareRevealAnimation();
			}
			if (_factRevealQueue[i].HasSource())
			{
				ShipLogEntryLink link = GetLink(_factRevealQueue[i].GetSourceID(), _factRevealQueue[i].GetEntryID());
				if (link != null && link.IsVisible() && !link.IsRevealAnimationReady())
				{
					link.PrepareRevealAnimation();
				}
			}
		}
	}

	private void UpdateRevealAnimation()
	{
		if (_animWaitSeconds > 0f)
		{
			_animWaitSeconds = Mathf.MoveTowards(_animWaitSeconds, 0f, Time.unscaledDeltaTime);
		}
		else if (_targetCard != null)
		{
			float t = Mathf.InverseLerp(_startPanTime, _startPanTime + _panDuration, Time.unscaledTime);
			t = Mathf.SmoothStep(0f, 1f, t);
			Vector2 b = -_targetCard.GetAnchoredPosition();
			_panRoot.anchoredPosition = Vector2.Lerp(_startPanPos, b, t);
			_scaleRoot.localScale = Vector2.Lerp(_startScale, Vector3.one, t);
			if (!(t >= 1f))
			{
				return;
			}
			for (int i = _queueIndex; i < _factRevealQueue.Count; i++)
			{
				ShipLogFact shipLogFact = _factRevealQueue[i];
				if (shipLogFact.HasSource() && shipLogFact.GetSourceID().Equals(_targetCard.GetEntry().GetID()))
				{
					ShipLogEntryLink link = GetLink(shipLogFact.GetSourceID(), shipLogFact.GetEntryID());
					if (link != null && link.IsRevealAnimationReady() && !_cardDict[shipLogFact.GetEntryID()].IsRevealAnimationReady())
					{
						link.PlayRevealAnimation(_panDuration);
					}
				}
			}
			_targetCard.PlayRevealAnimation();
			_oneShotSource.PlayOneShot(AudioType.ShipLogRevealEntry);
			_animWaitSeconds = 0.8f;
			_targetCard = null;
		}
		else if (_queueIndex < _factRevealQueue.Count)
		{
			ShipLogFact shipLogFact2 = _factRevealQueue[_queueIndex];
			ShipLogEntry entry = _manager.GetEntry(shipLogFact2.GetEntryID());
			ShipLogEntryCard shipLogEntryCard = _cardDict[entry.GetID()];
			if (shipLogEntryCard.IsRevealAnimationReady())
			{
				_targetCard = shipLogEntryCard;
				_startPanTime = Time.unscaledTime;
				_startPanPos = _panRoot.anchoredPosition;
				_startScale = _scaleRoot.localScale;
			}
			if (shipLogFact2.HasSource())
			{
				ShipLogEntryLink link2 = GetLink(shipLogFact2.GetSourceID(), shipLogFact2.GetEntryID());
				if (link2 != null && link2.IsRevealAnimationReady())
				{
					link2.PlayRevealAnimation(_panDuration);
				}
			}
			_queueIndex++;
		}
		else
		{
			FinishRevealAnimation();
		}
	}

	private void FinishRevealAnimation()
	{
		for (int i = 0; i < _factRevealQueue.Count; i++)
		{
			ShipLogFact shipLogFact = _factRevealQueue[i];
			ShipLogEntryCard shipLogEntryCard = _cardDict[shipLogFact.GetEntryID()];
			if (shipLogEntryCard.IsRevealAnimationReady())
			{
				shipLogEntryCard.PlayRevealAnimation();
			}
			if (shipLogFact.HasSource())
			{
				ShipLogEntryLink link = GetLink(shipLogFact.GetSourceID(), shipLogFact.GetEntryID());
				if (link != null && link.IsRevealAnimationReady())
				{
					link.PlayRevealAnimation(_panDuration);
				}
			}
		}
		_factRevealQueue.Clear();
		_updateRevealAnim = false;
		_targetCard = null;
		_updateFrameAll = true;
		_startPanTime = Time.unscaledTime;
		_startPanPos = _panRoot.anchoredPosition;
		_startScale = _scaleRoot.localScale;
	}

	private void GenerateEntryCards()
	{
		_cardList = new List<ShipLogEntryCard>();
		_cardDict = new Dictionary<string, ShipLogEntryCard>();
		List<ShipLogEntry> entryList = _manager.GetEntryList();
		for (int i = 0; i < entryList.Count; i++)
		{
			GameObject obj = Object.Instantiate(_entryCardTemplate, _entryCardTemplate.transform.parent);
			obj.name = entryList[i].GetID();
			ShipLogEntryCard component = obj.GetComponent<ShipLogEntryCard>();
			component.Init(entryList[i], _manager.GetEntryCardPosition(entryList[i].GetID()), _fontAndLanguageController);
			_cardList.Add(component);
			_cardDict.Add(entryList[i].GetID(), component);
		}
		Object.Destroy(_entryCardTemplate);
	}

	private void GenerateEntryLinks()
	{
		_linkList = new List<ShipLogEntryLink>();
		_linkDict = new Dictionary<LinkKey, ShipLogEntryLink>();
		for (int i = 0; i < _cardList.Count; i++)
		{
			ShipLogEntry entry = _cardList[i].GetEntry();
			List<ShipLogFact> rumorFacts = entry.GetRumorFacts();
			for (int j = 0; j < rumorFacts.Count; j++)
			{
				if (rumorFacts[j].HasSource())
				{
					ShipLogEntry entry2 = _manager.GetEntry(rumorFacts[j].GetSourceID());
					if (entry2 == null)
					{
						Debug.LogError("Failed to find source entry: " + rumorFacts[j].GetSourceID());
						Debug.Break();
					}
					ShipLogEntryCard sourceCard = _cardDict[entry2.GetID()];
					LinkKey key = new LinkKey(entry2.GetID(), entry.GetID());
					ShipLogEntryLink shipLogEntryLink = (_linkDict.ContainsKey(key) ? _linkDict[key] : null);
					if (shipLogEntryLink == null)
					{
						GameObject obj = Object.Instantiate(_entryLinkTemplate, _entryLinkTemplate.transform.parent);
						obj.name = "LINK_" + entry2.GetID() + "-->" + entry.GetID();
						shipLogEntryLink = obj.GetComponent<ShipLogEntryLink>();
						shipLogEntryLink.Init(sourceCard, _cardList[i], Application.isEditor && _editMode);
						_linkList.Add(shipLogEntryLink);
						_linkDict.Add(key, shipLogEntryLink);
					}
					shipLogEntryLink.AddRumorFact(rumorFacts[j]);
				}
			}
		}
		for (int k = 0; k < _linkList.Count; k++)
		{
			_linkList[k].UpdatePosition();
			_linkList[k].UpdateVisibility();
		}
		Object.Destroy(_entryLinkTemplate);
	}

	private ShipLogEntryLink GetLink(string sourceID, string targetID)
	{
		LinkKey key = new LinkKey(sourceID, targetID);
		if (_linkDict.ContainsKey(key))
		{
			return _linkDict[key];
		}
		return null;
	}
}
