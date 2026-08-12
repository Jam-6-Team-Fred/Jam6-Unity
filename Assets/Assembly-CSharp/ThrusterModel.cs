using System;
using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class ThrusterModel : MonoBehaviour
{
	public delegate void StartTranslationalThrustEvent();

	public delegate void StopTranslationalThrustEvent();

	public delegate void FireRotationalThrusterEvent();

	[SerializeField]
	protected float _maxTranslationalThrust = 10f;

	[SerializeField]
	protected float _maxRotationalThrust = 5f;

	[SerializeField]
	protected float _angularDrag = 0.96f;

	[SerializeField]
	protected bool _usePhysicsToRotate = true;

	protected Vector3 _translationalInput = Vector3.zero;

	protected Vector3 _rotationalInput = Vector3.zero;

	protected Vector3 _localAcceleration = Vector3.zero;

	protected Vector3 _localAngularAcceleration = Vector3.zero;

	protected Vector3 _manualAngularVelocity;

	protected bool _updateManualAngularVelocity = true;

	private float _adjustedManualDrag;

	protected bool _isTranslationalFiring;

	protected bool _wasTranslationalFiring;

	protected bool _isRotationalFiring;

	protected bool _wasRotationalFiring;

	protected OWRigidbody _owRigidbody;

	public event StartTranslationalThrustEvent OnStartTranslationalThrust;

	public event StopTranslationalThrustEvent OnStopTranslationalThrust;

	public event FireRotationalThrusterEvent OnFireRotationalThruster;

	protected virtual void Awake()
	{
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
		if (_usePhysicsToRotate)
		{
			_owRigidbody.GetRigidbody().angularDrag = _angularDrag;
			_owRigidbody.UnfreezeRotation();
		}
		else
		{
			_manualAngularVelocity = Vector3.zero;
			_owRigidbody.FreezeRotation();
		}
		AlignWithForce component = GetComponent<AlignWithForce>();
		if (component != null)
		{
			component.OnInitAlignment += OnInitAlignment;
			component.OnBreakAlignment += OnBreakAlignment;
		}
	}

	private void Start()
	{
		if (!_usePhysicsToRotate)
		{
			_adjustedManualDrag = Mathf.Pow(_angularDrag, OWTime.GetFixedTimestep() / 0.01666666f);
			_updateManualAngularVelocity = true;
		}
	}

	private void OnDisable()
	{
		_manualAngularVelocity = Vector3.zero;
	}

	protected virtual void OnDestroy()
	{
		AlignWithForce component = GetComponent<AlignWithForce>();
		if (component != null)
		{
			component.OnInitAlignment -= OnInitAlignment;
			component.OnBreakAlignment -= OnBreakAlignment;
		}
	}

	public virtual bool IsThrusterBankEnabled(ThrusterBank thrusterBank)
	{
		return true;
	}

	public virtual void ActivateBoost()
	{
	}

	public virtual void DeactivateBoost()
	{
	}

	public Vector3 GetAngularVelocity()
	{
		if (!_usePhysicsToRotate)
		{
			return _manualAngularVelocity;
		}
		return _owRigidbody.GetAngularVelocity();
	}

	public OWRigidbody GetAttachedOWRigidbody()
	{
		return _owRigidbody;
	}

	public float GetThrustFraction()
	{
		return _localAcceleration.magnitude / _maxTranslationalThrust;
	}

	public float GetRotationalThrustFraction()
	{
		return _localAngularAcceleration.magnitude / _maxRotationalThrust;
	}

	public float GetMaxTranslationalThrust()
	{
		return _maxTranslationalThrust;
	}

	public float GetMaxRotationalThrust()
	{
		return _maxRotationalThrust;
	}

	public bool IsTranslationalThrusterFiring()
	{
		return _isTranslationalFiring;
	}

	public void AddTranslationalInput(Vector3 input)
	{
		_translationalInput += input;
	}

	public void AddRotationalInput(Vector3 input)
	{
		_rotationalInput += input;
	}

	public Vector3 GetLocalAcceleration()
	{
		return _localAcceleration;
	}

	public Vector3 GetWorldAcceleration()
	{
		return base.transform.TransformDirection(_localAcceleration);
	}

	protected virtual void OnInitAlignment()
	{
		_manualAngularVelocity = Vector3.zero;
	}

	protected virtual void OnBreakAlignment()
	{
		_manualAngularVelocity = Vector3.zero;
	}

	protected virtual void FireTranslationalThrusters()
	{
		_localAcceleration = _translationalInput * _maxTranslationalThrust;
		if (_localAcceleration.sqrMagnitude > 0f)
		{
			_localAcceleration.x = Mathf.Clamp(_localAcceleration.x, 0f - _maxTranslationalThrust, _maxTranslationalThrust);
			_localAcceleration.y = Mathf.Clamp(_localAcceleration.y, 0f - _maxTranslationalThrust, _maxTranslationalThrust);
			_localAcceleration.z = Mathf.Clamp(_localAcceleration.z, 0f - _maxTranslationalThrust, _maxTranslationalThrust);
			_isTranslationalFiring = true;
			_owRigidbody.AddLocalAcceleration(_localAcceleration);
		}
		else
		{
			_isTranslationalFiring = false;
		}
	}

	protected void FireRotationalThrusters()
	{
		_localAngularAcceleration = _rotationalInput * _maxRotationalThrust;
		if (_localAngularAcceleration.sqrMagnitude > 0f)
		{
			float num = (OWInput.UsingGamepad() ? 1f : 2f);
			float num2 = _maxRotationalThrust * num;
			_localAngularAcceleration.x = Mathf.Clamp(_localAngularAcceleration.x, 0f - num2, num2);
			_localAngularAcceleration.y = Mathf.Clamp(_localAngularAcceleration.y, 0f - num2, num2);
			_localAngularAcceleration.z = Mathf.Clamp(_localAngularAcceleration.z, 0f - num2, num2);
			_isRotationalFiring = true;
			if (_usePhysicsToRotate)
			{
				_owRigidbody.AddLocalAngularAcceleration(_localAngularAcceleration);
			}
			else
			{
				_manualAngularVelocity += base.transform.TransformDirection(_localAngularAcceleration * Time.fixedDeltaTime);
			}
		}
		else
		{
			_isRotationalFiring = false;
		}
	}

	private void UpdateManualAngularVelocity()
	{
		_manualAngularVelocity *= _adjustedManualDrag;
		Quaternion deltaRotation = Quaternion.AngleAxis(_manualAngularVelocity.magnitude * 180f / (float)Math.PI * Time.fixedDeltaTime, _manualAngularVelocity.normalized);
		_owRigidbody.AddRotation(deltaRotation);
	}

	private void Update()
	{
		if (_isTranslationalFiring && !_wasTranslationalFiring)
		{
			if (this.OnStartTranslationalThrust != null)
			{
				this.OnStartTranslationalThrust();
			}
		}
		else if (_wasTranslationalFiring && !_isTranslationalFiring && this.OnStopTranslationalThrust != null)
		{
			this.OnStopTranslationalThrust();
		}
		if (_isRotationalFiring && !_wasRotationalFiring && this.OnFireRotationalThruster != null)
		{
			this.OnFireRotationalThruster();
		}
		_wasTranslationalFiring = _isTranslationalFiring;
		_wasRotationalFiring = _isRotationalFiring;
	}

	private void FixedUpdate()
	{
		FireTranslationalThrusters();
		FireRotationalThrusters();
		if (!_usePhysicsToRotate && _updateManualAngularVelocity)
		{
			UpdateManualAngularVelocity();
		}
		_translationalInput = Vector3.zero;
		_rotationalInput = Vector3.zero;
	}
}
