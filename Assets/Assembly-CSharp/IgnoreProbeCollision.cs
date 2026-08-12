using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IgnoreProbeCollision : MonoBehaviour
{
	private void Awake()
	{
		Debug.LogError("Depricated script, replace with IgnoreCollision");
		Debug.Break();
		GlobalMessenger<Collider>.AddListener("IgnoreProbeCollider", OnIgnoreProbeCollider);
	}

	private void OnDestroy()
	{
		GlobalMessenger<Collider>.RemoveListener("IgnoreProbeCollider", OnIgnoreProbeCollider);
	}

	private void OnIgnoreProbeCollider(Collider probeCollider)
	{
		if (GetComponent<Collider>().enabled)
		{
			Physics.IgnoreCollision(GetComponent<Collider>(), probeCollider);
		}
	}
}
