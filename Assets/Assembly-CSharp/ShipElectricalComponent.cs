using UnityEngine;

public class ShipElectricalComponent : ShipComponent
{
	[Space(10f)]
	[SerializeField]
	protected ElectricalSystem _electricalSystem;

	[SerializeField]
	protected ShipLogController _shipLogController;

	[SerializeField]
	protected LandingCamera _landingCamera;

	[SerializeField]
	protected float _disruptionImpactSpeed = 30f;

	[SerializeField]
	protected float _disruptionLength = 1f;

	[SerializeField]
	private OWAudioSource _audioSource;

	protected override void Awake()
	{
		base.Awake();
		GlobalMessenger.AddListener("ShipSystemFailure", OnSystemFailure);
	}

	private void Start()
	{
		_electricalSystem.SetPowered(powered: false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnSystemFailure);
	}

	protected override void OnComponentDamaged()
	{
		_electricalSystem.SetPowered(powered: false);
		_shipLogController.SetDamaged(damaged: true);
		_landingCamera.SetPowered(isPowered: false);
		if (_playerInShip)
		{
			_audioSource.PlayOneShot(AudioType.ShipDamageElectricalFailure);
		}
	}

	protected override void OnComponentRepaired()
	{
		if (_playerInShip)
		{
			_electricalSystem.SetPowered(powered: true);
			_electricalSystem.Disrupt(_disruptionLength);
		}
		_shipLogController.SetDamaged(damaged: false);
		_landingCamera.SetPowered(isPowered: true);
	}

	protected override void OnEnterShip()
	{
		base.OnEnterShip();
		if (!_damaged)
		{
			_electricalSystem.SetPowered(powered: true);
		}
	}

	protected override void OnExitShip()
	{
		base.OnExitShip();
		if (!_damaged)
		{
			_electricalSystem.SetPowered(powered: false);
		}
	}

	private void OnSystemFailure()
	{
		_electricalSystem.Disrupt(_disruptionLength);
		_electricalSystem.SetPowered(powered: false);
		_shipLogController.SetDamaged(damaged: true);
		_landingCamera.SetPowered(isPowered: false);
	}

	public override bool ApplyImpact(ImpactData impact)
	{
		if (impact.speed > _disruptionImpactSpeed)
		{
			_electricalSystem.Disrupt(_disruptionLength);
		}
		return base.ApplyImpact(impact);
	}
}
