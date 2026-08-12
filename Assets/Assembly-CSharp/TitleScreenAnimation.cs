using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TitleScreenAnimation : MonoBehaviour
{
	public delegate void TitleScreenCameraEvent();

	private Animator _animator;

	[SerializeField]
	private TitleCodeInputManager _titleCodeInputManager;

	[SerializeField]
	private MenuGammaSetting _gammaMenu;

	[Space(10f)]
	[SerializeField]
	private OWCamera _camera;

	[SerializeField]
	private Campfire _campfire;

	[SerializeField]
	private OWLightController _ambientLightController;

	[SerializeField]
	private OWAudioSource _ambienceSource;

	[SerializeField]
	private OWAudioSource _musicSource;

	[SerializeField]
	private CanvasGroupFadeController _gamepadSplashController;

	[SerializeField]
	private AnimationCurve _gamepadSplashCurve;

	private bool _gamepadSplash;

	private bool _doRunGammaFirstTimeSetup;

	private bool _introPan;

	private bool _skipIntroPanCommand;

	private bool _fadingIn;

	private float _fade;

	private float _fadeDuration;

	private bool _isPaused;

	private float _fadeOutGamepadTime;

	private bool _fadingInGamepad;

	private bool _fadingOutGamepad;

	private bool _ambienceFadeTriggered;

	private int _ambienceFrameCounter;

	public event TitleScreenCameraEvent OnLogoPanComplete;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_gamepadSplash = LoadManager.GetPreviousScene() == OWScene.None;
		_doRunGammaFirstTimeSetup = false;
		if (PlayerData.IsLoaded())
		{
			_doRunGammaFirstTimeSetup = !PlayerData.RanFirstRunGammaSetup();
		}
		_introPan = LoadManager.GetPreviousScene() == OWScene.None || LoadManager.GetPreviousScene() == OWScene.PostCreditsScene;
		_skipIntroPanCommand = false;
		_fadeDuration = (_introPan ? 3f : 1f);
	}

	private void Start()
	{
		_camera.postProcessingSettings.colorGrading.postExposure = -10f;
		_ambientLightController.SetIntensity(0f);
		_ambienceSource.SetLocalVolume(0f);
		_gamepadSplashController.FadeTo(0f, 0f);
		_animator.enabled = false;
		if (!_gamepadSplash)
		{
			if (_doRunGammaFirstTimeSetup)
			{
				_gammaMenu.OnGammaMenuFadeOutComplete += OnGammaMenuFadeOutComplete;
				_gammaMenu.ActivateAsFirstTimeSetup();
			}
			else
			{
				StartLogoAnimation();
			}
		}
		if (!_introPan)
		{
			_animator.enabled = true;
			_animator.Play(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
		}
	}

	private void OnGammaMenuFadeOutComplete(bool firstTimeRun)
	{
		_gammaMenu.OnGammaMenuFadeOutComplete -= OnGammaMenuFadeOutComplete;
		PlayerData.SetRanFirstRunGammaSetup(val: true);
		StartLogoAnimation();
	}

	private void StartLogoAnimation()
	{
		_fadingIn = true;
		_fade = 1f;
		_animator.enabled = true;
	}

	private void Update()
	{
		if (_gamepadSplash)
		{
			_gamepadSplashController.Update(_gamepadSplashCurve);
			if (!_fadingInGamepad && Time.time > 0.5f)
			{
				_fadeOutGamepadTime = Time.time + 6f;
				_gamepadSplashController.FadeTo(1f, 3f);
				_fadingInGamepad = true;
			}
			else if (_fadingOutGamepad && _gamepadSplashController.group.alpha <= 0f)
			{
				if (_doRunGammaFirstTimeSetup)
				{
					_gammaMenu.OnGammaMenuFadeOutComplete += OnGammaMenuFadeOutComplete;
					_gammaMenu.ActivateAsFirstTimeSetup();
				}
				else
				{
					StartLogoAnimation();
				}
				_gamepadSplash = false;
			}
			return;
		}
		if (!_ambienceFadeTriggered)
		{
			if (_ambienceFrameCounter >= 8)
			{
				_ambienceSource.FadeIn(3f);
				_ambienceFadeTriggered = true;
			}
			else
			{
				_ambienceFrameCounter++;
			}
		}
		if (_fadingIn)
		{
			_fade = Mathf.MoveTowards(_fade, 0f, Time.deltaTime / _fadeDuration);
			_camera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(0f, -10f, _fade);
			if (_fade <= 0f)
			{
				_fadingIn = false;
			}
		}
		if (!_animator.enabled)
		{
			return;
		}
		if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
		{
			FadeInMusic();
			_introPan = false;
			_animator.enabled = false;
			_campfire.SetState(Campfire.State.LIT);
			_ambientLightController.FadeTo(1f, 1f);
			if (this.OnLogoPanComplete != null)
			{
				this.OnLogoPanComplete();
			}
		}
		else if (_fade < 0.5f && (OWInput.IsNewlyPressed(InputLibrary.select) || OWInput.IsNewlyPressed(InputLibrary.cancel) || OWInput.IsNewlyPressed(InputLibrary.menuConfirm) || OWInput.GetAnyJoystickButtonPressed()) && !_skipIntroPanCommand && !_titleCodeInputManager.CodeInputInProgress() && !_isPaused)
		{
			_skipIntroPanCommand = true;
			_animator.Play(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
		}
	}

	private void LateUpdate()
	{
		if (_fadingInGamepad && !_fadingOutGamepad && !_isPaused)
		{
			bool flag = OWInput.IsNewlyPressed(InputLibrary.select) || OWInput.IsNewlyPressed(InputLibrary.menuConfirm) || OWInput.IsNewlyPressed(InputLibrary.cancel) || OWInput.IsNewlyPressed(InputLibrary.escape) || OWInput.IsNewlyPressed(InputLibrary.enter) || OWInput.IsNewlyPressed(InputLibrary.enter2) || OWInput.GetAnyJoystickButtonPressed();
			if (_titleCodeInputManager.CodeInputInProgress())
			{
				flag = false;
			}
			if (flag || Time.time > _fadeOutGamepadTime)
			{
				_fadingOutGamepad = true;
				_gamepadSplashController.FadeTo(0f, flag ? 0.5f : 3f);
			}
		}
	}

	public void FadeInMusic()
	{
		if (!_musicSource.isPlaying)
		{
			_musicSource.SetLocalVolume(0f);
			_musicSource.FadeIn(8f);
		}
	}

	public bool IsPlayingIntroAnimation()
	{
		return _introPan;
	}

	public bool IsFadingIn()
	{
		return _fadingIn;
	}

	public bool IsPaused()
	{
		return _isPaused;
	}

	public void Pause()
	{
		_animator.speed = 0f;
		_isPaused = true;
	}

	public void Resume()
	{
		_animator.speed = 1f;
		_isPaused = false;
	}

	public void SkipToTitle()
	{
		if (!_skipIntroPanCommand && _animator.enabled)
		{
			_skipIntroPanCommand = true;
			_animator.Play(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 1f);
		}
	}
}
