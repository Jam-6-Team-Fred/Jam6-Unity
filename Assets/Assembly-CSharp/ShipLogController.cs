using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShipLogController : MonoBehaviour, ILateInitializer
{
	[SerializeField]
	private Canvas _shipLogCanvas;

	[SerializeField]
	private PlayerAttachPoint _attachPoint;

	[SerializeField]
	private Transform _lockOnPoint;

	[SerializeField]
	private SingleInteractionVolume _interactVolume;

	[SerializeField]
	private ShipLogMode _detectiveMode;

	[SerializeField]
	private ShipLogMode _mapMode;

	[SerializeField]
	private ShipLogSplashScreen _splashScreen;

	[FormerlySerializedAs("_upperRightPromptList")]
	[SerializeField]
	private ScreenPromptList _upperRightPromptListMap;

	[SerializeField]
	private ScreenPromptList _upperRightPromptListDetective;

	[SerializeField]
	private ScreenPromptList _centerPromptList;

	[SerializeField]
	private OWAudioSource _ambienceSource;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	private RectTransform _upperRightPromptListRect;

	private CanvasGroupAnimator _canvasAnimator;

	private ShipLogManager _manager;

	private ShipLogMode _currentMode;

	private ScreenPrompt _exitPrompt;

	private bool _initialized;

	private bool _exiting;

	private bool _damaged;

	private bool _timeFrozen;

	private bool _usingShipLog;

	private void Awake()
	{
		LateInitializerManager.RegisterLateInitializer(this);
		_canvasAnimator = _shipLogCanvas.GetComponent<CanvasGroupAnimator>();
		_interactVolume.OnPressInteract += OnPressInteract;
		_exitPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.LogExitPrompt));
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
	}

	private void Start()
	{
		_canvasAnimator.SetImmediate(0f, Vector3.one * 0.001f);
		_manager = Locator.GetShipLogManager();
		_currentMode = _mapMode;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (!_initialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
		_interactVolume.OnPressInteract -= OnPressInteract;
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
		if (_usingShipLog)
		{
			_manager.ClearNewlyRevealedFacts();
		}
		if (_timeFrozen)
		{
			_timeFrozen = false;
			OWTime.Unpause(OWTime.PauseType.Reading);
		}
	}

	public void LateInitialize()
	{
		_initialized = true;
		_upperRightPromptListMap.Init();
		_upperRightPromptListMap.SetReversePromptOrder(reverse: true);
		_upperRightPromptListDetective.Init();
		_upperRightPromptListDetective.SetReversePromptOrder(reverse: true);
		_centerPromptList.Init();
		_detectiveMode.Initialize(_centerPromptList, _upperRightPromptListDetective, _oneShotSource);
		_mapMode.Initialize(_centerPromptList, _upperRightPromptListMap, _oneShotSource);
		if (_detectiveMode.IsEditMode())
		{
			_mapMode.gameObject.SetActive(value: false);
			_splashScreen.gameObject.SetActive(value: false);
			GetComponentInChildren<Mask>().enabled = false;
		}
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		if (_usingShipLog)
		{
			_manager.ClearNewlyRevealedFacts();
		}
		_shipLogCanvas.gameObject.SetActive(value: false);
	}

	public void SetDamaged(bool damaged)
	{
		_damaged = damaged;
		_splashScreen.SetDamaged(damaged);
		if (_damaged)
		{
			if (_usingShipLog)
			{
				ExitShipComputer();
			}
			_interactVolume.DisableInteraction();
		}
		else
		{
			_interactVolume.EnableInteraction();
		}
	}

	private void OnPressInteract()
	{
		if (_initialized)
		{
			EnterShipComputer();
		}
	}

	private void EnterShipComputer()
	{
		if (!_timeFrozen && PlayerData.GetFreezeTimeWhileReadingShipLog() && !Locator.GetGlobalMusicController().IsEndTimesPlaying())
		{
			_timeFrozen = true;
			OWTime.Pause(OWTime.PauseType.Reading);
		}
		base.enabled = true;
		_usingShipLog = true;
		_exiting = false;
		_shipLogCanvas.gameObject.SetActive(value: true);
		_canvasAnimator.AnimateTo(1f, Vector3.one * 0.001f, 0.5f);
		Locator.GetToolModeSwapper().UnequipTool();
		Locator.GetFlashlight().TurnOff(playAudio: false);
		_attachPoint.AttachPlayer();
		Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().SnapToDegrees(0f, -16.9f, 100f, smoothStep: true);
		Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().SnapToFieldOfView(38.33f, 0.7f, smoothStep: true);
		if (Locator.GetPlayerSuit().IsWearingSuit() && Locator.GetPlayerDetector().GetComponent<OxygenDetector>().GetDetectOxygen())
		{
			Locator.GetPlayerSuit().RemoveHelmet();
		}
		Locator.GetPromptManager().AddScreenPrompt(_exitPrompt, _upperRightPromptListMap, TextAnchor.MiddleRight);
		Locator.GetPromptManager().AddScreenPrompt(_exitPrompt, _upperRightPromptListDetective, TextAnchor.MiddleRight);
		List<ShipLogFact> list = BuildRevealQueue();
		if (list.Count > 0)
		{
			_currentMode = _detectiveMode;
		}
		if (!PlayerData.GetDetectiveModeEnabled() || !PlayerData.GetPersistentCondition("HAS_USED_SHIPLOG"))
		{
			_currentMode = _mapMode;
		}
		_oneShotSource.PlayOneShot(AudioType.ShipLogBootUp);
		_ambienceSource.FadeIn(Locator.GetAudioManager().GetSingleAudioClip(AudioType.ShipLogBootUp).length, fadeFromNothing: true);
		GlobalMessenger.FireEvent("EnterShipComputer");
		_splashScreen.OnEnterComputer();
		if (PlayerData.GetDetectiveModeEnabled())
		{
			_detectiveMode.OnEnterComputer();
		}
		_mapMode.OnEnterComputer();
		_currentMode.EnterMode("", list);
	}

	private void ExitShipComputer()
	{
		if (_timeFrozen)
		{
			_timeFrozen = false;
			OWTime.Unpause(OWTime.PauseType.Reading);
		}
		_exiting = true;
		_usingShipLog = false;
		_currentMode.ExitMode();
		Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().SnapToInitFieldOfView(0.5f, smoothStep: true);
		if (PlayerState.InZeroG())
		{
			Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().CenterCamera(50f, smoothStep: true);
		}
		_attachPoint.DetachPlayer();
		_interactVolume.ResetInteraction();
		if (Locator.GetPlayerSuit().IsWearingSuit() && !Locator.GetPlayerSuit().IsWearingHelmet())
		{
			Locator.GetPlayerSuit().PutOnHelmet();
		}
		Locator.GetPromptManager().RemoveScreenPrompt(_exitPrompt);
		_manager.ClearNewlyRevealedFacts();
		PlayerData.SaveCurrentGame();
		_splashScreen.OnExitComputer();
		_detectiveMode.OnExitComputer();
		_mapMode.OnExitComputer();
		_canvasAnimator.AnimateTo(0f, Vector3.one * 0.001f, 0.5f);
		if (_ambienceSource.isPlaying)
		{
			_ambienceSource.FadeOut(0.5f);
		}
		else
		{
			_ambienceSource.Stop();
		}
		_oneShotSource.Stop();
		if (!PlayerData.GetPersistentCondition("HAS_USED_SHIPLOG"))
		{
			PlayerData.SetPersistentCondition("HAS_USED_SHIPLOG", state: true);
		}
		GlobalMessenger.FireEvent("ExitShipComputer");
	}

	private void Update()
	{
		if (_exiting)
		{
			if (_canvasAnimator.IsComplete())
			{
				base.enabled = false;
				_shipLogCanvas.gameObject.SetActive(value: false);
			}
		}
		else
		{
			if (OWInput.GetInputMode() != InputMode.ShipComputer)
			{
				return;
			}
			_exitPrompt.SetVisibility(_currentMode.AllowCancelInput());
			if (_currentMode.AllowCancelInput() && OWInput.IsNewlyPressed(InputLibrary.cancel))
			{
				ExitShipComputer();
				return;
			}
			_currentMode.UpdateMode();
			if (_currentMode.AllowModeSwap() && OWInput.IsNewlyPressed(InputLibrary.swapShipLogMode))
			{
				ShipLogMode currentMode = _currentMode;
				string focusedEntryID = currentMode.GetFocusedEntryID();
				bool flag = currentMode.Equals(_mapMode);
				_currentMode = (flag ? _detectiveMode : _mapMode);
				currentMode.ExitMode();
				_currentMode.EnterMode(focusedEntryID);
				_oneShotSource.PlayOneShot(flag ? AudioType.ShipLogEnterDetectiveMode : AudioType.ShipLogEnterMapMode);
			}
		}
	}

	private List<ShipLogFact> BuildRevealQueue()
	{
		List<string> list = new List<string>(PlayerData.GetNewlyRevealedFactIDs());
		List<ShipLogFact> list2 = new List<ShipLogFact>();
		int num;
		for (num = 0; num < list.Count; num++)
		{
			ShipLogFact fact = _manager.GetFact(list[num]);
			if (fact == null)
			{
				Debug.LogWarning("Found entry in newlyRevealedFactIDs that does not match any fact in dictionary: " + list[num]);
				list.RemoveAt(num);
				num--;
			}
			else if (fact.HasSource())
			{
				for (int i = num + 1; i < list.Count; i++)
				{
					ShipLogFact fact2 = _manager.GetFact(list[i]);
					if (fact2.GetEntryID().Equals(fact.GetSourceID()) && !fact2.IsRumor())
					{
						list2.Add(fact2);
						list.RemoveAt(i);
						i--;
					}
				}
				list2.Add(fact);
				list.RemoveAt(num);
				num--;
			}
			else
			{
				list2.Add(fact);
				list.RemoveAt(num);
				num--;
				for (int j = num + 1; j < list.Count; j++)
				{
					ShipLogFact fact3 = _manager.GetFact(list[j]);
					if (fact3.GetEntryID().Equals(fact.GetEntryID()) && !fact3.HasSource())
					{
						list2.Add(fact3);
						list.RemoveAt(j);
						j--;
					}
				}
			}
		}
		return list2;
	}
}
