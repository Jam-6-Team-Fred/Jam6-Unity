using UnityEngine;
using UnityEngine.UI;

public class ShipLogEntryListItem : MonoBehaviour
{
	private const float UNFOCUSED_ALPHA = 0.2f;

	private const float HASPARENT_INDENT_OFFSET = 30f;

	[SerializeField]
	private RectTransform _animRoot;

	[SerializeField]
	private RectTransform _iconRoot;

	[SerializeField]
	private RectTransform _selectionArrowAnchor;

	[SerializeField]
	private LayoutElement _indentationElement;

	[SerializeField]
	private CanvasRenderer _borderLine;

	[SerializeField]
	private Text _nameField;

	[SerializeField]
	private Image _unreadIcon;

	[SerializeField]
	private Image _hudMarkerIcon;

	[SerializeField]
	private Image _moreToExploreIcon;

	[SerializeField]
	private UiSizeSetterShipLogEntry _uiSizeSetter;

	[SerializeField]
	private LayoutElement _rootLayoutElement;

	private ShipLogEntry _entry;

	private RectTransform _rectTransform;

	private bool _hasFocus;

	private float _animStartTime;

	private Vector2 _startPos;

	private Vector2 _targetPos;

	private float _startIndent;

	private float _targetIndent;

	private float _targetAlpha;

	private float _startAlpha;

	private float _animDuration;

	private float _animAlpha = 1f;

	private float _listAlpha = 1f;

	private float _focusAlpha;

	private Color _origNameColor;

	public void Init()
	{
		_rectTransform = GetComponent<RectTransform>();
		_hasFocus = false;
		_origNameColor = _nameField.color;
		_nameField.font = Locator.GetUIStyleManager().GetShipLogFont();
		base.enabled = false;
	}

	public void Setup(ShipLogEntry entry, float appearDelay)
	{
		_entry = entry;
		UpdateNameField();
		_unreadIcon.gameObject.SetActive(value: false);
		_hudMarkerIcon.gameObject.SetActive(value: false);
		_moreToExploreIcon.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: true);
		_animRoot.anchoredPosition = new Vector2(-10f, 0f);
		_animAlpha = 0f;
		_hasFocus = false;
		_focusAlpha = 0.2f;
		UpdateAlpha();
		_uiSizeSetter.MarkReadyForInitialization();
		_uiSizeSetter.DoResizeAction(PlayerData.GetTextSize());
		AnimateTo(1f, GetEntryIndentation(), 0.05f, appearDelay);
	}

	public void Reset()
	{
		_entry = null;
		_hasFocus = false;
		base.gameObject.SetActive(value: false);
	}

	public bool IsSubEntry()
	{
		return _entry.HasRevealedParent();
	}

	public Vector3 GetPosition()
	{
		return _rectTransform.position;
	}

	public Vector2 GetAnchoredPosition()
	{
		return _rectTransform.anchoredPosition;
	}

	public Vector3 GetSelectionArrowPosition()
	{
		return _selectionArrowAnchor.position;
	}

	public ShipLogEntry GetEntry()
	{
		return _entry;
	}

	public Text GetNameField()
	{
		return _nameField;
	}

	public LayoutElement GetRootLayoutElement()
	{
		return _rootLayoutElement;
	}

	public void SetMarkedOnHUD(bool markedOnHUD)
	{
		_hudMarkerIcon.gameObject.SetActive(markedOnHUD);
	}

	public void SetListAlpha(float listAlpha)
	{
		_listAlpha = listAlpha;
		UpdateAlpha();
	}

	public void SetFocus(bool hasFocus)
	{
		if (_entry != null && hasFocus != _hasFocus)
		{
			_hasFocus = hasFocus;
			_focusAlpha = (_hasFocus ? 1f : 0.2f);
			UpdateAlpha();
		}
	}

	public void MarkAsRead()
	{
		_entry.MarkAsRead();
		UpdateNameField();
		_unreadIcon.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		float num = Mathf.InverseLerp(_animStartTime, _animStartTime + _animDuration, Time.unscaledTime);
		_animAlpha = Mathf.Lerp(_startAlpha, _targetAlpha, num);
		UpdateAlpha();
		_indentationElement.minWidth = Mathf.Lerp(_startIndent, _targetIndent, num);
		if (num >= 1f)
		{
			base.enabled = false;
			_unreadIcon.gameObject.SetActive(_entry.HasUnreadFacts());
			_moreToExploreIcon.gameObject.SetActive(_entry.HasMoreToExplore());
			_iconRoot.anchoredPosition = new Vector2(_nameField.rectTransform.sizeDelta.x * _nameField.rectTransform.localScale.x + 2f, 0f);
		}
	}

	private void AnimateTo(float alpha, float indent, float duration, float delay = 0f)
	{
		_startAlpha = _nameField.canvasRenderer.GetAlpha();
		_targetAlpha = alpha;
		_startIndent = 0f;
		_targetIndent = indent;
		_animStartTime = Time.unscaledTime + delay;
		_animDuration = duration;
		base.enabled = true;
	}

	private void UpdateNameField()
	{
		_nameField.text = _entry.GetName(withLineBreaks: false);
		_nameField.color = ((_entry.GetState() == ShipLogEntry.State.Rumored) ? Locator.GetUIStyleManager().GetShipLogRumorColor() : _origNameColor);
	}

	private void UpdateAlpha()
	{
		_nameField.canvasRenderer.SetAlpha(_animAlpha * _listAlpha * _focusAlpha);
		_unreadIcon.canvasRenderer.SetAlpha(_animAlpha * _listAlpha * _focusAlpha);
		_moreToExploreIcon.canvasRenderer.SetAlpha(_animAlpha * _listAlpha * _focusAlpha);
		_hudMarkerIcon.canvasRenderer.SetAlpha(_animAlpha * _listAlpha * _focusAlpha);
		_borderLine.SetAlpha(_animAlpha * _listAlpha);
	}

	private float GetEntryIndentation()
	{
		if (!_entry.HasRevealedParent())
		{
			return 0f;
		}
		return 30f;
	}
}
