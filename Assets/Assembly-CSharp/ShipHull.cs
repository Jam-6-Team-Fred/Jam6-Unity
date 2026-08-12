using UnityEngine;

[RequireComponent(typeof(ShipModule))]
public class ShipHull : MonoBehaviour
{
	public enum Section
	{
		Top = 0,
		Bottom = 1,
		Left = 2,
		Right = 3,
		Front = 4,
		Back = 5
	}

	public delegate void DamageEvent(ShipHull shipHull);

	public delegate void ImpactEvent(ImpactData impact, float damage);

	[SerializeField]
	protected UITextType _hullName = UITextType.SatelliteNodeName;

	[SerializeField]
	protected Section _section;

	[SerializeField]
	protected GameObject _collidersGroup;

	[SerializeField]
	protected GameObject _componentsGroup;

	[SerializeField]
	protected DamageEffect _damageEffect;

	[SerializeField]
	protected float _repairTime = 5f;

	protected bool _damaged;

	protected float _integrity;

	protected ImpactData _dominantImpact;

	protected ShipModule _module;

	protected Collider[] _colliders;

	protected ShipComponent[] _components;

	protected bool _debugImpact;

	public UITextType hullName => _hullName;

	public Section section => _section;

	public bool isDamaged => _damaged;

	public float integrity => _integrity;

	public ShipModule shipModule => _module;

	public event DamageEvent OnDamaged;

	public event DamageEvent OnRepaired;

	public event ImpactEvent OnImpact;

	protected virtual void Awake()
	{
		_damaged = false;
		_integrity = 1f;
		_dominantImpact = null;
		_module = GetComponent<ShipModule>();
		_colliders = _collidersGroup.GetComponentsInChildren<Collider>();
		_components = _componentsGroup.GetComponentsInChildren<ShipComponent>();
	}

	protected virtual void Start()
	{
		for (int i = 0; i < _colliders.Length; i++)
		{
			if (!(_colliders[i].GetComponent<RepairReceiver>() != null))
			{
				RepairReceiver repairReceiver = _colliders[i].gameObject.AddComponent<RepairReceiver>();
				repairReceiver.type = RepairReceiver.Type.ShipHull;
				repairReceiver.targetHull = this;
				repairReceiver.repairDistance = 3f;
			}
		}
	}

	public virtual void FixedUpdate()
	{
		if (_debugImpact)
		{
			_integrity = 0.5f;
			_damaged = true;
			_debugImpact = false;
			if (this.OnDamaged != null)
			{
				this.OnDamaged(this);
			}
			if (_damageEffect != null)
			{
				_damageEffect.SetEffectBlend(1f - _integrity);
			}
			base.enabled = false;
		}
		if (_dominantImpact != null)
		{
			float num = Mathf.InverseLerp(30f, 200f, _dominantImpact.speed);
			if (num > 0f)
			{
				float num2 = 0.15f;
				if (num < num2 && _integrity > 1f - num2)
				{
					num = num2;
				}
				_integrity = Mathf.Max(_integrity - num, 0f);
				if (!_damaged)
				{
					_damaged = true;
					if (this.OnDamaged != null)
					{
						this.OnDamaged(this);
					}
				}
				if (_damageEffect != null)
				{
					_damageEffect.SetEffectBlend(1f - _integrity);
				}
			}
			for (int i = 0; i < _components.Length && (_components[i] == null || _components[i].isDamaged || !_components[i].ApplyImpact(_dominantImpact)); i++)
			{
			}
			if (this.OnImpact != null)
			{
				this.OnImpact(_dominantImpact, num);
			}
			_dominantImpact = null;
		}
		base.enabled = false;
	}

	public virtual void ApplyDebugImpact()
	{
		_debugImpact = true;
		base.enabled = true;
	}

	public virtual void ApplyImpact(ImpactData impact)
	{
		if (_dominantImpact == null || impact.speed > _dominantImpact.speed)
		{
			_dominantImpact = new ImpactData(impact);
			base.enabled = true;
		}
	}

	public void RepairTick()
	{
		if (!_damaged)
		{
			return;
		}
		_integrity = Mathf.Min(_integrity + Time.deltaTime / _repairTime, 1f);
		if (_integrity >= 1f)
		{
			_damaged = false;
			if (this.OnRepaired != null)
			{
				this.OnRepaired(this);
			}
		}
		if (_damageEffect != null)
		{
			_damageEffect.SetEffectBlend(1f - _integrity);
		}
	}

	public void TriggerSystemFailure()
	{
		if (_damageEffect != null)
		{
			_damageEffect.DisableActiveEffects();
		}
	}

	public bool ContainsCollider(Collider collider)
	{
		for (int i = 0; i < _colliders.Length; i++)
		{
			if (_colliders[i] == collider)
			{
				return true;
			}
		}
		return false;
	}
}
