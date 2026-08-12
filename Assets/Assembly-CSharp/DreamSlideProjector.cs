using UnityEngine;

public class DreamSlideProjector : SlideProjector
{
	[Space]
	[SerializeField]
	private DreamLibraryFlame _flame;

	private bool _lit;

	protected override void Start()
	{
		base.Start();
	}

	public void SetLit(bool lit)
	{
		if (lit != _lit)
		{
			_lit = lit;
			_flame.SetLit(lit);
			CheckLightStatus();
		}
	}

	protected override bool IsProjectorLit()
	{
		return _lit;
	}

	protected override bool IsProjectorFullyLit()
	{
		return _lit;
	}

	protected override void CheckLightStatus()
	{
		FadeProjectorLightTo(_lit ? 1f : 0f, 1f);
		if (_houseLightController != null)
		{
			_houseLightController.FadeTo(_lit ? 0f : 1f, 1f);
		}
		if (_lit && _displayCookie == null)
		{
			GetDisplayCookie();
		}
		else if (!_lit && _displayCookie != null)
		{
			FinishUsingDisplayCookie();
		}
		BlitCookie();
	}

	protected override void BlitCookie()
	{
		if (!(_displayCookie == null))
		{
			_cookieBlitMaterial.SetFloat("_isMasked", 1f);
			Graphics.Blit(_slideToDisplay, _displayCookie, _cookieBlitMaterial);
		}
	}
}
