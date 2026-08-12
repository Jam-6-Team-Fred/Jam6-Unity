using System.Collections.Generic;
using UnityEngine;

public class IceSpikes : SectoredMonoBehaviour
{
	[SerializeField]
	private List<GameObject> _toDeactivate;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter()
	{
		for (int i = 0; i < _toDeactivate.Count; i++)
		{
			_toDeactivate[i].SetActive(value: false);
		}
	}
}
