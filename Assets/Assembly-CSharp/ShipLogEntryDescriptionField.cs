using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipLogEntryDescriptionField : MonoBehaviour
{
	private const int FACT_LIST_POOL_SIZE = 10;

	[SerializeField]
	private GameObject _factItemTemplate;

	[SerializeField]
	private RectTransform _listRoot;

	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private ScreenPromptList _scrollPromptList;

	[SerializeField]
	private GameObject _scrollPromptRoot;

	[Space(10f)]
	[SerializeField]
	private VerticalLayoutGroup _factListLayoutGroup;

	[SerializeField]
	private int _defaultFactListLayoutSpacing = 5;

	[SerializeField]
	private int _cjkFactListLayoutSpacing = 15;

	[Space(10f)]
	[SerializeField]
	private ShipLogSlideProjector _slideProjector;

	private float _origYPos;

	private int _displayCount;

	private ShipLogFactListItem[] _factListItems;

	private ShipLogEntry _entry;

	private ShipLogEntryLink _link;

	private CanvasGroupAnimator _animator;

	private bool _visible;

	private RectTransform _scrollPromptRootRectTransform;

	private int _scrollPromptFontSize;

	private ScreenPrompt _scrollPromptGamepad;

	private ScreenPrompt _scrollPromptKbm;

	private bool _hasScrolledView;

	private RectTransform _thisRectTransform;

	private bool _usingGamepad;

	private void Awake()
	{
		_animator = GetComponent<CanvasGroupAnimator>();
		_thisRectTransform = this.GetRequiredComponent<RectTransform>();
		_visible = false;
		_origYPos = _listRoot.anchoredPosition.y;
	}

	private void Start()
	{
		_factListItems = new ShipLogFactListItem[10];
		for (int i = 0; i < _factListItems.Length; i++)
		{
			GameObject gameObject = Object.Instantiate(_factItemTemplate, _factItemTemplate.transform.parent);
			gameObject.name = "FactListItem_" + i;
			_factListItems[i] = gameObject.GetComponent<ShipLogFactListItem>();
		}
		if (TextTranslation.Get().IsLanguageCJK())
		{
			_factListLayoutGroup.spacing = _cjkFactListLayoutSpacing;
		}
		else
		{
			_factListLayoutGroup.spacing = _defaultFactListLayoutSpacing;
		}
		Object.Destroy(_factItemTemplate);
		PromptManager promptManager = Locator.GetPromptManager();
		_scrollPromptList.Init();
		_scrollPromptGamepad = new ScreenPrompt(InputLibrary.scrollLogText, UITextLibrary.GetString(UITextType.LogScrollTextPrompt));
		_scrollPromptKbm = new ScreenPrompt(InputLibrary.toolOptionY, UITextLibrary.GetString(UITextType.LogScrollTextPrompt));
		_usingGamepad = OWInput.UsingGamepad();
		promptManager.AddScreenPrompt(_scrollPromptGamepad, _scrollPromptList, TextAnchor.MiddleCenter, -1, _usingGamepad);
		promptManager.AddScreenPrompt(_scrollPromptKbm, _scrollPromptList, TextAnchor.MiddleCenter, -1, !_usingGamepad);
		_scrollPromptRoot.SetActive(value: false);
		_animator.SetImmediate(0f, new Vector3(1f, 0f, 1f));
		base.enabled = false;
	}

	private void OnDisable()
	{
		if (_audioSource.isPlaying)
		{
			_audioSource.Stop();
		}
	}

	private void OnDestroy()
	{
		if (Locator.GetPromptManager() != null)
		{
			Locator.GetPromptManager().RemoveScreenPrompt(_scrollPromptGamepad);
			Locator.GetPromptManager().RemoveScreenPrompt(_scrollPromptKbm);
		}
	}

	public void SetText(string text)
	{
		ResetListPos();
		_link = null;
		_entry = null;
		_displayCount = 1;
		_factListItems[0].DisplayText(text);
		for (int i = 1; i < 10; i++)
		{
			_factListItems[i].Clear();
		}
		base.enabled = false;
	}

	public void SetEntry(ShipLogEntry entry)
	{
		if (entry != _entry)
		{
			ResetListPos();
		}
		_link = null;
		_entry = entry;
		List<ShipLogFact> factsForDisplay = _entry.GetFactsForDisplay();
		DisplayFacts(factsForDisplay);
		if (_entry.HasMoreToExplore())
		{
			_factListItems[_displayCount].DisplayText(UITextLibrary.GetString(UITextType.ShipLogMoreThere));
			_displayCount++;
		}
		AttemptStartTextReveal();
		_slideProjector.CheckFactsForSlides(factsForDisplay, entry);
		if (_visible)
		{
			_slideProjector.AttemptSetVisible(visible: true);
		}
	}

	public void SetLink(ShipLogEntryLink link)
	{
		ResetListPos();
		_entry = null;
		_link = link;
		List<ShipLogFact> factsForDisplay = _link.GetFactsForDisplay();
		DisplayFacts(factsForDisplay);
		AttemptStartTextReveal();
		_slideProjector.CheckFactsForSlides(factsForDisplay);
	}

	public bool IsVisible()
	{
		return _visible;
	}

	public string GetDisplayedEntryID(bool includeLinks = true)
	{
		if (_visible)
		{
			if (_entry != null)
			{
				return _entry.GetID();
			}
			if (_link != null && includeLinks)
			{
				return _link.GetTargetEntryCard().GetEntry().GetID();
			}
		}
		return "";
	}

	public void SetVisible(bool visible)
	{
		if (visible != _visible)
		{
			_visible = visible;
			_animator.AnimateTo(_visible ? 1 : 0, _visible ? Vector3.one : new Vector3(1f, 0f, 1f), 0.1f);
			base.enabled = _visible;
			AttemptStartTextReveal();
			_slideProjector.AttemptSetVisible(visible);
		}
	}

	private void DisplayFacts(List<ShipLogFact> facts)
	{
		_displayCount = 0;
		for (int i = 0; i < 10; i++)
		{
			if (facts.Count > i)
			{
				_factListItems[i].DisplayFact(facts[i]);
				_displayCount++;
			}
			else
			{
				_factListItems[i].Clear();
			}
		}
	}

	private void ResetListPos()
	{
		SetListYPos(_origYPos);
		_hasScrolledView = false;
		_scrollPromptRoot.SetActive(value: false);
	}

	private void SetListYPos(float yPos)
	{
		Vector2 anchoredPosition = _listRoot.anchoredPosition;
		float num = Mathf.Min(0f, GetListBottomPos() + _thisRectTransform.rect.height - _factListLayoutGroup.spacing);
		anchoredPosition.y = Mathf.Clamp(yPos, _origYPos, _origYPos - num);
		_listRoot.anchoredPosition = anchoredPosition;
	}

	private float GetListBottomPos()
	{
		if (_displayCount <= 0)
		{
			return 0f;
		}
		return _factListItems[_displayCount - 1].GetPosition().y - _factListItems[_displayCount - 1].GetHeight();
	}

	private void Update()
	{
		if (_usingGamepad != OWInput.UsingGamepad())
		{
			_usingGamepad = !_usingGamepad;
		}
		float y = _listRoot.anchoredPosition.y;
		float num = 0f;
		num = ((!_usingGamepad) ? (OWInput.GetValue(InputLibrary.toolOptionY) * Time.unscaledDeltaTime * 300f) : (OWInput.GetValue(InputLibrary.scrollLogText) * Time.unscaledDeltaTime * 300f));
		y -= num;
		SetListYPos(y);
		if (!_hasScrolledView)
		{
			bool flag = 0f - _listRoot.anchoredPosition.y > GetListBottomPos() + _thisRectTransform.rect.height;
			bool flag2 = Mathf.Abs(_listRoot.anchoredPosition.y - _origYPos) > 0.1f;
			_scrollPromptRoot.SetActive(flag && !flag2);
			SetScrollPromptVisibility(_scrollPromptRoot.activeSelf);
			if (flag2)
			{
				_hasScrolledView = true;
			}
		}
		bool flag3 = false;
		for (int i = 0; i < _factListItems.Length; i++)
		{
			if (_factListItems[i].UpdateTextReveal())
			{
				flag3 = true;
			}
		}
		if (flag3 && !_audioSource.isPlaying)
		{
			_audioSource.Play();
		}
		else if (!flag3 && _audioSource.isPlaying)
		{
			_audioSource.Stop();
		}
	}

	private void SetScrollPromptVisibility(bool enable)
	{
		if (enable)
		{
			_scrollPromptGamepad.SetVisibility(_usingGamepad);
			_scrollPromptKbm.SetVisibility(!_usingGamepad);
		}
		else
		{
			_scrollPromptGamepad.SetVisibility(isVisible: false);
		}
	}

	private void AttemptStartTextReveal()
	{
		if (_visible)
		{
			for (int i = 0; i < _factListItems.Length; i++)
			{
				_factListItems[i].StartTextReveal();
			}
		}
	}
}
