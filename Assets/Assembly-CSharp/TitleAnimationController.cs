using UnityEngine;

public class TitleAnimationController : MonoBehaviour
{
	public delegate void TitleAnimationEvent();

	[SerializeField]
	private AnimationCurve _fadeCurve;

	[SerializeField]
	private float _logoFadeDelay = 1f;

	[SerializeField]
	private float _logoFadeDuration = 5f;

	[SerializeField]
	private float _echoesFadeDelay = 1f;

	[SerializeField]
	private float _optionsFadeDelay = 2f;

	[SerializeField]
	private float _optionsFadeDuration = 5f;

	[SerializeField]
	private float _optionsFadeSpacing = 0.3f;

	[SerializeField]
	private CanvasGroupFadeController _logoFadeController;

	[SerializeField]
	private CanvasGroupFadeController _echoesFadeController;

	[SerializeField]
	private CanvasGroupFadeController[] _buttonFadeControllers;

	[SerializeField]
	private CanvasGroupFadeController _footerFadeController;

	[SerializeField]
	private Animator _titleAnimator;

	private bool _fadingInLogo;

	private bool _showEchoesLogo;

	private bool _needEchoesRecheck;

	private bool _fadingInButtons;

	private bool _animationComplete;

	private float _logoStartFadeTime;

	public event TitleAnimationEvent OnTitleLogoAnimationComplete;

	public event TitleAnimationEvent OnTitleMenuAnimationComplete;

	private void Awake()
	{
		_logoFadeController.Reset();
		_footerFadeController.Reset();
		_echoesFadeController.Reset();
		_echoesFadeController.group.gameObject.SetActive(value: false);
		ResetMenuOptions();
	}

	public void ResetMenuOptions()
	{
		_fadingInButtons = false;
		_animationComplete = false;
		for (int i = 0; i < _buttonFadeControllers.Length; i++)
		{
			_buttonFadeControllers[i].Reset();
		}
	}

	public bool IsFadingInLogo()
	{
		return _fadingInLogo;
	}

	public bool IsFadingInMenuOptions()
	{
		return _fadingInButtons;
	}

	public bool IsTitleAnimationComplete()
	{
		return _animationComplete;
	}

	public void FadeInTitleLogo(bool instant = false)
	{
		_fadingInLogo = true;
		_logoStartFadeTime = Time.time;
		float duration = (instant ? 0f : _logoFadeDuration);
		_logoFadeController.FadeTo(1f, duration, _logoFadeDelay);
		_footerFadeController.FadeTo(1f, duration, _logoFadeDelay);
		CheckDlcEntitlement();
	}

	public void CheckDlcEntitlement()
	{
		switch (EntitlementsManager.IsDlcOwned())
		{
		case EntitlementsManager.AsyncOwnershipStatus.Owned:
			_showEchoesLogo = true;
			_echoesFadeController.group.gameObject.SetActive(value: true);
			break;
		case EntitlementsManager.AsyncOwnershipStatus.NotReady:
			_needEchoesRecheck = true;
			break;
		}
	}

	public void FadeInMenuOptions()
	{
		_fadingInButtons = true;
		float num = 0f;
		for (int i = 0; i < _buttonFadeControllers.Length; i++)
		{
			if (_buttonFadeControllers[i].group.gameObject.activeInHierarchy)
			{
				_buttonFadeControllers[i].FadeTo(1f, _optionsFadeDuration, num);
				num += _optionsFadeSpacing;
			}
		}
	}

	private void Update()
	{
		if (!_showEchoesLogo && _needEchoesRecheck)
		{
			_needEchoesRecheck = false;
			switch (EntitlementsManager.IsDlcOwned())
			{
			case EntitlementsManager.AsyncOwnershipStatus.Owned:
				_showEchoesLogo = true;
				_echoesFadeController.group.gameObject.SetActive(value: true);
				break;
			case EntitlementsManager.AsyncOwnershipStatus.NotReady:
				_needEchoesRecheck = true;
				break;
			}
		}
		_titleAnimator.SetFloat("Progression", Mathf.Clamp01(Time.timeSinceLevelLoad / 1320f));
		_logoFadeController.Update(_fadeCurve);
		_footerFadeController.Update(_fadeCurve);
		_echoesFadeController.Update(_fadeCurve);
		if (_fadingInLogo && Time.time > _logoStartFadeTime + _optionsFadeDelay)
		{
			_fadingInLogo = false;
			if (this.OnTitleLogoAnimationComplete != null)
			{
				this.OnTitleLogoAnimationComplete();
			}
		}
		bool flag = true;
		for (int i = 0; i < _buttonFadeControllers.Length; i++)
		{
			_buttonFadeControllers[i].Update(_fadeCurve);
			if (_buttonFadeControllers[i].group.gameObject.activeSelf && _buttonFadeControllers[i].group.alpha < 0.01f)
			{
				flag = false;
			}
		}
		if (_fadingInButtons && flag)
		{
			_fadingInButtons = false;
			_animationComplete = true;
			if (_showEchoesLogo)
			{
				_echoesFadeController.FadeTo(1f, _logoFadeDuration, _echoesFadeDelay);
			}
			if (this.OnTitleMenuAnimationComplete != null)
			{
				this.OnTitleMenuAnimationComplete();
			}
		}
	}
}
