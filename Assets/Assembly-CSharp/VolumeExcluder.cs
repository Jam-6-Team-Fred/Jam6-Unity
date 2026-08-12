using UnityEngine;

public class VolumeExcluder : MonoBehaviour
{
	[SerializeField]
	public ProximityTrigger _proximityTrigger;

	[SerializeField]
	public OWTriggerVolume _owTriggerVolume;

	public void SetEvents(ProximityTrigger.EnterProximityHandler OnEntry, ProximityTrigger.ExitProximityHandler OnExit, OWTriggerVolume.OWTriggerObjectEvent OnEntry2, OWTriggerVolume.OWTriggerObjectEvent OnExit2)
	{
		if (_proximityTrigger != null)
		{
			_proximityTrigger.AddListeners(OnEntry, OnExit);
		}
		if (_owTriggerVolume != null)
		{
			_owTriggerVolume.OnEntry += OnEntry2;
			_owTriggerVolume.OnExit += OnExit2;
		}
	}

	public void RemoveEvents(ProximityTrigger.EnterProximityHandler OnEntry, ProximityTrigger.ExitProximityHandler OnExit, OWTriggerVolume.OWTriggerObjectEvent OnEntry2, OWTriggerVolume.OWTriggerObjectEvent OnExit2)
	{
		if (_proximityTrigger != null)
		{
			_proximityTrigger.RemoveListeners(OnEntry, OnExit);
		}
		if (_owTriggerVolume != null)
		{
			_owTriggerVolume.OnEntry -= OnEntry2;
			_owTriggerVolume.OnExit -= OnExit2;
		}
	}
}
