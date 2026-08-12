using System;

public struct IntVector3
{
	public int x;

	public int y;

	public int z;

	public IntVector3(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public static IntVector3 operator +(IntVector3 v1, IntVector3 v2)
	{
		return new IntVector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
	}

	public static IntVector3 operator -(IntVector3 v1, IntVector3 v2)
	{
		return new IntVector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
	}

	public static bool operator ==(IntVector3 v1, IntVector3 v2)
	{
		if (v1.x == v2.x && v1.y == v2.y)
		{
			return v1.z == v2.z;
		}
		return false;
	}

	public static bool operator !=(IntVector3 v1, IntVector3 v2)
	{
		return !(v1 == v2);
	}

	public override int GetHashCode()
	{
		return (x + y + z) % int.MaxValue;
	}

	public override bool Equals(object other)
	{
		if (other is IntVector3)
		{
			return (IntVector3)other == this;
		}
		return false;
	}

	public bool Equals(IntVector3 other)
	{
		return other == this;
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null || format == "")
		{
			return $"({x}, {y}, {z})";
		}
		char c = format[0];
		string text = null;
		if (format.Length > 1)
		{
			text = format.Substring(1);
		}
		switch (c)
		{
		case 'x':
			return x.ToString(text, formatProvider);
		case 'y':
			return y.ToString(text, formatProvider);
		case 'z':
			return z.ToString(text, formatProvider);
		default:
			return $"({x.ToString(format, formatProvider)}, {y.ToString(format, formatProvider)}, {z.ToString(format, formatProvider)})";
		}
	}

	public override string ToString()
	{
		return ToString(null, null);
	}
}
