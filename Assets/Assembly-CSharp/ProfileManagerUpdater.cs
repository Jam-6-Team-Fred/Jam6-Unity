using UnityEngine;

public class ProfileManagerUpdater : MonoBehaviour
{
	private IProfileManager _profileManager;

	private void Start()
	{
		_profileManager = StandaloneProfileManager.SharedInstance;
		base.enabled = true;
	}

	private void Update()
	{
		if (_profileManager.hasPendingSaveOperation)
		{
			_profileManager.PerformPendingSaveOperation();
		}
	}
}
