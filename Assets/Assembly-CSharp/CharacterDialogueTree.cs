using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class CharacterDialogueTree : MonoBehaviour, ILateInitializer
{
	public delegate void ConversationEvent();

	public delegate void ConversationPageEvent(string nodeName, int pageNum);

	[SerializeField]
	private TextAsset _xmlCharacterDialogueAsset;

	[SerializeField]
	private Transform _attentionPoint;

	[SerializeField]
	private Vector3 _attentionPointOffset = Vector3.zero;

	[SerializeField]
	private bool _turnOffFlashlight = true;

	[SerializeField]
	private bool _turnOnFlashlight = true;

	private SingleInteractionVolume _interactVolume;

	private string _characterName;

	private bool _initialized;

	private IDictionary<string, DialogueNode> _mapDialogueNodes;

	private List<DialogueOption> _listOptionNodes;

	private DialogueNode _currentNode;

	private DialogueBoxVer2 _currentDialogueBox;

	private bool _wasFlashlightOn;

	private bool _timeFrozen;

	private bool _isRecording;

	private const string SIGN_NAME = "SIGN";

	private const string RECORDING_NAME = "RECORDING";

	private const float MINIMUM_TIME_TEXT_VISIBLE = 0.1f;

	public event ConversationEvent OnStartConversation;

	public event ConversationEvent OnEndConversation;

	public event ConversationPageEvent OnAdvancePage;

	public event ConversationEvent OnSelectDialogueOption;

	private void OnValidate()
	{
		if (_turnOnFlashlight && !_turnOffFlashlight)
		{
			_turnOnFlashlight = false;
		}
	}

	private void Awake()
	{
		_attentionPoint = ((_attentionPoint == null) ? base.transform : _attentionPoint);
		_initialized = false;
		LateInitializerManager.RegisterLateInitializer(this);
		_interactVolume = this.GetRequiredComponent<SingleInteractionVolume>();
		_interactVolume.OnPressInteract += OnPressInteract;
		GlobalMessenger.AddListener("DialogueConditionsReset", OnDialogueConditionsReset);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (!_initialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
		GlobalMessenger.RemoveListener("DialogueConditionsReset", OnDialogueConditionsReset);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
	}

	public void LateInitialize()
	{
		_mapDialogueNodes = new Dictionary<string, DialogueNode>(ComparerLibrary.stringEqComparer);
		_listOptionNodes = new List<DialogueOption>();
		_initialized = LoadXml();
		if (_interactVolume != null && _characterName != string.Empty)
		{
			if (_characterName == "SIGN")
			{
				_interactVolume.SetPromptText(UITextType.ReadPrompt);
			}
			else if (_characterName == "RECORDING")
			{
				_isRecording = true;
				_interactVolume.SetPromptText(UITextType.RecordingPrompt);
			}
			else
			{
				_interactVolume.SetPromptText(UITextType.TalkToPrompt, TextTranslation.Translate(_characterName));
			}
		}
	}

	public void VerifyInitialized()
	{
		if (!_initialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
			LateInitialize();
		}
	}

	public void SetTextXml(TextAsset textAsset)
	{
		_xmlCharacterDialogueAsset = textAsset;
		if (_initialized)
		{
			LoadXml();
		}
	}

	public bool InConversation()
	{
		return base.enabled;
	}

	public InteractVolume GetInteractVolume()
	{
		return _interactVolume;
	}

	private void Update()
	{
		if (!(_currentDialogueBox != null) || OWInput.GetInputMode() != InputMode.Dialogue)
		{
			return;
		}
		if (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.cancel) || OWInput.IsNewlyPressed(InputLibrary.enter) || OWInput.IsNewlyPressed(InputLibrary.enter2))
		{
			if (!_currentDialogueBox.AreTextEffectsComplete())
			{
				_currentDialogueBox.FinishAllTextEffects();
			}
			else if (!(_currentDialogueBox.TimeCompletelyRevealed() < 0.1f))
			{
				int selectedOption = _currentDialogueBox.GetSelectedOption();
				if (!InputDialogueOption(selectedOption))
				{
					EndConversation();
				}
			}
		}
		else if (OWInput.IsNewlyPressed(InputLibrary.down) || OWInput.IsNewlyPressed(InputLibrary.down2))
		{
			_currentDialogueBox.OnDownPressed();
		}
		else if (OWInput.IsNewlyPressed(InputLibrary.up) || OWInput.IsNewlyPressed(InputLibrary.up2))
		{
			_currentDialogueBox.OnUpPressed();
		}
	}

	private void SetEntryNode()
	{
		List<DialogueNode> list = new List<DialogueNode>();
		list.AddRange(_mapDialogueNodes.Values);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].EntryConditionsSatisfied())
			{
				_currentNode = list[num];
				return;
			}
		}
		Debug.LogWarning("CharacterDialogueTree " + _characterName + " no EntryConditions satisfied. Did you forget to add <EntryCondition>DEFAULT</EntryCondition> to a node?");
	}

	private bool LoadXml()
	{
		try
		{
			_mapDialogueNodes.Clear();
			_listOptionNodes.Clear();
			DialogueConditionManager sharedInstance = DialogueConditionManager.SharedInstance;
			XElement xElement = XDocument.Parse(OWUtilities.RemoveByteOrderMark(_xmlCharacterDialogueAsset)).Element("DialogueTree");
			XElement xElement2 = xElement.Element("NameField");
			if (xElement2 == null)
			{
				_characterName = "";
			}
			else
			{
				_characterName = xElement2.Value;
			}
			foreach (XElement item in xElement.Elements("DialogueNode"))
			{
				DialogueNode dialogueNode = new DialogueNode();
				if (item.Element("Name") != null)
				{
					dialogueNode.Name = item.Element("Name").Value;
				}
				else
				{
					Debug.LogWarning("Missing name on a Node");
				}
				bool randomize = item.Element("Randomize") != null;
				dialogueNode.DisplayTextData = new DialogueText(item.Elements("Dialogue"), randomize);
				XElement xElement3 = item.Element("DialogueTarget");
				if (xElement3 != null)
				{
					dialogueNode.TargetName = xElement3.Value;
				}
				IEnumerable<XElement> enumerable = item.Elements("EntryCondition");
				if (enumerable != null)
				{
					foreach (XElement item2 in enumerable)
					{
						dialogueNode.ListEntryCondition.Add(item2.Value);
						if (!sharedInstance.ConditionExists(item2.Value))
						{
							sharedInstance.AddCondition(item2.Value);
						}
					}
				}
				IEnumerable<XElement> enumerable2 = item.Elements("DialogueTargetShipLogCondition");
				if (enumerable2 != null)
				{
					foreach (XElement item3 in enumerable2)
					{
						dialogueNode.ListTargetCondition.Add(item3.Value);
					}
				}
				foreach (XElement item4 in item.Elements("SetCondition"))
				{
					dialogueNode.ConditionsToSet.Add(item4.Value);
				}
				XElement xElement4 = item.Element("SetPersistentCondition");
				if (xElement4 != null)
				{
					dialogueNode.PersistentConditionToSet = xElement4.Value;
				}
				XElement xElement5 = item.Element("DisablePersistentCondition");
				if (xElement5 != null)
				{
					dialogueNode.PersistentConditionToDisable = xElement5.Value;
				}
				XElement xElement6 = item.Element("RevealFacts");
				if (xElement6 != null)
				{
					IEnumerable<XElement> enumerable3 = xElement6.Elements("FactID");
					if (enumerable3 != null)
					{
						List<string> list = new List<string>();
						foreach (XElement item5 in enumerable3)
						{
							list.Add(item5.Value);
						}
						dialogueNode.DBEntriesToSet = new string[list.Count];
						for (int i = 0; i < list.Count; i++)
						{
							dialogueNode.DBEntriesToSet[i] = list[i];
						}
					}
				}
				List<DialogueOption> list2 = new List<DialogueOption>();
				XElement xElement7 = item.Element("DialogueOptionsList");
				if (xElement7 != null)
				{
					IEnumerable<XElement> enumerable4 = xElement7.Elements("DialogueOption");
					if (enumerable4 != null)
					{
						foreach (XElement item6 in enumerable4)
						{
							DialogueOption dialogueOption = new DialogueOption();
							dialogueOption.Text = OWUtilities.CleanupXmlText(item6.Element("Text").Value, replaceBlackslashN: false);
							dialogueOption.SetNodeId(dialogueNode.Name, _characterName);
							XElement xElement8 = item6.Element("DialogueTarget");
							if (xElement8 != null)
							{
								dialogueOption.TargetName = xElement8.Value;
							}
							if (item6.Element("RequiredCondition") != null)
							{
								dialogueOption.ConditionRequirement = item6.Element("RequiredCondition").Value;
								if (!sharedInstance.ConditionExists(dialogueOption.ConditionRequirement))
								{
									sharedInstance.AddCondition(dialogueOption.ConditionRequirement);
								}
							}
							foreach (XElement item7 in item6.Elements("RequiredPersistentCondition"))
							{
								dialogueOption.PersistentCondition.Add(item7.Value);
							}
							if (item6.Element("CancelledCondition") != null)
							{
								dialogueOption.CancelledRequirement = item6.Element("CancelledCondition").Value;
								if (!sharedInstance.ConditionExists(dialogueOption.CancelledRequirement))
								{
									sharedInstance.AddCondition(dialogueOption.CancelledRequirement);
								}
							}
							foreach (XElement item8 in item6.Elements("CancelledPersistentCondition"))
							{
								dialogueOption.CancelledPersistentRequirement.Add(item8.Value);
							}
							foreach (XElement item9 in item6.Elements("RequiredLogCondition"))
							{
								dialogueOption.LogConditionRequirement.Add(item9.Value);
							}
							if (item6.Element("ConditionToSet") != null)
							{
								dialogueOption.ConditionToSet = item6.Element("ConditionToSet").Value;
								if (!sharedInstance.ConditionExists(dialogueOption.ConditionToSet))
								{
									sharedInstance.AddCondition(dialogueOption.ConditionToSet);
								}
							}
							if (item6.Element("ConditionToCancel") != null)
							{
								dialogueOption.ConditionToCancel = item6.Element("ConditionToCancel").Value;
								if (!sharedInstance.ConditionExists(dialogueOption.ConditionToCancel))
								{
									sharedInstance.AddCondition(dialogueOption.ConditionToCancel);
								}
							}
							list2.Add(dialogueOption);
							_listOptionNodes.Add(dialogueOption);
						}
					}
				}
				dialogueNode.ListDialogueOptions = list2;
				_mapDialogueNodes.Add(dialogueNode.Name, dialogueNode);
			}
			List<DialogueNode> list3 = new List<DialogueNode>();
			list3.AddRange(_mapDialogueNodes.Values);
			for (int j = 0; j < list3.Count; j++)
			{
				DialogueNode dialogueNode2 = list3[j];
				string targetName = dialogueNode2.TargetName;
				if (targetName != "")
				{
					if (_mapDialogueNodes.ContainsKey(targetName))
					{
						dialogueNode2.Target = _mapDialogueNodes[targetName];
					}
					else
					{
						Debug.LogError("Target Node: " + targetName + " does not exist", this);
					}
				}
				else
				{
					dialogueNode2.Target = null;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("XML Error in CharacterDialogueTree!\n" + ex.Message, this);
			return false;
		}
		return true;
	}

	private void OnPressInteract()
	{
		StartConversation();
	}

	public void StartConversation()
	{
		VerifyInitialized();
		base.enabled = true;
		if (!_timeFrozen && PlayerData.GetFreezeTimeWhileReadingConversations() && !Locator.GetGlobalMusicController().IsEndTimesPlaying())
		{
			_timeFrozen = true;
			OWTime.Pause(OWTime.PauseType.Reading);
		}
		Locator.GetToolModeSwapper().UnequipTool();
		if (this.OnStartConversation != null)
		{
			this.OnStartConversation();
		}
		GlobalMessenger.FireEvent("EnterConversation");
		Locator.GetPlayerAudioController().PlayDialogueEnter(_isRecording);
		_wasFlashlightOn = Locator.GetFlashlight().IsFlashlightOn();
		if (_wasFlashlightOn && _turnOffFlashlight)
		{
			Locator.GetFlashlight().TurnOff(playAudio: false);
		}
		DialogueConditionManager.SharedInstance.ReadPlayerData();
		SetEntryNode();
		_currentDialogueBox = DisplayDialogueBox2();
		if (_attentionPoint != null && !PlayerState.InZeroG())
		{
			Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(_attentionPoint, _attentionPointOffset, 2f);
		}
		if (PlayerState.InZeroG() && !_timeFrozen)
		{
			Locator.GetPlayerBody().GetComponent<Autopilot>().StartMatchVelocity(this.GetAttachedOWRigidbody().GetReferenceFrame());
		}
	}

	public void EndConversation()
	{
		if (!base.enabled)
		{
			return;
		}
		base.enabled = false;
		if (_timeFrozen)
		{
			_timeFrozen = false;
			OWTime.Unpause(OWTime.PauseType.Reading);
		}
		_interactVolume.ResetInteraction();
		Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().BreakLock();
		if (this.OnEndConversation != null)
		{
			this.OnEndConversation();
		}
		GlobalMessenger.FireEvent("ExitConversation");
		Locator.GetPlayerAudioController().PlayDialogueExit(_isRecording);
		if (_wasFlashlightOn && _turnOffFlashlight && _turnOnFlashlight)
		{
			Locator.GetFlashlight().TurnOn(playAudio: false);
		}
		GameObject.FindWithTag("DialogueGui").GetRequiredComponent<DialogueBoxVer2>().OnEndDialogue();
		if (_currentNode != null)
		{
			_currentNode.SetNodeCompleted();
		}
		if (PlayerState.InZeroG())
		{
			Autopilot component = Locator.GetPlayerBody().GetComponent<Autopilot>();
			if (component.enabled)
			{
				component.Abort();
			}
		}
	}

	private bool InputDialogueOption(int optionIndex)
	{
		bool result = true;
		bool flag = true;
		if (optionIndex >= 0)
		{
			DialogueOption selectedOption = _currentDialogueBox.OptionFromUIIndex(optionIndex);
			ContinueToNextNode(selectedOption);
		}
		else if (!_currentNode.HasNext())
		{
			ContinueToNextNode();
		}
		if (_currentNode == null)
		{
			_currentDialogueBox = null;
			flag = false;
			result = false;
		}
		else
		{
			_currentDialogueBox = DisplayDialogueBox2();
		}
		if (this.OnSelectDialogueOption != null)
		{
			this.OnSelectDialogueOption();
		}
		if (flag)
		{
			Locator.GetPlayerAudioController().PlayDialogueAdvance();
		}
		return result;
	}

	private DialogueBoxVer2 DisplayDialogueBox2()
	{
		string mainText = "";
		List<DialogueOption> options = new List<DialogueOption>();
		_currentNode.RefreshConditionsData();
		if (_currentNode.HasNext())
		{
			_currentNode.GetNextPage(out mainText, ref options);
			if (this.OnAdvancePage != null)
			{
				this.OnAdvancePage(_currentNode.Name, _currentNode.CurrentPage);
			}
		}
		DialogueBoxVer2 requiredComponent = GameObject.FindWithTag("DialogueGui").GetRequiredComponent<DialogueBoxVer2>();
		requiredComponent.SetVisible(value: true);
		requiredComponent.SetDialogueText(mainText, options);
		if (_characterName == "")
		{
			requiredComponent.SetNameFieldVisible(value: false);
		}
		else
		{
			requiredComponent.SetNameFieldVisible(value: true);
			requiredComponent.SetNameField(TextTranslation.Translate(_characterName));
		}
		return requiredComponent;
	}

	private void ContinueToNextNode()
	{
		_currentNode.SetNodeCompleted();
		if (_currentNode.ListTargetCondition.Count > 0 && !_currentNode.TargetNodeConditionsSatisfied())
		{
			_currentNode = null;
		}
		else
		{
			_currentNode = _currentNode.Target;
		}
	}

	private void ContinueToNextNode(DialogueOption selectedOption)
	{
		_currentNode.SetNodeCompleted();
		if (selectedOption.ConditionToSet != string.Empty)
		{
			DialogueConditionManager.SharedInstance.SetConditionState(selectedOption.ConditionToSet, conditionState: true);
		}
		if (selectedOption.ConditionToCancel != string.Empty)
		{
			DialogueConditionManager.SharedInstance.SetConditionState(selectedOption.ConditionToCancel);
		}
		if (selectedOption.TargetName == string.Empty)
		{
			_currentNode = null;
			return;
		}
		if (!_mapDialogueNodes.ContainsKey(selectedOption.TargetName))
		{
			Debug.LogError("Cannot find target node " + selectedOption.TargetName);
			Debug.Break();
		}
		_currentNode = _mapDialogueNodes[selectedOption.TargetName];
	}

	private void OnDialogueConditionsReset()
	{
		if (_initialized)
		{
			LoadXml();
		}
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		if (base.enabled)
		{
			EndConversation();
		}
	}
}
