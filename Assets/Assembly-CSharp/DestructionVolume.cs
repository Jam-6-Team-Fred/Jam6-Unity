using UnityEngine;

public class DestructionVolume : VanishVolume
{
	[SerializeField]
	protected DeathType _deathType = DeathType.Energy;

	protected override void Vanish(OWRigidbody bodyToVanish, RelativeLocationData entryLocation)
	{
		bodyToVanish.gameObject.SetActive(value: false);
		ReferenceFrameTracker component = Locator.GetPlayerBody().GetComponent<ReferenceFrameTracker>();
		if (component.GetReferenceFrame() != null && component.GetReferenceFrame().GetOWRigidBody() == bodyToVanish)
		{
			component.UntargetReferenceFrame();
		}
		MapMarker component2 = bodyToVanish.GetComponent<MapMarker>();
		if (component2 != null)
		{
			component2.DisableMarker();
		}
		AstroObject component3 = bodyToVanish.GetComponent<AstroObject>();
		if (component3 != null && component3.GetAstroObjectName() == AstroObject.Name.Comet)
		{
			GlobalMessenger.FireEvent("CometDestroyed");
		}
	}

	protected override void VanishPlayer(OWRigidbody playerBody, RelativeLocationData entryLocation)
	{
		Locator.GetDeathManager().KillPlayer(_deathType);
	}

	protected override void VanishShip(OWRigidbody shipBody, RelativeLocationData entryLocation)
	{
		bool flag = !shipBody.GetComponent<ShipDamageController>().IsCockpitDetached();
		if (PlayerState.IsInsideShip() || PlayerState.UsingShipComputer() || (flag && PlayerState.AtFlightConsole()))
		{
			Autopilot component = shipBody.GetComponent<Autopilot>();
			if (component != null && component.IsFlyingToDestination())
			{
				AstroObject componentInParent = GetComponentInParent<AstroObject>();
				if (componentInParent != null && componentInParent.GetAstroObjectType() == AstroObject.Type.Star)
				{
					PlayerData.SetPersistentCondition("AUTOPILOT_INTO_SUN", state: true);
					MonoBehaviour.print("AUTOPILOT_INTO_SUN");
				}
			}
			Locator.GetDeathManager().KillPlayer(_deathType);
		}
		else
		{
			Vanish(shipBody, entryLocation);
			GlobalMessenger.FireEvent("ShipDestroyed");
		}
	}

	protected override void VanishShipCockpit(OWRigidbody shipCockpitBody, RelativeLocationData entryLocation)
	{
		if (PlayerState.AtFlightConsole())
		{
			Locator.GetDeathManager().KillPlayer(_deathType);
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
			Locator.GetDeathManager().KillPlayer(_deathType);
		}
		else
		{
			Vanish(shuttleBody, entryLocation);
		}
	}

	protected override void VanishProbe(OWRigidbody probeBody, RelativeLocationData entryLocation)
	{
		probeBody.GetRequiredComponent<SurveyorProbe>().ExternalRetrieve();
	}

	protected override void VanishModelRocketShip(OWRigidbody modelShipBody, RelativeLocationData entryLocation)
	{
		Achievements.Earn(Achievements.Type.MICAS_WRATH);
		Vanish(modelShipBody, entryLocation);
		GlobalMessenger.FireEvent("OnModelRocketShipDestroyed");
	}
}
