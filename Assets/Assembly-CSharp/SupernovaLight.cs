using UnityEngine;

[RequireComponent(typeof(Light))]
public class SupernovaLight : MonoBehaviour
{
	private float _initExplosionTime;

	private float _initIntensity;

	private float _initRange;

	private void Awake()
	{
		GlobalMessenger.AddListener("SunExploded", OnSunExploded);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("SunExploded", OnSunExploded);
	}

	private void OnSunExploded()
	{
		base.enabled = true;
		_initExplosionTime = Time.time;
		_initIntensity = GetComponent<Light>().intensity;
		_initRange = GetComponent<Light>().range;
	}

	private void Update()
	{
		float t = (Time.time - _initExplosionTime) / 0.5f;
		GetComponent<Light>().intensity = Mathf.Lerp(_initIntensity, _initIntensity * 5f, t);
		GetComponent<Light>().range = Mathf.Lerp(_initRange, _initRange * 5f, t);
	}
}
