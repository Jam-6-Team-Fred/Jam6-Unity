using UnityEngine;

public class HullBreachAudioController : SectoredMonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _rushingAirVolume;

	[SerializeField]
	private AbstractGhostDoorInterface _interface;

	[SerializeField]
	private RotatingDoor _door;

	[SerializeField]
	private OWAudioSource _loopingSource;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	private bool _open;

	protected override void Awake()
	{
		base.Awake();
		_interface.OnOpen += OnOpen;
		_door.OnCloseFinish += new OWEvent.OWCallback(OnCloseFinish);
	}

	private void Start()
	{
		_loopingSource.SetLocalVolume(0f);
		_rushingAirVolume.SetTriggerActivation(active: false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_interface.OnOpen -= OnOpen;
		_door.OnCloseFinish -= new OWEvent.OWCallback(OnCloseFinish);
	}

	private void OnOpen()
	{
		if (!_open)
		{
			_open = true;
			_rushingAirVolume.SetTriggerActivation(active: true);
			_oneShotSource.PlayOneShot(AudioType.Airlock_Depressurize);
			_loopingSource.FadeIn(1f);
		}
	}

	private void OnCloseFinish()
	{
		_open = false;
		_loopingSource.FadeOut(0.2f);
		_oneShotSource.PlayOneShot(AudioType.Airlock_Pressurize);
		_rushingAirVolume.SetTriggerActivation(active: false);
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (_open && _sector.ContainsOccupant(DynamicOccupant.Player))
		{
			_loopingSource.FadeIn(1f);
		}
		else
		{
			_loopingSource.FadeOut(1f);
		}
	}
}
