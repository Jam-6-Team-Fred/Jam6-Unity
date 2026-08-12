using UnityEngine;

public class NomaiStargazePlatform : SectoredMonoBehaviour
{
	[SerializeField]
	private AstroObject.Name _targetObject;

	private Transform _target;

	private QuantumMoon _quantumMoon;

	private void Start()
	{
		AstroObject astroObject = Locator.GetAstroObject(_targetObject);
		if (astroObject != null)
		{
			_target = astroObject.transform;
			if (astroObject.GetAstroObjectName() == AstroObject.Name.QuantumMoon)
			{
				_quantumMoon = astroObject.GetComponent<QuantumMoon>();
			}
		}
		base.enabled = false;
	}

	private void FixedUpdate()
	{
		if (_target != null)
		{
			if (_quantumMoon == null || _quantumMoon.GetStateIndex() != 5)
			{
				Vector3 forward = Vector3.ProjectOnPlane(_target.position - base.transform.position, base.transform.up);
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(forward, base.transform.up), 30f * Time.deltaTime);
			}
			else
			{
				base.transform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.Self);
			}
		}
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			_ = base.enabled;
			base.enabled = true;
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			_ = base.enabled;
			base.enabled = false;
		}
	}
}
