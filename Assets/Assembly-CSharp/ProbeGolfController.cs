using UnityEngine;

public class ProbeGolfController : MonoBehaviour
{
	[SerializeField]
	private ProbePhotoTarget[] _targets;

	[SerializeField]
	private OWTriggerVolume _gameTrigger;

	private NotificationData _scoreNotification;

	private bool _gameStarted;

	private void Awake()
	{
		_gameTrigger.OnEntry += OnEntryGameTrigger;
		_gameTrigger.OnExit += OnExitGameTrigger;
	}

	private void OnDestroy()
	{
		_gameTrigger.OnEntry -= OnEntryGameTrigger;
		_gameTrigger.OnExit -= OnExitGameTrigger;
		EndGame();
	}

	private void OnPressInteract()
	{
		if (!_gameStarted)
		{
			StartGame();
		}
		else
		{
			EndGame();
		}
	}

	private void OnEntryGameTrigger(GameObject hitObj)
	{
		if (!_gameStarted && hitObj.CompareTag("PlayerDetector"))
		{
			StartGame();
		}
	}

	private void OnExitGameTrigger(GameObject hitObj)
	{
		if (_gameStarted && hitObj.CompareTag("PlayerDetector"))
		{
			EndGame();
		}
	}

	private void StartGame()
	{
		_gameStarted = true;
		for (int i = 0; i < _targets.Length; i++)
		{
			_targets[i].OnPhotographedByProbe += OnTargetPhotographed;
		}
		NotificationManager.SharedInstance.PostNotification(_scoreNotification, pin: true);
	}

	private void EndGame()
	{
		_gameStarted = false;
		for (int i = 0; i < _targets.Length; i++)
		{
			_targets[i].OnPhotographedByProbe -= OnTargetPhotographed;
		}
		if (_scoreNotification != null)
		{
			NotificationManager.SharedInstance.UnpinNotification(_scoreNotification);
			_scoreNotification = null;
		}
	}

	private void OnTargetPhotographed(ProbePhotoTarget target, float score)
	{
		_scoreNotification.displayMessage = target.GetName() + ": " + score;
	}
}
