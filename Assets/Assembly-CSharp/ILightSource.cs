using UnityEngine;

public interface ILightSource
{
	LightSourceType GetLightSourceType();

	bool CheckIlluminationAtPoint(Vector3 point, float buffer, float maxDistance);

	OWLight2[] GetLights();
}
