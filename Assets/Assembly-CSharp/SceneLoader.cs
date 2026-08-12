using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	[SerializeField]
	private Transform _solarSystemRoot;

	[SerializeField]
	private GameObject _sun;

	[Header("Load Scenes:")]
	[SerializeField]
	private bool _hourglassTwins = true;

	[SerializeField]
	private bool _timberHearth = true;

	[SerializeField]
	private bool _brittleHollow = true;

	[SerializeField]
	private bool _giantsDeep = true;

	[SerializeField]
	private bool _darkBramble = true;

	[SerializeField]
	private bool _comet = true;

	[SerializeField]
	private bool _quantumMoon = true;

	[SerializeField]
	private bool _whiteHole = true;

	[SerializeField]
	private bool _invisiblePlanet;

	[Header("Load Proxy Scenes:")]
	[SerializeField]
	private bool _proxyHourglassTwins = true;

	[SerializeField]
	private bool _proxyTimberHearth = true;

	[SerializeField]
	private bool _proxyBrittleHollow = true;

	[SerializeField]
	private bool _proxyGiantsDeep = true;

	[SerializeField]
	private bool _proxyDarkBramble = true;

	[SerializeField]
	private bool _proxyComet = true;

	[SerializeField]
	private bool _proxyQuantumMoon = true;

	[SerializeField]
	private bool _proxyWhiteHole = true;

	[SerializeField]
	private bool _proxyInvisiblePlanet;

	private void Awake()
	{
		if (_hourglassTwins)
		{
			SceneManager.LoadScene("HourglassTwins", LoadSceneMode.Additive);
		}
		if (_timberHearth)
		{
			SceneManager.LoadScene("TimberHearth", LoadSceneMode.Additive);
		}
		if (_brittleHollow)
		{
			SceneManager.LoadScene("BrittleHollow", LoadSceneMode.Additive);
		}
		if (_giantsDeep)
		{
			SceneManager.LoadScene("GiantsDeep", LoadSceneMode.Additive);
		}
		if (_darkBramble)
		{
			SceneManager.LoadScene("DarkBramble", LoadSceneMode.Additive);
		}
		if (_comet)
		{
			SceneManager.LoadScene("Comet", LoadSceneMode.Additive);
		}
		if (_quantumMoon)
		{
			SceneManager.LoadScene("QuantumMoon", LoadSceneMode.Additive);
		}
		if (_whiteHole)
		{
			SceneManager.LoadScene("WhiteHole", LoadSceneMode.Additive);
		}
		if (_invisiblePlanet)
		{
			SceneManager.LoadScene("RingWorld", LoadSceneMode.Additive);
			SceneManager.LoadScene("DreamWorld", LoadSceneMode.Additive);
		}
		if (_proxyHourglassTwins)
		{
			SceneManager.LoadScene("ProxyHourglassTwins", LoadSceneMode.Additive);
		}
		if (_proxyTimberHearth)
		{
			SceneManager.LoadScene("ProxyTimberHearth", LoadSceneMode.Additive);
		}
		if (_proxyBrittleHollow)
		{
			SceneManager.LoadScene("ProxyBrittleHollow", LoadSceneMode.Additive);
		}
		if (_proxyGiantsDeep)
		{
			SceneManager.LoadScene("ProxyGiantsDeep", LoadSceneMode.Additive);
		}
		if (_proxyDarkBramble)
		{
			SceneManager.LoadScene("ProxyDarkBramble", LoadSceneMode.Additive);
		}
		if (_proxyComet)
		{
			SceneManager.LoadScene("ProxyComet", LoadSceneMode.Additive);
		}
		if (_proxyQuantumMoon)
		{
			SceneManager.LoadScene("ProxyQuantumMoon", LoadSceneMode.Additive);
		}
		if (_proxyWhiteHole)
		{
			SceneManager.LoadScene("ProxyWhiteHole", LoadSceneMode.Additive);
		}
		if (_proxyInvisiblePlanet)
		{
			SceneManager.LoadScene("ProxyRingWorld", LoadSceneMode.Additive);
		}
		if (!_timberHearth)
		{
			SpawnPoint[] componentsInChildren = GameObject.Find("SunStation_Body").GetComponentsInChildren<SpawnPoint>();
			SpawnPoint spawnPoint = null;
			SpawnPoint spawnPoint2 = null;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (spawnPoint == null && !componentsInChildren[i].IsShipSpawn())
				{
					spawnPoint = componentsInChildren[i];
				}
				if (spawnPoint2 == null && componentsInChildren[i].IsShipSpawn())
				{
					spawnPoint2 = componentsInChildren[i];
				}
			}
			GameObject gameObject = SpawnPrefab("Player/Player_Body");
			if ((bool)gameObject && (bool)spawnPoint)
			{
				gameObject.transform.position = spawnPoint.transform.position;
				gameObject.transform.rotation = spawnPoint.transform.rotation;
				gameObject.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(spawnPoint.GetAttachedOWRigidbody());
				gameObject.GetRequiredComponent<PlayerSpawner>().SetInitialSpawnPoint(spawnPoint);
			}
			GameObject gameObject2 = SpawnPrefab("Ship/Ship_Body");
			if ((bool)gameObject2 && (bool)spawnPoint2)
			{
				gameObject2.transform.position = spawnPoint2.transform.position;
				gameObject2.transform.rotation = spawnPoint2.transform.rotation;
				gameObject2.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(spawnPoint2.GetAttachedOWRigidbody());
			}
			SpawnPrefab("Player/Probe_Body");
		}
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
	}

	public Transform GetSolarSystemRoot()
	{
		return _solarSystemRoot;
	}

	public GameObject GetSun()
	{
		return _sun;
	}

	private GameObject SpawnPrefab(string path)
	{
		return null;
	}
}
