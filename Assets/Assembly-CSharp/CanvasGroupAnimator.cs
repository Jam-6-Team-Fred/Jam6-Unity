using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupAnimator : MonoBehaviour
{
	private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private CanvasGroup _canvasGroup;

	private RectTransform _rectTransform;

	private Vector3 _targetScale;

	private Vector3 _startScale;

	private float _startTime;

	private float _endTime;

	private float _targetAlpha;

	private float _startAlpha;

	private bool _isComplete;

	private bool _updatingCanvases;

	private bool _invertCurve;

	private void Awake()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
		_rectTransform = GetComponent<RectTransform>();
		_isComplete = true;
		_updatingCanvases = false;
	}

	private void OnDestroy()
	{
		if (_updatingCanvases)
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases;
		}
		_updatingCanvases = false;
	}

	public bool IsComplete()
	{
		return _isComplete;
	}

	public void SetImmediate(float alpha)
	{
		SetImmediate(alpha, Vector3.one);
	}

	public void SetImmediate(float alpha, Vector3 scale)
	{
		_canvasGroup.alpha = alpha;
		_rectTransform.localScale = scale;
		_isComplete = true;
		if (_updatingCanvases)
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases;
		}
		_updatingCanvases = false;
		Canvas.ForceUpdateCanvases();
	}

	public void AnimateTo(float alpha, Vector3 scale, float duration, AnimationCurve curve = null, bool invertCurve = false)
	{
		if (curve != null)
		{
			_fadeCurve = curve;
			_invertCurve = invertCurve;
		}
		_startTime = Time.unscaledTime;
		_endTime = Time.unscaledTime + duration;
		_targetAlpha = alpha;
		_targetScale = scale;
		_startAlpha = _canvasGroup.alpha;
		_startScale = _rectTransform.localScale;
		_isComplete = false;
		if (!_updatingCanvases)
		{
			Canvas.willRenderCanvases += OnWillRenderCanvases;
		}
		_updatingCanvases = true;
	}

	private void OnWillRenderCanvases()
	{
		float num = Mathf.InverseLerp(_startTime, _endTime, Time.unscaledTime);
		num = (_invertCurve ? (1f - _fadeCurve.Evaluate(1f - num)) : _fadeCurve.Evaluate(num));
		_rectTransform.localScale = Vector3.Lerp(_startScale, _targetScale, num);
		_canvasGroup.alpha = Mathf.Lerp(_startAlpha, _targetAlpha, num);
		if (num >= 1f)
		{
			_isComplete = true;
			if (_updatingCanvases)
			{
				Canvas.willRenderCanvases -= OnWillRenderCanvases;
			}
			_updatingCanvases = false;
		}
	}
}
