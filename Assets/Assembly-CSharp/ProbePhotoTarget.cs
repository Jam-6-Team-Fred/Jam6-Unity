using UnityEngine;

public class ProbePhotoTarget : VisibilityObject
{
	public delegate void ProbePhotoEvent(ProbePhotoTarget target, float score);

	[SerializeField]
	private float _maxPhotoDistance = 200f;

	[SerializeField]
	private float _baseScore = 10f;

	[SerializeField]
	private string _name = "";

	[SerializeField]
	private float _raycastOffset = 10f;

	public event ProbePhotoEvent OnPhotographedByProbe;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger<ProbeCamera>.RemoveListener("ProbeSnapshot", OnProbeSnapshot);
	}

	public string GetName()
	{
		return _name;
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Probe)
		{
			GlobalMessenger<ProbeCamera>.AddListener("ProbeSnapshot", OnProbeSnapshot);
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Probe)
		{
			GlobalMessenger<ProbeCamera>.RemoveListener("ProbeSnapshot", OnProbeSnapshot);
		}
	}

	private void OnProbeSnapshot(ProbeCamera camera)
	{
		if (!CheckVisibilityFromProbe(camera.GetOWCamera()))
		{
			return;
		}
		Vector3 vector = base.transform.position - camera.transform.position;
		float magnitude = vector.magnitude;
		if (!(magnitude > _maxPhotoDistance))
		{
			if (Physics.Raycast(camera.transform.position, vector.normalized, magnitude - _raycastOffset, OWLayerMask.physicalMask))
			{
				MonoBehaviour.print("photo blocked");
			}
			else if (this.OnPhotographedByProbe != null)
			{
				float baseScore = _baseScore;
				this.OnPhotographedByProbe(this, baseScore);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, _raycastOffset);
	}
}
