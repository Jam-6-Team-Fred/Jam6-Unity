using System;

[Flags]
public enum WarpCoreType
{
	Invalid = 0,
	Vessel = 1,
	VesselBroken = 2,
	Black = 4,
	White = 8,
	SimpleBroken = 0x10
}
