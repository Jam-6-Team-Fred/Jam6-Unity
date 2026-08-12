using System;

[Serializable]
public class OWString
{
	public string Value;

	public OWString()
	{
		Value = string.Empty;
	}

	public OWString(string str)
	{
		Value = str;
	}

	public static OWString operator +(OWString a, OWString b)
	{
		return new OWString(a.Value + b.Value);
	}

	public static bool operator ==(OWString a, OWString b)
	{
		return a.Value == b.Value;
	}

	public static bool operator !=(OWString a, OWString b)
	{
		return a.Value != b.Value;
	}

	public override int GetHashCode()
	{
		if (Value == null)
		{
			return 0;
		}
		return Value.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is OWString oWString))
		{
			return false;
		}
		return oWString.Value == Value;
	}
}
