using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

[Serializable]
public class ShipLogEntry
{
	public enum State
	{
		None = 0,
		Hidden = 1,
		Rumored = 2,
		Explored = 3
	}

	private string _id;

	private string _parentID;

	private string _astroObjectID;

	private string _name;

	private string _altPhotoConditionID;

	private Sprite _sprite;

	private Sprite _altSprite;

	private State _state;

	private State _previousState;

	private CuriosityName _curiosity;

	private bool _isCuriosity;

	private bool _ignoreMoreToExplore;

	private bool _parentIgnoreNotRevealed;

	private string _ignoreMoreToExploreCondition;

	private bool _extraLargeMoreToExploreIcon;

	private List<ShipLogFact> _rumorFacts;

	private List<ShipLogFact> _exploreFacts;

	private List<ShipLogFact> _completionFacts;

	[NonSerialized]
	private List<ShipLogEntry> _childEntries;

	public ShipLogEntry(string astroObjectID, XElement entryNode, string parentID = "")
	{
		_astroObjectID = astroObjectID;
		_completionFacts = new List<ShipLogFact>();
		_id = entryNode.Element("ID").Value;
		_parentID = parentID;
		_name = entryNode.Element("Name").Value;
		if (_id == "IP_DREAM_LAKE")
		{
			_extraLargeMoreToExploreIcon = true;
		}
		_ignoreMoreToExplore = entryNode.Element("IgnoreMoreToExplore") != null;
		_parentIgnoreNotRevealed = entryNode.Element("ParentIgnoreNotRevealed") != null;
		_isCuriosity = entryNode.Element("IsCuriosity") != null;
		XElement xElement = entryNode.Element("IgnoreMoreToExploreCondition");
		_ignoreMoreToExploreCondition = ((xElement != null) ? xElement.Value : string.Empty);
		XElement xElement2 = entryNode.Element("AltPhotoCondition");
		_altPhotoConditionID = ((xElement2 != null) ? xElement2.Value : string.Empty);
		XElement xElement3 = entryNode.Element("Curiosity");
		_curiosity = ((xElement3 != null) ? OWUtilities.CuriosityStringToName(xElement3.Value) : CuriosityName.None);
		IEnumerable<XElement> enumerable = entryNode.Elements("RumorFact");
		_rumorFacts = new List<ShipLogFact>();
		foreach (XElement item in enumerable)
		{
			ShipLogFact shipLogFact = new ShipLogFact(_id, rumor: true, item, _name);
			shipLogFact.OnFactRevealed += OnFactRevealed;
			_rumorFacts.Add(shipLogFact);
		}
		IEnumerable<XElement> enumerable2 = entryNode.Elements("ExploreFact");
		_exploreFacts = new List<ShipLogFact>();
		foreach (XElement item2 in enumerable2)
		{
			ShipLogFact shipLogFact2 = new ShipLogFact(_id, rumor: false, item2, _name);
			shipLogFact2.OnFactRevealed += OnFactRevealed;
			_exploreFacts.Add(shipLogFact2);
			_completionFacts.Add(shipLogFact2);
		}
		IEnumerable<XElement> enumerable3 = entryNode.Elements("Entry");
		_childEntries = new List<ShipLogEntry>();
		foreach (XElement item3 in enumerable3)
		{
			_childEntries.Add(new ShipLogEntry(astroObjectID, item3, _id));
		}
		UpdateState();
		_previousState = State.None;
		if (EntitlementsManager.IsDlcOwned() != EntitlementsManager.AsyncOwnershipStatus.Owned && _curiosity == CuriosityName.InvisiblePlanet)
		{
			_state = State.Hidden;
		}
	}

	public void SetSprite(Sprite sprite)
	{
		_sprite = sprite;
	}

	public void SetAltSprite(Sprite altSprite)
	{
		_altSprite = altSprite;
	}

	public string GetID()
	{
		return _id;
	}

	public string GetParentID()
	{
		return _parentID;
	}

	public string GetAstroObjectID()
	{
		return _astroObjectID;
	}

	public bool GetParentIgnoreNotRevealed()
	{
		return _parentIgnoreNotRevealed;
	}

	public Sprite GetSprite()
	{
		if (_altPhotoConditionID.Length <= 0 || !Locator.GetShipLogManager().IsFactRevealed(_altPhotoConditionID))
		{
			return _sprite;
		}
		return _altSprite;
	}

	public State GetState()
	{
		return _state;
	}

	public State GetPreviousState()
	{
		return _previousState;
	}

	public CuriosityName GetCuriosityName()
	{
		return _curiosity;
	}

	public bool IsCuriosity()
	{
		return _isCuriosity;
	}

	public bool HasParent()
	{
		return _parentID.Length > 0;
	}

	public bool HasRevealedParent()
	{
		if (_parentID.Length > 0)
		{
			return Locator.GetShipLogManager().GetEntry(_parentID).GetState() != State.Hidden;
		}
		return false;
	}

	public List<ShipLogFact> GetRumorFacts()
	{
		return _rumorFacts;
	}

	public List<ShipLogFact> GetExploreFacts()
	{
		return _exploreFacts;
	}

	public List<ShipLogEntry> GetChildren()
	{
		return _childEntries;
	}

	public void AddCompletionFact(ShipLogFact fact)
	{
		_completionFacts.Add(fact);
	}

	public int GetRevealOrder(bool includeChildren = true)
	{
		if (GetState() == State.Hidden)
		{
			return -1;
		}
		int num = int.MaxValue;
		if (includeChildren)
		{
			for (int i = 0; i < _childEntries.Count; i++)
			{
				if (_childEntries[i].GetState() != State.Hidden)
				{
					num = Mathf.Min(num, _childEntries[i].GetRevealOrder());
				}
			}
		}
		if (GetState() == State.Explored)
		{
			for (int j = 0; j < _exploreFacts.Count; j++)
			{
				if (_exploreFacts[j].IsRevealed())
				{
					num = Mathf.Min(num, _exploreFacts[j].GetRevealOrder());
				}
			}
		}
		for (int k = 0; k < _rumorFacts.Count; k++)
		{
			if (_rumorFacts[k].IsRevealed())
			{
				num = Mathf.Min(num, _rumorFacts[k].GetRevealOrder());
			}
		}
		return num;
	}

	public string GetName(bool withLineBreaks)
	{
		string text = TextTranslation.Translate_ShipLog(_name);
		bool flag = !TextTranslation.Get().IsLanguageLatin() && TextTranslation.Get().GetLanguage() != TextTranslation.Language.RUSSIAN && TextTranslation.Get().GetLanguage() != TextTranslation.Language.POLISH && TextTranslation.Get().GetLanguage() != TextTranslation.Language.TURKISH;
		if (withLineBreaks)
		{
			text = text.Replace("@@", "\n");
			text = text.Replace("$$", "-\n");
		}
		else
		{
			text = ((!flag) ? text.Replace("@@", " ") : text.Replace("@@", ""));
			text = text.Replace("$$", "");
		}
		if (_state == State.Explored)
		{
			return text;
		}
		int num = -1;
		string text2 = text;
		for (int i = 0; i < _rumorFacts.Count; i++)
		{
			if (_rumorFacts[i].IsRevealed() && _rumorFacts[i].GetEntryRumorNamePriority() > num)
			{
				num = _rumorFacts[i].GetEntryRumorNamePriority();
				text2 = _rumorFacts[i].GetEntryRumorName();
			}
		}
		if (withLineBreaks)
		{
			text2 = text2.Replace("@@", "\n");
			return text2.Replace("$$", "-\n");
		}
		text2 = ((!flag) ? text2.Replace("@@", " ") : text2.Replace("@@", ""));
		return text2.Replace("$$", "");
	}

	public List<ShipLogFact> GetFactsForDisplay()
	{
		List<ShipLogFact> list = new List<ShipLogFact>();
		if (GetState() == State.Explored)
		{
			for (int i = 0; i < _exploreFacts.Count; i++)
			{
				if (_exploreFacts[i].IsRevealed())
				{
					list.Add(_exploreFacts[i]);
				}
			}
		}
		else if (GetState() == State.Rumored)
		{
			for (int j = 0; j < _rumorFacts.Count; j++)
			{
				if (_rumorFacts[j].IsRevealed())
				{
					list.Add(_rumorFacts[j]);
				}
			}
		}
		return list;
	}

	public bool HasUnreadFacts()
	{
		if (_state == State.Hidden)
		{
			return false;
		}
		List<ShipLogFact> list = ((_state == State.Explored) ? _exploreFacts : _rumorFacts);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsRevealed() && !list[i].IsRead())
			{
				return true;
			}
		}
		return false;
	}

	public void MarkAsRead()
	{
		List<ShipLogFact> list = ((_state == State.Explored) ? _exploreFacts : _rumorFacts);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].MarkAsRead();
		}
	}

	public bool UseExtraLargeMoreToExploreIcon()
	{
		return _extraLargeMoreToExploreIcon;
	}

	public bool HasMoreToExplore()
	{
		if (!_ignoreMoreToExplore && _state == State.Explored)
		{
			if (_ignoreMoreToExploreCondition.Length > 0 && PlayerData.GetPersistentCondition(_ignoreMoreToExploreCondition))
			{
				return false;
			}
			for (int i = 0; i < _childEntries.Count; i++)
			{
				if (_childEntries[i].GetState() == State.Hidden && !_childEntries[i].GetParentIgnoreNotRevealed())
				{
					return true;
				}
			}
			for (int j = 0; j < _completionFacts.Count; j++)
			{
				if (!_completionFacts[j].GetIgnoreMoreToExplore() && !_completionFacts[j].IsRevealed() && (!_completionFacts[j].IsRumor() || Locator.GetShipLogManager().GetEntry(_completionFacts[j].GetEntryID()).GetState() != State.Explored))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public void UpdatePreviousState()
	{
		_previousState = CalculateState(ignoreNewFacts: true);
	}

	private void UpdateState()
	{
		_state = CalculateState();
	}

	private State CalculateState(bool ignoreNewFacts = false)
	{
		for (int i = 0; i < _exploreFacts.Count; i++)
		{
			if (_exploreFacts[i].IsRevealed() && (!_exploreFacts[i].IsNewlyRevealed() || !ignoreNewFacts))
			{
				return State.Explored;
			}
		}
		for (int j = 0; j < _rumorFacts.Count; j++)
		{
			if (_rumorFacts[j].IsRevealed() && (!_rumorFacts[j].IsNewlyRevealed() || !ignoreNewFacts))
			{
				return State.Rumored;
			}
		}
		return State.Hidden;
	}

	private void OnFactRevealed()
	{
		UpdateState();
	}
}
