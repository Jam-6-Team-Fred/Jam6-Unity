using UnityEngine;

public class PrisonerChoiceDialogue : MonoBehaviour
{
	[SerializeField]
	private QuantumCampsiteController _campsiteController;

	[Space]
	[SerializeField]
	private QuantumInstrument _instrument;

	[SerializeField]
	private CharacterDialogueTree _dialogueTree;

	[SerializeField]
	private InteractReceiver _interactReceiver;

	[Space]
	[SerializeField]
	private VisibilityObject _choiceVisibilityObject;

	[SerializeField]
	private VisibilityObject _campfireVisibilityObject;

	[Space]
	[SerializeField]
	private GameObject _choicePrisonerRoot;

	[SerializeField]
	private GameObject _campfirePrisonerRoot;

	private bool _choiceMade;

	private bool _joining;

	private bool _hasVanished;

	private void Awake()
	{
		_dialogueTree.OnEndConversation += OnEndConversation;
		_instrument.OnFinishGather += OnFinishGather;
	}

	private void Start()
	{
		base.enabled = false;
		_choicePrisonerRoot.SetActive(value: false);
		_choiceVisibilityObject.SetActivation(active: false);
		_campfirePrisonerRoot.SetActive(value: false);
		_campfireVisibilityObject.SetActivation(active: false);
	}

	private void OnDestroy()
	{
		_dialogueTree.OnEndConversation -= OnEndConversation;
		_instrument.OnFinishGather -= OnFinishGather;
	}

	private void OnFinishGather()
	{
		_choicePrisonerRoot.SetActive(value: true);
	}

	private void OnEndConversation()
	{
		if (DialogueConditionManager.SharedInstance.GetConditionState("PRISONER_JOIN"))
		{
			_choiceMade = true;
			_joining = true;
		}
		else if (DialogueConditionManager.SharedInstance.GetConditionState("PRISONER_LEAVE"))
		{
			_choiceMade = true;
			_joining = false;
		}
		if (_choiceMade)
		{
			_interactReceiver.DisableInteraction();
			_choiceVisibilityObject.SetActivation(active: true);
			_campfireVisibilityObject.SetActivation(_joining);
			base.enabled = true;
		}
	}

	private void Update()
	{
		if (!_choiceVisibilityObject.IsVisible())
		{
			_choicePrisonerRoot.SetActive(value: false);
			_choiceVisibilityObject.SetActivation(active: false);
			_hasVanished = true;
			if (!_joining)
			{
				_campsiteController.OnPrisonerErased();
				base.enabled = false;
			}
		}
		if (_hasVanished && _joining && !_campfireVisibilityObject.IsVisible())
		{
			_campfirePrisonerRoot.SetActive(value: true);
			_campfireVisibilityObject.SetActivation(active: false);
			_campsiteController.OnPrisonerJoined();
			base.enabled = false;
		}
	}
}
