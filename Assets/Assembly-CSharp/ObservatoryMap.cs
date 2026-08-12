using UnityEngine;

public class ObservatoryMap : MonoBehaviour
{
	private SingleInteractionVolume _interactVolume;

	private void Awake()
	{
		_interactVolume = this.GetRequiredComponent<SingleInteractionVolume>();
		if (_interactVolume != null)
		{
			_interactVolume.OnPressInteract += OnPressInteract;
		}
	}

	private void OnDestroy()
	{
		if (_interactVolume != null)
		{
			_interactVolume.OnPressInteract -= OnPressInteract;
		}
	}

	private void OnPressInteract()
	{
		GlobalMessenger.FireEvent("TriggerObservatoryMap");
		_interactVolume.ResetInteraction();
	}
}
