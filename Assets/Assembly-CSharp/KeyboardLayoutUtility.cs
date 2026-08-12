using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class KeyboardLayoutUtility
{
	public enum KeyboardLayout
	{
		UNDEFINED = 0,
		AZERTY = 1,
		QWERTY = 2,
		QWERTZ = 3,
		DVORAK = 4
	}

	private enum MicrosoftKeyboardLayout
	{
		ALBANIAN = 1052,
		ARABIC_101 = 1025,
		ARABIC_102 = 66561,
		ARABIC_102_AZERTY = 132097,
		ARMENIAN_EASTERN = 1067,
		ARMENIAN_PHONETIC = 132139,
		ARMENIAN_TYPEWRITER = 197675,
		ARMENIAN_WESTERN = 66603,
		ASSAMESE__INSCRIPT = 1101,
		AZERBAIJANI_STANDARD = 66604,
		AZERBAIJANI_CYRILLIC = 2092,
		AZERBAIJANI_LATIN = 1068,
		BASHKIR = 1133,
		BELARUSIAN = 1059,
		BELGIAN_COMMA = 67596,
		BELGIAN_PERIOD = 2067,
		BELGIAN_FRENCH = 2060,
		BANGLA_BANGLADESH = 1093,
		BANGLA_INDIA = 132165,
		BANGLA_INDIA__LEGACY = 66629,
		BOSNIAN_CYRILLIC = 8218,
		BUGINESE = 723968,
		BULGARIAN = 197634,
		BULGARIAN_LATIN = 66562,
		BULGARIAN_PHONETIC_LAYOUT = 132098,
		BULGARIAN_PHONETIC_TRADITIONAL = 263170,
		BULGARIAN_TYPEWRITER = 1026,
		CANADIAN_FRENCH = 4105,
		CANADIAN_FRENCH_LEGACY = 3084,
		CANADIAN_MULTILINGUAL_STANDARD = 69641,
		CENTRAL_ATLAS_TAMAZIGHT = 2143,
		CENTRAL_KURDISH = 1065,
		CHEROKEE_NATION = 1116,
		CHEROKEE_NATION_PHONETIC = 66652,
		CHINESE_SIMPLIFIED_US_KEYBOARD = 2052,
		CHINESE_TRADITIONAL_US_KEYBOARD = 1028,
		CHINESE_TRADITIONAL_HONG_KONG_SAR = 3076,
		CHINESE_TRADITIONAL_MACAO_SAR_US_KEYBOARD = 5124,
		CHINESE_SIMPLIFIED_SINGAPORE_US_KEYBOARD = 4100,
		CROATIAN = 1050,
		CZECH = 1029,
		CZECH_QWERTY = 66565,
		CZECH_PROGRAMMERS = 132101,
		DANISH = 1030,
		DEVANAGARIINSCRIPT = 1081,
		DIVEHI_PHONETIC = 1125,
		DIVEHI_TYPEWRITER = 66661,
		DUTCH = 1043,
		DZONGKHA = 3153,
		ESTONIAN = 1061,
		FAEROESE = 1080,
		FINNISH = 1035,
		FINNISH_WITH_SAMI = 67643,
		FRENCH = 1036,
		FUTHARK = 1182720,
		GEORGIAN = 1079,
		GEORGIAN_ERGONOMIC = 132151,
		GEORGIAN_QWERTY = 66615,
		GEORGIAN_MINISTRY_OF_EDUCATION_AND_SCIENCE_SCHOOLS = 197687,
		GEORGIAN_OLD_ALPHABETS = 263223,
		GERMAN = 1031,
		GERMAN_IBM = 66567,
		GOTHIC = 789504,
		GREEK = 1032,
		GREEK_220 = 66568,
		GREEK_220_LATIN = 197640,
		GREEK_319 = 132104,
		GREEK_319_LATIN = 263176,
		GREEK_LATIN = 328712,
		GREEK_POLYTONIC = 394248,
		GREENLANDIC = 1135,
		GUARANI = 1140,
		GUJARATI = 1095,
		HAUSA = 1128,
		HEBREW = 1037,
		HINDI_TRADITIONAL = 66617,
		HUNGARIAN = 1038,
		HUNGARIAN_101KEY = 66574,
		ICELANDIC = 1039,
		IGBO = 1136,
		INDIA = 16393,
		INUKTITUT__LATIN = 2141,
		INUKTITUT__NAQITTAUT = 66653,
		IRISH = 6153,
		ITALIAN = 1040,
		ITALIAN_142 = 66576,
		JAPANESE = 1041,
		JAVANESE = 1117184,
		KANNADA = 1099,
		KAZAKH = 1087,
		KHMER = 1107,
		KHMER_NIDA = 66643,
		KOREAN = 1042,
		KYRGYZ_CYRILLIC = 1088,
		LAO = 1108,
		LATIN_AMERICAN = 2058,
		LATVIAN_STANDARD = 132134,
		LATVIAN_LEGACY = 66598,
		LISU_BASIC = 461824,
		LISU_STANDARD = 527360,
		LITHUANIAN = 66599,
		LITHUANIAN_IBM = 1063,
		LITHUANIAN_STANDARD = 132135,
		LUXEMBOURGISH = 1134,
		MACEDONIA_FYROM = 1071,
		MACEDONIA_FYROM__STANDARD = 66607,
		MALAYALAM = 1100,
		MALTESE_47KEY = 1082,
		MALTESE_48KEY = 66618,
		MAORI = 1153,
		MARATHI = 1102,
		MONGOLIAN_MONGOLIAN_SCRIPT_LEGACY = 2128,
		MONGOLIAN_MONGOLIAN_SCRIPT_STANDARD = 133200,
		MONGOLIAN_CYRILLIC = 1104,
		MYANMAR = 68608,
		NKO = 592896,
		NEPALI = 1121,
		NEW_TAI_LUE = 134144,
		NORWEGIAN = 1044,
		NORWEGIAN_WITH_SAMI = 1083,
		ODIA = 1096,
		OL_CHIKI = 855040,
		OLD_ITALIC = 986112,
		OSMANYA = 920576,
		PASHTO_AFGHANISTAN = 1123,
		PERSIAN = 1065,
		PERSIAN_STANDARD = 328745,
		PHAGSPA = 658432,
		POLISH_214 = 66581,
		POLISH_PROGRAMMERS = 1045,
		PORTUGUESE = 2070,
		PORTUGUESE_BRAZILIAN_ABNT = 1046,
		PORTUGUESE_BRAZILIAN_ABNT2 = 66582,
		PUNJABI = 1094,
		ROMANIAN_LEGACY = 1048,
		ROMANIAN_PROGRAMMERS = 132120,
		ROMANIAN_STANDARD = 66584,
		RUSSIAN = 1049,
		RUSSIAN_MNEMONIC = 132121,
		RUSSIAN_TYPEWRITER = 66585,
		SAKHA = 1157,
		SAMI_EXTENDED_FINLANDSWEDEN = 133179,
		SAMI_EXTENDED_NORWAY = 66619,
		SCOTTISH_GAELIC = 71689,
		SERBIAN_CYRILLIC = 3098,
		SERBIAN_LATIN = 2074,
		SESOTHO_SA_LEBOA = 1132,
		SETSWANA = 1074,
		SINHALA = 1115,
		SINHALA__WIJ_9 = 66651,
		SLOVAK = 1051,
		SLOVAK_QWERTY = 66587,
		SLOVENIAN = 1060,
		SORA = 1051648,
		SORBIAN_EXTENDED = 66606,
		SORBIAN_STANDARD = 132142,
		SORBIAN_STANDARD_LEGACY = 1070,
		SPANISH = 1034,
		SPANISH_VARIATION = 66570,
		SWEDISH = 1053,
		SWEDISH_WITH_SAMI = 2107,
		SWISS_FRENCH = 4108,
		SWISS_GERMAN = 2055,
		SYRIAC = 1114,
		SYRIAC_PHONETIC = 66650,
		TAI_LE = 199680,
		TAJIK = 1064,
		TAMIL = 1097,
		TATAR = 66628,
		TATAR_LEGACY = 1092,
		TELUGU = 1098,
		THAI_KEDMANEE = 1054,
		THAI_KEDMANEE_NONSHIFTLOCK = 132126,
		THAI_PATTACHOTE = 66590,
		THAI_PATTACHOTE_NONSHIFTLOCK = 197662,
		TIBETAN_PRC__STANDARD = 66641,
		TIBETAN_PRC__LEGACY = 1105,
		TIFINAGH_BASIC = 330752,
		TIFINAGH_FULL = 396288,
		TURKISH_F = 66591,
		TURKISH_Q = 1055,
		TURKMEN = 1090,
		UYGHUR_1 = 66568,
		UYGHUR_LEGACY = 1152,
		UKRAINIAN = 1058,
		UKRAINIAN_ENHANCED = 132130,
		UNITED_KINGDOM = 2057,
		UNITED_KINGDOM_EXTENDED = 1106,
		UNITED_STATES_DVORAK = 66569,
		UNITED_STATES_INTERNATIONAL = 132105,
		UNITED_STATES_DVORAK_FOR_LEFT_HAND = 197641,
		UNITED_STATES_DVORAK_FOR_RIGHT_HAND = 263177,
		UNITED_STATES_ENGLISH = 1033,
		URDU = 1056,
		UYGHUR_2 = 66688,
		UZBEK_CYRILLIC = 2115,
		VIETNAMESE = 1066,
		WOLOF = 1160,
		YAKUT = 1157,
		YORUBA = 1130
	}

	private class ExternalDllWin32
	{
		[DllImport("user32.dll")]
		private static extern void DllTestImport();

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr process);

		[DllImport("user32.dll")]
		private static extern IntPtr GetKeyboardLayout(uint thread);

		internal static void TestExternalLibrary()
		{
			DllTestImport();
		}

		internal static IntPtr DllGetForegroundWindow()
		{
			return GetForegroundWindow();
		}

		internal static uint DllGetWindowThreadProcessId(IntPtr hwnd, IntPtr process)
		{
			return GetWindowThreadProcessId(hwnd, process);
		}

		internal static IntPtr DllGetKeyboardLayout(uint thread)
		{
			return GetKeyboardLayout(thread);
		}
	}

	private static KeyboardLayoutUtility s_instance;

	private static KeyboardLayout _currentKeyboardLayout;

	public static KeyboardLayoutUtility SharedInstance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new KeyboardLayoutUtility();
				s_instance.Initialize();
			}
			return s_instance;
		}
	}

	private void Initialize()
	{
		MicrosoftKeyboardLayout mSKeyboardLayout = GetMSKeyboardLayout();
		SetKeyboardLayout(mSKeyboardLayout);
	}

	public KeyboardLayout GetKeyboardLayout()
	{
		return _currentKeyboardLayout;
	}

	private void SetKeyboardLayout(MicrosoftKeyboardLayout msLayout)
	{
		switch (msLayout)
		{
		case MicrosoftKeyboardLayout.CHINESE_TRADITIONAL_US_KEYBOARD:
		case MicrosoftKeyboardLayout.UNITED_STATES_ENGLISH:
		case MicrosoftKeyboardLayout.SPANISH:
		case MicrosoftKeyboardLayout.ITALIAN:
		case MicrosoftKeyboardLayout.JAPANESE:
		case MicrosoftKeyboardLayout.KOREAN:
		case MicrosoftKeyboardLayout.DUTCH:
		case MicrosoftKeyboardLayout.POLISH_PROGRAMMERS:
		case MicrosoftKeyboardLayout.PORTUGUESE_BRAZILIAN_ABNT:
		case MicrosoftKeyboardLayout.RUSSIAN:
		case MicrosoftKeyboardLayout.UNITED_KINGDOM_EXTENDED:
		case MicrosoftKeyboardLayout.CHINESE_SIMPLIFIED_US_KEYBOARD:
		case MicrosoftKeyboardLayout.UNITED_KINGDOM:
		case MicrosoftKeyboardLayout.PORTUGUESE:
		case MicrosoftKeyboardLayout.CHINESE_TRADITIONAL_HONG_KONG_SAR:
		case MicrosoftKeyboardLayout.CHINESE_SIMPLIFIED_SINGAPORE_US_KEYBOARD:
		case MicrosoftKeyboardLayout.CANADIAN_FRENCH:
		case MicrosoftKeyboardLayout.CHINESE_TRADITIONAL_MACAO_SAR_US_KEYBOARD:
		case MicrosoftKeyboardLayout.CZECH_QWERTY:
		case MicrosoftKeyboardLayout.ITALIAN_142:
		case MicrosoftKeyboardLayout.PORTUGUESE_BRAZILIAN_ABNT2:
		case MicrosoftKeyboardLayout.CZECH_PROGRAMMERS:
		case MicrosoftKeyboardLayout.UNITED_STATES_INTERNATIONAL:
			_currentKeyboardLayout = KeyboardLayout.QWERTY;
			break;
		case MicrosoftKeyboardLayout.CZECH:
		case MicrosoftKeyboardLayout.GERMAN:
		case MicrosoftKeyboardLayout.SWISS_GERMAN:
		case MicrosoftKeyboardLayout.SWISS_FRENCH:
		case MicrosoftKeyboardLayout.GERMAN_IBM:
		case MicrosoftKeyboardLayout.POLISH_214:
			_currentKeyboardLayout = KeyboardLayout.QWERTZ;
			break;
		case MicrosoftKeyboardLayout.FRENCH:
		case MicrosoftKeyboardLayout.BELGIAN_FRENCH:
		case MicrosoftKeyboardLayout.BELGIAN_PERIOD:
		case MicrosoftKeyboardLayout.BELGIAN_COMMA:
			_currentKeyboardLayout = KeyboardLayout.AZERTY;
			break;
		default:
			_currentKeyboardLayout = KeyboardLayout.QWERTY;
			break;
		}
	}

	private MicrosoftKeyboardLayout GetMSKeyboardLayout()
	{
		try
		{
			MicrosoftKeyboardLayout microsoftKeyboardLayout = MicrosoftKeyboardLayout.UNITED_STATES_ENGLISH;
			int num = ExternalDllWin32.DllGetKeyboardLayout(ExternalDllWin32.DllGetWindowThreadProcessId(ExternalDllWin32.DllGetForegroundWindow(), IntPtr.Zero)).ToInt32() & 0xFFFF;
			if (num == 0 || !Enum.IsDefined(typeof(MicrosoftKeyboardLayout), num))
			{
				num = 1033;
			}
			microsoftKeyboardLayout = (MicrosoftKeyboardLayout)num;
			Debug.Log("GetMSKeyboardLayout got layout: " + microsoftKeyboardLayout);
			return microsoftKeyboardLayout;
		}
		catch (Exception ex)
		{
			Debug.Log("GetMSKeyboardLayout error: [" + ex.ToString() + "]" + ex.Message);
			return MicrosoftKeyboardLayout.UNITED_STATES_ENGLISH;
		}
	}
}
