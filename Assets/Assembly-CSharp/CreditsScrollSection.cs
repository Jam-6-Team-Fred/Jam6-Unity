using UnityEngine;
using UnityEngine.UI;

public class CreditsScrollSection : CreditsSection
{
	[SerializeField]
	private float _scrollDuration = 120f;

	private RectTransform _rectTransform;

	private float _scrollSpeed;

	private float _startTime;

	private CanvasScaler _parentCanvas;

	public RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	private float scaledScreenHeight
	{
		get
		{
			if (_parentCanvas == null)
			{
				_parentCanvas = GetComponentInParent<CanvasScaler>();
				Debug.Log("had to search for parent canvas");
			}
			return _parentCanvas.referenceResolution.y;
		}
	}

	public float height => rectTransform.sizeDelta.y;

	public override float GetTotalTime()
	{
		return _scrollDuration;
	}

	public void SetScrollDuration(float duration)
	{
		_scrollDuration = duration;
	}

	private void Start()
	{
		rectTransform.anchoredPosition = new Vector2(0f, 0f);
	}

	public override void Play()
	{
		rectTransform.anchoredPosition = new Vector2(0f, 0f);
		_scrollSpeed = (height + scaledScreenHeight) / _scrollDuration;
		_startTime = Time.time;
		_isPlaying = true;
	}

	private void Update()
	{
		if (_isPlaying)
		{
			_scrollSpeed = (height + scaledScreenHeight) / _scrollDuration;
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			anchoredPosition.y += _scrollSpeed * Time.deltaTime;
			rectTransform.anchoredPosition = anchoredPosition;
			if (Time.time >= _startTime + _scrollDuration)
			{
				_isPlaying = false;
			}
		}
	}

	public override bool SimulateTime(float time)
	{
		if (time > _scrollDuration)
		{
			return false;
		}
		_scrollSpeed = (height + scaledScreenHeight) / _scrollDuration;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.y = _scrollSpeed * time;
		rectTransform.anchoredPosition = anchoredPosition;
		return true;
	}

	public override void ResetSimulate()
	{
		rectTransform.anchoredPosition = Vector2.zero;
	}
}
