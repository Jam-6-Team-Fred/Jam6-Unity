using UnityEngine;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
	[SerializeField]
	private Canvas _gameOverTextCanvas;

	[Space(10f)]
	[SerializeField]
	private Text _deathText;

	[SerializeField]
	private int _deathTextMaxFontSize;

	[Space(10f)]
	[SerializeField]
	private CanvasGroupAnimator _textAnimator;

	[SerializeField]
	private AnimationCurve _fadeCurve;

	[Space(10f)]
	[SerializeField]
	private CanvasGroupAnimator _whiteFadeAnimator;

	[SerializeField]
	private AnimationCurve _whiteFadeCurve;

	private RectTransform _deathTextRectTransform;

	private OWCamera _flashbackCamera;

	private AudioListener _audioListener;

	private const float c_whiteFadeDelay = 3f;

	private float _textFadeDelay = 0.5f;

	private float _textStayDuration;

	private float _gameOverTime;

	private bool _fadedInText;

	private bool _fadedOutText;

	private bool _loading;

	private bool _updatingCanvases;

	private void Awake()
	{
		_flashbackCamera = GetComponent<OWCamera>();
		_audioListener = GetComponentInChildren<AudioListener>();
		_deathTextRectTransform = _deathText.rectTransform;
		GlobalMessenger.AddListener("TriggerDeathOutsideTimeLoop", OnTriggerDeathOutsideTimeLoop);
		GlobalMessenger.AddListener("TriggerDeathOfReality", OnTriggerDeathOfReality);
		GlobalMessenger.AddListener("TriggerDeathByVoid", OnTriggerDeathByVoid);
		GlobalMessenger.AddListener("TriggerDeathByRingworldEscape", OnTriggerDeathByRingworldEscape);
		GlobalMessenger.AddListener("TriggerDeathByDreamworldEscape", OnTriggerDeathByDreamworldEscape);
		GlobalMessenger.AddListener("TriggerDeathByQuantumMoon", OnTriggerDeathByQuantumMoon);
	}

	private void Start()
	{
		_gameOverTextCanvas.gameObject.SetActive(value: false);
		_whiteFadeAnimator.gameObject.SetActive(value: false);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("TriggerDeathOutsideTimeLoop", OnTriggerDeathOutsideTimeLoop);
		GlobalMessenger.RemoveListener("TriggerDeathOfReality", OnTriggerDeathOfReality);
		GlobalMessenger.RemoveListener("TriggerDeathByVoid", OnTriggerDeathByVoid);
		GlobalMessenger.RemoveListener("TriggerDeathByRingworldEscape", OnTriggerDeathByRingworldEscape);
		GlobalMessenger.RemoveListener("TriggerDeathByDreamworldEscape", OnTriggerDeathByDreamworldEscape);
		GlobalMessenger.RemoveListener("TriggerDeathByQuantumMoon", OnTriggerDeathByQuantumMoon);
		if (_updatingCanvases)
		{
			Canvas.willRenderCanvases -= FontSizeFitting;
		}
	}

	private void OnTriggerDeathOfReality()
	{
		Achievements.Earn(Achievements.Type.TERRIBLE_FATE);
		_deathText.text = UITextLibrary.GetString(UITextType.YouEndedRealityMessage);
		SetupGameOverScreen(4f);
	}

	private void OnTriggerDeathByVoid()
	{
		_deathText.text = UITextLibrary.GetString(UITextType.YouEscapedTheTimeLoopMessage);
		SetupGameOverScreen(6f);
	}

	private void OnTriggerDeathByRingworldEscape()
	{
		_deathText.text = UITextLibrary.GetString(UITextType.YouEscapeOnRingWorld);
		SetupGameOverScreen(6f);
	}

	private void OnTriggerDeathByDreamworldEscape()
	{
		_deathText.text = UITextLibrary.GetString(UITextType.YouEscapeInDreamWorld);
		SetupGameOverScreen(6f);
	}

	private void OnTriggerDeathOutsideTimeLoop()
	{
		DeathType deathType = Locator.GetDeathManager().GetDeathType();
		if (deathType == DeathType.Supernova || deathType == DeathType.Energy || deathType == DeathType.Lava || deathType == DeathType.DreamExplosion)
		{
			_textFadeDelay += 3f;
			_whiteFadeAnimator.gameObject.SetActive(value: true);
			_whiteFadeAnimator.SetImmediate(1f);
			_whiteFadeAnimator.AnimateTo(0f, Vector3.one, 3f, _whiteFadeCurve);
		}
		_deathText.text = UITextLibrary.GetString(UITextType.YouAreDeadMessage);
		SetupGameOverScreen(3f);
	}

	private void OnTriggerDeathByQuantumMoon()
	{
		_deathText.text = UITextLibrary.GetString(UITextType.YouAreQuantumMessage);
		SetupGameOverScreen(6f);
	}

	private void SetupGameOverScreen(float textStayDuration)
	{
		PlayerData.SetPersistentCondition("GAME_OVER_LAST_SAVE", state: true);
		CenterOfTheUniverse.DeactivateUniverse();
		Locator.GetActiveCamera().enabled = false;
		_flashbackCamera.clearFlags = CameraClearFlags.Color;
		_flashbackCamera.enabled = true;
		_flashbackCamera.postProcessing.enabled = false;
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _flashbackCamera);
		_audioListener.enabled = true;
		_gameOverTextCanvas.gameObject.SetActive(value: true);
		base.transform.position = Vector3.zero;
		_deathText.fontSize = _deathTextMaxFontSize;
		_textAnimator.SetImmediate(0f, Vector3.one);
		_updatingCanvases = true;
		_textStayDuration = textStayDuration;
		Canvas.willRenderCanvases += FontSizeFitting;
	}

	private void Update()
	{
		float num = 2f;
		if (!_fadedInText && Time.time > _gameOverTime + _textFadeDelay)
		{
			_textAnimator.AnimateTo(1f, Vector3.one, num, _fadeCurve);
			_fadedInText = true;
		}
		else if (!_fadedOutText && Time.time > _gameOverTime + _textFadeDelay + num + _textStayDuration)
		{
			_textAnimator.AnimateTo(0f, Vector3.one, num, _fadeCurve, invertCurve: true);
			_fadedOutText = true;
		}
		else if (_fadedOutText && _textAnimator.IsComplete() && !_loading)
		{
			LoadManager.LoadScene(OWScene.Credits_Fast);
			_loading = true;
		}
	}

	private void FontSizeFitting()
	{
		int num = _deathText.fontSize - 1;
		if ((_deathText.preferredHeight > _deathTextRectTransform.rect.height || _deathText.preferredWidth > _deathTextRectTransform.rect.width) && num > 0)
		{
			_deathText.fontSize = num;
			return;
		}
		if (num == 1)
		{
			Debug.LogWarning("GameOverController using font size of 1. Please check the input string and rect transform dimensions");
		}
		_updatingCanvases = false;
		Canvas.willRenderCanvases -= FontSizeFitting;
		_textAnimator.SetImmediate(0f, Vector3.one);
		_gameOverTime = Time.time;
		base.enabled = true;
	}
}
