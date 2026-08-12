using System;

[Serializable]
public class DynamicOccupantMask
{
	public bool player = true;

	public bool probe = true;

	public bool ship = true;

	public bool environment;

	private DynamicOccupant _mask;

	private bool _dirty = true;

	public DynamicOccupant GetMask()
	{
		if (_dirty)
		{
			_mask = DynamicOccupant.Undefined;
			if (player)
			{
				_mask |= DynamicOccupant.Player;
			}
			if (probe)
			{
				_mask |= DynamicOccupant.Probe;
			}
			if (ship)
			{
				_mask |= DynamicOccupant.Ship;
			}
			if (environment)
			{
				_mask |= DynamicOccupant.Environment;
			}
			_dirty = false;
		}
		return _mask;
	}

	public void SetMask(DynamicOccupant mask)
	{
		player = IsOccupantInMask(DynamicOccupant.Player, mask);
		probe = IsOccupantInMask(DynamicOccupant.Probe, mask);
		ship = IsOccupantInMask(DynamicOccupant.Ship, mask);
		environment = IsOccupantInMask(DynamicOccupant.Environment, mask);
		_dirty = true;
	}

	public bool ContainsOccupant(DynamicOccupant occupant)
	{
		return (GetMask() & occupant) == occupant;
	}

	public bool ContainsAnyOccupants(DynamicOccupant mask)
	{
		return (GetMask() & mask) != 0;
	}

	public static bool IsOccupantInMask(DynamicOccupant occupant, DynamicOccupant mask)
	{
		return (mask & occupant) == occupant;
	}

	public static bool AreAnyOccupantsInMask(DynamicOccupant mask1, DynamicOccupant mask2)
	{
		return (mask1 & mask2) != 0;
	}
}
