using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
	public enum Platform
	{
		Nothing = 0,
		Epic = 2,
		Steam = 4,
		PS4 = 8,
		XBoxOne = 16,
		Switch = 32,
		PS5 = 64,
		XBoxSeriesS = 128,
		XBoxSeriesX = 256,
		XBoxSeriesSX = 384,
		Sony = 72,
		Microsoft = 400,
		Nintendo = 32,
		PC = 6,
		All = int.MaxValue
	}

	public enum CreditsType
	{
		Final = 1,
		Fast = 4,
		Krazy = 8,
		All = int.MaxValue
	}

	[SerializeField]
	private CreditsType _type;

	[SerializeField]
	private CreditsAsset _creditsAsset;

	[Space]
	[SerializeField]
	private AnimationCurve _fadeFromWhiteCurve;

	[SerializeField]
	private Image _fadeImage;

	[SerializeField]
	private float _whiteFadeDuration = 0.5f;

	[SerializeField]
	private OWAudioSource _musicSource;

	[SerializeField]
	private OWAudioSource _kazooSource;

	[SerializeField]
	private AudioClip _previewClip;

	private List<CreditsSection> _topLevelSections;

	private int _currentPlayingSection;

	private bool _prevCurrentSectionPlaying;

	[SerializeField]
	[HideInInspector]
	private float _totalPlayTime;

	[SerializeField]
	[HideInInspector]
	private float _simulatedTimeNormalized;

	[SerializeField]
	[HideInInspector]
	private Platform _simulatePlatform;

	private float _whiteFadeStartTime;

	private int _originalVSyncCount;

	private bool _hasStartFade;

	private bool _graphicsSettingsChanged;

	private bool _musicHasPlayed;

	private bool _manualRefreshRate;

	private float _cumulativeUpdateTime;

	private int _frameCount;

	private float _avgUpdateTime;

	public float simulatedTimeNormalized
	{
		get
		{
			return _simulatedTimeNormalized;
		}
		set
		{
			_simulatedTimeNormalized = value;
			if (previewing)
			{
				Simulate();
			}
		}
	}

	public float totalPlayTime => _totalPlayTime;

	public Platform simulatePlatform
	{
		get
		{
			return _simulatePlatform;
		}
		set
		{
			_simulatePlatform = value;
		}
	}

	public bool previewing => base.transform.childCount > 0;

	private void Start()
	{
		if (LoadManager.GetCurrentScene() == OWScene.Credits_Fast && TimelineObliterationController.HasRealityEnded())
		{
			_type = CreditsType.Krazy;
			TimelineObliterationController.ResetHasRealityEnded();
		}
		if (_manualRefreshRate)
		{
			if (QualitySettings.vSyncCount > 0)
			{
				_originalVSyncCount = QualitySettings.vSyncCount;
				QualitySettings.vSyncCount = 0;
				_graphicsSettingsChanged = true;
			}
			Application.targetFrameRate = Screen.currentResolution.refreshRate;
		}
		if (_fadeImage != null)
		{
			_fadeImage.color = Color.white;
		}
		EndPreview();
		BuildCredits();
		_currentPlayingSection = -1;
		_whiteFadeStartTime = Time.time;
		_hasStartFade = _fadeImage != null;
	}

	private void Update()
	{
		if (_type != CreditsType.Final && (OWInput.IsNewlyPressed(InputLibrary.enter) || OWInput.IsNewlyPressed(InputLibrary.enter2) || OWInput.IsNewlyPressed(InputLibrary.select) || OWInput.IsNewlyPressed(InputLibrary.menuConfirm) || OWInput.IsNewlyPressed(InputLibrary.cancel) || OWInput.IsNewlyPressed(InputLibrary.escape)) && LoadManager.GetLoadingScene() == OWScene.None)
		{
			LoadNextScene();
		}
		if (_manualRefreshRate)
		{
			if (Time.unscaledDeltaTime - 1f / (float)Screen.currentResolution.refreshRate <= 1f / (float)Screen.currentResolution.refreshRate / 10f)
			{
				_cumulativeUpdateTime += Time.unscaledDeltaTime;
				_frameCount++;
			}
			if (_frameCount >= 100)
			{
				_avgUpdateTime = _cumulativeUpdateTime / (float)_frameCount;
				Application.targetFrameRate = Mathf.RoundToInt(1f / _avgUpdateTime);
			}
		}
		if (_hasStartFade && Time.time < _whiteFadeStartTime + _whiteFadeDuration)
		{
			float time = 1f - Mathf.InverseLerp(_whiteFadeStartTime, _whiteFadeStartTime + _whiteFadeDuration, Time.time);
			_fadeImage.color = new Color(1f, 1f, 1f, _fadeFromWhiteCurve.Evaluate(time));
			return;
		}
		if (_hasStartFade && _fadeImage.color.a > 0f)
		{
			_fadeImage.color = new Color(1f, 1f, 1f, 0f);
		}
		if (!_musicHasPlayed)
		{
			if (_type == CreditsType.Krazy)
			{
				if (!_kazooSource.isPlaying)
				{
					_kazooSource.FadeIn(5f, fadeFromNothing: true);
					_musicHasPlayed = true;
				}
			}
			else if (!_musicSource.isPlaying)
			{
				_musicSource.FadeIn(5f, fadeFromNothing: true);
				_musicHasPlayed = true;
			}
		}
		if (_currentPlayingSection < 0)
		{
			_currentPlayingSection = 0;
			if (_topLevelSections != null && _topLevelSections.Count > 0)
			{
				_topLevelSections[0].Play();
				_prevCurrentSectionPlaying = true;
			}
		}
		if (_prevCurrentSectionPlaying && !_topLevelSections[_currentPlayingSection].isPlaying)
		{
			_currentPlayingSection++;
			if (_currentPlayingSection < _topLevelSections.Count)
			{
				_topLevelSections[_currentPlayingSection].Play();
				return;
			}
			base.enabled = false;
			LoadNextScene();
		}
	}

	private void LoadNextScene()
	{
		if (_manualRefreshRate)
		{
			if (_graphicsSettingsChanged)
			{
				QualitySettings.vSyncCount = _originalVSyncCount;
			}
			Application.targetFrameRate = -1;
		}
		if (_type == CreditsType.Final)
		{
			LoadManager.LoadScene(OWScene.PostCreditsScene);
		}
		else
		{
			LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack);
		}
	}

	public void BuildCredits()
	{
		_totalPlayTime = 0f;
		_topLevelSections = _creditsAsset.BuildCredits(base.transform, _simulatePlatform, _type, ref _totalPlayTime);
	}

	public void EndPreview()
	{
		if (previewing)
		{
			while (base.transform.childCount > 0)
			{
				Object.DestroyImmediate(base.transform.GetChild(0).gameObject);
			}
		}
	}

	public void Simulate()
	{
		if (!previewing)
		{
			return;
		}
		float num = _simulatedTimeNormalized * _totalPlayTime;
		foreach (Transform item in base.transform)
		{
			CreditsSection component = item.GetComponent<CreditsSection>();
			float totalTime = component.GetTotalTime();
			if (num > 0f)
			{
				component.SimulateTime(num);
				num -= totalTime;
			}
			else
			{
				component.ResetSimulate();
			}
		}
	}
}
