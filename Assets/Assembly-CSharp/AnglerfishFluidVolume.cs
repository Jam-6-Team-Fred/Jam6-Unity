using UnityEngine;

public class AnglerfishFluidVolume : FluidVolume
{
	public delegate void CatchEvent(OWRigidbody caughtBody);

	[SerializeField]
	private Vector3 _attractionPoint = Vector3.zero;

	[SerializeField]
	private float _attractionRadius = 1f;

	[SerializeField]
	private float _flowSpeed = 1f;

	public event CatchEvent OnCaughtObject;

	protected override void Reset()
	{
		base.Reset();
		_density = 10000f;
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		base.OnEffectVolumeEnter(hitObj);
		if (this.OnCaughtObject != null)
		{
			this.OnCaughtObject(hitObj.GetAttachedOWRigidbody());
		}
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 vector = base.transform.TransformPoint(_attractionPoint);
		float num = Mathf.Clamp01(Vector3.Distance(worldPosition, vector) / _attractionRadius);
		return _attachedBody.GetPointVelocity(worldPosition) + Vector3.Normalize(vector - worldPosition) * _flowSpeed * num;
	}

	protected void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix * Matrix4x4.Scale(base.transform.lossyScale).inverse;
		Gizmos.color = new Color(1f, 0.5f, 0f);
		Gizmos.DrawWireSphere(_attractionPoint, _attractionRadius);
	}
}
