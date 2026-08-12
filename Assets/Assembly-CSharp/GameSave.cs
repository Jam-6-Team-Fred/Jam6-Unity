using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class GameSave
{
	public const string c_noSavedVersion = "NONE";

	public int loopCount = 1;

	public bool[] knownFrequencies = new bool[7] { true, true, false, false, false, false, false };

	public Dictionary<int, bool> knownSignals;

	public Dictionary<string, bool> dictConditions;

	public Dictionary<string, ShipLogFactSave> shipLogFactSaves;

	public List<string> newlyRevealedFactIDs;

	public DeathType lastDeathType;

	[OptionalField(VersionAdded = 2)]
	public int burnedMarshmallowEaten;

	[OptionalField(VersionAdded = 3)]
	public uint fullTimeloops;

	[OptionalField(VersionAdded = 3)]
	public uint perfectMarshmallowsEaten;

	[OptionalField(VersionAdded = 4)]
	public bool warpedToTheEye;

	[OptionalField(VersionAdded = 4)]
	public float secondsRemainingOnWarp;

	[OptionalField(VersionAdded = 5)]
	public int loopCountOnParadox;

	[OptionalField(VersionAdded = 6)]
	public StartupPopups shownPopups;

	[OptionalField(VersionAdded = 6)]
	public string version;

	[OptionalField(VersionAdded = 7)]
	public bool ps5Activity_canResumeExpedition;

	[OptionalField(VersionAdded = 7)]
	public List<string> ps5Activity_availableShipLogCards;

	[OptionalField(VersionAdded = 7)]
	[FormerlySerializedAs("runInitGammaSetting")]
	public bool didRunInitGammaSetting;

	[OnDeserializing]
	private void SetDefaultValuesOnDeserializing(StreamingContext context)
	{
		loopCountOnParadox = 0;
		shownPopups = StartupPopups.None;
		version = "NONE";
		ps5Activity_canResumeExpedition = false;
		ps5Activity_availableShipLogCards = new List<string>();
		didRunInitGammaSetting = true;
	}

	[OnDeserialized]
	private void SetDefaultValuesOnDeserialized(StreamingContext context)
	{
		if (knownFrequencies.Length < 7)
		{
			Array.Resize(ref knownFrequencies, 7);
		}
	}

	public GameSave()
	{
		dictConditions = new Dictionary<string, bool>(ComparerLibrary.stringEqComparer);
		shipLogFactSaves = new Dictionary<string, ShipLogFactSave>(ComparerLibrary.stringEqComparer);
		newlyRevealedFactIDs = new List<string>();
		knownSignals = new Dictionary<int, bool>
		{
			{ 31, false },
			{ 30, false },
			{ 32, false },
			{ 62, false },
			{ 60, false },
			{ 61, false },
			{ 23, false },
			{ 20, false },
			{ 24, false },
			{ 22, false },
			{ 21, false },
			{ 25, false },
			{ 11, false },
			{ 10, false },
			{ 14, false },
			{ 13, false },
			{ 12, false },
			{ 15, false },
			{ 16, false },
			{ 40, false },
			{ 43, false },
			{ 42, false },
			{ 49, false },
			{ 41, false },
			{ 46, false },
			{ 44, false },
			{ 48, false },
			{ 47, false },
			{ 45, false },
			{ 100, false },
			{ 101, false }
		};
		burnedMarshmallowEaten = 0;
		fullTimeloops = 0u;
		perfectMarshmallowsEaten = 0u;
		ps5Activity_canResumeExpedition = false;
		ps5Activity_availableShipLogCards = new List<string>();
		didRunInitGammaSetting = false;
		version = Application.version;
	}

	public void SetPersistentCondition(string condition, bool state)
	{
		if (dictConditions.ContainsKey(condition))
		{
			dictConditions[condition] = state;
		}
		else
		{
			dictConditions.Add(condition, state);
		}
	}

	public bool GetPersistentCondition(string condition)
	{
		if (dictConditions.ContainsKey(condition))
		{
			return dictConditions[condition];
		}
		return false;
	}

	public bool PersistentConditionExists(string condition)
	{
		return dictConditions.ContainsKey(condition);
	}

	public XmlDocument GetXmlDocument()
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlNode xmlNode = xmlDocument.CreateNode("GameSave");
		XmlNode xmlNode2 = xmlDocument.CreateNode("LoopCount");
		xmlNode2.SetValue(XmlConvert.ToString(loopCount));
		XmlNode xmlNode3 = xmlDocument.CreateNode("BurnedMarshmallowEaten");
		xmlNode3.SetValue(XmlConvert.ToString(burnedMarshmallowEaten));
		xmlDocument.CreateNode("PerfectMarshmallowsEaten").SetValue(XmlConvert.ToString(perfectMarshmallowsEaten));
		xmlDocument.CreateNode("FullTimeloops").SetValue(XmlConvert.ToString(fullTimeloops));
		XmlNode xmlNode4 = xmlDocument.CreateNode("WarpedToTheEye");
		xmlNode4.SetValue(XmlConvert.ToString(warpedToTheEye));
		XmlNode xmlNode5 = xmlDocument.CreateNode("SecondsRemainingOnWarp");
		xmlNode5.SetValue(XmlConvert.ToString(secondsRemainingOnWarp));
		XmlNode xmlNode6 = xmlDocument.CreateNode("KnownFrequencies");
		for (int i = 0; i < knownFrequencies.Length; i++)
		{
			XmlNode xmlNode7 = xmlDocument.CreateNode("Known");
			xmlNode7.SetValue(XmlConvert.ToString(knownFrequencies[i]));
			xmlNode6.AppendChild(xmlNode7);
		}
		XmlNode xmlNode8 = xmlDocument.CreateNode("ConditionDict");
		foreach (KeyValuePair<string, bool> dictCondition in dictConditions)
		{
			XmlNode xmlNode9 = xmlDocument.CreateNode("ConditionPair");
			XmlNode xmlNode10 = xmlDocument.CreateNode("Key");
			XmlNode xmlNode11 = xmlDocument.CreateNode("Value");
			xmlNode10.SetValue(dictCondition.Key);
			xmlNode11.SetValue(XmlConvert.ToString(dictCondition.Value));
			xmlNode9.AppendChild(xmlNode10);
			xmlNode9.AppendChild(xmlNode11);
			xmlNode8.AppendChild(xmlNode9);
		}
		xmlNode.AppendChild(xmlNode2);
		xmlNode.AppendChild(xmlNode3);
		xmlNode.AppendChild(xmlNode6);
		xmlNode.AppendChild(xmlNode8);
		xmlNode.AppendChild(xmlNode4);
		xmlNode.AppendChild(xmlNode5);
		xmlDocument.AppendChild(xmlNode);
		return xmlDocument;
	}

	public void SetXmlDocumentData(XmlDocument document)
	{
		XmlNode xmlNode = document.SelectSingleNode("GameSave");
		XmlNode xmlNode2 = xmlNode.SelectSingleNode("LoopCount");
		loopCount = Convert.ToInt32(xmlNode2.GetValue());
		XmlNode xmlNode3 = xmlNode.SelectSingleNode("BurnedMarshmallowEaten");
		burnedMarshmallowEaten = Convert.ToInt32(xmlNode3.GetValue());
		XmlNode xmlNode4 = xmlNode.SelectSingleNode("PerfectMarshmallowsEaten");
		perfectMarshmallowsEaten = Convert.ToUInt32(xmlNode4.GetValue());
		XmlNode xmlNode5 = xmlNode.SelectSingleNode("FullTimeloops");
		fullTimeloops = Convert.ToUInt32(xmlNode5.GetValue());
		XmlNode xmlNode6 = xmlNode.SelectSingleNode("WarpedToTheEye");
		warpedToTheEye = Convert.ToBoolean(xmlNode6.GetValue());
		XmlNode xmlNode7 = xmlNode.SelectSingleNode("SecondsRemainingOnWarp");
		secondsRemainingOnWarp = Convert.ToSingle(xmlNode7.GetValue());
		XmlNodeList xmlNodeList = xmlNode.SelectSingleNode("KnownFrequencies").SelectNodes("Known");
		for (int i = 0; i < xmlNodeList.Count; i++)
		{
			if (XmlConvert.ToBoolean(xmlNodeList[i].GetValue()))
			{
				knownFrequencies[i] = true;
			}
			else
			{
				knownFrequencies[i] = false;
			}
		}
		dictConditions.Clear();
		XmlNodeList xmlNodeList2 = xmlNode.SelectSingleNode("ConditionDict").SelectNodes("ConditionPair");
		for (int j = 0; j < xmlNodeList2.Count; j++)
		{
			XmlNode xmlNode8 = xmlNodeList2[j];
			string value = xmlNode8.SelectSingleNode("Key").GetValue();
			if (XmlConvert.ToBoolean(xmlNode8.SelectSingleNode("Value").GetValue()))
			{
				SetPersistentCondition(value, state: true);
			}
			else
			{
				SetPersistentCondition(value, state: false);
			}
		}
	}

	public string ToJson()
	{
		return JsonUtility.ToJson(this);
	}

	public static GameSave FromJson(string json)
	{
		try
		{
			return JsonUtility.FromJson<GameSave>(json);
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not load game save: " + ex.Message);
			return null;
		}
	}
}
