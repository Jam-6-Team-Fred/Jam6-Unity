using UnityEngine;

public abstract class FogWarpVolume : MonoBehaviour
{
	public delegate void FogWarpEvent(FogWarpDetector detector);

	[SerializeField]
	private Sector _sector;

	[Space]
	[SerializeField]
	[ColorUsage(false, true)]
	private Color _fogColor = Color.gray;

	protected OWRigidbody _attachedBody;

	public event FogWarpEvent OnWarpDetector;

	private void Awake()
	{
		_attachedBody = this.GetAttachedOWRigidbody();
		_sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
		_sector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
		OnAwake();
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnDestroy()
	{
		_sector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
		_sector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
	}

	public virtual float GetFogThickness()
	{
		return 50f;
	}

	public virtual bool IsProbeOnly()
	{
		return false;
	}

	public virtual bool UseFastFogFade()
	{
		return false;
	}

	public abstract bool IsOuterWarpVolume();

	public abstract void PropagateCanvasMarkerOutwards(CanvasMarker marker, bool addMarker, float warpDist = 0f);

	public abstract float CheckWarpProximity(FogWarpDetector detector);

	protected abstract void RepositionWarpedBody(OWRigidbody body, Vector3 localRelVelocity, Vector3 localPos, Quaternion localRot);

	public Sector GetSector()
	{
		return _sector;
	}

	public Color GetFogColor()
	{
		return _fogColor;
	}

	protected void ReceiveWarpedDetector(FogWarpDetector detector, Vector3 localRelVelocity, Vector3 localPos, Quaternion localRot)
	{
		OWRigidbody oWRigidbody = detector.GetOWRigidbody();
		int num;
		if (detector.CompareName(FogWarpDetector.Name.Player) && PlayerState.IsAttached())
		{
			num = ((!PlayerState.IsInsideShip()) ? 1 : 0);
			if (num != 0)
			{
				oWRigidbody = detector.GetOWRigidbody().transform.parent.GetComponentInParent<OWRigidbody>();
				MonoBehaviour.print("body to reposition: " + oWRigidbody.name);
			}
		}
		else
		{
			num = 0;
		}
		RepositionWarpedBody(oWRigidbody, localRelVelocity, localPos, localRot);
		if (num != 0)
		{
			GlobalMessenger.FireEvent("PlayerRepositioned");
		}
		if (detector.CompareName(FogWarpDetector.Name.Ship) && PlayerState.IsInsideShip())
		{
			_sector.GetTriggerVolume().AddObjectToVolume(Locator.GetPlayerDetector());
			_sector.GetTriggerVolume().AddObjectToVolume(Locator.GetPlayerCameraDetector());
		}
		_sector.GetTriggerVolume().AddObjectToVolume(detector.gameObject);
		detector.OnFogWarp();
	}

	protected void WarpDetector(FogWarpDetector detector, FogWarpVolume linkedWarpVolume)
	{
		bool flag = detector.CompareName(FogWarpDetector.Name.Player);
		bool flag2 = detector.CompareName(FogWarpDetector.Name.Ship);
		if (!flag || !PlayerState.IsInsideShip())
		{
			OWRigidbody oWRigidbody = detector.GetOWRigidbody();
			if (flag && PlayerState.IsAttached())
			{
				oWRigidbody = detector.GetOWRigidbody().transform.parent.GetComponentInParent<OWRigidbody>();
				MonoBehaviour.print("body to warp: " + oWRigidbody.name);
			}
			Vector3 localRelVelocity = base.transform.InverseTransformDirection(oWRigidbody.GetVelocity() - _attachedBody.GetVelocity());
			Vector3 localPos = base.transform.InverseTransformPoint(oWRigidbody.transform.position);
			Quaternion localRot = Quaternion.Inverse(base.transform.rotation) * oWRigidbody.transform.rotation;
			if (flag2 && PlayerState.IsInsideShip())
			{
				_sector.GetTriggerVolume().RemoveObjectFromVolume(Locator.GetPlayerDetector());
				_sector.GetTriggerVolume().RemoveObjectFromVolume(Locator.GetPlayerCameraDetector());
			}
			if (flag || (flag2 && PlayerState.IsInsideShip()))
			{
				GlobalMessenger.FireEvent("PlayerFogWarp");
			}
			_sector.GetTriggerVolume().RemoveObjectFromVolume(detector.gameObject);
			linkedWarpVolume.ReceiveWarpedDetector(detector, localRelVelocity, localPos, localRot);
			if (this.OnWarpDetector != null)
			{
				this.OnWarpDetector(detector);
			}
		}
	}

	private void OnOccupantEnterSector(SectorDetector detector)
	{
		FogWarpDetector component = detector.GetComponent<FogWarpDetector>();
		if (component != null)
		{
			component.TrackFogWarpVolume(this);
		}
	}

	private void OnOccupantExitSector(SectorDetector detector)
	{
		FogWarpDetector component = detector.GetComponent<FogWarpDetector>();
		if (component != null)
		{
			component.UntrackFogWarpVolume(this);
		}
	}
}
