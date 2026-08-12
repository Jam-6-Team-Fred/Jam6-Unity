using UnityEngine;

public class AirlockFluidVolume : FluidVolume
{
	private SphereCollider _collider;

	private SphereShape _sphereShape;

	private float _sphereRadius;

	protected override void Awake()
	{
		base.Awake();
		_sphereShape = GetComponent<SphereShape>();
		_sphereRadius = -1f;
		if (_sphereShape != null)
		{
			_sphereRadius = _sphereShape.radius;
		}
		else
		{
			_collider = GetComponent<SphereCollider>();
			if (_collider != null)
			{
				_sphereRadius = _collider.radius;
			}
		}
		if (_sphereRadius == -1f)
		{
			Debug.LogError("No sphere found!", this);
			_sphereRadius = 0f;
		}
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		if (detector.CompareTag("PlayerDetector"))
		{
			if (Vector3.Angle(detector.transform.forward, base.transform.right) < 60f)
			{
				Vector3 vector = worldPosition - base.transform.position;
				float magnitude = vector.magnitude;
				float num = Mathf.InverseLerp(0f, 0.5f, magnitude);
				_density = 1000f + Mathf.InverseLerp(_sphereRadius, 0f, magnitude) * 5000f;
				return _attachedBody.GetPointVelocity(worldPosition) - vector.normalized * 2f * num;
			}
			_density = 0f;
			return _attachedBody.GetPointVelocity(worldPosition);
		}
		Debug.LogError("Not the player!!!", this);
		Debug.Break();
		return Vector3.zero;
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.OnEffectVolumeEnter(hitObj);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.OnEffectVolumeExit(hitObj);
		}
	}
}
