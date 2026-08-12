using UnityEngine;

public class HUDDistanceMarker : HUDMarker
{
	[SerializeField]
	protected FogWarpDetector _fogWarpDetector;

	protected override void InitCanvasMarker()
	{
		base.InitCanvasMarker();
		if (_fogWarpDetector != null)
		{
			_canvasMarker.SetFogDetector(_fogWarpDetector);
		}
	}

	public override FogWarpVolume GetOuterFogWarpVolume()
	{
		if (_fogWarpDetector != null)
		{
			return _fogWarpDetector.GetOuterFogWarpVolume();
		}
		return null;
	}

	protected override void RefreshOwnVisibility()
	{
	}
}
