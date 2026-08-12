using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Audio/Audio Manager", 200)]
public class AudioManager : MonoBehaviour
{
	[SerializeField]
	private AudioLibrary _libraryAsset;

	[SerializeField]
	private bool _debugOn;

	private Dictionary<int, AudioLibrary.AudioEntry> _audioLibraryDict;

	private void Awake()
	{
		_audioLibraryDict = _libraryAsset.BuildAudioEntryDictionary();
		LoadAllAudioData();
	}

	private void OnDestroy()
	{
		CleanupAudioData();
	}

	private void OnDeathSequenceComplete()
	{
		UnloadAudioDataPreSceneLoad();
	}

	private void OnMemoryUplinkAssetDump()
	{
		UnloadAudioDataPreSceneLoad();
	}

	private void CleanupAudioData()
	{
		IEnumerator enumerator = _audioLibraryDict.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AudioClip[] clips = ((AudioLibrary.AudioEntry)enumerator.Current).clips;
			for (int i = 0; i < clips.Length; i++)
			{
				if (clips[i] != null && clips[i].loadType == AudioClipLoadType.Streaming)
				{
					clips[i].UnloadAudioData();
				}
			}
		}
	}

	public void UnloadAudioDataPreSceneLoad()
	{
		IEnumerator enumerator = _audioLibraryDict.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AudioClip[] clips = ((AudioLibrary.AudioEntry)enumerator.Current).clips;
			for (int i = 0; i < clips.Length; i++)
			{
				if (clips[i] != null && clips[i].loadState == AudioDataLoadState.Loaded && ((clips[i].loadInBackground && clips[i].preloadAudioData) || clips[i].loadType == AudioClipLoadType.Streaming))
				{
					clips[i].UnloadAudioData();
				}
			}
		}
	}

	private void LoadAllAudioData()
	{
		IEnumerator enumerator = _audioLibraryDict.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AudioClip[] clips = ((AudioLibrary.AudioEntry)enumerator.Current).clips;
			for (int i = 0; i < clips.Length; i++)
			{
				if (clips[i] != null && clips[i].loadInBackground && clips[i].preloadAudioData)
				{
					clips[i].LoadAudioData();
				}
			}
		}
	}

	public AudioLibrary.AudioEntry GetAudioEntry(AudioType type)
	{
		if (_audioLibraryDict.TryGetValue((int)type, out var value))
		{
			return value;
		}
		Debug.LogError("Failed to find AudioEntry: " + type);
		Debug.Break();
		return value;
	}

	public AudioClip GetSingleAudioClip(AudioType type, bool getRandomClip = true)
	{
		if (_debugOn)
		{
			Debug.Log("Request AudioType: " + type);
		}
		if (_audioLibraryDict.TryGetValue((int)type, out var value))
		{
			if (value.clips.Length == 0)
			{
				return null;
			}
			if (value.clips.Length == 1 || !getRandomClip)
			{
				return value.clips[0];
			}
			return value.clips[Random.Range(0, value.clips.Length)];
		}
		return null;
	}

	public AudioClip[] GetAudioClipArray(AudioType type)
	{
		if (_debugOn)
		{
			Debug.Log("Request AudioType: " + type);
		}
		if (_audioLibraryDict.TryGetValue((int)type, out var value))
		{
			return value.clips;
		}
		Debug.Log("Failed to find audio type: " + type);
		return null;
	}
}
