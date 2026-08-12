using UnityEngine;
using UnityEngine.UI;

public class JetpackPromptController : MonoBehaviour, ILateInitializer
{
	public static bool SPLIT_VERTICAL = false;

	private static bool s_boostPromptReminder = false;

	private static float s_boostPromptDuration = 5f;

	[SerializeField]
	private HUDCanvas _hudCanvas;

	private bool _screenPromptsInitialized;

	private ScreenPrompt _boostGaugePrompt;

	private ScreenPrompt _rollPrompt;

	private ScreenPrompt _matchVelocityPrompt;

	private ScreenPrompt _verticalThrust;

	private ScreenPrompt _verticalThrustCenter;

	private ScreenPrompt _downThrust;

	private ScreenPrompt _downThrustCenter;

	private ScreenPrompt _horizontalThrustPrompt;

	private JetpackThrusterController _jetpackController;

	private PlayerCharacterController _playerController;

	private JetpackThrusterModel _jetpackThruster;

	private PlayerSpacesuit _playerSuit;

	private float _boostUseAccumulator;

	private float _upThrustAccumulator;

	private bool _hasUsedJetpack;

	private bool _hasUsedBooster;

	private bool _hasUnlockedPreflightChecklist;

	private bool _moveBoostPromptToGauge;

	private float _startBoostPromptMoveTime;

	private void Awake()
	{
		_screenPromptsInitialized = false;
		LateInitializerManager.RegisterLateInitializer(this);
		_boostGaugePrompt = new ScreenPrompt(InputLibrary.boost, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.JetpackBoosterPrompt), 1);
		_rollPrompt = new ScreenPrompt(InputLibrary.rollMode, InputLibrary.look, UITextLibrary.GetString(UITextType.RollPrompt) + " <CMD1>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "  +<CMD2>", ScreenPrompt.MultiCommandType.CUSTOM_BOTH);
		_matchVelocityPrompt = new ScreenPrompt(InputLibrary.matchVelocity, UITextLibrary.GetString(UITextType.MatchVelocityPrompt) + " <CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "  ");
		if (SPLIT_VERTICAL)
		{
			_verticalThrust = new ScreenPrompt(InputLibrary.thrustUp, UITextLibrary.GetString(UITextType.UpPrompt));
			_verticalThrustCenter = new ScreenPrompt(InputLibrary.thrustUp, UITextLibrary.GetString(UITextType.UpPrompt));
			_downThrust = new ScreenPrompt(InputLibrary.thrustDown, UITextLibrary.GetString(UITextType.DownPrompt));
			_downThrustCenter = new ScreenPrompt(InputLibrary.thrustDown, UITextLibrary.GetString(UITextType.DownPrompt));
		}
		else
		{
			_verticalThrust = new ScreenPrompt(InputLibrary.thrustDown, InputLibrary.thrustUp, UITextLibrary.GetString(UITextType.DownUpPrompt), ScreenPrompt.MultiCommandType.POS_NEG);
			_verticalThrustCenter = new ScreenPrompt(InputLibrary.thrustDown, InputLibrary.thrustUp, UITextLibrary.GetString(UITextType.DownUpPrompt), ScreenPrompt.MultiCommandType.POS_NEG);
		}
		_horizontalThrustPrompt = new ScreenPrompt(InputLibrary.moveXZ, UITextLibrary.GetString(UITextType.HorizontalPrompt));
		_jetpackController = base.gameObject.GetRequiredComponent<JetpackThrusterController>();
		_jetpackThruster = base.gameObject.GetRequiredComponent<JetpackThrusterModel>();
		_playerController = base.gameObject.GetRequiredComponent<PlayerCharacterController>();
		_playerSuit = base.gameObject.GetRequiredComponent<PlayerSpacesuit>();
		GlobalMessenger.AddListener("SuitUp", OnSuitUp);
		GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
	}

	private void Start()
	{
		_hasUsedJetpack = PlayerData.GetPersistentCondition("HAS_USED_JETPACK");
		_hasUsedBooster = PlayerData.GetPersistentCondition("SUIT_BOOSTER_FIRED");
		_hasUnlockedPreflightChecklist = PlayerData.GetPersistentCondition("PREFLIGHT_CHECKLIST_UNLOCKED");
		if (!_hasUsedBooster)
		{
			s_boostPromptReminder = false;
			s_boostPromptDuration = 5f;
		}
		else if (LoadManager.GetPreviousScene() == OWScene.TitleScreen)
		{
			s_boostPromptReminder = true;
			s_boostPromptDuration = 3f;
		}
		if (s_boostPromptReminder)
		{
			_hasUsedBooster = false;
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (!_screenPromptsInitialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
		GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
		GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
	}

	public void LateInitialize()
	{
		_screenPromptsInitialized = true;
		Locator.GetPromptManager().AddScreenPrompt(_verticalThrustCenter, PromptPosition.Center);
		if (SPLIT_VERTICAL)
		{
			Locator.GetPromptManager().AddScreenPrompt(_downThrustCenter, PromptPosition.Center);
		}
		Locator.GetPromptManager().AddScreenPrompt(_rollPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_matchVelocityPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_horizontalThrustPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_verticalThrust, PromptPosition.UpperRight);
		if (SPLIT_VERTICAL)
		{
			Locator.GetPromptManager().AddScreenPrompt(_downThrust, PromptPosition.UpperRight);
		}
		Locator.GetPromptManager().AddScreenPrompt(_boostGaugePrompt, PromptPosition.BoostMeter);
		Locator.GetPromptManager().GetScreenPromptList(PromptPosition.BoostMeter).gameObject.GetComponentInChildren<ScreenPromptElement>(includeInactive: true).gameObject.GetComponent<LayoutElement>().minWidth = -1f;
	}

	public bool AllowMatchVelocityPrompt()
	{
		if (_jetpackController.IsMatchVelocityAvailable(!PlayerState.InBrambleDimension()))
		{
			return _jetpackController.GetLocalVelocity().magnitude > 1f;
		}
		return false;
	}

	private void OnSuitUp()
	{
		base.enabled = true;
	}

	private void OnRemoveSuit()
	{
		base.enabled = false;
		HideAllPrompts();
	}

	private void HideAllPrompts()
	{
		if (SPLIT_VERTICAL)
		{
			_downThrust.SetVisibility(isVisible: false);
			_downThrustCenter.SetVisibility(isVisible: false);
		}
		_verticalThrustCenter.SetVisibility(isVisible: false);
		_matchVelocityPrompt.SetVisibility(isVisible: false);
		_horizontalThrustPrompt.SetVisibility(isVisible: false);
		_verticalThrust.SetVisibility(isVisible: false);
		_rollPrompt.SetVisibility(isVisible: false);
		_boostGaugePrompt.SetVisibility(isVisible: false);
	}

	private void Update()
	{
		HideAllPrompts();
		if (!OWInput.IsInputMode(InputMode.Character) || PlayerState.IsInsideShip())
		{
			return;
		}
		if (Locator.GetToolModeSwapper().IsInToolMode(ToolMode.None) || Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item))
		{
			if (AllowMatchVelocityPrompt() && !PlayerState.InZeroGTraining())
			{
				_matchVelocityPrompt.SetVisibility(isVisible: true);
			}
			else if (PlayerState.InZeroGTraining())
			{
				_horizontalThrustPrompt.SetVisibility(isVisible: true);
				_verticalThrust.SetVisibility(isVisible: true);
				if (SPLIT_VERTICAL)
				{
					_downThrust.SetVisibility(isVisible: true);
				}
			}
			else if ((_playerSuit.IsTrainingSuit() || !_hasUsedJetpack) && (!PlayerState.InZeroG() || Locator.GetReferenceFrame() == null))
			{
				_horizontalThrustPrompt.SetVisibility(!_playerController.IsGrounded());
				_verticalThrust.SetVisibility(isVisible: true);
				if (SPLIT_VERTICAL)
				{
					_downThrust.SetVisibility(isVisible: true);
				}
				if (!_hasUsedJetpack)
				{
					if (_playerSuit.IsTrainingSuit())
					{
						_verticalThrust.SetVisibility(isVisible: false);
						_verticalThrustCenter.SetVisibility(isVisible: true);
						if (SPLIT_VERTICAL)
						{
							_downThrust.SetVisibility(isVisible: false);
							_downThrustCenter.SetVisibility(isVisible: true);
						}
					}
					if (_jetpackThruster.GetLocalAcceleration().y > 0f && !_playerController.IsGrounded())
					{
						_upThrustAccumulator += Time.deltaTime;
					}
					if (_upThrustAccumulator > 5f)
					{
						_hasUsedJetpack = true;
						PlayerData.SetPersistentCondition("HAS_USED_JETPACK", state: true);
					}
				}
			}
			if (PlayerState.InZeroG())
			{
				_rollPrompt.SetVisibility(isVisible: true);
			}
		}
		if (!_jetpackController.AllowBoostPrompt() || !(Locator.GetPlayerForceDetector().GetAlignmentAcceleration().magnitude * 1.1f > _jetpackThruster.GetMaxTranslationalThrust()))
		{
			return;
		}
		if (_jetpackThruster.IsBoosterFiring())
		{
			_boostUseAccumulator += Time.deltaTime;
		}
		if (!_hasUnlockedPreflightChecklist && _boostUseAccumulator > 2f)
		{
			_hasUnlockedPreflightChecklist = true;
			PlayerData.SetPersistentCondition("PREFLIGHT_CHECKLIST_UNLOCKED", state: true);
			MonoBehaviour.print("unlocked preflight checklist (next loop)");
		}
		if (!_hasUsedBooster && _boostUseAccumulator > s_boostPromptDuration)
		{
			_hasUsedBooster = true;
			s_boostPromptReminder = false;
			PlayerData.SetPersistentCondition("SUIT_BOOSTER_FIRED", state: true);
		}
		if (!_hasUsedBooster)
		{
			_boostGaugePrompt.SetVisibility(isVisible: true);
			RectTransform component = Locator.GetPromptManager().GetScreenPromptList(PromptPosition.BoostMeter).GetComponent<RectTransform>();
			Vector3 boostGaugeViewportPosition = _hudCanvas.GetBoostGaugeViewportPosition();
			boostGaugeViewportPosition.x *= 0.95f;
			boostGaugeViewportPosition.y *= 1.1f;
			if (!_moveBoostPromptToGauge && _jetpackThruster.IsBoosterFiring())
			{
				_moveBoostPromptToGauge = true;
				_startBoostPromptMoveTime = Time.time;
			}
			float t = (_moveBoostPromptToGauge ? Mathf.InverseLerp(_startBoostPromptMoveTime, _startBoostPromptMoveTime + 0.3f, Time.time) : 0f);
			t = Mathf.SmoothStep(0f, 1f, t);
			Vector3 vector = Vector3.Lerp(new Vector3(0.5f, 0.6f, 0f), boostGaugeViewportPosition, t);
			Vector2 sizeDelta = component.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta;
			component.anchoredPosition = new Vector2(sizeDelta.x * vector.x, sizeDelta.y * vector.y);
			Vector2 pivot = component.pivot;
			pivot.x = Mathf.Lerp(0.5f, 0f, t);
			component.pivot = pivot;
		}
	}
}
