using UnityEngine;

public class FogPillarController : MonoBehaviour
{
	[SerializeField]
	private bool _heads = true;

	private DayNightPlanetController _dayNightController;

	private ParticleSystem[] _particles;

	private float _playParticlesTime;

	private void Awake()
	{
		_dayNightController = GetComponentInParent<DayNightPlanetController>();
		_particles = GetComponentsInChildren<ParticleSystem>();
	}

	private void Update()
	{
		if (_dayNightController.IsDay(_heads) && Time.time > _playParticlesTime)
		{
			int num = Random.Range(0, _particles.Length);
			while (_particles[num].isPlaying)
			{
				num = Random.Range(0, _particles.Length);
			}
			_particles[num].Play();
			_playParticlesTime = Time.time + Random.Range(10f, 20f);
		}
	}
}
