using UnityEngine;
using UnityEngine.UI;

public class ShipLogFactListItem : MonoBehaviour
{
	[SerializeField]
	private Text _text;

	[SerializeField]
	private UiSizeSetterShipLogFact _uiSizeSetter;

	private ShipLogFact _fact;

	private RectTransform _rectTransform;

	private void Start()
	{
		_rectTransform = GetComponent<RectTransform>();
		_text.font = Locator.GetUIStyleManager().GetShipLogFont();
		_uiSizeSetter.MarkReadyForInitialization();
	}

	public Vector2 GetPosition()
	{
		return _rectTransform.anchoredPosition;
	}

	public float GetHeight()
	{
		return _rectTransform.sizeDelta.y;
	}

	public void DisplayText(string text)
	{
		_fact = null;
		_text.text = text;
		base.gameObject.SetActive(value: true);
	}

	public void DisplayFact(ShipLogFact fact)
	{
		_fact = fact;
		base.gameObject.SetActive(value: true);
	}

	public void Clear()
	{
		_fact = null;
		_text.text = string.Empty;
		base.gameObject.SetActive(value: false);
	}

	public void StartTextReveal()
	{
		if (_fact != null)
		{
			_fact.StartTextReveal();
		}
	}

	public bool UpdateTextReveal()
	{
		if (_fact != null)
		{
			_text.text = _fact.GetRevealedText(out var isRevealing);
			return isRevealing;
		}
		return false;
	}
}
