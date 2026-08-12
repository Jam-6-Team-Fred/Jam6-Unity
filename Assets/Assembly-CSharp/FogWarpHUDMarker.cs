using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InnerFogWarpVolume))]
public class FogWarpHUDMarker : HUDMarker
{
	private struct HUDDistanceMarkerPair
	{
		public HUDDistanceMarker hudDistanceMarker;

		public float warpDistance;

		public HUDDistanceMarkerPair(HUDDistanceMarker marker, float distance)
		{
			hudDistanceMarker = marker;
			warpDistance = distance;
		}
	}

	private List<HUDDistanceMarkerPair> _visibleMarkers;

	private InnerFogWarpVolume _warpVolume;

	protected override void Awake()
	{
		_visibleMarkers = new List<HUDDistanceMarkerPair>(8);
		_warpVolume = this.GetRequiredComponent<InnerFogWarpVolume>();
	}

	protected override void InitCanvasMarker()
	{
	}

	public override bool IsVisible()
	{
		return false;
	}

	public void AddMarker(HUDDistanceMarker marker, float warpDist)
	{
		_visibleMarkers.SafeAdd(new HUDDistanceMarkerPair(marker, warpDist));
	}

	public void RemoveMarker(HUDDistanceMarker marker)
	{
		for (int i = 0; i < _visibleMarkers.Count; i++)
		{
			if (_visibleMarkers[i].hudDistanceMarker == marker)
			{
				_visibleMarkers.QuickRemoveAt(i);
				break;
			}
		}
	}

	public override FogWarpVolume GetOuterFogWarpVolume()
	{
		return _warpVolume.GetContainerWarpVolume();
	}

	protected override void RefreshOwnVisibility()
	{
	}
}
