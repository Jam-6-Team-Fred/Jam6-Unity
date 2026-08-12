using UnityEngine;

public class EnableDisableMeshColliderTest : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.C))
		{
			MeshCollider component = GetComponent<MeshCollider>();
			component.enabled = !component.enabled;
			MonoBehaviour.print("Mesh Collider: enabled = " + component.enabled);
		}
	}
}
