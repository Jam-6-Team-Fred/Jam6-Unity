using UnityEngine;

public class NomaiTranslator : PlayerTool
{
	public static float distToClosestTextCenter = 1f;

	public const float MAX_INTERACT_RANGE = 25f;

	[SerializeField]
	private Transform _raycastTransform;

	private Collider _lastHitCollider;

	private NomaiTranslatorProp _translatorProp;

	private NomaiText _currentNomaiText;

	private NomaiAudioVolume _currentNomaiAudioVolume;

	private PlayerAudioController _audioController;

	private bool _hasAimedTranslator;

	private bool _showIncompleteMessageOnUnequip;

	private NomaiTextLine _lastHighlightedTextLine;

	private bool _lastLineWasTranslated;

	private bool _lastLineLocked;

	private float _lastLineDist;

	private ScreenPrompt _centerAimTranslatorPrompt;

	private void Awake()
	{
		_lastHitCollider = null;
		_translatorProp = this.GetRequiredComponentInChildren<NomaiTranslatorProp>();
		_currentNomaiText = null;
		_currentNomaiAudioVolume = null;
		_centerAimTranslatorPrompt = new ScreenPrompt(InputLibrary.look, "<CMD>   " + UITextLibrary.GetString(UITextType.TranslatorAimPrompt));
	}

	protected override void Start()
	{
		base.Start();
		_audioController = Locator.GetPlayerTransform().GetComponentInChildren<PlayerAudioController>();
		Locator.GetPromptManager().AddScreenPrompt(_centerAimTranslatorPrompt, PromptPosition.Center);
		_hasAimedTranslator = PlayerData.GetPersistentCondition("HAS_AIMED_TRANSLATOR");
	}

	private void OnDisable()
	{
		_translatorProp.OnFinishUnequipAnimation();
	}

	public override void EquipTool()
	{
		base.EquipTool();
		_audioController.PlayEquipTool();
		_translatorProp.OnEquipTool();
		GlobalMessenger.FireEvent("EquipTranslator");
	}

	public override void UnequipTool()
	{
		base.UnequipTool();
		_translatorProp.OnUnequipTool();
		_audioController.PlayUnequipTool();
		_centerAimTranslatorPrompt.SetVisibility(isVisible: false);
		if (_showIncompleteMessageOnUnequip)
		{
			NotificationData notificationData = new NotificationData(UITextLibrary.GetString(UITextType.NotificationTextRemaining));
			NotificationManager.SharedInstance.PostNotification(notificationData);
			_showIncompleteMessageOnUnequip = false;
			GlobalMessenger<float>.FireEvent("UntranslatedTextRemainingNotificationPosted", notificationData.minDuration);
		}
		GlobalMessenger.FireEvent("UnequipTranslator");
	}

	public void SetNomaiAudio(NomaiAudioVolume nomaiAudioVolume)
	{
		_currentNomaiAudioVolume = nomaiAudioVolume;
		if (_translatorProp.SetNomaiAudio(_currentNomaiAudioVolume) && _currentNomaiAudioVolume != null && !base.enabled)
		{
			base.enabled = true;
		}
	}

	public void ClearNomaiAudio()
	{
		if (_currentNomaiAudioVolume != null)
		{
			_currentNomaiAudioVolume = null;
			_translatorProp.ClearNomaiAudio();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!_isEquipped)
		{
			return;
		}
		_centerAimTranslatorPrompt.SetVisibility(isVisible: false);
		distToClosestTextCenter = 1f;
		bool tooCloseToTarget = false;
		float num = float.MaxValue;
		if (Physics.Raycast(_raycastTransform.position, _raycastTransform.forward, out var hitInfo, 25f, OWLayerMask.blockableInteractMask))
		{
			_lastHitCollider = hitInfo.collider;
			_currentNomaiText = _lastHitCollider.GetComponent<NomaiText>();
			if (_currentNomaiText != null && !_currentNomaiText.CheckAllowFocus(hitInfo.distance, _raycastTransform.forward))
			{
				_currentNomaiText = null;
			}
			num = hitInfo.distance;
		}
		else
		{
			_lastHitCollider = null;
			_currentNomaiText = null;
		}
		if (_currentNomaiText != null)
		{
			tooCloseToTarget = num < _currentNomaiText.GetMinimumReadableDistance();
			if (_currentNomaiText is NomaiWallText)
			{
				NomaiTextLine nomaiTextLine = (_currentNomaiText as NomaiWallText).GetClosestTextLineByCenter(hitInfo.point);
				if (_lastLineLocked && _lastHighlightedTextLine != null)
				{
					float distToCenter = _lastHighlightedTextLine.GetDistToCenter(hitInfo.point);
					if (distToCenter > _lastLineDist + 0.1f)
					{
						_lastHighlightedTextLine = nomaiTextLine;
						_lastLineWasTranslated = nomaiTextLine != null && nomaiTextLine.IsTranslated();
						_lastLineLocked = false;
					}
					else
					{
						nomaiTextLine = _lastHighlightedTextLine;
					}
					if (distToCenter < _lastLineDist)
					{
						_lastLineDist = distToCenter;
					}
				}
				else if (_lastHighlightedTextLine != null && _lastHighlightedTextLine.IsTranslated() && !_lastLineWasTranslated)
				{
					_lastLineWasTranslated = true;
					_lastLineDist = _lastHighlightedTextLine.GetDistToCenter(hitInfo.point);
					_lastLineLocked = true;
				}
				else
				{
					_lastHighlightedTextLine = nomaiTextLine;
					_lastLineWasTranslated = nomaiTextLine != null && nomaiTextLine.IsTranslated();
				}
				if ((bool)nomaiTextLine && !nomaiTextLine.IsHidden() && nomaiTextLine.IsActive())
				{
					distToClosestTextCenter = Vector3.Distance(hitInfo.point, nomaiTextLine.GetWorldCenter());
					_translatorProp.SetNomaiText(_currentNomaiText, nomaiTextLine.GetEntryID());
					_translatorProp.SetNomaiTextLine(nomaiTextLine);
					if (!_hasAimedTranslator && _currentNomaiText.GetNumTextBlocks() > 1 && nomaiTextLine.IsTranslated())
					{
						if (_currentNomaiText.GetNumTextBlocksTranslated() > 1)
						{
							if (Locator.GetPlayerSuit().IsWearingHelmet())
							{
								PlayerData.SetPersistentCondition("HAS_AIMED_TRANSLATOR", state: true);
								_hasAimedTranslator = true;
								_showIncompleteMessageOnUnequip = false;
							}
						}
						else
						{
							_centerAimTranslatorPrompt.SetVisibility(isVisible: true);
							_centerAimTranslatorPrompt.SetDisplayState(ScreenPrompt.DisplayState.Attention);
							_showIncompleteMessageOnUnequip = Locator.GetPlayerSuit().IsWearingHelmet();
						}
					}
				}
				else
				{
					_translatorProp.ClearNomaiText();
					_translatorProp.ClearNomaiTextLine();
					_lastHighlightedTextLine = null;
					_lastLineWasTranslated = false;
					_lastLineLocked = false;
				}
			}
			else if (_currentNomaiText is NomaiComputer)
			{
				float verticalDist;
				NomaiComputerRing closestRing = (_currentNomaiText as NomaiComputer).GetClosestRing(hitInfo.point, out verticalDist);
				if ((bool)closestRing)
				{
					distToClosestTextCenter = Mathf.Min(verticalDist * 2f, 1f);
					_translatorProp.SetNomaiText(_currentNomaiText, closestRing.GetEntryID());
					_translatorProp.SetNomaiComputerRing(closestRing);
				}
			}
			else if (_currentNomaiText is NomaiVesselComputer)
			{
				float verticalDist2;
				NomaiVesselComputerRing closestRing2 = (_currentNomaiText as NomaiVesselComputer).GetClosestRing(hitInfo.point, out verticalDist2);
				if ((bool)closestRing2)
				{
					distToClosestTextCenter = Mathf.Min(verticalDist2 * 2f, 1f);
					_translatorProp.SetNomaiText(_currentNomaiText, closestRing2.GetEntryID());
					_translatorProp.SetNomaiVesselComputerRing(closestRing2);
				}
			}
			else if (_currentNomaiText is GhostWallText)
			{
				GhostWallText ghostWallText = _currentNomaiText as GhostWallText;
				_translatorProp.SetTargetingGhostText(isTargetingGhostText: true);
				_translatorProp.SetNomaiTextLine(ghostWallText.GetTextLine());
			}
			else if (hitInfo.textureCoord2 == Vector2.zero)
			{
				_translatorProp.SetNomaiText(_currentNomaiText);
			}
			else
			{
				int textID = Mathf.RoundToInt(hitInfo.textureCoord2.x * 10f);
				_translatorProp.SetNomaiText(_currentNomaiText, textID);
			}
		}
		else if (_currentNomaiAudioVolume == null)
		{
			_translatorProp.ClearNomaiText();
			_translatorProp.ClearNomaiTextLine();
			_translatorProp.ClearNomaiComputerRing();
			_translatorProp.ClearNomaiVesselComputerRing();
			_translatorProp.SetTargetingGhostText(isTargetingGhostText: false);
		}
		_translatorProp.SetTooCloseToTarget(tooCloseToTarget);
	}
}
