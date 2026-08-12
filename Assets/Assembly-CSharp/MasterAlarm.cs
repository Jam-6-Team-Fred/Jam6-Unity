using UnityEngine;

public class MasterAlarm : MonoBehaviour
{
	private ShipDamageController _damageController;

	private ShipAudioController _audioController;

	private ShipReactorComponent _shipReactorComponent;

	private Light _light;

	private PulsingLight _pulsingLight;

	private bool _isAlarmOn;

	private bool _hullCritical;

	private bool _reactorCritical;

	private bool _shipDestroyed;

	private void Awake()
	{
		_damageController = this.GetAttachedOWRigidbody().GetComponentInChildren<ShipDamageController>();
		_shipReactorComponent = _damageController.GetComponentInChildren<ShipReactorComponent>();
		_light = GetComponent<Light>();
		_pulsingLight = GetComponent<PulsingLight>();
		_damageController.OnDamageUpdated += OnShipDamageUpdated;
		_shipReactorComponent.OnDamaged += OnReactorDamaged;
		_shipReactorComponent.OnRepaired += OnReactorRepaired;
		GlobalMessenger.AddListener("ShipSystemFailure", OnShipSystemFailure);
	}

	private void OnDestroy()
	{
		_damageController.OnDamageUpdated -= OnShipDamageUpdated;
		_shipReactorComponent.OnDamaged -= OnReactorDamaged;
		_shipReactorComponent.OnRepaired -= OnReactorRepaired;
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipSystemFailure);
	}

	private void OnShipDamageUpdated()
	{
		_hullCritical = _damageController.GetLowestHullIntegrity() < 0.3f;
		UpdateAlarmState();
	}

	private void OnReactorDamaged(ShipComponent shipComponent)
	{
		_reactorCritical = true;
		UpdateAlarmState();
	}

	private void OnReactorRepaired(ShipComponent shipComponent)
	{
		_reactorCritical = false;
		UpdateAlarmState();
	}

	private void OnShipSystemFailure()
	{
		_shipDestroyed = true;
		if (_isAlarmOn)
		{
			TurnOffAlarm();
		}
	}

	private void UpdateAlarmState()
	{
		if (!_shipDestroyed)
		{
			if ((_hullCritical || _reactorCritical) && !_isAlarmOn)
			{
				TurnOnAlarm();
			}
			else if (!_hullCritical && !_reactorCritical && _isAlarmOn)
			{
				TurnOffAlarm();
			}
		}
	}

	private void TurnOnAlarm()
	{
		if (_audioController == null)
		{
			_audioController = Locator.GetShipTransform().GetComponentInChildren<ShipAudioController>();
		}
		_isAlarmOn = true;
		_audioController.PlayAlarm();
		_light.enabled = true;
		_pulsingLight.Enable();
	}

	private void TurnOffAlarm()
	{
		_isAlarmOn = false;
		_audioController.StopAlarm();
		_light.enabled = false;
		_pulsingLight.Disable();
	}
}
