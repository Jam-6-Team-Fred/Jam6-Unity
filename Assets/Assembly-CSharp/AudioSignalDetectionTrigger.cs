using UnityEngine;

[AddComponentMenu("Audio/Audio Signal Detection Trigger", 400)]
[RequireComponent(typeof(OWTriggerVolume))]
public class AudioSignalDetectionTrigger : MonoBehaviour
{
	[SerializeField]
	private AudioSignal _signal;

	[SerializeField]
	private bool _allowUnderwater;

	[SerializeField]
	private bool _inDarkZone;

	private OWTriggerVolume _trigger;

	private NotificationData _nearbySignalMessage;

	private bool _isPlayerInside;

	private bool _isDetecting;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		_nearbySignalMessage = new NotificationData(NotificationTarget.All, UITextLibrary.GetString(UITextType.UnidentifiedSignal), 0f);
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void Update()
	{
		bool flag = PlayerData.KnowsSignal(_signal.GetName());
		bool flag2 = _signal.IsActive() && !flag && _isPlayerInside && !PlayerState.IsInsideShip() && Locator.GetPlayerSuit().IsWearingHelmet() && (_allowUnderwater || !PlayerState.IsCameraUnderwater()) && _inDarkZone == PlayerState.InDarkZone();
		if (!_isDetecting && flag2)
		{
			_isDetecting = true;
			Locator.GetToolModeSwapper().GetSignalScope().OnEnterSignalDetectionTrigger(_signal);
			Locator.GetToolModeSwapper().GetSignalScope().SelectFrequency(_signal.GetFrequency());
			NotificationManager.SharedInstance.PostNotification(_nearbySignalMessage, pin: true);
		}
		else if (_isDetecting && !flag2)
		{
			_isDetecting = false;
			Locator.GetToolModeSwapper().GetSignalScope().OnExitSignalDetectionTrigger(_signal);
			NotificationManager.SharedInstance.UnpinNotification(_nearbySignalMessage);
		}
		if (!_isPlayerInside)
		{
			base.enabled = false;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = true;
			_isPlayerInside = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_isPlayerInside = false;
		}
	}
}
