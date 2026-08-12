using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SecretSettings
{
	private static Dictionary<string, string> s_settings;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void TryLoadSettingsFromFile()
	{
		try
		{
			string path = Path.Combine(Application.persistentDataPath, "secretsettings.txt");
			if (!File.Exists(path))
			{
				try
				{
					TextAsset textAsset = Resources.Load<TextAsset>("secretsettings_default");
					if (textAsset == null)
					{
						throw new FileNotFoundException("Failed to find default settings file in project.", "secretsettings_default.txt");
					}
					File.WriteAllBytes(path, textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError("Failed to write default secret settings file!\n" + ex.ToString());
					return;
				}
			}
			string[] array = File.ReadAllLines(path);
			s_settings = new Dictionary<string, string>(array.Length, StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < array.Length; i++)
			{
				try
				{
					int num = array[i].IndexOf("//");
					if (num == 0)
					{
						continue;
					}
					if (num > -1)
					{
						array[i] = array[i].Substring(0, num);
					}
					int num2 = array[i].IndexOf("=");
					if (num2 >= 0 && num2 < array[i].Length - 1)
					{
						string text = array[i].Substring(0, num2).Trim();
						string text2 = array[i].Substring(num2 + 1).Trim();
						if (text.Length != 0 && text2.Length != 0)
						{
							s_settings[text] = text2;
						}
					}
				}
				catch (Exception ex2)
				{
					Debug.LogWarning("Failed to read line " + (i + 1) + " in secret settings file!\n" + ex2.ToString());
				}
			}
		}
		catch (Exception ex3)
		{
			Debug.LogError("Failed to read secret settings file!\n" + ex3.ToString());
		}
	}

	public static bool SettingExists(string settingName)
	{
		return s_settings.ContainsKey(settingName);
	}

	public static bool TryGetString(string settingName, out string value)
	{
		value = "";
		if (s_settings != null && s_settings.TryGetValue(settingName, out value))
		{
			return true;
		}
		return false;
	}

	public static bool TryGetBool(string settingName, out bool value)
	{
		value = false;
		if (s_settings != null && s_settings.TryGetValue(settingName, out var value2) && bool.TryParse(value2, out value))
		{
			return true;
		}
		return false;
	}

	public static bool TryGetInt(string settingName, out int value)
	{
		value = 0;
		if (s_settings != null && s_settings.TryGetValue(settingName, out var value2) && int.TryParse(value2, out value))
		{
			return true;
		}
		return false;
	}

	public static bool TryGetFloat(string settingName, out float value)
	{
		value = 0f;
		if (s_settings != null && s_settings.TryGetValue(settingName, out var value2) && float.TryParse(value2, out value))
		{
			return true;
		}
		return false;
	}
}
