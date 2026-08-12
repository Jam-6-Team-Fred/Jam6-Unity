using UnityEngine;

public abstract class ThrusterController : MonoBehaviour
{
	protected bool _isRollMode;

	protected int _rollScalar = 1;

	protected bool _isRotationalThrustEnabled = true;

	protected ThrusterModel _thrusterModel;

	private Vector3 _translationalInput;

	private Vector3 _rotationalInput;

	private bool _isRollPressed;

	protected virtual void Awake()
	{
		_thrusterModel = this.GetRequiredComponent<ThrusterModel>();
		AlignWithForce component = GetComponent<AlignWithForce>();
		if (component != null)
		{
			component.OnInitAlignment += OnInitAlignment;
			component.OnBreakAlignment += OnBreakAlignment;
		}
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

	public Vector3 GetTranslationalInput()
	{
		return _translationalInput;
	}

	public Vector3 GetRotationalInput()
	{
		return _rotationalInput;
	}

	public void SetRollMode(bool isRollMode, int rollScalar)
	{
		_rollScalar = rollScalar;
		_isRollMode = isRollMode;
		_isRollPressed = false;
	}

	protected abstract Vector3 ReadTranslationalInput();

	protected abstract Vector3 ReadRotationalInput();

	protected virtual void OnInitAlignment()
	{
		_isRotationalThrustEnabled = false;
	}

	protected virtual void OnBreakAlignment()
	{
		_isRotationalThrustEnabled = true;
	}

	protected virtual void Update()
	{
		if (OWInput.IsInputMode(InputMode.Character | InputMode.ShipCockpit | InputMode.LandingCam | InputMode.NomaiRemoteCam) && OWInput.IsNewlyPressed(InputLibrary.rollMode))
		{
			_isRollMode = !_isRollMode;
			_isRollPressed = true;
		}
		if (_isRollPressed && OWInput.IsNewlyReleased(InputLibrary.rollMode))
		{
			_isRollPressed = false;
			_isRollMode = !_isRollMode;
		}
	}

	public bool IsRollMode()
	{
		if (_isRollMode)
		{
			return PlayerState.InZeroG();
		}
		return false;
	}

	protected virtual bool AllowHorizontalThrust()
	{
		return true;
	}

	private void FixedUpdate()
	{
		_translationalInput = ReadTranslationalInput();
		if (!AllowHorizontalThrust())
		{
			_translationalInput.x = 0f;
			_translationalInput.z = 0f;
		}
		_thrusterModel.AddTranslationalInput(_translationalInput);
		if (_isRotationalThrustEnabled)
		{
			_rotationalInput = ReadRotationalInput();
			_thrusterModel.AddRotationalInput(_rotationalInput);
		}
	}
}
