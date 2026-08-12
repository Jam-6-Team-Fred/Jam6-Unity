using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Slide
{
	public Texture2D _image;

	private Texture2D _textureOverride;

	private SlideCollectionContainer _owningItem;

	public bool expanded;

	[SerializeField]
	private int _modulesList;

	[SerializeField]
	private ushort[] lengths;

	[SerializeField]
	private byte[] _modulesData;

	[SerializeField]
	private int _streamingImageID = -1;

	private List<SlideFunctionModule> _runtimeModuleCache;

	public Texture2D textureOverride
	{
		get
		{
			return _textureOverride;
		}
		set
		{
			if (!(_textureOverride == value))
			{
				_textureOverride = value;
				InvokeTextureUpdate();
			}
		}
	}

	public bool invertBlackFrames => _owningItem.invertBlackFrames;

	public Slide()
	{
		_image = null;
		expanded = false;
		_modulesList = 0;
		_modulesData = new byte[0];
		lengths = new ushort[0];
		_streamingImageID = -1;
	}

	public Slide(Slide other)
	{
		_image = other._image;
		expanded = false;
		_modulesList = other._modulesList;
		_modulesData = new byte[other._modulesData.Length];
		Array.Copy(other._modulesData, _modulesData, _modulesData.Length);
		lengths = new ushort[other.lengths.Length];
		Array.Copy(other.lengths, lengths, lengths.Length);
		_streamingImageID = other._streamingImageID;
	}

	public Texture GetTexture()
	{
		if (_textureOverride != null)
		{
			return _textureOverride;
		}
		if (_image != null)
		{
			return _image;
		}
		if (_streamingImageID > -1 && _owningItem != null)
		{
			if (!_owningItem.IsStreamingTextureIDAvailable(_streamingImageID))
			{
				return _owningItem.firstSlideStandIn;
			}
			return _owningItem.GetStreamingTexture(_streamingImageID) as Texture2D;
		}
		return null;
	}

	public int GetStreamingIndex()
	{
		return _streamingImageID;
	}

	public void SetupStreamingIndex(int slideIndex)
	{
		if (_streamingImageID > -1)
		{
			_image = null;
		}
	}

	public void Display(SlideCollectionContainer owner, bool forward)
	{
		if (_runtimeModuleCache == null)
		{
			_runtimeModuleCache = LoadModulesList();
		}
		_owningItem = owner;
		InvokeTextureUpdate();
		for (int i = 0; i < _runtimeModuleCache.Count; i++)
		{
			_runtimeModuleCache[i].EnterSlide(this, forward);
		}
	}

	public void Update(SlideCollectionContainer owner)
	{
		if (_runtimeModuleCache == null)
		{
			_runtimeModuleCache = LoadModulesList();
		}
		for (int i = 0; i < _runtimeModuleCache.Count; i++)
		{
			_runtimeModuleCache[i].Update(this);
		}
	}

	public void EndDisplay(SlideCollectionContainer owner, bool forward)
	{
		if (_runtimeModuleCache != null)
		{
			for (int i = 0; i < _runtimeModuleCache.Count; i++)
			{
				_runtimeModuleCache[i].ExitSlide(this, forward);
			}
			_runtimeModuleCache = null;
		}
	}

	public void InvokeTextureUpdate()
	{
		if (_owningItem != null)
		{
			_owningItem.onSlideTextureUpdated.Invoke();
		}
	}

	public void SetChangeSlidesAllowed(bool allowed)
	{
		_owningItem.SetChangeSlidesAllowed(allowed);
	}

	public void InvokeBounceLightUpdate(LightParameters lightParams)
	{
		if (_owningItem != null)
		{
			_owningItem.onNeedBounceLightUpdate.Invoke(lightParams);
		}
	}

	public void InvokePlayBeatAudio(AudioType audioType)
	{
		if (_owningItem != null)
		{
			_owningItem.onPlayBeatAudio.Invoke(audioType);
		}
	}

	public void RotateToMySection()
	{
		_owningItem.RotateToSection(_streamingImageID);
	}

	public void RotateToPrevSection()
	{
		_owningItem.RotateToPrevSection(_streamingImageID);
	}

	public void SetOwner(SlideCollectionContainer owner)
	{
		_owningItem = owner;
	}

	public bool HasModule(Type moduleType)
	{
		return HasModule(_modulesList, moduleType);
	}

	public static bool HasModule(int modulesList, Type moduleType)
	{
		return (modulesList & SlideFunctionModule.GetByteCodeFromType(moduleType)) > 0;
	}

	public static int CountModules(int modulesList)
	{
		int num = 0;
		while (modulesList > 0)
		{
			num += modulesList & 1;
			modulesList >>= 1;
		}
		return num;
	}

	public List<SlideFunctionModule> LoadModulesList(bool readData = true)
	{
		return LoadModulesList(_modulesList, _modulesData, readData);
	}

	public static List<SlideFunctionModule> LoadModulesList(int list, byte[] data, bool readData = true)
	{
		List<SlideFunctionModule> list2 = new List<SlideFunctionModule>();
		int num = 0;
		for (int num2 = 1; num2 <= 65536; num2 <<= 1)
		{
			if ((num2 & list) > 0)
			{
				SlideFunctionModule slideFunctionModule = SlideFunctionModule.ExtractFromByteCode(num2, data, num, readData);
				if (slideFunctionModule != null)
				{
					list2.Add(slideFunctionModule);
					num += slideFunctionModule.DataSize();
				}
			}
		}
		return list2;
	}

	public SlideFunctionModule GetModule(Type moduleType)
	{
		if (!HasModule(moduleType))
		{
			return null;
		}
		List<SlideFunctionModule> list = null;
		list = ((_runtimeModuleCache == null) ? LoadModulesList() : _runtimeModuleCache);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].GetType() == moduleType)
			{
				return list[i];
			}
		}
		return null;
	}

	public T GetModule<T>() where T : SlideFunctionModule
	{
		return GetModule(typeof(T)) as T;
	}

	public static void RemoveModule(List<SlideFunctionModule> modList, int idx, ref int modulesList, ref byte[] modulesData, ref ushort[] lengths)
	{
		modList[idx].Remove(ref modulesList, ref modulesData, ref lengths);
	}

	public static void AddModule(Type moduleType, ref int moduleList, ref byte[] moduleData, ref ushort[] lengths)
	{
		(moduleType.GetConstructor(new Type[0]).Invoke(new object[0]) as SlideFunctionModule).Write(ref moduleList, ref moduleData, ref lengths);
	}

	public static void WriteModules(List<SlideFunctionModule> modules, ref int moduleList, ref byte[] modulesData, ref ushort[] lengths)
	{
		if (lengths == null)
		{
			lengths = new ushort[modules.Count];
			Debug.Log(lengths.Length);
		}
		if (lengths.Length < modules.Count)
		{
			Array.Resize(ref lengths, modules.Count);
		}
		for (int i = 0; i < modules.Count; i++)
		{
			modules[i].Write(ref moduleList, ref modulesData, ref lengths);
		}
	}

	public static bool CheckIntegrity(List<SlideFunctionModule> modules, ref byte[] dataArray, ref ushort[] lengths)
	{
		if (lengths == null || lengths.Length != modules.Count)
		{
			WriteLengths(modules, ref lengths);
			return false;
		}
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<SlideFunctionModule> list4 = new List<SlideFunctionModule>();
		int num = 0;
		for (int i = 0; i < modules.Count; i++)
		{
			if (modules[i].ModuleIndex() != SlideFunctionModule.GetByteCodeFromType(typeof(SlideShipLogEntryModule)))
			{
				if (lengths[i] != (ushort)modules[i].DataSize())
				{
					list.Add(modules[i].DataSize() - lengths[i]);
					list2.Add(num);
					list3.Add(lengths[i]);
					list4.Add(modules[i]);
					lengths[i] = (ushort)modules[i].DataSize();
				}
				num += modules[i].DataSize();
			}
		}
		if (list.Count <= 0)
		{
			return true;
		}
		for (int j = 0; j < list.Count; j++)
		{
			int num2 = list2[j] + list3[j];
			if (list[j] > 0)
			{
				byte[] array = new byte[list[j]];
				byte[] array2 = new byte[dataArray.Length - num2];
				Buffer.BlockCopy(dataArray, num2, array2, 0, array2.Length);
				Array.Resize(ref dataArray, dataArray.Length + list[j]);
				Buffer.BlockCopy(array, 0, dataArray, num2, array.Length);
				Buffer.BlockCopy(array2, 0, dataArray, num2 + list[j], array2.Length);
			}
			else
			{
				byte[] array3 = new byte[dataArray.Length - num2];
				Buffer.BlockCopy(dataArray, num2, array3, 0, array3.Length);
				Array.Resize(ref dataArray, dataArray.Length + list[j]);
				Buffer.BlockCopy(array3, 0, dataArray, num2 + list[j], array3.Length);
			}
		}
		return false;
	}

	private static void WriteLengths(List<SlideFunctionModule> modules, ref ushort[] lengths)
	{
		lengths = new ushort[modules.Count];
		int num = 0;
		for (int i = 0; i < modules.Count; i++)
		{
			SlideFunctionModule slideFunctionModule = modules[i];
			lengths[num] = (ushort)slideFunctionModule.DataSize();
			num++;
		}
	}

	public static Slide CreateSlide(Texture2D texture)
	{
		Slide slide = new Slide
		{
			_image = texture
		};
		Type[] array = DefaultModules();
		for (int i = 0; i < array.Length; i++)
		{
			AddModule(array[i], ref slide._modulesList, ref slide._modulesData, ref slide.lengths);
		}
		return slide;
	}

	public static SlideFunctionModule GetSingleModule(int modulesList, ushort[] lengths, byte[] byteArray, Type moduleType)
	{
		if (!HasModule(modulesList, moduleType))
		{
			return null;
		}
		int num = 1;
		int num2 = 0;
		int num3 = 0;
		int byteCodeFromType = SlideFunctionModule.GetByteCodeFromType(moduleType);
		while (num != byteCodeFromType)
		{
			if ((modulesList & num) > 0)
			{
				num2 += lengths[num3];
				num3++;
			}
			num <<= 1;
		}
		return SlideFunctionModule.ExtractFromByteCode(byteCodeFromType, byteArray, num2);
	}

	public static void WriteModuleToData(ref int modulesList, ref ushort[] lengths, ref byte[] byteArray, SlideFunctionModule module, bool overwrite = false)
	{
		if (!HasModule(modulesList, module.GetType()) || overwrite)
		{
			module.Write(ref modulesList, ref byteArray, ref lengths);
		}
	}

	public static Type[] DefaultModules()
	{
		return new Type[0];
	}
}
