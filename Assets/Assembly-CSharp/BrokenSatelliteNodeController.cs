using UnityEngine;

public class BrokenSatelliteNodeController : MonoBehaviour
{
	[SerializeField]
	private Light _damageLight;

	private RepairVolume _repairVolume;

	private SparkEffectController[] _sparkEffects;

	private Vector3 _finalCoverLocalPos;

	private ReferenceFrameVolume _rfVolume;

	private void Awake()
	{
		_repairVolume = this.GetRequiredComponentInChildren<RepairVolume>();
		_repairVolume.OnCompleteRepair += OnCompleteRepair;
		_sparkEffects = GetComponentsInChildren<SparkEffectController>();
		_rfVolume = this.GetRequiredComponentInChildren<ReferenceFrameVolume>();
	}

	private void OnDestroy()
	{
		_repairVolume.OnCompleteRepair -= OnCompleteRepair;
	}

	private void OnCompleteRepair()
	{
		_damageLight.enabled = false;
		GetComponentInChildren<ReferenceFrameVolume>().gameObject.SetActive(value: false);
		if (Locator.GetPlayerTransform() != null)
		{
			ReferenceFrameTracker component = Locator.GetPlayerTransform().GetComponent<ReferenceFrameTracker>();
			if (component.GetReferenceFrame() == _rfVolume.GetReferenceFrame())
			{
				component.UntargetReferenceFrame();
			}
		}
		for (int i = 0; i < _sparkEffects.Length; i++)
		{
			if (_sparkEffects[i] != null)
			{
				_sparkEffects[i].Disable();
			}
		}
	}
}
