using UnityEngine;
using UnityEngine.Rendering;

public class DitheringAnimator : MonoBehaviour
{
	[SerializeField]
	private bool _toggleShadowCasting = true;

	private bool _visible = true;

	private float _visibleFraction = 1f;

	private float _fadeRate = 1f;

	private OWRenderer[] _renderers;

	private void Awake()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		_renderers = new OWRenderer[componentsInChildren.Length];
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i] = componentsInChildren[i].GetComponent<OWRenderer>();
			if (_renderers[i] == null)
			{
				_renderers[i] = componentsInChildren[i].gameObject.AddComponent<OWRenderer>();
			}
		}
	}

	private void Start()
	{
		base.enabled = false;
	}

	public void SetVisibleImmediate(bool visible)
	{
		if (_visible != visible)
		{
			_visible = visible;
			_visibleFraction = (_visible ? 1f : 0f);
			UpdateDithering();
			UpdateShadowCasting();
		}
	}

	public void SetVisible(bool visible, float fadeRate)
	{
		if (_visible != visible)
		{
			_visible = visible;
			_fadeRate = fadeRate;
			if (!_visible)
			{
				UpdateShadowCasting();
			}
			base.enabled = true;
		}
	}

	private void Update()
	{
		float num = (_visible ? 1f : 0f);
		_visibleFraction = Mathf.MoveTowards(_visibleFraction, num, _fadeRate * Time.deltaTime);
		if (OWMath.ApproxEquals(_visibleFraction, num))
		{
			_visibleFraction = num;
			base.enabled = false;
			if (_visible)
			{
				UpdateShadowCasting();
			}
		}
		UpdateDithering();
	}

	private void UpdateDithering()
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			if (_renderers[i] != null)
			{
				_renderers[i].SetDitherFade(1f - _visibleFraction);
			}
		}
	}

	private void UpdateShadowCasting()
	{
		if (_toggleShadowCasting)
		{
			for (int i = 0; i < _renderers.Length; i++)
			{
				_renderers[i].GetRenderer().shadowCastingMode = (_visible ? ShadowCastingMode.On : ShadowCastingMode.Off);
			}
		}
	}
}
