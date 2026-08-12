using UnityEngine;

public class MapSatelliteBlinkingLight : MonoBehaviour
{
	[Tooltip("Particle system shown in 1st person")]
	[SerializeField]
	private ParticleSystem _firstPersonParticleSystem;

	[Tooltip("Particle system shown in map")]
	[SerializeField]
	private ParticleSystem _mapParticleSystem;

	private void Awake()
	{
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
	}

	private void Start()
	{
		DetermineParticleSystem(isMap: false);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
	}

	private void Update()
	{
	}

	private void OnEnterMapView()
	{
		DetermineParticleSystem(isMap: true);
	}

	private void OnExitMapView()
	{
		DetermineParticleSystem(isMap: false);
	}

	private void DetermineParticleSystem(bool isMap)
	{
		if (isMap)
		{
			_firstPersonParticleSystem.Stop();
			_firstPersonParticleSystem.Clear();
			_mapParticleSystem.Play();
		}
		else
		{
			_mapParticleSystem.Stop();
			_mapParticleSystem.Clear();
			_firstPersonParticleSystem.Play();
		}
	}
}
