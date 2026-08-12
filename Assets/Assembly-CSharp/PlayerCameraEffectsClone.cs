using UnityEngine;

public class PlayerCameraEffectsClone : MonoBehaviour
{
	private OWCamera _owCamera;

	private void Awake()
	{
		_owCamera = this.GetRequiredComponent<OWCamera>();
		_owCamera.onThisPreRender += new OWEvent<OWCamera>.OWCallback(OnOWCameraPreRender);
	}

	private void OnDestroy()
	{
		_owCamera.onThisPreRender -= new OWEvent<OWCamera>.OWCallback(OnOWCameraPreRender);
	}

	private void OnOWCameraPreRender(OWCamera owCamera)
	{
		OWCamera playerCamera = Locator.GetPlayerCamera();
		if (playerCamera != null)
		{
			_owCamera.postProcessingSettings.CopySettingsFrom(playerCamera.postProcessingSettings);
		}
	}
}
