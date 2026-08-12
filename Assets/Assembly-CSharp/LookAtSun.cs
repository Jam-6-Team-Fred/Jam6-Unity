using UnityEngine;

public class LookAtSun : MonoBehaviour
{
	private Transform _transform;

	private Transform _sunTransform;

	private void Start()
	{
		_transform = base.transform;
		_sunTransform = Locator.GetSunTransform();
	}

	private void Update()
	{
		_transform.LookAt(_sunTransform.position);
	}
}
