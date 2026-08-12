using UnityEngine;

public class RotaryDial : MonoBehaviour
{
	[SerializeField]
	private Transform[] _rotatingElements;

	[Space]
	[SerializeField]
	private int _nbSymbols = 12;

	[SerializeField]
	private int _symbolSelected;

	[SerializeField]
	private float _timeToStartRotating = 0.4f;

	[SerializeField]
	private float _timePerRotation = 0.1f;

	[Space]
	[SerializeField]
	private float _rotationSpeed = 120f;

	[SerializeField]
	private float _anglePrecision = 5f;

	[SerializeField]
	private bool _snapToClosest = true;

	[Space]
	[SerializeField]
	private Material _glowMaterial;

	[SerializeField]
	private MeshRenderer _renderer;

	private float _time;

	private bool _startedRotating;

	private bool _positiveRotation;

	private bool _rotating;

	private bool _turningOff;

	private Material _origMaterial;

	public bool IsRotating()
	{
		return _rotating;
	}

	private void Awake()
	{
		if (_rotatingElements.Length < 1)
		{
			Debug.LogError("No rotating dial.");
		}
		if (_origMaterial != null)
		{
			_origMaterial = _renderer.sharedMaterial;
		}
		InstantRotate();
		_turningOff = false;
		base.enabled = false;
	}

	private void Update()
	{
		float num = _rotatingElements[0].localRotation.eulerAngles.y % 360f;
		if (_rotating)
		{
			float targetRotation = GetTargetRotation(num);
			for (int i = 0; i < _rotatingElements.Length; i++)
			{
				_rotatingElements[i].Rotate(new Vector3(0f, (_positiveRotation ? 1f : (-1f)) * _rotationSpeed * Time.deltaTime, 0f));
			}
			if (Mathf.Abs(num - targetRotation) < _anglePrecision)
			{
				_rotating = false;
				InstantRotate();
				if (_turningOff)
				{
					_rotating = false;
					base.enabled = false;
					_turningOff = false;
					return;
				}
			}
		}
		else
		{
			_time += Time.deltaTime;
		}
		if (!_turningOff && !_startedRotating && _time > _timeToStartRotating)
		{
			_startedRotating = true;
			_rotating = true;
			IncrementSymbol(1);
			_time = 0f;
		}
		else if (!_turningOff && _startedRotating && _time > _timePerRotation)
		{
			_rotating = true;
			IncrementSymbol(1);
			_time = 0f;
		}
	}

	public void InstantRotate()
	{
		for (int i = 0; i < _rotatingElements.Length; i++)
		{
			float y = _rotatingElements[i].localRotation.eulerAngles.y;
			_rotatingElements[i].Rotate(new Vector3(0f, (float)_symbolSelected * 360f / (float)_nbSymbols - y, 0f));
		}
	}

	private float GetTargetRotation(float currentRotation)
	{
		if (_symbolSelected == 0 && currentRotation > 180f)
		{
			return 360f;
		}
		return (float)_symbolSelected * 360f / (float)_nbSymbols;
	}

	private void IncrementSymbol(int i)
	{
		if (i >= 0)
		{
			_symbolSelected = (_symbolSelected + i) % _nbSymbols;
		}
		else
		{
			_symbolSelected = (_symbolSelected + _nbSymbols + i % _nbSymbols) % _nbSymbols;
		}
	}

	public void Rotate(bool positive)
	{
		IncrementSymbol(positive ? 1 : (-1));
		_rotating = true;
		base.enabled = true;
		_turningOff = true;
		_positiveRotation = positive;
	}

	public void StartRotation()
	{
		_time = 0f;
		_startedRotating = false;
		_rotating = false;
		base.enabled = true;
		if (_origMaterial != null)
		{
			_renderer.sharedMaterial = _glowMaterial;
		}
		_positiveRotation = true;
		if (_turningOff)
		{
			_turningOff = false;
		}
	}

	public int GetSymbolSelected()
	{
		return _symbolSelected;
	}

	public Transform GetCenterTransform()
	{
		return base.transform;
	}

	public void StopRotation()
	{
		if (_rotating)
		{
			float num = _rotatingElements[0].localRotation.eulerAngles.y % 360f;
			if (!_snapToClosest || num < ((float)_symbolSelected - 0.5f) * 360f / (float)_nbSymbols || (_symbolSelected == 0 && num < ((float)_nbSymbols - 0.5f) * 360f / (float)_nbSymbols))
			{
				IncrementSymbol(-1);
				_positiveRotation = false;
			}
			_turningOff = true;
			_time = 0f;
		}
		else
		{
			_rotating = false;
			base.enabled = false;
		}
		_startedRotating = false;
		if (_origMaterial != null)
		{
			_renderer.sharedMaterial = _origMaterial;
		}
	}
}
