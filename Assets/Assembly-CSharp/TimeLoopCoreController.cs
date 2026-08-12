using System;
using UnityEngine;

public class TimeLoopCoreController : MonoBehaviour
{
	private const float _blackHoleOpenDelay = 3f;

	private const float _blackHoleCloseDelay = 28f;

	private const float _chamberDestructionDelay = 33f;

	private const float _endGameDelay = 90f;

	private const float _chamberDestructionDuration = 0.5f;

	[SerializeField]
	private AudioVolume _musicVolume;

	[SerializeField]
	private OWAudioSource _coreCasingAudio;

	[SerializeField]
	private NomaiInterfaceSlot _openSlot;

	[SerializeField]
	private WarpCoreSocket _warpCoreSocket;

	[SerializeField]
	private TransformAnimator[] _panelAnimators;

	[Space(10f)]
	[SerializeField]
	private SingularityController _blackHoleEffect;

	[SerializeField]
	private EffectVolume _attractVolume;

	[SerializeField]
	private TimeLoopBlackHoleVolume _timeLoopBlackHoleVolume;

	[SerializeField]
	private TimeLoopLightController _timeLoopLightController;

	[Space(10f)]
	[SerializeField]
	private GameObject[] _dataStreamObjects;

	[SerializeField]
	private OWAudioSource _dataStreamAudio;

	[Space(10f)]
	[SerializeField]
	private Renderer _chamberDestructionEffect;

	[Space(10f)]
	[SerializeField]
	private TravelerController _duplicatePlayer;

	[SerializeField]
	private SurveyorProbe _duplicateProbe;

	[SerializeField]
	private Transform _duplicateProbeSocket;

	private SurveyorProbe _playerProbe;

	private NomaiEnergyCable[] _energyCables;

	private bool _coreOpen;

	private bool _preventCoreClose;

	private bool _poweringUp;

	private bool _attemptedToOpenBlackHole;

	private bool _blackHoleStable;

	private bool _dataTransmitting;

	private bool _dataTransmissionComplete;

	private bool _playerEnteredCoreLastLoop;

	private bool _probeEnteredCoreLastLoop;

	private bool _playerEnteredCoreCurrentLoop;

	private bool _probeEnteredCoreCurrentLoop;

	private bool _timelineBeingObliterated;

	private float _sunExplodeTime;

	private static bool s_paradoxExists;

	public static bool ParadoxExists()
	{
		return s_paradoxExists;
	}

	private void Awake()
	{
		_energyCables = GetComponents<NomaiEnergyCable>();
		_openSlot.OnSlotActivated += OnSlotActivated;
		_openSlot.OnSlotDeactivated += OnSlotDeactivated;
		_blackHoleEffect.OnCreation += OnBlackHoleStable;
		_timeLoopBlackHoleVolume.OnPlayerEnteredCore += OnPlayerEnteredCore;
		_timeLoopBlackHoleVolume.OnProbeEnteredCore += OnProbeEnteredCore;
		GlobalMessenger.AddListener("SunExploded", OnSunExploded);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		for (int i = 0; i < _dataStreamObjects.Length; i++)
		{
			_dataStreamObjects[i].SetActive(value: false);
		}
		if (_warpCoreSocket != null)
		{
			WarpCoreSocket warpCoreSocket = _warpCoreSocket;
			warpCoreSocket.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Combine(warpCoreSocket.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnSocketablePlaced));
			WarpCoreSocket warpCoreSocket2 = _warpCoreSocket;
			warpCoreSocket2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(warpCoreSocket2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSocketableRemoved));
		}
	}

	private void Start()
	{
		_attractVolume.SetVolumeActivation(active: false);
		_timeLoopBlackHoleVolume.SetActive(value: false);
		InitializeDuplicates();
		Locator.GetTimelineObliterationController().OnTimelineStartObliteration += OnTimelineStartObliteration;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_openSlot.OnSlotActivated -= OnSlotActivated;
		_openSlot.OnSlotDeactivated -= OnSlotDeactivated;
		_blackHoleEffect.OnCreation -= OnBlackHoleStable;
		_timeLoopBlackHoleVolume.OnPlayerEnteredCore -= OnPlayerEnteredCore;
		_timeLoopBlackHoleVolume.OnProbeEnteredCore -= OnProbeEnteredCore;
		if (Locator.GetTimelineObliterationController() != null)
		{
			Locator.GetTimelineObliterationController().OnTimelineStartObliteration -= OnTimelineStartObliteration;
		}
		if (_warpCoreSocket != null)
		{
			WarpCoreSocket warpCoreSocket = _warpCoreSocket;
			warpCoreSocket.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Remove(warpCoreSocket.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnSocketablePlaced));
			WarpCoreSocket warpCoreSocket2 = _warpCoreSocket;
			warpCoreSocket2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(warpCoreSocket2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSocketableRemoved));
		}
		if (_playerProbe != null)
		{
			_playerProbe.OnLaunchProbe -= OnPlayerProbeLaunch;
			_playerProbe.OnRetrieveProbe -= OnPlayerProbeRetrieve;
			_playerProbe.OnProbeDestroyed -= OnPlayerProbeDestroyed;
		}
		GlobalMessenger.RemoveListener("SunExploded", OnSunExploded);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
	}

	private void SetParadoxState()
	{
		_playerEnteredCoreLastLoop = PlayerData.GetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE");
		_probeEnteredCoreLastLoop = PlayerData.GetPersistentCondition("PROBE_ENTERED_TIMELOOPCORE");
		s_paradoxExists = _playerEnteredCoreLastLoop || _probeEnteredCoreLastLoop;
	}

	private void InitializeDuplicates()
	{
		SetParadoxState();
		_duplicatePlayer.gameObject.SetActive(_playerEnteredCoreLastLoop);
		_duplicateProbe.gameObject.SetActive(_probeEnteredCoreLastLoop);
		TimelineObliterationController.SetParadoxCoreProbeActive(_probeEnteredCoreLastLoop);
		if (_probeEnteredCoreLastLoop)
		{
			_duplicateProbe.SetAsTimeLoopCoreDuplicate(_duplicateProbeSocket);
			_playerProbe = Locator.GetProbe();
			_playerProbe.OnLaunchProbe += OnPlayerProbeLaunch;
			_playerProbe.OnRetrieveProbe += OnPlayerProbeRetrieve;
			_playerProbe.OnProbeDestroyed += OnPlayerProbeDestroyed;
		}
	}

	private void OnPlayerProbeDestroyed()
	{
		_duplicateProbe.GetProbeHUDMarker().MarkTLCoreDuplicatedState(value: false);
		if (_playerProbe != null)
		{
			_playerProbe.OnLaunchProbe -= OnPlayerProbeLaunch;
			_playerProbe.OnRetrieveProbe -= OnPlayerProbeRetrieve;
			_playerProbe.OnProbeDestroyed -= OnPlayerProbeDestroyed;
		}
	}

	private void OnPlayerProbeRetrieve()
	{
		_duplicateProbe.GetProbeHUDMarker().MarkTLCoreDuplicatedState(value: false);
		_playerProbe.GetProbeHUDMarker().MarkTLCoreDuplicatedState(value: false);
	}

	private void OnPlayerProbeLaunch()
	{
		_duplicateProbe.GetProbeHUDMarker().MarkTLCoreDuplicatedState(value: true);
		_playerProbe.GetProbeHUDMarker().MarkTLCoreDuplicatedState(value: true);
	}

	private void OnPlayerEnteredCore()
	{
		if (!_playerEnteredCoreCurrentLoop)
		{
			_playerEnteredCoreCurrentLoop = true;
			PlayerData.SetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE", state: true);
			if (_probeEnteredCoreLastLoop && !_probeEnteredCoreCurrentLoop && !Locator.GetProbe().IsRetrieved())
			{
				s_paradoxExists = true;
			}
			else
			{
				s_paradoxExists = false;
				_probeEnteredCoreLastLoop = false;
				PlayerData.SetPersistentCondition("PROBE_ENTERED_TIMELOOPCORE", state: false);
			}
			if (_playerEnteredCoreLastLoop && _playerEnteredCoreCurrentLoop)
			{
				PlayerData.SetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE_MULTIPLE", state: true);
			}
			if (!_playerEnteredCoreLastLoop && !_probeEnteredCoreLastLoop)
			{
				PlayerData.SetLoopCountOnParadoxStart();
			}
			Locator.GetDeathManager().KillPlayer(DeathType.BlackHole);
		}
	}

	private void OnProbeEnteredCore()
	{
		if (!_probeEnteredCoreCurrentLoop && !_playerEnteredCoreCurrentLoop)
		{
			_probeEnteredCoreCurrentLoop = true;
			PlayerData.SetPersistentCondition("PROBE_ENTERED_TIMELOOPCORE", state: true);
			if (_probeEnteredCoreLastLoop && _probeEnteredCoreCurrentLoop)
			{
				s_paradoxExists = false;
			}
			if (!_playerEnteredCoreLastLoop && !_probeEnteredCoreLastLoop)
			{
				PlayerData.SetLoopCountOnParadoxStart();
			}
		}
	}

	public bool IsOpen()
	{
		return _coreOpen;
	}

	private void OpenCore()
	{
		if (!_coreOpen)
		{
			_coreCasingAudio.PlayOneShot(AudioType.NomaiTimeLoopOpen);
			_coreOpen = true;
			for (int i = 0; i < _panelAnimators.Length; i++)
			{
				float num = ((i < 4) ? (-1f) : 1f);
				_panelAnimators[i].RotateToLocalEulerAngles(new Vector3(_panelAnimators[i].transform.localEulerAngles.x, _panelAnimators[i].transform.localEulerAngles.y, 90f * num), 3f);
			}
		}
	}

	private void CloseCore()
	{
		if (_coreOpen && !_preventCoreClose)
		{
			_coreCasingAudio.PlayOneShot(AudioType.NomaiTimeLoopClose);
			_coreOpen = false;
			for (int i = 0; i < _panelAnimators.Length; i++)
			{
				_panelAnimators[i].RotateToOriginalLocalRotation(3f);
			}
		}
	}

	private void Update()
	{
		if (_poweringUp && Time.time >= _sunExplodeTime + 3f)
		{
			if (_warpCoreSocket.IsSocketOccupied() && _warpCoreSocket.GetWarpCoreType() == WarpCoreType.Vessel)
			{
				_blackHoleEffect.Create();
				_timeLoopBlackHoleVolume.SetActive(value: true);
				_attractVolume.SetVolumeActivation(active: true);
				_warpCoreSocket.EnableInteraction(value: false);
				_warpCoreSocket.GetSocketedItem().EnableInteraction(value: false);
			}
			else
			{
				OnDataTransmissionFailure();
			}
			_poweringUp = false;
			_attemptedToOpenBlackHole = true;
		}
		if (_blackHoleStable && !_dataTransmissionComplete)
		{
			if (!_dataTransmitting)
			{
				_dataTransmitting = true;
				_dataStreamAudio.FadeIn(2f);
				for (int i = 0; i < _dataStreamObjects.Length; i++)
				{
					_dataStreamObjects[i].SetActive(value: true);
				}
			}
			else if (_dataTransmitting && Time.time >= _sunExplodeTime + 28f)
			{
				_dataTransmitting = false;
				_dataTransmissionComplete = true;
				_dataStreamAudio.FadeOut(2f);
				_blackHoleEffect.Collapse();
				_timeLoopBlackHoleVolume.SetActive(value: false);
				_attractVolume.SetVolumeActivation(active: false);
				for (int j = 0; j < _dataStreamObjects.Length; j++)
				{
					_dataStreamObjects[j].SetActive(value: false);
				}
			}
		}
		if (_timelineBeingObliterated)
		{
			return;
		}
		if (_dataTransmissionComplete && !Locator.GetDeathManager().IsPlayerDying() && _blackHoleEffect.GetState() == SingularityController.State.Collapsed)
		{
			OnDataTransmissionComplete();
		}
		if (Time.time >= _sunExplodeTime + 33f)
		{
			float num = Mathf.InverseLerp(_sunExplodeTime + 33f, _sunExplodeTime + 33f + 0.5f, Time.time);
			_chamberDestructionEffect.material.SetFloat("_Cutoff", 1f - num);
			if (num >= 1f)
			{
				GlobalMessenger.FireEvent("TimeLoopInteriorDestroyed");
			}
		}
		if (Time.time >= _sunExplodeTime + 33f + 90f)
		{
			if ((Locator.GetRingWorldController() != null && Locator.GetRingWorldController().isPlayerInside) || (PlayerState.InDreamWorld() && !PlayerState.IsResurrected()))
			{
				Locator.GetDeathManager().BeginEscapedTimeLoopSequence(TimeloopEscapeType.Ringworld);
			}
			else
			{
				Locator.GetDeathManager().BeginEscapedTimeLoopSequence(TimeloopEscapeType.Ship);
			}
			base.enabled = false;
		}
	}

	private void OnSunExploded()
	{
		OpenCore();
		for (int i = 0; i < _energyCables.Length; i++)
		{
			_energyCables[i].SetTargetGlow(1f);
		}
		_preventCoreClose = true;
		_sunExplodeTime = Time.time;
		_poweringUp = true;
		base.enabled = true;
	}

	private void OnBlackHoleStable()
	{
		_blackHoleStable = true;
	}

	private void OnSocketablePlaced(OWItem socketableItem)
	{
		if (!_attemptedToOpenBlackHole && socketableItem.GetType() == typeof(WarpCoreItem) && ((WarpCoreItem)socketableItem).GetWarpCoreType() == WarpCoreType.Vessel)
		{
			TimeLoop.SetTimeLoopEnabled(enabled: true);
			_timeLoopLightController.SetLightsOn(lightsOn: true, 0f, 0f, 2f);
			_musicVolume.Activate();
			_preventCoreClose = false;
		}
	}

	private void OnSocketableRemoved(OWItem socketableItem)
	{
		if (socketableItem.GetType() == typeof(WarpCoreItem) && ((WarpCoreItem)socketableItem).GetWarpCoreType() == WarpCoreType.Vessel)
		{
			TimeLoop.SetTimeLoopEnabled(enabled: false);
			_timeLoopLightController.SetLightsOn(lightsOn: false, 0f, 0f, 2f);
			_musicVolume.Deactivate();
			_preventCoreClose = true;
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		OpenCore();
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		CloseCore();
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		if (s_paradoxExists)
		{
			ResetParadoxConditions();
		}
	}

	private void OnDataTransmissionFailure()
	{
		if (_playerEnteredCoreLastLoop)
		{
			Locator.GetTimelineObliterationController().BeginTimelineObliteration(TimelineObliterationController.ObliterationType.TIME_LOOP_CORE, _duplicatePlayer);
			ResetParadoxConditions();
		}
		else if (_probeEnteredCoreLastLoop)
		{
			Locator.GetTimelineObliterationController().BeginTimelineObliteration(TimelineObliterationController.ObliterationType.TIME_LOOP_CORE, _duplicateProbe);
			ResetParadoxConditions();
		}
	}

	private void OnDataTransmissionComplete()
	{
		bool flag = false;
		if (_playerEnteredCoreLastLoop && !_playerEnteredCoreCurrentLoop)
		{
			flag = true;
			Locator.GetTimelineObliterationController().BeginTimelineObliteration(TimelineObliterationController.ObliterationType.TIME_LOOP_CORE, _duplicatePlayer);
			ResetParadoxConditions();
		}
		else if (_probeEnteredCoreLastLoop && !_probeEnteredCoreCurrentLoop)
		{
			flag = true;
			Locator.GetTimelineObliterationController().BeginTimelineObliteration(TimelineObliterationController.ObliterationType.TIME_LOOP_CORE, _duplicateProbe);
			ResetParadoxConditions();
		}
		if (!flag)
		{
			if (Vector3.Distance(Locator.GetPlayerBody().GetPosition(), Locator.GetSunTransform().position) > 10000000f)
			{
				Achievements.Earn(Achievements.Type.YOU_TRIED);
			}
			Locator.GetDeathManager().KillPlayer(DeathType.TimeLoop);
		}
	}

	private void ResetParadoxConditions()
	{
		PlayerData.SetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE", state: false);
		PlayerData.SetPersistentCondition("PROBE_ENTERED_TIMELOOPCORE", state: false);
		if (PlayerData.GetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE_MULTIPLE"))
		{
			PlayerData.SetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE_MULTIPLE", state: false);
		}
	}

	private void OnTimelineStartObliteration()
	{
		_timelineBeingObliterated = true;
	}
}
