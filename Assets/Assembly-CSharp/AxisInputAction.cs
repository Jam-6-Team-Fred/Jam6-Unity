using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class AxisInputAction : AbstractInputAction<float>, IInputAction<float>, IInputAction, IAxisInputAction, ISingleAction
{
	protected InputControl _mouseAxisControl;

	protected AxisSmoothingBuffer _smoothingBuffer;

	public InputAction Action { get; protected set; }

	public override InputControl ActiveControl => Action?.activeControl;

	protected AxisInputAction()
	{
	}

	public AxisInputAction(InputAction action)
	{
		SetAction(action);
	}

	public void SetAction(InputAction action)
	{
		if (action == null)
		{
			Debug.LogError("Failed to set InputAction..");
			return;
		}
		Enable(enable: false);
		Action = action;
		InitializeAxisID();
		_mouseAxisControl = null;
		for (int i = 0; i < Action.controls.Count; i++)
		{
			if (Action.controls[i].device is Mouse && !(Action.controls[i] is ButtonControl))
			{
				_mouseAxisControl = Action.controls[i];
				break;
			}
		}
		Enable(enable: true);
	}

	protected override void InitializeAxisID()
	{
		InputActionUtil.TryGetAxisID(Action, gamepad: true, out _axisIdGamepad);
		InputActionUtil.TryGetAxisID(Action, gamepad: false, out _axisIdKbm);
	}

	public override void Enable(bool enable)
	{
		EnableAction(Action, enable);
	}

	public override void Update()
	{
		base.ValueType = InputConsts.FindValueType(Action);
		base.HasActiveMouseInput = Action.activeControl != null && !(Action.activeControl is ButtonControl) && Action.activeControl.device is Mouse;
		float value = Action.ReadValue<float>();
		if (!OWInput.UsingGamepad() && _mouseAxisControl != null)
		{
			if (_smoothingBuffer == null)
			{
				_smoothingBuffer = new AxisSmoothingBuffer();
			}
			value = ((!(_mouseAxisControl.name == "x")) ? BaseInputManager.MouseDelta.y : BaseInputManager.MouseDelta.x);
			TryApplyMouseProcessing(_smoothingBuffer, ref value);
		}
		base.Value = value;
		PhaseUpdate(Action.phase);
	}

	public override bool HasSameBinding(IInputAction compare, bool usingGamepad)
	{
		InputActionUtil.ExtractInputActions(compare, out var first, out var second);
		if (first == null || second != null)
		{
			return false;
		}
		return InputActionUtil.UsingSameBinding(Action, first, usingGamepad);
	}

	public override List<Texture2D> GetUITextures(bool gamepad, bool combineImagesWhenPossible = false, bool forceRefresh = false)
	{
		List<Texture2D> textureList = new List<Texture2D>();
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
		InputActionUtil.PopulateUITextureList(Action, in textureList, gamepad);
		return textureList;
	}
}
