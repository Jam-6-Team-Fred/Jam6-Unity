using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveRotationTest : MonoBehaviour
{
	private Vector3 eulerAngleVelocity = new Vector3(0f, 100f, 0f);

	private void FixedUpdate()
	{
		Quaternion quaternion = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime);
		GetComponent<Rigidbody>().MoveRotation(GetComponent<Rigidbody>().rotation * quaternion);
	}
}
