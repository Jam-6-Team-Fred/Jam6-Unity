using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioLibrary : ScriptableObject
{
	[Serializable]
	public struct AudioEntry
	{
		public AudioType type;

		public AudioClip[] clips;

		public float volume;

		public AudioEntry(AudioType t, AudioClip[] c)
		{
			type = t;
			clips = c;
			volume = 1f;
		}

		public AudioEntry(AudioType t, AudioClip[] c, float v)
		{
			type = t;
			clips = c;
			volume = v;
		}
	}

	public AudioEntry[] audioEntries;

	public Dictionary<int, AudioEntry> BuildAudioEntryDictionary()
	{
		Dictionary<int, AudioEntry> dictionary = new Dictionary<int, AudioEntry>(audioEntries.Length);
		for (int i = 0; i < audioEntries.Length; i++)
		{
			dictionary.Add((int)audioEntries[i].type, audioEntries[i]);
		}
		return dictionary;
	}
}
