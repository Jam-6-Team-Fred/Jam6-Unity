using UnityEngine;

public class QuantumCampsiteController : MonoBehaviour
{
	private const int BANJO_INDEX = 0;

	private const int SOLANUM_INDEX = 4;

	private const int PRISONER_INDEX = 5;

	private const int ESKER_INDEX = 6;

	[SerializeField]
	private OWTriggerVolume _trigger;

	[Space]
	[SerializeField]
	private Campfire _campfire;

	[SerializeField]
	private GameObject _treeVolume;

	[SerializeField]
	private MultiStateQuantumObject _quantumCampfire;

	[SerializeField]
	private GameObject _campsiteRoot;

	[SerializeField]
	private EndlessTriggerVolume _endlessCampsiteVolume;

	[Space]
	[SerializeField]
	private MultiStateQuantumObject _quantumEsker;

	[SerializeField]
	private CharacterDialogueTree _eskerDialogue;

	[SerializeField]
	private CharacterDialogueTree _riebeckDialogue;

	[SerializeField]
	private GameObject _deepForestRoot;

	[SerializeField]
	private TravelerEyeController[] _travelerControllers;

	[SerializeField]
	private GameObject[] _instrumentZones;

	[Header("Alt Traveler Sockets")]
	[SerializeField]
	private Transform[] _travelerRoots;

	[SerializeField]
	private Transform[] _altTravelerSockets;

	private bool _areInstrumentsActive;

	private bool _hasJamSessionStarted;

	private bool _hasMetSolanum;

	private bool _hasMetPrisoner;

	private bool _hasErasedPrisoner;

	private void Awake()
	{
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		_campfire.OnCampfireStateChange += OnCampfireStateChange;
		_eskerDialogue.OnStartConversation += OnStartEskerConversation;
		_riebeckDialogue.OnEndConversation += OnEndRiebeckConversation;
		for (int i = 0; i < _travelerControllers.Length; i++)
		{
			_travelerControllers[i].OnStartPlaying += OnTravelerStartPlaying;
		}
	}

	private void Start()
	{
		EyeState state = Locator.GetEyeStateManager().GetState();
		_hasMetSolanum = PlayerData.GetPersistentCondition("MET_SOLANUM");
		_hasMetPrisoner = PlayerData.GetPersistentCondition("MET_PRISONER") && EntitlementsManager.IsDlcOwned() == EntitlementsManager.AsyncOwnershipStatus.Owned;
		if (_hasMetSolanum && _hasMetPrisoner)
		{
			for (int i = 0; i < _travelerRoots.Length; i++)
			{
				_travelerRoots[i].SetPositionAndRotation(_altTravelerSockets[i].position, _altTravelerSockets[i].rotation);
			}
		}
		for (int j = 0; j < _instrumentZones.Length; j++)
		{
			_instrumentZones[j].SetActive(value: false);
		}
		_treeVolume.SetActive(value: false);
		if (state <= EyeState.InstrumentHunt)
		{
			_campsiteRoot.SetActive(value: false);
			_deepForestRoot.SetActive(value: false);
			for (int k = 0; k < 6; k++)
			{
				_travelerControllers[k].gameObject.SetActive(value: false);
			}
			return;
		}
		switch (state)
		{
		case EyeState.JamSession:
			_campsiteRoot.SetActive(value: true);
			_deepForestRoot.SetActive(value: true);
			_quantumEsker.DebugForceToFinalQuantumState();
			_quantumCampfire.DebugForceToFinalQuantumState();
			_campfire.SetInitialState(Campfire.State.LIT);
			_treeVolume.SetActive(value: true);
			_endlessCampsiteVolume.SetActivation(active: false);
			_travelerControllers[4].gameObject.SetActive(_hasMetSolanum);
			_travelerControllers[5].gameObject.SetActive(_hasMetPrisoner);
			break;
		case EyeState.BigBang:
			_campsiteRoot.SetActive(value: true);
			_deepForestRoot.SetActive(value: true);
			_quantumEsker.DebugForceToFinalQuantumState();
			_quantumCampfire.DebugForceToFinalQuantumState();
			_campfire.SetInitialState(Campfire.State.UNLIT);
			_treeVolume.SetActive(value: true);
			_endlessCampsiteVolume.SetActivation(active: false);
			_travelerControllers[4].gameObject.SetActive(_hasMetSolanum);
			_travelerControllers[5].gameObject.SetActive(_hasMetPrisoner);
			break;
		}
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
		_campfire.OnCampfireStateChange -= OnCampfireStateChange;
		_eskerDialogue.OnStartConversation -= OnStartEskerConversation;
		_riebeckDialogue.OnEndConversation -= OnEndRiebeckConversation;
		for (int i = 0; i < _travelerControllers.Length; i++)
		{
			_travelerControllers[i].OnStartPlaying -= OnTravelerStartPlaying;
		}
	}

	public void OnPrisonerErased()
	{
		_instrumentZones[5].SetActive(value: false);
		_instrumentZones[0].SetActive(value: true);
		_hasErasedPrisoner = true;
		CheckTravelersGathered();
	}

	public void OnPrisonerJoined()
	{
		_hasErasedPrisoner = false;
		CheckTravelersGathered();
	}

	public AudioClip GetTravelerMusicEndClip()
	{
		bool flag = _hasMetPrisoner && !_hasErasedPrisoner;
		AudioType audioType = AudioType.TravelerEnd_NoPiano;
		if (_hasMetSolanum && flag)
		{
			audioType = AudioType.TravelerEnd_All_Prisoner;
		}
		else if (_hasMetSolanum)
		{
			audioType = AudioType.TravelerEnd_All;
		}
		else if (flag)
		{
			audioType = AudioType.TravelerEnd_NoPiano_Prisoner;
		}
		MonoBehaviour.print("choosing end clip: " + audioType);
		return Locator.GetAudioManager().GetSingleAudioClip(audioType);
	}

	public bool GetUseAltPostCollapseSocket()
	{
		if (_hasMetSolanum)
		{
			return _hasMetPrisoner;
		}
		return false;
	}

	private void OnStartEskerConversation()
	{
		_eskerDialogue.OnStartConversation -= OnStartEskerConversation;
		if (Locator.GetProbe() != null && Locator.GetProbe().IsLaunched())
		{
			Locator.GetProbe().FlickerOffAndOn(0f, 2f);
		}
		_deepForestRoot.SetActive(value: true);
		_instrumentZones[0].SetActive(value: true);
	}

	private void OnEndRiebeckConversation()
	{
		_riebeckDialogue.OnEndConversation -= OnEndRiebeckConversation;
		ActivateRemainingInstrumentZones();
	}

	private void ActivateRemainingInstrumentZones()
	{
		if (!_areInstrumentsActive)
		{
			_areInstrumentsActive = true;
			for (int i = 1; i < 4; i++)
			{
				_instrumentZones[i].SetActive(value: true);
			}
			_instrumentZones[4].SetActive(_hasMetSolanum);
			if (_hasMetPrisoner && _instrumentZones.Length > 5)
			{
				_instrumentZones[0].SetActive(value: false);
				_instrumentZones[5].SetActive(value: true);
			}
		}
	}

	private void OnTravelerStartPlaying()
	{
		if (!_hasJamSessionStarted)
		{
			_hasJamSessionStarted = true;
			for (int i = 0; i < _travelerControllers.Length; i++)
			{
				_travelerControllers[i].OnStartCosmicJamSession();
			}
		}
	}

	private void OnCampfireStateChange(Campfire fire)
	{
		if (fire.GetState() == Campfire.State.LIT)
		{
			_campsiteRoot.SetActive(value: true);
			_treeVolume.SetActive(value: true);
			_endlessCampsiteVolume.SetActivation(active: false);
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			CheckTravelersGathered();
		}
	}

	private void OnExit(GameObject hitObj)
	{
		hitObj.CompareTag("PlayerDetector");
	}

	private void CheckTravelersGathered()
	{
		if (!DialogueConditionManager.SharedInstance.GetConditionState("AllTravelersGathered") && AreAllTravelersGathered())
		{
			DialogueConditionManager.SharedInstance.SetConditionState("AllTravelersGathered", conditionState: true);
		}
		if (!DialogueConditionManager.SharedInstance.GetConditionState("AnyTravelersGathered") && AreAnyTravelersGathered())
		{
			DialogueConditionManager.SharedInstance.SetConditionState("AnyTravelersGathered", conditionState: true);
		}
	}

	private bool AreAnyTravelersGathered()
	{
		int num = 0;
		for (int i = 0; i < _travelerControllers.Length; i++)
		{
			if (_travelerControllers[i].gameObject.activeInHierarchy)
			{
				num++;
			}
		}
		return num > 1;
	}

	private bool AreAllTravelersGathered()
	{
		for (int i = 0; i < _travelerControllers.Length; i++)
		{
			if (!_travelerControllers[i].gameObject.activeInHierarchy && (i != 4 || _hasMetSolanum) && (i != 5 || (_hasMetPrisoner && !_hasErasedPrisoner)))
			{
				return false;
			}
		}
		return true;
	}
}
