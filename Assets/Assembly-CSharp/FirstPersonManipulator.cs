using System.Text;
using UnityEngine;

public class FirstPersonManipulator : MonoBehaviour
{
	private const float MAX_INTERACT_RANGE = 75f;

	private Collider _lastHitCollider;

	private RepairReceiver _focusedRepairReceiver;

	private NomaiText _focusedNomaiText;

	private OWItemSocket _focusedItemSocket;

	private OWItem _focusedItem;

	private IObservable _observable;

	private NomaiInterfaceOrb _pendingOrb;

	private NomaiInterfaceOrb _activeOrb;

	private IRaycastInteractable _interactReceiver;

	private ICameraViewInteractable _interactZone;

	private PlayerAudioController _playerAudioController;

	private ScreenPrompt _repairScreenPrompt;

	private ScreenPrompt _repairDetailsScreenPrompt;

	private bool _isRepairing;

	private StringBuilder _repairDetailsStringBuilder;

	private void Awake()
	{
		_repairScreenPrompt = new ScreenPrompt(InputLibrary.interact, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.ShipRepairPrompt));
		_repairDetailsScreenPrompt = new ScreenPrompt("");
		_isRepairing = false;
		_repairDetailsStringBuilder = new StringBuilder(128);
	}

	private void Start()
	{
		_playerAudioController = Locator.GetPlayerTransform().GetComponentInChildren<PlayerAudioController>();
		Locator.GetPromptManager().AddScreenPrompt(_repairScreenPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_repairDetailsScreenPrompt, PromptPosition.Center);
	}

	public NomaiText GetFocusedNomaiText()
	{
		return _focusedNomaiText;
	}

	public OWItemSocket GetFocusedItemSocket()
	{
		return _focusedItemSocket;
	}

	public OWItem GetFocusedOWItem()
	{
		return _focusedItem;
	}

	public bool HasFocusedInteractible()
	{
		if (_interactReceiver == null || !_interactReceiver.IsFocused())
		{
			if (_interactZone != null)
			{
				return _interactZone.IsFocused();
			}
			return false;
		}
		return true;
	}

	public void OnEnterInteractZone(ICameraViewInteractable zone)
	{
		_interactZone = zone;
	}

	public void OnExitInteractZone(ICameraViewInteractable zone)
	{
		zone.LoseFocus();
		_interactZone = null;
	}

	private void Update()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.UpdateInteractVolume();
			if (_interactZone != null && _interactZone.IsFocused() && _interactReceiver.IsFocused())
			{
				_interactZone.LoseFocus();
			}
		}
		if (_interactZone != null && (_interactReceiver == null || !_interactReceiver.IsFocused()))
		{
			_interactZone.UpdateInteractVolume();
		}
		if (_focusedRepairReceiver != null && !OWTime.IsPaused())
		{
			_repairDetailsStringBuilder.Length = 0;
			_repairDetailsStringBuilder.Append(UITextLibrary.GetString(_focusedRepairReceiver.GetRepairableName()));
			_repairDetailsStringBuilder.Append(": ");
			_repairDetailsStringBuilder.Append(_focusedRepairReceiver.GetRepairFraction().ToString("P0"));
			_repairDetailsScreenPrompt.SetText(_repairDetailsStringBuilder.ToString());
			if (!_repairScreenPrompt.IsVisible())
			{
				_repairScreenPrompt.SetVisibility(isVisible: true);
				_repairDetailsScreenPrompt.SetVisibility(isVisible: true);
			}
			if (!_isRepairing && OWInput.IsNewlyPressed(InputLibrary.interact))
			{
				_isRepairing = true;
				_focusedRepairReceiver.RepairTick();
				_playerAudioController.PlayRepairTool();
			}
			else
			{
				if (!_isRepairing)
				{
					return;
				}
				if (OWInput.IsPressed(InputLibrary.interact))
				{
					_focusedRepairReceiver.RepairTick();
					if (!_focusedRepairReceiver.IsDamaged())
					{
						_isRepairing = false;
						_repairScreenPrompt.SetVisibility(isVisible: false);
						_repairDetailsScreenPrompt.SetVisibility(isVisible: false);
						_playerAudioController.StopRepairTool();
						_playerAudioController.PlayRepairCompleteOneShot();
					}
				}
				else
				{
					_isRepairing = false;
					_playerAudioController.StopRepairTool();
				}
			}
		}
		else
		{
			if (_repairScreenPrompt.IsVisible())
			{
				_repairScreenPrompt.SetVisibility(isVisible: false);
				_repairDetailsScreenPrompt.SetVisibility(isVisible: false);
			}
			if (_isRepairing)
			{
				_isRepairing = false;
				_playerAudioController.StopRepairTool();
			}
		}
	}

	private void LateUpdate()
	{
		if (Physics.Raycast(base.transform.position, base.transform.forward, out var hitInfo, 75f, OWLayerMask.blockableInteractMask))
		{
			if (hitInfo.collider != _lastHitCollider)
			{
				_lastHitCollider = hitInfo.collider;
				if (_observable != null)
				{
					_observable.LoseFocus();
				}
				_observable = _lastHitCollider.GetComponent<IObservable>();
				if (_interactReceiver != null)
				{
					_interactReceiver.LoseFocus();
				}
				_interactReceiver = _lastHitCollider.GetComponent<IRaycastInteractable>();
				NomaiInterfaceOrb component = _lastHitCollider.GetComponent<NomaiInterfaceOrb>();
				if (_pendingOrb != null && component == null)
				{
					_pendingOrb.OnLoseStartDragFocus();
				}
				_pendingOrb = component;
			}
		}
		else
		{
			_lastHitCollider = null;
			if (_interactReceiver != null)
			{
				_interactReceiver.LoseFocus();
				_interactReceiver = null;
			}
			if (_observable != null)
			{
				_observable.LoseFocus();
				_observable = null;
			}
			_focusedRepairReceiver = null;
			_focusedNomaiText = null;
			_focusedItemSocket = null;
			_focusedItem = null;
			if (_pendingOrb != null)
			{
				_pendingOrb.OnLoseStartDragFocus();
			}
			_pendingOrb = null;
		}
		if (_activeOrb != null)
		{
			if (Physics.Raycast(base.transform.position, base.transform.forward, out var hitInfo2, 75f, OWLayerMask.interactMask))
			{
				if (!_activeOrb.UpdateDragFromPosition(base.transform.position, hitInfo2.point))
				{
					_activeOrb.CancelDrag();
					_activeOrb = null;
				}
			}
			else
			{
				_activeOrb.CancelDrag();
				_activeOrb = null;
			}
		}
		else if (_pendingOrb != null && _pendingOrb.StartDragFromPosition(base.transform.position))
		{
			_activeOrb = _pendingOrb;
		}
		if (_lastHitCollider != null)
		{
			_focusedRepairReceiver = _lastHitCollider.GetComponent<RepairReceiver>();
			if (_focusedRepairReceiver != null && (hitInfo.distance > _focusedRepairReceiver.repairDistance || !_focusedRepairReceiver.IsRepairable()))
			{
				_focusedRepairReceiver = null;
			}
			_focusedNomaiText = _lastHitCollider.GetComponent<NomaiText>();
			if (_focusedNomaiText != null && !_focusedNomaiText.CheckAllowFocus(hitInfo.distance, base.transform.forward))
			{
				_focusedNomaiText = null;
			}
			_focusedItemSocket = _lastHitCollider.GetComponent<OWItemSocket>();
			if (_focusedItemSocket != null && (!_focusedItemSocket.IsInteractable() || hitInfo.distance > _focusedItemSocket.GetInteractRange()))
			{
				_focusedItemSocket = null;
			}
			_focusedItem = _lastHitCollider.GetComponent<OWItem>();
			if (_focusedItem != null && (!_focusedItem.IsInteractable() || hitInfo.distance > _focusedItem.GetInteractRange()))
			{
				_focusedItem = null;
			}
		}
		if (_observable != null)
		{
			_observable.Observe(hitInfo, base.transform.position);
		}
		if (_interactReceiver != null)
		{
			_interactReceiver.Observe(hitInfo);
		}
	}
}
