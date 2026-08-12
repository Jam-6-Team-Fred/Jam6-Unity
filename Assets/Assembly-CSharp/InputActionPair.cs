using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputActionPair : AbstractInputAction<float>, IInputAction<float>, IInputAction, IAxisInputAction, IInputActionPair
{
	protected InputControl _mouseAxisControlPrimary;

	protected InputControl _mouseAxisControlSecondary;

	protected AxisSmoothingBuffer _smoothingBufferPrimary;

	protected AxisSmoothingBuffer _smoothingBufferSecondary;

	public InputAction PrimaryAction { get; protected set; }

	public InputAction SecondaryAction { get; protected set; }

	public override InputControl ActiveControl => PrimaryAction?.activeControl;

	protected InputActionPair()
	{
	}

	public InputActionPair(InputAction primaryAction, InputAction secondaryAction)
	{
		SetAction(primaryAction, secondaryAction);
	}

	public void SetAction(InputAction primary, InputAction secondary)
	{
		if (primary == null || secondary == null)
		{
			Debug.LogError("Failed to set InputActions for InputActionPair.");
			return;
		}
		Enable(enable: false);
		PrimaryAction = primary;
		SecondaryAction = secondary;
		PostInitializeActions();
		Enable(enable: true);
	}

	protected void PostInitializeActions()
	{
		InitializeAxisID();
		_mouseAxisControlPrimary = null;
		for (int i = 0; i < PrimaryAction.controls.Count; i++)
		{
			if (PrimaryAction.controls[i].device is Mouse && !(PrimaryAction.controls[i] is ButtonControl))
			{
				_mouseAxisControlPrimary = PrimaryAction.controls[i];
				break;
			}
		}
		_mouseAxisControlSecondary = null;
		for (int j = 0; j < SecondaryAction.controls.Count; j++)
		{
			if (SecondaryAction.controls[j].device is Mouse && !(SecondaryAction.controls[j] is ButtonControl))
			{
				_mouseAxisControlSecondary = SecondaryAction.controls[j];
				break;
			}
		}
	}

	protected override void InitializeAxisID()
	{
		InputActionUtil.TryGetSharedAxisID(PrimaryAction, SecondaryAction, gamepad: true, out _axisIdGamepad);
		InputActionUtil.TryGetSharedAxisID(PrimaryAction, SecondaryAction, gamepad: false, out _axisIdKbm);
	}

	public override void Enable(bool enable)
	{
		EnableAction(PrimaryAction, enable);
		EnableAction(SecondaryAction, enable);
	}

	public override void Update()
	{
		bool flag = PrimaryAction.activeControl != null && !(PrimaryAction.activeControl is ButtonControl) && PrimaryAction.activeControl.device is Mouse;
		bool flag2 = SecondaryAction.activeControl != null && !(SecondaryAction.activeControl is ButtonControl) && SecondaryAction.activeControl.device is Mouse;
		base.ValueType = InputConsts.InputValueType.SINGLE_AXIS;
		base.HasActiveMouseInput = flag || flag2;
		bool flag3 = InputActionUtil.UsingSameBinding(PrimaryAction, SecondaryAction, OWInput.UsingGamepad());
		float value = PrimaryAction.ReadValue<float>();
		float value2 = SecondaryAction.ReadValue<float>();
		if (!OWInput.UsingGamepad() && _mouseAxisControlPrimary != null)
		{
			if (_smoothingBufferPrimary == null)
			{
				_smoothingBufferPrimary = new AxisSmoothingBuffer();
			}
			value = ((!(_mouseAxisControlPrimary.name == "x")) ? BaseInputManager.MouseDelta.y : BaseInputManager.MouseDelta.x);
			TryApplyMouseProcessing(_smoothingBufferPrimary, ref value);
		}
		if (!OWInput.UsingGamepad() && _mouseAxisControlSecondary != null && !flag3)
		{
			if (_smoothingBufferSecondary == null)
			{
				_smoothingBufferSecondary = new AxisSmoothingBuffer();
			}
			value2 = ((!(_mouseAxisControlSecondary.name == "x")) ? BaseInputManager.MouseDelta.y : BaseInputManager.MouseDelta.x);
			TryApplyMouseProcessing(_smoothingBufferSecondary, ref value2);
		}
		base.Value = value;
		if (!flag3)
		{
			base.Value -= value2;
		}
		PhaseUpdate();
	}

	protected override void PhaseUpdate(InputActionPhase nextPhase = InputActionPhase.Waiting)
	{
		InputActionPhase phase = PrimaryAction.phase;
		InputActionPhase phase2 = SecondaryAction.phase;
		if (phase == InputActionPhase.Disabled || phase2 == InputActionPhase.Disabled)
		{
			base.Phase = InputActionPhase.Disabled;
			return;
		}
		bool flag = base.Phase == InputActionPhase.Performed || base.Phase == InputActionPhase.Started;
		if (phase == InputActionPhase.Performed || phase2 == InputActionPhase.Performed)
		{
			nextPhase = (flag ? InputActionPhase.Performed : InputActionPhase.Started);
		}
		else if (flag)
		{
			nextPhase = InputActionPhase.Canceled;
		}
		base.PhaseUpdate(nextPhase);
	}

	public override bool HasSameBinding(IInputAction compare, bool usingGamepad)
	{
		InputActionUtil.ExtractInputActions(compare, out var first, out var second);
		if (first == null || second == null)
		{
			return false;
		}
		if (InputActionUtil.UsingSameBinding(PrimaryAction, first, usingGamepad))
		{
			return InputActionUtil.UsingSameBinding(SecondaryAction, second, usingGamepad);
		}
		return false;
	}

	public override List<Texture2D> GetUITextures(bool gamepad, bool combineImagesWhenPossible, bool forceRefresh = false)
	{
		List<Texture2D> textureList = new List<Texture2D>();
		if (combineImagesWhenPossible)
		{
			if (gamepad)
			{
				if (_axisIdGamepad != 0)
				{
					textureList.Add(ButtonPromptLibrary.SharedInstance.GetAxisTexture(_axisIdGamepad));
					return textureList;
				}
			}
			else if (_axisIdKbm != 0)
			{
				textureList.Add(ButtonPromptLibrary.SharedInstance.GetAxisTexture(_axisIdKbm));
				return textureList;
			}
		}
		InputActionUtil.PopulateUITextureList(PrimaryAction, in textureList, gamepad);
		InputActionUtil.PopulateUITextureList(SecondaryAction, in textureList, gamepad);
		return textureList;
	}
}
