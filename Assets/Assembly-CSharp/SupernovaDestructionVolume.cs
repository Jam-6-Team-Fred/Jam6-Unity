public class SupernovaDestructionVolume : DestructionVolume
{
	private bool _checkForProbeDestruction;

	private bool _checkForPlayerDestruction;

	private bool _timeLoopDestroyed;

	private bool _playerInsideTimeLoopDevice;

	private void OnValidate()
	{
		if (_deathType != DeathType.Supernova)
		{
			_deathType = DeathType.Supernova;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		GlobalMessenger.AddListener("TimeLoopInteriorDestroyed", OnTimeLoopInteriorDestroyed);
		GlobalMessenger<OWRigidbody>.AddListener("EnterTimeLoopCentral", OnEnterTimeLoopCentral);
		GlobalMessenger<OWRigidbody>.AddListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("TimeLoopInteriorDestroyed", OnTimeLoopInteriorDestroyed);
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterTimeLoopCentral", OnEnterTimeLoopCentral);
		GlobalMessenger<OWRigidbody>.RemoveListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
	}

	public void SetActivation(bool active)
	{
		_collider.enabled = active;
		base.enabled = true;
	}

	protected override void Vanish(OWRigidbody bodyToVanish, RelativeLocationData entryLocation)
	{
		if (bodyToVanish.GetOrigParentBody() != null)
		{
			AstroObject component = bodyToVanish.GetOrigParentBody().GetComponent<AstroObject>();
			if (component != null && (component.GetAstroObjectName() == AstroObject.Name.DreamWorld || (bodyToVanish.GetMass() > 50f && component.GetAstroObjectName() == AstroObject.Name.GiantsDeep)))
			{
				return;
			}
		}
		SurveyorProbe componentInChildren = bodyToVanish.transform.GetComponentInChildren<SurveyorProbe>();
		if (!(componentInChildren != null) || !componentInChildren.IsTimeLoopCoreDuplicate())
		{
			base.Vanish(bodyToVanish, entryLocation);
		}
	}

	protected override void VanishPlayer(OWRigidbody playerBody, RelativeLocationData entryLocation)
	{
		if (!PlayerState.InDreamWorld())
		{
			_checkForPlayerDestruction = true;
		}
	}

	protected override void VanishProbe(OWRigidbody probeBody, RelativeLocationData entryLocation)
	{
		_checkForProbeDestruction = true;
	}

	protected override void VanishModelRocketShip(OWRigidbody modelShipBody, RelativeLocationData entryLocation)
	{
		Vanish(modelShipBody, entryLocation);
		GlobalMessenger.FireEvent("OnModelRocketShipDestroyed");
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_checkForPlayerDestruction && (_timeLoopDestroyed || !_playerInsideTimeLoopDevice))
		{
			Locator.GetDeathManager().KillPlayer(DeathType.Supernova);
			_checkForPlayerDestruction = false;
		}
		if (_checkForProbeDestruction && Locator.GetProbe() != null && Locator.GetProbe().IsLaunched() && (_timeLoopDestroyed || !Locator.GetProbe().GetSectorDetector().IsWithinSector(Sector.Name.TimeLoopDevice)))
		{
			Locator.GetProbe().ExternalRetrieve();
			_checkForProbeDestruction = false;
		}
	}

	private void OnTimeLoopInteriorDestroyed()
	{
		_timeLoopDestroyed = true;
	}

	private void OnEnterTimeLoopCentral(OWRigidbody body)
	{
		if (body.CompareTag("Player"))
		{
			_playerInsideTimeLoopDevice = true;
		}
	}

	private void OnExitTimeLoopCentral(OWRigidbody body)
	{
		if (body.CompareTag("Player"))
		{
			_playerInsideTimeLoopDevice = false;
		}
	}
}
