using UnityEngine;

public class GhostData
{
	public enum ThreatAwareness
	{
		EverythingIsNormal = 0,
		SomethingIsAmiss = 10,
		SomeoneIsInHere = 20,
		IntruderConfirmed = 30
	}

	public GhostLocationData playerLocation = new GhostLocationData();

	public GhostLocationData lastKnownPlayerLocation = new GhostLocationData();

	public GhostSensorData sensor = new GhostSensorData();

	public GhostSensorData lastKnownSensor = new GhostSensorData();

	private GhostSensorData firstUnknownSensor = new GhostSensorData();

	public ThreatAwareness threatAwareness;

	public GhostAction.Name currentAction = GhostAction.Name.None;

	public GhostAction.Name previousAction = GhostAction.Name.None;

	public bool isAlive = true;

	public bool hasWokenUp;

	public bool isPlayerLocationKnown;

	public bool wasPlayerLocationKnown;

	public bool reduceGuardUtility;

	public bool fastStalkUnlocked;

	public float timeLastSawPlayer;

	public float timeSincePlayerLocationKnown = float.PositiveInfinity;

	public float playerMinLanternRange;

	public float illuminatedByPlayerMeter;

	public bool hasChokePoint;

	public Vector3 chokePointLocalPosition;

	public Vector3 chokePointLocalFacing;

	public bool reducedFrights_allowChase;

	public bool lostPlayerDueToOcclusion
	{
		get
		{
			if (!isPlayerLocationKnown && !lastKnownSensor.isPlayerOccluded)
			{
				return firstUnknownSensor.isPlayerOccluded;
			}
			return false;
		}
	}

	public void TabulaRasa()
	{
		threatAwareness = ThreatAwareness.EverythingIsNormal;
		isPlayerLocationKnown = false;
		wasPlayerLocationKnown = false;
		reduceGuardUtility = false;
		fastStalkUnlocked = false;
		timeLastSawPlayer = 0f;
		timeSincePlayerLocationKnown = float.PositiveInfinity;
		playerMinLanternRange = 0f;
		illuminatedByPlayerMeter = 0f;
	}

	public void OnPlayerExitDreamWorld()
	{
		isPlayerLocationKnown = false;
		wasPlayerLocationKnown = false;
		reduceGuardUtility = false;
		fastStalkUnlocked = false;
		timeSincePlayerLocationKnown = float.PositiveInfinity;
	}

	public void OnEnterAction(GhostAction.Name actionName)
	{
		if (actionName == GhostAction.Name.IdentifyIntruder || (uint)(actionName - 50) <= 2u)
		{
			reduceGuardUtility = true;
		}
	}

	public void FixedUpdate_Data(GhostController controller, GhostSensors sensors)
	{
		wasPlayerLocationKnown = isPlayerLocationKnown;
		isPlayerLocationKnown = sensor.isPlayerVisible || sensor.isPlayerHeldLanternVisible || sensor.isIlluminatedByPlayer || sensor.inContactWithPlayer;
		if (!reduceGuardUtility && sensor.isIlluminatedByPlayer)
		{
			reduceGuardUtility = true;
		}
		Vector3 worldPosition = Locator.GetPlayerTransform().position - Locator.GetPlayerTransform().up;
		Vector3 worldVelocity = Locator.GetPlayerBody().GetVelocity() - controller.GetNodeMap().GetOWRigidbody().GetVelocity();
		playerLocation.Update(worldPosition, worldVelocity, controller);
		playerMinLanternRange = Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController()
			.GetMinRange();
		if (isPlayerLocationKnown)
		{
			lastKnownPlayerLocation.CopyFromOther(playerLocation);
			lastKnownSensor.CopyFromOther(sensor);
			timeLastSawPlayer = Time.time;
			timeSincePlayerLocationKnown = 0f;
		}
		else
		{
			if (wasPlayerLocationKnown)
			{
				firstUnknownSensor.CopyFromOther(sensor);
			}
			lastKnownPlayerLocation.Update(controller);
			timeSincePlayerLocationKnown += Time.deltaTime;
		}
		if (threatAwareness >= ThreatAwareness.IntruderConfirmed && sensor.isIlluminatedByPlayer && !PlayerData.GetReducedFrights())
		{
			illuminatedByPlayerMeter += Time.deltaTime;
		}
		else
		{
			illuminatedByPlayerMeter = Mathf.Max(0f, illuminatedByPlayerMeter - Time.deltaTime * 0.5f);
		}
	}
}
