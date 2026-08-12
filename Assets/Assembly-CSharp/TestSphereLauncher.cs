using UnityEngine;

public class TestSphereLauncher : MonoBehaviour
{
	[SerializeField]
	private GameObject _spherePrefab;

	private bool _launchSphereNextFrame;

	private void Update()
	{
		if (OWInput.IsNewlyPressed(InputLibrary.interact))
		{
			_launchSphereNextFrame = true;
		}
	}

	private void FixedUpdate()
	{
		if (_launchSphereNextFrame)
		{
			_launchSphereNextFrame = false;
			OWRigidbody component = Object.Instantiate(_spherePrefab, base.transform.position + base.transform.forward, base.transform.rotation, null).GetComponent<OWRigidbody>();
			Vector3 velocity = Locator.GetPlayerTransform().GetAttachedOWRigidbody().GetVelocity() + base.transform.forward * 20f;
			component.SetVelocity(velocity);
		}
	}
}
