using System.Collections.Generic;
using UnityEngine;

public class Achievement_TakeMeAlive : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _triggerVolume;

	[SerializeField]
	private GhostBrain _upstairsGhost;

	[SerializeField]
	private GhostBrain[] _otherGhosts;

	private List<GrabAction> _grabbingActions;

	private bool _jumped;

	private float _timeOfJump = float.PositiveInfinity;

	private void Awake()
	{
		_triggerVolume.OnEntry += OnEntry;
		_grabbingActions = new List<GrabAction>();
		base.enabled = false;
	}

	protected virtual void OnDestroy()
	{
		if (_jumped)
		{
			Achievements.Earn(Achievements.Type.TAKEMEALIVE);
			base.gameObject.SetActive(value: false);
		}
		_triggerVolume.OnEntry -= OnEntry;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (!hitObj.CompareTag("PlayerDetector"))
		{
			return;
		}
		if (_upstairsGhost.GetCurrentActionName() == GhostAction.Name.Chase || _upstairsGhost.GetCurrentActionName() == GhostAction.Name.Hunt || _upstairsGhost.GetCurrentActionName() == GhostAction.Name.Stalk || _upstairsGhost.GetCurrentActionName() == GhostAction.Name.IdentifyIntruder || _upstairsGhost.GetCurrentActionName() == GhostAction.Name.Grab)
		{
			_grabbingActions.Add(_upstairsGhost.GetAction(GhostAction.Name.Grab) as GrabAction);
			base.enabled = true;
			_jumped = true;
			_timeOfJump = Time.time;
		}
		for (int i = 0; i < _otherGhosts.Length; i++)
		{
			if (_otherGhosts[i].GetCurrentActionName() == GhostAction.Name.Chase || _otherGhosts[i].GetCurrentActionName() == GhostAction.Name.Stalk || _otherGhosts[i].GetCurrentActionName() == GhostAction.Name.Grab)
			{
				_grabbingActions.Add(_otherGhosts[i].GetAction(GhostAction.Name.Grab) as GrabAction);
				base.enabled = true;
				_jumped = true;
				_timeOfJump = Time.time;
			}
		}
	}

	private void Update()
	{
		for (int i = 0; i < _grabbingActions.Count; i++)
		{
			if (_grabbingActions[i].isPlayerGrabbed())
			{
				base.enabled = false;
				_jumped = false;
			}
		}
		if (_jumped && Time.time - _timeOfJump > 5f)
		{
			EarnAchievement();
		}
	}

	private void EarnAchievement()
	{
		Achievements.Earn(Achievements.Type.TAKEMEALIVE);
		base.gameObject.SetActive(value: false);
		base.enabled = false;
	}
}
