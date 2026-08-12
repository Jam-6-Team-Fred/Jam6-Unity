using System.Collections.Generic;
using UnityEngine;

public class MeteorLauncher : MonoBehaviour, ILateInitializer
{
	private const int kMeteorPoolSize = 16;

	private const int kDynamicMeteorPoolSize = 8;

	private const float _meteorDelay = 2.3f;

	[SerializeField]
	private GameObject _meteorPrefab;

	[SerializeField]
	private GameObject _dynamicMeteorPrefab;

	[SerializeField]
	private float _dynamicProbability = 0.05f;

	[SerializeField]
	private float _minLaunchSpeed = 50f;

	[SerializeField]
	private float _maxLaunchSpeed = 150f;

	[SerializeField]
	private float _minInterval = 5f;

	[SerializeField]
	private float _maxInterval = 20f;

	[SerializeField]
	private Vector3 _launchDirection = Vector3.up;

	[SerializeField]
	private ParticleSystem[] _launchParticles = new ParticleSystem[0];

	[Header("Audio")]
	[SerializeField]
	private Sector _audioSector;

	[SerializeField]
	private OWAudioSource _launchSource;

	[Space]
	[SerializeField]
	private ForceVolume _detectableField;

	[SerializeField]
	private FluidVolume _detectableFluid;

	private bool _initialized;

	private List<MeteorController> _meteorPool;

	private List<MeteorController> _launchedMeteors;

	private List<MeteorController> _dynamicMeteorPool;

	private List<MeteorController> _launchedDynamicMeteors;

	private OWRigidbody _parentBody;

	private float _lastLaunchTime;

	private float _launchDelay;

	private bool _areParticlesPlaying;

	private void Awake()
	{
		_parentBody = base.gameObject.GetAttachedOWRigidbody();
		_initialized = false;
		LateInitializerManager.RegisterLateInitializer(this);
	}

	private void Start()
	{
		_lastLaunchTime = Time.time + (30f - _maxInterval);
		_launchDelay = Random.Range(_minInterval, _maxInterval);
	}

	private void OnDestroy()
	{
		if (!_initialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
	}

	public void LateInitialize()
	{
		_initialized = true;
		if (_meteorPrefab != null)
		{
			_meteorPool = new List<MeteorController>(16);
			_launchedMeteors = new List<MeteorController>(16);
			for (int i = 0; i < 16; i++)
			{
				MeteorController requiredComponent = Object.Instantiate(_meteorPrefab).GetRequiredComponent<MeteorController>();
				requiredComponent.Suspend(base.transform);
				_meteorPool.Add(requiredComponent);
			}
		}
		if (_dynamicMeteorPrefab != null)
		{
			_dynamicMeteorPool = new List<MeteorController>(8);
			_launchedDynamicMeteors = new List<MeteorController>(8);
			for (int j = 0; j < 8; j++)
			{
				MeteorController requiredComponent2 = Object.Instantiate(_dynamicMeteorPrefab).GetRequiredComponent<MeteorController>();
				requiredComponent2.Suspend(base.transform);
				_dynamicMeteorPool.Add(requiredComponent2);
			}
		}
	}

	private void FixedUpdate()
	{
		if (_launchedMeteors != null)
		{
			for (int num = _launchedMeteors.Count - 1; num >= 0; num--)
			{
				if (_launchedMeteors[num] == null)
				{
					_launchedMeteors.QuickRemoveAt(num);
				}
				else if (_launchedMeteors[num].isSuspended)
				{
					_meteorPool.Add(_launchedMeteors[num]);
					_launchedMeteors.QuickRemoveAt(num);
				}
			}
		}
		if (_launchedDynamicMeteors != null)
		{
			for (int num2 = _launchedDynamicMeteors.Count - 1; num2 >= 0; num2--)
			{
				if (_launchedDynamicMeteors[num2] == null)
				{
					_launchedDynamicMeteors.QuickRemoveAt(num2);
				}
				else if (_launchedDynamicMeteors[num2].isSuspended)
				{
					_dynamicMeteorPool.Add(_launchedDynamicMeteors[num2]);
					_launchedDynamicMeteors.QuickRemoveAt(num2);
				}
			}
		}
		if (!_initialized || !(Time.time > _lastLaunchTime + _launchDelay))
		{
			return;
		}
		if (!_areParticlesPlaying)
		{
			_areParticlesPlaying = true;
			for (int i = 0; i < _launchParticles.Length; i++)
			{
				_launchParticles[i].Play();
			}
		}
		if (Time.time > _lastLaunchTime + _launchDelay + 2.3f)
		{
			LaunchMeteor();
			_lastLaunchTime = Time.time;
			_launchDelay = Random.Range(_minInterval, _maxInterval);
			_areParticlesPlaying = false;
			for (int j = 0; j < _launchParticles.Length; j++)
			{
				_launchParticles[j].Stop();
			}
		}
	}

	private void LaunchMeteor()
	{
		bool num = _dynamicMeteorPool != null && (_meteorPool == null || Random.value < _dynamicProbability);
		MeteorController meteorController = null;
		if (!num)
		{
			if (_meteorPool.Count == 0)
			{
				Debug.LogWarning("MeteorLauncher is out of Meteors!", this);
			}
			else
			{
				meteorController = _meteorPool[_meteorPool.Count - 1];
				meteorController.Initialize(base.transform, _detectableField, _detectableFluid);
				_meteorPool.QuickRemoveAt(_meteorPool.Count - 1);
				_launchedMeteors.Add(meteorController);
			}
		}
		else if (_dynamicMeteorPool.Count == 0)
		{
			Debug.LogWarning("MeteorLauncher is out of Dynamic Meteors!", this);
		}
		else
		{
			meteorController = _dynamicMeteorPool[_dynamicMeteorPool.Count - 1];
			meteorController.Initialize(base.transform, null, null);
			_dynamicMeteorPool.QuickRemoveAt(_dynamicMeteorPool.Count - 1);
			_launchedDynamicMeteors.Add(meteorController);
		}
		if (meteorController != null)
		{
			Vector3 linearVelocity = _parentBody.GetPointVelocity(base.transform.position) + base.transform.TransformDirection(_launchDirection) * Random.Range(_minLaunchSpeed, _maxLaunchSpeed);
			Vector3 angularVelocity = base.transform.forward * 2f;
			meteorController.Launch(null, base.transform.position, base.transform.rotation, linearVelocity, angularVelocity);
			if (_audioSector.ContainsOccupant(DynamicOccupant.Player))
			{
				_launchSource.pitch = Random.Range(0.4f, 0.6f);
				_launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawRay(base.transform.position, base.transform.TransformDirection(_launchDirection).normalized * (_minLaunchSpeed + _maxLaunchSpeed) * 0.5f);
		Gizmos.DrawWireSphere(base.transform.position, 10f);
	}
}
