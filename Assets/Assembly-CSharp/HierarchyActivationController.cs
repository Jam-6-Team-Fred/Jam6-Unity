using UnityEngine;

public class HierarchyActivationController : MonoBehaviour
{
	[SerializeField]
	private GameObject _hierarchyRoot;

	[SerializeField]
	private DynamicOccupantMask _activationMask;

	private Sector _sector;

	private void Start()
	{
		_sector = base.gameObject.GetRequiredComponent<Sector>();
		_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
	}

	private void OnDestroy()
	{
		_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
	}

	private void OnSectorOccupantsUpdated()
	{
		if (_sector.ContainsAnyOccupants(_activationMask.GetMask()))
		{
			_hierarchyRoot.SetActive(value: true);
		}
		else
		{
			_hierarchyRoot.SetActive(value: false);
		}
	}
}
