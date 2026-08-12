using UnityEngine;

public class InteractZone : SingleInteractionVolume, ICameraViewInteractable
{
	[SerializeField]
	private float _viewingWindow = 360f;

	private OWTriggerVolume _trigger;

	protected override void Awake()
	{
		base.Awake();
		_trigger = base.gameObject.GetAddComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	public override void EnableInteraction()
	{
		base.EnableInteraction();
		_trigger.SetTriggerActivation(active: true);
	}

	public override void DisableInteraction()
	{
		base.DisableInteraction();
		_trigger.SetTriggerActivation(active: false);
	}

	public override void UpdateInteractVolume()
	{
		float num = 2f * Vector3.Angle(_playerCam.transform.forward, base.transform.forward);
		_focused = num <= _viewingWindow;
		base.UpdateInteractVolume();
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			FirstPersonManipulator component = Locator.GetPlayerCamera().GetComponent<FirstPersonManipulator>();
			if (component != null)
			{
				component.OnEnterInteractZone(this);
			}
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			FirstPersonManipulator component = Locator.GetPlayerCamera().GetComponent<FirstPersonManipulator>();
			if (component != null)
			{
				component.OnExitInteractZone(this);
			}
		}
	}
}
