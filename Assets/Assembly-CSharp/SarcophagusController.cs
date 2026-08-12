using UnityEngine;

public class SarcophagusController : MonoBehaviour
{
	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private AudioVolume _quietAudioVolume;

	[Header("Mind Projector")]
	[SerializeField]
	private MindProjectorTrigger _mindProjector;

	[SerializeField]
	private OWTriggerVolume _mindProjectorDisableVolume;

	[Header("Seals")]
	[SerializeField]
	private DreamObjectProjector _firstSealProjector;

	[SerializeField]
	private DreamObjectProjector _secondSealProjector;

	[SerializeField]
	private DreamObjectProjector _thirdSealProjector;

	[SerializeField]
	private DreamObjectProjection[] _sealProjections = new DreamObjectProjection[0];

	[SerializeField]
	private Animation _sarcophagusAnimation;

	[SerializeField]
	private bool _debugUnlock;

	[Header("Secret Tunnel")]
	[SerializeField]
	private OWTriggerVolume _tunnelSwapVolume;

	[SerializeField]
	private OWTriggerVolume _tunnelEntrywayTrigger;

	[SerializeField]
	private OWRenderer[] _sarcoBackRenderers = new OWRenderer[0];

	[SerializeField]
	private OWCollider[] _sarcoBackColliders = new OWCollider[0];

	[SerializeField]
	private OWRenderer[] _tunnelRenderers = new OWRenderer[0];

	[SerializeField]
	private OWCollider[] _tunnelColliders = new OWCollider[0];

	[SerializeField]
	private OWRenderer[] _sarcoBackFade = new OWRenderer[0];

	[SerializeField]
	private float _sarcoBackFadeStartDist = 2f;

	[SerializeField]
	private float _sarcoBackFadeEndDist = 1f;

	private bool _isOpen;

	private bool _isSlightlyOpen;

	private bool _attemptOpenAfterDelay;

	private float _openAttemptTime;

	private bool _waitToRumble;

	private float _rumbleTime;

	private bool _waitToProject;

	private float _projectTime;

	private bool _playerInTunnelSwapVolume;

	private bool _playerInSecretTunnel;

	public DreamObjectProjector firstSealProjector => _firstSealProjector;

	public DreamObjectProjector secondSealProjector => _secondSealProjector;

	public DreamObjectProjector thirdSealProjector => _thirdSealProjector;

	private void Awake()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract += OnPressInteract;
		}
		if (_mindProjectorDisableVolume != null)
		{
			_mindProjectorDisableVolume.OnExit += OnExitMindProjectorDisableVolume;
		}
		if (_tunnelSwapVolume != null)
		{
			_tunnelSwapVolume.OnEntry += OnEnterTunnelSwapVolume;
			_tunnelSwapVolume.OnExit += OnExitTunnelSwapVolume;
		}
		if (_tunnelEntrywayTrigger != null)
		{
			_tunnelEntrywayTrigger.OnEntry += OnEnterTunnel;
			_tunnelEntrywayTrigger.OnExit += OnExitTunnel;
		}
	}

	private void Start()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.SetPromptText(UITextType.RotateGearPrompt);
		}
		if (_quietAudioVolume != null)
		{
			_quietAudioVolume.SetVolumeActivation(active: false);
		}
		if (_tunnelSwapVolume != null)
		{
			_tunnelSwapVolume.SetTriggerActivation(active: false);
		}
		if (_tunnelEntrywayTrigger != null)
		{
			_tunnelEntrywayTrigger.SetTriggerActivation(active: false);
		}
		UpdateTunnelState();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract -= OnPressInteract;
		}
		if (_mindProjectorDisableVolume != null)
		{
			_mindProjectorDisableVolume.OnExit -= OnExitMindProjectorDisableVolume;
		}
		if (_tunnelSwapVolume != null)
		{
			_tunnelSwapVolume.OnEntry -= OnEnterTunnelSwapVolume;
			_tunnelSwapVolume.OnExit -= OnExitTunnelSwapVolume;
		}
		if (_tunnelEntrywayTrigger != null)
		{
			_tunnelEntrywayTrigger.OnEntry -= OnEnterTunnel;
			_tunnelEntrywayTrigger.OnExit -= OnExitTunnel;
		}
	}

	private void Update()
	{
		if (_attemptOpenAfterDelay && Time.time > _openAttemptTime)
		{
			bool flag = false;
			for (int i = 0; i < _sealProjections.Length; i++)
			{
				if (_sealProjections[i].IsVisible())
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (!_isSlightlyOpen)
				{
					_sarcophagusAnimation.Play("Sarcophagus_OpenFail_1");
					_oneShotSource.PlayOneShot(AudioType.Sarcophagus_OpenFail);
					_isSlightlyOpen = true;
				}
				else if (!_sarcophagusAnimation.isPlaying)
				{
					_sarcophagusAnimation.Play("Sarcophagus_OpenFail_2");
					_oneShotSource.PlayOneShot(AudioType.Sarcophagus_OpenFail);
				}
				if (!_mindProjector.IsActive() && !_waitToProject)
				{
					_waitToProject = true;
					_waitToRumble = true;
					_rumbleTime = Time.time + 2f;
					_projectTime = Time.time + 4f;
					if (_quietAudioVolume != null)
					{
						_quietAudioVolume.SetVolumeActivation(active: true);
					}
				}
			}
			else
			{
				_oneShotSource.PlayOneShot(AudioType.Sarcophagus_Open);
				_sarcophagusAnimation.Play(_isSlightlyOpen ? "Sarcophagus_Open_2" : "Sarcophagus_Open_1");
				_mindProjector.SetProjectorActive(active: false);
				_isOpen = true;
				if (_quietAudioVolume != null)
				{
					_quietAudioVolume.SetVolumeActivation(active: true);
				}
				if (_tunnelSwapVolume != null)
				{
					_tunnelSwapVolume.SetTriggerActivation(active: true);
				}
				if (_tunnelEntrywayTrigger != null)
				{
					_tunnelEntrywayTrigger.SetTriggerActivation(active: true);
				}
			}
			_interactReceiver.EnableInteraction();
			_attemptOpenAfterDelay = false;
		}
		if (_waitToRumble && Time.time > _rumbleTime)
		{
			_waitToRumble = false;
			_oneShotSource.PlayOneShot(AudioType.Sarcophagus_SomethingIsComing);
		}
		if (_waitToProject && Time.time > _projectTime)
		{
			_waitToProject = false;
			_mindProjector.SetProjectorActive(active: true);
			Locator.GetShipLogManager().RevealFact("IP_DREAM_LAKE_X2");
			if (_quietAudioVolume != null)
			{
				_quietAudioVolume.SetVolumeActivation(active: false);
			}
		}
		if (_sarcoBackFade.Length != 0)
		{
			if (_playerInTunnelSwapVolume)
			{
				Vector3 position = Locator.GetPlayerCamera().transform.position;
				Vector3 vector = base.transform.InverseTransformPoint(position);
				float fade = Mathf.InverseLerp(_sarcoBackFadeStartDist, _sarcoBackFadeEndDist, vector.z);
				for (int j = 0; j < _sarcoBackFade.Length; j++)
				{
					_sarcoBackFade[j].SetFade(fade);
				}
			}
			else
			{
				for (int k = 0; k < _sarcoBackFade.Length; k++)
				{
					_sarcoBackFade[k].SetFade(_playerInSecretTunnel ? 1f : 0f);
				}
			}
		}
		if (!_waitToProject && !_attemptOpenAfterDelay && !_playerInTunnelSwapVolume)
		{
			base.enabled = false;
		}
	}

	private void UpdateTunnelState()
	{
		bool flag = _playerInTunnelSwapVolume || _playerInSecretTunnel;
		for (int i = 0; i < _sarcoBackRenderers.Length; i++)
		{
			_sarcoBackRenderers[i].SetActivation(!flag);
		}
		for (int j = 0; j < _sarcoBackColliders.Length; j++)
		{
			_sarcoBackColliders[j].SetActivation(!flag);
		}
		for (int k = 0; k < _tunnelRenderers.Length; k++)
		{
			_tunnelRenderers[k].SetActivation(flag);
		}
		for (int l = 0; l < _tunnelColliders.Length; l++)
		{
			_tunnelColliders[l].SetActivation(flag);
		}
	}

	private void OnPressInteract()
	{
		if (!_isOpen)
		{
			_interactReceiver.DisableInteraction();
			_attemptOpenAfterDelay = true;
			_openAttemptTime = Time.time + 0.5f;
			base.enabled = true;
		}
	}

	private void OnExitMindProjectorDisableVolume(GameObject hitObj)
	{
		bool flag = Locator.GetDreamWorldController().IsInDream() && !Locator.GetDreamWorldController().IsExitingDream();
		if (hitObj.CompareTag("PlayerDetector") && (flag || Locator.GetShipLogManager().IsFactRevealed("IP_ZONE_2_CODE_R1")))
		{
			_mindProjector.SetProjectorActive(active: false);
			_waitToProject = false;
		}
	}

	private void OnEnterTunnelSwapVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInTunnelSwapVolume = true;
			UpdateTunnelState();
			base.enabled = true;
		}
	}

	private void OnExitTunnelSwapVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInTunnelSwapVolume = false;
			UpdateTunnelState();
		}
	}

	private void OnEnterTunnel(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInSecretTunnel = true;
			UpdateTunnelState();
		}
	}

	private void OnExitTunnel(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInSecretTunnel = false;
			UpdateTunnelState();
		}
	}
}
