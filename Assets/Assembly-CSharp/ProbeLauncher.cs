using System;
using System.IO;
using UnityEngine;

public class ProbeLauncher : PlayerTool
{
	public enum Name
	{
		Default = 0,
		Player = 1,
		Ship = 2,
		Stationary = 3
	}

	public delegate void LaunchProbeEvent(SurveyorProbe probe);

	public delegate void RetrieveProbeEvent();

	public delegate void RotateCameraEvent();

	public delegate void TakeSnapshotEvent(RenderTexture texture, ProbeCamera camera, float secondsAnchored);

	[SerializeField]
	protected Name _name;

	[SerializeField]
	protected Transform _launcherTransform;

	[SerializeField]
	private GameObject _launcherGeometry;

	[SerializeField]
	private GameObject _preLaunchProbeProxy;

	[SerializeField]
	private GameObject _probePrefab;

	[Space]
	[SerializeField]
	private SingularityWarpEffect _probeRetrievalEffect;

	[SerializeField]
	private float _probeRetrievalLength = 0.5f;

	[Space]
	[SerializeField]
	private bool _alwaysVisible;

	[SerializeField]
	private bool _shareActiveProbes;

	[SerializeField]
	private NotificationTarget _notificationFilter = NotificationTarget.None;

	[SerializeField]
	private FluidDetector _fluidDetector;

	[SerializeField]
	private ProbeLauncherUI[] _launcherUIs;

	protected SurveyorProbe _activeProbe;

	private ProbeCamera _preLaunchCamera;

	private ProbeLauncherEffects _effects;

	private ProbePromptReceiver _promptTrigger;

	private RenderTexture _lastSnapshot;

	private bool _allowRetrieval;

	private bool _isRetrieving;

	private float _orbitLaunchAngle;

	private float _orbitSpeed;

	private bool _justEquipped;

	private bool _photoMode;

	public event LaunchProbeEvent OnLaunchProbe;

	public event RetrieveProbeEvent OnRetrieveMyProbe;

	public event RotateCameraEvent OnRotateCamera;

	public event TakeSnapshotEvent OnTakeSnapshot;

	protected virtual void Awake()
	{
		if (!_launcherTransform)
		{
			_launcherTransform = base.transform;
		}
		_effects = this.GetRequiredComponent<ProbeLauncherEffects>();
		ProbeCamera[] componentsInChildren = GetComponentsInChildren<ProbeCamera>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].GetID() == ProbeCamera.ID.PreLaunch)
			{
				_preLaunchCamera = componentsInChildren[i];
			}
		}
		if (!_alwaysVisible && (bool)_launcherGeometry)
		{
			_launcherGeometry.SetActive(value: false);
		}
		GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherEquipped", OnProbeLauncherEquipped);
		GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherUnequipped", OnProbeLauncherUnequipped);
		GlobalMessenger<SurveyorProbe>.AddListener("RetrieveProbe", OnAnyProbeRetrieved);
		GlobalMessenger<SurveyorProbe>.AddListener("ForceRetrieveProbe", OnForceRetrieveProbe);
		GlobalMessenger<SurveyorProbe>.AddListener("ForceRetrieveProbeSilent", OnForceRetrieveProbeSilent);
		for (int j = 0; j < _launcherUIs.Length; j++)
		{
			_launcherUIs[j].SetProbeLauncher(this);
		}
	}

	protected override void Start()
	{
		base.Start();
		base.enabled = true;
		_photoMode = !AllowLaunchMode();
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
		if (!_alwaysVisible && (bool)_launcherGeometry)
		{
			_launcherGeometry.SetActive(value: false);
		}
	}

	protected virtual void OnDestroy()
	{
		GlobalMessenger<ProbeLauncher>.RemoveListener("ProbeLauncherEquipped", OnProbeLauncherEquipped);
		GlobalMessenger<ProbeLauncher>.RemoveListener("ProbeLauncherUnequipped", OnProbeLauncherUnequipped);
		GlobalMessenger<SurveyorProbe>.RemoveListener("RetrieveProbe", OnAnyProbeRetrieved);
		GlobalMessenger<SurveyorProbe>.RemoveListener("ForceRetrieveProbe", OnForceRetrieveProbe);
		GlobalMessenger<SurveyorProbe>.RemoveListener("ForceRetrieveProbeSilent", OnForceRetrieveProbeSilent);
	}

	public void AddProbeLauncherUI(ProbeLauncherUI ui)
	{
		if (_launcherUIs.SafeAdd(ui))
		{
			ui.SetProbeLauncher(this);
		}
	}

	public void RemoveProbeLauncherUI(ProbeLauncherUI ui)
	{
		_launcherUIs.QuickRemove(ui);
		ui.SetProbeLauncher(null);
	}

	public override void EquipTool()
	{
		base.EquipTool();
		base.enabled = true;
		_justEquipped = true;
		if ((bool)_launcherGeometry)
		{
			_launcherGeometry.SetActive(value: true);
		}
		GlobalMessenger<ProbeLauncher>.FireEvent("ProbeLauncherEquipped", this);
		for (int i = 0; i < _launcherUIs.Length; i++)
		{
			_launcherUIs[i].ActivateUI();
		}
	}

	public override void UnequipTool()
	{
		base.UnequipTool();
		base.enabled = true;
		GlobalMessenger<ProbeLauncher>.FireEvent("ProbeLauncherUnequipped", this);
		for (int i = 0; i < _launcherUIs.Length; i++)
		{
			_launcherUIs[i].DeactivateUI();
		}
	}

	public Name GetName()
	{
		return _name;
	}

	public bool InPhotoMode()
	{
		if (IsEquipped() && (_activeProbe == null || !_activeProbe.IsLaunched()))
		{
			return _photoMode;
		}
		return false;
	}

	public void ExitPhotoMode()
	{
		_photoMode = false;
	}

	public bool IsReverseCamAvailable()
	{
		if (_activeProbe != null)
		{
			return !InputLibrary.toolActionSecondary.HasSameBinding(InputLibrary.probeRetrieve, OWInput.UsingGamepad());
		}
		return false;
	}

	public SurveyorProbe GetActiveProbe()
	{
		return _activeProbe;
	}

	public void SetActiveProbe(SurveyorProbe probe)
	{
		_activeProbe = probe;
		_preLaunchProbeProxy.gameObject.SetActive(!_activeProbe.IsLaunched());
	}

	public bool SharesActiveProbes()
	{
		return _shareActiveProbes;
	}

	public void SetProbePromptReceiver(ProbePromptReceiver promptTrigger)
	{
		_promptTrigger = promptTrigger;
		_justEquipped = false;
	}

	public virtual bool AllowLaunchMode()
	{
		return true;
	}

	public virtual bool AllowPhotoMode()
	{
		return _name == Name.Player;
	}

	protected virtual bool AllowInput()
	{
		return OWInput.IsInputMode(InputMode.Character | InputMode.ShipCockpit);
	}

	protected virtual float GetHorizonRadiusOverride()
	{
		return 0f;
	}

	protected override void Update()
	{
		base.Update();
		base.enabled = true;
		if (!AllowInput() || !_isEquipped)
		{
			return;
		}
		if (Locator.GetProbe() == null)
		{
			if (OWInput.IsNewlyPressed(InputLibrary.probeLaunch) || OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary) || OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary))
			{
				NotificationData data = new NotificationData(UITextLibrary.GetString(UITextType.NotificationUnableToRetrieveProbe));
				NotificationManager.SharedInstance.PostNotification(data);
				Locator.GetPlayerAudioController().PlayNegativeUISound();
			}
			return;
		}
		UpdateOrbitalLaunchValues();
		if (_justEquipped)
		{
			_justEquipped = false;
			if (UpdateOnEquip())
			{
				return;
			}
		}
		if (_activeProbe == null)
		{
			UpdatePreLaunch();
		}
		else
		{
			UpdatePostLaunch();
		}
	}

	protected virtual bool UpdateOnEquip()
	{
		if (!OWInput.IsPressed(InputLibrary.toolActionPrimary, 0.3f))
		{
			if (_activeProbe == null)
			{
				return true;
			}
			if (_activeProbe.IsAnchored())
			{
				TakeSnapshotWithCamera(_activeProbe.GetRotatingCamera());
				return true;
			}
			TakeSnapshotWithCamera(_activeProbe.GetForwardCamera());
			return true;
		}
		return false;
	}

	protected virtual void UpdatePreLaunch()
	{
		if (_name == Name.Player && (OWInput.IsNewlyPressed(InputLibrary.toolOptionLeft) || OWInput.IsNewlyPressed(InputLibrary.toolOptionRight)))
		{
			_photoMode = !_photoMode;
			_effects.PlayChangeModeClip();
		}
		else
		{
			if (!OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary))
			{
				return;
			}
			if (InPhotoMode())
			{
				if (_launcherGeometry != null)
				{
					_launcherGeometry.SetActive(value: false);
				}
				TakeSnapshotWithCamera(_preLaunchCamera);
				if (_launcherGeometry != null)
				{
					_launcherGeometry.SetActive(value: true);
				}
			}
			else if (AllowLaunchMode())
			{
				LaunchProbe();
			}
		}
	}

	protected virtual void UpdatePostLaunch()
	{
		if (_activeProbe.IsRetrieving())
		{
			return;
		}
		if (_activeProbe.IsAnchored() && !_activeProbe.transform.parent.gameObject.activeInHierarchy)
		{
			_activeProbe.Unanchor();
		}
		if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary))
		{
			if (_activeProbe.IsAnchored())
			{
				TakeSnapshotWithCamera(_activeProbe.GetRotatingCamera());
			}
			else
			{
				TakeSnapshotWithCamera(_activeProbe.GetForwardCamera());
			}
		}
		else if (IsReverseCamAvailable() && OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary))
		{
			TakeSnapshotWithCamera(_activeProbe.GetReverseCamera());
		}
		if (_activeProbe.IsAnchored())
		{
			float num = 30f;
			if (OWInput.IsNewlyPressed(InputLibrary.toolOptionRight))
			{
				_activeProbe.GetRotatingCamera().RotateHorizontal(num);
				TakeSnapshotWithCamera(_activeProbe.GetRotatingCamera());
				if (this.OnRotateCamera != null)
				{
					this.OnRotateCamera();
				}
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.toolOptionLeft))
			{
				_activeProbe.GetRotatingCamera().RotateHorizontal(0f - num);
				TakeSnapshotWithCamera(_activeProbe.GetRotatingCamera());
				if (this.OnRotateCamera != null)
				{
					this.OnRotateCamera();
				}
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.toolOptionUp))
			{
				_activeProbe.GetRotatingCamera().RotateVertical(0f - num);
				TakeSnapshotWithCamera(_activeProbe.GetRotatingCamera());
				if (this.OnRotateCamera != null)
				{
					this.OnRotateCamera();
				}
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.toolOptionDown))
			{
				_activeProbe.GetRotatingCamera().RotateVertical(num);
				TakeSnapshotWithCamera(_activeProbe.GetRotatingCamera());
				if (this.OnRotateCamera != null)
				{
					this.OnRotateCamera();
				}
			}
		}
		bool flag = InputLibrary.toolActionPrimary.HasSameBinding(InputLibrary.probeRetrieve, OWInput.UsingGamepad());
		if (_allowRetrieval && ((flag && OWInput.IsPressed(InputLibrary.probeRetrieve, 0.5f)) || (!flag && OWInput.IsNewlyPressed(InputLibrary.probeRetrieve))))
		{
			RetrieveProbe();
			_allowRetrieval = false;
		}
		else if (!_allowRetrieval && !OWInput.IsPressed(InputLibrary.toolActionPrimary))
		{
			_allowRetrieval = true;
		}
	}

	protected void RetrieveProbe(bool playEffects = true, bool forcedRetrieval = false)
	{
		if (_isRetrieving || !(_activeProbe != null))
		{
			return;
		}
		if (_activeProbe.IsLaunched() && TimelineObliterationController.IsParadoxProbeActive() && !forcedRetrieval)
		{
			NotificationData data = new NotificationData(_notificationFilter, UITextLibrary.GetString(UITextType.NotificationMultProbe), 3f);
			NotificationManager.SharedInstance.PostNotification(data);
			Locator.GetPlayerAudioController().PlayNegativeUISound();
			return;
		}
		_activeProbe.GetRotatingCamera().ResetRotation();
		_preLaunchProbeProxy.SetActive(value: true);
		if (playEffects)
		{
			_effects.PlayRetrievalClip();
			_probeRetrievalEffect.WarpObjectIn(_probeRetrievalLength);
		}
		_activeProbe.Retrieve(_probeRetrievalLength);
		_isRetrieving = true;
	}

	private void UpdateOrbitalLaunchValues()
	{
		_orbitSpeed = 0f;
		_orbitLaunchAngle = 0f;
		PlanetoidRuleset planetoidRuleset = Locator.GetPlayerRulesetDetector().GetPlanetoidRuleset();
		if (planetoidRuleset != null)
		{
			GravityVolume gravityVolume = planetoidRuleset.GetGravityVolume();
			Vector3 to = gravityVolume.CalculateForceAccelerationAtPoint(_launcherTransform.position);
			float magnitude = to.magnitude;
			if (magnitude > 0f)
			{
				_orbitSpeed = Mathf.Sqrt(magnitude * (gravityVolume.GetCenterPosition() - _launcherTransform.position).magnitude);
				_orbitLaunchAngle = Vector3.Angle(_launcherTransform.forward, to) - 90f;
			}
		}
	}

	private void TakeSnapshotWithCamera(ProbeCamera camera)
	{
		RenderTexture texture = (_lastSnapshot = camera.TakeSnapshot());
		_effects.PlaySnapshotClip(camera.GetID() == ProbeCamera.ID.Reverse);
		GlobalMessenger<ProbeCamera>.FireEvent("ProbeSnapshot", camera);
		if (this.OnTakeSnapshot != null)
		{
			float secondsAnchored = ((_activeProbe == null) ? 0f : _activeProbe.GetAnchor().GetSecondsAnchored());
			this.OnTakeSnapshot(texture, camera, secondsAnchored);
		}
	}

	protected virtual bool IsLaunchObstructed()
	{
		return false;
	}

	private float GetMaxLaunchSpeed()
	{
		switch (_name)
		{
		case Name.Ship:
			return 160f;
		case Name.Player:
			return 80f;
		case Name.Stationary:
			return 60f;
		default:
			return 80f;
		}
	}

	private void LaunchProbe()
	{
		if (IsLaunchObstructed())
		{
			NotificationData data = new NotificationData(_notificationFilter, UITextLibrary.GetString(UITextType.NotificationProbeLaunchObstructed), 2f, playSound: false);
			NotificationManager.SharedInstance.PostNotification(data);
			Locator.GetPlayerAudioController().PlayNegativeUISound();
			return;
		}
		_activeProbe = Locator.GetProbe();
		_allowRetrieval = false;
		_preLaunchProbeProxy.SetActive(value: false);
		float num = GetMaxLaunchSpeed();
		bool horizonTracking = false;
		if (_promptTrigger != null)
		{
			_promptTrigger.OnLaunchProbe();
		}
		if (_promptTrigger != null && _promptTrigger.GetOverrideLaunchSpeed())
		{
			num = _promptTrigger.GetLaunchSpeed();
		}
		else if (Locator.GetPlayerRulesetDetector().GetProbeRuleSet() != null && Locator.GetPlayerRulesetDetector().GetProbeRuleSet().GetUseProbeSpeedOverride())
		{
			num = Locator.GetPlayerRulesetDetector().GetProbeRuleSet().GetProbeSpeedOverride();
		}
		else if (Locator.GetPlayerRulesetDetector().GetPlanetoidRuleset() != null && Locator.GetPlayerRulesetDetector().GetPlanetoidRuleset().GetHorizonRadius() > 0f)
		{
			bool flag = false;
			if (Physics.Raycast(_launcherTransform.position, _launcherTransform.forward, out var hitInfo, 1000f, OWLayerMask.physicalMask) && hitInfo.rigidbody.mass > 100f && hitInfo.rigidbody != Locator.GetPlayerRulesetDetector().GetPlanetoidRuleset().GetAttachedOWRigidbody()
				.GetRigidbody())
			{
				flag = true;
			}
			if (!flag && _orbitSpeed > 0f)
			{
				if (_orbitLaunchAngle > -10f && _orbitLaunchAngle < 50f)
				{
					num = ((_orbitLaunchAngle > 0f) ? (_orbitSpeed * 1.1f / Mathf.Cos(_orbitLaunchAngle * ((float)Math.PI / 180f))) : _orbitSpeed);
					horizonTracking = true;
				}
				else if (_orbitLaunchAngle <= -10f)
				{
					num = _orbitSpeed;
				}
			}
		}
		OWRigidbody attachedOWRigidbody = Locator.GetPlayerTransform().GetAttachedOWRigidbody();
		Vector3 vector = attachedOWRigidbody.GetVelocity() + _launcherTransform.forward * num;
		Vector3 velocityChange = (attachedOWRigidbody.GetVelocity() - vector) * 0.05f;
		attachedOWRigidbody.AddVelocityChange(velocityChange);
		_activeProbe.Launch(_launcherTransform, vector, horizonTracking, GetHorizonRadiusOverride());
		bool underwater = _fluidDetector != null && _fluidDetector.InFluidType(FluidVolume.Type.WATER);
		_effects.PlayLaunchClip(underwater);
		_effects.PlayLaunchParticles(underwater);
		if (_name == Name.Player && num > 20f)
		{
			RumbleManager.PulseProbeLaunch();
		}
		GlobalMessenger<SurveyorProbe>.FireEvent("LaunchProbe", _activeProbe);
		if (this.OnLaunchProbe != null)
		{
			this.OnLaunchProbe(_activeProbe);
		}
	}

	private void OnProbeLauncherEquipped(ProbeLauncher launcher)
	{
		if (launcher == this)
		{
			Locator.GetPlayerAudioController().PlayEquipTool();
		}
		else if (_shareActiveProbes && launcher.SharesActiveProbes() && _activeProbe != null)
		{
			launcher.SetActiveProbe(_activeProbe);
		}
	}

	private void OnProbeLauncherUnequipped(ProbeLauncher launcher)
	{
		if (launcher == this)
		{
			Locator.GetPlayerAudioController().PlayUnequipTool();
		}
	}

	private void OnAnyProbeRetrieved(SurveyorProbe probe)
	{
		if (probe == _activeProbe)
		{
			_isRetrieving = false;
			if (this.OnRetrieveMyProbe != null)
			{
				this.OnRetrieveMyProbe();
			}
			_activeProbe = null;
		}
	}

	private void OnForceRetrieveProbe(SurveyorProbe probe)
	{
		if (probe == _activeProbe && !_isRetrieving)
		{
			RetrieveProbe(playEffects: true, forcedRetrieval: true);
			NotificationData data = new NotificationData(NotificationTarget.All, UITextLibrary.GetString(UITextType.NotificationScoutRecall));
			NotificationManager.SharedInstance.PostNotification(data);
		}
	}

	private void OnForceRetrieveProbeSilent(SurveyorProbe probe)
	{
		if (probe == _activeProbe && !_isRetrieving)
		{
			RetrieveProbe(playEffects: false, forcedRetrieval: true);
		}
	}

	private void SaveSnapshotToFile()
	{
		byte[] bytes = _lastSnapshot.ToTexture2D().EncodeToPNG();
		string text = Application.persistentDataPath + "/ProbeScreenshots/";
		string text2 = DateTime.Now.ToString("yy-MM-dd_hh-mm-ss");
		string text3 = "ProbePhoto_" + text2 + ".png";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		int num = 1;
		while (File.Exists(text + text3))
		{
			text3 = "ProbePhoto_" + text2 + "_" + num + ".png";
			num++;
		}
		File.WriteAllBytes(text + text3, bytes);
		MonoBehaviour.print("Probe snapshot saved to " + text + text3);
	}
}
