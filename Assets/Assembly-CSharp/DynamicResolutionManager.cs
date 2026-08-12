using UnityEngine;
using UnityEngine.SceneManagement;

public class DynamicResolutionManager : MonoBehaviour
{
	public enum TargetResolution
	{
		Full = -1,
		_720 = 720,
		_900 = 900,
		_1080 = 1080,
		_1152 = 1152,
		_1224 = 1224,
		_1296 = 1296,
		_1368 = 1368,
		_1440 = 1440,
		_1512 = 1512,
		_1584 = 1584,
		_1656 = 1656,
		_1728 = 1728,
		_1800 = 1800,
		_1872 = 1872,
		_1944 = 1944,
		_2016 = 2016,
		_2088 = 2088,
		_2160 = 2160
	}

	private static DynamicResolutionManager s_instance = null;

	private static bool s_active = true;

	private static float s_currentResScale = 1f;

	private static float s_targetResScale = 1f;

	private static float s_resChangeSpeed = 1f / 6f;

	public static bool isActive
	{
		get
		{
			if (s_instance != null)
			{
				return s_active;
			}
			return false;
		}
		set
		{
			s_active = value;
			if (s_instance != null && s_active)
			{
				s_instance.enabled = true;
			}
		}
	}

	public static float currentResolutionScale => s_currentResScale;

	public static float targetResolutionScale
	{
		get
		{
			return s_targetResScale;
		}
		set
		{
			s_targetResScale = Mathf.Clamp01(value);
			if (s_instance != null)
			{
				s_instance.enabled = true;
			}
		}
	}

	public static float resolutionChangeSpeed
	{
		get
		{
			return s_resChangeSpeed;
		}
		set
		{
			s_resChangeSpeed = Mathf.Max(value, 0f);
		}
	}

	public static void SetTargetResolution(TargetResolution targetResolution)
	{
		if (targetResolution == TargetResolution.Full)
		{
			s_targetResScale = 1f;
		}
		else
		{
			s_targetResScale = (float)targetResolution / (float)Screen.height;
		}
		if (s_instance != null)
		{
			s_instance.enabled = true;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
	}

	private static void OnActiveSceneChanged(Scene prevScene, Scene newScene)
	{
		s_currentResScale = 1f;
		s_targetResScale = 1f;
		s_resChangeSpeed = 1f / 6f;
		if (s_active)
		{
			ScalableBufferManager.ResizeBuffers(1f, 1f);
		}
	}

	private void Update()
	{
		s_currentResScale = Mathf.MoveTowards(s_currentResScale, s_targetResScale, s_resChangeSpeed * Time.unscaledDeltaTime);
		if (Mathf.Approximately(s_currentResScale, s_targetResScale))
		{
			s_currentResScale = s_targetResScale;
			base.enabled = false;
		}
		if (s_active)
		{
			ScalableBufferManager.ResizeBuffers(s_currentResScale, s_currentResScale);
		}
	}
}
