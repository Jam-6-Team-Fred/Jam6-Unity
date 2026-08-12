using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class TractorBeamFluid : FluidVolume
{
	[SerializeField]
	private float _radius = 2f;

	[SerializeField]
	private float _height = 20f;

	[SerializeField]
	private float _verticalSpeed = 10f;

	[SerializeField]
	private float _reverseSpeed = -10f;

	[SerializeField]
	private float _inwardSpeed = 5f;

	[SerializeField]
	private float _outwardSpeed = 3f;

	[SerializeField]
	private float _exitLength;

	[SerializeField]
	private float _exitOffset;

	[SerializeField]
	private bool _fadeBeamIfPlayerInside;

	[SerializeField]
	private TractorBeamParticleController _ringParticleController;

	private bool _reversed;

	private float _playerEntryTime;

	private JetpackThrusterModel _playerThrusterModel;

	private float _playerBoostTime;

	private float _shipEntryTime;

	private ThrusterModel _shipThrusterModel;

	public float GetHeight()
	{
		return _height;
	}

	public float GetRadius()
	{
		return _radius;
	}

	public float GetVerticalSpeed()
	{
		if (!_reversed)
		{
			return _verticalSpeed;
		}
		return _reverseSpeed;
	}

	public void SetFluidReversed(bool reversed)
	{
		if (reversed != _reversed)
		{
			_reversed = reversed;
			OnValidate();
		}
	}

	public bool IsFluidReversed()
	{
		return _reversed;
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 vector = worldPosition - base.transform.position;
		Vector3 vector2 = Vector3.Project(vector, base.transform.up);
		Vector3 vector3 = vector - vector2;
		float magnitude = vector2.magnitude;
		float num = ((_exitLength > 0f) ? Mathf.Clamp01((magnitude - _exitOffset) / _exitLength) : 1f);
		Vector3 vector4 = -base.transform.up * GetVerticalSpeed() * num + _attachedBody.GetPointVelocity(worldPosition);
		if (magnitude < _exitLength + _exitOffset && magnitude > _exitOffset)
		{
			return vector4 + base.transform.forward * _outwardSpeed;
		}
		bool flag = detector.CompareName(Detector.Name.Player);
		bool flag2 = detector.CompareName(Detector.Name.Ship);
		if (flag || flag2)
		{
			Vector3 zero = Vector3.zero;
			if (flag && Locator.GetPlayerController().IsGrounded())
			{
				return vector4;
			}
			if (flag && _playerThrusterModel.IsBoosterFiring())
			{
				_playerBoostTime = Time.time;
			}
			if (flag && Time.time - _playerEntryTime > 1f)
			{
				zero += _playerThrusterModel.GetWorldAcceleration();
			}
			if (flag2 && Time.time - _shipEntryTime > 1f)
			{
				zero += _shipThrusterModel.GetWorldAcceleration();
			}
			float num2 = Vector3.Angle(base.transform.up, zero);
			if (num2 > 90f)
			{
				num2 = 180f - num2;
			}
			Vector3 onNormal = Vector3.ProjectOnPlane(zero, base.transform.up);
			if (onNormal.magnitude > 1f && num2 > 45f)
			{
				Vector3 vector5 = vector3 - Vector3.Project(vector3, onNormal);
				vector5 *= _inwardSpeed;
				Vector3 vector6 = onNormal.normalized * Mathf.Min(onNormal.magnitude, 5f);
				return vector4 - vector5 + vector6;
			}
			if (flag && Time.time - _playerBoostTime < 1f)
			{
				Vector3 up = Locator.GetPlayerTransform().up;
				float num3 = Vector3.Angle(base.transform.up, up);
				if (num3 > 45f && num3 < 135f && Vector3.Dot(up, vector3) > 0f)
				{
					vector3 -= Vector3.Project(vector3, up);
				}
			}
		}
		return vector4 - vector3 * _inwardSpeed;
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		if (!(hitObj.GetComponent<SectorDetector>() != null))
		{
			return;
		}
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerEntryTime = Time.time;
			_playerThrusterModel = Locator.GetPlayerTransform().GetComponent<JetpackThrusterModel>();
			if (_fadeBeamIfPlayerInside && _ringParticleController != null)
			{
				_ringParticleController.OnPlayerEnterBeam();
			}
		}
		else if (hitObj.CompareTag("ShipDetector"))
		{
			_shipEntryTime = Time.time;
			_shipThrusterModel = Locator.GetShipTransform().GetComponent<ThrusterModel>();
		}
		base.OnEffectVolumeEnter(hitObj);
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		if (!(hitObj.GetComponent<SectorDetector>() != null))
		{
			return;
		}
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerThrusterModel = null;
			if (_fadeBeamIfPlayerInside && _ringParticleController != null)
			{
				_ringParticleController.OnPlayerExitBeam();
			}
		}
		else if (hitObj.CompareTag("ShipDetector"))
		{
			_shipThrusterModel = null;
		}
		base.OnEffectVolumeExit(hitObj);
	}

	private void OnValidate()
	{
		if (base.transform.localPosition.x != 0f || base.transform.localPosition.z != 0f)
		{
			base.transform.localPosition = new Vector3(0f, base.transform.localPosition.y, 0f);
		}
		if (base.transform.localRotation != Quaternion.identity)
		{
			base.transform.localRotation = Quaternion.identity;
		}
		if (base.transform.localScale != Vector3.one)
		{
			base.transform.localScale = Vector3.one;
		}
		float num = Mathf.Max(0f, _radius);
		float num2 = Mathf.Max(num, _height);
		float num3 = Mathf.Max(num, num2 * 0.5f);
		CapsuleCollider component = GetComponent<CapsuleCollider>();
		if (_radius != num || _height != num2 || component.radius != num || component.height != num2 || component.center.y != num3)
		{
			_radius = num;
			_height = num2;
			component.radius = num;
			component.height = num2;
			component.center = new Vector3(component.center.x, num3, component.center.z);
		}
		TractorBeamParticleController[] componentsInChildren = GetComponentsInChildren<TractorBeamParticleController>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].OnValidate();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 vector = base.transform.position + base.transform.up * _exitOffset;
		Vector3 vector2 = vector + _exitLength * base.transform.up;
		Gizmos.color = new ColorHSV(148f, 1f, 1f).ToColorRGB();
		OWGizmos.DrawWireCircle(vector, base.transform.up, _radius);
		OWGizmos.DrawWireCircle(vector2, base.transform.up, _radius);
		Gizmos.color = new ColorHSV(148f, 1f, 1f).ToColorRGB();
		Gizmos.DrawLine(vector, vector2);
	}
}
