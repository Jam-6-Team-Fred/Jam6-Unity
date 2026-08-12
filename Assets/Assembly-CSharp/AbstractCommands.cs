using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AbstractCommands : IInputCommands
{
	private IInputManager _inputManager;

	protected AxisIdentifier _axisID;

	private Vector2Int _inversionFactorVector = Vector2Int.one;

	protected List<Texture2D> textureList = new List<Texture2D>();

	protected bool isGamepadTextures;

	protected IInputManager InputManager => _inputManager ?? (_inputManager = OWInput.SharedInputManager);

	public InputConsts.InputCommandType CommandType { get; protected set; }

	public InputConsts.InputValueType ValueType { get; protected set; }

	public abstract bool IsRebindable { get; }

	public AxisIdentifier AxisID
	{
		get
		{
			return _axisID;
		}
		protected set
		{
			_axisID = value;
		}
	}

	public float PressedThreshold { get; set; } = 0.4f;


	public float Sensitivity { get; set; } = 1f;


	public int InversionFactor
	{
		get
		{
			return _inversionFactorVector.y;
		}
		set
		{
			_inversionFactorVector.y = value;
		}
	}

	protected bool Consumed { get; set; }

	public float PressDuration
	{
		get
		{
			if (!(Time.realtimeSinceStartup > InputStartedTime))
			{
				return 0f;
			}
			return Time.realtimeSinceStartup - InputStartedTime;
		}
	}

	protected float InputStartedTime { get; set; } = float.MaxValue;


	protected bool WasActiveLastFrame { get; set; }

	protected bool IsActiveThisFrame { get; set; }

	protected virtual Vector2 AxisValue { get; set; }

	protected virtual float SingleValue => AxisValue.x;

	public event Action OnStarted;

	public event Action OnPerformed;

	public event Action OnCancelled;

	private void InputStarted()
	{
		this.OnStarted?.Invoke();
	}

	private void InputPerformed()
	{
		this.OnPerformed?.Invoke();
	}

	private void InputCancelled()
	{
		this.OnCancelled?.Invoke();
	}

	protected void SetAction<T>(ref T action, in T newAction) where T : class, IInputAction
	{
		if (action != null)
		{
			action.Enable(enable: false);
			action.OnStarted -= InputStarted;
			action.OnPerformed -= InputPerformed;
			action.OnCancelled -= InputCancelled;
		}
		action = newAction;
		if (action != null)
		{
			action.OnStarted += InputStarted;
			action.OnPerformed += InputPerformed;
			action.OnCancelled += InputCancelled;
			action.Enable(enable: true);
		}
	}

	public bool IsPressed(float minPressDuration = 0f)
	{
		if (IsActiveThisFrame)
		{
			return Time.realtimeSinceStartup - InputStartedTime >= minPressDuration;
		}
		return false;
	}

	public bool IsNewlyPressed()
	{
		if (Consumed)
		{
			return false;
		}
		if (!WasActiveLastFrame)
		{
			return IsActiveThisFrame;
		}
		return false;
	}

	public bool IsNewlyReleased()
	{
		if (WasActiveLastFrame)
		{
			return !IsActiveThisFrame;
		}
		return false;
	}

	public float GetValue()
	{
		return SingleValue * Sensitivity * (float)InversionFactor;
	}

	public Vector2 GetAxisValue(bool useSensitivity = true)
	{
		Vector2 result = AxisValue * _inversionFactorVector;
		if (useSensitivity)
		{
			result *= Sensitivity;
		}
		return result;
	}

	public void BlockNextRelease()
	{
	}

	public void ConsumeInput()
	{
		Consumed = true;
	}

	public abstract InputControl GetActiveDevice();

	public abstract bool HasSameBinding(IInputCommands toCompare, bool usingGamepad);

	public abstract List<Texture2D> GetUITextures(bool gamepad, bool forceRefresh = false);

	protected abstract void UpdateFromAction();

	public abstract void EnableAllActions(bool enable);

	public void Update()
	{
		Consumed = false;
		WasActiveLastFrame = IsActiveThisFrame;
		IsActiveThisFrame = false;
		UpdateFromAction();
		if (!IsActiveThisFrame)
		{
			if (WasActiveLastFrame)
			{
				InputStartedTime = float.MaxValue;
			}
		}
		else if (!WasActiveLastFrame)
		{
			InputStartedTime = Time.realtimeSinceStartup;
		}
	}
}
