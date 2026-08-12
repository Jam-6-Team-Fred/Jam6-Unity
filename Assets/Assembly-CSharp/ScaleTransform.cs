using UnityEngine;

public class ScaleTransform : MonoBehaviour
{
	private void FixedUpdate()
	{
		base.transform.localScale = new Vector3(base.transform.localScale.x + Time.deltaTime * 0.1f, base.transform.localScale.y + Time.deltaTime * 0.1f, base.transform.localScale.z + Time.deltaTime * 0.1f);
	}
}
