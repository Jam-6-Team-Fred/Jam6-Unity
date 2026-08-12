using UnityEngine;

public class DarkMatterSubmergeController : SectoredMonoBehaviour
{
	[SerializeField]
	private bool _activeWhenSubmerged;

	[Space]
	[SerializeField]
	private EffectVolume[] _effectVolumes = new EffectVolume[0];

	[SerializeField]
	private OWRenderer[] _renderers = new OWRenderer[0];

	[Header("Sensors")]
	[SerializeField]
	private ConstantFluidDetector _fluidDetector;

	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	private bool _active = true;

	private bool _submerged;

	protected override void Awake()
	{
		base.Awake();
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
		}
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
		}
	}

	private void FixedUpdate()
	{
		_submerged = _fluidDetector.GetFractionSubmergedFromOnlyDetectableFluid() > 0f;
		UpdateActivation();
	}

	private void UpdateActivation()
	{
		bool flag = _submerged == _activeWhenSubmerged;
		if (_active != flag)
		{
			_active = flag;
			for (int i = 0; i < _effectVolumes.Length; i++)
			{
				_effectVolumes[i].SetVolumeActivation(_active);
			}
			for (int j = 0; j < _renderers.Length; j++)
			{
				_renderers[j].SetActivation(_active);
			}
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (_fluidDetector != null)
		{
			base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		}
	}

	private void OnFloodImpact()
	{
		_submerged = true;
		UpdateActivation();
	}
}
