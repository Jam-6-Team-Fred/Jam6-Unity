using UnityEngine;

public class TempFogEnabler : MonoBehaviour
{
	[SerializeField]
	private PlanetaryFogController _fogController;

	[SerializeField]
	private OWTriggerVolume _triggerVolume;

	private void Awake()
	{
		_triggerVolume.OnEntry += OnEntry;
		_triggerVolume.OnExit += OnExit;
	}

	private void Start()
	{
		_fogController.enabled = false;
	}

	private void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEntry;
		_triggerVolume.OnExit -= OnExit;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_fogController.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_fogController.enabled = false;
		}
	}
}
