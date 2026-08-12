using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Layouts;

public class OWDeviceLayoutOverride
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		InputSystem.RegisterLayoutOverride(BuildDualShock4LayoutOverride().ToJson());
		InputSystem.RegisterLayoutOverride(BuildDualSenseLayoutOverride().ToJson());
	}

	private static InputControlLayout BuildDualShock4LayoutOverride()
	{
		InputControlLayout.Builder builder = new InputControlLayout.Builder().Extend("DualShock4GamepadHID").WithType<DualShock4GamepadHID>().WithName("PS4ShareButtonOverride");
		builder.AddControl("select").WithDisplayName("Touchpad Button").WithByteOffset(6u)
			.WithBitOffset(1u);
		builder.AddControl("share").WithDisplayName("Share").WithLayout("Button")
			.WithByteOffset(5u)
			.WithBitOffset(4u);
		builder.AddControl("touchpadButton").WithDisplayName("Touchpad Button");
		return builder.Build();
	}

	private static InputControlLayout BuildDualSenseLayoutOverride()
	{
		InputControlLayout.Builder builder = new InputControlLayout.Builder().Extend("DualSenseGamepadHID").WithType<DualSenseGamepadHID>().WithName("PS5ShareButtonOverride");
		builder.AddControl("select").WithDisplayName("Touchpad Button").WithByteOffset(10u)
			.WithBitOffset(1u);
		builder.AddControl("share").WithDisplayName("Share").WithLayout("Button")
			.WithByteOffset(9u)
			.WithBitOffset(4u);
		builder.AddControl("touchpadButton").WithDisplayName("Touchpad Button");
		return builder.Build();
	}
}
