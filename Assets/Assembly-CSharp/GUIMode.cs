using UnityEngine;

public class GUIMode : MonoBehaviour
{
	public enum RenderMode
	{
		Full = 0,
		FPS = 1,
		Debug = 2,
		GhostAI = 3,
		Input = 4,
		InputVerbose = 5,
		InputRaw = 6,
		Capture = 7,
		Hidden = 8
	}

	private static RenderMode _renderMode;

	private void Awake()
	{
	}

	public static bool AreScreenPromptsVisible()
	{
		if (_renderMode != RenderMode.Capture)
		{
			return _renderMode != RenderMode.Hidden;
		}
		return false;
	}

	public static bool AreHUDMarkersVisible()
	{
		if (_renderMode != RenderMode.Capture)
		{
			return _renderMode != RenderMode.Hidden;
		}
		return false;
	}

	public static bool IsReticleVisible()
	{
		if (_renderMode != RenderMode.Capture)
		{
			return _renderMode != RenderMode.Hidden;
		}
		return false;
	}

	public static bool IsFullMode()
	{
		return _renderMode == RenderMode.Full;
	}

	public static bool IsFPSMode()
	{
		return _renderMode == RenderMode.FPS;
	}

	public static bool IsHiddenMode()
	{
		return _renderMode == RenderMode.Hidden;
	}

	public static bool IsCaptureMode()
	{
		return _renderMode == RenderMode.Capture;
	}

	public static bool IsDebugMode()
	{
		return _renderMode == RenderMode.Debug;
	}

	public static bool IsGhostAIMode()
	{
		return _renderMode == RenderMode.GhostAI;
	}

	public static bool IsInputMode()
	{
		return _renderMode == RenderMode.Input;
	}

	public static bool IsInputVerboseMode()
	{
		return _renderMode == RenderMode.InputVerbose;
	}

	public static bool IsInputRawMode()
	{
		return _renderMode == RenderMode.InputRaw;
	}

	public static void SetRenderMode(RenderMode mode)
	{
		if (mode != _renderMode)
		{
			_renderMode = mode;
			GlobalMessenger.FireEvent("ChangeGUIMode");
		}
	}
}
