using System.Collections.Generic;
using UnityEngine;

public class CosmicInflationController : MonoBehaviour
{
	private enum State
	{
		Inactive = 0,
		Forming = 1,
		ReadyToCollapse = 2,
		Collapsing = 3,
		Inflating = 4,
		HotBigBang = 5
	}

	public class InflationObject
	{
		private Transform transform;

		private OWRigidbody body;

		private Vector3 origPosition;

		private Vector3 targetPosition;

		public InflationObject(Transform transform, Transform centerOfInflation, float inflationDistance)
		{
			this.transform = transform;
			Vector3 vector = transform.position - centerOfInflation.position;
			vector.y = 0f;
			origPosition = transform.position;
			targetPosition = origPosition + vector.normalized * inflationDistance;
		}

		public InflationObject(OWRigidbody body, Transform centerOfInflation, float inflationDistance)
		{
			transform = body.transform;
			this.body = body;
			Vector3 vector = transform.position - centerOfInflation.position;
			vector.y = 0f;
			origPosition = transform.position;
			targetPosition = origPosition + vector.normalized * inflationDistance;
		}

		public void UpdatePosition(float inflationFraction)
		{
			Vector3 vector = Vector3.Lerp(origPosition, targetPosition, inflationFraction);
			if (body != null)
			{
				body.MoveToPosition(vector);
			}
			else
			{
				transform.position = vector;
			}
		}
	}

	private const float _inflationDist = 1000f;

	private const float _inflationDuration = 3f;

	[SerializeField]
	private QuantumCampsiteController _campsiteController;

	[Space]
	[SerializeField]
	private float _sphereInflationDuration = 0.6f;

	[SerializeField]
	private float _sphereInflationDelay = 5f;

	[SerializeField]
	private float _bigBangParticlesPlayOffset;

	[SerializeField]
	private float _bigBangParticlesCutoffTime = 6f;

	[SerializeField]
	private float _bigBangIgniteDelay;

	[SerializeField]
	private float _bigBangAudioDelay;

	[SerializeField]
	private float _bigBangHelmetCrackDelay;

	[SerializeField]
	private float _bigBangMusicDelay = 0.7f;

	[SerializeField]
	private float _inflationScale = 340f;

	[SerializeField]
	private float _postInflationRate = 0.115f;

	[Space]
	[SerializeField]
	private AnimationCurve _inflationCurve;

	[SerializeField]
	private AnimationCurve _sphereCurve;

	[Space]
	[SerializeField]
	private ParticleSystem[] _smokeSphereParticles;

	[SerializeField]
	private ParticleSystem[] _inflationParticles;

	[SerializeField]
	private ParticleSystem[] _bigBangParticles;

	[SerializeField]
	private OWRenderer[] _smokeStreamRenderers;

	[Space]
	[SerializeField]
	private bool _inflateOnButtonPress;

	[SerializeField]
	private OWAudioSource _musicSource;

	[SerializeField]
	private OWAudioSource _sfxSource;

	[SerializeField]
	private OWAudioSource _travelerFinaleSource;

	[SerializeField]
	private TravelerEyeController[] _travelers;

	[SerializeField]
	private Campfire _campfire;

	[SerializeField]
	private PossibilitySphereController _possibilitySphereController;

	[SerializeField]
	private Transform _possibilitySphereRoot;

	[SerializeField]
	private Transform _inflationSphereRoot;

	[SerializeField]
	private FluidVolume _repelVolume;

	[Header("Post Collapse")]
	[SerializeField]
	private Transform _playerPostCollapseSocket;

	[SerializeField]
	private Transform _altPlayerPostCollapseSocket;

	[SerializeField]
	private GameObject _altTravelerToHidePostCollapse;

	[Space]
	[SerializeField]
	private OWLight _inflationLight;

	[SerializeField]
	private Transform[] _inflationObjects;

	[SerializeField]
	private GameObject[] _activateOnInflation;

	[SerializeField]
	private GameObject[] _deactivateOnInflation;

	[SerializeField]
	private GameObject[] _deactivateAfterInflationLightFades;

	[SerializeField]
	private OWRenderer[] _groundRenderers;

	[Space]
	[SerializeField]
	private TessellatedSphereRenderer _bigBangRenderer;

	[SerializeField]
	private Material _endBigBangMaterial;

	[SerializeField]
	private GameObject _lightShafts;

	[SerializeField]
	[ColorUsage(false, true)]
	private Color _bigBangStartColor;

	[SerializeField]
	[ColorUsage(false, true)]
	private Color _bigBangEndColor;

	[Space]
	[SerializeField]
	private ForceVolume _gravityVolume;

	[SerializeField]
	private EndlessCylinder _endlessCylinder;

	[SerializeField]
	private OWTriggerVolume _smokeSphereTrigger;

	[SerializeField]
	private OWTriggerVolume _probeDestroyTrigger;

	[SerializeField]
	private OWTriggerVolume _bigBangTrigger;

	[SerializeField]
	private InteractReceiver _interactReceiver;

	private State _state;

	private List<InflationObject> _inflationList;

	private Material _lightShaftMaterial;

	private Renderer[] _lightShaftRenderers;

	private Color _lightShaftColor;

	private float _stateChangeTime;

	private int _playingTravelerCount;

	private int _maxTravelerCount;

	private float _crossFadeMusicTime;

	private float _startFormationTime;

	private float _finishFormationTime;

	private bool _waitForAllPlaying;

	private bool _waitForCrossfade;

	private bool _waitForMusicEnd;

	private float _inflationStartScale;

	private Vector3 _collapseStartPos;

	private float _inflationLightStartRange;

	private float _bigBangParticlesStartTime;

	private bool _hasInflationLightFaded;

	private bool _hasEnabledInteraction;

	private bool _haveBigBangParticlesPlayed;

	private bool _haveBigBangParticlesStopped;

	private bool _hasSphereInflationStarted;

	private bool _hasBigBangIgnited;

	private bool _hasBigBangAudioPlayed;

	private bool _hasHelmetCracked;

	private Vector3 _playerAnchor;

	private void Awake()
	{
		_interactReceiver.OnPressInteract += OnPressInteract;
		_smokeSphereTrigger.OnEntry += OnEnterFogSphere;
		_bigBangTrigger.OnEntry += OnEnterBigBang;
		for (int i = 0; i < _travelers.Length; i++)
		{
			_travelers[i].OnStartPlaying += OnTravelerStartPlaying;
		}
		_bigBangStartColor = _bigBangStartColor.linear;
		_bigBangEndColor = _bigBangEndColor.linear;
	}

	private void Start()
	{
		base.gameObject.SetActive(value: false);
		_bigBangRenderer.enabled = false;
		_lightShafts.SetActive(value: false);
		_bigBangTrigger.SetTriggerActivation(active: false);
		_interactReceiver.DisableInteraction();
		_interactReceiver.SetPromptText(UITextType.InflationPrompt);
		_inflationLight.SetIntensity(0f);
		for (int i = 0; i < _smokeStreamRenderers.Length; i++)
		{
			_smokeStreamRenderers[i].SetActivation(active: false);
		}
		if (Locator.GetEyeStateManager().GetState() == EyeState.BigBang)
		{
			base.gameObject.SetActive(value: true);
			FinishFormation();
		}
	}

	private void OnDestroy()
	{
		_interactReceiver.OnPressInteract -= OnPressInteract;
		_smokeSphereTrigger.OnEntry -= OnEnterFogSphere;
		_bigBangTrigger.OnEntry -= OnEnterBigBang;
		for (int i = 0; i < _travelers.Length; i++)
		{
			_travelers[i].OnStartPlaying -= OnTravelerStartPlaying;
		}
	}

	private void OnTravelerStartPlaying()
	{
		if (_state == State.Inactive)
		{
			Locator.GetPauseCommandListener().AddPauseCommandLock();
			_state = State.Forming;
			_stateChangeTime = Time.time;
			_possibilitySphereRoot.localScale = Vector3.zero;
			base.gameObject.SetActive(value: true);
			_smokeSphereTrigger.SetTriggerActivation(active: false);
			_probeDestroyTrigger.SetTriggerActivation(active: false);
			_repelVolume.SetVolumeActivation(active: true);
			_waitForAllPlaying = true;
			_maxTravelerCount = 0;
			for (int i = 0; i < _travelers.Length; i++)
			{
				if (_travelers[i].gameObject.activeInHierarchy)
				{
					_maxTravelerCount++;
				}
			}
		}
		if (_playingTravelerCount < _smokeStreamRenderers.Length)
		{
			_smokeStreamRenderers[_playingTravelerCount].SetActivation(active: true);
		}
		_playingTravelerCount++;
	}

	private void OnEnterFogSphere(GameObject obj)
	{
		if (obj.CompareTag("PlayerCameraDetector") && _state == State.ReadyToCollapse)
		{
			_smokeSphereTrigger.SetTriggerActivation(active: false);
			_probeDestroyTrigger.SetTriggerActivation(active: false);
			StartCollapse();
		}
	}

	private void FixedUpdate()
	{
		if (_state == State.Forming)
		{
			UpdateFormation();
		}
		else if (_state == State.Collapsing)
		{
			UpdateCollapse();
		}
		else if (_state == State.Inflating)
		{
			UpdateInflation();
		}
		else if (_state == State.HotBigBang)
		{
			UpdateHotBigBang();
		}
	}

	private void UpdateFormation()
	{
		if (_waitForAllPlaying && _playingTravelerCount == _maxTravelerCount)
		{
			MonoBehaviour.print("all travelers are playing");
			_waitForAllPlaying = false;
			_waitForCrossfade = true;
			_crossFadeMusicTime = Time.time + _travelers[0].GetSecondsUntilCrossfadeToFinale();
			AudioClip travelerMusicEndClip = _campsiteController.GetTravelerMusicEndClip();
			_travelerFinaleSource.clip = travelerMusicEndClip;
			_startFormationTime = Time.time;
			_finishFormationTime = _crossFadeMusicTime + travelerMusicEndClip.length - 4f;
		}
		if (_waitForCrossfade && Time.time >= _crossFadeMusicTime)
		{
			MonoBehaviour.print("crossfade time!");
			_waitForCrossfade = false;
			_waitForMusicEnd = true;
			_travelerFinaleSource.SetLocalVolume(0f);
			_travelerFinaleSource.FadeIn(5f);
			for (int i = 0; i < _travelers.Length; i++)
			{
				_travelers[i].OnCrossfadeToFinale(5f);
			}
		}
		if (_waitForCrossfade || _waitForMusicEnd)
		{
			float num = Mathf.InverseLerp(_startFormationTime, _finishFormationTime, Time.time);
			_possibilitySphereRoot.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, num);
			if (num >= 1f)
			{
				_waitForMusicEnd = false;
				FinishFormation();
			}
		}
	}

	private void FinishFormation()
	{
		DialogueConditionManager.SharedInstance.SetConditionState("JamSessionIsOver", conditionState: true);
		for (int i = 0; i < _travelers.Length; i++)
		{
			_travelers[i].OnStopCosmicJamSession();
		}
		_campfire.SetState(Campfire.State.UNLIT);
		_campfire.SetInteractionEnabled(enabled: false);
		_state = State.ReadyToCollapse;
		_stateChangeTime = Time.time;
		_possibilitySphereController.Activate();
		_possibilitySphereRoot.localScale = Vector3.one;
		_smokeSphereTrigger.SetTriggerActivation(active: true);
		_probeDestroyTrigger.SetTriggerActivation(active: true);
		_repelVolume.SetVolumeActivation(active: false);
		for (int j = 0; j < _smokeSphereParticles.Length; j++)
		{
			_smokeSphereParticles[j].Play();
		}
		for (int k = 0; k < _smokeStreamRenderers.Length; k++)
		{
			_smokeStreamRenderers[k].SetActivation(active: false);
		}
	}

	private void StartCollapse()
	{
		ReticleController.Hide();
		Locator.GetFlashlight().TurnOff(playAudio: false);
		Locator.GetPromptManager().SetPromptsVisible(visible: false);
		_state = State.Collapsing;
		_stateChangeTime = Time.time;
		_collapseStartPos = _possibilitySphereRoot.localPosition;
		_smokeSphereTrigger.SetTriggerActivation(active: false);
		_inflationLight.FadeTo(1f, 1f);
		Vector3 vector = Locator.GetPlayerBody().GetPosition() - _possibilitySphereRoot.position;
		vector.y = 0f;
		_possibilitySphereController.OnCollapse();
		if (_campsiteController.GetUseAltPostCollapseSocket())
		{
			_playerPostCollapseSocket = _altPlayerPostCollapseSocket;
			_altTravelerToHidePostCollapse.SetActive(value: false);
		}
		Locator.GetPlayerBody().SetPosition(_playerPostCollapseSocket.position);
		Locator.GetPlayerBody().SetRotation(_playerPostCollapseSocket.rotation);
		Locator.GetPlayerBody().SetVelocity(-_playerPostCollapseSocket.forward);
		Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(_possibilitySphereRoot, 2f);
		OWInput.ChangeInputMode(InputMode.None);
		for (int i = 0; i < _smokeSphereParticles.Length; i++)
		{
			_smokeSphereParticles[i].Stop();
		}
	}

	private void UpdateCollapse()
	{
		float t = Mathf.InverseLerp(_stateChangeTime, _stateChangeTime + 0.5f, Time.time);
		float num = Mathf.InverseLerp(_stateChangeTime, _stateChangeTime + 2f, Time.time);
		float num2 = Mathf.InverseLerp(_stateChangeTime, _stateChangeTime + 3.5f, Time.time);
		_possibilitySphereRoot.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.01f, t);
		_possibilitySphereRoot.localPosition = Vector3.Lerp(_collapseStartPos, new Vector3(0f, 0f, 0f), Mathf.SmoothStep(0f, 1f, num));
		Vector3 vector = Locator.GetPlayerCamera().transform.position - _possibilitySphereRoot.transform.position;
		_inflationLight.transform.position = _possibilitySphereRoot.transform.position + vector.normalized * 0.2f;
		if (_inflateOnButtonPress)
		{
			if (num >= 1f && !_hasEnabledInteraction)
			{
				_interactReceiver.EnableInteraction();
				Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().BreakLock();
				_hasEnabledInteraction = true;
			}
		}
		else if (num2 >= 1f)
		{
			StartInflation();
		}
	}

	private void OnPressInteract()
	{
		_interactReceiver.DisableInteraction();
		StartInflation();
	}

	private void StartInflation()
	{
		_state = State.Inflating;
		_stateChangeTime = Time.time;
		_inflationSphereRoot.localScale = _possibilitySphereRoot.localScale;
		_inflationStartScale = _inflationSphereRoot.localScale.x;
		_inflationLightStartRange = _inflationLight.GetLight().range;
		_bigBangRenderer.sharedMaterial.SetFloat("_ColorTime", 0f);
		_bigBangRenderer.sharedMaterial.SetColor("_Color", _bigBangStartColor);
		_bigBangRenderer.enabled = true;
		_inflationList = new List<InflationObject>(_inflationObjects.Length + 1);
		for (int i = 0; i < _activateOnInflation.Length; i++)
		{
			_activateOnInflation[i].SetActive(value: true);
		}
		for (int j = 0; j < _deactivateOnInflation.Length; j++)
		{
			_deactivateOnInflation[j].SetActive(value: false);
		}
		for (int k = 0; k < _inflationObjects.Length; k++)
		{
			Collider[] componentsInChildren = _inflationObjects[k].GetComponentsInChildren<Collider>();
			for (int l = 0; l < componentsInChildren.Length; l++)
			{
				componentsInChildren[l].enabled = false;
			}
			_inflationList.Add(new InflationObject(_inflationObjects[k], base.transform, 1000f));
		}
		Locator.GetPlayerController().SetColliderActivation(active: false);
		Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().BreakLock();
		_gravityVolume.SetVolumeActivation(active: false);
		_endlessCylinder.SetActivation(active: false);
		_inflationList.Add(new InflationObject(Locator.GetPlayerBody(), base.transform, 1000f));
		for (int m = 0; m < _inflationParticles.Length; m++)
		{
			_inflationParticles[m].Play();
		}
		_sfxSource.PlayOneShot(AudioType.EyeCosmicInflation);
		RumbleManager.PlayCosmicInflation();
	}

	private void UpdateInflation()
	{
		float time = Mathf.InverseLerp(_stateChangeTime, _stateChangeTime + 3f, Time.time);
		time = _inflationCurve.Evaluate(time);
		for (int i = 0; i < _inflationList.Count; i++)
		{
			_inflationList[i].UpdatePosition(time);
		}
		float ditherFade = Mathf.InverseLerp(_stateChangeTime + 0.6f, _stateChangeTime + 1.5f, Time.time);
		for (int j = 0; j < _groundRenderers.Length; j++)
		{
			_groundRenderers[j].SetDitherFade(ditherFade);
		}
		if (!_hasInflationLightFaded)
		{
			float t = Mathf.InverseLerp(_stateChangeTime, _stateChangeTime + 1f, Time.time);
			_inflationLight.GetLight().range = Mathf.Lerp(_inflationLightStartRange, 400f, t);
			float num = Mathf.InverseLerp(0.2f, 0.3f, time);
			_inflationLight.SetIntensity(1f - num);
			if (num >= 1f)
			{
				_hasInflationLightFaded = true;
				for (int k = 0; k < _deactivateAfterInflationLightFades.Length; k++)
				{
					_deactivateAfterInflationLightFades[k].SetActive(value: false);
				}
			}
		}
		Vector3 toDirection = _inflationSphereRoot.transform.position - Locator.GetPlayerCamera().transform.position;
		Quaternion deltaRotation = Quaternion.FromToRotation(Locator.GetPlayerCamera().transform.forward, toDirection);
		Locator.GetPlayerBody().AddRotation(deltaRotation);
		float time2 = Mathf.InverseLerp(_stateChangeTime + _sphereInflationDelay, _stateChangeTime + _sphereInflationDelay + _sphereInflationDuration, Time.time);
		time2 = _sphereCurve.Evaluate(time2);
		_inflationSphereRoot.localScale = Vector3.one * Mathf.Lerp(_inflationStartScale, _inflationScale, time2);
		if (time2 > 0f && !_hasSphereInflationStarted)
		{
			_hasSphereInflationStarted = true;
			_sfxSource.PlayOneShot(AudioType.EyeSphereInflation);
		}
		if ((!_haveBigBangParticlesPlayed && Time.time > _stateChangeTime + _sphereInflationDelay + _sphereInflationDuration + _bigBangParticlesPlayOffset) || time2 >= 1f)
		{
			_haveBigBangParticlesPlayed = true;
			_bigBangParticlesStartTime = Time.time;
			for (int l = 0; l < _bigBangParticles.Length; l++)
			{
				_bigBangParticles[l].Play();
			}
		}
		if (time2 >= 1f)
		{
			StartHotBigBang();
		}
	}

	private void StartHotBigBang()
	{
		_state = State.HotBigBang;
		_stateChangeTime = Time.time;
		_bigBangTrigger.SetTriggerActivation(active: true);
		_lightShafts.SetActive(value: true);
		_lightShaftRenderers = _lightShafts.GetComponentsInChildren<Renderer>();
		_lightShaftMaterial = new Material(_lightShaftRenderers[0].sharedMaterial);
		_lightShaftColor = _lightShaftMaterial.color;
		_lightShaftMaterial.color = new Color(1f, 1f, 1f, 0f);
		for (int i = 0; i < _lightShaftRenderers.Length; i++)
		{
			_lightShaftRenderers[i].sharedMaterial = _lightShaftMaterial;
		}
		_playerAnchor = Locator.GetPlayerTransform().position;
		_bigBangTrigger.SetTriggerActivation(active: true);
		OWInput.ChangeInputMode(InputMode.Character);
		if (Locator.GetToolModeSwapper().GetProbeLauncher().GetActiveProbe() != null)
		{
			Object.Destroy(Locator.GetToolModeSwapper().GetProbeLauncher().GetActiveProbe()
				.gameObject);
				Debug.Log("PROBE DESTROYED (LEFT BEHIND)");
			}
		}

		private void UpdateHotBigBang()
		{
			_inflationSphereRoot.localScale += Vector3.one * _postInflationRate * (60f * Time.fixedDeltaTime);
			float num = _stateChangeTime + _bigBangIgniteDelay;
			float num2 = Mathf.InverseLerp(num, num + 1f, Time.time);
			_bigBangRenderer.sharedMaterial.SetFloat("_ColorTime", num2 * num2);
			_bigBangRenderer.sharedMaterial.SetColor("_Color", Color.Lerp(_bigBangStartColor, _bigBangEndColor, num2 * num2));
			if (!_hasBigBangIgnited && Time.time > num)
			{
				_hasBigBangIgnited = true;
				_musicSource.PlayDelayed(_bigBangMusicDelay);
			}
			if (_haveBigBangParticlesPlayed && !_haveBigBangParticlesStopped && Time.time > _bigBangParticlesStartTime + _bigBangParticlesCutoffTime)
			{
				_haveBigBangParticlesStopped = true;
				for (int i = 0; i < _bigBangParticles.Length; i++)
				{
					_bigBangParticles[i].Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
				}
			}
			if (!_hasHelmetCracked && Time.time > _stateChangeTime + _bigBangHelmetCrackDelay)
			{
				_hasHelmetCracked = true;
				GlobalMessenger.FireEvent("BigBangHelmetCrack");
			}
			if (!_hasBigBangAudioPlayed && Time.time > _stateChangeTime + _bigBangAudioDelay)
			{
				_hasBigBangAudioPlayed = true;
				_sfxSource.PlayOneShot(AudioType.EyeBigBang);
			}
			float t = Mathf.InverseLerp(num + 1f, num + 1.5f, Time.time);
			_lightShaftMaterial.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), _lightShaftColor, t);
			Vector3 vector = _playerAnchor - Locator.GetPlayerTransform().position;
			Locator.GetPlayerBody().SetVelocity(vector * Time.deltaTime);
		}

		private void OnEnterBigBang(GameObject hitObject)
		{
			if (hitObject.CompareTag("PlayerDetector"))
			{
				Locator.GetDeathManager().KillPlayer(DeathType.BigBang);
				PlayerData.SaveEyeCompletion();
			}
			else if (hitObject.CompareTag("ProbeDetector") && Locator.GetProbe() != null)
			{
				Locator.GetProbe().ExternalRetrieve();
			}
		}
	}
