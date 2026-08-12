using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public abstract class AlignWithDirection : MonoBehaviour
{
	private enum InterpolationMode
	{
		None = 0,
		Exponential = 1,
		Linear = 2,
		InverseExponential = 3
	}

	public delegate void InitAlignmentEvent();

	public delegate void BreakAlignmentEvent();

	[SerializeField]
	protected Vector3 _localAlignmentAxis = new Vector3(0f, -1f, 0f);

	protected OWRigidbody _owRigidbody;

	protected Vector3 _currentDirection;

	protected Vector3 _alignmentDirection;

	protected float _degreesToTarget;

	protected float _adjustedSlerpRate;

	protected bool _doAlignment;

	private bool _allowAlignment;

	private bool _lastAllowAlignment;

	[SerializeField]
	private InterpolationMode _interpolationMode;

	[SerializeField]
	private float _interpolationRate = 100f;

	[SerializeField]
	private bool _usePhysicsToRotate;

	public event InitAlignmentEvent OnInitAlignment;

	public event BreakAlignmentEvent OnBreakAlignment;

	protected virtual void Awake()
	{
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
	}

	private void OnEnable()
	{
		_allowAlignment = false;
		_lastAllowAlignment = false;
		FixedUpdateManager.Register(this);
	}

	protected virtual void OnDisable()
	{
		FixedUpdateManager.Unregister(this);
		if (_doAlignment)
		{
			BreakAlignment();
		}
	}

	protected virtual void Start()
	{
		if (_usePhysicsToRotate)
		{
			_owRigidbody.UnfreezeRotation();
		}
		else
		{
			_owRigidbody.FreezeRotation();
		}
	}

	public void SetUsePhysicsToRotate(bool shouldUsePhysics)
	{
		if (_usePhysicsToRotate != shouldUsePhysics)
		{
			_usePhysicsToRotate = shouldUsePhysics;
			if (_usePhysicsToRotate)
			{
				_owRigidbody.UnfreezeRotation();
			}
			else
			{
				_owRigidbody.FreezeRotation();
			}
		}
	}

	public void SetLocalAlignmentAxis(Vector3 newLocalAxis)
	{
		_localAlignmentAxis = newLocalAxis;
	}

	protected virtual void InitAlignment()
	{
		if (this.OnInitAlignment != null)
		{
			this.OnInitAlignment();
		}
		_doAlignment = true;
	}

	protected virtual void BreakAlignment()
	{
		if (this.OnBreakAlignment != null)
		{
			this.OnBreakAlignment();
		}
		_doAlignment = false;
	}

	protected abstract Vector3 GetAlignmentDirection();

	protected abstract bool CheckAlignmentRequirements();

	public virtual void ManagedFixedUpdate()
	{
		UpdateAlignment();
		if (_doAlignment)
		{
			_currentDirection = base.transform.TransformDirection(_localAlignmentAxis);
			_alignmentDirection = GetAlignmentDirection();
			_degreesToTarget = Vector3.Angle(_currentDirection, _alignmentDirection);
			if (_degreesToTarget == 0f)
			{
				_degreesToTarget = 0.0001f;
			}
			switch (_interpolationMode)
			{
			case InterpolationMode.Exponential:
				_adjustedSlerpRate = _interpolationRate * Time.fixedDeltaTime;
				break;
			case InterpolationMode.Linear:
				_adjustedSlerpRate = _interpolationRate / _degreesToTarget * Time.fixedDeltaTime;
				break;
			case InterpolationMode.InverseExponential:
				_adjustedSlerpRate = _interpolationRate / Mathf.Pow(_degreesToTarget, 2f) * Time.fixedDeltaTime;
				break;
			default:
				_adjustedSlerpRate = 1f;
				break;
			}
			_adjustedSlerpRate = Mathf.Clamp01(_adjustedSlerpRate);
			UpdateRotation(_currentDirection, _alignmentDirection, _adjustedSlerpRate, _usePhysicsToRotate);
		}
	}

	protected virtual void UpdateRotation(Vector3 currentDirection, Vector3 targetDirection, float slerpRate, bool usePhysics)
	{
		if (usePhysics)
		{
			Vector3 vector = OWPhysics.FromToAngularVelocity(currentDirection, targetDirection);
			_owRigidbody.SetAngularVelocity(Vector3.zero);
			_owRigidbody.AddAngularVelocityChange(vector * slerpRate);
		}
		else
		{
			Quaternion quaternion = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(currentDirection, targetDirection), slerpRate);
			_owRigidbody.GetRigidbody().rotation = quaternion * _owRigidbody.GetRotation();
		}
	}

	protected void UpdateAlignment()
	{
		_lastAllowAlignment = _allowAlignment;
		_allowAlignment = CheckAlignmentRequirements();
		if (_allowAlignment != _lastAllowAlignment)
		{
			if (_allowAlignment)
			{
				InitAlignment();
			}
			else
			{
				BreakAlignment();
			}
		}
	}
}
