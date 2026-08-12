using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class NomaiText : SectoredMonoBehaviour, ILateInitializer
{
	public enum Location
	{
		UNSPECIFIED = 0,
		A = 1,
		B = 2
	}

	protected struct NomaiTextData
	{
		public int ID;

		public int ParentID;

		public XmlNode TextNode;

		public bool IsTranslated;

		public Location SpeakerLocation;

		public bool DefaultFontOverride;

		public NomaiTextData(int id, int parentID, XmlNode textNode, bool translated, Location speakerLocation = Location.UNSPECIFIED, bool defaultFontOverride = false)
		{
			ID = id;
			TextNode = textNode;
			ParentID = parentID;
			IsTranslated = translated;
			SpeakerLocation = speakerLocation;
			DefaultFontOverride = defaultFontOverride;
		}
	}

	protected struct NomaiTextConditionData
	{
		public string DatabaseID;

		public int[][] ConditionBlock;

		public Location LocationCondition;

		public NomaiTextConditionData(string id, int[][] conditionBlock, Location locationCondition)
		{
			DatabaseID = id;
			ConditionBlock = conditionBlock;
			LocationCondition = locationCondition;
		}
	}

	[SerializeField]
	protected float _interactRange = 5f;

	[SerializeField]
	protected TextAsset _nomaiTextAsset;

	[SerializeField]
	protected Location _location;

	protected bool _initialized;

	protected IDictionary<int, NomaiTextData> _dictNomaiTextData;

	protected List<NomaiTextConditionData> _listDBConditions;

	protected float _minimumReadableDistance = 1f;

	protected override void Awake()
	{
		base.Awake();
		_initialized = false;
		LateInitializerManager.RegisterLateInitializer(this);
		if (_interactRange > 25f)
		{
			Debug.LogWarning("Nomai Translator Max Interact Range is less than the interact range of this NomaiText!", this);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (!_initialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
	}

	public virtual bool CheckTurnOffFlashlight()
	{
		return false;
	}

	public virtual void LateInitialize()
	{
		_initialized = true;
		_dictNomaiTextData = new Dictionary<int, NomaiTextData>(ComparerLibrary.intEqComparer);
		_listDBConditions = new List<NomaiTextConditionData>();
		InitializeXmlAsset();
	}

	public void VerifyInitialized()
	{
		if (!_initialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
			LateInitialize();
		}
	}

	protected virtual void InitializeXmlAsset()
	{
		if (_nomaiTextAsset == null)
		{
			Debug.LogWarning("NomaiText does not have a TextAsset!", this);
		}
		else
		{
			LoadXml();
		}
	}

	public virtual bool CheckAllowFocus(float focusDistance, Vector3 focusDirection)
	{
		return focusDistance <= _interactRange;
	}

	public void SetTextAsset(TextAsset textasset)
	{
		_nomaiTextAsset = textasset;
	}

	public virtual void SetNewXmlData(XmlNode rootNode)
	{
		VerifyInitialized();
		LoadTextXml(rootNode);
	}

	public virtual void SetAsTranslated(int id)
	{
		VerifyInitialized();
		if (_dictNomaiTextData.ContainsKey(id))
		{
			if (!_dictNomaiTextData[id].IsTranslated)
			{
				NomaiTextData value = _dictNomaiTextData[id];
				value.IsTranslated = true;
				_dictNomaiTextData[id] = value;
				CheckSetDatabaseCondition();
			}
		}
		else
		{
			Debug.LogError("ID does not exist, cannot set as translated");
		}
	}

	public virtual bool UseDefaultFontOverride(int id)
	{
		VerifyInitialized();
		if (_dictNomaiTextData.ContainsKey(id))
		{
			return _dictNomaiTextData[id].DefaultFontOverride;
		}
		return false;
	}

	protected void CheckSetDatabaseCondition()
	{
		VerifyInitialized();
		for (int i = 0; i < _listDBConditions.Count; i++)
		{
			if ((_listDBConditions[i].LocationCondition != 0 && _listDBConditions[i].LocationCondition != _location) || 1 == 0)
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < _listDBConditions[i].ConditionBlock.Length; j++)
			{
				bool flag2 = false;
				for (int k = 0; k < _listDBConditions[i].ConditionBlock[j].Length; k++)
				{
					int key = _listDBConditions[i].ConditionBlock[j][k];
					if (_dictNomaiTextData.ContainsKey(key) && _dictNomaiTextData[key].IsTranslated)
					{
						flag2 = true;
					}
				}
				if (!flag2 && _listDBConditions[i].ConditionBlock[j].Length != 0)
				{
					flag = false;
				}
			}
			if (flag)
			{
				Locator.GetShipLogManager().RevealFact(_listDBConditions[i].DatabaseID);
			}
		}
	}

	public virtual int GetNumTextBlocksTranslated()
	{
		VerifyInitialized();
		int num = 0;
		IEnumerator<KeyValuePair<int, NomaiTextData>> enumerator = _dictNomaiTextData.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.IsTranslated)
			{
				num++;
			}
		}
		return num;
	}

	public virtual bool IsTranslated(int id)
	{
		VerifyInitialized();
		if (_dictNomaiTextData.ContainsKey(id))
		{
			return _dictNomaiTextData[id].IsTranslated;
		}
		return false;
	}

	public virtual int GetNumTextBlocks()
	{
		VerifyInitialized();
		return _dictNomaiTextData.Count;
	}

	protected virtual void LoadXml()
	{
		string xml = OWUtilities.RemoveByteOrderMark(_nomaiTextAsset);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xml);
		XmlNode xmlNode = xmlDocument.SelectSingleNode("NomaiObject");
		LoadTextXml(xmlNode);
		XmlNodeList xmlNodeList = xmlNode.SelectNodes("ShipLogConditions");
		_listDBConditions.Clear();
		foreach (XmlNode item in xmlNodeList)
		{
			LoadDBConditionsXml(item);
		}
	}

	protected void LoadTextXml(XmlNode rootNode)
	{
		_dictNomaiTextData.Clear();
		foreach (XmlNode item in rootNode.SelectNodes("TextBlock"))
		{
			int result = -1;
			XmlNode xmlNode2 = item.SelectSingleNode("ID");
			if (xmlNode2 == null)
			{
				XmlNode xmlNode3 = item.SelectSingleNode("Text");
				break;
			}
			if (int.TryParse(xmlNode2.InnerText, out result))
			{
				XmlNode xmlNode3 = item.SelectSingleNode("Text");
				XmlNode xmlNode4 = item.SelectSingleNode("ParentID");
				int result2 = -1;
				if (xmlNode4 != null && !int.TryParse(xmlNode4.InnerText, out result2))
				{
					result2 = -1;
				}
				Location speakerLocation = Location.UNSPECIFIED;
				if (item.SelectSingleNode("LocationA") != null)
				{
					speakerLocation = Location.A;
				}
				else if (item.SelectSingleNode("LocationB") != null)
				{
					speakerLocation = Location.B;
				}
				bool defaultFontOverride = item.SelectSingleNode("DefaultFontOverride") != null;
				NomaiTextData value = new NomaiTextData(result, result2, xmlNode3, translated: false, speakerLocation, defaultFontOverride);
				_dictNomaiTextData.Add(result, value);
			}
			else
			{
				Debug.LogError("NomaiText ID parse error");
			}
		}
	}

	protected void LoadDBConditionsXml(XmlNode rootNode)
	{
		XmlNodeList xmlNodeList = rootNode.SelectNodes("RevealFact");
		Location locationCondition = Location.UNSPECIFIED;
		if (rootNode.SelectSingleNode("LocationA") != null)
		{
			locationCondition = Location.A;
		}
		else if (rootNode.SelectSingleNode("LocationB") != null)
		{
			locationCondition = Location.B;
		}
		foreach (XmlNode item2 in xmlNodeList)
		{
			XmlNode xmlNode2 = item2.SelectSingleNode("FactID");
			if (xmlNode2 == null)
			{
				Debug.LogWarning("Nomai Text xml " + _nomaiTextAsset.name + " RevealFact does not have FactID defined. Ignoring.");
				continue;
			}
			string innerText = xmlNode2.InnerText;
			XmlNodeList xmlNodeList2 = item2.SelectNodes("Condition");
			int[][] array;
			if (xmlNodeList2 == null || xmlNodeList2.Count == 0)
			{
				array = new int[0][];
			}
			else
			{
				array = new int[xmlNodeList2.Count][];
				int num = 0;
				foreach (XmlNode item3 in xmlNodeList2)
				{
					string[] array2 = item3.InnerText.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					int[] array3 = new int[array2.Length];
					for (int i = 0; i < array2.Length; i++)
					{
						array2[i] = array2[i].Trim();
						if (!int.TryParse(array2[i], out array3[i]))
						{
							Debug.LogWarning("Nomai Text xml " + _nomaiTextAsset.name + " Could not parse IDs for RevealFact");
							array3[i] = 0;
						}
					}
					array[num] = array3;
					num++;
				}
			}
			NomaiTextConditionData item = new NomaiTextConditionData(innerText, array, locationCondition);
			_listDBConditions.Add(item);
		}
	}

	public float GetMinimumReadableDistance()
	{
		return _minimumReadableDistance;
	}

	public string GetTextNode()
	{
		return GetTextNode(-1);
	}

	public virtual string GetTextNode(int id)
	{
		VerifyInitialized();
		if (_dictNomaiTextData.ContainsKey(id))
		{
			return TextTranslation.Translate(_dictNomaiTextData[id].TextNode.InnerText);
		}
		return null;
	}

	public virtual bool IsAnimationPlaying()
	{
		return false;
	}
}
