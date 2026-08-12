using UnityEngine;

[RequireComponent(typeof(PrefabSpawner))]
public class MiniGalaxyController : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _killTrigger;

	[SerializeField]
	private OWTriggerVolume _expandLightsTrigger;

	[SerializeField]
	private OWAudioSource _musicSource;

	[SerializeField]
	private OWAudioSource _forestAmbience;

	[SerializeField]
	private OWAudioSource _desolateAmbience;

	private PrefabSpawner _spawner;

	private MiniGalaxy[] _galaxies;

	private float _forestIsDarkTime;

	private bool _fadingMusicOut;

	private void Awake()
	{
		_spawner = base.gameObject.GetRequiredComponent<PrefabSpawner>();
		_killTrigger.OnEntry += OnEnterKillTrigger;
		_expandLightsTrigger.OnEntry += OnEnterExpandLightsTrigger;
		_spawner.Spawn();
		_galaxies = GetComponentsInChildren<MiniGalaxy>(includeInactive: true);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_killTrigger.OnEntry -= OnEnterKillTrigger;
		_expandLightsTrigger.OnEntry -= OnEnterExpandLightsTrigger;
	}

	public void TurnOnGalaxies()
	{
		for (int i = 0; i < _galaxies.Length; i++)
		{
			_galaxies[i].AppearAfterSeconds(Random.Range(4f, 14f));
		}
	}

	public void KillGalaxies()
	{
		float num = 60f;
		_galaxies = GetComponentsInChildren<MiniGalaxy>(includeInactive: true);
		for (int i = 0; i < _galaxies.Length; i++)
		{
			_galaxies[i].DieAfterSeconds(Random.Range(30f, num), playDeathParticles: true, AudioType.EyeGalaxyBlowAway);
		}
		_forestIsDarkTime = Time.time + num + 5f;
		base.enabled = true;
	}

	private void Update()
	{
		if (Locator.GetEyeStateManager().GetState() == EyeState.ForestOfGalaxies)
		{
			if (!_fadingMusicOut && Time.time > _forestIsDarkTime - 10f)
			{
				_musicSource.FadeOut(10f);
				_forestAmbience.FadeOut(10f);
				_desolateAmbience.SetLocalVolume(0f);
				_desolateAmbience.FadeIn(10f);
				_fadingMusicOut = true;
			}
			else if (Time.time > _forestIsDarkTime)
			{
				Locator.GetEyeStateManager().SetState(EyeState.ForestIsDark);
				base.enabled = false;
			}
		}
	}

	private void OnEnterKillTrigger(GameObject obj)
	{
		if (obj.CompareTag("PlayerDetector") && Locator.GetEyeStateManager().GetState() == EyeState.ForestOfGalaxies)
		{
			_killTrigger.OnEntry -= OnEnterKillTrigger;
			KillGalaxies();
			_musicSource.SetLocalVolume(0f);
			_musicSource.FadeIn(5f);
		}
	}

	private void OnEnterExpandLightsTrigger(GameObject obj)
	{
		if (obj.CompareTag("PlayerDetector") && Locator.GetEyeStateManager().GetState() == EyeState.ForestOfGalaxies)
		{
			_forestAmbience.SetLocalVolume(0f);
			_forestAmbience.FadeIn(5f);
			_expandLightsTrigger.OnEntry -= OnEnterExpandLightsTrigger;
			for (int i = 0; i < _galaxies.Length; i++)
			{
				_galaxies[i].ExpandLightRange();
			}
		}
	}
}
