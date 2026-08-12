using UnityEngine;

public class OWRendererFadeController : MonoBehaviour
{
	[SerializeField]
	private OWRenderer[] _renderers;

	private float _fade = 1f;

	private float _fadeStartTime;

	private float _fadeDuration;

	private float _startFade;

	private float _targetFade;

	private void Awake()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
	}

	public void SetFade(float fade)
	{
		_fade = fade;
		UpdateVisuals();
	}

	public void FadeTo(float fade, float duration)
	{
		if (duration <= 0f)
		{
			_fade = fade;
			UpdateVisuals();
			return;
		}
		_startFade = _fade;
		_targetFade = fade;
		_fadeDuration = duration;
		_fadeStartTime = Time.time;
		base.enabled = true;
	}

	private void Update()
	{
		float num = Mathf.InverseLerp(_fadeStartTime, _fadeStartTime + _fadeDuration, Time.time);
		_fade = Mathf.Lerp(_startFade, _targetFade, Mathf.SmoothStep(0f, 1f, num));
		if (num >= 1f)
		{
			base.enabled = false;
		}
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].SetFade(1f - _fade);
		}
	}
}
