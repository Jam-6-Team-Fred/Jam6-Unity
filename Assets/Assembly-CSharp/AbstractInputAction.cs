using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AbstractInputAction<T> : AbstractInputAction, IInputAction<T>, IInputAction where T : struct
{
	public T Value { get; protected set; }
}
public abstract class AbstractInputAction : IInputAction
{
	private IInputManager _inputManager;

	protected AxisIdentifier _axisIdKbm;

	protected AxisIdentifier _axisIdGamepad;

	protected IInputManager InputManager => _inputManager ?? (_inputManager = OWInput.SharedInputManager);

	public AxisIdentifier AxisID
	{
		get
		{
			if (!OWInput.UsingGamepad())
			{
				return _axisIdKbm;
			}
			return _axisIdGamepad;
		}
	}

	public InputActionPhase Phase { get; protected set; }

	public abstract InputControl ActiveControl { get; }

	public bool HasActiveMouseInput { get; protected set; }

	public InputConsts.InputValueType ValueType { get; protected set; }

	public event Action OnStarted;

	public event Action OnPerformed;

	public event Action OnCancelled;

	protected virtual void InputStarted(InputAction.CallbackContext context)
	{
		this.OnStarted?.Invoke();
	}

	protected virtual void InputPerformed(InputAction.CallbackContext context)
	{
		InputManager.UpdateLastInputCommandType(context);
		this.OnPerformed?.Invoke();
	}

	protected virtual void InputCancelled(InputAction.CallbackContext context)
	{
		this.OnCancelled?.Invoke();
	}

	protected virtual void PhaseUpdate(InputActionPhase nextPhase = InputActionPhase.Waiting)
	{
		if (Phase != nextPhase)
		{
			switch (nextPhase)
			{
			case InputActionPhase.Started:
				this.OnStarted?.Invoke();
				break;
			case InputActionPhase.Performed:
				this.OnPerformed?.Invoke();
				break;
			case InputActionPhase.Canceled:
				this.OnCancelled?.Invoke();
				break;
			}
		}
		Phase = nextPhase;
	}

	public abstract void Enable(bool enable);

	public abstract void Update();

	public abstract bool HasSameBinding(IInputAction toCompare, bool usingGamepad);

	public abstract List<Texture2D> GetUITextures(bool gamepad, bool combineImagesWhenPossible = false, bool forceRefresh = false);

	protected abstract void InitializeAxisID();

	protected void EnableAction(InputAction action, bool enable)
	{
		if (action != null)
		{
			if (enable)
			{
				action.Enable();
				action.started += InputStarted;
				action.performed += InputPerformed;
				action.canceled += InputCancelled;
			}
			else
			{
				action.started -= InputStarted;
				action.performed -= InputPerformed;
				action.canceled -= InputCancelled;
				action.Disable();
			}
		}
	}

	protected void TryApplyMouseProcessing(AxisSmoothingBuffer smoothingBuffer, ref float value)
	{
		smoothingBuffer.Update(value);
		value = smoothingBuffer.GetAverage() * 0.05f;
	}
}
