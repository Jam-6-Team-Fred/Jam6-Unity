using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SubstanceDetector : MonoBehaviour
{
	private List<SubstanceVolume> _substanceVolumeList;

	private void Awake()
	{
		GetComponent<Collider>().isTrigger = false;
		base.gameObject.layer = LayerMask.NameToLayer("AdvancedDetector");
		_substanceVolumeList = new List<SubstanceVolume>();
	}

	public void AddSubstanceVolume(SubstanceVolume substanceVolume)
	{
		if (!_substanceVolumeList.Contains(substanceVolume))
		{
			_substanceVolumeList.Add(substanceVolume);
		}
	}

	public void RemoveSubstanceVolume(SubstanceVolume substanceVolume)
	{
		if (_substanceVolumeList.Contains(substanceVolume))
		{
			_substanceVolumeList.Remove(substanceVolume);
		}
	}
}
