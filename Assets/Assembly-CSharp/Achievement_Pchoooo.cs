using UnityEngine;

public class Achievement_Pchoooo : MonoBehaviour
{
	[SerializeField]
	private EffectVolume _gravityVolume;

	[SerializeField]
	private OWTriggerVolume _triggerVolume;

	private bool _shipInVolume;

	private void Start()
	{
		_gravityVolume.OnActivate += OnEffectVolumeActivated;
		_triggerVolume.OnEntry += OnEntry;
		_triggerVolume.OnExit += OnExit;
		_shipInVolume = false;
	}

	protected virtual void OnDestroy()
	{
		_gravityVolume.OnActivate -= OnEffectVolumeActivated;
	}

	protected void OnEffectVolumeActivated(bool active)
	{
		if (active && _shipInVolume)
		{
			Achievements.Earn(Achievements.Type.PCHOOOOOOO);
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("ShipDetector"))
		{
			_shipInVolume = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("ShipDetector"))
		{
			_shipInVolume = false;
		}
	}
}
