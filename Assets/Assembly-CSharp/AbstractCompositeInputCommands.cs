using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractCompositeInputCommands<T> : AbstractCommands, IInputCommands, ICompositeInputCommands where T : class, IInputAction
{
	protected T _primaryAction;

	protected T _secondaryAction;

	public IInputAction PrimaryAction => _primaryAction;

	public IInputAction SecondaryAction => _secondaryAction;

	public override bool IsRebindable
	{
		get
		{
			if (_primaryAction is IRebindableInputAction)
			{
				return _secondaryAction is IRebindableInputAction;
			}
			return false;
		}
	}

	public bool TryCastActions<TA>(out TA castPrimary, out TA castSecondary) where TA : IInputAction
	{
		castPrimary = default(TA);
		castSecondary = default(TA);
		T primaryAction;
		if ((primaryAction = _primaryAction) is TA)
		{
			TA val = (TA)(object)primaryAction;
			if ((primaryAction = _secondaryAction) is TA)
			{
				TA val2 = (TA)(object)primaryAction;
				castPrimary = val;
				castSecondary = val2;
				return true;
			}
			return false;
		}
		return false;
	}

	public override bool HasSameBinding(IInputCommands compare, bool usingGamepad)
	{
		InputActionUtil.ExtractInputActions(compare, out var primary, out var secondary);
		if (primary == null || secondary == null)
		{
			return false;
		}
		if (InputActionUtil.UsingSameBinding(PrimaryAction, primary, usingGamepad))
		{
			return InputActionUtil.UsingSameBinding(SecondaryAction, secondary, usingGamepad);
		}
		return false;
	}

	public override List<Texture2D> GetUITextures(bool gamepad, bool forceRefresh = false)
	{
		if (textureList.Count > 0 && isGamepadTextures == gamepad && !forceRefresh)
		{
			return textureList;
		}
		textureList.Clear();
		isGamepadTextures = gamepad;
		if (gamepad && !OWInput.UsingGamepad())
		{
			return textureList;
		}
		if (base.AxisID != 0)
		{
			textureList.Add(ButtonPromptLibrary.SharedInstance.GetAxisTexture(base.AxisID));
			return textureList;
		}
		int num;
		int num2;
		if (PrimaryAction.AxisID != 0)
		{
			num = ((SecondaryAction.AxisID != AxisIdentifier.NONE) ? 1 : 0);
			if (num != 0)
			{
				num2 = ((PrimaryAction.AxisID == SecondaryAction.AxisID) ? 1 : 0);
				goto IL_00ab;
			}
		}
		else
		{
			num = 0;
		}
		num2 = 0;
		goto IL_00ab;
		IL_00ab:
		bool flag = (byte)num2 != 0;
		if (num != 0)
		{
			textureList.Add(ButtonPromptLibrary.SharedInstance.GetAxisTexture(PrimaryAction.AxisID));
			if (!flag)
			{
				textureList.Add(ButtonPromptLibrary.SharedInstance.GetAxisTexture(SecondaryAction.AxisID));
			}
			return textureList;
		}
		textureList.AddRange(PrimaryAction.GetUITextures(gamepad, combineImagesWhenPossible: true));
		List<Texture2D> uITextures = SecondaryAction.GetUITextures(gamepad, combineImagesWhenPossible: true);
		uITextures.Reverse();
		textureList.AddRange(uITextures);
		return textureList;
	}

	public override void EnableAllActions(bool enable)
	{
		if (PrimaryAction != null)
		{
			PrimaryAction.Enable(enable);
		}
		if (SecondaryAction != null)
		{
			SecondaryAction.Enable(enable);
		}
	}
}
