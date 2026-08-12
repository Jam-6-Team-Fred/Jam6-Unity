using System.Collections.Generic;
using UnityEngine;

public class FragmentIntegrity : MonoBehaviour
{
	public delegate void TakeDamageEvent(float integrity);

	private const float MAX_IMPACT_DAMAGE = 20f;

	[SerializeField]
	protected float _integrity = 20f;

	[SerializeField]
	private float _randomIntegrityRange = 5f;

	[SerializeField]
	protected float _damageMultiplier = 1f;

	[SerializeField]
	private float _minImpactMass = 0.01f;

	[SerializeField]
	private float _maxImpactMass = 1f;

	[SerializeField]
	private float _minImpactSpeed = 10f;

	[SerializeField]
	private float _maxImpactSpeed = 100f;

	[SerializeField]
	private List<FragmentIntegrity> _childFragmentList;

	[SerializeField]
	private bool _breakWithLastChild;

	[SerializeField]
	private bool _ignoreMeteorDamage;

	[SerializeField]
	private BHPathNode _pathNode;

	private FragmentIntegrity _motherFragment;

	private FragmentIntegrity _parentFragment;

	private float _origIntegrity;

	public event TakeDamageEvent OnTakeDamage;

	protected virtual void Awake()
	{
		_parentFragment = base.transform.parent.GetComponentInParent<FragmentIntegrity>();
		if (_ignoreMeteorDamage && _integrity > 10f)
		{
			Debug.LogError("Big fragments should not be set to ignore meteors!", this);
			Debug.Break();
		}
		_integrity += Random.Range(0f - _randomIntegrityRange, _randomIntegrityRange);
		_integrity = Mathf.Max(0f, _integrity);
		_origIntegrity = _integrity;
		for (int i = 0; i < _childFragmentList.Count; i++)
		{
			_childFragmentList[i].SetMother(this);
		}
	}

	public void Init(float integrity, float propagateToChildFraction = 0f, Material fractureMaterial = null)
	{
		_integrity = integrity;
	}

	public FragmentIntegrity GetParentFragment()
	{
		return _parentFragment;
	}

	public void SetMother(FragmentIntegrity motherFragment)
	{
		if (_motherFragment != null)
		{
			Debug.LogError("This fragment, " + base.gameObject.name + ", has more than one mom!");
		}
		_motherFragment = motherFragment;
	}

	public float GetIntegrity()
	{
		return _integrity;
	}

	public BHPathNode GetPathNode()
	{
		return _pathNode;
	}

	public bool GetIgnoreMeteorDamage()
	{
		return _ignoreMeteorDamage;
	}

	public float GetIntegrityPercent()
	{
		float num = _integrity / _origIntegrity * 100f;
		if (Mathf.Round(num) == 0f && num > 0f)
		{
			return 1f;
		}
		return Mathf.Round(num);
	}

	public void HandleImpact(float impactMass, float impactSpeed)
	{
		float num = Mathf.InverseLerp(_minImpactSpeed, _maxImpactSpeed, impactSpeed);
		float num2 = Mathf.InverseLerp(_minImpactMass, _maxImpactMass, impactMass);
		if (_minImpactSpeed == _maxImpactSpeed && impactSpeed > _minImpactSpeed)
		{
			num = 1f;
		}
		if (_minImpactMass == _maxImpactMass && impactMass > _minImpactMass)
		{
			num2 = 1f;
		}
		float num3 = Mathf.Min(_origIntegrity, 20f) * num * num2;
		if (num3 > 0f)
		{
			AddDamage(num3);
		}
	}

	public virtual void AddDamage(float damage)
	{
		if (!(_integrity <= 0f))
		{
			if (CanBreak())
			{
				_integrity = Mathf.Max(0f, _integrity - damage * DamageMultiplier());
			}
			else
			{
				_integrity = Mathf.Max(0f, _integrity - Mathf.Min(damage * DamageMultiplier(), _integrity / 2f));
			}
			if (_integrity == 0f && _motherFragment != null)
			{
				_motherFragment.ChildIsBroken();
			}
			CallOnTakeDamage();
		}
	}

	protected virtual float DamageMultiplier()
	{
		if (_pathNode != null)
		{
			return _damageMultiplier * _pathNode.GetDamageMultiplier();
		}
		return _damageMultiplier;
	}

	protected virtual bool CanBreak()
	{
		if (!TimeLoop.IsTimeFlowing() || MeteorImpactMapper.AreFragmentsLocked())
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < _childFragmentList.Count; i++)
		{
			if (_childFragmentList[i].GetIntegrity() != 0f)
			{
				num++;
			}
		}
		if (_breakWithLastChild && num == 1)
		{
			for (int j = 0; j < _childFragmentList.Count; j++)
			{
				if (_childFragmentList[j].GetIntegrity() != 0f)
				{
					_childFragmentList[j].AddDamage(1000f);
				}
			}
			return true;
		}
		return num == 0;
	}

	public void ChildIsBroken()
	{
		if (!_breakWithLastChild)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < _childFragmentList.Count; i++)
		{
			if (_childFragmentList[i].GetIntegrity() != 0f)
			{
				num++;
			}
		}
		if (num == 0)
		{
			AddDamage(1000f);
		}
	}

	protected void CallOnTakeDamage()
	{
		if (this.OnTakeDamage != null)
		{
			this.OnTakeDamage(_integrity);
		}
	}
}
