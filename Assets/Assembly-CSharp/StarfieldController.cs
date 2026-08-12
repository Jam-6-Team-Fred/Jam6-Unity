using System;
using Starfield;
using UnityEngine;

public class StarfieldController : MonoBehaviour
{
	[Serializable]
	private struct StarLookup
	{
		public int groupIndex;

		public int starIndex;

		public StarLookup(int groupIndex, int starIndex)
		{
			this.groupIndex = groupIndex;
			this.starIndex = starIndex;
		}
	}

	private const int _kParticlePoolSize = 32;

	[SerializeField]
	private StarfieldData _starfieldData;

	[SerializeField]
	private Renderer _starfieldRenderer;

	[SerializeField]
	private GameObject _supernovaPrefab;

	[Space]
	[SerializeField]
	private GameObject _eyeSupernovaPrefab;

	[SerializeField]
	private Transform _eyeSupernovaSpawnPos;

	[Space]
	[SerializeField]
	private Renderer _dreamStarfieldRenderer;

	private bool _firstUpdate;

	private int _lastAliveStarIndex;

	[SerializeField]
	[HideInInspector]
	private StarLookup[] _orderedStarIndices;

	private ParticleSystemPool _supernovaParticleSystems;

	private int _propID_GameTime;

	private GameObject _eyeSupernova;

	private float _spawnEyeSupernovaTime;

	private bool _eyeParticlesPaused;

	private bool _hasEyeSunExploded;

	private bool _playerInDreamWorld;

	private void Awake()
	{
		_firstUpdate = true;
		_lastAliveStarIndex = 0;
		_supernovaParticleSystems = new ParticleSystemPool(_supernovaPrefab, 32, base.transform);
		_propID_GameTime = Shader.PropertyToID("_GameTime");
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void Start()
	{
		if (_eyeSupernovaPrefab != null && Locator.GetEyeStateManager() != null)
		{
			SpawnEyeSupernova();
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void Update()
	{
		_supernovaParticleSystems.Update();
		float num = ((LoadManager.GetCurrentScene() == OWScene.TitleScreen) ? Time.timeSinceLevelLoad : TimeLoop.GetSecondsElapsed());
		while (_lastAliveStarIndex < _orderedStarIndices.Length)
		{
			StarInstance starInstance = _starfieldData.starGroups[_orderedStarIndices[_lastAliveStarIndex].groupIndex].stars[_orderedStarIndices[_lastAliveStarIndex].starIndex];
			if (starInstance.deathStartTime + starInstance.deathLength > num)
			{
				break;
			}
			if (starInstance.supernova && !_firstUpdate && !_playerInDreamWorld)
			{
				ParticleSystem particleSystem = _supernovaParticleSystems.Instantiate(base.transform, starInstance.position, Quaternion.LookRotation(starInstance.position));
				if (particleSystem != null)
				{
					ParticleSystem.MainModule main = particleSystem.main;
					main.startColor = main.startColor.color * starInstance.color;
				}
			}
			_lastAliveStarIndex++;
		}
		_starfieldRenderer.material.SetFloat(_propID_GameTime, num / 60f);
		if (_eyeSupernova != null)
		{
			UpdateEyeSupernova();
		}
		if (_firstUpdate)
		{
			_firstUpdate = false;
		}
	}

	private void SpawnEyeSupernova()
	{
		_eyeSupernova = UnityEngine.Object.Instantiate(_eyeSupernovaPrefab);
		_eyeSupernova.transform.parent = base.transform;
		_eyeSupernova.transform.position = _eyeSupernovaSpawnPos.position;
		ParticleSystem[] componentsInChildren = _eyeSupernova.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Play();
		}
		_spawnEyeSupernovaTime = Time.time;
	}

	private void UpdateEyeSupernova()
	{
		if (!_hasEyeSunExploded && TimeLoop.GetSecondsRemaining() <= 0f)
		{
			ParticleSystem[] componentsInChildren = _eyeSupernova.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Play();
			}
			_hasEyeSunExploded = true;
		}
		if (!_eyeParticlesPaused && !_hasEyeSunExploded && Time.time > _spawnEyeSupernovaTime + 0.5f)
		{
			ParticleSystem[] componentsInChildren2 = _eyeSupernova.GetComponentsInChildren<ParticleSystem>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].Pause();
			}
			_eyeParticlesPaused = true;
		}
	}

	private void OnEnterDreamWorld()
	{
		_starfieldRenderer.enabled = false;
		_dreamStarfieldRenderer.enabled = true;
		_supernovaParticleSystems.StopAndReturnAll();
		_playerInDreamWorld = true;
	}

	private void OnExitDreamWorld()
	{
		_starfieldRenderer.enabled = true;
		_dreamStarfieldRenderer.enabled = false;
		_playerInDreamWorld = false;
	}
}
