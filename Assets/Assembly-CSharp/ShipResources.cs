using UnityEngine;

public class ShipResources : MonoBehaviour
{
	[SerializeField]
	private float _maxOxygen = 6000f;

	[SerializeField]
	private float _maxFuel = 10000f;

	private float _currentOxygen;

	private float _currentFuel;

	private bool _hullBreach;

	private NotificationData _fuelDepletedNotification;

	private ShipThrusterModel _shipThruster;

	private bool _killingResources;

	private void Awake()
	{
		_currentOxygen = _maxOxygen;
		_currentFuel = _maxFuel;
		_hullBreach = false;
		_fuelDepletedNotification = new NotificationData(NotificationTarget.Ship, "SHIP FUEL DEPLETED", 1f);
		_shipThruster = this.GetRequiredComponent<ShipThrusterModel>();
		GlobalMessenger.AddListener("ShipHullBreach", OnShipHullBreach);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("ShipHullBreach", OnShipHullBreach);
	}

	private void OnShipHullBreach()
	{
		_hullBreach = true;
	}

	public bool AreThrustersUsable()
	{
		return _currentFuel > 0f;
	}

	public float GetOxygen()
	{
		return _currentOxygen;
	}

	public float GetFractionalOxygen()
	{
		return _currentOxygen / _maxOxygen;
	}

	public void SetOxygen(float oxygen)
	{
		_currentOxygen = Mathf.Clamp(oxygen, 0f, _maxOxygen);
	}

	public void DrainOxygen(float amount)
	{
		_currentOxygen = Mathf.Max(_currentOxygen - amount, 0f);
	}

	public void AddOxygen(float amount)
	{
		_currentOxygen = Mathf.Min(_currentOxygen + amount, _maxOxygen);
	}

	public float GetFuel()
	{
		return _currentFuel;
	}

	public float GetFractionalFuel()
	{
		return _currentFuel / _maxFuel;
	}

	public void SetFuel(float fuel)
	{
		_currentFuel = Mathf.Clamp(fuel, 0f, _maxFuel);
	}

	public void DrainFuel(float amount)
	{
		_currentFuel = Mathf.Max(_currentFuel - amount, 0f);
	}

	public void AddFuel(float amount)
	{
		_currentFuel = Mathf.Min(_currentFuel + amount, _maxFuel);
	}

	public void SetDebugKillResources(bool killResources)
	{
		_killingResources = killResources;
	}

	public void RefillResources()
	{
		_currentOxygen = _maxOxygen;
		_currentFuel = _maxFuel;
	}

	private void Update()
	{
		if (_killingResources)
		{
			DebugKillResources();
			return;
		}
		float magnitude = _shipThruster.GetLocalAcceleration().magnitude;
		if (magnitude > 0f)
		{
			DrainFuel(magnitude * 0.1f * Time.deltaTime);
		}
		if (_currentFuel <= 0f && !NotificationManager.SharedInstance.IsPinnedNotification(_fuelDepletedNotification))
		{
			NotificationManager.SharedInstance.PostNotification(_fuelDepletedNotification, pin: true);
		}
		else if (_currentFuel > 0f && NotificationManager.SharedInstance.IsPinnedNotification(_fuelDepletedNotification))
		{
			NotificationManager.SharedInstance.UnpinNotification(_fuelDepletedNotification);
		}
		if (PlayerState.IsInsideShip())
		{
			DrainOxygen(0.13f * Time.deltaTime);
		}
		if (_hullBreach)
		{
			DrainOxygen(1000f * Time.deltaTime);
		}
	}

	private void DebugKillResources()
	{
		if (_currentFuel >= 0f)
		{
			_currentFuel -= Time.deltaTime * _maxFuel;
		}
	}
}
