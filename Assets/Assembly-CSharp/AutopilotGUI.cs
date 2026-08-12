using UnityEngine;

public class AutopilotGUI : MonoBehaviour
{
	[SerializeField]
	private bool _isShipAutopilot;

	private Autopilot _autopilot;

	private bool _wasAligning;

	private bool _wasApproaching;

	private NotificationData _matchVelocityNotification;

	private NotificationData _matchVelocityCompleteNotification;

	private bool _displayingMatchVelocity;

	private NotificationData _stageOneNotification;

	private NotificationData _stageTwoNotification;

	private NotificationData _stageThreeNotification;

	private ShipAudioController _shipAudio;

	private void Awake()
	{
		_autopilot = base.gameObject.GetAttachedOWRigidbody().GetRequiredComponent<Autopilot>();
		_stageOneNotification = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationAutopilotAlign), 0f);
		_stageTwoNotification = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationAutopilotAccelerate), 0f);
		_stageThreeNotification = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationAutopilotRetro), 0f);
		if (_autopilot != null)
		{
			_autopilot.OnInitMatchVelocity += OnInitMatchVelocity;
			_autopilot.OnMatchedVelocity += OnMatchedVelocity;
		}
		if (_isShipAutopilot)
		{
			_autopilot.OnInitFlyToDestination += OnInitFlyToDestination;
			_autopilot.OnFireRetroRockets += OnFireRetroRockets;
			_autopilot.OnAbortAutopilot += OnAbortAutopilot;
			_autopilot.OnAlreadyAtDestination += OnAlreadyAtDestination;
			_autopilot.OnArriveAtDestination += OnArriveAtDestination;
			GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
			GlobalMessenger.AddListener("ExitFlightConsole", OnExitFlightConsole);
			_matchVelocityNotification = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationVelocityMatching), 0f);
			_matchVelocityCompleteNotification = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationVelocityMatched), 3f);
		}
		else
		{
			GlobalMessenger.AddListener("SuitUp", OnSuitUp);
			GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
			_matchVelocityNotification = new NotificationData(NotificationTarget.Player, UITextLibrary.GetString(UITextType.NotificationVelocityMatching), 0f);
			_matchVelocityCompleteNotification = new NotificationData(NotificationTarget.Player, UITextLibrary.GetString(UITextType.NotificationVelocityMatched), 3f);
		}
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
	}

	private void Start()
	{
		base.enabled = false;
		_wasAligning = false;
		_wasApproaching = false;
		if (Locator.GetShipBody() != null)
		{
			_shipAudio = Locator.GetShipBody().GetComponentInChildren<ShipAudioController>();
		}
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_autopilot != null)
		{
			_autopilot.OnInitMatchVelocity -= OnInitMatchVelocity;
			_autopilot.OnMatchedVelocity -= OnMatchedVelocity;
		}
		if (_isShipAutopilot)
		{
			_autopilot.OnInitFlyToDestination -= OnInitFlyToDestination;
			_autopilot.OnFireRetroRockets -= OnFireRetroRockets;
			_autopilot.OnAbortAutopilot -= OnAbortAutopilot;
			_autopilot.OnAlreadyAtDestination -= OnAlreadyAtDestination;
			_autopilot.OnArriveAtDestination -= OnArriveAtDestination;
			GlobalMessenger<OWRigidbody>.RemoveListener("EnterFlightConsole", OnEnterFlightConsole);
			GlobalMessenger.RemoveListener("ExitFlightConsole", OnExitFlightConsole);
		}
		else
		{
			GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
			GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
		}
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
	}

	private void UnpinAutopilotNotifications()
	{
		if (NotificationManager.SharedInstance.IsPinnedNotification(_stageOneNotification))
		{
			NotificationManager.SharedInstance.UnpinNotification(_stageOneNotification);
		}
		if (NotificationManager.SharedInstance.IsPinnedNotification(_stageTwoNotification))
		{
			NotificationManager.SharedInstance.UnpinNotification(_stageTwoNotification);
		}
		if (NotificationManager.SharedInstance.IsPinnedNotification(_stageThreeNotification))
		{
			NotificationManager.SharedInstance.UnpinNotification(_stageThreeNotification);
		}
	}

	private void OnEnterFlightConsole(OWRigidbody shipBody)
	{
		base.enabled = true;
	}

	private void OnExitFlightConsole()
	{
		base.enabled = false;
	}

	private void OnSuitUp()
	{
		base.enabled = true;
	}

	private void OnRemoveSuit()
	{
		base.enabled = false;
	}

	private void OnInitMatchVelocity()
	{
		NotificationManager.SharedInstance.IsPinnedNotification(_matchVelocityNotification);
		_displayingMatchVelocity = true;
	}

	private void OnInitFlyToDestination()
	{
		_shipAudio.PlayAutopilotOn();
	}

	private void OnMatchedVelocity()
	{
		_displayingMatchVelocity = false;
		if (NotificationManager.SharedInstance.IsPinnedNotification(_matchVelocityNotification))
		{
			NotificationManager.SharedInstance.UnpinNotification(_matchVelocityNotification);
		}
		NotificationManager.SharedInstance.PostNotification(_matchVelocityCompleteNotification);
	}

	private void OnFireRetroRockets()
	{
		UnpinAutopilotNotifications();
		NotificationManager.SharedInstance.PostNotification(_stageThreeNotification, pin: true);
	}

	private void OnAbortAutopilot()
	{
		bool flag = true;
		if (Locator.GetCloakFieldController() != null && Locator.GetCloakFieldController().isPlayerInsideCloak)
		{
			flag = false;
		}
		if (flag)
		{
			_shipAudio.PlayAutopilotOff();
		}
		UnpinAutopilotNotifications();
		if (_autopilot.IsFlyingToDestination())
		{
			NotificationData data = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationAutopilotAbort), 3f);
			NotificationManager.SharedInstance.PostNotification(data);
		}
		else
		{
			NotificationData data2 = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationVelocityAbort), 3f);
			NotificationManager.SharedInstance.PostNotification(data2);
		}
	}

	private void OnAlreadyAtDestination()
	{
		_shipAudio.PlayAutopilotOff();
		NotificationData data = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationAutopilotTooClose), 3f);
		NotificationManager.SharedInstance.PostNotification(data);
	}

	private void OnArriveAtDestination(float arrivalError)
	{
		UnpinAutopilotNotifications();
		if (arrivalError > 50f)
		{
			NotificationData data = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationAutopilotUndershot), 3f);
			NotificationManager.SharedInstance.PostNotification(data);
		}
		else if (arrivalError < -50f)
		{
			NotificationData data2 = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationAutopilotComplete), 3f);
			NotificationManager.SharedInstance.PostNotification(data2);
		}
		else
		{
			NotificationData data3 = new NotificationData(NotificationTarget.Ship, UITextLibrary.GetString(UITextType.NotificationAutopilotComplete), 3f);
			NotificationManager.SharedInstance.PostNotification(data3);
		}
		if (arrivalError > -50f && arrivalError < 50f)
		{
			_shipAudio.PlayAutopilotOn();
		}
		else
		{
			_shipAudio.PlayAutopilotOff();
		}
	}

	private void Update()
	{
		bool flag = _autopilot.IsLiningUpDestination();
		bool flag2 = _autopilot.IsApproachingDestination();
		if (_displayingMatchVelocity && !_autopilot.IsMatchingVelocity())
		{
			_displayingMatchVelocity = false;
			if (NotificationManager.SharedInstance.IsPinnedNotification(_matchVelocityNotification))
			{
				NotificationManager.SharedInstance.UnpinNotification(_matchVelocityNotification);
			}
		}
		if (flag && !_wasAligning)
		{
			UnpinAutopilotNotifications();
			NotificationManager.SharedInstance.PostNotification(_stageOneNotification, pin: true);
		}
		if (flag2 && !_wasApproaching)
		{
			UnpinAutopilotNotifications();
			NotificationManager.SharedInstance.PostNotification(_stageTwoNotification, pin: true);
		}
		_wasAligning = flag;
		_wasApproaching = flag2;
	}
}
