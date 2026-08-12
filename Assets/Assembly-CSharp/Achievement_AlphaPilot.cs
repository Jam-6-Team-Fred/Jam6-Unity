using UnityEngine;

public class Achievement_AlphaPilot : EffectVolume
{
	[SerializeField]
	private NomaiWarpReceiver _warpReceiver;

	private bool _hasUsedWarpReceiver;

	protected override void Awake()
	{
		base.Awake();
		_warpReceiver.OnReceivePlayerBody += OnWarpReceiverUsed;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_warpReceiver.OnReceivePlayerBody -= OnWarpReceiverUsed;
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		if (!_hasUsedWarpReceiver)
		{
			OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody();
			if (attachedOWRigidbody != null && attachedOWRigidbody.CompareTag("Player"))
			{
				MonoBehaviour.print("TEST");
				Achievements.Earn(Achievements.Type.ALPHA_PILOT);
				PlayerData.SetPersistentCondition("FLEW_TO_SS", state: true);
			}
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
	}

	private void OnWarpReceiverUsed()
	{
		_hasUsedWarpReceiver = true;
	}
}
