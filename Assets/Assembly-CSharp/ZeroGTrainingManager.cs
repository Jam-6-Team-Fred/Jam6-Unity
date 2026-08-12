using UnityEngine;

public class ZeroGTrainingManager : MonoBehaviour
{
	[SerializeField]
	private GameObject _satelliteObj;

	[SerializeField]
	private OWTriggerVolume _trigger;

	[SerializeField]
	private OWAudioSource _audioSource;

	private NotificationData _helmetProgressNotification;

	private SatelliteNode[] _satelliteNodes;

	private Light[] _satelliteLights;

	private int _repairCount;

	private void Awake()
	{
		_satelliteNodes = _satelliteObj.GetComponentsInChildren<SatelliteNode>();
		for (int i = 0; i < _satelliteNodes.Length; i++)
		{
			_satelliteNodes[i].OnRepaired += OnRepairNode;
		}
		_satelliteLights = _satelliteObj.GetComponentsInChildren<Light>();
		if (_trigger != null)
		{
			_trigger.OnEntry += OnEntry;
			_trigger.OnExit += OnExit;
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _satelliteNodes.Length; i++)
		{
			_satelliteNodes[i].OnRepaired -= OnRepairNode;
		}
		if (_trigger != null)
		{
			_trigger.OnEntry -= OnEntry;
			_trigger.OnExit -= OnExit;
		}
	}

	private void OnRepairNode(SatelliteNode node)
	{
		_repairCount++;
		UpdateNotification();
		if (_repairCount >= _satelliteNodes.Length)
		{
			DialogueConditionManager.SharedInstance.SetConditionState("PostZeroG", conditionState: true);
			Locator.GetShipLogManager().RevealFact("TH_ZERO_G_CAVE_X2");
			for (int i = 0; i < _satelliteLights.Length; i++)
			{
				_satelliteLights[i].enabled = true;
			}
			_audioSource.PlayOneShot(AudioType.TH_ZeroGTrainingAllRepaired);
		}
	}

	private void UpdateNotification()
	{
		if (_helmetProgressNotification != null)
		{
			NotificationManager.SharedInstance.UnpinNotification(_helmetProgressNotification);
		}
		if (_repairCount < _satelliteNodes.Length)
		{
			_helmetProgressNotification = new NotificationData(NotificationTarget.Player, _repairCount + UITextLibrary.GetString(UITextType.Notification0gTraining));
			NotificationManager.SharedInstance.PostNotification(_helmetProgressNotification, pin: true);
		}
		else
		{
			_helmetProgressNotification = new NotificationData(NotificationTarget.Player, UITextLibrary.GetString(UITextType.Notification0gEnd), 10f);
			NotificationManager.SharedInstance.PostNotification(_helmetProgressNotification, pin: true);
		}
	}

	private void RemoveNotification()
	{
		if (_helmetProgressNotification != null)
		{
			NotificationManager.SharedInstance.UnpinNotification(_helmetProgressNotification);
		}
		_helmetProgressNotification = null;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			UpdateNotification();
			GlobalMessenger.FireEvent("EnterZeroGTraining");
			Locator.GetShipLogManager().RevealFact("TH_ZERO_G_CAVE_X1");
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			RemoveNotification();
			GlobalMessenger.FireEvent("ExitZeroGTraining");
		}
	}
}
