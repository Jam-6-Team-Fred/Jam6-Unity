using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PromptManager : MonoBehaviour
{
	public delegate void UpdatePromptImageEvent();

	[SerializeField]
	private Font _font;

	[Header("Default Size Settings")]
	[SerializeField]
	private int _fontSize;

	[SerializeField]
	private float _promptElementMinWidth;

	[SerializeField]
	private float _promptElementMinHeight;

	[FormerlySerializedAs("_fontSizeConsole")]
	[SerializeField]
	private int _fontSizeLarge;

	[FormerlySerializedAs("_promptElementMinWidthConsole")]
	[SerializeField]
	private float _promptElementMinWidthLarge;

	[FormerlySerializedAs("_promptElementMinHeightConsole")]
	[SerializeField]
	private float _promptElementMinHeightLarge;

	[Header("Screen Prompt Lists")]
	[SerializeField]
	private ScreenPromptList _bottomLeftList;

	[SerializeField]
	private ScreenPromptList _centerList;

	[SerializeField]
	private ScreenPromptList _topRightList;

	[SerializeField]
	private ScreenPromptList _topLeftList;

	[SerializeField]
	private ScreenPromptList _bottomCenterList;

	[SerializeField]
	private ScreenPromptList _boostPromptList;

	[Header("Opacity Control CanvasGroups")]
	[SerializeField]
	private float _alphaValWithNotifications;

	[SerializeField]
	private CanvasGroup[] _opacityControlGroups;

	private List<ScreenPromptList> _worldPromptLists;

	private bool _promptsVisible = true;

	private bool _photoScreenVisible;

	private ProbeLauncher.Name _probeLauncherName;

	private Vector2 _centerPromptScreenPosition;

	private ScreenPrompt _activeCenterPrompt;

	private List<ScreenPrompt> _screenPromptsRequestingRefresh;

	private List<ScreenPrompt> _screenPromptsRequestingVisibilityRefresh;

	private List<UiSizeSetterScreenPromptList> _screenPromptListSizeSetters;

	private Font _currentFont;

	private int _currentFontSize;

	private bool _usingGamepad;

	private float _lastButtonImageRefresh;

	private bool _disableEssentialPromptLists;

	private bool _uiSizeInitialized;

	public static event UpdatePromptImageEvent OnUpdatePromptImages;

	private void Awake()
	{
		_screenPromptsRequestingRefresh = new List<ScreenPrompt>();
		_screenPromptsRequestingVisibilityRefresh = new List<ScreenPrompt>();
		_screenPromptListSizeSetters = new List<UiSizeSetterScreenPromptList>(GetComponentsInChildren<UiSizeSetterScreenPromptList>());
		Canvas.willRenderCanvases += OnWillRenderCanvases;
		OnUpdatePromptImages += PromptManager.OnUpdatePromptImages;
		ButtonPromptLibrary.OnUpdateButtonPromptConfig += OnButtonImagesChanged;
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherEquipped", OnProbeLauncherEquipped);
		GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherUnequipped", OnProbeLauncherUnequipped);
		GlobalMessenger<ProbeCamera>.AddListener("ProbeSnapshot", OnProbeSnapshot);
		GlobalMessenger.AddListener("Probe Snapshot Removed", OnProbeSnapshotRemoved);
		GlobalMessenger.AddListener("ChangeGUIMode", OnChangeGUIMode);
		OWInput.SharedInputManager.OnUpdateInputCommands += OnKeyBindingsChanged;
		_worldPromptLists = new List<ScreenPromptList>();
		if (TextTranslation.Get().IsLanguageLatin())
		{
			_currentFont = _font;
		}
		else
		{
			_currentFont = TextTranslation.GetFont();
		}
		TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
		int value = 0;
		if (SecretSettings.TryGetInt("DisableEssentialPrompts", out value) && value == 1)
		{
			_disableEssentialPromptLists = true;
		}
		else
		{
			_disableEssentialPromptLists = false;
		}
	}

	private void Start()
	{
		if (PlayerData.IsLoaded())
		{
			SetPromptListSize();
		}
		_usingGamepad = OWInput.UsingGamepad();
		UpdatePromptsEnabled(PlayerData.GetPromptsEnabled());
	}

	private void OnButtonImagesChanged()
	{
		RebuildUI();
	}

	public void OnChangePromptSize()
	{
		SetPromptListSize();
		RebuildUI();
	}

	private void SetPromptListSize()
	{
		foreach (UiSizeSetterScreenPromptList screenPromptListSizeSetter in _screenPromptListSizeSetters)
		{
			if (screenPromptListSizeSetter.readyForResize)
			{
				screenPromptListSizeSetter.DoResizeAction(PlayerData.GetTextSize());
			}
		}
		_uiSizeInitialized = true;
	}

	private void Update()
	{
		if (!_uiSizeInitialized && PlayerData.IsLoaded() && !PlayerData.IsBusy())
		{
			SetPromptListSize();
		}
		if (OWInput.UsingGamepad() != _usingGamepad && Time.time - _lastButtonImageRefresh > ButtonPromptLibrary.BUTTONIMAGE_MIN_REFRESH_TIME)
		{
			_lastButtonImageRefresh = Time.time;
			_usingGamepad = !_usingGamepad;
			RebuildUI();
			if (PromptManager.OnUpdatePromptImages != null)
			{
				PromptManager.OnUpdatePromptImages();
			}
		}
		float alpha = 1f;
		if (PlayerData.IsUILargeTextSize())
		{
			bool flag = NotificationManager.SharedInstance.AreNotificationsVisible(NotificationTarget.Player) && Locator.GetPlayerSuit().IsWearingHelmet();
			if (!flag)
			{
				flag = NotificationManager.SharedInstance.AreNotificationsVisible(NotificationTarget.Ship) && PlayerState.AtFlightConsole();
			}
			if (flag)
			{
				alpha = _alphaValWithNotifications;
			}
		}
		for (int i = 0; i < _opacityControlGroups.Length; i++)
		{
			_opacityControlGroups[i].alpha = alpha;
		}
	}

	private void OnWillRenderCanvases()
	{
		if (_screenPromptsRequestingRefresh.Count > 0)
		{
			for (int i = 0; i < _screenPromptsRequestingRefresh.Count; i++)
			{
				RefreshOnWillRender(_screenPromptsRequestingRefresh[i]);
			}
			_screenPromptsRequestingRefresh.Clear();
		}
		if (_screenPromptsRequestingVisibilityRefresh.Count > 0)
		{
			for (int j = 0; j < _screenPromptsRequestingVisibilityRefresh.Count; j++)
			{
				RefreshVisibilityOnWillRender(_screenPromptsRequestingVisibilityRefresh[j]);
			}
			_screenPromptsRequestingVisibilityRefresh.Clear();
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger<ProbeCamera>.RemoveListener("ProbeSnapshot", OnProbeSnapshot);
		GlobalMessenger.RemoveListener("Probe Snapshot Removed", OnProbeSnapshotRemoved);
		GlobalMessenger<ProbeLauncher>.RemoveListener("ProbeLauncherEquipped", OnProbeLauncherEquipped);
		GlobalMessenger<ProbeLauncher>.RemoveListener("ProbeLauncherUnequipped", OnProbeLauncherUnequipped);
		GlobalMessenger.RemoveListener("ChangeGUIMode", OnChangeGUIMode);
		OWInput.SharedInputManager.OnUpdateInputCommands -= OnKeyBindingsChanged;
		ButtonPromptLibrary.OnUpdateButtonPromptConfig -= OnButtonImagesChanged;
		OnUpdatePromptImages -= PromptManager.OnUpdatePromptImages;
		TextTranslation.Get().OnLanguageChanged -= OnLanguageChanged;
		Canvas.willRenderCanvases -= OnWillRenderCanvases;
	}

	private void OnLanguageChanged()
	{
		if (TextTranslation.Get().IsLanguageLatin())
		{
			_currentFont = _font;
		}
		else
		{
			_currentFont = TextTranslation.GetFont();
		}
		RebuildUI();
	}

	public void SetPromptsVisible(bool visible)
	{
		_promptsVisible = visible;
		UpdatePromptsEnabled(PlayerData.GetPromptsEnabled());
	}

	public void UpdatePromptsEnabled(bool promptsOptionEnabled)
	{
		bool active = !_disableEssentialPromptLists && _promptsVisible;
		_centerList.gameObject.SetActive(active);
		_bottomLeftList.gameObject.SetActive(active);
		_boostPromptList.gameObject.SetActive(active);
		for (int i = 0; i < _worldPromptLists.Count; i++)
		{
			_worldPromptLists[i].gameObject.SetActive(active);
		}
		bool active2 = promptsOptionEnabled && _promptsVisible;
		_topRightList.gameObject.SetActive(active2);
		_topLeftList.gameObject.SetActive(active2);
		_bottomCenterList.gameObject.SetActive(active2);
	}

	public ScreenPromptList GetScreenPromptList(PromptPosition listPosition)
	{
		switch (listPosition)
		{
		case PromptPosition.Center:
			return _centerList;
		case PromptPosition.LowerLeft:
			return _bottomLeftList;
		case PromptPosition.UpperLeft:
			return _topLeftList;
		case PromptPosition.UpperRight:
			return _topRightList;
		case PromptPosition.BottomCenter:
			return _bottomCenterList;
		case PromptPosition.BoostMeter:
			return _boostPromptList;
		default:
			return null;
		}
	}

	public TextAnchor GetTextAnchor(PromptPosition listPosition)
	{
		switch (listPosition)
		{
		case PromptPosition.Center:
			return TextAnchor.MiddleCenter;
		case PromptPosition.LowerLeft:
			return TextAnchor.MiddleLeft;
		case PromptPosition.UpperLeft:
			return TextAnchor.MiddleLeft;
		case PromptPosition.UpperRight:
			return TextAnchor.MiddleRight;
		case PromptPosition.BottomCenter:
			return TextAnchor.MiddleCenter;
		case PromptPosition.BoostMeter:
			return TextAnchor.MiddleCenter;
		default:
			return TextAnchor.MiddleCenter;
		}
	}

	public void AddScreenPrompt(ScreenPrompt buttonPrompt, bool makeVisible = false)
	{
		AddScreenPrompt(buttonPrompt, PromptPosition.LowerLeft, makeVisible);
	}

	public ScreenPromptElement AddScreenPrompt(ScreenPrompt buttonPrompt, ScreenPromptList list, TextAnchor elementAlignment, int fontSize = -1, bool makeVisible = false)
	{
		buttonPrompt.SetVisibility(makeVisible);
		buttonPrompt.SetScrollOnVisible(value: false);
		int fontSize2 = list.GetFontSize();
		fontSize2 = Mathf.CeilToInt((float)fontSize2 * TextTranslation.GetFontSizeModifier());
		GameObject obj = ScreenPromptElement.CreateNewScreenPrompt(buttonPrompt, fontSize2, _currentFont, list.transform, elementAlignment);
		obj.name = "ScreenPrompt";
		ScreenPromptElement requiredComponent = obj.GetRequiredComponent<ScreenPromptElement>();
		list.AddScreenPrompt(requiredComponent);
		if (!_worldPromptLists.Contains(list))
		{
			_worldPromptLists.Add(list);
		}
		return requiredComponent;
	}

	public void AddScreenPrompt(ScreenPrompt buttonPrompt, PromptPosition promptPosition, bool makeVisible = false)
	{
		buttonPrompt.SetVisibility(makeVisible);
		ScreenPromptList screenPromptList = GetScreenPromptList(promptPosition);
		if (screenPromptList != null)
		{
			buttonPrompt.SetScrollOnVisible(value: false);
			int fontSize = screenPromptList.GetFontSize();
			GameObject obj = ScreenPromptElement.CreateNewScreenPrompt(buttonPrompt, fontSize, _currentFont, screenPromptList.transform, GetTextAnchor(promptPosition));
			obj.name = "ScreenPrompt";
			ScreenPromptElement requiredComponent = obj.GetRequiredComponent<ScreenPromptElement>();
			screenPromptList.AddScreenPrompt(requiredComponent);
		}
	}

	public void RemoveScreenPrompt(ScreenPrompt buttonPrompt, PromptPosition promptPosition)
	{
		ScreenPromptList screenPromptList = GetScreenPromptList(promptPosition);
		if (screenPromptList != null && screenPromptList.Contains(buttonPrompt))
		{
			screenPromptList.RemoveScreenPrompt(buttonPrompt);
		}
	}

	public void RemoveScreenPrompt(ScreenPrompt buttonPrompt)
	{
		if (_bottomLeftList.Contains(buttonPrompt))
		{
			_bottomLeftList.RemoveScreenPrompt(buttonPrompt);
		}
		if (_centerList.Contains(buttonPrompt))
		{
			_centerList.RemoveScreenPrompt(buttonPrompt);
		}
		if (_topRightList.Contains(buttonPrompt))
		{
			_topRightList.RemoveScreenPrompt(buttonPrompt);
		}
		if (_topLeftList.Contains(buttonPrompt))
		{
			_topLeftList.RemoveScreenPrompt(buttonPrompt);
		}
		if (_bottomCenterList.Contains(buttonPrompt))
		{
			_bottomCenterList.RemoveScreenPrompt(buttonPrompt);
		}
		if (_boostPromptList.Contains(buttonPrompt))
		{
			_boostPromptList.RemoveScreenPrompt(buttonPrompt);
		}
		for (int i = 0; i < _worldPromptLists.Count; i++)
		{
			if (_worldPromptLists[i].Contains(buttonPrompt))
			{
				_worldPromptLists[i].RemoveScreenPrompt(buttonPrompt);
			}
		}
	}

	public int GetHighestPriorityApplicableToPrompt(ScreenPrompt prompt)
	{
		if (_bottomLeftList.Contains(prompt))
		{
			return _bottomLeftList.GetHighestVisiblePriority();
		}
		if (_centerList.Contains(prompt))
		{
			return _centerList.GetHighestVisiblePriority();
		}
		if (_topRightList.Contains(prompt))
		{
			return _topRightList.GetHighestVisiblePriority();
		}
		if (_topLeftList.Contains(prompt))
		{
			return _topLeftList.GetHighestVisiblePriority();
		}
		if (_bottomCenterList.Contains(prompt))
		{
			return _bottomCenterList.GetHighestVisiblePriority();
		}
		if (_boostPromptList.Contains(prompt))
		{
			return _boostPromptList.GetHighestVisiblePriority();
		}
		for (int i = 0; i < _worldPromptLists.Count; i++)
		{
			if (_worldPromptLists[i].Contains(prompt))
			{
				return _worldPromptLists[i].GetHighestVisiblePriority();
			}
		}
		return 0;
	}

	public void UpdateText(ScreenPrompt prompt, string updatedText)
	{
		if (_bottomLeftList.Contains(prompt))
		{
			_bottomLeftList.OnUpdateScreenPromptText(prompt, updatedText);
		}
		if (_centerList.Contains(prompt))
		{
			_centerList.OnUpdateScreenPromptText(prompt, updatedText);
		}
		if (_topRightList.Contains(prompt))
		{
			_topRightList.OnUpdateScreenPromptText(prompt, updatedText);
		}
		if (_topLeftList.Contains(prompt))
		{
			_topLeftList.OnUpdateScreenPromptText(prompt, updatedText);
		}
		if (_bottomCenterList.Contains(prompt))
		{
			_bottomCenterList.OnUpdateScreenPromptText(prompt, updatedText);
		}
		if (_boostPromptList.Contains(prompt))
		{
			_boostPromptList.OnUpdateScreenPromptText(prompt, updatedText);
		}
		for (int i = 0; i < _worldPromptLists.Count; i++)
		{
			if (_worldPromptLists[i].Contains(prompt))
			{
				_worldPromptLists[i].OnUpdateScreenPromptText(prompt, updatedText);
			}
		}
		TriggerRefresh(prompt);
	}

	public void TriggerRebuild(ScreenPrompt prompt)
	{
		if (_bottomLeftList.Contains(prompt))
		{
			_bottomLeftList.RebuildScreenPromptUIElement(prompt);
		}
		if (_centerList.Contains(prompt))
		{
			_centerList.RebuildScreenPromptUIElement(prompt);
		}
		if (_topRightList.Contains(prompt))
		{
			_topRightList.RebuildScreenPromptUIElement(prompt);
		}
		if (_topLeftList.Contains(prompt))
		{
			_topLeftList.RebuildScreenPromptUIElement(prompt);
		}
		if (_bottomCenterList.Contains(prompt))
		{
			_bottomCenterList.RebuildScreenPromptUIElement(prompt);
		}
		if (_boostPromptList.Contains(prompt))
		{
			_boostPromptList.RebuildScreenPromptUIElement(prompt);
		}
		for (int i = 0; i < _worldPromptLists.Count; i++)
		{
			if (_worldPromptLists[i].Contains(prompt))
			{
				_worldPromptLists[i].RebuildScreenPromptUIElement(prompt);
			}
		}
		TriggerRefresh(prompt);
	}

	public void TriggerVisibilityRefresh(ScreenPrompt prompt)
	{
		_screenPromptsRequestingVisibilityRefresh.Add(prompt);
	}

	public void TriggerRefresh(ScreenPrompt prompt)
	{
		_screenPromptsRequestingRefresh.Add(prompt);
	}

	private void RefreshOnWillRender(ScreenPrompt prompt)
	{
		if (_bottomLeftList.Contains(prompt))
		{
			_bottomLeftList.RefreshScreenPromptUIElement(prompt);
		}
		if (_centerList.Contains(prompt))
		{
			_centerList.RefreshScreenPromptUIElement(prompt);
		}
		if (_topRightList.Contains(prompt))
		{
			_topRightList.RefreshScreenPromptUIElement(prompt);
		}
		if (_topLeftList.Contains(prompt))
		{
			_topLeftList.RefreshScreenPromptUIElement(prompt);
		}
		if (_bottomCenterList.Contains(prompt))
		{
			_bottomCenterList.RefreshScreenPromptUIElement(prompt);
		}
		if (_boostPromptList.Contains(prompt))
		{
			_boostPromptList.RefreshScreenPromptUIElement(prompt);
		}
		for (int i = 0; i < _worldPromptLists.Count; i++)
		{
			if (_worldPromptLists[i].Contains(prompt))
			{
				_worldPromptLists[i].RefreshScreenPromptUIElement(prompt);
			}
		}
	}

	private void RefreshVisibilityOnWillRender(ScreenPrompt prompt)
	{
		if (_bottomLeftList.Contains(prompt))
		{
			_bottomLeftList.RefreshScreenPromptUIElementVisibility(prompt);
		}
		if (_centerList.Contains(prompt))
		{
			_centerList.RefreshScreenPromptUIElementVisibility(prompt);
		}
		if (_topRightList.Contains(prompt))
		{
			_topRightList.RefreshScreenPromptUIElementVisibility(prompt);
		}
		if (_topLeftList.Contains(prompt))
		{
			_topLeftList.RefreshScreenPromptUIElementVisibility(prompt);
		}
		if (_bottomCenterList.Contains(prompt))
		{
			_bottomCenterList.RefreshScreenPromptUIElementVisibility(prompt);
		}
		if (_boostPromptList.Contains(prompt))
		{
			_boostPromptList.RefreshScreenPromptUIElementVisibility(prompt);
		}
		for (int i = 0; i < _worldPromptLists.Count; i++)
		{
			if (_worldPromptLists[i].Contains(prompt))
			{
				_worldPromptLists[i].RefreshScreenPromptUIElementVisibility(prompt);
			}
		}
	}

	public bool IsCenterPromptDisplayed()
	{
		return _centerList.IsDisplayingAnyPrompt();
	}

	private void OnAddScreenPrompt(ScreenPrompt buttonPrompt, PromptPosition promptPosition)
	{
		AddScreenPrompt(buttonPrompt, promptPosition);
	}

	private void OnRemoveScreenPrompt(ScreenPrompt buttonPrompt)
	{
		RemoveScreenPrompt(buttonPrompt);
	}

	public void RebuildUI()
	{
		_bottomLeftList.RebuildAllScreenPromptUIElements();
		_centerList.RebuildAllScreenPromptUIElements();
		_topRightList.RebuildAllScreenPromptUIElements();
		_topLeftList.RebuildAllScreenPromptUIElements();
		_bottomCenterList.RebuildAllScreenPromptUIElements();
		_boostPromptList.RebuildAllScreenPromptUIElements();
		for (int i = 0; i < _worldPromptLists.Count; i++)
		{
			_worldPromptLists[i].RebuildAllScreenPromptUIElements();
		}
	}

	private void OnPlayerDeath(DeathType deathType)
	{
	}

	private void OnProbeSnapshot(ProbeCamera camera)
	{
		bool flag = camera.GetID() == ProbeCamera.ID.PreLaunch && camera.GetAttachedOWRigidbody() is PlayerBody;
		if ((!_photoScreenVisible && _probeLauncherName == ProbeLauncher.Name.Player) || flag)
		{
			RectTransform requiredComponent = _topRightList.GetRequiredComponent<RectTransform>();
			Vector2 anchoredPosition = requiredComponent.anchoredPosition;
			anchoredPosition.y = -250f;
			requiredComponent.anchoredPosition = anchoredPosition;
			_photoScreenVisible = true;
		}
	}

	private void OnProbeSnapshotRemoved()
	{
		if (_photoScreenVisible && _probeLauncherName == ProbeLauncher.Name.Player)
		{
			RectTransform requiredComponent = _topRightList.GetRequiredComponent<RectTransform>();
			Vector2 anchoredPosition = requiredComponent.anchoredPosition;
			anchoredPosition.y = 0f;
			requiredComponent.anchoredPosition = anchoredPosition;
			_photoScreenVisible = false;
		}
	}

	private void OnProbeLauncherEquipped(ProbeLauncher launcher)
	{
		_probeLauncherName = launcher.GetName();
		if (!_photoScreenVisible && _probeLauncherName == ProbeLauncher.Name.Ship)
		{
			RectTransform requiredComponent = _topRightList.GetRequiredComponent<RectTransform>();
			Vector2 anchoredPosition = requiredComponent.anchoredPosition;
			anchoredPosition.y = -250f;
			requiredComponent.anchoredPosition = anchoredPosition;
			_photoScreenVisible = true;
		}
	}

	private void OnProbeLauncherUnequipped(ProbeLauncher launcher)
	{
		if (_photoScreenVisible && _probeLauncherName == ProbeLauncher.Name.Ship)
		{
			RectTransform requiredComponent = _topRightList.GetRequiredComponent<RectTransform>();
			Vector2 anchoredPosition = requiredComponent.anchoredPosition;
			anchoredPosition.y = 0f;
			requiredComponent.anchoredPosition = anchoredPosition;
			_photoScreenVisible = false;
		}
	}

	private void OnChangeGUIMode()
	{
		bool active = true;
		if (GUIMode.IsHiddenMode())
		{
			active = false;
		}
		if (_bottomLeftList != null)
		{
			_bottomLeftList.gameObject.SetActive(active);
		}
		if (_centerList != null)
		{
			_centerList.gameObject.SetActive(active);
		}
		if (_topRightList != null)
		{
			_topRightList.gameObject.SetActive(active);
		}
		if (_topLeftList != null)
		{
			_topLeftList.gameObject.SetActive(active);
		}
		if (_bottomCenterList != null)
		{
			_bottomCenterList.gameObject.SetActive(active);
		}
		if (_boostPromptList != null)
		{
			_boostPromptList.gameObject.SetActive(active);
		}
		if (_worldPromptLists != null)
		{
			for (int i = 0; i < _worldPromptLists.Count; i++)
			{
				_worldPromptLists[i].gameObject.SetActive(active);
			}
		}
	}

	protected virtual void OnKeyBindingsChanged()
	{
		RebuildUI();
	}
}
