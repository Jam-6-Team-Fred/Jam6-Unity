using UnityEngine;

public class SkyShellController : MonoBehaviour
{
	private Transform _sunTransform;

	private Transform _skyTransform;

	private void Start()
	{
		_skyTransform = base.transform;
		_sunTransform = Locator.GetSunTransform();
	}

	private void Update()
	{
		_skyTransform.LookAt(_sunTransform.position);
	}
}
