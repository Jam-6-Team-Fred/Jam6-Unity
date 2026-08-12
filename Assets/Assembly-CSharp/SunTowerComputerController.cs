using UnityEngine;

public class SunTowerComputerController : SectoredMonoBehaviour
{
	[SerializeField]
	private NomaiComputer _computer;

	private bool _activated;

	private void FixedUpdate()
	{
		if (TimeLoop.GetSecondsElapsed() >= 690f && !_activated)
		{
			_computer.DisplayEntry(3);
			base.enabled = false;
			_activated = true;
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}
}
