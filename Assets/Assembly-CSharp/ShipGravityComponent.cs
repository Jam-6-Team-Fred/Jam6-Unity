using UnityEngine;

public class ShipGravityComponent : ShipComponent
{
	[SerializeField]
	private OWAudioSource _gravityAudio;

	private ShipDirectionalForceVolume _gravityVolume;

	protected void Start()
	{
		_gravityVolume = Locator.GetShipBody().GetComponentInChildren<ShipDirectionalForceVolume>();
		_gravityAudio.SetLocalVolume(0f);
	}

	protected override void OnComponentDamaged()
	{
		_gravityVolume.SetVolumeActivation(active: false);
		_gravityAudio.FadeOut(0.5f);
	}

	protected override void OnComponentRepaired()
	{
		_gravityVolume.SetVolumeActivation(active: true);
		_gravityAudio.FadeIn(0.5f);
		if (PlayerState.IsInsideShip())
		{
			_gravityVolume.GetOWTriggerVolume().AddObjectToVolume(Locator.GetPlayerDetector());
			_gravityVolume.GetOWTriggerVolume().AddObjectToVolume(Locator.GetPlayerCameraDetector());
		}
	}

	protected override void OnEnterShip()
	{
		base.OnEnterShip();
		if (!_damaged)
		{
			_gravityAudio.FadeIn(1f);
		}
	}

	protected override void OnExitShip()
	{
		base.OnExitShip();
		_gravityAudio.FadeOut(1f);
	}
}
