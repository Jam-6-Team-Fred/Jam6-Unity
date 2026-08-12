using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{
	private enum State
	{
		OFF = 0,
		FADE_IN = 1,
		HOLD = 2,
		FADE_OUT = 3
	}

	[SerializeField]
	private bool _finalCredits;

	[SerializeField]
	private AnimationCurve _fadeCurve;

	[Space]
	[SerializeField]
	private AnimationCurve _fadeFromWhiteCurve;

	[SerializeField]
	private Image _fadeImage;

	[SerializeField]
	private float _fadeLength = 0.5f;

	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private OWAudioSource _kazooSource;

	private float _fadeStartTime;

	private bool _audioPlaying;

	private bool _krazyCredits;

	private CreditsData.Credits m_credits;

	private RawImage m_bg;

	private CreditsDisplay m_display;

	private int m_currentPage;

	private int m_advancePage;

	private Dictionary<string, Text> m_textPrototypes;

	private bool m_isInitialized;

	private State m_state;

	private float m_stateTimer;

	private const float s_fadeTime = 0.5f;

	private const float s_holdTime = 3f;

	private const string s_fastCreditsFile_xbox = "Credits/credits_fast_xbox";

	private const string s_krazyCreditsFile_xbox = "Credits/credits_krazy_xbox";

	private const string s_finalCreditsFile_xbox = "Credits/credits_final_xbox";

	private const string s_fastCreditsFile_epic = "Credits/credits_fast_epic";

	private const string s_krazyCreditsFile_epic = "Credits/credits_krazy_epic";

	private const string s_finalCreditsFile_epic = "Credits/credits_final_epic";

	private const string s_fastCreditsFile_playstation = "Credits/credits_fast_playstation";

	private const string s_krazyCreditsFile_playstation = "Credits/credits_krazy_playstation";

	private const string s_finalCreditsFile_playstation = "Credits/credits_final_playstation";

	private const string s_fastCreditsFile_steam = "Credits/credits_fast_steam";

	private const string s_krazyCreditsFile_steam = "Credits/credits_krazy_steam";

	private const string s_finalCreditsFile_steam = "Credits/credits_final_steam";

	private GraphicSettings startSettings;

	private void Start()
	{
		_krazyCredits = TimelineObliterationController.HasRealityEnded();
		TimelineObliterationController.ResetHasRealityEnded();
		startSettings = PlayerData.GetGraphicSettings();
		GraphicSettings graphicSettings = startSettings;
		graphicSettings.vSyncEnabled = true;
		PlayerData.SetGraphicSettings(graphicSettings);
		if (_fadeImage != null)
		{
			_fadeImage.color = Color.white;
		}
		m_textPrototypes = new Dictionary<string, Text>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			Text component = child.GetComponent<Text>();
			if (null != component)
			{
				m_textPrototypes[component.name] = component;
				child.gameObject.SetActive(value: false);
			}
		}
		TextAsset textAsset = (_finalCredits ? (Resources.Load("Credits/credits_final_steam") as TextAsset) : ((!_krazyCredits) ? (Resources.Load("Credits/credits_fast_steam") as TextAsset) : (Resources.Load("Credits/credits_krazy_steam") as TextAsset)));
		if (null != textAsset)
		{
			string xml = OWUtilities.RemoveByteOrderMark(textAsset);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			XmlNode xmlNode = xmlDocument.SelectSingleNode("Credits");
			m_credits = new CreditsData.Credits();
			XmlNode xmlNode2 = xmlNode.SelectSingleNode("m_fadeInTime");
			if (xmlNode2 != null)
			{
				m_credits.m_fadeInTime = float.Parse(xmlNode2.InnerText, OWUtilities.owFormatProvider);
			}
			XmlNode xmlNode3 = xmlNode.SelectSingleNode("m_fadeOutTime");
			if (xmlNode3 != null)
			{
				m_credits.m_fadeOutTime = float.Parse(xmlNode3.InnerText, OWUtilities.owFormatProvider);
			}
			XmlNode xmlNode4 = xmlNode.SelectSingleNode("m_ySpacing");
			if (xmlNode4 != null)
			{
				m_credits.m_ySpacing = float.Parse(xmlNode4.InnerText, OWUtilities.owFormatProvider);
			}
			m_credits.m_pages = new List<CreditsData.CreditsPage>();
			foreach (XmlNode item in xmlNode.SelectNodes("m_pages"))
			{
				CreditsData.CreditsPage creditsPage = new CreditsData.CreditsPage();
				XmlNode xmlNode5 = item.SelectSingleNode("m_yAlign");
				if (xmlNode5 != null)
				{
					creditsPage.m_yAlign = (CreditsData.CreditsPage.YAlign)Enum.Parse(typeof(CreditsData.CreditsPage.YAlign), xmlNode5.InnerText);
				}
				XmlNode xmlNode6 = item.SelectSingleNode("m_scrollSpeed");
				if (xmlNode6 != null)
				{
					creditsPage.m_scrollSpeed = float.Parse(xmlNode6.InnerText, OWUtilities.owFormatProvider);
				}
				XmlNode xmlNode7 = item.SelectSingleNode("m_displayTime");
				if (xmlNode7 != null)
				{
					creditsPage.m_displayTime = float.Parse(xmlNode7.InnerText, OWUtilities.owFormatProvider);
				}
				XmlNode xmlNode8 = item.SelectSingleNode("m_waitTime");
				if (xmlNode8 != null)
				{
					creditsPage.m_waitTime = float.Parse(xmlNode8.InnerText, OWUtilities.owFormatProvider);
				}
				creditsPage.m_lines = new List<CreditsData.CreditsLine>();
				foreach (XmlNode item2 in item.SelectNodes("m_lines"))
				{
					CreditsData.CreditsLine creditsLine = new CreditsData.CreditsLine();
					XmlNode namedItem = item2.Attributes.GetNamedItem("m_style");
					if (namedItem != null)
					{
						creditsLine.m_style = namedItem.InnerText;
					}
					XmlNode namedItem2 = item2.Attributes.GetNamedItem("m_yOffset");
					if (namedItem2 != null)
					{
						creditsLine.m_yOffset = float.Parse(namedItem2.InnerText, OWUtilities.owFormatProvider);
					}
					XmlNode namedItem3 = item2.Attributes.GetNamedItem("m_string");
					if (namedItem3 != null)
					{
						creditsLine.m_string = namedItem3.InnerText;
					}
					creditsPage.m_lines.Add(creditsLine);
				}
				m_credits.m_pages.Add(creditsPage);
			}
		}
		Resources.UnloadAsset(textAsset);
		m_currentPage = 0;
		m_advancePage = 0;
		m_display = new CreditsDisplay(m_credits.m_pages[m_currentPage], base.transform, m_textPrototypes, m_credits.m_ySpacing, m_credits.m_fadeInTime, m_credits.m_fadeOutTime, _fadeCurve);
	}

	private void Init()
	{
		if (m_isInitialized)
		{
			return;
		}
		m_textPrototypes = new Dictionary<string, Text>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			Text component = child.GetComponent<Text>();
			if (null != component)
			{
				m_textPrototypes[component.name] = component;
				child.gameObject.SetActive(value: false);
			}
		}
		m_isInitialized = true;
	}

	private void AdvancePage()
	{
		if (m_display != null)
		{
			m_display.Destroy();
			m_display = null;
		}
		if (m_advancePage != 0)
		{
			m_currentPage += m_advancePage;
			if (m_currentPage < 0)
			{
				m_currentPage = 0;
			}
			m_advancePage = 0;
		}
		else
		{
			m_currentPage++;
		}
		if (m_currentPage >= m_credits.m_pages.Count)
		{
			if (_finalCredits)
			{
				LoadManager.LoadScene(OWScene.PostCreditsScene);
			}
			else
			{
				LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack);
			}
		}
		else
		{
			m_display = new CreditsDisplay(m_credits.m_pages[m_currentPage], base.transform, m_textPrototypes, m_credits.m_ySpacing, m_credits.m_fadeInTime, m_credits.m_fadeOutTime, _fadeCurve);
		}
	}

	private void Update()
	{
		if (!_finalCredits && (OWInput.IsNewlyPressed(InputLibrary.enter) || OWInput.IsNewlyPressed(InputLibrary.enter2) || OWInput.IsNewlyPressed(InputLibrary.select) || OWInput.IsNewlyPressed(InputLibrary.menuConfirm) || OWInput.IsNewlyPressed(InputLibrary.cancel) || OWInput.IsNewlyPressed(InputLibrary.escape)))
		{
			PlayerData.SetGraphicSettings(startSettings);
			if (LoadManager.GetLoadingScene() == OWScene.None)
			{
				if (_finalCredits)
				{
					LoadManager.LoadScene(OWScene.PostCreditsScene);
				}
				else
				{
					LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack);
				}
			}
		}
		bool flag = true;
		if (_fadeImage != null)
		{
			if (_fadeStartTime == 0f)
			{
				_fadeStartTime = Time.time;
			}
			float time = Mathf.Clamp01((Time.time - _fadeStartTime) / _fadeLength);
			_fadeImage.color = new Color(Color.white.r, Color.white.g, Color.white.b, 1f - _fadeFromWhiteCurve.Evaluate(time));
			if (Time.time < _fadeStartTime + _fadeLength + 1.5f)
			{
				flag = false;
			}
		}
		if (!flag)
		{
			return;
		}
		if (!_audioPlaying)
		{
			if (_finalCredits)
			{
				_audioSource.FadeIn(4f, fadeFromNothing: true);
			}
			else if (_krazyCredits)
			{
				_kazooSource.FadeIn(5f, fadeFromNothing: true);
			}
			else
			{
				_audioSource.FadeIn(5f, fadeFromNothing: true);
			}
			_audioPlaying = true;
		}
		if (m_display != null)
		{
			m_display.Update();
			if (m_display.IsDone() || m_advancePage != 0)
			{
				AdvancePage();
			}
		}
	}
}
