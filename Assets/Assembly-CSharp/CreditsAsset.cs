using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class CreditsAsset : ScriptableObject, ISerializationCallbackReceiver
{
	public TextAsset xml;

	public GameObject fadeSectionTemplate;

	public GameObject scrollSectionTemplate;

	public GameObject lineSpacerTemplate;

	[SerializeField]
	[HideInInspector]
	private TemplateDictionary _templates;

	[SerializeField]
	[HideInInspector]
	private string _xmlText;

	private readonly string[] newLines = new string[3] { "\r\n", "\r", "\n" };

	private Dictionary<string, XmlNode> _namedSections;

	public TemplateDictionary templates
	{
		get
		{
			return _templates;
		}
		set
		{
			_templates = value;
		}
	}

	public string xmlText
	{
		get
		{
			return _xmlText;
		}
		set
		{
			_xmlText = value;
		}
	}

	public List<CreditsSection> BuildCredits(Transform root, Credits.Platform simulatedPlatform, Credits.CreditsType creditsType, ref float totalPlayTime)
	{
		List<CreditsSection> list = new List<CreditsSection>();
		BuildCreditsFromXml(root, OWUtilities.RemoveByteOrderMark(xml), list, creditsType, simulatedPlatform, ref totalPlayTime);
		return list;
	}

	public void OnBeforeSerialize()
	{
		if (xml != null)
		{
			_xmlText = xml.text;
		}
		else
		{
			_xmlText = "";
		}
	}

	public void OnAfterDeserialize()
	{
	}

	private void BuildCreditsFromXml(Transform root, string xmlString, List<CreditsSection> topLevelSectionsList, Credits.CreditsType creditsType, Credits.Platform simulatedPlatform, ref float totalPlayTime)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xmlString);
		PopulateNamedSectionsRef(xmlDocument);
		XmlNode parentNode = xmlDocument.SelectSingleNode("Credits");
		ProcessChildren(root, parentNode, topLevelSectionsList, creditsType, simulatedPlatform, ref totalPlayTime);
		_namedSections.Clear();
	}

	private void PopulateNamedSectionsRef(XmlDocument document)
	{
		_namedSections = new Dictionary<string, XmlNode>();
		foreach (XmlNode item in document.GetElementsByTagName("section"))
		{
			XmlAttribute xmlAttribute = item.Attributes["name"];
			if (xmlAttribute != null)
			{
				if (_namedSections == null)
				{
					_namedSections = new Dictionary<string, XmlNode>();
				}
				_namedSections.Add(xmlAttribute.Value, item);
			}
		}
	}

	private void ProcessChildren(Transform currentParent, XmlNode parentNode, List<CreditsSection> topLevelSectionsList, Credits.CreditsType creditsType, Credits.Platform simulatedPlatform, ref float totalPlayTime)
	{
		for (int i = 0; i < parentNode.ChildNodes.Count; i++)
		{
			ProcessNode(currentParent, parentNode.ChildNodes[i], topLevelSectionsList, creditsType, simulatedPlatform, ref totalPlayTime);
		}
	}

	private void ProcessNode(Transform currentParent, XmlNode node, List<CreditsSection> topLevelSectionsList, Credits.CreditsType creditsType, Credits.Platform simulatedPlatform, ref float totalPlayTime)
	{
		if (string.Equals(node.Name, "section", StringComparison.CurrentCultureIgnoreCase))
		{
			ProcessSection(currentParent, node, topLevelSectionsList, creditsType, simulatedPlatform, ref totalPlayTime);
		}
		else if (string.Equals(node.Name, "entry", StringComparison.CurrentCultureIgnoreCase) || string.Equals(node.Name, "layout", StringComparison.CurrentCultureIgnoreCase))
		{
			ProcessEntry(currentParent, node);
		}
	}

	private void ProcessSection(Transform currentParent, XmlNode sectionNode, List<CreditsSection> topLevelSectionsList, Credits.CreditsType creditsType, Credits.Platform simulatedPlatform, ref float totalPlayTime)
	{
		if (!ShouldDisplaySectionForPlatform(sectionNode, simulatedPlatform) || !ShouldDisplaySectionForCreditsType(sectionNode, creditsType))
		{
			return;
		}
		XmlAttribute xmlAttribute = sectionNode.Attributes["type"];
		XmlAttribute xmlAttribute2 = sectionNode.Attributes["name"];
		CreditsSection creditsSection = null;
		if (xmlAttribute != null)
		{
			if (string.Equals(xmlAttribute.Value, "fade", StringComparison.CurrentCultureIgnoreCase))
			{
				creditsSection = UnityEngine.Object.Instantiate(fadeSectionTemplate, currentParent).GetComponent<CreditsSection>();
				if (xmlAttribute2 != null)
				{
					creditsSection.name = xmlAttribute2.Value;
				}
				CreditsFadeSection creditsFadeSection = creditsSection as CreditsFadeSection;
				XmlAttribute xmlAttribute3 = sectionNode.Attributes["fadeInTime"];
				if (xmlAttribute3 != null)
				{
					creditsFadeSection.fadeInDuration = float.Parse(xmlAttribute3.Value, OWUtilities.owFormatProvider);
				}
				XmlAttribute xmlAttribute4 = sectionNode.Attributes["displayTime"];
				if (xmlAttribute3 != null)
				{
					creditsFadeSection.displayDuration = float.Parse(xmlAttribute4.Value, OWUtilities.owFormatProvider);
				}
				XmlAttribute xmlAttribute5 = sectionNode.Attributes["fadeOutTime"];
				if (xmlAttribute3 != null)
				{
					creditsFadeSection.fadeOutDuration = float.Parse(xmlAttribute5.Value, OWUtilities.owFormatProvider);
				}
				XmlAttribute xmlAttribute6 = sectionNode.Attributes["waitTime"];
				if (xmlAttribute3 != null)
				{
					creditsFadeSection.waitDuration = float.Parse(xmlAttribute6.Value, OWUtilities.owFormatProvider);
				}
			}
			else if (string.Equals(xmlAttribute.Value, "scroll", StringComparison.CurrentCultureIgnoreCase))
			{
				creditsSection = UnityEngine.Object.Instantiate(scrollSectionTemplate, currentParent).GetComponent<CreditsSection>();
				if (xmlAttribute2 != null)
				{
					creditsSection.name = xmlAttribute2.Value;
				}
				XmlAttribute xmlAttribute7 = sectionNode.Attributes["scrollDuration"];
				if (xmlAttribute7 != null)
				{
					(creditsSection as CreditsScrollSection).SetScrollDuration(float.Parse(xmlAttribute7.Value, OWUtilities.owFormatProvider));
				}
			}
			if (creditsSection != null)
			{
				ProcessSectionLayoutAttributes(creditsSection, sectionNode);
				totalPlayTime += creditsSection.GetTotalTime();
				topLevelSectionsList.Add(creditsSection);
			}
		}
		Transform currentParent2 = ((creditsSection != null) ? creditsSection.transform : currentParent);
		if (sectionNode.Attributes["copy-content"] != null)
		{
			XmlNode parentNode = _namedSections[sectionNode.Attributes["copy-content"].Value];
			ProcessChildren(currentParent2, parentNode, topLevelSectionsList, creditsType, simulatedPlatform, ref totalPlayTime);
		}
		else
		{
			ProcessChildren(currentParent2, sectionNode, topLevelSectionsList, creditsType, simulatedPlatform, ref totalPlayTime);
		}
	}

	private void ProcessSectionLayoutAttributes(CreditsSection section, XmlNode sectionNode)
	{
		bool flag = false;
		bool flag2 = false;
		RectOffset rectOffset = new RectOffset(0, 0, 0, 0);
		float spacing = 0f;
		float num = -1f;
		if (sectionNode.Attributes["padding-top"] != null)
		{
			rectOffset.top = int.Parse(sectionNode.Attributes["padding-top"].Value, OWUtilities.owFormatProvider);
			flag = true;
		}
		if (sectionNode.Attributes["padding-left"] != null)
		{
			rectOffset.left = int.Parse(sectionNode.Attributes["padding-left"].Value, OWUtilities.owFormatProvider);
			flag = true;
		}
		if (sectionNode.Attributes["padding-bottom"] != null)
		{
			rectOffset.bottom = int.Parse(sectionNode.Attributes["padding-bottom"].Value, OWUtilities.owFormatProvider);
			flag = true;
		}
		if (sectionNode.Attributes["padding-right"] != null)
		{
			rectOffset.right = int.Parse(sectionNode.Attributes["padding-right"].Value, OWUtilities.owFormatProvider);
			flag = true;
		}
		if (sectionNode.Attributes["spacing"] != null)
		{
			spacing = float.Parse(sectionNode.Attributes["spacing"].Value, OWUtilities.owFormatProvider);
			flag = true;
		}
		if (sectionNode.Attributes["width"] != null)
		{
			num = float.Parse(sectionNode.Attributes["width"].Value, OWUtilities.owFormatProvider);
			flag2 = true;
		}
		if (flag)
		{
			VerticalLayoutGroup component = section.GetComponent<VerticalLayoutGroup>();
			component.padding = rectOffset;
			component.spacing = spacing;
		}
		if (flag2)
		{
			RectTransform component2 = section.GetComponent<RectTransform>();
			if (num >= 0f)
			{
				component2.sizeDelta = new Vector2(num, component2.sizeDelta.y);
			}
		}
	}

	private int GetSectionPlatform(XmlNode sectionNode)
	{
		if (sectionNode.Attributes["platform"] == null)
		{
			return int.MaxValue;
		}
		int num = 0;
		string[] array = sectionNode.Attributes["platform"].Value.Split(' ');
		for (int i = 0; i < array.Length; i++)
		{
			num += (int)Enum.Parse(typeof(Credits.Platform), array[i].Trim());
		}
		return num;
	}

	private Credits.Platform GetCurrentPlatform(Credits.Platform simulatedPlatform)
	{
		return Credits.Platform.Steam;
	}

	private bool ShouldDisplaySectionForPlatform(XmlNode sectionNode, Credits.Platform simulatePlatform)
	{
		return (int)((uint)GetCurrentPlatform(simulatePlatform) & (uint)GetSectionPlatform(sectionNode)) > 0;
	}

	private bool ShouldDisplaySectionForCreditsType(XmlNode sectionNode, Credits.CreditsType requestedType)
	{
		if (sectionNode.Attributes["credits-type"] == null)
		{
			return true;
		}
		int num = 0;
		string[] array = sectionNode.Attributes["credits-type"].Value.Split(' ');
		for (int i = 0; i < array.Length; i++)
		{
			num += (int)Enum.Parse(typeof(Credits.CreditsType), array[i].Trim());
		}
		return (int)((uint)num & (uint)requestedType) > 0;
	}

	private void ProcessEntry(Transform currentParent, XmlNode entryNode)
	{
		XmlAttribute xmlAttribute = entryNode.Attributes["type"];
		if (xmlAttribute != null)
		{
			float spacerBaseHeight = 0f;
			if (entryNode.Attributes["spacer-base-height"] != null)
			{
				spacerBaseHeight = float.Parse(entryNode.Attributes["spacer-base-height"].Value, OWUtilities.owFormatProvider);
			}
			ProcessEntryNodes(currentParent, entryNode, xmlAttribute.Value, spacerBaseHeight);
		}
	}

	private void ProcessEntryNodes(Transform currentParent, XmlNode entryNode, string templateName, float spacerBaseHeight)
	{
		for (int i = 0; i < entryNode.ChildNodes.Count; i++)
		{
			switch (entryNode.ChildNodes[i].NodeType)
			{
			case XmlNodeType.Text:
				ProcessEntryLineInnerText(currentParent, entryNode.ChildNodes[i].Value, CreditsEntry.Style.Content, templateName);
				break;
			case XmlNodeType.Element:
			{
				if (entryNode.ChildNodes[i].Name.Equals("spacer", StringComparison.CurrentCultureIgnoreCase))
				{
					ProcessSpacerNode(currentParent, entryNode.ChildNodes[i], spacerBaseHeight);
					break;
				}
				CreditsEntry[] entries = ProcessEntryLineInnerText(currentParent, entryNode.ChildNodes[i].InnerText, GetStyleEnum(entryNode.ChildNodes[i].Name), templateName);
				ProcessEntryNodeLayoutAttributes(entries, entryNode.ChildNodes[i]);
				break;
			}
			}
		}
	}

	private void ProcessSpacerNode(Transform currentParent, XmlNode spacerNode, float baseHeight)
	{
		RectTransform component = UnityEngine.Object.Instantiate(lineSpacerTemplate.gameObject, currentParent).GetComponent<RectTransform>();
		if (spacerNode.Attributes["height"] != null)
		{
			component.sizeDelta = new Vector2(component.sizeDelta.x, float.Parse(spacerNode.Attributes["height"].Value, OWUtilities.owFormatProvider));
		}
		else
		{
			component.sizeDelta = new Vector2(component.sizeDelta.x, baseHeight);
		}
	}

	private CreditsEntry[] ProcessEntryLineInnerText(Transform currentParent, string innerText, CreditsEntry.Style style, string templateName)
	{
		string[] array = innerText.Split(newLines, StringSplitOptions.None);
		List<CreditsEntry> list = new List<CreditsEntry>();
		for (int i = 0; i < array.Length; i++)
		{
			if (!string.IsNullOrEmpty(array[i]) && !string.IsNullOrWhiteSpace(array[i]))
			{
				CreditsEntry creditsEntry = null;
				switch (style)
				{
				case CreditsEntry.Style.Title:
					creditsEntry = _templates[templateName].titleTemplate;
					break;
				case CreditsEntry.Style.Header:
					creditsEntry = _templates[templateName].headerTemplate;
					break;
				default:
					creditsEntry = _templates[templateName].contentTemplate;
					break;
				}
				if (creditsEntry != null)
				{
					string[] contents = array[i].Split('#');
					GameObject gameObject = UnityEngine.Object.Instantiate(creditsEntry.gameObject, currentParent);
					gameObject.name = array[i].Trim();
					CreditsEntry component = gameObject.GetComponent<CreditsEntry>();
					component.SetContents(contents);
					list.Add(component);
				}
			}
		}
		return list.ToArray();
	}

	private void ProcessEntryNodeLayoutAttributes(CreditsEntry[] entries, XmlNode entryNode)
	{
		bool flag = false;
		bool flag2 = false;
		float num = -1f;
		if (entryNode.Attributes["height"] != null)
		{
			flag = true;
			num = float.Parse(entryNode.Attributes["height"].Value, OWUtilities.owFormatProvider);
		}
		TextAnchor alignment = TextAnchor.MiddleCenter;
		if (entryNode.Attributes["text-align"] != null)
		{
			flag2 = true;
			alignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), entryNode.Attributes["text-align"].Value);
		}
		if (!flag && !flag2)
		{
			return;
		}
		for (int i = 0; i < entries.Length; i++)
		{
			if (flag)
			{
				RectTransform component = entries[i].GetComponent<RectTransform>();
				if (num > 0f)
				{
					component.sizeDelta = new Vector2(component.sizeDelta.x, num);
				}
			}
			if (flag2)
			{
				Text[] componentsInChildren = entries[i].GetComponentsInChildren<Text>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[i].alignment = alignment;
				}
			}
		}
	}

	private CreditsEntry.Style GetStyleEnum(string styleName)
	{
		if (string.Equals(styleName, "title", StringComparison.CurrentCultureIgnoreCase))
		{
			return CreditsEntry.Style.Title;
		}
		if (string.Equals(styleName, "heading", StringComparison.CurrentCultureIgnoreCase) || string.Equals(styleName, "header", StringComparison.CurrentCultureIgnoreCase))
		{
			return CreditsEntry.Style.Header;
		}
		return CreditsEntry.Style.Content;
	}
}
