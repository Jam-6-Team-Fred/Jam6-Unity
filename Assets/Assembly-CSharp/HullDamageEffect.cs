using UnityEngine;

public class HullDamageEffect : DamageEffect
{
	[Space]
	[SerializeField]
	private OWRenderer[] _hullDamageDecals = new OWRenderer[0];

	private int _propID_Damage;

	protected override void Awake()
	{
		_propID_Damage = Shader.PropertyToID("_Damage");
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < _hullDamageDecals.Length; i++)
		{
			_hullDamageDecals[i].SetActivation(active: true);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (!_passiveEffectsOnly)
		{
			for (int i = 0; i < _hullDamageDecals.Length; i++)
			{
				_hullDamageDecals[i].SetActivation(active: false);
			}
		}
	}

	public override void SetEffectBlend(float blend)
	{
		base.SetEffectBlend(blend);
		float value = Mathf.Sqrt(_blend);
		for (int i = 0; i < _hullDamageDecals.Length; i++)
		{
			_hullDamageDecals[i].SetMaterialProperty(_propID_Damage, value);
		}
	}
}
