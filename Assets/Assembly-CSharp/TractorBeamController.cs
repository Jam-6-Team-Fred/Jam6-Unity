using UnityEngine;

public class TractorBeamController : SectoredMonoBehaviour
{
	[SerializeField]
	private bool _deactivateOnAwake;

	[Space(10f)]
	[SerializeField]
	private OWAudioSource _oneShotAudioSrc;

	[SerializeField]
	private Transform _baseTransform;

	private TractorBeamFluid _fluid;

	private ParticleSystemRenderer[] _particleRenderers;

	private bool _activated = true;

	private SandLevelController _sandController;

	private float _elevation;

	protected override void Awake()
	{
		base.Awake();
		_fluid = GetComponentInChildren<TractorBeamFluid>();
		_particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>();
		if (_fluid.GetFluidType() != FluidVolume.Type.TRACTOR_BEAM)
		{
			Debug.LogError("Tractor beam fluid type is not TRACTOR_BEAM");
			Debug.Break();
		}
	}

	private void Start()
	{
		if (_deactivateOnAwake)
		{
			SetActivation(active: false, initActivation: true);
		}
		base.enabled = false;
		AstroObject component = _fluid.GetAttachedOWRigidbody().GetComponent<AstroObject>();
		if (component != null && _baseTransform != null && (component.GetAstroObjectName() == AstroObject.Name.CaveTwin || component.GetAstroObjectName() == AstroObject.Name.TowerTwin))
		{
			_sandController = _fluid.GetAttachedOWRigidbody().GetComponentInChildren<SandLevelController>();
			_elevation = Vector3.Distance(_sandController.transform.position, _baseTransform.position) + 1f;
		}
	}

	public bool IsActive()
	{
		return _activated;
	}

	public void SetReversed(bool reversed)
	{
		if (_fluid.IsFluidReversed() != reversed)
		{
			_fluid.SetFluidReversed(reversed);
		}
	}

	public void SetActivation(bool active, bool initActivation = false)
	{
		_fluid.SetVolumeActivation(active);
		if (_activated != active && !initActivation)
		{
			if (active)
			{
				_oneShotAudioSrc.PlayOneShot(AudioType.NomaiTractorBeamActivate);
			}
			else
			{
				_oneShotAudioSrc.PlayOneShot(AudioType.NomaiTractorBeamDeactivate);
			}
		}
		_activated = active;
		_deactivateOnAwake = false;
		for (int i = 0; i < _particleRenderers.Length; i++)
		{
			_particleRenderers[i].gameObject.SetActive(active);
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (_sandController != null)
		{
			base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship);
		}
	}

	private void Update()
	{
		if (_sandController != null)
		{
			if (_activated && _sandController.GetRadius() > _elevation)
			{
				SetActivation(active: false);
			}
			else if (!_activated && _sandController.GetRadius() <= _elevation)
			{
				SetActivation(active: true);
			}
		}
	}
}
