using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
	[SerializeField]
	private float _destroyDelay;

	private float _collideTime;

	private void Awake()
	{
		base.enabled = false;
	}

	private void Update()
	{
		if (Time.time > _collideTime + _destroyDelay)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		_collideTime = Time.time;
		base.enabled = true;
	}
}
