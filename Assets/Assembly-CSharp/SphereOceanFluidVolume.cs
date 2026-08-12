using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OWOceanCollider))]
[RequireComponent(typeof(WaveHeightCalculator))]
public class SphereOceanFluidVolume : RadialFluidVolume
{
	private class SplashData
	{
		public Transform splashTransform;

		public float altitude;

		public SplashData(Transform splashTransform, Vector3 fluidPosition)
		{
			this.splashTransform = splashTransform;
			altitude = Vector3.Distance(fluidPosition, splashTransform.position);
		}
	}

	[SerializeField]
	private float _surfaceCurrentSpeed = 10f;

	[SerializeField]
	private float _equatorRepelAngle = 20f;

	[SerializeField]
	private float _equatorRepelSpeed = 1f;

	[SerializeField]
	private float _barrierUpperRadius = 460f;

	[SerializeField]
	private float _barrierLowerRadius = 440f;

	[SerializeField]
	private float _barrierRepelSpeed = 50f;

	private WaveHeightCalculator _waveHeightCalculator;

	private List<SplashData> _splashData = new List<SplashData>(16);

	protected override void OnValidate()
	{
		OWOceanCollider component = GetComponent<OWOceanCollider>();
		if (component != null)
		{
			component.SetWavelessRadius(_radius);
		}
	}

	protected override void Awake()
	{
		_waveHeightCalculator = base.gameObject.GetRequiredComponent<WaveHeightCalculator>();
		base.Awake();
	}

	public override bool CheckTriggerProbeDragIncrease(FluidDetector probeDetector)
	{
		return false;
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		Vector3 displacement = worldPosition - base.transform.position;
		Vector3 oceanCurrentVelocity = GetOceanCurrentVelocity(displacement, detector);
		return pointVelocity + oceanCurrentVelocity;
	}

	public Vector3 GetPointBarrierVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 vector = worldPosition - base.transform.position;
		float magnitude = vector.magnitude;
		float num = Mathf.InverseLerp(_barrierUpperRadius, _barrierLowerRadius, magnitude);
		HandleBarrierRumble(detector, magnitude, num);
		return vector.normalized * _barrierRepelSpeed * num;
	}

	public bool IsPointInBarrierZone(Vector3 worldPos)
	{
		float magnitude = (worldPos - base.transform.position).magnitude;
		if (magnitude > _barrierLowerRadius)
		{
			return magnitude < _barrierUpperRadius;
		}
		return false;
	}

	public override float GetFractionSubmerged(FluidDetector detector)
	{
		Vector3 vector = detector.transform.position - base.transform.position;
		float fluidRadius = (detector.GetBuoyancyData().checkAgainstWaves ? _waveHeightCalculator.GetOceanRadius(detector.transform.position) : _radius);
		return detector.GetBuoyancyData().CalculateSubmergedFraction(vector.magnitude, fluidRadius);
	}

	public override void RegisterSplashTransform(Transform splashTransform)
	{
		_splashData.Add(new SplashData(splashTransform, base.transform.position));
	}

	private void FixedUpdate()
	{
		for (int num = _splashData.Count - 1; num >= 0; num--)
		{
			if (_splashData[num].splashTransform == null)
			{
				_splashData.RemoveAt(num);
			}
			else
			{
				Vector3 position = _splashData[num].splashTransform.position;
				Vector3 oceanCurrentVelocity = GetOceanCurrentVelocity(position - base.transform.position, null);
				Vector3 vector = position + oceanCurrentVelocity * Time.fixedDeltaTime;
				Vector3 vector2 = (vector - base.transform.position).normalized * _splashData[num].altitude;
				vector = base.transform.position + vector2;
				_splashData[num].splashTransform.position = vector;
				_splashData[num].splashTransform.rotation = Quaternion.FromToRotation(_splashData[num].splashTransform.up, vector2) * _splashData[num].splashTransform.rotation;
			}
		}
	}

	private Vector3 GetOceanCurrentVelocity(Vector3 displacement, FluidDetector detector)
	{
		Vector3 zero = Vector3.zero;
		float magnitude = displacement.magnitude;
		if (magnitude > _barrierLowerRadius)
		{
			float num = Mathf.InverseLerp(_barrierUpperRadius, _barrierLowerRadius, magnitude);
			Vector3 vector = displacement.normalized * _barrierRepelSpeed * num;
			zero += vector;
			HandleBarrierRumble(detector, magnitude, num);
			float num2 = Vector3.Angle(displacement, base.transform.up);
			float num3 = ((num2 < 90f) ? 1f : (-1f));
			Vector3 vector2 = num3 * Vector3.Cross(displacement, base.transform.up).normalized;
			float num4 = ((num2 < 90f) ? num2 : (180f - num2));
			if (num4 > 20f)
			{
				float num5 = ((90f - num4 < _equatorRepelAngle) ? 1f : 0f);
				float num6 = Mathf.Sin((float)Math.PI / 180f * num4);
				float num7 = _surfaceCurrentSpeed * num6 + num * _barrierRepelSpeed;
				zero += num7 * vector2 + num3 * num5 * _equatorRepelSpeed * base.transform.up;
			}
		}
		return zero;
	}

	private void HandleBarrierRumble(FluidDetector detector, float altitude, float barrierDepthFraction)
	{
		if (detector != null)
		{
			if (detector.AffectsRumble())
			{
				RumbleManager.AddFluidRumble(Type.WATER, barrierDepthFraction);
			}
			if (detector.GetType() == typeof(ProbeFluidDetector) && altitude < _barrierUpperRadius)
			{
				((ProbeFluidDetector)detector).SetOceanBarrierDrag();
			}
		}
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position, _barrierLowerRadius);
		Gizmos.DrawWireSphere(base.transform.position, _barrierUpperRadius);
	}
}
