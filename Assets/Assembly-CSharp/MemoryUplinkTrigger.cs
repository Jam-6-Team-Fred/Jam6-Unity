using System.Collections;
using UnityEngine;

public class MemoryUplinkTrigger : MonoBehaviour
{
	[SerializeField]
	private Transform _lockOnTransform;

	[SerializeField]
	private Vector3 _lockOnOffset;

	[SerializeField]
	private TransformAnimator _statueTransformAnimator;

	[SerializeField]
	private OWRenderer _eyeRenderer;

	[SerializeField]
	private OWLight _planetAmbientLight;

	[SerializeField]
	private OWLight _observatoryAmbientLight;

	[SerializeField]
	private TransformAnimator[] _upperLidAnimators;

	[SerializeField]
	private TransformAnimator[] _lowerLidAnimators;

	[SerializeField]
	private OWAudioSource _statueAudioSource;

	private const float HEAD_TURN_DURATION = 4f;

	private static Quaternion _savedStatueRotation;

	private OWCollider _owCollider;

	private PlayerLockOnTargeting _lockOnTargeting;

	private float _eyeGlowLerp;

	private bool _hasSimulationResumed;

	private bool _waitForPlayerGrounded;

	private void Awake()
	{
		SetEyeGlow(0f);
		_owCollider = base.gameObject.GetAddComponent<OWCollider>();
		_owCollider.Assert("AdvancedEffectVolume", isTrigger: true);
		GlobalMessenger<int>.AddListener("StartOfTimeLoop", OnStartOfTimeLoop);
		GlobalMessenger.AddListener("ResumeSimulation", OnResumeSimulation);
		GlobalMessenger.AddListener("MemoryUplinkComplete", OnMemoryUplinkComplete);
		GlobalMessenger.AddListener("LearnLaunchCodes", OnLearnLaunchCodes);
	}

	private void Start()
	{
		_observatoryAmbientLight.SetIntensity(0f);
		_lockOnTargeting = Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>();
		_statueAudioSource.loop = true;
		_statueAudioSource.AssignAudioLibraryClip(AudioType.NomaiHeadStatueRotate_LP);
		_statueAudioSource.SetTrack(OWAudioMixer.TrackName.Death);
	}

	private void OnDestroy()
	{
		GlobalMessenger<int>.RemoveListener("StartOfTimeLoop", OnStartOfTimeLoop);
		GlobalMessenger.RemoveListener("ResumeSimulation", OnResumeSimulation);
		GlobalMessenger.RemoveListener("MemoryUplinkComplete", OnMemoryUplinkComplete);
		GlobalMessenger.RemoveListener("LearnLaunchCodes", OnLearnLaunchCodes);
	}

	private void OnStartOfTimeLoop(int loopCount)
	{
		if (!TimeLoop.IsTimeFlowing())
		{
			for (int i = 0; i < _lowerLidAnimators.Length; i++)
			{
				_lowerLidAnimators[i].transform.Rotate(Vector3.up, -33f);
			}
			for (int j = 0; j < _upperLidAnimators.Length; j++)
			{
				_upperLidAnimators[j].transform.Rotate(Vector3.up, 33f);
			}
		}
		if (PlayerData.KnowsLaunchCodes() || loopCount != 1)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnResumeSimulation()
	{
		_hasSimulationResumed = true;
		_eyeGlowLerp = 1f;
		SetEyeGlow(_eyeGlowLerp);
		_statueTransformAnimator.transform.localRotation = _savedStatueRotation;
		Locator.GetAudioMixer().UnmixMemoryUplink();
		Locator.GetShipLogManager().RevealFact("TH_VILLAGE_X2");
	}

	private void Update()
	{
		if (_waitForPlayerGrounded)
		{
			if (Locator.GetPlayerController().IsGrounded())
			{
				StartCoroutine(BeginUplinkSequence());
				_waitForPlayerGrounded = false;
				base.enabled = false;
			}
		}
		else
		{
			if (!_hasSimulationResumed || _eyeGlowLerp <= 0f)
			{
				base.enabled = false;
			}
			_eyeGlowLerp = Mathf.MoveTowards(_eyeGlowLerp, 0f, Time.deltaTime);
			SetEyeGlow(_eyeGlowLerp);
		}
	}

	private void OnLearnLaunchCodes()
	{
		GlobalMessenger.FireEvent("NomaiStatueActivated");
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		if (hitCollider.CompareTag("PlayerDetector") && PlayerData.KnowsLaunchCodes() && !_hasSimulationResumed)
		{
			_waitForPlayerGrounded = true;
			base.enabled = true;
		}
	}

	private IEnumerator BeginUplinkSequence()
	{
		OWInput.ChangeInputMode(InputMode.None);
		Locator.GetPauseCommandListener().AddPauseCommandLock();
		Locator.GetToolModeSwapper().UnequipTool();
		Locator.GetFlashlight().TurnOff(playAudio: false);
		_lockOnTargeting.LockOn(_lockOnTransform, _lockOnOffset, 3f);
		_statueTransformAnimator.TurnTowardPosition(Locator.GetPlayerTransform().position, 4f);
		_statueAudioSource.SetLocalVolume(0f);
		_statueAudioSource.FadeIn(1f);
		_observatoryAmbientLight.GetLight().enabled = true;
		_observatoryAmbientLight.FadeTo(1f, 10f);
		_planetAmbientLight.FadeTo(0f, 10f);
		Locator.GetAudioMixer().MixMemoryUplink(4f);
		yield return new WaitForSeconds(4f);
		_statueAudioSource.Stop();
		_statueAudioSource.PlayOneShot(AudioType.NomaiDoorStopBig);
		GlobalMessenger.FireEvent("TriggerMemoryUplinkAssetDump");
		yield return new WaitForSeconds(1f);
		GlobalMessenger.FireEvent("TriggerMemoryUplink");
		yield return new WaitForSeconds(0.5f);
		SetEyeGlow(1f);
		for (int i = 0; i < _lowerLidAnimators.Length; i++)
		{
			_lowerLidAnimators[i].RotateToOriginalLocalRotation(1f);
		}
		for (int j = 0; j < _upperLidAnimators.Length; j++)
		{
			_upperLidAnimators[j].RotateToOriginalLocalRotation(1f);
		}
	}

	private void OnMemoryUplinkComplete()
	{
		_savedStatueRotation = _statueTransformAnimator.transform.localRotation;
		TimeLoop.ResetSimulation();
	}

	private void SetEyeGlow(float lerp)
	{
		_eyeRenderer.SetEmissionColor(Color.Lerp(Color.black, _eyeRenderer.GetOriginalEmissionColor(), lerp));
	}

	private void OnDrawGizmos()
	{
		if (_lockOnTransform != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(_lockOnTransform.TransformPoint(_lockOnOffset), 0.05f);
		}
	}
}
