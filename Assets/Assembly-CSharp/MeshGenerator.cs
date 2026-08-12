using UnityEngine;

public class MeshGenerator
{
	public static Mesh GenerateQuad(float scale = 1f)
	{
		Mesh mesh = new Mesh();
		float num = scale * 0.5f;
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(num, num, 0f),
			new Vector3(0f - num, num, 0f),
			new Vector3(num, 0f - num, 0f),
			new Vector3(0f - num, 0f - num, 0f)
		};
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(0f, 0f),
			new Vector2(1f, 0f)
		};
		int[] triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		return mesh;
	}
}
