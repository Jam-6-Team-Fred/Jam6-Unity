using UnityEngine;

public class ConstantFluidDetector : DynamicFluidDetector
{
	[SerializeField]
	private FluidVolume _onlyDetectableFluid;

	private bool _addOnStart = true;

	private void Start()
	{
		if (_addOnStart)
		{
			base.AddVolume((EffectVolume)_onlyDetectableFluid);
		}
	}

	protected override void SetupColliders()
	{
	}

	public void SetDetectableFluid(FluidVolume onlyDetectableFluid)
	{
		base.RemoveVolume((EffectVolume)_onlyDetectableFluid);
		base.AddVolume((EffectVolume)onlyDetectableFluid);
		_onlyDetectableFluid = onlyDetectableFluid;
		_addOnStart = false;
	}

	public override void AddVolume(EffectVolume volume)
	{
	}

	public override void RemoveVolume(EffectVolume volume)
	{
	}

	protected override void AccumulateFluidAcceleration()
	{
		ResetAccumulators();
		float fractionSubmerged = _onlyDetectableFluid.GetFractionSubmerged(this);
		AddDrag(_onlyDetectableFluid, fractionSubmerged);
		AddBuoyancy(_onlyDetectableFluid, fractionSubmerged);
	}

	public float GetFractionSubmergedFromOnlyDetectableFluid()
	{
		if (_onlyDetectableFluid == null)
		{
			Debug.LogError("This is not designed for multiple fluids", this);
			return 0f;
		}
		return _onlyDetectableFluid.GetFractionSubmerged(this);
	}
}
