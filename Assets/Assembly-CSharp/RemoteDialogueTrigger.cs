using System;
using UnityEngine;

public class RemoteDialogueTrigger : MonoBehaviour
{
	public enum MultiConditionType
	{
		OR = 0,
		AND = 1
	}

	[Serializable]
	public struct RemoteDialogueCondition
	{
		public int priority;

		public CharacterDialogueTree dialogue;

		public MultiConditionType prereqConditionType;

		public string[] prereqConditions;

		public string[] onTriggerEnterConditions;
	}

	[SerializeField]
	private RemoteDialogueCondition[] _listDialogues;

	private bool[] _activatedDialogues;

	private CharacterDialogueTree _activeRemoteDialogue;

	[SerializeField]
	private bool _deactivateTriggerPostConversation;

	private Collider _collider;

	private bool _inRemoteDialogue;

	private void Awake()
	{
		_activatedDialogues = new bool[_listDialogues.Length];
		for (int i = 0; i < _listDialogues.Length; i++)
		{
			_activatedDialogues[i] = false;
			_listDialogues[i].dialogue.OnEndConversation += OnEndConversation;
		}
		_collider = GetComponent<Collider>();
		_collider.isTrigger = true;
		_inRemoteDialogue = false;
		base.gameObject.layer = LayerMask.NameToLayer("AdvancedEffectVolume");
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _listDialogues.Length; i++)
		{
			_listDialogues[i].dialogue.OnEndConversation -= OnEndConversation;
		}
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		if (hitCollider.CompareTag("PlayerDetector") && !PlayerState.InConversation() && ConversationTriggered(out var dialogue))
		{
			_activeRemoteDialogue = dialogue.dialogue;
			for (int i = 0; i < dialogue.onTriggerEnterConditions.Length; i++)
			{
				DialogueConditionManager.SharedInstance.SetConditionState(dialogue.onTriggerEnterConditions[i], conditionState: true);
			}
			_activeRemoteDialogue.StartConversation();
			_activeRemoteDialogue.GetInteractVolume().DisableInteraction();
			_inRemoteDialogue = true;
		}
	}

	private bool ConversationTriggered(out RemoteDialogueCondition dialogue)
	{
		dialogue = default(RemoteDialogueCondition);
		int num = int.MaxValue;
		int num2 = -1;
		DialogueConditionManager sharedInstance = DialogueConditionManager.SharedInstance;
		for (int i = 0; i < _listDialogues.Length; i++)
		{
			if (_activatedDialogues[i])
			{
				continue;
			}
			bool flag = true;
			bool flag2 = false;
			if (_listDialogues[i].prereqConditions.Length == 0)
			{
				flag2 = true;
			}
			for (int j = 0; j < _listDialogues[i].prereqConditions.Length; j++)
			{
				if (sharedInstance.GetConditionState(_listDialogues[i].prereqConditions[j]))
				{
					flag2 = true;
				}
				else
				{
					flag = false;
				}
			}
			bool flag3 = false;
			switch (_listDialogues[i].prereqConditionType)
			{
			case MultiConditionType.AND:
				if (flag)
				{
					flag3 = true;
				}
				break;
			case MultiConditionType.OR:
				if (flag2)
				{
					flag3 = true;
				}
				break;
			}
			if (flag3 && _listDialogues[i].priority < num)
			{
				dialogue = _listDialogues[i];
				num2 = i;
			}
		}
		if (num2 == -1)
		{
			return false;
		}
		_activatedDialogues[num2] = true;
		return true;
	}

	private void OnEndConversation()
	{
		if (_inRemoteDialogue)
		{
			_activeRemoteDialogue.GetInteractVolume().EnableInteraction();
			_activeRemoteDialogue = null;
			_inRemoteDialogue = false;
		}
		if (_deactivateTriggerPostConversation && ConversationTriggered(out var _))
		{
			_collider.enabled = false;
		}
	}
}
