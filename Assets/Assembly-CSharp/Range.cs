using System;
using UnityEngine;

[Serializable]
public struct Range
{
	public float min;

	public float max;

	public float random => UnityEngine.Random.Range(min, max);

	public float span => Mathf.Abs(max - min);

	public bool Contains(float value)
	{
		if (value >= min)
		{
			return value <= max;
		}
		return false;
	}

	public Range(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	public void Set(float new_min, float new_max)
	{
		min = new_min;
		max = new_max;
	}

	public override string ToString()
	{
		return string.Format("({0:F1} to {1:F1})", new object[2] { min, max });
	}

	public string ToString(string format)
	{
		return string.Format("({0} to {1})", new object[2]
		{
			min.ToString(format),
			max.ToString(format)
		});
	}

	public override int GetHashCode()
	{
		return min.GetHashCode() ^ (max.GetHashCode() << 2);
	}

	public override bool Equals(object other)
	{
		if (!(other is Range range))
		{
			return false;
		}
		if (min.Equals(range.min))
		{
			return max.Equals(range.max);
		}
		return false;
	}
}
