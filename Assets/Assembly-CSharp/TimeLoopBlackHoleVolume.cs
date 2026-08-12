using UnityEngine;

public class TimeLoopBlackHoleVolume : VanishVolume
{
	public delegate void TimeLoopBlackHoleEvent();

	public event TimeLoopBlackHoleEvent OnProbeEnteredCore;

	public event TimeLoopBlackHoleEvent OnPlayerEnteredCore;

	public void SetActive(bool value)
	{
		_collider.enabled = value;
		base.enabled = value;
	}

	protected override void Vanish(OWRigidbody bodyToVanish, RelativeLocationData entryLocation)
	{
		bodyToVanish.gameObject.SetActive(value: false);
	}

	protected override void VanishPlayer(OWRigidbody playerBody, RelativeLocationData entryLocation)
	{
		if (this.OnPlayerEnteredCore != null)
		{
			this.OnPlayerEnteredCore();
		}
	}

	protected override void VanishShip(OWRigidbody shipBody, RelativeLocationData entryLocation)
	{
		bool flag = !shipBody.GetComponent<ShipDamageController>().IsCockpitDetached();
		if (PlayerState.IsInsideShip() || PlayerState.UsingShipComputer() || (flag && PlayerState.AtFlightConsole()))
		{
			if (this.OnPlayerEnteredCore != null)
			{
				this.OnPlayerEnteredCore();
			}
		}
		else
		{
			Vanish(shipBody, entryLocation);
		}
	}

	protected override void VanishShipCockpit(OWRigidbody shipCockpitBody, RelativeLocationData entryLocation)
	{
		if (PlayerState.AtFlightConsole())
		{
			Locator.GetDeathManager().KillPlayer(DeathType.TimeLoop);
		}
		else
		{
			Vanish(shipCockpitBody, entryLocation);
		}
	}

	protected override void VanishNomaiShuttle(OWRigidbody shuttleBody, RelativeLocationData entryLocation)
	{
		if (shuttleBody.GetComponentInChildren<NomaiShuttleController>().IsPlayerInside())
		{
			Locator.GetDeathManager().KillPlayer(DeathType.TimeLoop);
		}
		else
		{
			Vanish(shuttleBody, entryLocation);
		}
	}

	protected override void VanishProbe(OWRigidbody probeBody, RelativeLocationData entryLocation)
	{
		SurveyorProbe requiredComponent = probeBody.GetRequiredComponent<SurveyorProbe>();
		if (requiredComponent.IsLaunched())
		{
			if (this.OnProbeEnteredCore != null)
			{
				this.OnProbeEnteredCore();
			}
			Object.Destroy(requiredComponent.gameObject);
		}
	}

	protected override void VanishModelRocketShip(OWRigidbody modelShipBody, RelativeLocationData entryLocation)
	{
		Achievements.Earn(Achievements.Type.MICAS_WRATH);
		Vanish(modelShipBody, entryLocation);
		GlobalMessenger.FireEvent("OnModelRocketShipDestroyed");
	}
}
