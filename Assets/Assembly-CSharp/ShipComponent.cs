using UnityEngine;

public class ShipComponent : MonoBehaviour
{
	public delegate void DamageEvent(ShipComponent shipComponent);

	[SerializeField]
	protected UITextType _componentName = UITextType.SatelliteNodeName;

	[SerializeField]
	protected DamageEffect _damageEffect;

	[SerializeField]
	protected float _repairTime = 3f;

	[SerializeField]
	protected AnimationCurve _damageProbabilityCurve = GetDefaultProbabilityCurve();

	[Space(10f)]
	[SerializeField]
	protected Collider _repairCollider;

	[SerializeField]
	protected bool _persistentCollider;

	[SerializeField]
	protected bool _internalRepairPoint;

	protected bool _damaged;

	protected float _repairFraction;

	protected RepairReceiver _repairReceiver;

	protected bool _playerInShip;

	public UITextType componentName => _componentName;

	public bool isInternalRepairPoint => _internalRepairPoint;

	public bool isDamaged => _damaged;

	public float repairFraction => _repairFraction;

	public event DamageEvent OnDamaged;

	public event DamageEvent OnRepaired;

	private static AnimationCurve GetDefaultProbabilityCurve()
	{
		return new AnimationCurve(new Keyframe(0f, 0f, float.PositiveInfinity, float.PositiveInfinity), new Keyframe(30f, 0.33f, float.PositiveInfinity, float.PositiveInfinity), new Keyframe(100f, 0.5f, float.PositiveInfinity, float.PositiveInfinity), new Keyframe(200f, 1f, float.PositiveInfinity, float.PositiveInfinity))
		{
			preWrapMode = WrapMode.ClampForever,
			postWrapMode = WrapMode.ClampForever
		};
	}

	protected virtual void Awake()
	{
		_damaged = false;
		_repairFraction = 1f;
		_repairReceiver = _repairCollider.gameObject.AddComponent<RepairReceiver>();
		_repairReceiver.type = RepairReceiver.Type.ShipComponent;
		_repairReceiver.targetComponent = this;
		_repairReceiver.repairDistance = 3f;
		_repairFraction = 1f;
		_playerInShip = false;
		UpdateColliderState();
		GlobalMessenger.AddListener("EnterShip", OnEnterShip);
		GlobalMessenger.AddListener("ExitShip", OnExitShip);
	}

	protected virtual void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterShip", OnEnterShip);
		GlobalMessenger.RemoveListener("ExitShip", OnExitShip);
	}

	public void SetDamaged(bool damaged)
	{
		if (_damaged == damaged)
		{
			return;
		}
		if (damaged)
		{
			_damaged = true;
			_repairFraction = 0f;
			OnComponentDamaged();
			if (this.OnDamaged != null)
			{
				this.OnDamaged(this);
			}
		}
		else
		{
			_damaged = false;
			_repairFraction = 1f;
			OnComponentRepaired();
			if (this.OnRepaired != null)
			{
				this.OnRepaired(this);
			}
		}
		UpdateColliderState();
		if ((bool)_damageEffect)
		{
			_damageEffect.SetEffectBlend(1f - _repairFraction);
		}
	}

	public virtual bool ApplyImpact(ImpactData impact)
	{
		if (_damaged)
		{
			return false;
		}
		if (Random.value < _damageProbabilityCurve.Evaluate(impact.speed))
		{
			SetDamaged(damaged: true);
			return true;
		}
		return false;
	}

	public void RepairTick()
	{
		if (_damaged)
		{
			_repairFraction = Mathf.Clamp01(_repairFraction + Time.deltaTime / _repairTime);
			if (_repairFraction >= 1f)
			{
				SetDamaged(damaged: false);
			}
			if ((bool)_damageEffect)
			{
				_damageEffect.SetEffectBlend(1f - _repairFraction);
			}
		}
	}

	public void TriggerSystemFailure()
	{
		if (_damageEffect != null)
		{
			_damageEffect.DisableActiveEffects();
		}
	}

	protected void UpdateColliderState()
	{
		if (!_persistentCollider)
		{
			if (_damaged && _internalRepairPoint == _playerInShip)
			{
				_repairReceiver.EnableCollider();
			}
			else
			{
				_repairReceiver.DisableCollider();
			}
		}
	}

	protected virtual void OnComponentDamaged()
	{
	}

	protected virtual void OnComponentRepaired()
	{
	}

	protected virtual void OnEnterShip()
	{
		_playerInShip = true;
		UpdateColliderState();
	}

	protected virtual void OnExitShip()
	{
		_playerInShip = false;
		UpdateColliderState();
	}
}
