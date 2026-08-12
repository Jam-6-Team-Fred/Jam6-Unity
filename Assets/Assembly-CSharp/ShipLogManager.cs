using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class ShipLogManager : MonoBehaviour
{
	private struct FactRevealData
	{
		public ShipLogFact fact;

		public string message;

		public FactRevealData(ShipLogFact fact, string message)
		{
			this.fact = fact;
			this.message = message;
		}
	}

	[SerializeField]
	private TextAsset[] _shipLogXmlAssets;

	[SerializeField]
	private ShipLogLibrary _shipLogLibrary;

	private List<ShipLogEntry> _entryList;

	private Dictionary<string, ShipLogEntry> _entryDict;

	private Dictionary<string, EntryData> _entryDataDict;

	private List<ShipLogFact> _factList;

	private Dictionary<string, ShipLogFact> _factDict;

	private float _postNotificationTime;

	private List<FactRevealData> _pendingRevealData;

	private float _minPostNotificationTime;

	private int _factRevealCount;

	private bool _markOnHUDTutorialActive;

	private bool _markOnHUDTutorialComplete;

	private void Awake()
	{
		_entryList = new List<ShipLogEntry>();
		_entryDict = new Dictionary<string, ShipLogEntry>();
		_factList = new List<ShipLogFact>();
		_factDict = new Dictionary<string, ShipLogFact>();
		_pendingRevealData = new List<FactRevealData>();
		for (int i = 0; i < _shipLogXmlAssets.Length; i++)
		{
			GenerateEntriesFromXml(_shipLogXmlAssets[i]);
		}
		_entryDataDict = _shipLogLibrary.BuildEntryDataDictionary();
		for (int j = 0; j < _factList.Count; j++)
		{
			if (_factList[j].HasSource())
			{
				_entryDict[_factList[j].GetSourceID()].AddCompletionFact(_factList[j]);
			}
			_factList[j].InitializeAltText(this);
		}
		for (int k = 0; k < _entryList.Count; k++)
		{
			if (_entryDataDict.ContainsKey(_entryList[k].GetID()) && _entryDataDict[_entryList[k].GetID()].sprite != null)
			{
				_entryList[k].SetSprite(_entryDataDict[_entryList[k].GetID()].sprite);
				_entryList[k].SetAltSprite(_entryDataDict[_entryList[k].GetID()].altSprite);
			}
			else
			{
				_entryList[k].SetSprite(_shipLogLibrary.defaultEntrySprite);
			}
		}
		GlobalMessenger<float>.AddListener("UntranslatedTextRemainingNotificationPosted", OnUntranslatedTextRemainingNotificationPosted);
	}

	private void Start()
	{
		base.enabled = false;
		RevealFact("TH_VILLAGE_X1", saveGame: false);
		CheckForCompletionAchievement();
		_markOnHUDTutorialComplete = PlayerData.GetPersistentCondition("MARK_ON_HUD_TUTORIAL_COMPLETE");
		if (!_markOnHUDTutorialComplete && IsFactRevealed("IP_RING_WORLD_X1"))
		{
			_markOnHUDTutorialActive = true;
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger<float>.RemoveListener("UntranslatedTextRemainingNotificationPosted", OnUntranslatedTextRemainingNotificationPosted);
	}

	public ScreenPrompt.DisplayState GetMarkOnHUDDisplayState()
	{
		if (!_markOnHUDTutorialActive)
		{
			return ScreenPrompt.DisplayState.Normal;
		}
		return ScreenPrompt.DisplayState.Attention;
	}

	public void OnPressMarkOnHUD()
	{
		if (_markOnHUDTutorialActive)
		{
			_markOnHUDTutorialActive = false;
			_markOnHUDTutorialComplete = true;
			PlayerData.SetPersistentCondition("MARK_ON_HUD_TUTORIAL_COMPLETE", state: true);
		}
	}

	public List<ShipLogEntry> GetEntryList()
	{
		return _entryList;
	}

	public ShipLogEntry GetEntry(string id)
	{
		if (!_entryDict.ContainsKey(id))
		{
			return null;
		}
		return _entryDict[id];
	}

	public ShipLogFact GetFact(string id)
	{
		if (!_factDict.ContainsKey(id))
		{
			return null;
		}
		return _factDict[id];
	}

	public List<ShipLogEntry> GetEntriesByAstroBody(string astroObjectID)
	{
		List<ShipLogEntry> list = new List<ShipLogEntry>();
		List<ShipLogEntry> list2 = new List<ShipLogEntry>();
		for (int i = 0; i < _entryList.Count; i++)
		{
			if (_entryList[i].GetAstroObjectID().Equals(astroObjectID))
			{
				if (_entryList[i].HasRevealedParent())
				{
					list2.Add(_entryList[i]);
				}
				else
				{
					list.Add(_entryList[i]);
				}
			}
		}
		ShipLogEntryOrderComparer comparer = new ShipLogEntryOrderComparer();
		list.Sort(comparer);
		list2.Sort(comparer);
		for (int j = 0; j < list2.Count; j++)
		{
			for (int k = 0; k < list.Count; k++)
			{
				if (list[k].GetID().Equals(list2[j].GetParentID()))
				{
					list.Insert(k + 1, list2[j]);
					break;
				}
			}
		}
		return list;
	}

	public Vector2 GetEntryCardPosition(string entryID)
	{
		if (_entryDataDict.ContainsKey(entryID))
		{
			return _entryDataDict[entryID].cardPosition;
		}
		return Vector2.zero;
	}

	public void SetEntryCardPosition(string entryID, Vector2 position)
	{
		if (_entryDataDict.ContainsKey(entryID))
		{
			EntryData value = _entryDataDict[entryID];
			value.cardPosition = position;
			_entryDataDict[entryID] = value;
		}
	}

	public void ClearNewlyRevealedFacts()
	{
		List<string> newlyRevealedFactIDs = PlayerData.GetNewlyRevealedFactIDs();
		for (int i = 0; i < newlyRevealedFactIDs.Count; i++)
		{
			GetFact(newlyRevealedFactIDs[i])?.ClearNewlyRevealed();
		}
		PlayerData.ClearNewlyRevealedFactIDs();
	}

	public bool IsFactRevealed(string id)
	{
		if (_factDict.ContainsKey(id))
		{
			return _factDict[id].IsRevealed();
		}
		Debug.LogError("Failed to find fact ID " + id);
		Debug.Break();
		return false;
	}

	public bool RevealFact(string id, bool saveGame = true, bool showNotification = true)
	{
		if (_factDict.ContainsKey(id))
		{
			ShipLogFact shipLogFact = _factDict[id];
			if (!shipLogFact.IsRevealed())
			{
				ShipLogEntry.State state = GetEntry(shipLogFact.GetEntryID()).GetState();
				shipLogFact.Reveal(_factRevealCount);
				_factRevealCount++;
				Debug.Log("Revealed ship log fact " + id);
				GlobalMessenger.FireEvent("ShipLogUpdated");
				if (!Locator.GetPlayerSuit().IsTrainingSuit() && PlayerData.GetShowShipLogNotifications() && showNotification)
				{
					bool flag = true;
					if (shipLogFact.IsRumor() && GetEntry(shipLogFact.GetEntryID()).GetState() == ShipLogEntry.State.Explored)
					{
						flag = false;
					}
					for (int num = _pendingRevealData.Count - 1; num >= 0; num--)
					{
						if (_pendingRevealData[num].fact.GetEntryID().Equals(shipLogFact.GetEntryID()))
						{
							if (_pendingRevealData[num].fact.IsRumor() && !shipLogFact.IsRumor())
							{
								_pendingRevealData.RemoveAt(num);
							}
							else
							{
								flag = false;
							}
						}
					}
					if (flag)
					{
						FactRevealData item = new FactRevealData(shipLogFact, ConstructFactRevealMessage(id, state));
						_pendingRevealData.Add(item);
						_postNotificationTime = Time.time + Time.deltaTime;
						base.enabled = true;
					}
				}
				if (saveGame)
				{
					PlayerData.SaveCurrentGame();
				}
				CheckForCompletionAchievement();
				return true;
			}
		}
		else
		{
			Debug.LogError("Failed to find fact ID " + id);
			Debug.Break();
		}
		return false;
	}

	public void RevealAllFacts(bool rumorsOnly = false)
	{
		for (int i = 0; i < _factList.Count; i++)
		{
			if (_factList[i].IsRumor() || !rumorsOnly)
			{
				_factList[i].Reveal(_factRevealCount);
				_factRevealCount++;
			}
		}
		GlobalMessenger.FireEvent("ShipLogUpdated");
		CheckForCompletionAchievement();
	}

	private void GenerateEntriesFromXml(TextAsset xml)
	{
		XElement xElement = XDocument.Parse(OWUtilities.RemoveByteOrderMark(xml)).Element("AstroObjectEntry");
		string value = xElement.Element("ID").Value;
		foreach (XElement item in xElement.Elements("Entry"))
		{
			ShipLogEntry entry = new ShipLogEntry(value, item);
			AddEntry(entry);
		}
	}

	private void AddEntry(ShipLogEntry entry)
	{
		_entryList.Add(entry);
		_entryDict.Add(entry.GetID(), entry);
		for (int i = 0; i < entry.GetRumorFacts().Count; i++)
		{
			_factRevealCount = Mathf.Max(_factRevealCount, entry.GetRumorFacts()[i].GetRevealOrder());
			_factList.Add(entry.GetRumorFacts()[i]);
			_factDict.Add(entry.GetRumorFacts()[i].GetID(), entry.GetRumorFacts()[i]);
		}
		for (int j = 0; j < entry.GetExploreFacts().Count; j++)
		{
			_factRevealCount = Mathf.Max(_factRevealCount, entry.GetExploreFacts()[j].GetRevealOrder());
			_factList.Add(entry.GetExploreFacts()[j]);
			_factDict.Add(entry.GetExploreFacts()[j].GetID(), entry.GetExploreFacts()[j]);
		}
		for (int k = 0; k < entry.GetChildren().Count; k++)
		{
			AddEntry(entry.GetChildren()[k]);
		}
	}

	private void Update()
	{
		if (PlayerState.InConversation() || PlayerState.IsViewingProjector() || PlayerState.IsPeeping() || !Locator.GetToolModeSwapper().AllowTranslatorFactRevealNotification())
		{
			_postNotificationTime = Time.time + 1f;
		}
		_postNotificationTime = Mathf.Max(_postNotificationTime, _minPostNotificationTime);
		if (Time.time > _postNotificationTime)
		{
			NotificationManager.SharedInstance.PostNotification(new NotificationData(NotificationTarget.All, UITextLibrary.GetString(UITextType.NotificationLogUpdated), 7f));
			_pendingRevealData.Clear();
			base.enabled = false;
		}
	}

	private string ConstructFactRevealMessage(string factID, ShipLogEntry.State lastState)
	{
		ShipLogFact shipLogFact = _factDict[factID];
		ShipLogEntry entry = GetEntry(shipLogFact.GetEntryID());
		string text = entry.GetName(withLineBreaks: false).ToUpper();
		bool flag = entry.GetState() != lastState;
		if (entry.GetState() == ShipLogEntry.State.Rumored)
		{
			text += " (?)";
		}
		if (flag)
		{
			text += UITextLibrary.GetString(UITextType.NotificationNew);
		}
		return "<color=orange>" + text + "</color>";
	}

	private void OnUntranslatedTextRemainingNotificationPosted(float notificationDuration)
	{
		_minPostNotificationTime = Time.time + notificationDuration;
	}

	private void CheckForCompletionAchievement()
	{
		foreach (KeyValuePair<string, ShipLogFact> item in _factDict)
		{
			if (!item.Value.IsRumor() && !item.Value.IsRevealed() && !item.Key.Equals("TH_VILLAGE_X3") && !item.Key.Equals("GD_GABBRO_ISLAND_X1") && GetEntry(item.Value.GetEntryID()).GetCuriosityName() != CuriosityName.InvisiblePlanet)
			{
				return;
			}
		}
		Achievements.Earn(Achievements.Type.STUDIOUS);
	}
}
