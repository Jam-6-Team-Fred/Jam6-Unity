using UnityEngine;

public class DampedSpringTest : MonoBehaviour
{
	public DampedSpring3D translationSpring = new DampedSpring3D();

	public DampedSpringQuat rotationSpring = new DampedSpringQuat();

	private void Update()
	{
		base.transform.position = translationSpring.Update(base.transform.position, Vector3.zero, Time.deltaTime);
		base.transform.rotation = rotationSpring.Update(base.transform.rotation, Quaternion.identity, Time.deltaTime);
	}
}
