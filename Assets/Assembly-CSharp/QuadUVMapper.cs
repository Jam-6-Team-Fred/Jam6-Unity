using UnityEngine;

public class QuadUVMapper : MonoBehaviour
{
	[SerializeField]
	private int _cols = 1;

	[SerializeField]
	private int _rows = 1;

	[SerializeField]
	private int _index;

	[SerializeField]
	private float _scale = 1f;

	private void Awake()
	{
		GenerateMesh();
		Object.Destroy(this);
	}

	public void GenerateMesh()
	{
		float num = 1f / (float)_cols;
		float num2 = 1f / (float)_rows;
		int num3 = _index % _cols;
		int num4 = _index / _rows;
		float num5 = (float)num3 * num;
		float num6 = 1f - num2 - (float)num4 * num2;
		Vector2[] array = new Vector2[4]
		{
			new Vector2(0f * num + num5, 0f * num2 + num6),
			new Vector2(1f * num + num5, 0f * num2 + num6),
			new Vector2(0f * num + num5, 1f * num2 + num6),
			new Vector2(1f * num + num5, 1f * num2 + num6)
		};
		if (!OWMath.ApproxEquals(_scale, 1f))
		{
			float num7 = 1f / _scale;
			Vector2 vector = (array[3] + array[0]) * 0.5f;
			array[0] = (array[0] - vector) * num7 + vector;
			array[1] = (array[1] - vector) * num7 + vector;
			array[2] = (array[2] - vector) * num7 + vector;
			array[3] = (array[3] - vector) * num7 + vector;
		}
		Mesh mesh = new Mesh();
		mesh.name = base.gameObject.name + "_Quad";
		mesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
		mesh.vertices = new Vector3[4]
		{
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f)
		};
		mesh.normals = new Vector3[4]
		{
			new Vector3(0f, 0f, -1f),
			new Vector3(0f, 0f, -1f),
			new Vector3(0f, 0f, -1f),
			new Vector3(0f, 0f, -1f)
		};
		mesh.tangents = new Vector4[4]
		{
			new Vector4(1f, 0f, 0f, -1f),
			new Vector4(1f, 0f, 0f, -1f),
			new Vector4(1f, 0f, 0f, -1f),
			new Vector4(1f, 0f, 0f, -1f)
		};
		mesh.uv = array;
		mesh.triangles = new int[6] { 0, 2, 1, 1, 2, 3 };
		mesh.UploadMeshData(markNoLongerReadable: true);
		GetComponent<MeshFilter>().sharedMesh = mesh;
	}
}
