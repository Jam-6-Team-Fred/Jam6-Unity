using System.Collections.Generic;
using UnityEngine;

public class TranslatedSign : MonoBehaviour
{
	public delegate void StartConversationEvent();

	public delegate void EndConversationEvent();

	[SerializeField]
	private UITextType _text;

	[SerializeField]
	private Transform _attentionPoint;

	[SerializeField]
	private Vector3 _attentionPointOffset = Vector3.zero;

	private SingleInteractionVolume _interactVolume;

	private DialogueBoxVer2 _currentDialogueBox;

	private bool _timeFrozen;

	private const float MINIMUM_TIME_TEXT_VISIBLE = 0.1f;

	public event StartConversationEvent OnStartConversation;

	public event EndConversationEvent OnEndConversation;

	private void Awake()
	{
		_attentionPoint = ((_attentionPoint == null) ? base.transform : _attentionPoint);
		_interactVolume = this.GetRequiredComponent<SingleInteractionVolume>();
		_interactVolume.OnPressInteract += OnPressInteract;
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
	}

	private void Start()
	{
		_interactVolume.SetPromptText(UITextType.ReadPrompt);
		if (PlayerData.GetSavedLanguage() == TextTranslation.Language.ENGLISH)
		{
			_interactVolume.gameObject.SetActive(value: false);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
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
		if (!(_currentDialogueBox != null))
		{
			return;
		}
		if (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.jump))
		{
			if (!_currentDialogueBox.AreTextEffectsComplete())
			{
				_currentDialogueBox.FinishAllTextEffects();
			}
			else if (!(_currentDialogueBox.TimeCompletelyRevealed() < 0.1f))
			{
				EndConversation();
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

	private void OnPressInteract()
	{
		StartConversation();
	}

	public void StartConversation()
	{
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
		Locator.GetPlayerAudioController().PlayDialogueEnter();
		DialogueConditionManager.SharedInstance.ReadPlayerData();
		_currentDialogueBox = DisplayDialogueBox2();
		if (_attentionPoint != null)
		{
			Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(_attentionPoint, _attentionPointOffset, 2f);
		}
	}

	private DialogueBoxVer2 DisplayDialogueBox2()
	{
		string @string = UITextLibrary.GetString(_text);
		List<DialogueOption> listOptions = new List<DialogueOption>();
		DialogueBoxVer2 requiredComponent = GameObject.FindWithTag("DialogueGui").GetRequiredComponent<DialogueBoxVer2>();
		requiredComponent.SetVisible(value: true);
		requiredComponent.SetDialogueText(@string, listOptions);
		requiredComponent.SetNameFieldVisible(value: false);
		return requiredComponent;
	}

	public void EndConversation()
	{
		if (base.enabled)
		{
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
			Locator.GetPlayerAudioController().PlayDialogueExit();
			GameObject.FindWithTag("DialogueGui").GetRequiredComponent<DialogueBoxVer2>().OnEndDialogue();
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
