using UnityEngine;

[RequireComponent(typeof(ImpactSensor))]
public class MapSatelliteStateController : SectoredMonoBehaviour
{
	public enum MapSatelliteState
	{
		NORMAL = 0,
		OFF_COURSE = 1,
		BROKEN = 2
	}

	public delegate void StateChangeEvent(MapSatelliteState newState);

	[SerializeField]
	private float _velocityNominalChange = 70f;

	[SerializeField]
	private float _impactSpeedLimit = 10f;

	[Space]
	[SerializeField]
	private InitialMotion _motionController;

	[SerializeField]
	private AlignWithTargetBody _alignScript;

	[Space]
	[SerializeField]
	private GameObject[] _damagedEffects;

	[SerializeField]
	private LightFlicker _lightFlicker;

	private MapSatelliteState _currentState;

	private OWRigidbody _rigidBody;

	private float _savedVelocitySqr;

	private int _nbOccupants;

	public event StateChangeEvent OnSatelliteStateChange;

	protected override void Awake()
	{
		base.Awake();
		this.GetRequiredComponent<ImpactSensor>().OnImpact += OnImpact;
		_rigidBody = this.GetRequiredComponent<OWRigidbody>();
	}

	private void Start()
	{
		_savedVelocitySqr = _motionController.GetInitVelocity().sqrMagnitude;
		base.enabled = false;
		_nbOccupants = 0;
		for (int i = 0; i < _damagedEffects.Length; i++)
		{
			_damagedEffects[i].SetActive(value: false);
			_lightFlicker.enabled = false;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		this.GetRequiredComponent<ImpactSensor>().OnImpact -= OnImpact;
	}

	private void FixedUpdate()
	{
		CheckAlignment();
	}

	private void CheckAlignment()
	{
		float sqrMagnitude = _rigidBody.GetVelocity().sqrMagnitude;
		if (sqrMagnitude != 0f && _currentState == MapSatelliteState.NORMAL && (sqrMagnitude > _savedVelocitySqr + _velocityNominalChange || sqrMagnitude < _savedVelocitySqr - _velocityNominalChange))
		{
			SetState(MapSatelliteState.OFF_COURSE);
		}
	}

	private void OnImpact(ImpactData impactData)
	{
		if (impactData.speed > _impactSpeedLimit)
		{
			SetState(MapSatelliteState.BROKEN);
			Achievements.Earn(Achievements.Type.SILENCED_CARTOGRAPHER);
		}
	}

	public MapSatelliteState GetState()
	{
		return _currentState;
	}

	private void SetState(MapSatelliteState newState)
	{
		if (newState > _currentState)
		{
			switch (newState)
			{
			case MapSatelliteState.OFF_COURSE:
				OnSatelliteMisaligned();
				break;
			case MapSatelliteState.BROKEN:
				OnSatelliteBroken(_currentState);
				break;
			}
			_currentState = newState;
			if (this.OnSatelliteStateChange != null)
			{
				this.OnSatelliteStateChange(_currentState);
			}
		}
	}

	private void OnSatelliteBroken(MapSatelliteState previousState)
	{
		if (previousState == MapSatelliteState.NORMAL)
		{
			OnSatelliteMisaligned();
		}
		for (int i = 0; i < _damagedEffects.Length; i++)
		{
			_damagedEffects[i].SetActive(value: true);
			_lightFlicker.enabled = true;
		}
	}

	private void OnSatelliteMisaligned()
	{
		DialogueConditionManager.SharedInstance.SetConditionState("BrokeMapSatellite", conditionState: true);
		GlobalMessenger.FireEvent("BrokeMapSatellite");
		_alignScript.enabled = false;
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		base.enabled = true;
		_nbOccupants++;
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		_nbOccupants--;
		if (_nbOccupants == 0)
		{
			base.enabled = false;
		}
	}
}
