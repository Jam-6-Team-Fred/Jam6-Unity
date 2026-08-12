using System.Collections.Generic;
using UnityEngine;

public class WhiteHoleVolume : MonoBehaviour
{
	[SerializeField]
	private Sector _whiteHoleSector;

	[SerializeField]
	private NomaiAirlock[] _airlocksToOpen;

	[Space]
	[SerializeField]
	private float _radius = 50f;

	[SerializeField]
	private float _debrisDistMin = 150f;

	[SerializeField]
	private float _debrisDistMax = 1000f;

	private OWRigidbody _whiteHoleBody;

	private ProxyShadowCasterSuperGroup _whiteHoleProxyShadowSuperGroup;

	private List<OWRigidbody> _growQueue;

	private List<RelativeLocationData> _growQueueLocationData;

	private List<OWRigidbody> _ejectedBodyList;

	private OWRigidbody _growingBody;

	private WhiteHoleFluidVolume _fluidVolume;

	private float _nextGrowCheckTime;

	private float _lastShipWarpTime;

	private void Awake()
	{
		_growQueue = new List<OWRigidbody>(8);
		_growQueueLocationData = new List<RelativeLocationData>(8);
		_ejectedBodyList = new List<OWRigidbody>(64);
		_whiteHoleBody = base.gameObject.GetAttachedOWRigidbody();
		_whiteHoleProxyShadowSuperGroup = _whiteHoleBody.GetComponentInChildren<ProxyShadowCasterSuperGroup>();
		_fluidVolume = base.gameObject.GetRequiredComponent<WhiteHoleFluidVolume>();
	}

	public void ReceiveWarpedBody(OWRigidbody warpedBody, RelativeLocationData entryData)
	{
		if (warpedBody.CompareTag("Player") || warpedBody.CompareTag("Ship") || warpedBody.CompareTag("ShipCockpit") || warpedBody.CompareTag("Probe") || warpedBody.CompareTag("NomaiShuttleBody"))
		{
			SpawnImmediately(warpedBody, entryData);
			return;
		}
		FluidDetector attachedFluidDetector = warpedBody.GetAttachedFluidDetector();
		ForceDetector attachedForceDetector = warpedBody.GetAttachedForceDetector();
		if (attachedFluidDetector != null && attachedFluidDetector is ConstantFluidDetector)
		{
			(attachedFluidDetector as ConstantFluidDetector).SetDetectableFluid(_fluidVolume);
		}
		if (attachedForceDetector != null && attachedForceDetector is ConstantForceDetector)
		{
			((ConstantForceDetector)attachedForceDetector).ClearAllFields();
		}
		if (warpedBody.CompareTag("DetachedFragment"))
		{
			warpedBody.GetComponentInChildren<DetachableFragment>().ChangeFragmentSector(_whiteHoleSector, _whiteHoleProxyShadowSuperGroup);
		}
		AddToGrowQueue(warpedBody, entryData);
	}

	private void SpawnImmediately(OWRigidbody overrideBody, RelativeLocationData entryData)
	{
		MoveBodyToOverrideExitLocation(overrideBody, entryData);
		bool flag = false;
		if (overrideBody.CompareTag("Player"))
		{
			flag = true;
			MakeRoomForBody(overrideBody);
		}
		else if (overrideBody.CompareTag("Ship"))
		{
			if (Time.time > _lastShipWarpTime + Time.deltaTime)
			{
				_lastShipWarpTime = Time.time;
				bool flag2 = !overrideBody.GetComponent<ShipDamageController>().IsCockpitDetached();
				if (PlayerState.IsInsideShip() || PlayerState.UsingShipComputer() || (flag2 && PlayerState.AtFlightConsole()))
				{
					flag = true;
					MakeRoomForBody(overrideBody);
				}
			}
		}
		else if (overrideBody.CompareTag("ShipCockpit"))
		{
			if (PlayerState.AtFlightConsole())
			{
				flag = true;
				MakeRoomForBody(overrideBody);
				GlobalMessenger.FireEvent("PlayerRepositioned");
			}
		}
		else if (overrideBody.CompareTag("NomaiShuttleBody"))
		{
			if (Time.time > _lastShipWarpTime + Time.deltaTime)
			{
				_lastShipWarpTime = Time.time;
				if (PlayerState.IsInsideShuttle())
				{
					flag = true;
					MakeRoomForBody(overrideBody);
				}
			}
		}
		else if (!overrideBody.CompareTag("Probe"))
		{
			Debug.LogError("Cannot identify warped override body", overrideBody);
			Debug.Break();
		}
		if (flag)
		{
			Locator.GetPlayerAudioController().PlayPlayerSingularityTransit();
			for (int i = 0; i < _airlocksToOpen.Length; i++)
			{
				_airlocksToOpen[i].ResetToOpenState();
			}
		}
	}

	private void MakeRoomForBody(OWRigidbody bodyToAvoid)
	{
		for (int i = 0; i < _growQueue.Count; i++)
		{
			MoveBodyToExitLocation(_growQueue[i], _growQueueLocationData[i]);
			FinishGrowing(_growQueue[i]);
			MoveToAvoidBody(_growQueue[i], bodyToAvoid);
		}
		_growQueue.Clear();
		_growQueueLocationData.Clear();
		if (_growingBody != null)
		{
			FinishGrowing(_growingBody);
			MoveToAvoidBody(_growingBody, bodyToAvoid);
		}
		_growingBody = null;
		for (int num = _ejectedBodyList.Count - 1; num >= 0; num--)
		{
			if (_ejectedBodyList[num] != null)
			{
				MoveToAvoidBody(_ejectedBodyList[num], bodyToAvoid);
			}
			else
			{
				_ejectedBodyList.RemoveAt(num);
			}
		}
	}

	private void MoveToAvoidBody(OWRigidbody body, OWRigidbody bodyToAvoid)
	{
		if (body.GetRigidbody() != null && bodyToAvoid.GetRigidbody() != null)
		{
			if (Vector3.Distance(body.GetPosition(), bodyToAvoid.GetPosition()) < 200f)
			{
				body.GetComponent<DebrisLeash>().MoveByDistance(200f);
				if (_whiteHoleBody.GetRelativeVelocity(body).magnitude < 1f)
				{
					Debug.LogError(body.name + " was moved out of the way before it started moving", body);
					Debug.Break();
				}
			}
		}
		else
		{
			MonoBehaviour.print("WHAT THE ACTUAL FUCK: " + body.name + "   " + bodyToAvoid.name);
		}
	}

	private void AddToGrowQueue(OWRigidbody bodyToGrow, RelativeLocationData entryData)
	{
		if (!bodyToGrow.CompareTag("Probe") && !bodyToGrow.CompareTag("Player") && !bodyToGrow.CompareTag("Ship") && !bodyToGrow.CompareTag("NomaiShuttleBody"))
		{
			bodyToGrow.SetLocalScale(Vector3.one * 0.1f);
			bodyToGrow.SetPosition(base.transform.position);
			bodyToGrow.SetVelocity(_whiteHoleBody.GetVelocity());
			if (!_growQueue.Contains(bodyToGrow))
			{
				_growQueue.Add(bodyToGrow);
				_growQueueLocationData.Add(entryData);
			}
		}
	}

	private void FixedUpdate()
	{
		if (_growingBody != null)
		{
			_growingBody.SetLocalScale(_growingBody.GetLocalScale() * 1.05f);
			if (_growingBody.GetLocalScale().x >= 1f)
			{
				FinishGrowing(_growingBody);
				_growingBody = null;
			}
		}
		else if (_growQueue.Count > 0)
		{
			if (_growQueue[0] == null)
			{
				_growQueue.RemoveAt(0);
				_growQueueLocationData.RemoveAt(0);
			}
			else if (Time.time > _nextGrowCheckTime)
			{
				_nextGrowCheckTime = Time.time + 1f;
				_growingBody = _growQueue[0];
				MoveBodyToExitLocation(_growingBody, _growQueueLocationData[0]);
				_growQueue.RemoveAt(0);
				_growQueueLocationData.RemoveAt(0);
			}
		}
	}

	private void MoveBodyToOverrideExitLocation(OWRigidbody body, RelativeLocationData blackHoleEntryLocation)
	{
		Vector3 up = base.transform.up;
		body.SetPosition(base.transform.position + up * _radius);
		Vector3 fromDirection = (body.CompareTag("Player") ? Locator.GetPlayerCamera().transform.forward : body.transform.forward);
		body.SetRotation(Quaternion.FromToRotation(fromDirection, up) * body.transform.rotation);
		body.SetVelocity(_whiteHoleBody.GetVelocity());
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
	}

	private void MoveBodyToExitLocation(OWRigidbody body, RelativeLocationData blackHoleEntryLocation)
	{
		body.SetPosition(base.transform.TransformPoint(-blackHoleEntryLocation.localPosition));
		body.SetRotation(base.transform.rotation * blackHoleEntryLocation.localRotation);
		body.SetVelocity(_whiteHoleBody.GetVelocity());
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
	}

	private void FinishGrowing(OWRigidbody body)
	{
		body.SetLocalScale(Vector3.one);
		body.SetVelocity((body.GetPosition() - _whiteHoleBody.GetPosition()).normalized * _fluidVolume.GetMassiveFlowSpeed());
		if (body.CompareTag("DetachedFragment"))
		{
			body.GetComponentInChildren<DetachableFragment>().EndWarpScaling();
		}
		body.gameObject.AddComponent<DebrisLeash>().Init(_whiteHoleBody, Random.Range(_debrisDistMin, _debrisDistMax));
		_ejectedBodyList.Add(body);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(base.transform.position, _radius);
		OWGizmos.DrawWireCircle(base.transform.position, Vector3.up, _debrisDistMin);
		OWGizmos.DrawBillboardedWireCircle(base.transform.position, _debrisDistMin);
		OWGizmos.DrawWireCircle(base.transform.position, Vector3.up, _debrisDistMax);
		OWGizmos.DrawBillboardedWireCircle(base.transform.position, _debrisDistMax);
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(base.transform.position + base.transform.forward * _radius, 0.5f);
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.forward * 200f);
	}
}
