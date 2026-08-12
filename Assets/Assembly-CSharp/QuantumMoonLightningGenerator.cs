using UnityEngine;

public class QuantumMoonLightningGenerator : CloudLightningGenerator
{
	[SerializeField]
	private float _minDegreesX;

	[SerializeField]
	private float _maxDegreesX;

	[SerializeField]
	private float _minDegreesZ;

	[SerializeField]
	private float _maxDegreesZ;

	protected override Vector3 GetLightningStartPosition()
	{
		float x = Random.Range(_minDegreesX, _maxDegreesX);
		float z = Random.Range(_minDegreesZ, _maxDegreesZ);
		return (Quaternion.Euler(x, 0f, z) * Vector3.up).normalized * _altitude;
	}
}
