using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OWMenuInputModule : InputSystemUIInputModule
{
	public delegate void InputModuleCancelEvent(GameObject selectedObj, BaseEventData eventData);

	public delegate void InputModuleSubmitEvent(GameObject selectedObj, BaseEventData eventData);

	public delegate void InputModuleTabEvent(GameObject selectedObj, TabEventData eventData);

	private struct InputActionReferenceState
	{
		public int refCount;

		public bool enabledByInputModule;
	}

	private float m_NextAction;

	private float _timeToButtonHold = 0.5f;

	[SerializeField]
	private float _onMoveButtonHoldActionsPerSec = 10f;

	[SerializeField]
	private float _inputActionsPerSecond = 2f;

	private bool _allowInputs = true;

	private bool _allowMouseInputs = true;

	private bool _debugScene;

	private List<Selectable> _nextSelectableQueue;

	private static Dictionary<InputAction, InputActionReferenceState> s_InputActionReferenceCounts = new Dictionary<InputAction, InputActionReferenceState>();

	private bool _needToPurgeStalePointers;

	private int _currentPointerId = -1;

	private int _currentPointerIndex = -1;

	private UIPointerType _currentPointerType;

	private InlinedArray<int> _pointerIds;

	private InlinedArray<InputControl> _pointerTouchControls;

	private InlinedArray<PointerModelExposed> _pointerStates;

	internal const float kPixelPerLine = 20f;

	private const float kClickSpeed = 0.3f;

	public float inputActionsPerSecond
	{
		get
		{
			return _inputActionsPerSecond;
		}
		set
		{
			_inputActionsPerSecond = value;
			_timeToButtonHold = 1f / _inputActionsPerSecond;
		}
	}

	public float onHoldInputActionsPerSecond
	{
		get
		{
			return _onMoveButtonHoldActionsPerSec;
		}
		set
		{
			_onMoveButtonHoldActionsPerSec = value;
		}
	}

	public float buttonHoldTimeThreshold => _timeToButtonHold;

	private bool explictlyIgnoreFocus => InputSystem.settings.backgroundBehavior == InputSettings.BackgroundBehavior.IgnoreFocus;

	private bool shouldIgnoreFocus
	{
		get
		{
			if (!explictlyIgnoreFocus)
			{
				return Application.runInBackground;
			}
			return true;
		}
	}

	public event InputModuleCancelEvent OnInputModuleCancel;

	public event InputModuleSubmitEvent OnInputModuleSubmit;

	public event InputModuleTabEvent OnInputModuleTab;

	protected OWMenuInputModule()
	{
	}

	protected override void Start()
	{
		base.Start();
		_nextSelectableQueue = new List<Selectable>();
		_timeToButtonHold = 1f / _inputActionsPerSecond;
		OWInput.SharedInputManager.OnUpdateInputCommands += OnControlsChanged;
		if (SceneManager.GetActiveScene().name.Contains("Canvas"))
		{
			_debugScene = true;
		}
		if (_debugScene)
		{
			OWInput.ChangeInputMode(InputMode.Menu);
		}
		EnableMouseInputs();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		OWInput.SharedInputManager.OnUpdateInputCommands -= OnControlsChanged;
	}

	public void EnableInputs()
	{
		Debug.Log("ENABLE INPUT");
		_allowInputs = true;
	}

	public void DisableInputs()
	{
		Debug.Log("DISABLE INPUT");
		_allowInputs = false;
	}

	public void EnableMouseInputs()
	{
		_allowMouseInputs = true;
		SetPointerActionCallbacks(install: true);
		EnableAllActions();
	}

	public void DisableMouseInputs()
	{
		_allowMouseInputs = false;
		SetPointerActionCallbacks(install: false);
	}

	public bool IsPendingSelection()
	{
		if (_nextSelectableQueue.Count > 0)
		{
			return _nextSelectableQueue[0] != null;
		}
		return false;
	}

	public void SelectOnNextUpdate(Selectable selectable)
	{
		while (_nextSelectableQueue.Remove(selectable))
		{
		}
		_nextSelectableQueue.Add(selectable);
	}

	public override void ActivateModule()
	{
		base.ActivateModule();
		GameObject gameObject = base.eventSystem.currentSelectedGameObject;
		if (gameObject == null)
		{
			gameObject = base.eventSystem.firstSelectedGameObject;
		}
		base.eventSystem.SetSelectedGameObject(null, GetBaseEventData());
		base.eventSystem.SetSelectedGameObject(gameObject, GetBaseEventData());
	}

	public override void DeactivateModule()
	{
		base.DeactivateModule();
		Debug.Log("DeactivateModule");
		base.eventSystem.SetSelectedGameObject(null, GetBaseEventData());
	}

	public override void Process()
	{
		if (!_allowInputs)
		{
			return;
		}
		bool flag = false;
		while (_nextSelectableQueue.Count > 0)
		{
			flag = true;
			Selectable selectable = _nextSelectableQueue[0];
			_nextSelectableQueue.RemoveAt(0);
			if (selectable == null)
			{
				base.eventSystem.SetSelectedGameObject(null, GetBaseEventData());
			}
			else
			{
				selectable.Select();
			}
		}
		if (flag)
		{
			return;
		}
		if (_allowMouseInputs)
		{
			ProcessMouseEvent();
		}
		if (!OWInput.IsInputMode(InputMode.Menu | InputMode.KeyboardInput))
		{
			return;
		}
		bool flag2 = SendUpdateEventToSelectedObject();
		bool flag3 = false;
		GameObject currentSelectedGameObject = base.eventSystem.currentSelectedGameObject;
		if (currentSelectedGameObject != null)
		{
			flag3 = currentSelectedGameObject.GetComponent<InputField>() != null;
		}
		if (base.eventSystem.sendNavigationEvents)
		{
			if (!flag2)
			{
				flag2 |= SendMoveEventToSelectedObject();
			}
			if (!flag2 || flag3)
			{
				flag2 |= SendTabEventToSelectedObject();
			}
			if (!flag2 || flag3)
			{
				flag2 |= SendSubmitEventToSelectedObject(flag3);
			}
		}
	}

	private bool SendSubmitEventToSelectedObject(bool inputFieldSelected)
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		GameObject currentSelectedGameObject = base.eventSystem.currentSelectedGameObject;
		BaseEventData baseEventData = GetBaseEventData();
		if (!inputFieldSelected)
		{
			for (int i = 0; i < InputLibrary.submitCommandTypes.Length; i++)
			{
				IInputCommands inputCommand = InputLibrary.GetInputCommand(InputLibrary.submitCommandTypes[i]);
				if (inputCommand.IsNewlyPressed())
				{
					bool num = ExecuteEvents.Execute(currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
					if (this.OnInputModuleSubmit != null)
					{
						this.OnInputModuleSubmit(currentSelectedGameObject, baseEventData);
					}
					if (num)
					{
						inputCommand.ConsumeInput();
					}
					break;
				}
			}
		}
		if ((InputLibrary.enter.IsNewlyPressed() || InputLibrary.enter2.IsNewlyPressed() || InputLibrary.confirm.IsNewlyPressed()) && inputFieldSelected)
		{
			bool num2 = ExecuteEvents.Execute(currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
			if (this.OnInputModuleSubmit != null)
			{
				this.OnInputModuleSubmit(currentSelectedGameObject, baseEventData);
			}
			if (num2)
			{
				InputLibrary.enter.ConsumeInput();
				InputLibrary.enter2.ConsumeInput();
				InputLibrary.confirm.ConsumeInput();
			}
		}
		if ((InputLibrary.cancel.IsNewlyPressed() && !inputFieldSelected) || InputLibrary.escape.IsNewlyPressed() || (InputLibrary.cancel.IsNewlyPressed() && inputFieldSelected && OWInput.UsingGamepad()))
		{
			bool num3 = ExecuteEvents.Execute(currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
			if (this.OnInputModuleCancel != null)
			{
				this.OnInputModuleCancel(currentSelectedGameObject, baseEventData);
			}
			if (num3)
			{
				InputLibrary.cancel.ConsumeInput();
				InputLibrary.escape.ConsumeInput();
			}
		}
		if (InputLibrary.pause.IsNewlyPressed() && ExecuteEvents.CanHandleEvent<IPauseHandler>(base.eventSystem.currentSelectedGameObject) && ExecuteEvents.Execute(currentSelectedGameObject, baseEventData, OWExecuteEvents.pauseHandler))
		{
			InputLibrary.pause.ConsumeInput();
		}
		return baseEventData.used;
	}

	private bool MoveButtonHeld()
	{
		return (InputLibrary.up.IsPressed(_timeToButtonHold) || InputLibrary.up2.IsPressed(_timeToButtonHold)) | (InputLibrary.menuLeft.IsPressed(_timeToButtonHold) || InputLibrary.left2.IsPressed(_timeToButtonHold)) | (InputLibrary.down.IsPressed(_timeToButtonHold) || InputLibrary.down2.IsPressed(_timeToButtonHold)) | (InputLibrary.menuRight.IsPressed(_timeToButtonHold) || InputLibrary.right2.IsPressed(_timeToButtonHold));
	}

	private bool AllowMoveEventProcessing(float time)
	{
		return (InputLibrary.up.IsNewlyPressed() || InputLibrary.down.IsNewlyPressed()) | (InputLibrary.menuLeft.IsNewlyPressed() || InputLibrary.menuRight.IsNewlyPressed()) | (InputLibrary.up2.IsNewlyPressed() || InputLibrary.down2.IsNewlyPressed()) | (InputLibrary.left2.IsNewlyPressed() || InputLibrary.right2.IsNewlyPressed()) | (InputLibrary.tabL.IsNewlyPressed() || InputLibrary.tabR.IsNewlyPressed()) | (InputLibrary.tabL2.IsNewlyPressed() || InputLibrary.tabR2.IsNewlyPressed()) | InputLibrary.tab.IsNewlyPressed() | (time > m_NextAction);
	}

	private Vector2 GetRawMoveVector()
	{
		Vector2 zero = Vector2.zero;
		if (InputLibrary.up.IsPressed() || InputLibrary.up2.IsPressed())
		{
			zero.y = 1f;
		}
		else if (InputLibrary.down.IsPressed() || InputLibrary.down2.IsPressed())
		{
			zero.y = -1f;
		}
		else if (InputLibrary.menuLeft.IsPressed() || InputLibrary.left2.IsPressed())
		{
			zero.x = -1f;
		}
		else if (InputLibrary.menuRight.IsPressed() || InputLibrary.right2.IsPressed())
		{
			zero.x = 1f;
		}
		return zero;
	}

	private bool SendMoveEventToSelectedObject()
	{
		float unscaledTime = Time.unscaledTime;
		if (!AllowMoveEventProcessing(unscaledTime))
		{
			return false;
		}
		Vector2 rawMoveVector = GetRawMoveVector();
		AxisEventData axisEventData = GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0.6f);
		if (!Mathf.Approximately(axisEventData.moveVector.x, 0f) || !Mathf.Approximately(axisEventData.moveVector.y, 0f))
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
		}
		if (MoveButtonHeld())
		{
			m_NextAction = unscaledTime + 1f / _onMoveButtonHoldActionsPerSec;
		}
		else
		{
			m_NextAction = unscaledTime + 1f / _inputActionsPerSecond;
		}
		return axisEventData.used;
	}

	private bool SendTabEventToSelectedObject()
	{
		float unscaledTime = Time.unscaledTime;
		if (!AllowMoveEventProcessing(unscaledTime))
		{
			return false;
		}
		int num = 0;
		if (InputLibrary.tab.IsPressed())
		{
			num = ((!InputLibrary.shiftL.IsPressed() && !InputLibrary.shiftR.IsPressed()) ? 1 : (-1));
		}
		else if (InputLibrary.tabL.IsPressed() || InputLibrary.tabL2.IsPressed())
		{
			num = -1;
		}
		else if (InputLibrary.tabR.IsPressed() || InputLibrary.tabR2.IsPressed())
		{
			num = 1;
		}
		if (num != 0)
		{
			TabEventData eventData = new TabEventData(base.eventSystem, num);
			bool num2 = ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, eventData, OWUIEvents.tabEventHandler);
			if (this.OnInputModuleTab != null)
			{
				this.OnInputModuleTab(base.eventSystem.currentSelectedGameObject, eventData);
			}
			m_NextAction = unscaledTime + 1f / _inputActionsPerSecond;
			if (num2)
			{
				InputLibrary.tab.ConsumeInput();
				InputLibrary.tabL.ConsumeInput();
				InputLibrary.tabL2.ConsumeInput();
				InputLibrary.tabR.ConsumeInput();
				InputLibrary.tabR2.ConsumeInput();
			}
			return true;
		}
		return false;
	}

	private bool SendUpdateEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return baseEventData.used;
	}

	private void OnPoint(InputAction.CallbackContext context)
	{
		if (!CheckForRemovedDevice(ref context) && !context.canceled)
		{
			int pointerStateIndexFor = GetPointerStateIndexFor(context.control);
			if (pointerStateIndexFor != -1)
			{
				GetPointerStateForIndex(pointerStateIndexFor).screenPosition = context.ReadValue<Vector2>();
			}
		}
	}

	private bool IgnoreNextClick(ref InputAction.CallbackContext context)
	{
		if (explictlyIgnoreFocus)
		{
			return false;
		}
		if (context.canceled && !Application.isFocused)
		{
			return !context.control.device.canRunInBackground;
		}
		return false;
	}

	private void OnLeftClick(InputAction.CallbackContext context)
	{
		int pointerStateIndexFor = GetPointerStateIndexFor(ref context);
		if (pointerStateIndexFor != -1)
		{
			ref PointerModelExposed pointerStateForIndex = ref GetPointerStateForIndex(pointerStateIndexFor);
			pointerStateForIndex.leftButton.isPressed = context.ReadValueAsButton();
			pointerStateForIndex.changedThisFrame = true;
			if (IgnoreNextClick(ref context))
			{
				pointerStateForIndex.leftButton.ignoreNextClick = true;
			}
		}
	}

	private void OnRightClick(InputAction.CallbackContext context)
	{
		int pointerStateIndexFor = GetPointerStateIndexFor(ref context);
		if (pointerStateIndexFor != -1)
		{
			ref PointerModelExposed pointerStateForIndex = ref GetPointerStateForIndex(pointerStateIndexFor);
			pointerStateForIndex.rightButton.isPressed = context.ReadValueAsButton();
			pointerStateForIndex.changedThisFrame = true;
			if (IgnoreNextClick(ref context))
			{
				pointerStateForIndex.rightButton.ignoreNextClick = true;
			}
		}
	}

	private void OnMiddleClick(InputAction.CallbackContext context)
	{
		int pointerStateIndexFor = GetPointerStateIndexFor(ref context);
		if (pointerStateIndexFor != -1)
		{
			ref PointerModelExposed pointerStateForIndex = ref GetPointerStateForIndex(pointerStateIndexFor);
			pointerStateForIndex.middleButton.isPressed = context.ReadValueAsButton();
			pointerStateForIndex.changedThisFrame = true;
			if (IgnoreNextClick(ref context))
			{
				pointerStateForIndex.middleButton.ignoreNextClick = true;
			}
		}
	}

	private bool CheckForRemovedDevice(ref InputAction.CallbackContext context)
	{
		if (context.canceled && !context.control.device.added)
		{
			_needToPurgeStalePointers = true;
			return true;
		}
		return false;
	}

	private void OnScroll(InputAction.CallbackContext context)
	{
		int pointerStateIndexFor = GetPointerStateIndexFor(ref context);
		if (pointerStateIndexFor != -1)
		{
			GetPointerStateForIndex(pointerStateIndexFor).scrollDelta = context.ReadValue<Vector2>() * 0.05f;
		}
	}

	private void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
	{
		int pointerStateIndexFor = GetPointerStateIndexFor(ref context);
		if (pointerStateIndexFor != -1)
		{
			GetPointerStateForIndex(pointerStateIndexFor).worldOrientation = context.ReadValue<Quaternion>();
		}
	}

	private void OnTrackedDevicePosition(InputAction.CallbackContext context)
	{
		int pointerStateIndexFor = GetPointerStateIndexFor(ref context);
		if (pointerStateIndexFor != -1)
		{
			GetPointerStateForIndex(pointerStateIndexFor).worldPosition = context.ReadValue<Vector3>();
		}
	}

	private void OnControlsChanged()
	{
		_needToPurgeStalePointers = true;
	}

	private ref PointerModelExposed GetPointerStateForIndex(int index)
	{
		if (index == 0)
		{
			return ref _pointerStates.firstValue;
		}
		return ref _pointerStates.additionalValues[index - 1];
	}

	private int GetPointerStateIndexFor(ref InputAction.CallbackContext context)
	{
		if (CheckForRemovedDevice(ref context))
		{
			return -1;
		}
		InputActionPhase phase = context.phase;
		return GetPointerStateIndexFor(context.control, phase != InputActionPhase.Canceled);
	}

	private int GetPointerStateIndexFor(InputControl control, bool createIfNotExists = true)
	{
		InputDevice device = control.device;
		InputControl parent = control.parent;
		int num = _pointerTouchControls.IndexOfReference(parent);
		if (num != -1)
		{
			_currentPointerId = _pointerIds[num];
			_currentPointerIndex = num;
			_currentPointerType = UIPointerType.Touch;
			return num;
		}
		int num2 = device.deviceId;
		int num3 = 0;
		Vector2 screenPosition = Vector2.zero;
		if (parent is TouchControl touchControl)
		{
			num3 = touchControl.touchId.ReadValue();
			screenPosition = touchControl.position.ReadValue();
		}
		else if (parent is Touchscreen touchscreen)
		{
			num3 = touchscreen.primaryTouch.touchId.ReadValue();
			screenPosition = touchscreen.primaryTouch.position.ReadValue();
		}
		if (num3 != 0)
		{
			num2 = ExtendedPointerEventData.MakePointerIdForTouch(num2, num3);
		}
		if (_currentPointerId == num2)
		{
			return _currentPointerIndex;
		}
		if (num3 == 0)
		{
			for (int i = 0; i < _pointerIds.length; i++)
			{
				if (_pointerIds[i] == num2)
				{
					_currentPointerId = num2;
					_currentPointerIndex = i;
					_currentPointerType = _pointerStates[i].pointerType;
					return i;
				}
			}
		}
		if (!createIfNotExists)
		{
			return -1;
		}
		UIPointerType uIPointerType = UIPointerType.None;
		if (num3 != 0)
		{
			uIPointerType = UIPointerType.Touch;
		}
		else if (HaveControlForDevice(device, base.point))
		{
			uIPointerType = UIPointerType.MouseOrPen;
		}
		else if (HaveControlForDevice(device, base.trackedDevicePosition))
		{
			uIPointerType = UIPointerType.Tracked;
		}
		if (base.pointerBehavior == UnityEngine.InputSystem.UI.UIPointerBehavior.SingleMouseOrPenButMultiTouchAndTrack)
		{
			switch (uIPointerType)
			{
			case UIPointerType.MouseOrPen:
			{
				for (int k = 0; k < _pointerStates.length; k++)
				{
					if (_pointerStates[k].pointerType != UIPointerType.MouseOrPen)
					{
						SendPointerExitEventsAndRemovePointer(k);
						k--;
					}
				}
				break;
			}
			default:
			{
				for (int j = 0; j < _pointerStates.length; j++)
				{
					if (_pointerStates[j].pointerType == UIPointerType.MouseOrPen)
					{
						SendPointerExitEventsAndRemovePointer(j);
						j--;
					}
				}
				break;
			}
			case UIPointerType.None:
				break;
			}
		}
		if ((base.pointerBehavior == UnityEngine.InputSystem.UI.UIPointerBehavior.SingleUnifiedPointer && uIPointerType != 0) || (base.pointerBehavior == UnityEngine.InputSystem.UI.UIPointerBehavior.SingleMouseOrPenButMultiTouchAndTrack && uIPointerType == UIPointerType.MouseOrPen))
		{
			if (_currentPointerIndex == -1)
			{
				_currentPointerIndex = AllocatePointer(num2, num3, uIPointerType, control, device, (num3 != 0) ? parent : null);
			}
			else
			{
				ExtendedPointerEventData eventData = GetPointerStateForIndex(_currentPointerIndex).eventData;
				eventData.control = control;
				eventData.device = device;
				eventData.pointerType = uIPointerType;
				eventData.pointerId = num2;
				eventData.touchId = num3;
				eventData.trackedDeviceOrientation = default(Quaternion);
				eventData.trackedDevicePosition = default(Vector3);
			}
			if (uIPointerType == UIPointerType.Touch)
			{
				GetPointerStateForIndex(_currentPointerIndex).screenPosition = screenPosition;
			}
			_currentPointerId = num2;
			_currentPointerType = uIPointerType;
			return _currentPointerIndex;
		}
		int num4 = -1;
		if (uIPointerType != 0)
		{
			num4 = AllocatePointer(num2, num3, uIPointerType, control, device, (num3 != 0) ? parent : null);
		}
		else
		{
			if (_currentPointerId != -1)
			{
				return _currentPointerIndex;
			}
			ReadOnlyArray<InputControl>? readOnlyArray = base.point?.action?.controls;
			InputDevice inputDevice = ((readOnlyArray.HasValue && readOnlyArray.Value.Count > 0) ? readOnlyArray.Value[0].device : null);
			if (inputDevice != null && !(inputDevice is Touchscreen))
			{
				num4 = AllocatePointer(inputDevice.deviceId, 0, UIPointerType.MouseOrPen, readOnlyArray.Value[0], inputDevice);
			}
			else
			{
				ReadOnlyArray<InputControl>? readOnlyArray2 = base.trackedDevicePosition?.action?.controls;
				InputDevice inputDevice2 = ((readOnlyArray2.HasValue && readOnlyArray2.Value.Count > 0) ? readOnlyArray2.Value[0].device : null);
				num4 = ((inputDevice2 == null) ? AllocatePointer(num2, 0, UIPointerType.None, control, device) : AllocatePointer(inputDevice2.deviceId, 0, UIPointerType.Tracked, readOnlyArray2.Value[0], inputDevice2));
			}
		}
		if (uIPointerType == UIPointerType.Touch)
		{
			GetPointerStateForIndex(num4).screenPosition = screenPosition;
		}
		_currentPointerId = num2;
		_currentPointerIndex = num4;
		_currentPointerType = uIPointerType;
		return num4;
	}

	private int AllocatePointer(int pointerId, int touchId, UIPointerType pointerType, InputControl control, InputDevice device, InputControl touchControl = null)
	{
		ExtendedPointerEventData extendedPointerEventData = null;
		if (_pointerStates.Capacity > _pointerStates.length)
		{
			extendedPointerEventData = ((_pointerStates.length != 0) ? _pointerStates.additionalValues[_pointerStates.length - 1].eventData : _pointerStates.firstValue.eventData);
		}
		if (extendedPointerEventData == null)
		{
			extendedPointerEventData = new ExtendedPointerEventData(base.eventSystem);
		}
		extendedPointerEventData.pointerId = pointerId;
		extendedPointerEventData.touchId = touchId;
		extendedPointerEventData.pointerType = pointerType;
		extendedPointerEventData.control = control;
		extendedPointerEventData.device = device;
		_pointerIds.AppendWithCapacity(pointerId);
		_pointerTouchControls.AppendWithCapacity(touchControl);
		return _pointerStates.AppendWithCapacity(new PointerModelExposed(extendedPointerEventData));
	}

	private void ProcessMouseEvent()
	{
		if (_needToPurgeStalePointers)
		{
			PurgeStalePointers();
		}
		if (!base.eventSystem.isFocused && !shouldIgnoreFocus)
		{
			for (int i = 0; i < _pointerStates.length; i++)
			{
				_pointerStates[i].OnFrameFinished();
			}
			return;
		}
		for (int j = 0; j < _pointerStates.length; j++)
		{
			ref PointerModelExposed pointerStateForIndex = ref GetPointerStateForIndex(j);
			pointerStateForIndex.eventData.ReadDeviceState();
			pointerStateForIndex.CopyTouchOrPenStateFrom(pointerStateForIndex.eventData);
			ProcessPointer(ref pointerStateForIndex);
			if (pointerStateForIndex.pointerType == UIPointerType.Touch && !pointerStateForIndex.leftButton.isPressed && !pointerStateForIndex.leftButton.wasReleasedThisFrame)
			{
				RemovePointerAtIndex(j);
				j--;
			}
			else
			{
				pointerStateForIndex.OnFrameFinished();
			}
		}
	}

	private RaycastResult PerformRaycast(ExtendedPointerEventData eventData)
	{
		if (eventData == null)
		{
			throw new ArgumentNullException("eventData");
		}
		if (eventData.pointerType == UIPointerType.Tracked && TrackedDeviceRaycaster.s_Instances.length > 0)
		{
			for (int i = 0; i < TrackedDeviceRaycaster.s_Instances.length; i++)
			{
				TrackedDeviceRaycaster trackedDeviceRaycaster = TrackedDeviceRaycaster.s_Instances[i];
				m_RaycastResultCache.Clear();
				trackedDeviceRaycaster.PerformRaycast(eventData, m_RaycastResultCache);
				if (m_RaycastResultCache.Count > 0)
				{
					RaycastResult result = m_RaycastResultCache[0];
					m_RaycastResultCache.Clear();
					return result;
				}
			}
			return default(RaycastResult);
		}
		base.eventSystem.RaycastAll(eventData, m_RaycastResultCache);
		RaycastResult result2 = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
		m_RaycastResultCache.Clear();
		return result2;
	}

	private void SendPointerExitEventsAndRemovePointer(int index)
	{
		ExtendedPointerEventData eventData = _pointerStates[index].eventData;
		if (eventData.pointerEnter != null)
		{
			ProcessPointerMovement(eventData, null);
		}
		RemovePointerAtIndex(index);
	}

	private void RemovePointerAtIndex(int index)
	{
		ExtendedPointerEventData eventData = _pointerStates[index].eventData;
		if (index == _currentPointerIndex)
		{
			_currentPointerId = -1;
			_currentPointerIndex = -1;
			_currentPointerType = UIPointerType.None;
		}
		else if (_currentPointerIndex == _pointerIds.length - 1)
		{
			_currentPointerIndex = index;
		}
		_pointerIds.RemoveAtByMovingTailWithCapacity(index);
		_pointerTouchControls.RemoveAtByMovingTailWithCapacity(index);
		_pointerStates.RemoveAtByMovingTailWithCapacity(index);
		eventData.hovered.Clear();
		eventData.device = null;
		eventData.pointerCurrentRaycast = default(RaycastResult);
		eventData.pointerPressRaycast = default(RaycastResult);
		eventData.pointerPress = null;
		eventData.pointerPress = null;
		eventData.pointerDrag = null;
		eventData.pointerEnter = null;
		eventData.rawPointerPress = null;
		if (_pointerStates.length == 0)
		{
			_pointerStates.firstValue.eventData = eventData;
		}
		else
		{
			_pointerStates.additionalValues[_pointerStates.length - 1].eventData = eventData;
		}
	}

	private void PurgeStalePointers()
	{
		for (int i = 0; i < _pointerStates.length; i++)
		{
			InputDevice device = GetPointerStateForIndex(i).eventData.device;
			if (!device.added || (!HaveControlForDevice(device, base.point) && !HaveControlForDevice(device, base.trackedDevicePosition) && !HaveControlForDevice(device, base.trackedDeviceOrientation)))
			{
				SendPointerExitEventsAndRemovePointer(i);
				i--;
			}
		}
		_needToPurgeStalePointers = false;
	}

	private static bool HaveControlForDevice(InputDevice device, InputActionReference actionReference)
	{
		InputAction inputAction = actionReference?.action;
		if (inputAction == null)
		{
			return false;
		}
		ReadOnlyArray<InputControl> controls = inputAction.controls;
		for (int i = 0; i < controls.Count; i++)
		{
			if (controls[i].device == device)
			{
				return true;
			}
		}
		return false;
	}

	private void ProcessPointer(ref PointerModelExposed state)
	{
		ExtendedPointerEventData eventData = state.eventData;
		UIPointerType pointerType = eventData.pointerType;
		if (pointerType == UIPointerType.MouseOrPen && Cursor.lockState == CursorLockMode.Locked)
		{
			eventData.position = ((base.cursorLockBehavior == CursorLockBehavior.OutsideScreen) ? new Vector2(-1f, -1f) : new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f));
			eventData.delta = default(Vector2);
		}
		else if (pointerType == UIPointerType.Tracked)
		{
			Vector3 position = state.worldPosition;
			Quaternion quaternion = state.worldOrientation;
			if (base.xrTrackingOrigin != null)
			{
				position = base.xrTrackingOrigin.TransformPoint(position);
				quaternion = base.xrTrackingOrigin.rotation * quaternion;
			}
			eventData.trackedDeviceOrientation = quaternion;
			eventData.trackedDevicePosition = position;
		}
		else
		{
			eventData.delta = state.screenPosition - eventData.position;
			eventData.position = state.screenPosition;
		}
		eventData.Reset();
		eventData.pointerCurrentRaycast = PerformRaycast(eventData);
		if (pointerType == UIPointerType.Tracked && eventData.pointerCurrentRaycast.isValid)
		{
			Vector2 screenPosition = eventData.pointerCurrentRaycast.screenPosition;
			eventData.delta = screenPosition - eventData.position;
			eventData.position = eventData.pointerCurrentRaycast.screenPosition;
		}
		eventData.button = PointerEventData.InputButton.Left;
		state.leftButton.CopyPressStateTo(eventData);
		ProcessPointerMovement(ref state, eventData);
		if (state.changedThisFrame || (!(base.xrTrackingOrigin == null) && state.pointerType == UIPointerType.Tracked))
		{
			ProcessPointerButton(ref state.leftButton, eventData);
			ProcessPointerButtonDrag(ref state.leftButton, eventData);
			ProcessPointerScroll(ref state, eventData);
			eventData.button = PointerEventData.InputButton.Right;
			state.rightButton.CopyPressStateTo(eventData);
			ProcessPointerButton(ref state.rightButton, eventData);
			ProcessPointerButtonDrag(ref state.rightButton, eventData);
			eventData.button = PointerEventData.InputButton.Middle;
			state.middleButton.CopyPressStateTo(eventData);
			ProcessPointerButton(ref state.middleButton, eventData);
			ProcessPointerButtonDrag(ref state.middleButton, eventData);
		}
	}

	private void ProcessPointerMovement(ref PointerModelExposed pointer, ExtendedPointerEventData eventData)
	{
		GameObject currentPointerTarget = ((eventData.pointerType == UIPointerType.Touch && !pointer.leftButton.isPressed && !pointer.leftButton.wasReleasedThisFrame) ? null : eventData.pointerCurrentRaycast.gameObject);
		ProcessPointerMovement(eventData, currentPointerTarget);
	}

	private bool PointerShouldIgnoreTransform(Transform t)
	{
		if (base.eventSystem is MultiplayerEventSystem multiplayerEventSystem && multiplayerEventSystem.playerRoot != null && !t.IsChildOf(multiplayerEventSystem.playerRoot.transform))
		{
			return true;
		}
		return false;
	}

	private void ProcessPointerMovement(ExtendedPointerEventData eventData, GameObject currentPointerTarget)
	{
		if (currentPointerTarget == null || eventData.pointerEnter == null)
		{
			for (int i = 0; i < eventData.hovered.Count; i++)
			{
				ExecuteEvents.Execute(eventData.hovered[i], eventData, ExecuteEvents.pointerExitHandler);
			}
			eventData.hovered.Clear();
			if (currentPointerTarget == null)
			{
				eventData.pointerEnter = null;
				return;
			}
		}
		if (eventData.pointerEnter == currentPointerTarget && (bool)currentPointerTarget)
		{
			return;
		}
		Transform transform = BaseInputModule.FindCommonRoot(eventData.pointerEnter, currentPointerTarget)?.transform;
		if (eventData.pointerEnter != null)
		{
			Transform parent = eventData.pointerEnter.transform;
			while (parent != null && parent != transform)
			{
				ExecuteEvents.Execute(parent.gameObject, eventData, ExecuteEvents.pointerExitHandler);
				eventData.hovered.Remove(parent.gameObject);
				parent = parent.parent;
			}
		}
		eventData.pointerEnter = currentPointerTarget;
		if (currentPointerTarget != null)
		{
			Transform parent2 = currentPointerTarget.transform;
			while (parent2 != null && parent2 != transform && !PointerShouldIgnoreTransform(parent2))
			{
				ExecuteEvents.Execute(parent2.gameObject, eventData, ExecuteEvents.pointerEnterHandler);
				eventData.hovered.Add(parent2.gameObject);
				parent2 = parent2.parent;
			}
		}
	}

	private void ProcessPointerButton(ref PointerModelExposed.ButtonState button, PointerEventData eventData)
	{
		GameObject gameObject = eventData.pointerCurrentRaycast.gameObject;
		if (gameObject != null && PointerShouldIgnoreTransform(gameObject.transform))
		{
			return;
		}
		if (button.wasPressedThisFrame)
		{
			button.pressTime = Time.unscaledTime;
			eventData.delta = Vector2.zero;
			eventData.dragging = false;
			eventData.pressPosition = eventData.position;
			eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;
			eventData.eligibleForClick = true;
			eventData.useDragThreshold = true;
			GameObject eventHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(gameObject);
			if (eventHandler != base.eventSystem.currentSelectedGameObject && (eventHandler != null || base.deselectOnBackgroundClick))
			{
				base.eventSystem.SetSelectedGameObject(null, eventData);
			}
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, eventData, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			button.clickedOnSameGameObject = gameObject2 == eventData.lastPress && button.pressTime - eventData.clickTime <= 0.3f;
			if (eventData.clickCount > 0 && !button.clickedOnSameGameObject)
			{
				eventData.clickCount = 0;
				eventData.clickTime = 0f;
			}
			eventData.pointerPress = gameObject2;
			eventData.rawPointerPress = gameObject;
			eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (eventData.pointerDrag != null)
			{
				ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (button.wasReleasedThisFrame)
		{
			GameObject eventHandler2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			int num;
			if (eventData.pointerPress == eventHandler2)
			{
				num = (eventData.eligibleForClick ? 1 : 0);
				if (num != 0)
				{
					if (button.clickedOnSameGameObject)
					{
						int clickCount = eventData.clickCount + 1;
						eventData.clickCount = clickCount;
					}
					else
					{
						eventData.clickCount = 1;
					}
					eventData.clickTime = Time.unscaledTime;
				}
			}
			else
			{
				num = 0;
			}
			ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);
			if (num != 0)
			{
				ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerClickHandler);
			}
			else if (eventData.dragging && eventData.pointerDrag != null)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, eventData, ExecuteEvents.dropHandler);
			}
			eventData.eligibleForClick = false;
			eventData.pointerPress = null;
			eventData.rawPointerPress = null;
			if (eventData.dragging && eventData.pointerDrag != null)
			{
				ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.endDragHandler);
			}
			eventData.dragging = false;
			eventData.pointerDrag = null;
			button.ignoreNextClick = false;
		}
		button.CopyPressStateFrom(eventData);
	}

	private void ProcessPointerButtonDrag(ref PointerModelExposed.ButtonState button, ExtendedPointerEventData eventData)
	{
		if (!eventData.IsPointerMoving() || (eventData.pointerType == UIPointerType.MouseOrPen && Cursor.lockState == CursorLockMode.Locked) || eventData.pointerDrag == null)
		{
			return;
		}
		if (!eventData.dragging && (!eventData.useDragThreshold || (double)(eventData.pressPosition - eventData.position).sqrMagnitude >= (double)base.eventSystem.pixelDragThreshold * (double)base.eventSystem.pixelDragThreshold * (double)((eventData.pointerType == UIPointerType.Tracked) ? base.trackedDeviceDragThresholdMultiplier : 1f)))
		{
			ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.beginDragHandler);
			eventData.dragging = true;
		}
		if (eventData.dragging)
		{
			if (eventData.pointerPress != eventData.pointerDrag)
			{
				ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);
				eventData.eligibleForClick = false;
				eventData.pointerPress = null;
				eventData.rawPointerPress = null;
			}
			ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);
			button.CopyPressStateFrom(eventData);
		}
	}

	private static void ProcessPointerScroll(ref PointerModelExposed pointer, PointerEventData eventData)
	{
		Vector2 scrollDelta = pointer.scrollDelta;
		if (!Mathf.Approximately(scrollDelta.sqrMagnitude, 0f))
		{
			eventData.scrollDelta = scrollDelta;
			ExecuteEvents.ExecuteHierarchy(ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.pointerEnter), eventData, ExecuteEvents.scrollHandler);
		}
	}

	private void EnableAllActions()
	{
		EnableInputAction(base.point);
		EnableInputAction(base.leftClick);
		EnableInputAction(base.rightClick);
		EnableInputAction(base.middleClick);
		EnableInputAction(base.scrollWheel);
		EnableInputAction(base.trackedDeviceOrientation);
		EnableInputAction(base.trackedDevicePosition);
	}

	private void DisableAllActions()
	{
		DisableInputAction(base.point);
		DisableInputAction(base.leftClick);
		DisableInputAction(base.rightClick);
		DisableInputAction(base.middleClick);
		DisableInputAction(base.scrollWheel);
		DisableInputAction(base.trackedDeviceOrientation);
		DisableInputAction(base.trackedDevicePosition);
	}

	private void EnableInputAction(InputActionReference inputActionReference)
	{
		InputAction inputAction = inputActionReference?.action;
		if (inputAction != null)
		{
			if (s_InputActionReferenceCounts.TryGetValue(inputActionReference.action, out var value))
			{
				value.refCount++;
				s_InputActionReferenceCounts[inputAction] = value;
			}
			else
			{
				InputActionReferenceState inputActionReferenceState = default(InputActionReferenceState);
				inputActionReferenceState.refCount = 1;
				inputActionReferenceState.enabledByInputModule = !inputAction.enabled;
				value = inputActionReferenceState;
				s_InputActionReferenceCounts.Add(inputAction, value);
			}
			inputAction.Enable();
		}
	}

	private void DisableInputAction(InputActionReference inputActionReference)
	{
		InputAction inputAction = inputActionReference?.action;
		if (inputAction != null && s_InputActionReferenceCounts.TryGetValue(inputAction, out var value))
		{
			if (value.refCount - 1 == 0 && value.enabledByInputModule)
			{
				inputAction.Disable();
				s_InputActionReferenceCounts.Remove(inputAction);
			}
			else
			{
				value.refCount--;
				s_InputActionReferenceCounts[inputAction] = value;
			}
		}
	}

	private void SetPointerActionCallbacks(bool install)
	{
		SetActionCallback(base.point, OnPoint, install);
		SetActionCallback(base.leftClick, OnLeftClick, install);
		SetActionCallback(base.rightClick, OnRightClick, install);
		SetActionCallback(base.middleClick, OnMiddleClick, install);
		SetActionCallback(base.scrollWheel, OnScroll, install);
		SetActionCallback(base.trackedDeviceOrientation, OnTrackedDeviceOrientation, install);
		SetActionCallback(base.trackedDevicePosition, OnTrackedDevicePosition, install);
	}

	private static void SetActionCallback(InputActionReference actionReference, Action<InputAction.CallbackContext> callback, bool install)
	{
		if ((!install && callback == null) || actionReference == null)
		{
			return;
		}
		InputAction action = actionReference.action;
		if (action != null)
		{
			if (install)
			{
				action.performed += callback;
				action.canceled += callback;
			}
			else
			{
				action.performed -= callback;
				action.canceled -= callback;
			}
		}
	}
}
