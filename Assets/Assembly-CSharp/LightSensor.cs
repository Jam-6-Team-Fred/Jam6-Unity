using UnityEngine;

public abstract class LightSensor : MonoBehaviour
{
	public OWEvent OnDetectLight = new OWEvent(16);

	public OWEvent OnDetectDarkness = new OWEvent(16);

	public abstract bool IsIlluminated();

	public abstract bool IsIlluminatedByGhostLantern();

	public abstract bool IsIlluminatedByLantern(DreamLanternController lantern);
}
