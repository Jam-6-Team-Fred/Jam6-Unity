using System;
using UnityEngine;

public class MusicStreamParticleController : MonoBehaviour
{
	[SerializeField]
	private TravelerEyeController _travelerController;

	[SerializeField]
	private float _offsetDegrees;

	private ParticleSystem _particleSystem;

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		_travelerController.OnStartPlaying += OnStartPlaying;
		_travelerController.OnStopPlaying += OnStopPlaying;
		base.enabled = false;
	}

	private void OnStartPlaying()
	{
		_particleSystem.Play();
		base.enabled = true;
	}

	private void OnStopPlaying()
	{
		_particleSystem.Stop();
		base.enabled = false;
	}

	private void Update()
	{
		float num = 90f;
		float num2 = Time.time * num % 360f + _offsetDegrees;
		float f = (float)Math.PI / 180f * num2;
		float x = Mathf.Cos(f);
		float y = Mathf.Sin(f);
		Vector3 toDirection = Vector3.forward + new Vector3(x, y, 0f) * 0.5f;
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, toDirection);
		base.transform.localRotation = quaternion * Quaternion.identity;
	}
}
