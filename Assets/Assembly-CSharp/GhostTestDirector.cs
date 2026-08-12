using UnityEngine;

public class GhostTestDirector : GhostDirector
{
	[Space]
	[SerializeField]
	private OWTriggerVolume _suspicionTrigger;

	[SerializeField]
	private DreamObjectProjector _wakeTrigger;

	protected override void Awake()
	{
		base.Awake();
		if (_suspicionTrigger != null)
		{
			_suspicionTrigger.OnEntry += OnEnterSuspicionTrigger;
		}
		if (_wakeTrigger != null)
		{
			_wakeTrigger.OnProjectorExtinguished += new OWEvent.OWCallback(WakeGhosts);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_suspicionTrigger != null)
		{
			_suspicionTrigger.OnEntry -= OnEnterSuspicionTrigger;
		}
		if (_wakeTrigger != null)
		{
			_wakeTrigger.OnProjectorExtinguished -= new OWEvent.OWCallback(WakeGhosts);
		}
	}

	private void OnEnterSuspicionTrigger(GameObject hitObj)
	{
		if (_ghostsAreAwake && hitObj.CompareTag("PlayerDetector"))
		{
			for (int i = 0; i < _directedGhosts.Length; i++)
			{
				_directedGhosts[i].EscalateThreatAwareness(GhostData.ThreatAwareness.SomeoneIsInHere);
			}
		}
	}

	protected override void WakeGhosts()
	{
		base.WakeGhosts();
		for (int i = 0; i < _directedGhosts.Length; i++)
		{
			_directedGhosts[i].EscalateThreatAwareness(GhostData.ThreatAwareness.SomethingIsAmiss);
		}
	}
}
