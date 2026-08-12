using UnityEngine;

public class OceanSplasher : MonoBehaviour
{
	[SerializeField]
	private OceanEffectController _ocean;

	[SerializeField]
	private float _minSplashSpeed = 15f;

	[SerializeField]
	private float _splashRadius = 10f;

	[SerializeField]
	private float _splashLength = 5f;

	[SerializeField]
	private float _waveHeight = 1f;

	[SerializeField]
	private float _splashWidth = 1f;

	public float minSplashSpeed => _minSplashSpeed;

	public void Splash()
	{
		_ocean.CreateSplash(base.transform.position, _splashRadius, _splashLength, _waveHeight, _splashWidth);
	}
}
