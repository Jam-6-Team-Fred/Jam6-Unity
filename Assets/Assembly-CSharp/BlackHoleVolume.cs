using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BlackHoleVolume : VanishVolume
{
	[SerializeField]
	private SingularityController _singularityController;

	[Header("Audio")]
	[SerializeField]
	private Sector _audioSector;

	[SerializeField]
	private OWAudioSource _emissionSource;

	private WhiteHoleVolume _whiteHole;

	protected override void Awake()
	{
		base.Awake();
	}

	private void Start()
	{
		AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.WhiteHole);
		if ((bool)astroObject)
		{
			_whiteHole = astroObject.GetRequiredComponentInChildren<WhiteHoleVolume>();
		}
	}

	protected override void Vanish(OWRigidbody bodyToVanish, RelativeLocationData entryLocation)
	{
		if (_audioSector.ContainsOccupant(DynamicOccupant.Player))
		{
			_emissionSource.PlayOneShot(AudioType.BH_BlackHoleEmission);
		}
		if (_singularityController != null)
		{
			_singularityController.PlayEntryAudio();
		}
		MeteorController component = bodyToVanish.GetComponent<MeteorController>();
		if (component != null && (_whiteHole == null || Random.value > 0.1f))
		{
			component.transform.localScale = Vector3.one;
			component.Suspend();
		}
		else if (_whiteHole == null || bodyToVanish.GetMass() < 0.05f)
		{
			Object.Destroy(bodyToVanish.gameObject);
		}
		else
		{
			_whiteHole.ReceiveWarpedBody(bodyToVanish, entryLocation);
		}
	}

	protected override void VanishModelRocketShip(OWRigidbody modelShipBody, RelativeLocationData entryLocation)
	{
		Vanish(modelShipBody, entryLocation);
	}

	protected override void VanishPlayer(OWRigidbody playerBody, RelativeLocationData entryLocation)
	{
		if (PlayerState.IsInsideShip() || PlayerState.IsInsideShuttle())
		{
			return;
		}
		if (_whiteHole == null)
		{
			Locator.GetDeathManager().KillPlayer(DeathType.Supernova);
			return;
		}
		if (_singularityController != null)
		{
			_singularityController.PlayEntryAudio(isPlayer: true);
		}
		GlobalMessenger.FireEvent("PlayerEnterBlackHole");
		_whiteHole.ReceiveWarpedBody(playerBody, entryLocation);
	}

	protected override void VanishNomaiShuttle(OWRigidbody shuttleBody, RelativeLocationData entryLocation)
	{
		if (_whiteHole == null)
		{
			Object.Destroy(shuttleBody.gameObject);
			return;
		}
		if (shuttleBody.GetComponentInChildren<NomaiShuttleController>().IsPlayerInside())
		{
			MonoBehaviour.print("warp shuttle");
			if (_singularityController != null)
			{
				_singularityController.PlayEntryAudio(isPlayer: true);
			}
		}
		else
		{
			if (_audioSector.ContainsOccupant(DynamicOccupant.Player))
			{
				_emissionSource.PlayOneShot(AudioType.BH_BlackHoleEmission);
			}
			if (_singularityController != null)
			{
				_singularityController.PlayEntryAudio();
			}
		}
		_whiteHole.ReceiveWarpedBody(shuttleBody, entryLocation);
	}

	protected override void VanishShip(OWRigidbody shipBody, RelativeLocationData entryLocation)
	{
		if (_whiteHole == null)
		{
			Object.Destroy(shipBody.gameObject);
			return;
		}
		bool flag = !shipBody.GetComponent<ShipDamageController>().IsCockpitDetached();
		if (PlayerState.IsInsideShip() || PlayerState.UsingShipComputer() || (flag && PlayerState.AtFlightConsole()))
		{
			if (_singularityController != null)
			{
				_singularityController.PlayEntryAudio(isPlayer: true);
			}
		}
		else
		{
			if (_audioSector.ContainsOccupant(DynamicOccupant.Player))
			{
				_emissionSource.PlayOneShot(AudioType.BH_BlackHoleEmission);
			}
			if (_singularityController != null)
			{
				_singularityController.PlayEntryAudio();
			}
		}
		_whiteHole.ReceiveWarpedBody(shipBody, entryLocation);
	}

	protected override void VanishShipCockpit(OWRigidbody shipCockpitBody, RelativeLocationData entryLocation)
	{
		if (_whiteHole == null)
		{
			Object.Destroy(shipCockpitBody.gameObject);
			return;
		}
		if (PlayerState.AtFlightConsole())
		{
			if (_singularityController != null)
			{
				_singularityController.PlayEntryAudio(isPlayer: true);
			}
		}
		else
		{
			if (_audioSector.ContainsOccupant(DynamicOccupant.Player))
			{
				_emissionSource.PlayOneShot(AudioType.BH_BlackHoleEmission);
			}
			if (_singularityController != null)
			{
				_singularityController.PlayEntryAudio();
			}
		}
		_whiteHole.ReceiveWarpedBody(shipCockpitBody, entryLocation);
	}

	protected override void VanishProbe(OWRigidbody probeBody, RelativeLocationData entryLocation)
	{
		SurveyorProbe requiredComponent = probeBody.GetRequiredComponent<SurveyorProbe>();
		if (_whiteHole == null)
		{
			requiredComponent.ExternalRetrieve();
		}
		else if (!requiredComponent.IsAnchored())
		{
			if (_audioSector.ContainsOccupant(DynamicOccupant.Player))
			{
				_emissionSource.PlayOneShot(AudioType.BH_BlackHoleEmission);
			}
			if (_singularityController != null)
			{
				_singularityController.PlayEntryAudio();
			}
			_whiteHole.ReceiveWarpedBody(probeBody, entryLocation);
		}
	}
}
