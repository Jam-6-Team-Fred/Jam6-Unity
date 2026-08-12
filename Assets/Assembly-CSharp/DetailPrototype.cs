using System;
using UnityEngine;

[Serializable]
public struct DetailPrototype
{
	public enum AlignmentType
	{
		None = 0,
		SurfaceNormal = 1,
		Gravity = 2
	}

	public Mesh mesh;

	public Material material;

	public float radius;

	public Range rotation;

	public Range scale;

	public Gradient color;

	public AlignmentType alignment;

	public bool hanging;

	public float density;

	public void Reset(bool clearMesh = true)
	{
		if (clearMesh)
		{
			mesh = null;
			material = null;
		}
		radius = 0f;
		rotation = new Range(0f, 15f);
		scale = new Range(1f, 1f);
		color = new Gradient();
		alignment = AlignmentType.SurfaceNormal;
		hanging = false;
		density = 1f;
	}

	public Quaternion GetRandomRotation()
	{
		if (alignment == AlignmentType.Gravity && hanging)
		{
			return Quaternion.AngleAxis(rotation.random, Vector3.forward);
		}
		Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
		return Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up) * Quaternion.AngleAxis(rotation.random, new Vector3(normalized.x, 0f, normalized.y));
	}
}
