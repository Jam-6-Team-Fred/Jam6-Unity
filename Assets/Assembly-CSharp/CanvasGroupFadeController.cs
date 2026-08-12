using System;
using UnityEngine;

[Serializable]
public class CanvasGroupFadeController
{
	public CanvasGroup group;

	private bool _fading;

	private float _startAlpha;

	private float _targetAlpha;

	private float _startTime;

	private float _duration;

	private bool _unscaled;

	public void Reset(bool reversed = false)
	{
		group.alpha = (reversed ? 1f : 0f);
		_fading = false;
	}

	public void UseUnscaledTime()
	{
		_unscaled = true;
	}

	public void FadeTo(float alpha, float duration, float delay = 0f)
	{
		_startAlpha = group.alpha;
		_targetAlpha = alpha;
		_duration = duration;
		_startTime = (_unscaled ? Time.unscaledTime : Time.time) + delay;
		_fading = true;
	}

	public void Update(AnimationCurve fadeCurve)
	{
		if (_fading)
		{
			float num = Mathf.InverseLerp(_startTime, _startTime + _duration, _unscaled ? Time.unscaledTime : Time.time);
			group.alpha = Mathf.Lerp(_startAlpha, _targetAlpha, fadeCurve.Evaluate(num));
			if (num >= 1f)
			{
				_fading = false;
			}
		}
	}
}
