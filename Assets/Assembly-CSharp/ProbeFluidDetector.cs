using System.Collections.Generic;
using UnityEngine;

public class ProbeFluidDetector : AsymmetricFluidDetector
{
	private SurveyorProbe probe;

	private Vector3 _origDragFactor;

	private List<FluidVolume> _movingFluids;

	private int _splashBlockFrameCount;

	protected override void Awake()
	{
		base.Awake();
		probe = GetComponentInParent<SurveyorProbe>();
		probe.OnLaunchProbe += OnLaunchProbe;
		probe.OnRetrieveProbe += OnRetrieveProbe;
		_origDragFactor = _dragFactor;
		_movingFluids = new List<FluidVolume>(16);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		probe.OnLaunchProbe -= OnLaunchProbe;
		probe.OnRetrieveProbe -= OnRetrieveProbe;
	}

	public override void ManagedFixedUpdate()
	{
		base.ManagedFixedUpdate();
		_splashBlockFrameCount = Mathf.Max(_splashBlockFrameCount - 1, 0);
	}

	public void SetOceanBarrierDrag()
	{
		_dragFactor = Vector3.one * 10f;
	}

	public void SetHurricaneDrag()
	{
		_dragFactor = Vector3.one * 10f;
	}

	protected override void OnVolumeActivated(PriorityVolume eVol)
	{
		base.OnVolumeActivated(eVol);
		if (!(eVol as FluidVolume != null))
		{
			return;
		}
		FluidVolume fluidVolume = eVol as FluidVolume;
		if (fluidVolume.CheckTriggerProbeDragIncrease(this))
		{
			float num = 10f;
			if (fluidVolume.GetFluidType() == FluidVolume.Type.SAND)
			{
				num = 50f;
			}
			else if (fluidVolume.GetFluidType() == FluidVolume.Type.GEYSER)
			{
				num = 1f;
			}
			_movingFluids.Add(fluidVolume);
			_dragFactor = Vector3.one * num;
			MonoBehaviour.print("Probe entered moving fluid. New drag is " + num);
		}
		else if (fluidVolume.GetFluidType() == FluidVolume.Type.WATER)
		{
			MonoBehaviour.print("Probe entered water. New drag is 0.3");
			_dragFactor = Vector3.one * 0.3f;
		}
	}

	protected override void OnVolumeDeactivated(PriorityVolume eVol)
	{
		base.OnVolumeDeactivated(eVol);
		if (eVol as FluidVolume != null)
		{
			FluidVolume item = eVol as FluidVolume;
			if (_movingFluids.Remove(item) && _movingFluids.Count == 0)
			{
				MonoBehaviour.print("Probe exited moving fluid. New drag is 1");
				_dragFactor = Vector3.one;
			}
		}
	}

	protected override void OnEnterFluidType_Internal(FluidVolume fluid)
	{
		if (_splashBlockFrameCount == 0)
		{
			base.OnEnterFluidType_Internal(fluid);
		}
	}

	private void OnLaunchProbe()
	{
		_splashBlockFrameCount = 2;
	}

	private void OnRetrieveProbe()
	{
		_dragFactor = _origDragFactor;
	}
}
