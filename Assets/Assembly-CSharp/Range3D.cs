using System;
using UnityEngine;

[Serializable]
public struct Range3D
{
	public Vector3 min;

	public Vector3 max;

	public Vector3 random => new Vector3(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y), UnityEngine.Random.Range(min.z, max.z));

	public Vector3 span => new Vector3(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y), Mathf.Abs(max.z - min.z));

	public bool Contains(Vector3 value)
	{
		if (value.x >= min.x && value.x <= max.x && value.y >= min.y && value.y <= max.y && value.z >= min.z)
		{
			return value.z <= max.z;
		}
		return false;
	}

	public Range3D(float min_x, float min_y, float min_z, float max_x, float max_y, float max_z)
	{
		min = new Vector3(min_x, min_y, min_z);
		max = new Vector3(max_x, max_y, max_z);
	}

	public Range3D(Vector3 min, Vector3 max)
	{
		this.min = min;
		this.max = max;
	}

	public void Set(float new_min_x, float new_min_y, float new_min_z, float new_max_x, float new_max_y, float new_max_z)
	{
		min.Set(new_min_x, new_min_y, new_min_z);
		max.Set(new_max_x, new_max_y, new_max_z);
	}

	public void Set(Vector3 new_min, Vector3 new_max)
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
		if (!(other is Range3D range3D))
		{
			return false;
		}
		if (min.Equals(range3D.min))
		{
			return max.Equals(range3D.max);
		}
		return false;
	}
}
