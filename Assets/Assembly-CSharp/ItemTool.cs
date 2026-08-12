using UnityEngine;

public class ItemTool : PlayerTool
{
	private enum PromptState
	{
		NONE = 0,
		PICK_UP = 1,
		DROP = 2,
		UNSOCKET = 3,
		SOCKET = 4,
		WRONG_SOCKET_TYPE = 5,
		CANNOT_HOLD_MORE = 6,
		CANNOT_DROP = 7,
		GIVE = 8,
		TAKE = 9
	}

	[SerializeField]
	private GameObject _defaultItemSocket;

	[SerializeField]
	private GameObject _scrollSocket;

	[SerializeField]
	private GameObject _sharedStoneSocket;

	[SerializeField]
	private GameObject _warpCoreSocket;

	[SerializeField]
	private GameObject _vesselCoreSocket;

	[SerializeField]
	private GameObject _simpleLanternSocket;

	[SerializeField]
	private GameObject _dreamLanternSocket;

	[SerializeField]
	private GameObject _slideReelSocket;

	[SerializeField]
	private GameObject _visionTorchSocket;

	[SerializeField]
	private Transform _vesselCoreStowTransform;

	[SerializeField]
	private float _dropDistanceInFront;

	[SerializeField]
	private float _maxDroppableSlopeAngle = 30f;

	private Transform _origStowTransform;

	private GameObject _itemSocket;

	private OWItem _heldItem;

	private PromptState _promptState;

	private ScreenPrompt _messageOnlyPrompt;

	private ScreenPrompt _interactButtonPrompt;

	private ScreenPrompt _cancelButtonPrompt;

	private const float _maxDroppableLookAngle = 70f;

	private const float _minDroppableLookAngle = 0f;

	private const float _maxDropDistance = 4f;

	private const float _minDropDistance = 2.5f;

	private bool _waitForUnsocketAnimation;

	private void Awake()
	{
		_origStowTransform = _stowTransform;
		_itemSocket = _defaultItemSocket;
		_heldItem = null;
	}

	protected override void Start()
	{
		base.Start();
		_messageOnlyPrompt = new ScreenPrompt(string.Empty);
		_interactButtonPrompt = new ScreenPrompt(InputLibrary.interact, string.Empty);
		_cancelButtonPrompt = new ScreenPrompt(InputLibrary.cancel, string.Empty);
		Locator.GetPromptManager().AddScreenPrompt(_messageOnlyPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_interactButtonPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_cancelButtonPrompt, PromptPosition.UpperRight);
	}

	public OWItem GetHeldItem()
	{
		return _heldItem;
	}

	public ItemType GetHeldItemType()
	{
		if (!(_heldItem == null))
		{
			return _heldItem.GetItemType();
		}
		return ItemType.Invalid;
	}

	public bool IsAnyPromptVisible()
	{
		if (_promptState != 0)
		{
			if (!_interactButtonPrompt.IsVisible())
			{
				return _messageOnlyPrompt.IsVisible();
			}
			return true;
		}
		return false;
	}

	public override void UnequipTool()
	{
		base.UnequipTool();
		_messageOnlyPrompt.SetVisibility(isVisible: false);
		_interactButtonPrompt.SetVisibility(isVisible: false);
		_cancelButtonPrompt.SetVisibility(isVisible: false);
	}

	protected override bool AllowEquipAnimation()
	{
		if (!(_heldItem == null))
		{
			return !_heldItem.IsAnimationPlaying();
		}
		return true;
	}

	public void PickUpItemInstantly(OWItem item)
	{
		if (_heldItem == null)
		{
			MoveItemToCarrySocket(item);
			_heldItem = item;
			Locator.GetToolModeSwapper().EquipToolMode(ToolMode.Item);
			Locator.GetToolModeSwapper().GetItemCarryTool().InstantlyCenterTool();
		}
		else
		{
			Debug.LogError("Already holding item!!!");
			Debug.Break();
		}
	}

	public void UnsocketItemInstantly(OWItemSocket socket)
	{
		OWItem oWItem = socket.RemoveFromSocket();
		PickUpItemInstantly(oWItem);
		oWItem.OnCompleteUnsocket();
	}

	public void DropItemInstantly(Sector sector, Transform socket)
	{
		if (_heldItem != null)
		{
			_heldItem.DropItem(socket.position, socket.up, socket, sector, null);
			_heldItem = null;
		}
	}

	public bool UpdateInteract(FirstPersonManipulator firstPersonManipulator, bool inputBlocked)
	{
		OWItemSocket focusedItemSocket = firstPersonManipulator.GetFocusedItemSocket();
		OWItem focusedOWItem = firstPersonManipulator.GetFocusedOWItem();
		string itemName = "";
		PromptState newState = PromptState.NONE;
		bool result = false;
		bool flag = OWInput.IsNewlyPressed(InputLibrary.interact);
		if (!OWInput.IsInputMode(InputMode.Character | InputMode.NomaiRemoteCam) || PlayerState.IsViewingProjector())
		{
			inputBlocked = true;
		}
		if (_waitForUnsocketAnimation)
		{
			if (!_heldItem.IsAnimationPlaying())
			{
				CompleteUnsocketItem();
				_waitForUnsocketAnimation = false;
			}
		}
		else if (!inputBlocked)
		{
			if (focusedItemSocket != null)
			{
				if (focusedItemSocket.IsSocketOccupied())
				{
					if (_heldItem == null)
					{
						newState = (focusedItemSocket.UsesGiveTakePrompts() ? PromptState.TAKE : PromptState.UNSOCKET);
						itemName = focusedItemSocket.GetSocketedItem().GetDisplayName();
						if (flag)
						{
							StartUnsocketItem(focusedItemSocket);
							result = true;
						}
					}
					else if (focusedItemSocket is OWItemDoubleSocket)
					{
						itemName = _heldItem.GetDisplayName();
						OWItemDoubleSocket oWItemDoubleSocket = focusedItemSocket as OWItemDoubleSocket;
						if ((oWItemDoubleSocket.AcceptsItemInFirstSocket(_heldItem) && !oWItemDoubleSocket.IsFirstSocketOccupied()) || (oWItemDoubleSocket.AcceptsItemInSecondSocket(_heldItem) && !oWItemDoubleSocket.IsSecondSocketOccupied()))
						{
							newState = PromptState.SOCKET;
							if (flag)
							{
								SocketItem(focusedItemSocket);
							}
						}
						else
						{
							newState = ((!oWItemDoubleSocket.AcceptsItemInFirstSocket(_heldItem) && !oWItemDoubleSocket.AcceptsItemInSecondSocket(_heldItem)) ? PromptState.WRONG_SOCKET_TYPE : PromptState.CANNOT_HOLD_MORE);
						}
					}
					else
					{
						newState = PromptState.CANNOT_HOLD_MORE;
						itemName = _heldItem.GetDisplayName();
					}
				}
				else if (_heldItem != null)
				{
					itemName = _heldItem.GetDisplayName();
					if (focusedItemSocket.AcceptsItem(_heldItem))
					{
						newState = (focusedItemSocket.UsesGiveTakePrompts() ? PromptState.GIVE : PromptState.SOCKET);
						if (flag)
						{
							SocketItem(focusedItemSocket);
						}
					}
					else
					{
						newState = PromptState.WRONG_SOCKET_TYPE;
					}
				}
			}
			else if (focusedOWItem != null)
			{
				if (_heldItem == null)
				{
					newState = PromptState.PICK_UP;
					itemName = focusedOWItem.GetDisplayName();
					if (flag)
					{
						MoveItemToCarrySocket(focusedOWItem);
						_heldItem = focusedOWItem;
						Locator.GetPlayerAudioController().PlayPickUpItem(_heldItem.GetItemType());
						result = true;
					}
				}
				else
				{
					newState = PromptState.CANNOT_HOLD_MORE;
					itemName = _heldItem.GetDisplayName();
				}
			}
			else if ((bool)_heldItem && IsEquipped())
			{
				if (UpdateIsDroppable(out var hit, out var targetRigidbody, out var dropTarget))
				{
					newState = PromptState.DROP;
					itemName = _heldItem.GetDisplayName();
					if (flag)
					{
						DropItem(hit, targetRigidbody, dropTarget);
					}
				}
				else
				{
					newState = PromptState.CANNOT_DROP;
				}
			}
		}
		UpdateState(newState, itemName);
		return result;
	}

	private void UpdateState(PromptState newState, string itemName)
	{
		if (_promptState != newState)
		{
			_promptState = newState;
			string text = string.Empty;
			string text2 = string.Empty;
			string empty = string.Empty;
			switch (_promptState)
			{
			case PromptState.PICK_UP:
				text2 = UITextLibrary.GetString(UITextType.ItemPickUpPrompt) + itemName;
				break;
			case PromptState.DROP:
				text2 = UITextLibrary.GetString(UITextType.ItemDropPrompt) + itemName;
				break;
			case PromptState.UNSOCKET:
				text2 = UITextLibrary.GetString(UITextType.ItemRemovePrompt) + itemName;
				break;
			case PromptState.SOCKET:
				text2 = UITextLibrary.GetString(UITextType.ItemInsertPrompt) + itemName;
				break;
			case PromptState.WRONG_SOCKET_TYPE:
				text = itemName + UITextLibrary.GetString(UITextType.ItemNotFitPrompt);
				break;
			case PromptState.CANNOT_HOLD_MORE:
				text = UITextLibrary.GetString(UITextType.ItemAlreadyHoldingPrompt) + itemName;
				break;
			case PromptState.TAKE:
				text2 = UITextLibrary.GetString(UITextType.TakePrompt) + " " + itemName;
				break;
			case PromptState.GIVE:
				text2 = UITextLibrary.GetString(UITextType.GivePrompt) + " " + itemName;
				break;
			}
			if (text == string.Empty)
			{
				_messageOnlyPrompt.SetVisibility(isVisible: false);
			}
			else
			{
				_messageOnlyPrompt.SetVisibility(isVisible: true);
				_messageOnlyPrompt.SetText(text);
			}
			if (empty == string.Empty)
			{
				_cancelButtonPrompt.SetVisibility(isVisible: false);
			}
			else
			{
				_cancelButtonPrompt.SetVisibility(isVisible: true);
				_cancelButtonPrompt.SetText(empty);
			}
			if (text2 == string.Empty)
			{
				_interactButtonPrompt.SetVisibility(isVisible: false);
				return;
			}
			_interactButtonPrompt.SetVisibility(isVisible: true);
			_interactButtonPrompt.SetText(text2);
		}
	}

	private void MoveItemToCarrySocket(OWItem item)
	{
		_stowTransform = _origStowTransform;
		switch (item.GetItemType())
		{
		case ItemType.Scroll:
			_itemSocket = _scrollSocket;
			break;
		case ItemType.SharedStone:
			_itemSocket = _sharedStoneSocket;
			break;
		case ItemType.WarpCore:
			if (((WarpCoreItem)item).IsVesselCoreType())
			{
				_stowTransform = _vesselCoreStowTransform;
				_itemSocket = _vesselCoreSocket;
			}
			else
			{
				_itemSocket = _warpCoreSocket;
			}
			break;
		case ItemType.Lantern:
			_itemSocket = _simpleLanternSocket;
			break;
		case ItemType.DreamLantern:
			_itemSocket = _dreamLanternSocket;
			break;
		case ItemType.SlideReel:
			_itemSocket = _slideReelSocket;
			break;
		case ItemType.VisionTorch:
			_itemSocket = _visionTorchSocket;
			break;
		default:
			_itemSocket = _defaultItemSocket;
			break;
		}
		item.PickUpItem(_itemSocket.transform);
	}

	private bool UpdateIsDroppable(out RaycastHit hit, out OWRigidbody targetRigidbody, out IItemDropTarget dropTarget)
	{
		hit = default(RaycastHit);
		targetRigidbody = null;
		dropTarget = null;
		PlayerCharacterController playerController = Locator.GetPlayerController();
		if (!playerController.IsGrounded() || PlayerState.IsAttached() || PlayerState.IsInsideShip())
		{
			return false;
		}
		if (_heldItem != null && !_heldItem.CheckIsDroppable())
		{
			return false;
		}
		if (playerController.GetRelativeGroundVelocity().sqrMagnitude >= playerController.GetRunSpeedMagnitude() * playerController.GetRunSpeedMagnitude())
		{
			return false;
		}
		Vector3 forward = Locator.GetPlayerTransform().forward;
		Vector3 forward2 = Locator.GetPlayerCamera().transform.forward;
		float value = Vector3.Angle(forward, forward2);
		float num = Mathf.InverseLerp(0f, 70f, value);
		float maxDistance = 2.5f;
		if (num <= 1f)
		{
			maxDistance = Mathf.Lerp(2.5f, 4f, num);
		}
		if (Physics.Raycast(Locator.GetPlayerCamera().transform.position, forward2, out hit, maxDistance, (int)OWLayerMask.physicalMask | (int)OWLayerMask.interactMask))
		{
			if (OWLayerMask.IsLayerInMask(hit.collider.gameObject.layer, OWLayerMask.interactMask))
			{
				return false;
			}
			if (Vector3.Angle(Locator.GetPlayerTransform().up, hit.normal) <= _maxDroppableSlopeAngle)
			{
				IgnoreCollision component = hit.collider.GetComponent<IgnoreCollision>();
				if (component == null || !component.PreventsItemDrop())
				{
					targetRigidbody = hit.collider.GetAttachedOWRigidbody();
					if (targetRigidbody.gameObject.CompareTag("Ship"))
					{
						return false;
					}
					dropTarget = hit.collider.GetComponentInParent<IItemDropTarget>();
					return true;
				}
			}
		}
		return false;
	}

	private void SocketItem(OWItemSocket socket)
	{
		Locator.GetPlayerAudioController().PlayInsertItem(_heldItem.GetItemType());
		socket.PlaceIntoSocket(_heldItem);
		_heldItem = null;
		Locator.GetToolModeSwapper().UnequipTool();
	}

	private void StartUnsocketItem(OWItemSocket socket)
	{
		OWItem oWItem = (_heldItem = socket.RemoveFromSocket());
		Locator.GetPlayerAudioController().PlayRemoveItem(oWItem.GetItemType());
		if (_heldItem.IsAnimationPlaying())
		{
			_waitForUnsocketAnimation = true;
		}
		else
		{
			CompleteUnsocketItem();
		}
	}

	private void CompleteUnsocketItem()
	{
		MoveItemToCarrySocket(_heldItem);
		_heldItem.OnCompleteUnsocket();
	}

	private void DropItem(RaycastHit hit, OWRigidbody targetRigidbody, IItemDropTarget customDropTarget)
	{
		Locator.GetPlayerAudioController().PlayDropItem(_heldItem.GetItemType());
		GameObject gameObject = hit.collider.gameObject;
		ISectorGroup component = gameObject.GetComponent<ISectorGroup>();
		Sector sector = null;
		while (component == null && gameObject.transform.parent != null)
		{
			gameObject = gameObject.transform.parent.gameObject;
			component = gameObject.GetComponent<ISectorGroup>();
		}
		if (component != null)
		{
			sector = component.GetSector();
			if (sector == null && component is SectorCullGroup)
			{
				SectorProxy controllingProxy = (component as SectorCullGroup).GetControllingProxy();
				if (controllingProxy != null)
				{
					sector = controllingProxy.GetSector();
				}
			}
		}
		Transform parent = ((customDropTarget == null) ? targetRigidbody.transform : customDropTarget.GetItemDropTargetTransform(hit.collider.gameObject));
		_heldItem.DropItem(hit.point, hit.normal, parent, sector, customDropTarget);
		customDropTarget?.AddDroppedItem(hit.collider.gameObject, _heldItem);
		_heldItem = null;
		Locator.GetToolModeSwapper().UnequipTool();
	}
}
