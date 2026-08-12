using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
public class ShipLogEntryLink : MonoBehaviour, IShipLogSelectable
{
	[SerializeField]
	private Image _arrow;

	[SerializeField]
	private Image _arrowBackground;

	[SerializeField]
	private Image _line;

	[SerializeField]
	private RectTransform _boundsRect;

	private RectTransform _rectTransform;

	private ShipLogEntryCard _sourceCard;

	private ShipLogEntryCard _targetCard;

	private List<ShipLogFact> _rumorFacts;

	private bool _updateRevealAnim;

	private bool _editMode;

	private bool _isRevealAnimReady;

	private bool _hasFocus;

	private bool _hidden;

	private float _startRevealTime;

	private float _revealDuration;

	private float _arrowPosY;

	private Color _baseColor;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		_rectTransform.SetAsFirstSibling();
	}

	public void Init(ShipLogEntryCard sourceCard, ShipLogEntryCard targetCard, bool editMode = false)
	{
		_sourceCard = sourceCard;
		_targetCard = targetCard;
		_rumorFacts = new List<ShipLogFact>();
		_isRevealAnimReady = false;
		_updateRevealAnim = false;
		_hasFocus = false;
		_hidden = false;
		_baseColor = Locator.GetUIStyleManager().GetShipLogNeutralColor();
		_line.color = _baseColor;
		_arrow.color = _baseColor;
		_arrowBackground.color = _baseColor;
		base.enabled = (_editMode = editMode);
	}

	public int GetRevealOrder()
	{
		int num = int.MaxValue;
		bool flag = false;
		for (int i = 0; i < _rumorFacts.Count; i++)
		{
			if (_rumorFacts[i].IsRevealed() && _rumorFacts[i].GetRevealOrder() < num)
			{
				num = _rumorFacts[i].GetRevealOrder();
				flag = true;
			}
		}
		if (!flag)
		{
			return -1;
		}
		return num;
	}

	public List<ShipLogFact> GetFactsForDisplay()
	{
		List<ShipLogFact> list = new List<ShipLogFact>();
		for (int i = 0; i < _rumorFacts.Count; i++)
		{
			if (_rumorFacts[i].IsRevealed())
			{
				list.Add(_rumorFacts[i]);
			}
		}
		return list;
	}

	public ShipLogEntryCard GetSourceEntryCard()
	{
		return _sourceCard;
	}

	public ShipLogEntryCard GetTargetEntryCard()
	{
		return _targetCard;
	}

	public bool IsVisible()
	{
		if (!_hidden)
		{
			return _rectTransform.gameObject.activeSelf;
		}
		return false;
	}

	public bool IsRevealAnimationReady()
	{
		return _isRevealAnimReady;
	}

	public void Hide()
	{
		_hidden = true;
		_rectTransform.gameObject.SetActive(value: false);
	}

	public void AddRumorFact(ShipLogFact fact)
	{
		_rumorFacts.Add(fact);
	}

	public bool CheckPointCollision(Vector3 point)
	{
		return _boundsRect.rect.Contains(_boundsRect.InverseTransformPoint(point));
	}

	public void OnGainFocus()
	{
		_arrow.color = Locator.GetUIStyleManager().GetShipLogNeutralColor(highlight: true);
		_line.color = Locator.GetUIStyleManager().GetShipLogNeutralColor(highlight: true);
		_arrowBackground.color = Locator.GetUIStyleManager().GetShipLogNeutralColor(highlight: true);
		_hasFocus = true;
		base.enabled = true;
	}

	public void OnLoseFocus()
	{
		_arrow.color = _baseColor;
		_line.color = _baseColor;
		_arrowBackground.color = _baseColor;
		_hasFocus = false;
	}

	public void OnSelect()
	{
	}

	public void MarkAsRead()
	{
		for (int i = 0; i < _rumorFacts.Count; i++)
		{
			_rumorFacts[i].MarkAsRead();
		}
		_targetCard.UpdateUnreadIconVisibility();
	}

	public void PrepareRevealAnimation()
	{
		bool flag = true;
		for (int i = 0; i < _rumorFacts.Count; i++)
		{
			if (_rumorFacts[i].IsRevealed() && !_rumorFacts[i].IsNewlyRevealed())
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			_isRevealAnimReady = true;
			_arrow.gameObject.SetActive(value: false);
			_arrow.rectTransform.localScale = Vector3.zero;
			_arrowBackground.gameObject.SetActive(value: false);
			_arrowBackground.rectTransform.localScale = Vector3.zero;
			_line.rectTransform.localScale = new Vector3(1f, 0f, 1f);
		}
	}

	public void PlayRevealAnimation(float duration)
	{
		if (_isRevealAnimReady)
		{
			base.enabled = true;
			_revealDuration = duration;
			_updateRevealAnim = true;
			_startRevealTime = Time.unscaledTime;
			_isRevealAnimReady = false;
		}
		else
		{
			Debug.LogError("Reveal animation is not prepared!", this);
			Debug.Break();
		}
	}

	public void UpdatePosition()
	{
		Vector2 to = _targetCard.GetAnchoredPosition() - _sourceCard.GetAnchoredPosition();
		float magnitude = to.magnitude;
		_rectTransform.anchoredPosition = _sourceCard.GetAnchoredPosition();
		float z = Vector2.Angle(Vector2.up, to) * (0f - Mathf.Sign(to.x));
		_rectTransform.localEulerAngles = new Vector3(0f, 0f, z);
		_line.rectTransform.sizeDelta = new Vector2(_line.rectTransform.sizeDelta.x, magnitude);
		_boundsRect.sizeDelta = new Vector2(_boundsRect.sizeDelta.x, magnitude);
		Vector2 edgeIntersection = _sourceCard.GetEdgeIntersection(_targetCard.GetAnchoredPosition());
		Vector2 edgeIntersection2 = _targetCard.GetEdgeIntersection(_sourceCard.GetAnchoredPosition());
		_arrowPosY = (edgeIntersection - _sourceCard.GetAnchoredPosition()).magnitude + (edgeIntersection2 - edgeIntersection).magnitude * 0.5f;
		_arrow.rectTransform.anchoredPosition = new Vector2(0f, _arrowPosY);
		_arrowBackground.rectTransform.anchoredPosition = new Vector2(0f, _arrowPosY);
	}

	public void UpdateVisibility()
	{
		bool flag = false;
		for (int i = 0; i < _rumorFacts.Count; i++)
		{
			if (_rumorFacts[i].IsRevealed())
			{
				flag = true;
				break;
			}
		}
		bool active = !_hidden && flag && _sourceCard.GetEntry().GetState() != ShipLogEntry.State.Hidden;
		_rectTransform.gameObject.SetActive(active);
		_baseColor = _line.color;
		_line.color = _baseColor;
		_arrow.color = _baseColor;
		_arrowBackground.color = _baseColor;
	}

	private void Update()
	{
		if (_editMode)
		{
			UpdatePosition();
		}
		bool flag = _hasFocus;
		float num = (_hasFocus ? 2f : 1f);
		if (!Mathf.Approximately(_arrow.rectTransform.localScale.x, num))
		{
			flag = true;
			float num2 = Mathf.MoveTowards(_arrow.rectTransform.localScale.x, num, Time.unscaledDeltaTime * 6f);
			_arrow.rectTransform.localScale = Vector3.one * num2;
			_arrowBackground.rectTransform.localScale = Vector3.one * num2;
		}
		if (_updateRevealAnim)
		{
			float t = Mathf.InverseLerp(_startRevealTime, _startRevealTime + _revealDuration, Time.unscaledTime);
			t = Mathf.SmoothStep(0f, 1f, t);
			_line.rectTransform.localScale = Vector3.Lerp(new Vector3(1f, 0f, 1f), Vector3.one, t);
			_arrow.gameObject.SetActive(value: true);
			_arrow.rectTransform.anchoredPosition = new Vector2(0f, Mathf.Lerp(0f, _arrowPosY, t));
			_arrow.rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.Clamp01(t * 2f));
			_arrowBackground.gameObject.SetActive(value: true);
			_arrowBackground.rectTransform.anchoredPosition = new Vector2(0f, Mathf.Lerp(0f, _arrowPosY, t));
			_arrowBackground.rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.Clamp01(t * 2f));
			if (t >= 1f)
			{
				_updateRevealAnim = false;
			}
		}
		if (!_editMode && !_updateRevealAnim && !flag)
		{
			base.enabled = false;
		}
	}
}
