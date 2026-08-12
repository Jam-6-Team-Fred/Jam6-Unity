using UnityEngine;

public class BrokenNode : MonoBehaviour
{
	[SerializeField]
	private Material _repairedMaterial;

	private RepairVolume _repairVolume;

	private void Awake()
	{
		_repairVolume = this.GetRequiredComponentInChildren<RepairVolume>();
		_repairVolume.OnCompleteRepair += OnCompleteRepair;
	}

	private void OnDestroy()
	{
		_repairVolume.OnCompleteRepair -= OnCompleteRepair;
	}

	private void OnCompleteRepair()
	{
		ParticleSystem componentInChildren = GetComponentInChildren<ParticleSystem>();
		if (componentInChildren != null)
		{
			componentInChildren.gameObject.SetActive(value: false);
		}
		GetComponentInChildren<Light>().color = Color.green;
		MeshRenderer componentInChildren2 = GetComponentInChildren<MeshRenderer>();
		if (componentInChildren2 != null && _repairedMaterial != null)
		{
			componentInChildren2.sharedMaterial = _repairedMaterial;
		}
	}
}
