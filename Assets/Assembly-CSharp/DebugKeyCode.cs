using UnityEngine;
using UnityEngine.InputSystem;

public class DebugKeyCode
{
	public static KeyCode destroyShip = KeyCode.Delete;

	public static KeyCode cycleGUIMode = KeyCode.F1;

	public static KeyCode suitUp = KeyCode.F2;

	public static KeyCode learnLaunchCodes = KeyCode.F3;

	public static KeyCode refillResources = KeyCode.F4;

	public static KeyCode learnForgetFrequencies = KeyCode.F5;

	public static KeyCode spawnEffect = KeyCode.F6;

	public static KeyCode powerOverwhelming = KeyCode.F7;

	public static KeyCode revealAllShipLogFacts = KeyCode.F8;

	public static KeyCode triggerDisconnectMenu = KeyCode.F9;

	public static KeyCode saveProbeSnapshot = KeyCode.F11;

	public static KeyCode triggerSupernova = KeyCode.End;

	public static KeyCode destroyTimeline = KeyCode.KeypadPeriod;

	public static KeyCode breakAllFragments = KeyCode.Home;

	public static KeyCode activateWarp = KeyCode.BackQuote;

	public static KeyCode timeLapse = KeyCode.Equals;

	public static KeyCode fastForwardTimeLoop = KeyCode.RightBracket;

	public static KeyCode rewindTimeLoop = KeyCode.LeftBracket;

	public static KeyCode uiTestAndSuicide = KeyCode.Keypad0;

	public static KeyCode probeAnchorDebug = KeyCode.Backslash;

	public static KeyCode cometWarp = KeyCode.Alpha1;

	public static KeyCode hourglassTwinsWarp = KeyCode.Alpha2;

	public static KeyCode homePlanetWarp = KeyCode.Alpha3;

	public static KeyCode brittleHollowWarp = KeyCode.Alpha4;

	public static KeyCode gasGiantWarp = KeyCode.Alpha5;

	public static KeyCode darkBrambleWarp = KeyCode.Alpha6;

	public static KeyCode shipWarp = KeyCode.Alpha0;

	public static KeyCode quantumWarp = KeyCode.Alpha7;

	public static KeyCode moonWarp = KeyCode.Alpha8;

	public static KeyCode IPWarp = KeyCode.Alpha9;

	public static KeyCode damageAutopilot = KeyCode.Alpha1;

	public static KeyCode damageLights = KeyCode.Alpha2;

	public static KeyCode damageCamera = KeyCode.Alpha3;

	public static KeyCode damageFuel = KeyCode.Alpha4;

	public static KeyCode damageOxygen = KeyCode.Alpha5;

	public static KeyCode damageGravity = KeyCode.Alpha6;

	public static KeyCode damageElectric = KeyCode.Alpha0;

	public static KeyCode damageLeftThrust = KeyCode.Alpha7;

	public static KeyCode damageRightThrust = KeyCode.Alpha8;

	public static KeyCode damageReactor = KeyCode.Alpha9;

	public static KeyCode damageModifierHull = KeyCode.Alpha0;

	public static KeyCode damageLanding = KeyCode.Alpha1;

	public static KeyCode damageTop = KeyCode.Alpha2;

	public static KeyCode damagePort = KeyCode.Alpha3;

	public static KeyCode damageStarboard = KeyCode.Alpha4;

	public static KeyCode damageForward = KeyCode.Alpha5;

	public static KeyCode damageAft = KeyCode.Alpha6;

	public static KeyCode putOnHelmet = KeyCode.I;

	public static KeyCode altWarp = KeyCode.P;

	public static KeyCode warpAllowed = KeyCode.Minus;

	public static KeyCode hidePlayerBody = KeyCode.B;

	public static KeyCode cycleUiSize = KeyCode.Period;

	public static KeyCode setWarpPoint = KeyCode.PageDown;

	public static KeyCode gotoWarpPoint = KeyCode.PageUp;

	public static KeyCode advanceRingWorldState = KeyCode.K;

	public static KeyCode extinguishLighthouseLamp = KeyCode.L;

	public static KeyCode toggleDreamObjectProjectors = KeyCode.L;

	public static bool GetKey(KeyCode keyCode)
	{
		if (Keyboard.current == null)
		{
			return false;
		}
		if (InputTransitionUtil.TryGetKey(keyCode, out var key))
		{
			return Keyboard.current[key].isPressed;
		}
		return false;
	}

	public static bool GetKeyUp(KeyCode keyCode)
	{
		if (Keyboard.current == null)
		{
			return false;
		}
		if (InputTransitionUtil.TryGetKey(keyCode, out var key))
		{
			return Keyboard.current[key].wasReleasedThisFrame;
		}
		return false;
	}

	public static bool GetKeyDown(KeyCode keyCode)
	{
		if (Keyboard.current == null)
		{
			return false;
		}
		if (InputTransitionUtil.TryGetKey(keyCode, out var key))
		{
			return Keyboard.current[key].wasPressedThisFrame;
		}
		return false;
	}
}
