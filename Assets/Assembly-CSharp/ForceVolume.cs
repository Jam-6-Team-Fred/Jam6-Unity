using UnityEngine;

public abstract class ForceVolume : PriorityVolume
{
	[SerializeField]
	private int _alignmentPriority;

	[SerializeField]
	private bool _inheritable;

	public abstract bool GetAffectsAlignment(OWRigidbody targetBody);

	public abstract Vector3 CalculateForceAccelerationAtPoint(Vector3 worldPos);

	public virtual bool GetPlayGravityCrystalAudio()
	{
		return false;
	}

	public virtual Vector3 CalculateForceAccelerationOnBody(OWRigidbody targetBody)
	{
		return CalculateForceAccelerationAtPoint(targetBody.GetWorldCenterOfMass());
	}

	public int GetAlignmentPriority()
	{
		return _alignmentPriority;
	}

	public bool IsInheritable()
	{
		return _inheritable;
	}

	public ForceDetector GetAttachedForceDetector()
	{
		return _attachedBody.GetAttachedForceDetector();
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		ForceDetector component = hitObj.GetComponent<ForceDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		ForceDetector component = hitObj.GetComponent<ForceDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}
}
