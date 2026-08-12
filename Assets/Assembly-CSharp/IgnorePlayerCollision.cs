using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IgnorePlayerCollision : MonoBehaviour
{
	private void Awake()
	{
		Debug.LogError("Depricated script, replace with IgnoreCollision");
		Debug.Break();
		Physics.IgnoreCollision(GetComponent<Collider>(), Locator.GetPlayerCollider());
	}
}
