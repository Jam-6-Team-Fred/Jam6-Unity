using UnityEngine;

public class ParticleSystemSelfDestruct : MonoBehaviour
{
	private ParticleSystem ps;

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		if ((bool)ps && ps.particleCount == 0)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
