using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class TextTranslation : MonoBehaviour, IPermanentManagerWorker
{
	public delegate void LanguageChanged();

	public enum Language
	{
		UNKNOWN = -1,
		ENGLISH = 0,
		SPANISH_LA = 1,
		GERMAN = 2,
		FRENCH = 3,
		ITALIAN = 4,
		POLISH = 5,
		PORTUGUESE_BR = 6,
		JAPANESE = 7,
		RUSSIAN = 8,
		CHINESE_SIMPLE = 9,
		KOREAN = 10,
		TURKISH = 11,
		TOTAL = 12
	}

	[Serializable]
	public class TranslationTableEntry
	{
		[XmlElement("key")]
		public string key;

		[XmlElement("value")]
		public string value;

		public TranslationTableEntry()
		{
		}

		public TranslationTableEntry(string _key, string _value)
		{
			key = _key;
			value = _value;
		}
	}

	[Serializable]
	public class TranslationTableEntryUI
	{
		[XmlElement("key")]
		public int key;

		[XmlElement("value")]
		public string value;

		public TranslationTableEntryUI()
		{
		}

		public TranslationTableEntryUI(int _key, string _value)
		{
			key = _key;
			value = _value;
		}
	}

	[Serializable]
	[XmlRoot("TranslationTable_XML")]
	public class TranslationTable_XML
	{
		[XmlElement("entry")]
		public List<TranslationTableEntry> table;

		public List<TranslationTableEntry> table_shipLog;

		public List<TranslationTableEntryUI> table_ui;

		public TranslationTable_XML()
		{
			table = new List<TranslationTableEntry>();
			table_shipLog = new List<TranslationTableEntry>();
			table_ui = new List<TranslationTableEntryUI>();
		}

		public TranslationTable_XML(TranslationTable _table)
		{
			table = new List<TranslationTableEntry>();
			table_shipLog = new List<TranslationTableEntry>();
			table_ui = new List<TranslationTableEntryUI>();
			foreach (KeyValuePair<string, string> item4 in _table.theTable)
			{
				TranslationTableEntry item = new TranslationTableEntry
				{
					key = item4.Key,
					value = item4.Value
				};
				table.Add(item);
			}
			foreach (KeyValuePair<string, string> item5 in _table.theShipLogTable)
			{
				TranslationTableEntry item2 = new TranslationTableEntry
				{
					key = item5.Key,
					value = item5.Value
				};
				table_shipLog.Add(item2);
			}
			foreach (KeyValuePair<int, string> item6 in _table.theUITable)
			{
				TranslationTableEntryUI item3 = new TranslationTableEntryUI
				{
					key = item6.Key,
					value = item6.Value
				};
				table_ui.Add(item3);
			}
		}
	}

	public class TranslationTable
	{
		public Dictionary<string, string> theTable;

		public Dictionary<string, string> theShipLogTable;

		public Dictionary<int, string> theUITable;

		public TranslationTable()
		{
			theTable = new Dictionary<string, string>();
			theShipLogTable = new Dictionary<string, string>();
			theUITable = new Dictionary<int, string>();
		}

		public TranslationTable(TranslationTable_XML xml)
		{
			theTable = new Dictionary<string, string>();
			for (int i = 0; i < xml.table.Count; i++)
			{
				theTable[xml.table[i].key] = xml.table[i].value;
			}
			theShipLogTable = new Dictionary<string, string>();
			for (int j = 0; j < xml.table_shipLog.Count; j++)
			{
				theShipLogTable[xml.table_shipLog[j].key] = xml.table_shipLog[j].value;
			}
			theUITable = new Dictionary<int, string>();
			for (int k = 0; k < xml.table_ui.Count; k++)
			{
				theUITable[xml.table_ui[k].key] = xml.table_ui[k].value;
			}
		}

		public void Insert(string key, string value)
		{
			key = key.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "")
				.Replace("]]>", "");
			if (value == null)
			{
				Debug.LogError("why is this null?");
			}
			else
			{
				value = value.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "")
					.Replace("]]>", "");
			}
			if (theTable.ContainsKey(key) && theTable[key] != value)
			{
				Debug.LogError("Duplicate key strings found! Key string \"" + key + "\"");
				Debug.LogError("previous translation \"" + theTable[key] + "\" new  translation \"" + value + "\"");
			}
			else
			{
				theTable[key] = value;
			}
		}

		public void InsertShipLog(string key, string value)
		{
			key = key.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "")
				.Replace("]]>", "");
			if (value == null)
			{
				Debug.LogError("why is this null?");
			}
			else
			{
				value = value.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "")
					.Replace("]]>", "");
			}
			if (theShipLogTable.ContainsKey(key) && theShipLogTable[key] != value)
			{
				Debug.LogError("Duplicate key strings found! Key string \"" + key + "\"");
				Debug.LogError("previous translation \"" + theShipLogTable[key] + "\" new  translation \"" + value + "\"");
			}
			else
			{
				theShipLogTable[key] = value;
			}
		}

		public string Get(string key)
		{
			if (theTable.ContainsKey(key))
			{
				return theTable[key];
			}
			return null;
		}

		public string GetShipLog(string key)
		{
			if (theShipLogTable.ContainsKey(key))
			{
				return theShipLogTable[key];
			}
			return null;
		}

		public bool IsTranslated(string key)
		{
			key = key.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "")
				.Replace("]]>", "");
			return theTable.ContainsKey(key);
		}

		public bool IsTranslatedShipLog(string key)
		{
			key = key.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "")
				.Replace("]]>", "");
			return theShipLogTable.ContainsKey(key);
		}

		public void Insert_UI(int key, string value)
		{
			if (theUITable.ContainsKey(key) && theUITable[key] != value)
			{
				Debug.LogError("Duplicate key IDs found!");
				Debug.LogError("Key ID \"" + key + "\"");
				Debug.LogError("previous translation \"" + theUITable[key] + "\"");
				Debug.LogError("new  translation \"" + value + "\"");
			}
			else
			{
				theUITable[key] = value;
			}
		}

		public string Get_UI(int key)
		{
			if (theUITable.ContainsKey(key))
			{
				return theUITable[key];
			}
			return null;
		}

		public bool IsTranslated_UI(int key)
		{
			return theUITable.ContainsKey(key);
		}
	}

	private struct TextInfo
	{
		public string text;

		public bool isTag;

		public UiTagType tagType;
	}

	private enum UiTagType
	{
		NONE = 0,
		REGULAR_UI_START = 1,
		REGULAR_UI_END = 2,
		LARGE_UI_START = 3,
		LARGE_UI_END = 4,
		SMALL_TEXT_START = 5,
		SMALL_TEXT_END = 6
	}

	public Font[] m_fonts;

	public Font[] m_dynamicFonts;

	public Font[] m_languageFonts;

	public float[] m_defaultSpacing;

	public float[] m_fontSizeModifier;

	public bool m_ignoreManualLineBreaks;

	private const int c_smallFontSizeDefault = 20;

	private const int c_smallFontSizeLargeUiDefault = 25;

	private const int c_smallFontSizeNonLatin = 25;

	private const int c_smallFontSizeLargeUiNonLatin = 30;

	public static readonly string[] s_langFolder = new string[12]
	{
		"ENG", "SPA", "GER", "FRE", "ITA", "POL", "POR", "JPN", "RUS", "ZHO",
		"KOR", "TUR"
	};

	private static TextTranslation s_theTable;

	private TranslationTable m_table;

	private Language m_language = Language.UNKNOWN;

	private Language m_systemLanguage = Language.UNKNOWN;

	public event LanguageChanged OnLanguageChanged;

	public void InitializeOnAwake()
	{
		if (null != s_theTable)
		{
			Debug.LogError("There should be only one TextTranslation object in the world");
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			s_theTable = this;
			InitializeLanguage();
		}
	}

	public static TextTranslation Get()
	{
		return s_theTable;
	}

	private void InitializeLanguage()
	{
		m_language = PlayerData.GetSavedLanguage();
		if (m_language == Language.UNKNOWN)
		{
			m_language = GetSystemLanguage();
		}
		SetLanguage(m_language);
	}

	private Language ConvertFromSystemLanguage()
	{
		SystemLanguage systemLanguage = Application.systemLanguage;
		switch (systemLanguage)
		{
		case SystemLanguage.English:
			return Language.ENGLISH;
		case SystemLanguage.Spanish:
			return Language.SPANISH_LA;
		case SystemLanguage.German:
			return Language.GERMAN;
		case SystemLanguage.French:
			return Language.FRENCH;
		case SystemLanguage.Italian:
			return Language.ITALIAN;
		case SystemLanguage.Polish:
			return Language.POLISH;
		case SystemLanguage.Portuguese:
			return Language.PORTUGUESE_BR;
		case SystemLanguage.Japanese:
			return Language.JAPANESE;
		case SystemLanguage.Russian:
			return Language.RUSSIAN;
		case SystemLanguage.Korean:
			return Language.KOREAN;
		case SystemLanguage.Turkish:
			return Language.TURKISH;
		case SystemLanguage.Chinese:
		case SystemLanguage.ChineseSimplified:
			return Language.CHINESE_SIMPLE;
		default:
			Debug.LogWarning("System Language unsupported: " + systemLanguage);
			return Language.UNKNOWN;
		}
	}

	public Language GetLanguage()
	{
		return m_language;
	}

	public Language GetSystemLanguage()
	{
		if (m_systemLanguage == Language.UNKNOWN)
		{
			m_systemLanguage = ConvertFromSystemLanguage();
		}
		return m_systemLanguage;
	}

	public bool IsLanguageLatin()
	{
		Language language = GetLanguage();
		if (language == Language.POLISH || (uint)(language - 7) <= 4u)
		{
			return false;
		}
		return true;
	}

	public bool IsLanguageCJK()
	{
		Language language = GetLanguage();
		if (language == Language.JAPANESE || (uint)(language - 9) <= 1u)
		{
			return true;
		}
		return false;
	}

	public void SetLanguage(Language lang)
	{
		if (lang == Language.UNKNOWN)
		{
			Debug.LogWarning("Tried to set Language to UNKNOWN; defaulting to English(US)");
			lang = Language.ENGLISH;
		}
		m_language = lang;
		m_table = null;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Translation");
		stringBuilder.Append(Path.DirectorySeparatorChar);
		stringBuilder.Append(s_langFolder[(int)m_language]);
		stringBuilder.Append(Path.DirectorySeparatorChar);
		stringBuilder.Append("Translation");
		TextAsset textAsset = Resources.Load<TextAsset>(stringBuilder.ToString());
		if (null == textAsset)
		{
			Debug.LogError("Unable to load text translation file for language " + s_langFolder[(int)m_language]);
			return;
		}
		string xml = OWUtilities.RemoveByteOrderMark(textAsset);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xml);
		XmlNode xmlNode = xmlDocument.SelectSingleNode("TranslationTable_XML");
		XmlNodeList xmlNodeList = xmlNode.SelectNodes("entry");
		TranslationTable_XML translationTable_XML = new TranslationTable_XML();
		foreach (XmlNode item in xmlNodeList)
		{
			translationTable_XML.table.Add(new TranslationTableEntry(item.SelectSingleNode("key").InnerText, item.SelectSingleNode("value").InnerText));
		}
		foreach (XmlNode item2 in xmlNode.SelectSingleNode("table_shipLog").SelectNodes("TranslationTableEntry"))
		{
			translationTable_XML.table_shipLog.Add(new TranslationTableEntry(item2.SelectSingleNode("key").InnerText, item2.SelectSingleNode("value").InnerText));
		}
		foreach (XmlNode item3 in xmlNode.SelectSingleNode("table_ui").SelectNodes("TranslationTableEntryUI"))
		{
			translationTable_XML.table_ui.Add(new TranslationTableEntryUI(int.Parse(item3.SelectSingleNode("key").InnerText), item3.SelectSingleNode("value").InnerText));
		}
		m_table = new TranslationTable(translationTable_XML);
		Resources.UnloadAsset(textAsset);
		if (this.OnLanguageChanged != null)
		{
			this.OnLanguageChanged();
		}
	}

	public static string Translate(string key)
	{
		if (s_theTable != null)
		{
			return s_theTable._Translate(key);
		}
		Debug.LogError("ERROR! TextTable Not Loaded Yet!");
		return key;
	}

	public static string Translate_ShipLog(string key)
	{
		if (s_theTable != null)
		{
			return s_theTable._Translate_ShipLog(key);
		}
		Debug.LogError("ERROR! TextTable Not Loaded Yet!");
		return key;
	}

	public static string Translate_UI(int key)
	{
		string text;
		if (s_theTable != null)
		{
			text = s_theTable._Translate_UI(key);
		}
		else
		{
			Debug.LogError("ERROR! TextTable Not Loaded Yet!");
			text = key.ToString();
		}
		return text.Replace("\\\\n", "\n");
	}

	public static Font GetFont(bool dynamicFont = false)
	{
		if (s_theTable != null)
		{
			int num = (int)s_theTable.m_language;
			if (num < 0)
			{
				num = 0;
			}
			if (dynamicFont)
			{
				return s_theTable.m_dynamicFonts[num];
			}
			return s_theTable.m_fonts[num];
		}
		return null;
	}

	public static Font GetLanguageFont()
	{
		if (s_theTable != null)
		{
			int num = (int)s_theTable.m_language;
			if (num < 0)
			{
				num = 0;
			}
			return s_theTable.m_languageFonts[num];
		}
		return null;
	}

	public static float GetDefaultFontSpacing()
	{
		if (s_theTable != null)
		{
			int num = (int)s_theTable.m_language;
			if (num < 0)
			{
				num = 0;
			}
			return s_theTable.m_defaultSpacing[num];
		}
		return 1f;
	}

	public static float GetFontSizeModifier()
	{
		if (s_theTable != null)
		{
			int num = (int)s_theTable.m_language;
			if (num < 0)
			{
				num = 0;
			}
			return s_theTable.m_fontSizeModifier[num];
		}
		return 1f;
	}

	public static int GetModifiedFontSize(int originalFontSize)
	{
		return Mathf.CeilToInt((float)originalFontSize * GetFontSizeModifier());
	}

	private string _Translate(string key)
	{
		if (m_table == null)
		{
			Debug.LogError("TextTranslation not initialized");
			return key;
		}
		string text = m_table.Get(key);
		if (text == null)
		{
			Debug.LogError("String \"" + key + "\" not found in table for language " + s_langFolder[(int)m_language]);
			return key;
		}
		text = ProcessCustomWhitespace(text);
		return ProcessSizeDependentText(text);
	}

	private string _Translate_ShipLog(string key)
	{
		if (m_table == null)
		{
			Debug.LogError("TextTranslation not initialized");
			return key;
		}
		string shipLog = m_table.GetShipLog(key);
		if (shipLog == null)
		{
			Debug.LogError("String \"" + key + "\" not found in ShipLog table for language " + s_langFolder[(int)m_language]);
			return key;
		}
		shipLog = ProcessCustomWhitespace(shipLog);
		return ProcessSizeDependentText(shipLog);
	}

	private string _Translate_UI(int key)
	{
		if (m_table == null)
		{
			Debug.LogError("TextTranslation not initialized");
			return key.ToString();
		}
		string text = m_table.Get_UI(key);
		if (text == null)
		{
			Debug.LogWarning("UI String #" + key + " not found in table for language " + s_langFolder[(int)m_language]);
			return key.ToString();
		}
		text = ProcessCustomWhitespace(text);
		return ProcessSizeDependentText(text);
	}

	private string ProcessCustomWhitespace(string inputString)
	{
		string text = inputString;
		Language language = GetLanguage();
		string newValue = " ";
		if (language == Language.CHINESE_SIMPLE || language == Language.JAPANESE)
		{
			newValue = "";
		}
		if (m_ignoreManualLineBreaks)
		{
			text = text.Replace("\\\\N\\\\n", newValue);
			text = text.Replace("\\\\n\\\\N", newValue);
			text = text.Replace("\\\\n", newValue);
			text = text.Replace("\\\\N", newValue);
		}
		else if (PlayerData.IsUILargeTextSize())
		{
			text = text.Replace("\\\\N\\\\n", "\n");
			text = text.Replace("\\\\n\\\\N", "\n");
			text = text.Replace("\\\\N", "\n");
			text = text.Replace("\\\\n", newValue);
		}
		else
		{
			text = text.Replace("\\\\N\\\\n", "\n");
			text = text.Replace("\\\\n\\\\N", "\n");
			text = text.Replace("\\\\n", "\n");
			text = text.Replace("\\\\N", newValue);
		}
		if (m_language == Language.KOREAN)
		{
			text = text.Replace(" ", "  ");
		}
		return text;
	}

	private string ProcessSizeDependentText(string inputString)
	{
		List<TextInfo> list = new List<TextInfo>();
		List<string> list2 = new List<string>();
		string[] array = new string[2] { "<", ">" };
		int[] array2 = Enumerable.ToArray(Enumerable.Select(array, (string d) => inputString.IndexOf(d)));
		int num = 0;
		int num2;
		do
		{
			num2 = int.MaxValue;
			string text = null;
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i] != -1 && array2[i] < num2)
				{
					num2 = array2[i];
					text = array[i];
				}
			}
			if (num2 != int.MaxValue)
			{
				string item = inputString.Substring(num, num2 - num);
				num = num2 + text.Length;
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j] != -1 && array2[j] < num)
					{
						array2[j] = inputString.IndexOf(array[j], num);
					}
				}
				list2.Add(item);
				list2.Add(text);
			}
			else
			{
				string item = inputString.Substring(num);
				list2.Add(item);
			}
		}
		while (num2 != int.MaxValue);
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		for (int k = 0; k < list2.Count; k++)
		{
			if (flag)
			{
				stringBuilder.Append(list2[k]);
				if (list2[k] == ">")
				{
					flag = false;
					TextInfo item2 = default(TextInfo);
					string text2 = (item2.text = stringBuilder.ToString());
					item2.isTag = true;
					text2 = text2.TrimStart('<');
					switch (text2.TrimEnd('>'))
					{
					case "RegularUi":
						item2.tagType = UiTagType.REGULAR_UI_START;
						break;
					case "/RegularUi":
						item2.tagType = UiTagType.REGULAR_UI_END;
						break;
					case "LargeUi":
						item2.tagType = UiTagType.LARGE_UI_START;
						break;
					case "/LargeUi":
						item2.tagType = UiTagType.LARGE_UI_END;
						break;
					case "SmallText":
						item2.tagType = UiTagType.SMALL_TEXT_START;
						break;
					case "/SmallText":
						item2.tagType = UiTagType.SMALL_TEXT_END;
						break;
					default:
						item2.isTag = false;
						item2.tagType = UiTagType.NONE;
						break;
					}
					list.Add(item2);
				}
			}
			else if (list2[k] == "<")
			{
				stringBuilder.Length = 0;
				flag = true;
				stringBuilder.Append(list2[k]);
			}
			else
			{
				TextInfo item2 = default(TextInfo);
				item2.text = list2[k];
				item2.isTag = false;
				list.Add(item2);
			}
		}
		if (flag)
		{
			Debug.LogError("Tag not closed. Use &lt; and &gt; instead of < and > if necessary");
		}
		stringBuilder.Length = 0;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = PlayerData.IsUILargeTextSize();
		foreach (TextInfo item3 in list)
		{
			if (item3.isTag)
			{
				switch (item3.tagType)
				{
				case UiTagType.LARGE_UI_START:
					flag2 = true;
					break;
				case UiTagType.LARGE_UI_END:
					flag2 = false;
					break;
				case UiTagType.REGULAR_UI_START:
					flag3 = true;
					break;
				case UiTagType.REGULAR_UI_END:
					flag3 = false;
					break;
				case UiTagType.SMALL_TEXT_START:
				{
					string text3 = "<size=";
					if (IsLanguageLatin())
					{
						text3 += (flag4 ? 25 : 20);
						text3 += ">";
					}
					else
					{
						text3 += (flag4 ? 30 : 25);
						text3 += ">";
					}
					stringBuilder.Append(text3);
					break;
				}
				case UiTagType.SMALL_TEXT_END:
					stringBuilder.Append("</size>");
					break;
				default:
					Debug.LogWarning("TextTranslation tag parsing error");
					break;
				}
			}
			else
			{
				if (!flag2 && !flag3)
				{
					stringBuilder.Append(item3.text);
				}
				if (flag2 && flag4)
				{
					stringBuilder.Append(item3.text);
				}
				else if (flag3 && !flag4)
				{
					stringBuilder.Append(item3.text);
				}
			}
		}
		if (flag2 || flag3)
		{
			Debug.LogWarning("TextTranslation tag parsing error, tag not closed");
		}
		return stringBuilder.ToString();
	}
}
