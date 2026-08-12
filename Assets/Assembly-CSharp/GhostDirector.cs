using UnityEngine;

public class GhostDirector : MonoBehaviour
{
	[SerializeField]
	protected Sector _sector;

	[SerializeField]
	protected DreamArrivalPoint _dreamArrivalPoint;

	[SerializeField]
	protected GhostBrain[] _directedGhosts = new GhostBrain[0];

	[SerializeField]
	protected bool _startAwake = true;

	[Space]
	[SerializeField]
	private OWAudioSource _ghostGroupDeathAudioSource;

	protected DreamCampfire _connectedDreamCampfire;

	protected bool _ghostsAreAwake;

	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
		if (_startAwake)
		{
			WakeGhosts();
		}
		if (_dreamArrivalPoint != null)
		{
			_connectedDreamCampfire = Locator.GetDreamCampfire(_dreamArrivalPoint.GetLocation());
		}
		if (_connectedDreamCampfire != null)
		{
			_connectedDreamCampfire.OnDreamCampfireExtinguished += new OWEvent.OWCallback(OnConnectedCampfireExtinguished);
		}
	}

	protected virtual void OnDestroy()
	{
		if (_connectedDreamCampfire != null)
		{
			_connectedDreamCampfire.OnDreamCampfireExtinguished -= new OWEvent.OWCallback(OnConnectedCampfireExtinguished);
		}
	}

	protected virtual void OnConnectedCampfireExtinguished()
	{
		if (PlayerState.InDreamWorld() && _ghostGroupDeathAudioSource != null)
		{
			_ghostGroupDeathAudioSource.PlayOneShot(AudioType.Ghost_DeathGroup);
		}
		for (int i = 0; i < _directedGhosts.Length; i++)
		{
			_directedGhosts[i].Die();
		}
	}

	protected virtual void WakeGhosts()
	{
		for (int i = 0; i < _directedGhosts.Length; i++)
		{
			_directedGhosts[i].WakeUp();
		}
		_ghostsAreAwake = true;
		GlobalMessenger.FireEvent("GhostsAwoken");
	}
}
