using UnityEngine;

public static class OWInputProcessorUtil
{
	public const float MOUSE_IDLE_DELAY = 0.2f;

	public const float MOUSE_INPUT_SCALE = 0.05f;

	public const float DEFAULT_PRESSED_THRESHOLD = 0.4f;

	public const float JOYSTICK_DEFAULT_INNER_DEADZONE = 0.2f;

	public const float JOYSTICK_DEFAULT_OUTER_DEADZONE = 0.05f;

	public const float TRIGGER_DEFAULT_INNER_DEADZONE = 0.1f;

	public const float TRIGGER_DEFAULT_OUTER_DEADZONE = 0.05f;

	private static float s_innerDeadZoneMult = 1f;

	private static float s_outerDeadZoneMult = 1f;

	public static float InnerDeadZoneMultiplier => s_innerDeadZoneMult;

	public static float OuterDeadZoneMultiplier => s_outerDeadZoneMult;

	public static bool SetDefaultValue(ref float value, float actualDefault)
	{
		bool num = value == 0f;
		if (num)
		{
			value = actualDefault;
		}
		return num;
	}

	public static float ApplyOWAxisDeadzone(float value, float inner, float outer)
	{
		float num = Mathf.Sign(value);
		float value2 = Mathf.Abs(value);
		return num * Mathf.InverseLerp(inner, 1f - outer, value2);
	}

	public static Vector2 ApplyOWDoubleAxisDeadzones(Vector2 value, float inner, float outer)
	{
		return value.normalized * Mathf.InverseLerp(inner, 1f - outer, value.magnitude);
	}

	public static void SetDeadZoneMultipliers(float innerMult, float outerMult)
	{
		s_innerDeadZoneMult = innerMult;
		s_outerDeadZoneMult = outerMult;
	}
}
