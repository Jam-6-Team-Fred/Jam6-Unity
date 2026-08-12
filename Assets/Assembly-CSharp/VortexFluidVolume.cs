using UnityEngine;

public class VortexFluidVolume : FluidVolume
{
	private ParticleSystem _particleSystem;

	protected override void Awake()
	{
		base.Awake();
		_particleSystem = base.gameObject.GetComponentInChildren<ParticleSystem>();
	}

	public void Update()
	{
		ParticleSystem.Particle[] array = new ParticleSystem.Particle[_particleSystem.particleCount];
		int particles = _particleSystem.GetParticles(array);
		for (int num = array.Length - 1; num >= 0; num--)
		{
			Vector3 worldPosition = _particleSystem.transform.TransformPoint(array[num].position);
			array[num].position += _particleSystem.transform.InverseTransformDirection(GetRelativePointFluidVelocity(worldPosition) * Time.deltaTime);
		}
		_particleSystem.SetParticles(array, particles);
	}

	private Vector3 GetRelativePointFluidVelocity(Vector3 worldPosition)
	{
		Vector3 zero = Vector3.zero;
		Vector3 vector = base.transform.InverseTransformPoint(worldPosition);
		float x = vector.x + vector.z;
		float y = 0f - vector.y;
		float z = vector.z - vector.x;
		Vector3 direction = new Vector3(x, y, z) / 5f;
		return zero + base.transform.TransformDirection(direction);
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		Vector3 vector = base.transform.InverseTransformPoint(worldPosition);
		float x = vector.x + vector.z;
		float y = 0f - vector.y;
		float z = vector.z - vector.x;
		Vector3 direction = new Vector3(x, y, z) / 5f;
		return pointVelocity + base.transform.TransformDirection(direction);
	}
}
