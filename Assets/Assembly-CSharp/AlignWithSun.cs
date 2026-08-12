using UnityEngine;

[RequireComponent(typeof(AlignWithTargetBody))]
public class AlignWithSun : MonoBehaviour
{
	private void Start()
	{
		Transform sunTransform = Locator.GetSunTransform();
		if (sunTransform != null)
		{
			GetComponent<AlignWithTargetBody>().SetTargetBody(sunTransform.GetAttachedOWRigidbody());
		}
	}
}
