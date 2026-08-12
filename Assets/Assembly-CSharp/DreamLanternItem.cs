using UnityEngine;

public class DreamLanternItem : OWItem
{
	private const float FOCUS_SPEED = 2f;

	[SerializeField]
	private DreamLanternType _lanternType = DreamLanternType.Functioning;

	[SerializeField]
	private bool _startLit;

	[Space]
	[SerializeField]
	private FluidDetector _fluidDetector;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	private DreamLanternController _lanternController;

	private float _forceUnfocusTime;

	private bool _wasFocusing;

	private bool _focusing;

	private float _origMinRange;

	private float _origMaxRange;

	protected override void Awake()
	{
		_type = ItemType.DreamLantern;
		_lanternController = GetComponent<DreamLanternController>();
		base.Awake();
	}

	private void Start()
	{
		if (_lanternController != null)
		{
			_origMinRange = _lanternController.GetMinRange();
			_origMaxRange = _lanternController.GetMaxRange();
			_lanternController.SetLit(_startLit);
			_lanternController.enabled = _startLit;
		}
		if (_fluidDetector != null)
		{
			_fluidDetector.GetShape().SetActivation(newActive: false);
		}
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		if (_fluidDetector != null)
		{
			_fluidDetector.OnEnterFluidType -= OnEnterFluidType;
		}
		base.OnDestroy();
	}

	public override string GetDisplayName()
	{
		return UITextLibrary.GetString(UITextType.ItemUnknownArtifactPrompt);
	}

	public DreamLanternType GetLanternType()
	{
		return _lanternType;
	}

	public DreamLanternController GetLanternController()
	{
		return _lanternController;
	}

	public FluidDetector GetFluidDetector()
	{
		return _fluidDetector;
	}

	public void OverrideMaxRunSpeed(ref float maxSpeedX, ref float maxSpeedZ)
	{
		float num = 1f - _lanternController.GetFocus();
		num *= num;
		maxSpeedX = Mathf.Lerp(2f, maxSpeedX, num);
		maxSpeedZ = Mathf.Lerp(2f, maxSpeedZ, num);
	}

	public void OnEnterDreamWorld()
	{
		_lanternController.SetConcealed(concealed: false);
		_fluidDetector.GetShape().SetActivation(newActive: true);
		_fluidDetector.OnEnterFluidType += OnEnterFluidType;
	}

	public void OnExitDreamWorld()
	{
		SetLit(lit: false);
		_fluidDetector.GetShape().SetActivation(newActive: false);
		_fluidDetector.OnEnterFluidType -= OnEnterFluidType;
		_lanternController.grabbedByGhost = false;
	}

	public void SetLit(bool lit)
	{
		if (_lanternController.IsLit() != lit)
		{
			if (_oneShotSource != null)
			{
				_oneShotSource.PlayOneShot(lit ? AudioType.Artifact_Light : AudioType.Artifact_Extinguish);
			}
			_lanternController.SetLit(lit);
		}
	}

	public override void PickUpItem(Transform holdTranform)
	{
		base.PickUpItem(holdTranform);
		if (_lanternType == DreamLanternType.Functioning)
		{
			base.enabled = true;
		}
		if (_lanternController != null)
		{
			_lanternController.enabled = true;
			_lanternController.SetDetectorScaleCompensation(_lanternController.transform.lossyScale);
			_lanternController.SetHeldByPlayer(heldByPlayer: true);
			Locator.GetPlayerController().SetDreamLantern(this);
		}
	}

	public override bool CheckIsDroppable()
	{
		if (_lanternController != null && (_lanternController.IsFocused(0.01f) || _lanternController.IsConcealed()))
		{
			return false;
		}
		if (!PlayerState.InDreamWorld())
		{
			return true;
		}
		bool num = Locator.GetPlayerCameraController().GetDegreesY() < Locator.GetPlayerCameraController().GetMinDegreesY() + 20f;
		bool flag = Locator.GetPlayerController().GetRelativeGroundVelocity().sqrMagnitude < Locator.GetPlayerController().GetWalkSpeedMagnitude() * Locator.GetPlayerController().GetWalkSpeedMagnitude();
		return num && flag;
	}

	public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
	{
		base.DropItem(position, normal, parent, sector, customDropTarget);
		base.enabled = false;
		if (_lanternController != null)
		{
			_lanternController.SetDetectorScaleCompensation(_lanternController.transform.lossyScale);
			_lanternController.SetHeldByPlayer(heldByPlayer: false);
			_lanternController.enabled = _lanternController.IsLit();
			Locator.GetPlayerController().SetDreamLantern(null);
		}
	}

	public override void SocketItem(Transform socketTransform, Sector sector)
	{
		base.SocketItem(socketTransform, sector);
		base.enabled = false;
		if (_lanternController != null)
		{
			_lanternController.SetDetectorScaleCompensation(_lanternController.transform.lossyScale);
			_lanternController.SetSocketed(socketed: true);
			_lanternController.SetHeldByPlayer(heldByPlayer: false);
			_lanternController.enabled = _lanternController.IsLit();
			Locator.GetPlayerController().SetDreamLantern(null);
		}
	}

	public override void OnCompleteUnsocket()
	{
		if (_lanternController != null)
		{
			_lanternController.SetSocketed(socketed: false);
		}
	}

	public void ForceUnfocus()
	{
		_forceUnfocusTime = Time.time;
		_focusing = false;
		UpdateFocus();
	}

	private void Update()
	{
		bool flag = Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item);
		_wasFocusing = _focusing;
		_focusing = OWInput.IsPressed(InputLibrary.toolActionPrimary, InputMode.Character) && Time.time > _forceUnfocusTime + 1f && flag;
		bool flag2 = OWInput.IsPressed(InputLibrary.toolActionSecondary, InputMode.Character) && flag;
		if (flag2 && !_lanternController.IsConcealed())
		{
			Locator.GetPlayerAudioController().OnArtifactConceal();
			_lanternController.SetConcealed(concealed: true);
		}
		else if (!flag2 && _lanternController.IsConcealed())
		{
			Locator.GetPlayerAudioController().OnArtifactUnconceal();
			_lanternController.SetConcealed(concealed: false);
		}
		if (_focusing != _wasFocusing)
		{
			if (_focusing)
			{
				Locator.GetPlayerAudioController().OnArtifactFocus();
			}
			else
			{
				Locator.GetPlayerAudioController().OnArtifactUnfocus();
			}
		}
		UpdateFocus();
	}

	private void UpdateFocus()
	{
		DreamLanternRuleset dreamLanternRuleset = Locator.GetPlayerRulesetDetector().GetDreamLanternRuleset();
		float target = ((dreamLanternRuleset == null) ? _origMinRange : dreamLanternRuleset.minRangeOverride);
		float target2 = ((dreamLanternRuleset == null) ? _origMaxRange : dreamLanternRuleset.maxRangeOverride);
		float num = ((dreamLanternRuleset == null) ? 1f : dreamLanternRuleset.transitionRate);
		if (_lanternController.grabbedByGhost)
		{
			target = 1.7f;
			num = 5f;
		}
		_lanternController.SetRange(Mathf.MoveTowards(_lanternController.GetMinRange(), target, num * Time.deltaTime), Mathf.MoveTowards(_lanternController.GetMaxRange(), target2, num * Time.deltaTime));
		if (_focusing)
		{
			_lanternController.MoveTowardFocus(1f, 2f);
		}
		else
		{
			_lanternController.MoveTowardFocus(0f, 4f);
		}
	}

	private void OnEnterFluidType(FluidVolume.Type fluidType)
	{
		if (fluidType == FluidVolume.Type.WATER)
		{
			Locator.GetDreamWorldController().ExitDreamWorld(DreamWakeType.LanternSubmerged);
		}
	}
}
