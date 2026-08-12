using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
public class ShipLogEntryCard : MonoBehaviour, IShipLogSelectable
{
	[SerializeField]
	private Image _border;

	[SerializeField]
	private Image _nameBackground;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private Image _photo;

	[SerializeField]
	private Text _name;

	[SerializeField]
	private Text _questionMark;

	[SerializeField]
	private Image _unreadIcon;

	[SerializeField]
	private Image _hudMarkerIcon;

	[SerializeField]
	private Image _moreToExploreIcon;

	private ShipLogEntry _entry;

	private RectTransform _rectTransform;

	private float _startAnimTime;

	private bool _isRevealAnimReady;

	private Color _borderColor;

	private Color _origBorderColor;

	private Vector2 _origIconSize;

	private float _size = 1f;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		base.enabled = false;
	}

	public Vector2 GetAnchoredPosition()
	{
		return _rectTransform.anchoredPosition;
	}

	public bool IsVisible()
	{
		return base.gameObject.activeSelf;
	}

	public Vector2 GetEdgeIntersection(Vector2 outsidePos)
	{
		Vector2 normalized = (outsidePos - _rectTransform.anchoredPosition).normalized;
		float num = _rectTransform.sizeDelta.x * 0.5f * _size;
		float num2 = _rectTransform.sizeDelta.y * 0.5f * _size;
		if (Mathf.Abs(normalized.y / normalized.x) < Mathf.Abs(num2 / num))
		{
			float num3 = Mathf.Sign(normalized.x) * num;
			float y = normalized.y / normalized.x * num3;
			return _rectTransform.anchoredPosition + new Vector2(num3, y);
		}
		float num4 = Mathf.Sign(normalized.y) * num2;
		float x = normalized.x / normalized.y * num4;
		return _rectTransform.anchoredPosition + new Vector2(x, num4);
	}

	public void Init(ShipLogEntry entry, Vector2 position, FontAndLanguageController fontAndLangCtrl)
	{
		_entry = entry;
		_rectTransform.anchoredPosition = position;
		_isRevealAnimReady = false;
		_name.text = _entry.GetName(withLineBreaks: true);
		_name.font = Locator.GetUIStyleManager().GetShipLogCardFont();
		_name.lineSpacing = Locator.GetUIStyleManager().GetShipLogCardSpacing();
		_questionMark.color = Locator.GetUIStyleManager().GetShipLogRumorColor();
		_unreadIcon.gameObject.SetActive(value: false);
		_hudMarkerIcon.gameObject.SetActive(value: false);
		_moreToExploreIcon.gameObject.SetActive(value: false);
		_origIconSize = _moreToExploreIcon.rectTransform.sizeDelta;
		fontAndLangCtrl.AddTextElement(_name, rescale: false);
		_name.SetAllDirty();
		_questionMark.SetAllDirty();
		UpdateStateVisuals();
	}

	public void OnEnterComputer()
	{
		_name.text = _entry.GetName(withLineBreaks: true);
		_photo.sprite = _entry.GetSprite();
		if (_name.preferredHeight > _name.rectTransform.rect.height)
		{
			float num = _name.preferredHeight - _name.rectTransform.rect.height;
			Vector2 sizeDelta = _rectTransform.sizeDelta;
			sizeDelta.y += num;
			_rectTransform.sizeDelta = sizeDelta;
		}
		if (_entry.HasRevealedParent())
		{
			ShipLogEntry entry = Locator.GetShipLogManager().GetEntry(_entry.GetParentID());
			_size = (entry.IsCuriosity() ? 0.8f : 0.6f);
		}
		else if (_entry.IsCuriosity())
		{
			_size = 2f;
		}
		_border.color = Locator.GetUIStyleManager().GetCuriosityColor(_entry.GetCuriosityName());
		_origBorderColor = (_borderColor = _border.color);
		_rectTransform.localScale = Vector3.one * _size;
		if (_size < 1f)
		{
			Vector2 vector = _origIconSize / _size;
			_hudMarkerIcon.rectTransform.sizeDelta = vector;
			_unreadIcon.rectTransform.sizeDelta = vector;
			if (_entry.UseExtraLargeMoreToExploreIcon())
			{
				_moreToExploreIcon.rectTransform.sizeDelta = vector * 1.5f;
			}
			else
			{
				_moreToExploreIcon.rectTransform.sizeDelta = vector;
			}
		}
		_name.SetAllDirty();
		_photo.SetAllDirty();
	}

	public void OnEnterDetectiveMode()
	{
		UpdateUnreadIconVisibility();
		_borderColor = _origBorderColor;
		_border.color = _borderColor;
		_nameBackground.color = _borderColor;
		_moreToExploreIcon.gameObject.SetActive(_entry.HasMoreToExplore());
		UpdateStateVisuals();
	}

	public void SetMarkedOnHUD(bool markedOnHUD)
	{
		_hudMarkerIcon.gameObject.SetActive(markedOnHUD);
	}

	public ShipLogEntry GetEntry()
	{
		return _entry;
	}

	public bool IsRevealAnimationReady()
	{
		return _isRevealAnimReady;
	}

	public bool CheckPointCollision(Vector3 point)
	{
		return _border.rectTransform.rect.Contains(_border.rectTransform.InverseTransformPoint(point));
	}

	public void OnGainFocus()
	{
		_border.color = Locator.GetUIStyleManager().GetCuriosityColor(_entry.GetCuriosityName(), highlight: true);
		_nameBackground.color = Locator.GetUIStyleManager().GetCuriosityColor(_entry.GetCuriosityName(), highlight: true);
	}

	public void OnLoseFocus()
	{
		_border.color = _borderColor;
		_nameBackground.color = _borderColor;
	}

	public void MarkAsRead()
	{
		_entry.MarkAsRead();
		_unreadIcon.gameObject.SetActive(value: false);
	}

	public void UpdateUnreadIconVisibility()
	{
		_unreadIcon.gameObject.SetActive(_entry.HasUnreadFacts());
	}

	public void PrepareRevealAnimation()
	{
		UpdateUnreadIconVisibility();
		_entry.UpdatePreviousState();
		if (_entry.GetPreviousState() != _entry.GetState())
		{
			_isRevealAnimReady = true;
			base.gameObject.SetActive(value: true);
			_background.gameObject.SetActive(value: true);
			if (_entry.GetPreviousState() == ShipLogEntry.State.Hidden)
			{
				_rectTransform.localScale = new Vector3(1f, 0f, 1f);
			}
			else if (_entry.GetPreviousState() == ShipLogEntry.State.Rumored)
			{
				_photo.rectTransform.localScale = new Vector3(1f, 0f, 1f);
				_questionMark.gameObject.SetActive(value: true);
				_questionMark.SetAllDirty();
			}
			if (_entry.GetState() == ShipLogEntry.State.Explored)
			{
				_photo.gameObject.SetActive(value: true);
			}
		}
	}

	public void PlayRevealAnimation()
	{
		if (_isRevealAnimReady)
		{
			_startAnimTime = Time.unscaledTime;
			_isRevealAnimReady = false;
			base.enabled = true;
		}
		else
		{
			Debug.LogError("Reveal animation is not prepared!", this);
			Debug.Break();
		}
	}

	public void OnSelect()
	{
	}

	private void UpdateStateVisuals()
	{
		switch (_entry.GetState())
		{
		case ShipLogEntry.State.Hidden:
			base.gameObject.SetActive(value: false);
			break;
		case ShipLogEntry.State.Rumored:
			base.gameObject.SetActive(value: true);
			_photo.gameObject.SetActive(value: false);
			_background.gameObject.SetActive(value: true);
			break;
		case ShipLogEntry.State.Explored:
			base.gameObject.SetActive(value: true);
			_photo.gameObject.SetActive(value: true);
			_background.gameObject.SetActive(value: false);
			_questionMark.gameObject.SetActive(value: false);
			break;
		}
	}

	private void Update()
	{
		float num = Mathf.InverseLerp(_startAnimTime, _startAnimTime + 0.2f, Time.unscaledTime);
		if (_entry.GetPreviousState() == ShipLogEntry.State.Hidden)
		{
			_rectTransform.localScale = Vector3.Lerp(new Vector3(1f, 0f, 1f) * _size, Vector3.one * _size, num);
		}
		if (_entry.GetPreviousState() == ShipLogEntry.State.Rumored)
		{
			_photo.rectTransform.localScale = Vector3.Lerp(new Vector3(1f, 0f, 1f), Vector3.one, num);
		}
		if (num == 1f)
		{
			UpdateStateVisuals();
			base.enabled = false;
		}
	}
}
