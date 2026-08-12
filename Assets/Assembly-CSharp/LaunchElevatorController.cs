using UnityEngine;

public class LaunchElevatorController : MonoBehaviour
{
	private Elevator _launchElevator;

	private void Awake()
	{
		Collider component = GetComponent<Collider>();
		if (component != null)
		{
			component.Assert(OWLayerMask.effectVolumeMask, isTrigger: true);
		}
		_launchElevator = this.GetRequiredComponentInChildren<Elevator>();
		_launchElevator.OnPressInteractEvent += OnPressInteract;
		GlobalMessenger.AddListener("LearnLaunchCodes", OnLearnLaunchCodes);
		GlobalMessenger.AddListener("LaunchCodesEntered", OnLaunchCodesEntered);
	}

	private void Start()
	{
		_launchElevator.TakeControlFromElevator();
		if (PlayerData.KnowsLaunchCodes())
		{
			_launchElevator.ResetInteractVolume(UITextType.EnterLaunchCodePrompt, showKeyCommandIcon: true);
		}
		else
		{
			_launchElevator.ResetInteractVolume(UITextType.RequLaunchCodePrompt, showKeyCommandIcon: false);
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("LaunchCodesEntered", OnLaunchCodesEntered);
		GlobalMessenger.RemoveListener("LearnLaunchCodes", OnLearnLaunchCodes);
		_launchElevator.OnPressInteractEvent -= OnPressInteract;
	}

	private void OnLaunchCodesEntered()
	{
		GetComponentInChildren<Light>().enabled = true;
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		if (hitCollider.CompareTag("PlayerDetector") && _launchElevator.GetTrackPositionFraction() > 0.9f)
		{
			_launchElevator.ReturnToStart();
		}
	}

	private void OnLearnLaunchCodes()
	{
		_launchElevator.ChangePromptText(UITextType.EnterLaunchCodePrompt, showKeyCommandIcon: true);
	}

	private void OnPressInteract()
	{
		if (PlayerData.KnowsLaunchCodes())
		{
			Locator.GetPlayerAudioController().PlayEnterLaunchCodes();
			GlobalMessenger.FireEvent("LaunchCodesEntered");
			_launchElevator.ChangePromptText(UITextType.ActivateLiftPrompt, showKeyCommandIcon: true);
			_launchElevator.AttachPlayerAndStartLift();
		}
	}
}
