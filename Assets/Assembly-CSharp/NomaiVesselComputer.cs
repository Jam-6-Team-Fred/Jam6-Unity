using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NomaiVesselComputer : NomaiText
{
	private enum State
	{
		Stowed = 0,
		TurningOn = 1,
		Raising = 2,
		Activating = 3,
		Activated = 4,
		Deactivating = 5,
		Descending = 6,
		TurningOff = 7
	}

	private OWCollider _interactOWCollider;

	[SerializeField]
	private bool _startActivated;

	[Space]
	[SerializeField]
	private Transform _pillarObject;

	[Space]
	[SerializeField]
	private float _pillarStowedHeight;

	[SerializeField]
	private float _pillarExtendedHeight = 2.575525f;

	[SerializeField]
	private float _pillarMoveTime = 1f;

	[Space]
	[SerializeField]
	private OWLight2 _topLight;

	[SerializeField]
	private OWEmissiveRenderer _topLightRenderer;

	[SerializeField]
	private float _lightFadeTime = 1f;

	[Space]
	[SerializeField]
	private NomaiVesselComputerRing[] _computerRings = new NomaiVesselComputerRing[0];

	[SerializeField]
	private float _ringDelay = 1f;

	[Space]
	[SerializeField]
	private OWAudioSource _audioSource;

	private State _state;

	private float _stateChangeTime;

	private float _pillarStartHeight;

	private float _lightStartIntensityScale;

	private float _lightStartEmissionScale;

	private bool _reboot;

	private TextAsset _newTextAsset;

	private int _numEntries;

	protected override void Awake()
	{
		base.Awake();
		_interactOWCollider = base.gameObject.GetAddComponent<OWCollider>();
		_interactOWCollider.SetLODActivationMask(DynamicOccupant.Player);
		_interactOWCollider.SetActivation(_startActivated);
		_reboot = false;
		_newTextAsset = null;
	}

	private void Start()
	{
		if (_startActivated)
		{
			_state = State.Activated;
			_pillarObject.localPosition = new Vector3(0f, _pillarExtendedHeight, 0f);
		}
		else
		{
			_state = State.Stowed;
			_pillarObject.localPosition = new Vector3(0f, _pillarStowedHeight, 0f);
			_topLight.SetIntensityScale(0f);
			_topLightRenderer.SetEmissiveScale(0f);
		}
		_audioSource.SetLocalVolume(0f);
		base.enabled = false;
	}

	public override void LateInitialize()
	{
		base.LateInitialize();
		_numEntries = GetNumTextBlocks();
		if (_startActivated)
		{
			for (int i = 0; i < _numEntries; i++)
			{
				_computerRings[i].ActivateInstant(i + 1, _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe));
			}
		}
	}

	public override void SetAsTranslated(int id)
	{
		base.SetAsTranslated(id);
		for (int i = 0; i < _computerRings.Length; i++)
		{
			if (_computerRings[i].IsActivated() && _computerRings[i].GetEntryID() == id)
			{
				_computerRings[i].SetAsTranslated();
				break;
			}
		}
	}

	private void FixedUpdate()
	{
		switch (_state)
		{
		case State.Stowed:
			if (_reboot)
			{
				ChangeState(State.TurningOn);
				LoadNewText(_newTextAsset);
				_reboot = false;
				_newTextAsset = null;
			}
			else
			{
				base.enabled = false;
			}
			break;
		case State.TurningOn:
		{
			float t4 = (Time.time - _stateChangeTime) / _lightFadeTime;
			_topLight.SetIntensityScale(Mathf.SmoothStep(_lightStartIntensityScale, 1f, t4));
			_topLightRenderer.SetEmissiveScale(Mathf.Lerp(_lightStartEmissionScale, 1f, t4));
			if (Time.time > _stateChangeTime + _lightFadeTime)
			{
				ChangeState(State.Raising);
			}
			break;
		}
		case State.Raising:
		{
			float t3 = (Time.time - _stateChangeTime) / _pillarMoveTime;
			float y2 = Mathf.SmoothStep(_pillarStartHeight, _pillarExtendedHeight, t3);
			_pillarObject.localPosition = new Vector3(0f, y2, 0f);
			if (Time.time > _stateChangeTime + _pillarMoveTime)
			{
				ChangeState(State.Activating);
				for (int k = 0; k < _numEntries; k++)
				{
					_computerRings[k].Activate(k + 1, (float)k * _ringDelay);
				}
			}
			break;
		}
		case State.Activating:
		{
			bool flag = true;
			for (int i = 0; i < _numEntries; i++)
			{
				if (!_computerRings[i].IsDoneFlickering())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				ChangeState(State.Activated);
				_interactOWCollider.SetActivation(active: true);
			}
			break;
		}
		case State.Activated:
			base.enabled = false;
			break;
		case State.Deactivating:
		{
			bool flag2 = true;
			for (int j = 0; j < _numEntries; j++)
			{
				if (!_computerRings[j].IsDoneFlickering())
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				ChangeState(State.Descending);
				_pillarStartHeight = _pillarObject.localPosition.y;
			}
			break;
		}
		case State.Descending:
		{
			float t2 = (Time.time - _stateChangeTime) / _pillarMoveTime;
			float y = Mathf.SmoothStep(_pillarStartHeight, _pillarStowedHeight, t2);
			_pillarObject.localPosition = new Vector3(0f, y, 0f);
			if (Time.time > _stateChangeTime + _pillarMoveTime)
			{
				ChangeState(State.TurningOff);
			}
			break;
		}
		case State.TurningOff:
		{
			float t = (Time.time - _stateChangeTime) / _lightFadeTime;
			_topLight.SetIntensityScale(Mathf.SmoothStep(_lightStartIntensityScale, 0f, t));
			_topLightRenderer.SetEmissiveScale(Mathf.Lerp(_lightStartEmissionScale, 0f, t));
			if (Time.time > _stateChangeTime + _lightFadeTime)
			{
				ChangeState(State.Stowed);
			}
			break;
		}
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag2 = (base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe));
		for (int i = 0; i < _numEntries; i++)
		{
			_computerRings[i].enabled = flag2;
		}
		if (_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			_audioSource.FadeIn(0.5f);
		}
		else
		{
			_audioSource.FadeOut(0.5f);
		}
	}

	private void ChangeState(State newState)
	{
		_state = newState;
		_stateChangeTime = Time.time;
		_pillarStartHeight = _pillarObject.localPosition.y;
		_lightStartIntensityScale = _topLight.GetIntensityScale();
		_lightStartEmissionScale = _topLightRenderer.GetEmissiveScale();
	}

	private void LoadNewText(TextAsset textAsset)
	{
		SetTextAsset(_newTextAsset);
		InitializeXmlAsset();
		_numEntries = GetNumTextBlocks();
	}

	public void TurnOn()
	{
		switch (_state)
		{
		case State.Stowed:
			ChangeState(State.TurningOn);
			base.enabled = true;
			break;
		case State.Deactivating:
		{
			ChangeState(State.Activating);
			for (int i = 0; i < _numEntries; i++)
			{
				_computerRings[i].Activate(i + 1, (float)i * _ringDelay);
			}
			break;
		}
		case State.Descending:
			ChangeState(State.Raising);
			_pillarStartHeight = _pillarObject.localPosition.y;
			break;
		case State.TurningOff:
			ChangeState(State.TurningOn);
			break;
		}
	}

	public void TurnOff()
	{
		_interactOWCollider.SetActivation(active: false);
		switch (_state)
		{
		case State.TurningOn:
			ChangeState(State.TurningOff);
			break;
		case State.Raising:
			ChangeState(State.Descending);
			_pillarStartHeight = _pillarObject.localPosition.y;
			break;
		case State.Activating:
		{
			ChangeState(State.Deactivating);
			for (int j = 0; j < _numEntries; j++)
			{
				_computerRings[j].Deactivate((float)j * _ringDelay);
			}
			break;
		}
		case State.Activated:
		{
			ChangeState(State.Deactivating);
			for (int i = 0; i < _numEntries; i++)
			{
				_computerRings[i].Deactivate((float)i * _ringDelay);
			}
			base.enabled = true;
			break;
		}
		}
	}

	public void RebootWithNewText(TextAsset textAsset)
	{
		TurnOff();
		_reboot = true;
		_newTextAsset = textAsset;
		base.enabled = true;
	}

	public bool IsOn()
	{
		return _state != State.Stowed;
	}

	public bool IsTurningOn()
	{
		if (_state != State.TurningOn && _state != State.Raising)
		{
			return _state == State.Activating;
		}
		return true;
	}

	public bool IsTurningOff()
	{
		if (_state != State.Deactivating && _state != State.Descending)
		{
			return _state == State.TurningOff;
		}
		return true;
	}

	public NomaiVesselComputerRing GetClosestRing(Vector3 worldPos, out float verticalDist)
	{
		Vector3 vector = _pillarObject.InverseTransformPoint(worldPos);
		float num = float.PositiveInfinity;
		NomaiVesselComputerRing result = null;
		for (int i = 0; i < _computerRings.Length; i++)
		{
			if (_computerRings[i].IsActivated())
			{
				float num2 = Mathf.Abs(vector.y - _computerRings[i].transform.localPosition.y);
				if (num2 < num)
				{
					num = num2;
					result = _computerRings[i];
				}
			}
		}
		verticalDist = num;
		return result;
	}

	public NomaiVesselComputerRing GetClosestRing(Vector3 worldPos)
	{
		float verticalDist;
		return GetClosestRing(worldPos, out verticalDist);
	}
}
