using UnityEngine;

public class PunctureController : MonoBehaviour
{
	private ParticleSystem _particleSystem;

	private bool _isPunctured;

	private float _puncturePatchTime;

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		GlobalMessenger.AddListener("SuitUp", OnSuitUp);
		GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
		base.gameObject.SetActive(value: false);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
		GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
	}

	private void Update()
	{
		if (!_isPunctured && Time.time >= _puncturePatchTime)
		{
			if (_particleSystem != null)
			{
				_particleSystem.Stop();
			}
			base.enabled = false;
		}
	}

	public void StartPuncture()
	{
		if (_particleSystem != null)
		{
			_particleSystem.Play();
		}
		_isPunctured = true;
	}

	public void StopPuncture(float delay = 0f)
	{
		_isPunctured = false;
		_puncturePatchTime = Time.time + delay;
		base.enabled = true;
	}

	private void OnSuitUp()
	{
		base.gameObject.SetActive(value: true);
		if (_particleSystem != null && _isPunctured)
		{
			_particleSystem.Play();
		}
	}

	private void OnRemoveSuit()
	{
		base.gameObject.SetActive(value: false);
	}

	public bool IsPunctured()
	{
		return _isPunctured;
	}
}
