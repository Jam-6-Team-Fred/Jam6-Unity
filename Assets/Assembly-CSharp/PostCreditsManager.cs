using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PostCreditsManager : MonoBehaviour
{
	[SerializeField]
	private OWCamera _camera;

	[SerializeField]
	private float _fadeInTime = 1f;

	[SerializeField]
	private float _fadeOutTime = 1f;

	[Space]
	[SerializeField]
	private GameObject[] _solanumObjects = new GameObject[0];

	[SerializeField]
	private OWAudioSource _campfireAudio;

	[SerializeField]
	private OWAudioSource _campfireOneShot;

	[SerializeField]
	private OWLightController _campfireLightController;

	[SerializeField]
	private Light _campfirePrimaryLight;

	[SerializeField]
	private Renderer _campfireFlamesRenderer;

	[SerializeField]
	private Renderer _campfireSmokeRenderer;

	[SerializeField]
	private Image[] _campfireLitImages = new Image[0];

	[Space]
	[SerializeField]
	private GameObject[] _prisonerObjects = new GameObject[0];

	[SerializeField]
	private OWAudioSource _ruinsOneShot;

	[SerializeField]
	private AnimationCurve _ruinsLightCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private Image[] _ruinsLitImages = new Image[0];

	[SerializeField]
	private OWAudioSource _lanternOneShot;

	[SerializeField]
	private AnimationCurve _lanternLightCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private Image[] _lanternLitImages = new Image[0];

	[Space]
	[SerializeField]
	private GameObject[] _probeObjects = new GameObject[0];

	[SerializeField]
	private Animator _probeAnimator;

	[SerializeField]
	private OWAudioSource _probeAudio;

	[SerializeField]
	private OWAudioSource _musicSource;

	[SerializeField]
	private OWAudioSource _ambientSource;

	private bool _campfireLit;

	private float _campfireLightTime;

	private bool _ruinsLit;

	private float _ruinsLightTime;

	private bool _lanternLit;

	private float _lanternLightTime;

	private float _fade;

	private bool _fadingOut;

	private bool _fadeOutAfterDelay;

	private float _delayedFadeTime;

	private bool _metSolanum;

	private bool _prisonerJoined;

	private bool _probeEnteredEye;

	private void Awake()
	{
		_campfireLit = false;
		_ruinsLit = false;
		_lanternLit = false;
		_fade = 1f;
		_fadingOut = false;
	}

	private void Start()
	{
		if (!OWMath.ApproxEquals(_camera.aspect, 1.777f, 0.01f))
		{
			if (_camera.aspect < 1.777f)
			{
				float num = _camera.aspect / 1.7777778f;
				_camera.mainCamera.rect = new Rect(0f, (1f - num) * 0.5f, 1f, num);
			}
			else
			{
				float num2 = 1.7777778f / _camera.aspect;
				_camera.mainCamera.rect = new Rect((1f - num2) * 0.5f, 0f, num2, 1f);
			}
		}
		_camera.postProcessingSettings.colorGrading.postExposure = -10f;
		_campfireLightController.FadeTo(0f, 0f);
		_campfireFlamesRenderer.material.SetTextureOffset("_MainTex", new Vector2(1f, 1f));
		_campfireSmokeRenderer.material.SetAlpha(0f);
		_campfireSmokeRenderer.material.SetFloat("_MaskBias", 0f);
		for (int i = 0; i < _campfireLitImages.Length; i++)
		{
			_campfireLitImages[i].color = Color.black;
		}
		for (int j = 0; j < _ruinsLitImages.Length; j++)
		{
			_ruinsLitImages[j].color = new Color(1f, 1f, 1f, 0f);
		}
		for (int k = 0; k < _lanternLitImages.Length; k++)
		{
			_lanternLitImages[k].color = Color.black;
		}
		_probeAnimator.enabled = false;
		_metSolanum = PlayerData.GetPersistentCondition("MET_SOLANUM");
		_prisonerJoined = DialogueConditionManager.SharedInstance.GetConditionState("PRISONER_JOIN");
		_probeEnteredEye = DialogueConditionManager.SharedInstance.GetConditionState("PROBE_ENTERED_EYE");
		if (!_metSolanum)
		{
			for (int l = 0; l < _solanumObjects.Length; l++)
			{
				_solanumObjects[l].SetActive(value: false);
			}
		}
		if (!_prisonerJoined)
		{
			for (int m = 0; m < _prisonerObjects.Length; m++)
			{
				_prisonerObjects[m].SetActive(value: false);
			}
		}
		if (!_probeEnteredEye)
		{
			for (int n = 0; n < _probeObjects.Length; n++)
			{
				_probeObjects[n].SetActive(value: false);
			}
		}
		_musicSource.PlayDelayed(8f);
		_ambientSource.SetLocalVolume(0f);
		_campfireAudio.SetLocalVolume(0f);
	}

	private void Update()
	{
		if (_probeAnimator.enabled && _probeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
		{
			_probeAnimator.enabled = false;
			_fadeOutAfterDelay = true;
			_delayedFadeTime = Time.time + 1f;
		}
		if (_campfireLit)
		{
			float num = Mathf.Clamp01(Time.timeSinceLevelLoad - _campfireLightTime);
			_campfireFlamesRenderer.material.SetTextureOffset("_MainTex", new Vector2(1f, 1f - num));
			_campfireSmokeRenderer.material.SetAlpha(num);
			_campfireSmokeRenderer.material.SetFloat("_MaskBias", num * 0.95f);
			Color color = new Color(_campfirePrimaryLight.intensity, _campfirePrimaryLight.intensity, _campfirePrimaryLight.intensity, 1f);
			for (int i = 0; i < _campfireLitImages.Length; i++)
			{
				_campfireLitImages[i].color = color;
			}
		}
		if (_ruinsLit)
		{
			float time = Mathf.Max(Time.timeSinceLevelLoad - _ruinsLightTime, 0f);
			float a = _ruinsLightCurve.Evaluate(time);
			Color color2 = new Color(1f, 1f, 1f, a);
			for (int j = 0; j < _ruinsLitImages.Length; j++)
			{
				_ruinsLitImages[j].color = color2;
			}
		}
		if (_lanternLit)
		{
			float time2 = Mathf.Max(Time.timeSinceLevelLoad - _lanternLightTime, 0f);
			float num2 = _lanternLightCurve.Evaluate(time2);
			Color color3 = new Color(num2, num2, num2, 1f);
			for (int k = 0; k < _lanternLitImages.Length; k++)
			{
				_lanternLitImages[k].color = color3;
			}
		}
		if (_fadingOut)
		{
			_fade = Mathf.MoveTowards(_fade, 1f, Time.deltaTime / _fadeOutTime);
		}
		else
		{
			_fade = Mathf.MoveTowards(_fade, 0f, Time.deltaTime / _fadeInTime);
			if (!_ambientSource.isPlaying)
			{
				_ambientSource.FadeIn(8f);
			}
			if (_fadeOutAfterDelay && Time.time >= _delayedFadeTime)
			{
				_fadeOutAfterDelay = false;
				FadeOut();
			}
		}
		_camera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(0f, -10f, _fade);
		if (_fadingOut && _fade >= 1f)
		{
			LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 0.5f);
			base.enabled = false;
		}
	}

	private void LightCampfire()
	{
		_campfireLit = true;
		_campfireLightTime = Time.timeSinceLevelLoad;
		_campfireAudio.FadeIn(1f);
		_campfireOneShot.PlayOneShot(AudioType.TH_Campfire_Ignite);
		_campfireLightController.FadeTo(1f, 1f);
	}

	private void LightRuins()
	{
		_ruinsLit = true;
		_ruinsLightTime = Time.timeSinceLevelLoad;
		_ruinsOneShot.PlayOneShot(AudioType.PostCredit_RuinReveal);
	}

	private void LightLantern()
	{
		_lanternLit = true;
		_lanternLightTime = Time.timeSinceLevelLoad;
		_lanternOneShot.PlayOneShot(AudioType.PostCredit_LanternLight);
	}

	private void ShowProbe()
	{
		_probeAnimator.enabled = true;
		_probeAudio.Play();
	}

	private void FadeOut()
	{
		_fadingOut = true;
		_ambientSource.FadeOut(_fadeOutTime);
		_campfireAudio.FadeOut(_fadeOutTime);
	}

	public void TriggerBeat1()
	{
		if (_metSolanum)
		{
			LightCampfire();
			return;
		}
		if (_prisonerJoined)
		{
			LightRuins();
			return;
		}
		if (_probeEnteredEye)
		{
			ShowProbe();
			return;
		}
		_fadeOutAfterDelay = true;
		_delayedFadeTime = Time.time + 5f;
	}

	public void TriggerBeat2()
	{
		if (_metSolanum)
		{
			if (_prisonerJoined)
			{
				LightRuins();
				return;
			}
			if (_probeEnteredEye)
			{
				ShowProbe();
				return;
			}
			_fadeOutAfterDelay = true;
			_delayedFadeTime = Time.time + 3f;
		}
		else if (_prisonerJoined)
		{
			if (_probeEnteredEye)
			{
				ShowProbe();
				return;
			}
			_fadeOutAfterDelay = true;
			_delayedFadeTime = Time.time + 3f;
		}
	}

	public void TriggerBeat3()
	{
		if (_metSolanum && _prisonerJoined)
		{
			LightLantern();
		}
	}

	public void TriggerBeat4()
	{
		if (_metSolanum && _prisonerJoined)
		{
			if (_probeEnteredEye)
			{
				ShowProbe();
				return;
			}
			_fadeOutAfterDelay = true;
			_delayedFadeTime = Time.time + 3f;
		}
	}
}
