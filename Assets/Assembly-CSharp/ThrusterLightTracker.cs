using UnityEngine;

public class ThrusterLightTracker : MonoBehaviour
{
	[SerializeField]
	private Light[] _thrusterLights;

	[SerializeField]
	private float _buffer = 2f;

	private float _lightRange;

	public float GetLightRange()
	{
		return _lightRange;
	}

	public bool CheckIlluminationAtPoint(Vector3 point, float buffer = 0f, float maxDistance = float.PositiveInfinity)
	{
		float num = Vector3.Distance(point, base.transform.position) - buffer;
		if (_lightRange > 0f && num < _lightRange)
		{
			return num <= maxDistance;
		}
		return false;
	}

	private void LateUpdate()
	{
		_lightRange = 0f;
		for (int i = 0; i < _thrusterLights.Length; i++)
		{
			if (_thrusterLights[i].enabled && _thrusterLights[i].intensity > 0f)
			{
				_lightRange += _thrusterLights[i].range;
			}
		}
		if (_lightRange > 0f)
		{
			_lightRange += _buffer;
		}
	}
}
