using UnityEngine;

public class WhiteHoleStationRotation : MonoBehaviour
{
	private enum StationRotationState
	{
		HALTED = 0,
		ACC_CLOCK = 1,
		ACC_COUNTER = 2,
		CLOCK = 3,
		COUNTER = 4,
		DEC_CLOCK = 5,
		DEC_COUNTER = 6,
		NONE = 7
	}

	[SerializeField]
	private NomaiInterfaceSlot _slotClockwise;

	[SerializeField]
	private NomaiInterfaceSlot _slotCounterClockwise;

	[SerializeField]
	private OWRigidbody _stationBody;

	[SerializeField]
	private float _stationSlowdownFriction = 1f;

	[SerializeField]
	private float _stationAngularAcceleration = 1f;

	[SerializeField]
	private float _maxAngularVelocity = 0.1f;

	[SerializeField]
	private float _alignmentAngularVelThreshold = 0.25f;

	[SerializeField]
	private OWLightController _onLights;

	[SerializeField]
	private OWLightController _offLights;

	[Space]
	[SerializeField]
	private OWAudioSource _audioSource;

	private bool _hasStationBeenActivated;

	private StationRotationState _rotationState;

	private void Awake()
	{
		_slotClockwise.OnSlotActivated += OnSlotActivated;
		_slotCounterClockwise.OnSlotActivated += OnSlotActivated;
	}

	private void Start()
	{
		_onLights.SetIntensity(0f);
		_offLights.SetIntensity(1f);
	}

	private void OnDestroy()
	{
		_slotClockwise.OnSlotActivated -= OnSlotActivated;
		_slotCounterClockwise.OnSlotActivated -= OnSlotActivated;
	}

	private void switchLights(bool on)
	{
		_onLights.FadeTo(on ? 1 : 0, 1f);
		_offLights.FadeTo((!on) ? 1 : 0, 1f);
	}

	private void FixedUpdate()
	{
		switch (_rotationState)
		{
		case StationRotationState.DEC_CLOCK:
		case StationRotationState.DEC_COUNTER:
			_stationBody.AddAngularAcceleration(_stationBody.GetAngularVelocity() * (0f - _stationSlowdownFriction));
			if (_stationBody.GetAngularVelocity().sqrMagnitude < _alignmentAngularVelThreshold * _alignmentAngularVelThreshold)
			{
				_stationBody.SetAngularVelocity(Vector3.zero);
				_rotationState = StationRotationState.HALTED;
			}
			break;
		case StationRotationState.ACC_CLOCK:
			_stationBody.AddAngularAcceleration(new Vector3(0f, 1f, 0f) * _stationAngularAcceleration);
			if (_stationBody.GetAngularVelocity().sqrMagnitude > _maxAngularVelocity * _maxAngularVelocity && _stationBody.GetAngularVelocity().y > 0f)
			{
				_rotationState = StationRotationState.CLOCK;
			}
			break;
		case StationRotationState.ACC_COUNTER:
			_stationBody.AddAngularAcceleration(new Vector3(0f, -1f, 0f) * _stationAngularAcceleration);
			if (_stationBody.GetAngularVelocity().sqrMagnitude > _maxAngularVelocity * _maxAngularVelocity && _stationBody.GetAngularVelocity().y < 0f)
			{
				_rotationState = StationRotationState.COUNTER;
			}
			break;
		default:
			Debug.LogError("White Hole Station in a weird state!");
			break;
		case StationRotationState.HALTED:
		case StationRotationState.CLOCK:
		case StationRotationState.COUNTER:
			break;
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		if (!_hasStationBeenActivated && (slot == _slotClockwise || slot == _slotCounterClockwise))
		{
			_hasStationBeenActivated = true;
			_audioSource.PlayOneShot(AudioType.WHS_StationActivation);
		}
		if (slot == _slotClockwise)
		{
			switch (_rotationState)
			{
			case StationRotationState.HALTED:
			case StationRotationState.DEC_CLOCK:
			case StationRotationState.DEC_COUNTER:
				switchLights(on: true);
				_rotationState = StationRotationState.ACC_CLOCK;
				break;
			case StationRotationState.ACC_COUNTER:
			case StationRotationState.COUNTER:
				_rotationState = StationRotationState.ACC_CLOCK;
				break;
			default:
				Debug.LogError("ERROR");
				break;
			}
		}
		else if (slot == _slotCounterClockwise)
		{
			switch (_rotationState)
			{
			case StationRotationState.HALTED:
			case StationRotationState.DEC_CLOCK:
			case StationRotationState.DEC_COUNTER:
				switchLights(on: true);
				_rotationState = StationRotationState.ACC_COUNTER;
				break;
			case StationRotationState.ACC_CLOCK:
			case StationRotationState.CLOCK:
				_rotationState = StationRotationState.ACC_COUNTER;
				break;
			default:
				Debug.LogError("ERROR");
				break;
			}
		}
		else
		{
			Debug.LogError("There should not be an off slot for the White Hole Station Anymore");
			switchLights(on: false);
			switch (_rotationState)
			{
			case StationRotationState.ACC_CLOCK:
			case StationRotationState.CLOCK:
				_rotationState = StationRotationState.DEC_CLOCK;
				break;
			case StationRotationState.ACC_COUNTER:
			case StationRotationState.COUNTER:
				_rotationState = StationRotationState.DEC_COUNTER;
				break;
			}
		}
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		if (slot == _slotClockwise)
		{
			StationRotationState rotationState = _rotationState;
			if (rotationState == StationRotationState.ACC_CLOCK || rotationState == StationRotationState.CLOCK)
			{
				_rotationState = StationRotationState.DEC_CLOCK;
			}
			else
			{
				Debug.LogError("Trying to stop a halted or halting White Hole Station");
			}
		}
		else
		{
			StationRotationState rotationState = _rotationState;
			if (rotationState == StationRotationState.ACC_COUNTER || rotationState == StationRotationState.COUNTER)
			{
				_rotationState = StationRotationState.DEC_COUNTER;
			}
			else
			{
				Debug.LogError("Trying to stop a halted or halting White Hole Station");
			}
		}
	}
}
