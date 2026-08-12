using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
	[SerializeField]
	private float _secondsUntilSelfDestruct = 1f;

	private float _spawnTime;

	private void Start()
	{
		_spawnTime = Time.time;
	}

	public void SetDelay(float delaySeconds)
	{
		_secondsUntilSelfDestruct = delaySeconds;
	}

	private void Update()
	{
		if (Time.time > _spawnTime + _secondsUntilSelfDestruct)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
