using UnityEngine;

public class CrumpleMesh : MonoBehaviour
{
	public float scale = 1f;

	public float speed = 1f;

	public bool recalculateNormals;

	private Vector3[] baseVertices;

	private Perlin noise;

	private void Start()
	{
		noise = new Perlin();
	}

	private void Update()
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		if (baseVertices == null)
		{
			baseVertices = mesh.vertices;
		}
		Vector3[] array = new Vector3[baseVertices.Length];
		float num = Time.time * speed + 0.1365143f;
		float num2 = Time.time * speed + 1.21688f;
		float num3 = Time.time * speed + 2.5564f;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector = baseVertices[i];
			vector.x += noise.Noise(num + vector.x, num + vector.y, num + vector.z) * scale;
			vector.y += noise.Noise(num2 + vector.x, num2 + vector.y, num2 + vector.z) * scale;
			vector.z += noise.Noise(num3 + vector.x, num3 + vector.y, num3 + vector.z) * scale;
			array[i] = vector;
		}
		mesh.vertices = array;
		if (recalculateNormals)
		{
			mesh.RecalculateNormals();
		}
		mesh.RecalculateBounds();
	}
}
