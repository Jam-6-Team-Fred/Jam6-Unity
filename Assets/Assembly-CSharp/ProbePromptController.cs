using System.Collections.Generic;
using UnityEngine;

public class ProbePromptController : MonoBehaviour, ILateInitializer
{
	private ProbeLauncher _activeLauncher;

	private bool _screenPromptsInitialized;

	private bool _holdToRetrieve;

	private ScreenPrompt _launchPrompt;

	private ScreenPrompt _unequipPrompt;

	private ScreenPrompt _retrievePrompt;

	private ScreenPrompt _retrieveCenterPrompt;

	private ScreenPrompt _forwardCamPrompt;

	private ScreenPrompt _takeSnapshotPrompt;

	private ScreenPrompt _snapshotCenterPrompt;

	private ScreenPrompt _reverseCamPrompt;

	private ScreenPrompt _photoModePrompt;

	private ScreenPrompt _launchModePrompt;

	private ScreenPrompt _aimPrompt;

	private ScreenPrompt _rotatePrompt;

	private ScreenPrompt _rotateCenterPrompt;

	private int _midairSnapshotCount;

	private int _rotateCameraCount;

	private bool _hasRetrievedProbe;

	private bool _wasAnchored;

	private void Awake()
	{
		_screenPromptsInitialized = false;
		LateInitializerManager.RegisterLateInitializer(this);
		_unequipPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.ProbeAwayPrompt) + "   <CMD>");
		_aimPrompt = new ScreenPrompt(InputLibrary.look, UITextLibrary.GetString(UITextType.ProbeAimPrompt) + "   <CMD>");
		_launchPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.ProbeLaunchPrompt) + "   <CMD>");
		_holdToRetrieve = InputLibrary.toolActionPrimary.HasSameBinding(InputLibrary.probeRetrieve, OWInput.UsingGamepad());
		if (_holdToRetrieve)
		{
			_retrievePrompt = new ScreenPrompt(InputLibrary.probeRetrieve, UITextLibrary.GetString(UITextType.ProbeRetrievePrompt) + "   <CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt));
			_retrieveCenterPrompt = new ScreenPrompt(InputLibrary.probeRetrieve, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.ProbeRetrievePrompt));
		}
		else
		{
			_retrievePrompt = new ScreenPrompt(InputLibrary.probeRetrieve, UITextLibrary.GetString(UITextType.ProbeRetrievePrompt) + "   <CMD>");
			_retrieveCenterPrompt = new ScreenPrompt(InputLibrary.probeRetrieve, "<CMD>   " + UITextLibrary.GetString(UITextType.ProbeRetrievePrompt));
		}
		_photoModePrompt = new ScreenPrompt(InputLibrary.toolOptionLeft, InputLibrary.toolOptionRight, UITextLibrary.GetString(UITextType.PhotoModePrompt) + "   <CMD1> <CMD2>", ScreenPrompt.MultiCommandType.CUSTOM_BOTH);
		_launchModePrompt = new ScreenPrompt(InputLibrary.toolOptionLeft, InputLibrary.toolOptionRight, UITextLibrary.GetString(UITextType.LaunchModePrompt) + "   <CMD1> <CMD2>", ScreenPrompt.MultiCommandType.CUSTOM_BOTH);
		_forwardCamPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.ProbeForwardSnapshotPrompt) + "   <CMD>");
		_takeSnapshotPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.ProbeSnapshotPrompt) + "   <CMD>");
		_reverseCamPrompt = new ScreenPrompt(InputLibrary.toolActionSecondary, UITextLibrary.GetString(UITextType.ProbeRearSnapshotPrompt) + "   <CMD>");
		List<IInputCommands> cmdList = new List<IInputCommands>
		{
			InputLibrary.toolOptionUp,
			InputLibrary.toolOptionDown,
			InputLibrary.toolOptionLeft,
			InputLibrary.toolOptionRight
		};
		_rotatePrompt = new ScreenPrompt(cmdList, UITextLibrary.GetString(UITextType.ProbeRotatePrompt) + "   <CMD>", ScreenPrompt.MultiCommandType.NONE);
		_rotateCenterPrompt = new ScreenPrompt(cmdList, "<CMD>   " + UITextLibrary.GetString(UITextType.ProbeRotatePrompt), ScreenPrompt.MultiCommandType.NONE);
		_snapshotCenterPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, "<CMD>   " + UITextLibrary.GetString(UITextType.ProbeSnapshotPrompt));
		GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherEquipped", OnProbeLauncherEquipped);
		GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherUnequipped", OnProbeLauncherUnequipped);
	}

	private void Start()
	{
		_hasRetrievedProbe = PlayerData.GetPersistentCondition("HAS_RETRIEVED_PROBE");
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (!_screenPromptsInitialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
		GlobalMessenger<ProbeLauncher>.RemoveListener("ProbeLauncherEquipped", OnProbeLauncherEquipped);
		GlobalMessenger<ProbeLauncher>.RemoveListener("ProbeLauncherUnequipped", OnProbeLauncherUnequipped);
	}

	public void LateInitialize()
	{
		_screenPromptsInitialized = true;
		Locator.GetPromptManager().AddScreenPrompt(_snapshotCenterPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_rotateCenterPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_retrieveCenterPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_unequipPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_aimPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_photoModePrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_launchModePrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_launchPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_retrievePrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_forwardCamPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_reverseCamPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_takeSnapshotPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_rotatePrompt, PromptPosition.UpperRight);
	}

	private void OnProbeLauncherEquipped(ProbeLauncher launcher)
	{
		base.enabled = true;
		_activeLauncher = launcher;
		launcher.OnLaunchProbe += OnLaunchProbe;
		launcher.OnTakeSnapshot += OnTakeSnapshot;
		launcher.OnRetrieveMyProbe += OnRetrieveProbe;
		launcher.OnRotateCamera += OnRotateCamera;
		if (!launcher.AllowLaunchMode())
		{
			_aimPrompt.SetText(UITextLibrary.GetString(UITextType.ProbeCameraAimPrompt) + "   <CMD>");
		}
		else
		{
			_aimPrompt.SetText(UITextLibrary.GetString(UITextType.ProbeAimPrompt) + "   <CMD>");
		}
	}

	private void OnProbeLauncherUnequipped(ProbeLauncher launcher)
	{
		base.enabled = false;
		_activeLauncher = null;
		launcher.OnLaunchProbe -= OnLaunchProbe;
		launcher.OnTakeSnapshot -= OnTakeSnapshot;
		launcher.OnRetrieveMyProbe -= OnRetrieveProbe;
		launcher.OnRotateCamera -= OnRotateCamera;
		HideAllPrompts();
	}

	private void HideAllPrompts()
	{
		_unequipPrompt.SetVisibility(isVisible: false);
		_snapshotCenterPrompt.SetVisibility(isVisible: false);
		_rotateCenterPrompt.SetVisibility(isVisible: false);
		_retrieveCenterPrompt.SetVisibility(isVisible: false);
		_launchPrompt.SetVisibility(isVisible: false);
		_forwardCamPrompt.SetVisibility(isVisible: false);
		_reverseCamPrompt.SetVisibility(isVisible: false);
		_retrievePrompt.SetVisibility(isVisible: false);
		_photoModePrompt.SetVisibility(isVisible: false);
		_launchModePrompt.SetVisibility(isVisible: false);
		_aimPrompt.SetVisibility(isVisible: false);
		_rotatePrompt.SetVisibility(isVisible: false);
		_takeSnapshotPrompt.SetVisibility(isVisible: false);
	}

	private void Update()
	{
		HideAllPrompts();
		if (!OWInput.IsInputMode(InputMode.Character | InputMode.ShipCockpit | InputMode.StationaryProbeLauncher))
		{
			return;
		}
		if (_activeLauncher.GetActiveProbe() == null)
		{
			_aimPrompt.SetVisibility(_activeLauncher.GetName() == ProbeLauncher.Name.Default);
			_unequipPrompt.SetVisibility(isVisible: true);
			if (_activeLauncher.InPhotoMode())
			{
				_snapshotCenterPrompt.SetVisibility(!PlayerData.GetPersistentCondition("HAS_TAKEN_HANDHELD_SNAPSHOT") && !OWInput.IsInputMode(InputMode.ShipCockpit));
				_takeSnapshotPrompt.SetVisibility(PlayerData.GetPersistentCondition("HAS_TAKEN_HANDHELD_SNAPSHOT") || OWInput.IsInputMode(InputMode.ShipCockpit));
				_launchModePrompt.SetVisibility(_activeLauncher.AllowLaunchMode());
			}
			else
			{
				_launchPrompt.SetVisibility(isVisible: true);
				_photoModePrompt.SetVisibility(_activeLauncher.AllowPhotoMode());
			}
			return;
		}
		bool flag = InputLibrary.toolActionPrimary.HasSameBinding(InputLibrary.probeRetrieve, OWInput.UsingGamepad());
		if (_holdToRetrieve != flag)
		{
			if (flag)
			{
				_retrievePrompt.SetText(UITextLibrary.GetString(UITextType.ProbeRetrievePrompt) + "   <CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt));
				_retrieveCenterPrompt.SetText("<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.ProbeRetrievePrompt));
			}
			else
			{
				_retrievePrompt.SetText(UITextLibrary.GetString(UITextType.ProbeRetrievePrompt) + "   <CMD>");
				_retrieveCenterPrompt.SetText("<CMD>   " + UITextLibrary.GetString(UITextType.ProbeRetrievePrompt));
			}
			_holdToRetrieve = flag;
		}
		bool num = _activeLauncher.GetActiveProbe().IsAnchored();
		bool flag2 = num && !_hasRetrievedProbe;
		_retrievePrompt.SetVisibility(!flag2);
		_retrieveCenterPrompt.SetVisibility(flag2);
		if (num)
		{
			bool persistentCondition = PlayerData.GetPersistentCondition("HAS_ROTATED_PROBE_CAMERA");
			_rotatePrompt.SetVisibility(persistentCondition);
			_rotateCenterPrompt.SetVisibility(!persistentCondition);
			_takeSnapshotPrompt.SetVisibility(isVisible: true);
			_wasAnchored = true;
		}
		else if (!PlayerData.GetPersistentCondition("HAS_TAKEN_MIDAIR_SNAPSHOT"))
		{
			_snapshotCenterPrompt.SetVisibility(isVisible: true);
		}
		else
		{
			_forwardCamPrompt.SetVisibility(isVisible: true);
			_reverseCamPrompt.SetVisibility(_activeLauncher.IsReverseCamAvailable());
		}
	}

	private void OnLaunchProbe(SurveyorProbe probe)
	{
	}

	private void OnRetrieveProbe()
	{
		_midairSnapshotCount = 0;
		_rotateCameraCount = 0;
		if (!_hasRetrievedProbe && _wasAnchored && _activeLauncher.GetName() == ProbeLauncher.Name.Player)
		{
			_hasRetrievedProbe = true;
			PlayerData.SetPersistentCondition("HAS_RETRIEVED_PROBE", state: true);
		}
		_wasAnchored = false;
	}

	private void OnRotateCamera()
	{
		_rotateCameraCount++;
		if (!PlayerData.GetPersistentCondition("HAS_ROTATED_PROBE_CAMERA") && _rotateCameraCount >= 3 && _activeLauncher.GetName() == ProbeLauncher.Name.Player)
		{
			PlayerData.SetPersistentCondition("HAS_ROTATED_PROBE_CAMERA", state: true);
		}
	}

	private void OnTakeSnapshot(RenderTexture snapshot, ProbeCamera camera, float secondsAnchored)
	{
		if (!PlayerData.GetPersistentCondition("HAS_TAKEN_HANDHELD_SNAPSHOT") && camera.GetID() == ProbeCamera.ID.PreLaunch)
		{
			if (_activeLauncher.GetName() == ProbeLauncher.Name.Player)
			{
				PlayerData.SetPersistentCondition("HAS_TAKEN_HANDHELD_SNAPSHOT", state: true);
			}
		}
		else if (secondsAnchored == 0f && camera.GetID() == ProbeCamera.ID.Forward)
		{
			_midairSnapshotCount++;
			if (!PlayerData.GetPersistentCondition("HAS_TAKEN_MIDAIR_SNAPSHOT") && _midairSnapshotCount >= 3 && _activeLauncher.GetName() == ProbeLauncher.Name.Player)
			{
				PlayerData.SetPersistentCondition("HAS_TAKEN_MIDAIR_SNAPSHOT", state: true);
			}
		}
	}
}
