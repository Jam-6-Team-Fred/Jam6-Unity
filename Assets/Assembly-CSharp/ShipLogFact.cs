using System;
using System.Xml.Linq;
using UnityEngine;

[Serializable]
public class ShipLogFact
{
	public delegate void FactEvent();

	private string _entryName;

	private ShipLogFactSave _save;

	private string _entryID;

	private string _sourceID;

	private string _entryRumorName;

	private int _entryRumorNamePriority;

	private string _text;

	private bool _rumor;

	private bool _ignoreMoreToExplore;

	private XElement _altTextNode;

	private string _altText;

	private SlideCollection _slideCollection;

	private bool _startedTextReveal;

	private float _startRevealTime;

	public event FactEvent OnFactRevealed;

	public ShipLogFact(string entryID, bool rumor, XElement factNode, string entryName)
	{
		_entryID = entryID;
		_rumor = rumor;
		_entryName = entryName;
		string value = factNode.Element("ID").Value;
		XElement xElement = factNode.Element("Text");
		_text = ((xElement != null) ? xElement.Value : "");
		_altTextNode = factNode.Element("AltText");
		XElement xElement2 = factNode.Element("SourceID");
		_sourceID = ((xElement2 != null) ? xElement2.Value : "");
		XElement xElement3 = factNode.Element("RumorName");
		_entryRumorName = ((xElement3 != null) ? xElement3.Value : "");
		_entryRumorNamePriority = -1;
		if (_entryRumorName.Length > 0)
		{
			XElement xElement4 = factNode.Element("RumorNamePriority");
			_entryRumorNamePriority = ((xElement4 != null) ? int.Parse(xElement4.Value) : 0);
		}
		_ignoreMoreToExplore = factNode.Element("IgnoreMoreToExplore") != null;
		_save = PlayerData.GetShipLogFactSave(value);
		if (_save == null)
		{
			_save = new ShipLogFactSave(value);
			PlayerData.AddShipLogFactSave(_save);
		}
	}

	public void Reveal(int revealOrder)
	{
		if (!IsRevealed())
		{
			_save.revealOrder = revealOrder;
			_save.read = false;
			_save.newlyRevealed = true;
			if (this.OnFactRevealed != null)
			{
				this.OnFactRevealed();
			}
			PlayerData.AddNewlyRevealedFactID(_save.id);
		}
	}

	public void StartTextReveal()
	{
		if (!_save.read && !_startedTextReveal)
		{
			_startedTextReveal = true;
			_startRevealTime = Time.unscaledTime;
		}
	}

	public string GetRevealedText(out bool isRevealing)
	{
		string text = GetText();
		if (_startedTextReveal)
		{
			int num = (int)Mathf.Min(60f * (Time.unscaledTime - _startRevealTime), text.Length);
			isRevealing = num < text.Length;
			return text.Substring(0, num);
		}
		isRevealing = false;
		return text;
	}

	public string GetID()
	{
		return _save.id;
	}

	public string GetEntryRumorName()
	{
		return TextTranslation.Translate_ShipLog(_entryRumorName);
	}

	public int GetEntryRumorNamePriority()
	{
		return _entryRumorNamePriority;
	}

	public bool IsRevealed()
	{
		return _save.revealOrder > -1;
	}

	public int GetRevealOrder()
	{
		return _save.revealOrder;
	}

	public bool IsNewlyRevealed()
	{
		return _save.newlyRevealed;
	}

	public bool IsRead()
	{
		return _save.read;
	}

	public bool IsRumor()
	{
		return _rumor;
	}

	public string GetText()
	{
		return TextTranslation.Translate_ShipLog(_entryName + _text);
	}

	public bool HasSource()
	{
		return _sourceID.Length > 0;
	}

	public string GetSourceID()
	{
		return _sourceID;
	}

	public string GetEntryID()
	{
		return _entryID;
	}

	public bool GetIgnoreMoreToExplore()
	{
		return _ignoreMoreToExplore;
	}

	public void ClearNewlyRevealed()
	{
		_save.newlyRevealed = false;
	}

	public void MarkAsRead()
	{
		_save.read = true;
	}

	public void RegisterSlideCollection(SlideCollection slideCollection)
	{
		_slideCollection = slideCollection;
	}

	public SlideCollection GetSlideCollection()
	{
		return _slideCollection;
	}

	public void InitializeAltText(ShipLogManager manager)
	{
		if (_altTextNode == null)
		{
			return;
		}
		XElement xElement = _altTextNode.Element("Text");
		_altText = ((xElement != null) ? xElement.Value : "");
		foreach (XElement item in _altTextNode.Elements("Condition"))
		{
			ShipLogFact fact = manager.GetFact(item.Value);
			if (fact != null)
			{
				if (fact.IsRevealed())
				{
					_text = _altText;
					break;
				}
				fact.OnFactRevealed += OnAltTextConditionFactRevealed;
			}
			else
			{
				Debug.LogError(_entryID + " failed to find alt text condition fact: " + item.Value);
			}
		}
	}

	public void OnAltTextConditionFactRevealed()
	{
		_text = _altText;
	}
}
