using UnityEngine;

public class SatelliteNode : SectoredMonoBehaviour
{
	public delegate void RepairEvent(SatelliteNode satelliteNode);

	[Space]
	[SerializeField]
	private DamageEffect _damageEffect;

	[SerializeField]
	private ReferenceFrameVolume _rfVolume;

	[Space]
	[SerializeField]
	private Light _lanternLight;

	[SerializeField]
	private Color _lightRepairedColor = new Color(0.25f, 1f, 0.25f);

	[SerializeField]
	private MeshRenderer _lanternEmissiveRenderer;

	[SerializeField]
	private int _lanternMaterialIndex;

	[SerializeField]
	private Material _lanternRepairedMaterial;

	[Space]
	[SerializeField]
	private float _repairTime = 3f;

	private bool _damaged;

	private float _repairFraction;

	private Material[] _lanternMaterials;

	public bool isDamaged => _damaged;

	public float repairFraction => _repairFraction;

	public event RepairEvent OnRepaired;

	protected override void Awake()
	{
		base.Awake();
		_damaged = true;
		_repairFraction = 0f;
		if (_lanternEmissiveRenderer != null)
		{
			_lanternMaterials = new Material[_lanternEmissiveRenderer.sharedMaterials.Length];
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (_damageEffect != null)
		{
			if (_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
			{
				_damageEffect.SetEffectBlend(1f - _repairFraction);
			}
			else
			{
				_damageEffect.enabled = false;
			}
		}
	}

	public void RepairTick()
	{
		if (!_damaged)
		{
			return;
		}
		_repairFraction = Mathf.Clamp01(_repairFraction + Time.deltaTime / _repairTime);
		if (_repairFraction >= 1f)
		{
			_damaged = false;
			ReferenceFrameTracker component = Locator.GetPlayerTransform().GetComponent<ReferenceFrameTracker>();
			if (component.GetReferenceFrame() == _rfVolume.GetReferenceFrame())
			{
				component.UntargetReferenceFrame();
			}
			if (_rfVolume != null)
			{
				_rfVolume.gameObject.SetActive(value: false);
			}
			if (_lanternLight != null)
			{
				_lanternLight.color = _lightRepairedColor;
			}
			if (_lanternEmissiveRenderer != null)
			{
				_lanternEmissiveRenderer.sharedMaterials.CopyTo(_lanternMaterials, 0);
				_lanternMaterials[_lanternMaterialIndex] = _lanternRepairedMaterial;
				_lanternEmissiveRenderer.sharedMaterials = _lanternMaterials;
			}
			if (this.OnRepaired != null)
			{
				this.OnRepaired(this);
			}
		}
		if (_damageEffect != null)
		{
			_damageEffect.SetEffectBlend(1f - _repairFraction);
		}
	}
}
