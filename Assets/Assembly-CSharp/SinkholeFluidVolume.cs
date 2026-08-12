using UnityEngine;

public class SinkholeFluidVolume : FluidVolume
{
	public delegate void SinkholeEvent();

	[SerializeField]
	private Vector3 _vortexCenter = Vector3.zero;

	[SerializeField]
	private Vector3 _vortexDirection = Vector3.down;

	[SerializeField]
	private float _vortexSuctionRadius = 1f;

	[Space(10f)]
	[SerializeField]
	private float _downwardFlowSpeed = 0.5f;

	[SerializeField]
	private float _inwardFlowSpeed = 1.5f;

	private bool _activated;

	public event SinkholeEvent OnSinkholeActivated;

	protected override void Reset()
	{
		base.Reset();
		_density = 10000f;
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		base.OnEffectVolumeEnter(hitObj);
		if (!_activated && this.OnSinkholeActivated != null)
		{
			_activated = true;
			this.OnSinkholeActivated();
		}
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 vector = base.transform.TransformPoint(_vortexCenter);
		Vector3 vector2 = base.transform.TransformDirection(_vortexDirection);
		float num = Mathf.Clamp01(Vector3.Distance(worldPosition, vector) / _vortexSuctionRadius);
		Vector3 vector3 = OWMath.ClosestPointOnRay(worldPosition, new Ray(vector, vector2));
		return _attachedBody.GetPointVelocity(worldPosition) + Vector3.Normalize(vector3 - worldPosition) * _inwardFlowSpeed * num + Vector3.Normalize(vector2) * _downwardFlowSpeed;
	}

	protected void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix * Matrix4x4.Scale(base.transform.lossyScale).inverse;
		Gizmos.color = new Color(1f, 0.5f, 0f);
		Gizmos.DrawWireSphere(_vortexCenter, _vortexSuctionRadius);
		Gizmos.DrawRay(_vortexCenter, _vortexDirection * _vortexSuctionRadius * 2f);
	}
}
