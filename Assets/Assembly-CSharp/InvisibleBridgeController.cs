using UnityEngine;

public class InvisibleBridgeController : SectoredMonoBehaviour
{
	[SerializeField]
	private int _codeIndex;

	[SerializeField]
	private Transform _codeDial;

	[SerializeField]
	private Transform _simulationVisuals;

	private void Start()
	{
		base.enabled = false;
	}

	private void Update()
	{
		float y = OWMath.WrapAngle(_codeDial.localEulerAngles.y - (float)_codeIndex * 45f) / 180f * 8f;
		Vector3 localPosition = base.transform.localPosition;
		localPosition.y = y;
		base.transform.localPosition = localPosition;
		_simulationVisuals.localPosition = localPosition;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsOccupant(DynamicOccupant.Player);
	}
}
