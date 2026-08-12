using UnityEngine;

public class ShipCameraComponent : ShipComponent
{
	[Space(10f)]
	[SerializeField]
	private LandingCamera _landingCamera;

	protected override void OnComponentDamaged()
	{
		_landingCamera.SetDamaged(isDamaged: true);
	}

	protected override void OnComponentRepaired()
	{
		_landingCamera.SetDamaged(isDamaged: false);
	}
}
