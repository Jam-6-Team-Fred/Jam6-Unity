using UnityEngine;

public class HideInMapView : MonoBehaviour
{
	private void Awake()
	{
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
	}

	private void Start()
	{
		if (base.gameObject.FindWithRequiredTag("MapCamera").GetComponent<OWCamera>().enabled)
		{
			GetComponent<Renderer>().enabled = false;
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
	}

	private void OnEnterMapView()
	{
		GetComponent<Renderer>().enabled = false;
	}

	private void OnExitMapView()
	{
		GetComponent<Renderer>().enabled = true;
	}
}
