using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShipLogLibrary : ScriptableObject
{
	public Sprite defaultEntrySprite;

	public EntryData[] entryData;

	public Dictionary<string, EntryData> BuildEntryDataDictionary()
	{
		Dictionary<string, EntryData> dictionary = new Dictionary<string, EntryData>();
		for (int i = 0; i < entryData.Length; i++)
		{
			dictionary.Add(entryData[i].id, entryData[i]);
		}
		return dictionary;
	}
}
