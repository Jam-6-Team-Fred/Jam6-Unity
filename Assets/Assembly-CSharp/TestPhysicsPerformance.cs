using UnityEngine;

public class TestPhysicsPerformance : MonoBehaviour
{
	public GameObject meshToDuplicateA;

	public GameObject meshToDuplicateB;

	public bool _convex;

	private void Awake()
	{
		_ = Debug.isDebugBuild;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Z))
		{
			GameObject obj = Object.Instantiate(meshToDuplicateA, new Vector3(0f, 0f, 0f), Quaternion.identity);
			Rigidbody rigidbody = obj.AddComponent<Rigidbody>();
			obj.GetComponent<MeshCollider>().convex = _convex;
			rigidbody.AddForce(Random.onUnitSphere * Random.Range(0.5f, 5f), ForceMode.VelocityChange);
		}
		if (Input.GetKeyDown(KeyCode.X))
		{
			if (_convex)
			{
				Debug.LogError("Cannot create convex collider for the HiPoly object");
			}
			else
			{
				Object.Instantiate(meshToDuplicateB, new Vector3(0f, 0f, 0f), Quaternion.identity).AddComponent<Rigidbody>().AddForce(Random.onUnitSphere * Random.Range(0.5f, 5f), ForceMode.VelocityChange);
			}
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			for (int i = 0; i < 10; i++)
			{
				GameObject obj2 = Object.Instantiate(meshToDuplicateA, new Vector3((float)i * 2f, 0f, 0f), Quaternion.identity);
				Rigidbody rigidbody2 = obj2.AddComponent<Rigidbody>();
				obj2.GetComponent<MeshCollider>().convex = _convex;
				rigidbody2.AddForce(Random.onUnitSphere * Random.Range(0.5f, 5f), ForceMode.VelocityChange);
			}
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			if (_convex)
			{
				Debug.LogError("Cannot create convex collider for the HiPoly object");
			}
			else
			{
				for (int j = 0; j < 10; j++)
				{
					Object.Instantiate(meshToDuplicateB, new Vector3((float)j * 2f, 0f, 0f), Quaternion.identity).AddComponent<Rigidbody>().AddForce(Random.onUnitSphere * Random.Range(0.5f, 5f), ForceMode.VelocityChange);
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.C))
		{
			GameObject obj3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			obj3.transform.position = new Vector3(0f, 0f, 0f);
			obj3.AddComponent<Rigidbody>().AddForce(Random.onUnitSphere * Random.Range(0.5f, 5f), ForceMode.VelocityChange);
		}
		if (Input.GetKeyDown(KeyCode.D))
		{
			for (int k = 0; k < 10; k++)
			{
				GameObject obj4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				obj4.transform.position = new Vector3((float)k * 2f, 0f, 0f);
				obj4.AddComponent<Rigidbody>().AddForce(Random.onUnitSphere * Random.Range(0.5f, 5f), ForceMode.VelocityChange);
			}
		}
	}
}
