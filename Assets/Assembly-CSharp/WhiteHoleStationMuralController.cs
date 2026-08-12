using UnityEngine;

public class WhiteHoleStationMuralController : SectoredMonoBehaviour
{
	[SerializeField]
	private Transform _rotatingDisk;

	private Transform _whitHoleStationTransform;

	private void Start()
	{
		_whitHoleStationTransform = base.gameObject.GetAttachedOWRigidbody().transform;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}

	private void FixedUpdate()
	{
		AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.BrittleHollow);
		if (astroObject != null)
		{
			Vector3 from = astroObject.transform.position - _whitHoleStationTransform.position;
			from.y = 0f;
			Vector3 forward = _whitHoleStationTransform.forward;
			forward.y = 0f;
			float y = OWMath.Angle(from, forward, Vector3.up);
			_rotatingDisk.localEulerAngles = new Vector3(0f, y, 0f);
		}
	}
}
