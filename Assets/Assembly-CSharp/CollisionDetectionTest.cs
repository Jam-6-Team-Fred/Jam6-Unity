using UnityEngine;

public class CollisionDetectionTest : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			ContactPoint[] contacts = collision.contacts;
			for (int i = 0; i < contacts.Length; i++)
			{
				ContactPoint contactPoint = contacts[i];
				MonoBehaviour.print(contactPoint.thisCollider.name);
				MonoBehaviour.print(contactPoint.otherCollider.name);
			}
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
	}
}
