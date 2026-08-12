using UnityEngine;

public class MovingPlatformTest : MonoBehaviour
{
	private void FixedUpdate()
	{
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().AddForce(base.transform.forward * 2f, ForceMode.VelocityChange);
	}
}
