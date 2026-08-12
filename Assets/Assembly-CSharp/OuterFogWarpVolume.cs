using System.Collections.Generic;
using UnityEngine;

public class OuterFogWarpVolume : SphericalFogWarpVolume
{
	public enum Name
	{
		None = 0,
		Hub = 1,
		EscapePod = 2,
		AnglerNest = 3,
		Pioneer = 4,
		ExitOnly = 5,
		Vessel = 6,
		Cluster = 7,
		SmallNest = 8
	}

	[SerializeField]
	private InnerFogWarpVolume _linkedInnerWarpVolume;

	[SerializeField]
	private Name _name;

	private List<InnerFogWarpVolume> _senderWarps;

	protected override void OnAwake()
	{
		base.OnAwake();
		_senderWarps = new List<InnerFogWarpVolume>();
		Locator.RegisterOuterFogWarp(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Locator.UnregisterOuterFogWarp(this);
	}

	public Name GetName()
	{
		return _name;
	}

	public void RegisterSenderWarp(InnerFogWarpVolume senderWarp)
	{
		_senderWarps.SafeAdd(senderWarp);
	}

	public override void PropagateCanvasMarkerOutwards(CanvasMarker marker, bool addMarker, float warpDist = 0f)
	{
		for (int i = 0; i < _senderWarps.Count; i++)
		{
			_senderWarps[i].PropagateCanvasMarkerOutwards(marker, addMarker, warpDist);
		}
	}

	public override bool IsOuterWarpVolume()
	{
		return true;
	}

	public override FogWarpVolume GetLinkedFogWarpVolume()
	{
		return _linkedInnerWarpVolume;
	}
}
