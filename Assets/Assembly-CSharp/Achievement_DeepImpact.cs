using UnityEngine;

public class Achievement_DeepImpact : SectoredMonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _tornadoDownTrigger;

	[SerializeField]
	private OWTriggerVolume _oceanInteriorTrigger;

	private bool _touchedTornado;

	protected override void Awake()
	{
		base.Awake();
		_tornadoDownTrigger.OnEntry += OnEnterTornadoDown;
		_oceanInteriorTrigger.OnEntry += OnEnterOcean;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_tornadoDownTrigger.OnEntry -= OnEnterTornadoDown;
		_oceanInteriorTrigger.OnEntry -= OnEnterOcean;
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (!_sector.ContainsAnyOccupants(DynamicOccupant.Player))
		{
			_touchedTornado = false;
		}
	}

	private void OnEnterTornadoDown(GameObject hitObj)
	{
		_touchedTornado = true;
	}

	private void OnEnterOcean(GameObject hitObj)
	{
		if (!_touchedTornado && hitObj.CompareTag("PlayerDetector"))
		{
			Achievements.Earn(Achievements.Type.DEEP_IMPACT);
			Object.Destroy(this);
		}
	}
}
