public class MinimapHUD : RetractableHUDElement
{
	private bool _minimapEnabled;

	protected override void Awake()
	{
		GlobalMessenger.AddListener("MinimapEnabled", OnMinimapEnabled);
		GlobalMessenger.AddListener("MinimapDisabled", OnMinimapDisabled);
		base.Awake();
	}

	protected override void OnDestroy()
	{
		GlobalMessenger.RemoveListener("MinimapEnabled", OnMinimapEnabled);
		GlobalMessenger.RemoveListener("MinimapDisabled", OnMinimapDisabled);
		base.OnDestroy();
	}

	protected override bool AllowVisibility()
	{
		if (_isHelmetHUDOn)
		{
			return _minimapEnabled;
		}
		return false;
	}

	private void OnMinimapEnabled()
	{
		_minimapEnabled = true;
		UpdateVisibility();
	}

	private void OnMinimapDisabled()
	{
		_minimapEnabled = false;
		UpdateVisibility();
	}
}
