using UnityEngine;

public class ProbePromptReceiver : InteractReceiver
{
	[SerializeField]
	private bool _overrideLaunchSpeed;

	[SerializeField]
	private float _launchSpeed = 10f;

	private bool _launcherWasAlreadyEquipped;

	private bool _probeWasAlreadyLaunched;

	private void Reset()
	{
		_interactRange = 3f;
	}

	protected override void Awake()
	{
		_screenPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, "<CMD>   " + UITextLibrary.GetString(UITextType.ProbeLaunchPrompt));
		_noCommandIconPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.ProbeLaunchPrompt));
	}

	public bool GetOverrideLaunchSpeed()
	{
		return _overrideLaunchSpeed;
	}

	public float GetLaunchSpeed()
	{
		return _launchSpeed;
	}

	public void OnLaunchProbe()
	{
		_probeWasAlreadyLaunched = false;
	}

	public override void LoseFocus()
	{
		if (Locator.GetToolModeSwapper().GetToolMode() == ToolMode.Probe && !_launcherWasAlreadyEquipped && (Locator.GetToolModeSwapper().GetProbeLauncher().GetActiveProbe() == null || _probeWasAlreadyLaunched))
		{
			Locator.GetToolModeSwapper().UnequipTool();
		}
		Locator.GetToolModeSwapper().GetProbeLauncher().SetProbePromptReceiver(null);
		base.LoseFocus();
	}

	protected override void UpdateInput()
	{
	}

	protected override void GainFocus()
	{
		if (Locator.GetPlayerSuit().IsWearingSuit(includeTrainingSuit: false))
		{
			_launcherWasAlreadyEquipped = Locator.GetToolModeSwapper().GetToolMode() == ToolMode.Probe;
			_probeWasAlreadyLaunched = Locator.GetToolModeSwapper().GetProbeLauncher().GetActiveProbe() != null;
			Locator.GetToolModeSwapper().EquipToolMode(ToolMode.Probe);
			Locator.GetToolModeSwapper().GetProbeLauncher().ExitPhotoMode();
		}
		Locator.GetToolModeSwapper().GetProbeLauncher().SetProbePromptReceiver(this);
		base.GainFocus();
	}

	protected override void UpdatePromptVisibility()
	{
		_screenPrompt.SetVisibility(_focused && Locator.GetPlayerSuit().IsWearingSuit(includeTrainingSuit: false) && Locator.GetToolModeSwapper().GetToolMode() == ToolMode.Probe && Locator.GetToolModeSwapper().GetProbeLauncher().GetActiveProbe() == null && !Locator.GetToolModeSwapper().GetProbeLauncher().InPhotoMode());
	}
}
