using UnityEngine;

public class MummyCircleController : SectoredMonoBehaviour
{
	[SerializeField]
	private Animator[] _animators;

	private bool _sectorActive;

	private bool _fastForwarding;

	protected override void Awake()
	{
		base.Awake();
		GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
	}

	private void UpdateAnimatorsActive()
	{
		bool flag = _sectorActive && !_fastForwarding;
		for (int i = 0; i < _animators.Length; i++)
		{
			_animators[i].enabled = flag;
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		_sectorActive = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		UpdateAnimatorsActive();
	}

	protected virtual void OnStartFastForward()
	{
		_fastForwarding = true;
		UpdateAnimatorsActive();
	}

	protected virtual void OnEndFastForward()
	{
		_fastForwarding = false;
		UpdateAnimatorsActive();
	}
}
