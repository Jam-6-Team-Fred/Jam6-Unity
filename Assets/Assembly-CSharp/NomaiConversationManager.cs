using System;
using UnityEngine;

public class NomaiConversationManager : MonoBehaviour
{
	[Serializable]
	private struct StonePair
	{
		public NomaiWord wordA;

		public NomaiWord wordB;

		public NomaiWallText response;

		public bool CheckForMatch(NomaiWord otherWordA, NomaiWord otherWordB)
		{
			if (wordA != otherWordA || wordB != otherWordB)
			{
				if (wordA == otherWordB)
				{
					return wordB == otherWordA;
				}
				return false;
			}
			return true;
		}
	}

	private enum State
	{
		WatchingSky = 0,
		WatchingPlayer = 1,
		CreatingStones = 2,
		RaisingCairns = 3,
		ErasingResponse = 4,
		WritingResponse = 5
	}

	[SerializeField]
	private SolanumAnimController _solanumAnimController;

	[SerializeField]
	private NomaiConversationStone[] _conversationStones;

	[SerializeField]
	private Animator _cairnAnimator;

	[SerializeField]
	private OWCollider _cairnCollision;

	[SerializeField]
	private NomaiConversationStoneSocket _stoneSocketA;

	[SerializeField]
	private NomaiConversationStoneSocket _stoneSocketB;

	[SerializeField]
	private OWTriggerVolume _watchPlayerVolume;

	[SerializeField]
	private CharacterDialogueTree _characterDialogueTree;

	[Space]
	[SerializeField]
	private StonePair[] _questions;

	private OWCollider _stoneSocketATrigger;

	private OWCollider _stoneSocketBTrigger;

	private State _state;

	private bool _playerInWatchVolume;

	private bool _playerWasHoldingStone;

	private bool _dialogueComplete;

	private bool _conversationStonesCreated;

	private bool _cairnRaised;

	private float _stoneCreationTimer;

	private float _stoneGestureTimer;

	private float _cairnGestureTimer;

	private bool _hasValidSocketedStonePair;

	private NomaiWallText _activeResponseText;

	private NomaiWallText _pendingResponseText;

	private bool _initialized;

	private void Awake()
	{
		_stoneSocketATrigger = _stoneSocketA.gameObject.GetAddComponent<OWCollider>();
		_stoneSocketBTrigger = _stoneSocketB.gameObject.GetAddComponent<OWCollider>();
		_cairnCollision.SetActivation(active: false);
		_stoneSocketATrigger.SetActivation(active: false);
		_stoneSocketBTrigger.SetActivation(active: false);
		_state = State.WatchingSky;
		_playerInWatchVolume = false;
		_playerWasHoldingStone = false;
		_dialogueComplete = false;
		_conversationStonesCreated = false;
		_cairnRaised = false;
		_stoneCreationTimer = 1f;
		_stoneGestureTimer = 3f;
		_cairnGestureTimer = 2f;
		_hasValidSocketedStonePair = false;
		_activeResponseText = null;
		_pendingResponseText = null;
		NomaiConversationStoneSocket stoneSocketA = _stoneSocketA;
		stoneSocketA.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Combine(stoneSocketA.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnStonePlacedInSocket));
		NomaiConversationStoneSocket stoneSocketB = _stoneSocketB;
		stoneSocketB.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Combine(stoneSocketB.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnStonePlacedInSocket));
		NomaiConversationStoneSocket stoneSocketA2 = _stoneSocketA;
		stoneSocketA2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(stoneSocketA2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnStoneRemovedFromSocket));
		NomaiConversationStoneSocket stoneSocketB2 = _stoneSocketB;
		stoneSocketB2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(stoneSocketB2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnStoneRemovedFromSocket));
		_watchPlayerVolume.OnEntry += OnEnterWatchVolume;
		_watchPlayerVolume.OnExit += OnExitWatchVolume;
		_characterDialogueTree.OnStartConversation += OnStartDialogue;
		_characterDialogueTree.OnEndConversation += OnFinishDialogue;
		_solanumAnimController.OnTouchRock += OnTouchRock;
		_solanumAnimController.OnWriteResponse += OnWriteResponse;
	}

	private void OnDestroy()
	{
		NomaiConversationStoneSocket stoneSocketA = _stoneSocketA;
		stoneSocketA.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Remove(stoneSocketA.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnStonePlacedInSocket));
		NomaiConversationStoneSocket stoneSocketB = _stoneSocketB;
		stoneSocketB.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Remove(stoneSocketB.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnStonePlacedInSocket));
		NomaiConversationStoneSocket stoneSocketA2 = _stoneSocketA;
		stoneSocketA2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(stoneSocketA2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnStoneRemovedFromSocket));
		NomaiConversationStoneSocket stoneSocketB2 = _stoneSocketB;
		stoneSocketB2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(stoneSocketB2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnStoneRemovedFromSocket));
		_watchPlayerVolume.OnEntry -= OnEnterWatchVolume;
		_watchPlayerVolume.OnExit -= OnExitWatchVolume;
		_characterDialogueTree.OnStartConversation -= OnStartDialogue;
		_characterDialogueTree.OnEndConversation -= OnFinishDialogue;
		_solanumAnimController.OnTouchRock -= OnTouchRock;
		_solanumAnimController.OnWriteResponse -= OnWriteResponse;
	}

	private void OnEnable()
	{
		if (_cairnRaised)
		{
			_cairnAnimator.Play("Cairns_Raise", 0, 1f);
		}
	}

	private void InitializeNomaiText()
	{
		for (int i = 0; i < _questions.Length; i++)
		{
			_questions[i].response.HideImmediate();
		}
		_initialized = true;
	}

	private void ResetStoneGestureTimer()
	{
		_stoneGestureTimer = UnityEngine.Random.Range(10f, 30f);
	}

	private void ResetCairnGestureTimer()
	{
		_cairnGestureTimer = UnityEngine.Random.Range(8f, 20f);
	}

	private void Update()
	{
		if (!_initialized)
		{
			InitializeNomaiText();
		}
		OWItem heldItem = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
		bool flag = heldItem != null && heldItem is NomaiConversationStone;
		switch (_state)
		{
		case State.WatchingSky:
			if (_playerInWatchVolume)
			{
				_state = State.WatchingPlayer;
				_solanumAnimController.StartWatchingPlayer();
			}
			break;
		case State.WatchingPlayer:
			if (_solanumAnimController.isPerformingAction)
			{
				break;
			}
			if (!_playerInWatchVolume)
			{
				_state = State.WatchingSky;
				_solanumAnimController.StopWatchingPlayer();
			}
			else
			{
				if (!_dialogueComplete)
				{
					break;
				}
				if (_dialogueComplete && !_conversationStonesCreated)
				{
					_stoneCreationTimer -= Time.deltaTime;
					if (_stoneCreationTimer <= 0f)
					{
						_state = State.CreatingStones;
						_solanumAnimController.PlayCreateWordStones();
					}
				}
				else if (_conversationStonesCreated && !_cairnRaised)
				{
					if (!flag)
					{
						_stoneGestureTimer -= Time.deltaTime;
						if (_stoneGestureTimer <= 0f)
						{
							_solanumAnimController.PlayGestureToWordStones();
							_stoneGestureTimer = UnityEngine.Random.Range(8f, 16f);
						}
					}
					else if (_solanumAnimController.IsPlayerLooking())
					{
						_state = State.RaisingCairns;
						_solanumAnimController.PlayRaiseCairns();
						_cairnAnimator.SetTrigger("Raise");
						_cairnCollision.SetActivation(active: true);
					}
				}
				else if (_activeResponseText == null && _hasValidSocketedStonePair)
				{
					_activeResponseText = _pendingResponseText;
					_pendingResponseText = null;
					_state = State.WritingResponse;
					_solanumAnimController.StartWritingMessage();
				}
				else if (_activeResponseText != null && (!_hasValidSocketedStonePair || _pendingResponseText != null))
				{
					_state = State.ErasingResponse;
					_solanumAnimController.StartWritingMessage();
				}
				else if (!flag)
				{
					if (_playerWasHoldingStone)
					{
						ResetStoneGestureTimer();
					}
					_stoneGestureTimer -= Time.deltaTime;
					if (_stoneGestureTimer < 0f)
					{
						_solanumAnimController.PlayGestureToWordStones();
						ResetStoneGestureTimer();
					}
				}
				else
				{
					if (!_playerWasHoldingStone)
					{
						ResetCairnGestureTimer();
					}
					_cairnGestureTimer -= Time.deltaTime;
					if (_cairnGestureTimer < 0f)
					{
						_solanumAnimController.PlayGestureToCairns();
						ResetCairnGestureTimer();
					}
				}
			}
			break;
		case State.CreatingStones:
			if (!_solanumAnimController.isPerformingAction)
			{
				_state = State.WatchingPlayer;
				_conversationStonesCreated = true;
			}
			break;
		case State.RaisingCairns:
			if (!_solanumAnimController.isPerformingAction)
			{
				_state = State.WatchingPlayer;
				_cairnRaised = true;
				_stoneSocketATrigger.SetActivation(active: true);
				_stoneSocketBTrigger.SetActivation(active: true);
			}
			break;
		case State.ErasingResponse:
			if (!_solanumAnimController.isStartingWrite && !_activeResponseText.IsAnimationPlaying())
			{
				_activeResponseText = null;
				if (_pendingResponseText == null)
				{
					_state = State.WatchingPlayer;
					_solanumAnimController.StopWritingMessage(gestureToText: false);
					break;
				}
				_activeResponseText = _pendingResponseText;
				_pendingResponseText = null;
				_state = State.WritingResponse;
				_activeResponseText.Show();
			}
			break;
		case State.WritingResponse:
			if (!_solanumAnimController.isStartingWrite && !_activeResponseText.IsAnimationPlaying())
			{
				_state = State.WatchingPlayer;
				_solanumAnimController.StopWritingMessage(gestureToText: true);
			}
			break;
		}
		_playerWasHoldingStone = flag;
	}

	private void OnStonePlacedInSocket(OWItem owItem)
	{
		if (!_stoneSocketA.IsSocketOccupied() || !_stoneSocketB.IsSocketOccupied())
		{
			return;
		}
		NomaiConversationStone nomaiConversationStone = _stoneSocketA.GetSocketedItem() as NomaiConversationStone;
		NomaiConversationStone nomaiConversationStone2 = _stoneSocketB.GetSocketedItem() as NomaiConversationStone;
		for (int i = 0; i < _questions.Length; i++)
		{
			if (_questions[i].CheckForMatch(nomaiConversationStone.GetWord(), nomaiConversationStone2.GetWord()))
			{
				_hasValidSocketedStonePair = true;
				_pendingResponseText = _questions[i].response;
			}
		}
	}

	private void OnStoneRemovedFromSocket(OWItem owItem)
	{
		_hasValidSocketedStonePair = false;
		_pendingResponseText = null;
	}

	private void OnEnterWatchVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInWatchVolume = true;
		}
	}

	private void OnExitWatchVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInWatchVolume = false;
		}
	}

	private void OnStartDialogue()
	{
		_solanumAnimController.StartConversation();
	}

	private void OnFinishDialogue()
	{
		Locator.GetShipLogManager().RevealFact("QM_SIXTH_LOCATION_X1", saveGame: true, showNotification: false);
		_solanumAnimController.EndConversation();
		_characterDialogueTree.GetInteractVolume().DisableInteraction();
		_dialogueComplete = true;
	}

	private void OnTouchRock(int rockIndex)
	{
		_conversationStones[rockIndex].Reveal();
	}

	private void OnWriteResponse(int unused)
	{
		if (_state == State.ErasingResponse)
		{
			_activeResponseText.Hide();
		}
		else if (_state == State.WritingResponse)
		{
			_activeResponseText.Show();
		}
	}
}
