using UnityEngine;

[RequireComponent(typeof(ImpactSensor))]
public class ShipDamageController : MonoBehaviour
{
	public delegate void ShipDamageEvent();

	private const float _lightImpactMinSpeed = 5f;

	public const float _lightImpactMaxSpeed = 15f;

	public const float _hullDamageMinSpeed = 30f;

	public const float _hullDamageMaxSpeed = 200f;

	private const float _explodeOnImpactSpeed = 300f;

	[Space(10f)]
	[SerializeField]
	private ExplosionController _explosion;

	[SerializeField]
	private OWTriggerVolume[] _shipTriggerVolumes;

	[SerializeField]
	private GameObject[] _stencils = new GameObject[0];

	private OWRigidbody _shipBody;

	private ImpactSensor _impactSensor;

	private ShipAudioController _audioController;

	private ShipModule[] _shipModules;

	private ShipHull[] _shipHulls;

	private ShipComponent[] _shipComponents;

	private ShipElectricalComponent _shipElectricalComponent;

	private ShipReactorComponent _shipReactorComponent;

	private ShipGravityComponent _shipGravityComponent;

	private bool _cockpitDetached;

	private bool _hullBreach;

	private bool _systemFailure;

	private bool _exploded;

	private bool _invincible;

	public event ShipDamageEvent OnDamageUpdated;

	public event ShipHull.DamageEvent OnShipHullDamaged;

	public event ShipHull.DamageEvent OnShipHullRepaired;

	public event ShipComponent.DamageEvent OnShipComponentDamaged;

	public event ShipComponent.DamageEvent OnShipComponentRepaired;

	private void Awake()
	{
		_shipBody = this.GetAttachedOWRigidbody();
		_impactSensor = GetComponent<ImpactSensor>();
		_shipModules = GetComponentsInChildren<ShipModule>();
		_shipHulls = GetComponentsInChildren<ShipHull>();
		_shipComponents = GetComponentsInChildren<ShipComponent>();
		_cockpitDetached = false;
		_hullBreach = false;
		_systemFailure = false;
		_exploded = false;
		_audioController = _shipBody.GetComponentInChildren<ShipAudioController>();
		if (_shipTriggerVolumes.Length == 0)
		{
			_shipTriggerVolumes = _shipBody.GetComponentsInChildren<OWTriggerVolume>();
		}
		_impactSensor.OnImpact += OnImpact;
		for (int i = 0; i < _shipModules.Length; i++)
		{
			if (_shipModules[i] is ShipDetachableModule)
			{
				(_shipModules[i] as ShipDetachableModule).OnModuleDetach += OnModuleDetach;
			}
		}
		for (int j = 0; j < _shipHulls.Length; j++)
		{
			_shipHulls[j].OnDamaged += OnHullDamaged;
			_shipHulls[j].OnRepaired += OnHullRepaired;
			_shipHulls[j].OnImpact += OnHullImpact;
		}
		for (int k = 0; k < _shipComponents.Length; k++)
		{
			_shipComponents[k].OnDamaged += OnComponentDamaged;
			_shipComponents[k].OnRepaired += OnComponentRepaired;
			if (_shipComponents[k] is ShipElectricalComponent)
			{
				_shipElectricalComponent = _shipComponents[k] as ShipElectricalComponent;
			}
			else if (_shipComponents[k] is ShipReactorComponent)
			{
				_shipReactorComponent = _shipComponents[k] as ShipReactorComponent;
			}
			else if (_shipComponents[k] is ShipGravityComponent)
			{
				_shipGravityComponent = _shipComponents[k] as ShipGravityComponent;
			}
		}
	}

	private void OnDestroy()
	{
		_impactSensor.OnImpact -= OnImpact;
		for (int i = 0; i < _shipModules.Length; i++)
		{
			if (_shipModules[i] is ShipDetachableModule)
			{
				(_shipModules[i] as ShipDetachableModule).OnModuleDetach -= OnModuleDetach;
			}
		}
		for (int j = 0; j < _shipHulls.Length; j++)
		{
			_shipHulls[j].OnDamaged -= OnHullDamaged;
			_shipHulls[j].OnRepaired -= OnHullRepaired;
			_shipHulls[j].OnImpact -= OnHullImpact;
		}
		for (int k = 0; k < _shipComponents.Length; k++)
		{
			_shipComponents[k].OnDamaged -= OnComponentDamaged;
			_shipComponents[k].OnRepaired -= OnComponentRepaired;
		}
	}

	public bool IsDamaged()
	{
		for (int i = 0; i < _shipHulls.Length; i++)
		{
			if (_shipHulls[i].isDamaged)
			{
				return true;
			}
		}
		for (int j = 0; j < _shipComponents.Length; j++)
		{
			if (_shipComponents[j].isDamaged)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCockpitDetached()
	{
		return _cockpitDetached;
	}

	public bool IsHullBreached()
	{
		return _hullBreach;
	}

	public bool IsSystemFailed()
	{
		return _systemFailure;
	}

	public bool IsDestroyed()
	{
		if (!_hullBreach)
		{
			return _systemFailure;
		}
		return true;
	}

	public bool IsElectricalFailed()
	{
		if (_shipElectricalComponent != null)
		{
			return _shipElectricalComponent.isDamaged;
		}
		return false;
	}

	public bool IsReactorCritical()
	{
		if (_shipReactorComponent != null)
		{
			return _shipReactorComponent.isDamaged;
		}
		return false;
	}

	public float GetLowestHullIntegrity()
	{
		float num = 1f;
		for (int i = 0; i < _shipHulls.Length; i++)
		{
			if (_shipHulls[i].integrity < num)
			{
				num = _shipHulls[i].integrity;
			}
		}
		return num;
	}

	public void DebugDamageShip(UITextType textType)
	{
		switch (textType)
		{
		case UITextType.ShipPartTop:
		case UITextType.ShipPartForward:
		case UITextType.ShipPartPort:
		case UITextType.ShipPartLanding:
		case UITextType.ShipPartStarboard:
		case UITextType.ShipPartAft:
		{
			for (int j = 0; j < _shipHulls.Length; j++)
			{
				if (_shipHulls[j].hullName == textType)
				{
					if (!_shipHulls[j].isDamaged)
					{
						_shipHulls[j].ApplyDebugImpact();
					}
					break;
				}
			}
			break;
		}
		case UITextType.ShipPartAutopilot:
		case UITextType.ShipPartFuel:
		case UITextType.ShipPartGravity:
		case UITextType.ShipPartLights:
		case UITextType.ShipPartCamera:
		case UITextType.ShipPartLeftThrust:
		case UITextType.ShipPartElectric:
		case UITextType.ShipPartO2:
		case UITextType.ShipPartReactor:
		case UITextType.ShipPartRightThrust:
		{
			for (int i = 0; i < _shipComponents.Length; i++)
			{
				if (_shipComponents[i].componentName == textType)
				{
					if (!_shipComponents[i].isDamaged)
					{
						_shipComponents[i].SetDamaged(damaged: true);
					}
					break;
				}
			}
			break;
		}
		}
	}

	public void TriggerElectricalFailure(bool debug = false)
	{
		if ((!_invincible || debug) && _shipElectricalComponent != null)
		{
			_shipElectricalComponent.SetDamaged(damaged: true);
		}
	}

	public void TriggerReactorCritical(bool debug = false)
	{
		if ((!_invincible || debug) && _shipReactorComponent != null)
		{
			_shipReactorComponent.SetDamaged(damaged: true);
		}
	}

	public void TriggerHullBreach(bool debug = false)
	{
		if (!_hullBreach && (!_invincible || debug))
		{
			if (PlayerState.IsInsideShip() && Locator.GetPlayerSuit().IsWearingSuit() && !Locator.GetPlayerSuit().IsWearingHelmet())
			{
				Locator.GetPlayerSuit().PutOnHelmet();
			}
			_hullBreach = true;
			_shipGravityComponent.SetDamaged(damaged: true);
			NotificationData data = new NotificationData(NotificationTarget.Player, UITextLibrary.GetString(UITextType.NotificationHullBreak));
			NotificationManager.SharedInstance.PostNotification(data);
			for (int i = 0; i < _shipTriggerVolumes.Length; i++)
			{
				_shipTriggerVolumes[i].SetTriggerActivation(active: false);
			}
			for (int j = 0; j < _stencils.Length; j++)
			{
				_stencils[j].SetActive(value: false);
			}
			GlobalMessenger.FireEvent("ShipHullBreach");
		}
	}

	public void TriggerSystemFailure(bool debug = false)
	{
		if (!_systemFailure && (!_invincible || debug))
		{
			Locator.GetShipBody().GetComponentInChildren<ShipDirectionalForceVolume>().SetVolumeActivation(active: false);
			for (int i = 0; i < _shipHulls.Length; i++)
			{
				_shipHulls[i].TriggerSystemFailure();
			}
			for (int j = 0; j < _shipComponents.Length; j++)
			{
				_shipComponents[j].TriggerSystemFailure();
			}
			_systemFailure = true;
			GlobalMessenger.FireEvent("ShipSystemFailure");
		}
	}

	public void Explode(bool debug = false)
	{
		if (_exploded || (_invincible && !debug))
		{
			return;
		}
		for (int i = 0; i < _shipModules.Length; i++)
		{
			if (_shipModules[i] is ShipDetachableModule)
			{
				(_shipModules[i] as ShipDetachableModule).Detach();
			}
			else if (_shipModules[i] is ShipLandingModule)
			{
				(_shipModules[i] as ShipLandingModule).DetachAllLegs();
			}
		}
		if (_explosion != null)
		{
			_explosion.Play();
		}
		TriggerHullBreach();
		TriggerSystemFailure();
		_exploded = true;
	}

	private void OnImpact(ImpactData impact)
	{
		if (impact.otherCollider.attachedRigidbody != null && impact.otherCollider.attachedRigidbody.CompareTag("Player") && PlayerState.IsInsideShip())
		{
			return;
		}
		if (impact.speed >= 300f && !_exploded)
		{
			Explode();
		}
		else if (!_invincible)
		{
			for (int i = 0; i < _shipModules.Length; i++)
			{
				_shipModules[i].ApplyImpact(impact);
			}
		}
	}

	private void OnHullImpact(ImpactData impact, float damage)
	{
		if (damage > 0.2f)
		{
			_audioController.PlayImpactAtPosition(AudioType.ShipImpact_HeavyDamage, 1f, Random.Range(0.7f, 0.9f), impact.point);
			if (PlayerState.IsInsideShip() && (PlayerState.IsAttached() || Locator.GetPlayerController().IsGrounded()))
			{
				RumbleManager.PulseHeavyImpact();
			}
		}
		else if (damage > 0f)
		{
			AudioType audioType = AudioType.ShipImpact_LightDamage;
			_audioController.PlayImpactAtPosition(audioType, 0.8f, Random.Range(0.9f, 1.1f), impact.point);
			if (PlayerState.IsInsideShip() && (PlayerState.IsAttached() || Locator.GetPlayerController().IsGrounded()))
			{
				RumbleManager.PulseMediumImpact();
			}
		}
		else if (impact.speed >= 5f)
		{
			float volume = 0.3f * Mathf.InverseLerp(5f, 15f, impact.speed);
			_audioController.PlayImpactAtPosition(AudioType.ShipImpact_NoDamage, volume, Random.Range(0.9f, 1.1f), impact.point);
			if (PlayerState.IsInsideShip() && (PlayerState.IsAttached() || Locator.GetPlayerController().IsGrounded()))
			{
				RumbleManager.PulseLightImpact();
			}
		}
	}

	private void OnModuleDetach(ShipDetachableModule module)
	{
		if (module.CompareTag("ShipCockpit"))
		{
			_cockpitDetached = true;
			GlobalMessenger<OWRigidbody>.FireEvent("ShipCockpitDetached", module.GetAttachedOWRigidbody());
		}
		TriggerHullBreach();
		TriggerSystemFailure();
	}

	private void OnHullDamaged(ShipHull shipHull)
	{
		if (this.OnShipHullDamaged != null)
		{
			this.OnShipHullDamaged(shipHull);
		}
		if (this.OnDamageUpdated != null)
		{
			this.OnDamageUpdated();
		}
	}

	private void OnHullRepaired(ShipHull shipHull)
	{
		if (this.OnShipHullRepaired != null)
		{
			this.OnShipHullRepaired(shipHull);
		}
		if (this.OnDamageUpdated != null)
		{
			this.OnDamageUpdated();
		}
	}

	private void OnComponentRepaired(ShipComponent shipComponent)
	{
		if (this.OnShipComponentRepaired != null)
		{
			this.OnShipComponentRepaired(shipComponent);
		}
		if (this.OnDamageUpdated != null)
		{
			this.OnDamageUpdated();
		}
	}

	private void OnComponentDamaged(ShipComponent shipComponent)
	{
		if (this.OnShipComponentDamaged != null)
		{
			this.OnShipComponentDamaged(shipComponent);
		}
		if (this.OnDamageUpdated != null)
		{
			this.OnDamageUpdated();
		}
	}

	public void ToggleInvincibility()
	{
		_invincible = !_invincible;
	}

	public bool ShouldExitShipToRepair()
	{
		for (int i = 0; i < _shipHulls.Length; i++)
		{
			if (_shipHulls[i].isDamaged)
			{
				return true;
			}
		}
		for (int j = 0; j < _shipComponents.Length; j++)
		{
			if (_shipComponents[j].isDamaged && _shipComponents[j].componentName != UITextType.ShipPartGravity && _shipComponents[j].componentName != UITextType.ShipPartReactor)
			{
				return true;
			}
		}
		return false;
	}
}
