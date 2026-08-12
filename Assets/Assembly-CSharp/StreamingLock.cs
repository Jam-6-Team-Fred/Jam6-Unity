using UnityEngine;

public class StreamingLock : MonoBehaviour
{
	[SerializeField]
	private StreamingGroup _streamingGroup;

	private bool _locked;

	private void Start()
	{
		if (_streamingGroup.AreRequiredAssetsLoaded())
		{
			base.enabled = false;
			return;
		}
		OWTime.Pause(OWTime.PauseType.Streaming);
		SpinnerUI.Show();
		_locked = true;
	}

	private void OnDestroy()
	{
		if (_locked)
		{
			OWTime.Unpause(OWTime.PauseType.Streaming);
			SpinnerUI.Hide();
			_locked = false;
		}
	}

	private void LateUpdate()
	{
		if (_locked && _streamingGroup.AreRequiredAssetsLoaded())
		{
			OWTime.Unpause(OWTime.PauseType.Streaming);
			SpinnerUI.Hide();
			_locked = false;
			base.enabled = false;
		}
	}
}
