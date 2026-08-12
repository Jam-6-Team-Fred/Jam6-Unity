using UnityEngine;

public class SectorSunOverride : SectoredMonoBehaviour
{
	private Light _sun;

	private Light _sunAmbient;

	[SerializeField]
	private OWTriggerVolume _exclusionVolume;

	private bool _playerInSector;

	private bool _playerInExclusionVolume;

	protected override void Awake()
	{
		base.Awake();
		if (_exclusionVolume != null)
		{
			_exclusionVolume.OnEntry += OnExclusionVolumeEntry;
			_exclusionVolume.OnExit += OnExclusionVolumeExit;
		}
	}

	private void Start()
	{
		Light[] componentsInChildren = Locator.GetSunTransform().GetComponentsInChildren<Light>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].type == LightType.Directional)
			{
				_sun = componentsInChildren[i];
				break;
			}
		}
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (componentsInChildren[j].type == LightType.Point)
			{
				_sunAmbient = componentsInChildren[j];
				break;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_exclusionVolume != null)
		{
			_exclusionVolume.OnEntry -= OnExclusionVolumeEntry;
			_exclusionVolume.OnExit -= OnExclusionVolumeExit;
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		_playerInSector = _sector.ContainsOccupant(DynamicOccupant.Player);
		UpdateSunActive();
	}

	private void OnExclusionVolumeEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInExclusionVolume = true;
			UpdateSunActive();
		}
	}

	private void OnExclusionVolumeExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInExclusionVolume = false;
			UpdateSunActive();
		}
	}

	private void UpdateSunActive()
	{
		_sun.enabled = !_playerInSector || _playerInExclusionVolume;
		_sunAmbient.enabled = !_playerInSector || _playerInExclusionVolume;
	}
}
