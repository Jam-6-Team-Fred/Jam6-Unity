using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OWUtilities
{
	public static IFormatProvider owFormatProvider = CultureInfo.CreateSpecificCulture("en-US");

	public static void Assert(bool condition, string message, UnityEngine.Object obj = null)
	{
		if (!condition)
		{
			Debug.LogError(message, obj);
			Debug.Break();
		}
	}

	public static CuriosityName CuriosityStringToName(string nameString)
	{
		if (nameString.Equals("VESSEL"))
		{
			return CuriosityName.Vessel;
		}
		if (nameString.Equals("TIME_LOOP"))
		{
			return CuriosityName.TimeLoop;
		}
		if (nameString.Equals("SUNKEN_MODULE"))
		{
			return CuriosityName.SunkenModule;
		}
		if (nameString.Equals("QUANTUM_MOON"))
		{
			return CuriosityName.QuantumMoon;
		}
		if (nameString.Equals("COMET_CORE"))
		{
			return CuriosityName.CometCore;
		}
		if (nameString.Equals("INVISIBLE_PLANET"))
		{
			return CuriosityName.InvisiblePlanet;
		}
		return CuriosityName.None;
	}

	public static string GetCuriosityTextColor(CuriosityName curiosity)
	{
		switch (curiosity)
		{
		case CuriosityName.Vessel:
			return "<color=#80CC80>";
		case CuriosityName.SunkenModule:
			return "<color=#66FFFF>";
		case CuriosityName.TimeLoop:
			return "<color=#FFCC66>";
		case CuriosityName.QuantumMoon:
			return "<color=#9999FF>";
		case CuriosityName.CometCore:
			return "<color=#0000FF>";
		default:
			return "<color=white>";
		}
	}

	public static string RemoveByteOrderMark(TextAsset textAsset)
	{
		byte[] bytes = textAsset.bytes;
		byte[] preamble = Encoding.UTF8.GetPreamble();
		byte[] preamble2 = Encoding.Unicode.GetPreamble();
		byte[] preamble3 = Encoding.BigEndianUnicode.GetPreamble();
		if (bytes.StartsWith(preamble))
		{
			return Encoding.UTF8.GetString(bytes, preamble.Length, bytes.Length - preamble.Length);
		}
		if (bytes.StartsWith(preamble2))
		{
			return Encoding.Unicode.GetString(bytes, preamble2.Length, bytes.Length - preamble2.Length);
		}
		if (bytes.StartsWith(preamble3))
		{
			return Encoding.BigEndianUnicode.GetString(bytes, preamble3.Length, bytes.Length - preamble3.Length);
		}
		return textAsset.text;
	}

	public static string CleanupXmlText(string text, bool replaceBlackslashN = true)
	{
		string text2 = text;
		text2 = text2.Trim();
		if (replaceBlackslashN)
		{
			text2 = text2.Replace("\\\\n", "\n");
		}
		return text2;
	}

	public static int IncrementIndex(int currentIndex, int minIndex, int maxIndex, int increment = 1, bool loop = true)
	{
		int num = currentIndex + increment;
		if (loop)
		{
			num = ((num < minIndex) ? maxIndex : num);
			return (num > maxIndex) ? minIndex : num;
		}
		return Mathf.Clamp(num, minIndex, maxIndex);
	}

	public static bool RunningOnMac()
	{
		if (Application.platform != 0)
		{
			return Application.platform == RuntimePlatform.OSXPlayer;
		}
		return true;
	}

	public static Quaternion GetWobbleRotation(float rate = 0.4f, float amount = 0.04f)
	{
		float x = (0.5f - Mathf.PerlinNoise(Time.time * rate, 0f)) * 2f;
		float y = (0.5f - Mathf.PerlinNoise(Time.time * rate, 10f)) * 2f;
		Vector3 vector = new Vector3(x, y, 0f) * amount;
		return Quaternion.LookRotation(Vector3.forward + vector, Vector3.up);
	}

	public static Font GetDialogueFont()
	{
		return (Font)Resources.Load("Fonts/GillSansMT/Gill Sans MT");
	}

	public static Font GetHelmetFont()
	{
		return (Font)Resources.Load("Fonts/Digital7/digital-7");
	}

	public static GUIStyle GetHelmetGUIStyle(int fontSize = 30, Color fontColor = default(Color))
	{
		GUIStyle obj = new GUIStyle
		{
			font = GetHelmetFont(),
			fontSize = fontSize
		};
		if (fontColor == default(Color))
		{
			fontColor = new ColorHSV(42f, 0.2f, 0.9f).ToColorRGB();
		}
		obj.normal.textColor = fontColor;
		return obj;
	}

	public static Color GetDefaultColor()
	{
		return new ColorHSV(42f, 0.2f, 0.9f).ToColorRGB();
	}

	public static GUIStyle GetPromptGUIStyle(int fontSize = 30, Color fontColor = default(Color))
	{
		GUIStyle obj = new GUIStyle
		{
			font = GetDialogueFont(),
			fontSize = fontSize
		};
		if (fontColor == default(Color))
		{
			fontColor = new ColorHSV(42f, 0.2f, 0.9f).ToColorRGB();
		}
		obj.normal.textColor = fontColor;
		return obj;
	}

	public static GUIStyle GetPromptGUIStyleCharacterName(int fontSize = 30, Color fontColor = default(Color))
	{
		GUIStyle obj = new GUIStyle
		{
			font = (Font)Resources.Load("Fonts/Gill Sans MT Bold"),
			fontSize = fontSize
		};
		if (fontColor == default(Color))
		{
			fontColor = new ColorHSV(42f, 0.2f, 0.9f).ToColorRGB();
		}
		obj.normal.textColor = fontColor;
		return obj;
	}

	public static Vector2 GetScreenCenterPos()
	{
		return new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
	}

	public static Vector2 EncodeFloatRG(float v)
	{
		Vector2 vector = new Vector2(1f, 255f);
		float num = 0.003921569f;
		Vector2 result = vector * v;
		result.x = OWMath.Frac(result.x);
		result.y = OWMath.Frac(result.y);
		result.x -= result.y * num;
		return result;
	}

	public static float DecodeFloatRG(Vector2 enc)
	{
		Vector2 rhs = new Vector2(1f, 0.003921569f);
		return Vector2.Dot(enc, rhs);
	}

	public static AudioType[] GetAudioTypesFromCategory(AudioTypeCategory category)
	{
		Array values = Enum.GetValues(typeof(AudioTypeCategory));
		Array values2 = Enum.GetValues(typeof(AudioType));
		int num = int.MaxValue;
		for (int num2 = values.Length - 1; num2 > -1; num2--)
		{
			if (num2 != values.Length - 1)
			{
				num = (int)values.GetValue(num2 + 1);
			}
			if ((AudioTypeCategory)values.GetValue(num2) == category)
			{
				break;
			}
		}
		int num3 = 0;
		for (int i = 0; i < values2.Length; i++)
		{
			int num4 = (int)values2.GetValue(i);
			if (num4 >= (int)category && num4 < num)
			{
				num3++;
			}
			if (num4 >= num)
			{
				break;
			}
		}
		AudioType[] array = new AudioType[num3];
		int num5 = 0;
		int num6 = 0;
		for (int j = 0; j < num3; j++)
		{
			num6 = (int)(category + num5 + j);
			if (Enum.IsDefined(typeof(AudioType), num6))
			{
				array[j] = (AudioType)num6;
				continue;
			}
			while (!Enum.IsDefined(typeof(AudioType), num6) && num6 < num)
			{
				num5++;
				num6 = (int)(category + num5 + j);
			}
			if (!Enum.IsDefined(typeof(AudioType), num6))
			{
				Debug.LogError("Something went wrong. Unable To Parse AudioType list for category");
			}
			else
			{
				array[j] = (AudioType)num6;
			}
		}
		return array;
	}

	public static AudioTypeCategory GetAudioTypeCategoryFromAudioType(AudioType type)
	{
		Array values = Enum.GetValues(typeof(AudioTypeCategory));
		AudioTypeCategory audioTypeCategory = AudioTypeCategory.None;
		for (int num = values.Length - 1; num > -1; num--)
		{
			audioTypeCategory = (AudioTypeCategory)values.GetValue(num);
			if ((int)type >= (int)audioTypeCategory)
			{
				break;
			}
		}
		return audioTypeCategory;
	}

	public static Vector3 GetShipThrusterFilter(Thruster shipThrusterID)
	{
		switch (shipThrusterID)
		{
		case Thruster.Down_LeftThruster:
		case Thruster.Down_RightThruster:
			return Vector3.down;
		case Thruster.Forward_LeftThruster:
		case Thruster.Forward_RightThruster:
			return Vector3.forward;
		case Thruster.Backward_LeftThruster:
		case Thruster.Backward_RightThruster:
			return Vector3.back;
		case Thruster.Right_Thruster:
			return Vector3.right;
		case Thruster.Left_Thruster:
			return Vector3.left;
		case Thruster.Up_LeftThruster:
		case Thruster.Up_RightThruster:
			return Vector3.up;
		default:
			return Vector3.zero;
		}
	}

	public static ThrusterBank GetShipThrusterBank(Thruster thruster)
	{
		switch (thruster)
		{
		case Thruster.Down_LeftThruster:
		case Thruster.Forward_LeftThruster:
		case Thruster.Right_Thruster:
		case Thruster.Backward_LeftThruster:
		case Thruster.Up_LeftThruster:
			return ThrusterBank.Left;
		case Thruster.Down_RightThruster:
		case Thruster.Forward_RightThruster:
		case Thruster.Left_Thruster:
		case Thruster.Backward_RightThruster:
		case Thruster.Up_RightThruster:
			return ThrusterBank.Right;
		default:
			return ThrusterBank.Undefined;
		}
	}

	public static string GetSceneAssetDirectory()
	{
		return "";
	}

	public static string SaveSceneAsset(string filename, UnityEngine.Object objToSave, string assetGuidToReplace = "")
	{
		return "";
	}

	public static bool DestroySceneAsset(string filename)
	{
		return false;
	}

	public static bool DestroySceneAsset(UnityEngine.Object objToDestroy)
	{
		return false;
	}

	public static T[] FindAllObjectsOfType<T>(Scene scene) where T : UnityEngine.Object
	{
		List<T> list = new List<T>();
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			list.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive: true));
		}
		return list.ToArray();
	}

	public static T[] FindAllObjectsOfType<T>() where T : UnityEngine.Object
	{
		return FindAllObjectsOfType<T>(SceneManager.GetActiveScene());
	}

	public static void TakeScreenshot(int resolutionScale = 1)
	{
		string text = Application.persistentDataPath + "/Screenshots/";
		string text2 = DateTime.Now.ToString("yy-MM-dd_hh-mm-ss");
		string text3 = "OuterWilds_" + text2 + ".png";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		int num = 1;
		while (File.Exists(text + text3))
		{
			text3 = "OuterWilds_" + text2 + "_" + num + ".png";
			num++;
		}
		ScreenCapture.CaptureScreenshot(text + text3, resolutionScale);
		Debug.Log("Screenshot saved to " + text + text3);
	}
}
