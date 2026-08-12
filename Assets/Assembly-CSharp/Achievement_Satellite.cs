using UnityEngine;

public class Achievement_Satellite : SectoredMonoBehaviour
{
	[SerializeField]
	private OWRigidbody _targetBody;

	private OWRigidbody _owRigidbody;

	private float _startingDistance;

	private void Start()
	{
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
		_startingDistance = (_targetBody.GetWorldCenterOfMass() - _owRigidbody.GetWorldCenterOfMass()).sqrMagnitude;
		base.enabled = false;
	}

	private void Update()
	{
		if (Mathf.Abs((_targetBody.GetWorldCenterOfMass() - _owRigidbody.GetWorldCenterOfMass()).sqrMagnitude - _startingDistance) > 2500f)
		{
			Achievements.Earn(Achievements.Type.SATELLITE);
			Object.Destroy(this);
		}
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		base.enabled = true;
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (!_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
		{
			base.enabled = false;
		}
	}
}
