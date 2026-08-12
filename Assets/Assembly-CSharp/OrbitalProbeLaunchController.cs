using UnityEngine;

public class OrbitalProbeLaunchController : MonoBehaviour
{
	[SerializeField]
	private OWRigidbody _probeBody;

	[SerializeField]
	private OWRigidbody[] _fakeDebrisBodies;

	[SerializeField]
	private SectorProxy[] _realDebrisSectorProxies;

	[SerializeField]
	private ParticleSystem[] _launchParticles;

	[SerializeField]
	private bool _debugShowCannonBreaking;

	private Transform _giantsDeepTransform;

	private Transform _timberHearthTransform;

	private bool _showCannonBreaking;

	private int _fakeCount;

	private int _realCount;

	private bool _hasLaunchedProbe;

	private float _probeLaunchTime;

	public SectorProxy[] realDebrisSectorProxies => _realDebrisSectorProxies;

	private void Awake()
	{
		Vector3 vector = GameObject.FindWithTag("Player").transform.position - base.transform.position;
		base.transform.forward = Vector3.Cross(Vector3.up, vector);
		base.transform.rotation = Quaternion.FromToRotation(base.transform.up, Vector3.up) * base.transform.rotation;
		float angle = Random.Range(0f, 360f);
		float angle2 = Random.Range(-90f, 90f);
		int num = PlayerData.LoadLoopCount();
		if (num <= 7)
		{
			angle2 = Random.Range(-45f, 0f);
			angle = ((Random.value > 0.5f) ? Random.Range(-80f, -100f) : ((float)Random.Range(50, 70)));
			if (num == 3)
			{
				angle2 = -110f;
				angle = 70f;
			}
		}
		Quaternion quaternion = Quaternion.AngleAxis(angle, vector);
		base.transform.rotation = quaternion * base.transform.rotation;
		Quaternion quaternion2 = Quaternion.AngleAxis(angle2, base.transform.up);
		base.transform.rotation = quaternion2 * base.transform.rotation;
		GlobalMessenger<int>.AddListener("StartOfTimeLoop", OnStartOfTimeLoop);
		GlobalMessenger.AddListener("ResumeSimulation", OnResumeSimulation);
	}

	private void OnDestroy()
	{
		GlobalMessenger<int>.RemoveListener("StartOfTimeLoop", OnStartOfTimeLoop);
		GlobalMessenger.RemoveListener("ResumeSimulation", OnResumeSimulation);
	}

	private void OnStartOfTimeLoop(int loopCount)
	{
		_probeLaunchTime = Time.time + 1f;
		bool flag = Locator.GetAstroObject(AstroObject.Name.TimberHearth) != null && Locator.GetAstroObject(AstroObject.Name.GiantsDeep) != null;
		_showCannonBreaking = (LoadManager.GetCurrentScene() == OWScene.SolarSystem && flag) || _debugShowCannonBreaking;
		if (_showCannonBreaking)
		{
			_timberHearthTransform = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
			_giantsDeepTransform = Locator.GetAstroObject(AstroObject.Name.GiantsDeep).transform;
			for (int i = 0; i < _realDebrisSectorProxies.Length; i++)
			{
				_realDebrisSectorProxies[i].gameObject.SetActive(value: false);
			}
		}
		else
		{
			for (int j = 0; j < _fakeDebrisBodies.Length; j++)
			{
				Object.Destroy(_fakeDebrisBodies[j].gameObject);
			}
		}
	}

	private void OnResumeSimulation()
	{
		base.enabled = false;
		Object.Destroy(_probeBody.gameObject);
		for (int i = 0; i < _fakeDebrisBodies.Length; i++)
		{
			Object.Destroy(_fakeDebrisBodies[i].gameObject);
		}
	}

	private void FixedUpdate()
	{
		if (!_hasLaunchedProbe)
		{
			if (Time.time > _probeLaunchTime)
			{
				LaunchProbe();
				_hasLaunchedProbe = true;
			}
			return;
		}
		if (_showCannonBreaking)
		{
			Vector3 to = _giantsDeepTransform.position - _timberHearthTransform.position;
			for (int i = 0; i < _fakeDebrisBodies.Length; i++)
			{
				if (_fakeDebrisBodies[i] != null && Vector3.Angle(_fakeDebrisBodies[i].transform.position - _giantsDeepTransform.position, to) < 20f)
				{
					Object.Destroy(_fakeDebrisBodies[i].gameObject);
					_fakeCount++;
				}
			}
			for (int j = 0; j < _realDebrisSectorProxies.Length; j++)
			{
				if (!_realDebrisSectorProxies[j].gameObject.activeSelf && Vector3.Angle(_realDebrisSectorProxies[j].GetAttachedOWRigidbody().transform.position - _giantsDeepTransform.position, to) < 20f)
				{
					_realDebrisSectorProxies[j].gameObject.SetActive(value: true);
					_realCount++;
				}
			}
		}
		if (!_showCannonBreaking || (_realCount == _realDebrisSectorProxies.Length && _fakeCount == _fakeDebrisBodies.Length))
		{
			base.enabled = false;
		}
	}

	private void LaunchProbe()
	{
		if (_probeBody == null)
		{
			Debug.LogError("Nomai probe is NULL");
			return;
		}
		_probeBody.AddVelocityChange(_probeBody.GetOrigParentBody().transform.forward * 500f);
		_probeBody.AddAngularVelocityChange(_probeBody.transform.right * 0.1f);
		if (_showCannonBreaking)
		{
			for (int i = 0; i < _launchParticles.Length; i++)
			{
				_launchParticles[i].Play();
			}
			for (int j = 0; j < _fakeDebrisBodies.Length; j++)
			{
				_fakeDebrisBodies[j].AddVelocityChange(_fakeDebrisBodies[j].GetOrigParentBody().transform.forward * 20f * (j + 1));
			}
		}
	}
}
