using System;
using UnityEngine;

[Serializable]
public struct Range2D
{
	public Vector2 min;

	public Vector2 max;

	public Vector2 random => new Vector2(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y));

	public Vector2 span => new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));

	public bool Contains(Vector2 value)
	{
		if (value.x >= min.x && value.x <= max.x && value.y >= min.y)
		{
			return value.y <= max.y;
		}
		return false;
	}

	public Range2D(float min_x, float min_y, float max_x, float max_y)
	{
		min = new Vector2(min_x, min_y);
		max = new Vector2(max_x, max_y);
	}

	public Range2D(Vector2 min, Vector2 max)
	{
		this.min = min;
		this.max = max;
	}

	public void Set(float new_min_x, float new_min_y, float new_max_x, float new_max_y)
	{
		min.Set(new_min_x, new_min_y);
		max.Set(new_max_x, new_max_y);
	}

	public void Set(Vector2 new_min, Vector2 new_max)
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
		if (!(other is Range2D range2D))
		{
			return false;
		}
		if (min.Equals(range2D.min))
		{
			return max.Equals(range2D.max);
		}
		return false;
	}
}
