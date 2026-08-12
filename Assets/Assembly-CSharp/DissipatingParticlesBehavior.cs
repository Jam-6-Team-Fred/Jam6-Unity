using UnityEngine;

public class DissipatingParticlesBehavior : MonoBehaviour
{
	private ParticleSystem _dissipatingParticles;

	private void Start()
	{
		_dissipatingParticles = base.gameObject.GetComponent<ParticleSystem>();
		_dissipatingParticles.Stop();
		_dissipatingParticles.Clear();
		GlobalMessenger.AddListener("SunExploded", OnSunExploded);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("SunExploded", OnSunExploded);
	}

	private void OnSunExploded()
	{
		_dissipatingParticles.Play();
	}
}
