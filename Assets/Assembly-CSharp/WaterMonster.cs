using System.Collections.Generic;
using UnityEngine;

public class WaterMonster : MonoBehaviour
{
	[SerializeField]
	private float _maxSpeed;

	[SerializeField]
	private float _maxAccel;

	[SerializeField]
	private float _maxAccelWithBloodlust;

	[SerializeField]
	private float _maxBloodlust;

	[SerializeField]
	private float _bloodlustCooldown;

	[Space]
	[SerializeField]
	private bool _doesProbeTrigger;

	[Space]
	[SerializeField]
	private SwampFluidVolume _swampVolume;

	[SerializeField]
	private OWTriggerVolume _mouth;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private GameObject _headsRotation;

	[SerializeField]
	private GameObject _tailsRotation;

	private DayNightPlanetController _dayNightController;

	private GameObject _prey;

	private List<GameObject> _potentialPreys;

	private OWTriggerVolume _water;

	private int _bloodlust;

	private Vector2 _speed;

	private Vector3 _targetPos;

	private bool _isHunting;

	private float _lastTimeHunting;

	private void Awake()
	{
		_water = _swampVolume.GetComponent<OWTriggerVolume>();
		if (_water == null)
		{
			Debug.LogError("Missing OWTriggerVolume on the swamp water");
		}
		else
		{
			_water.OnEntry += OnWaterEntered;
			_water.OnExit += OnWaterExit;
		}
		_mouth.OnEntry += OnEating;
		_potentialPreys = new List<GameObject>(5);
		_bloodlust = 0;
	}

	private void Start()
	{
		_dayNightController = Locator.GetAstroObject(AstroObject.Name.RingWorld).GetComponent<DayNightPlanetController>();
		_dayNightController.OnDayHeads += OnSunrise;
		_dayNightController.OnDayTails += OnSunrise;
		_speed = new Vector2(0f, 0f);
		_prey = null;
		_isHunting = false;
	}

	private void OnDestroy()
	{
		if (_water != null)
		{
			_water.OnEntry -= OnWaterEntered;
			_water.OnExit -= OnWaterExit;
		}
	}

	private void OnSunrise()
	{
		if (_dayNightController.IsDay(heads: true))
		{
			base.transform.localRotation = _tailsRotation.transform.localRotation;
		}
		else
		{
			base.transform.localRotation = _headsRotation.transform.localRotation;
		}
		base.transform.SetLocalPositionY(_swampVolume.GetLocalSurfaceYPos(_dayNightController.IsDay(heads: false)));
		if (_prey != null)
		{
			_potentialPreys.Add(_prey);
			_prey = null;
		}
		CheckForNewPrey();
	}

	private void FixedUpdate()
	{
		base.transform.SetLocalPositionY(_swampVolume.GetLocalSurfaceYPos(_dayNightController.IsDay(heads: false)));
		if (_prey != null)
		{
			_isHunting = true;
			_targetPos = _water.transform.InverseTransformPoint(_prey.transform.position);
		}
		else
		{
			if (_isHunting)
			{
				_isHunting = false;
				_lastTimeHunting = Time.time;
			}
			if (_bloodlust > 0 && Time.time - _lastTimeHunting > Mathf.Pow(_bloodlustCooldown, 1f + _maxBloodlust - (float)_bloodlust))
			{
				_bloodlust--;
			}
		}
		Vector2 vector = default(Vector2);
		vector.x = _targetPos.x - base.transform.localPosition.x;
		vector.y = _targetPos.z - base.transform.localPosition.z;
		if (vector.sqrMagnitude > 0.01f)
		{
			Vector2 vector2 = vector - _speed;
			if (vector2.sqrMagnitude > MaxAccel() * MaxAccel())
			{
				vector2.Normalize();
				vector2 *= MaxAccel();
			}
			_speed += vector2;
			if (_speed.sqrMagnitude > _maxSpeed * _maxSpeed)
			{
				_speed.Normalize();
				_speed *= _maxSpeed;
			}
			base.transform.SetLocalPositionX(base.transform.localPosition.x + _speed.x * Time.fixedDeltaTime);
			base.transform.SetLocalPositionZ(base.transform.localPosition.z + _speed.y * Time.fixedDeltaTime);
		}
	}

	private float MaxAccel()
	{
		return _maxAccel + (_maxAccelWithBloodlust - _maxAccel) * ((float)_bloodlust / _maxBloodlust);
	}

	private void OnWaterEntered(GameObject hitObj)
	{
		if (!hitObj.CompareTag("PlayerDetector") && (!_doesProbeTrigger || !hitObj.CompareTag("ProbeDetector")))
		{
			return;
		}
		if (_dayNightController.IsPointOnDaySide(hitObj.transform.position))
		{
			_potentialPreys.Add(hitObj);
			return;
		}
		triggerHunt();
		if (_prey != null)
		{
			_potentialPreys.Add(_prey);
		}
		_prey = hitObj;
	}

	private void OnWaterExit(GameObject hitObj)
	{
		if ((hitObj.CompareTag("PlayerDetector") || (_doesProbeTrigger && hitObj.CompareTag("ProbeDetector"))) && hitObj == _prey)
		{
			_prey = null;
			CheckForNewPrey();
		}
	}

	private void OnEating(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
		}
	}

	private void CheckForNewPrey()
	{
		for (int i = 0; i < _potentialPreys.Count; i++)
		{
			if (!_dayNightController.IsPointOnDaySide(_potentialPreys[i].transform.position))
			{
				triggerHunt();
				_prey = _potentialPreys[i];
				_potentialPreys.RemoveAt(i);
				break;
			}
		}
	}

	private void triggerHunt()
	{
		_oneShotSource.PlayOneShot(AudioType.DBAnglerfishDetectTarget);
		if ((float)_bloodlust < _maxBloodlust)
		{
			_bloodlust++;
		}
	}
}
