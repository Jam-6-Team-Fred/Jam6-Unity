using UnityEngine;

public class DetachOnFloodImpact : MonoBehaviour
{
	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[SerializeField]
	private DetachableBuilding _detachableBuilding;

	[Space]
	[SerializeField]
	private GameObject _detectorGameObject;

	[SerializeField]
	private Collider[] _preDetachColliders = new Collider[0];

	[SerializeField]
	private Collider[] _postDetachColliders = new Collider[0];

	[Space]
	[SerializeField]
	private SectorCullGroup _cullGroup;

	[SerializeField]
	private Sector _newCullSector;

	[SerializeField]
	private SectorCollisionGroup _collisionGroup;

	[SerializeField]
	private Sector _newCollisionSector;

	private void Awake()
	{
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact += new OWEvent.OWCallback(Detach);
		}
		if (_detectorGameObject != null && _detectorGameObject.activeSelf)
		{
			Debug.LogWarning("DetachOnFloodImpact Detectors should start with their GameObject Disabled!", this);
		}
	}

	private void Start()
	{
		for (int i = 0; i < _postDetachColliders.Length; i++)
		{
			_postDetachColliders[i].GetComponent<OWCollider>().SetActivation(active: false);
		}
	}

	private void OnDestroy()
	{
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(Detach);
		}
	}

	public void Detach()
	{
		for (int i = 0; i < _preDetachColliders.Length; i++)
		{
			_preDetachColliders[i].GetComponent<OWCollider>().SetActivation(active: false);
		}
		_detachableBuilding.Detach(kinematicSimulation: false);
		for (int j = 0; j < _postDetachColliders.Length; j++)
		{
			_postDetachColliders[j].GetComponent<OWCollider>().SetActivation(active: true);
		}
		if (_detectorGameObject != null)
		{
			_detectorGameObject.SetActive(value: true);
		}
		if (_cullGroup != null)
		{
			_cullGroup.SetSector(_newCullSector);
		}
		if (_collisionGroup != null)
		{
			_collisionGroup.SetSector(_newCollisionSector);
		}
	}
}
