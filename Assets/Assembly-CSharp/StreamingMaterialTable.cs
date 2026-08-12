using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "StreamingMaterialTable", menuName = "Streaming Material Table", order = 650)]
public class StreamingMaterialTable : ScriptableObject
{
	public enum Type
	{
		General = 0,
		Terrain = 1,
		Detail = 2,
		Character = 3
	}

	[Serializable]
	public struct PropertyLookup // CHANGED
	{
		public string propertyName;

		public int propertyID;

		public int textureIndex;

		public Texture lodTexture;
	}

	[Serializable]
	public struct MaterialPropertyLookup // CHANGED
	{
		public Material material;

		public PropertyLookup[] propertyLookups;
	}

	[SerializeField]
	public string assetBundle;

	[SerializeField]
	public Type type;

	[SerializeField]
	[HideInInspector]
	public MaterialPropertyLookup[] _materialPropertyLookups; // CHANGED

	private bool _propIDsCached;

	public void CachePropertyIDs()
	{
		for (int i = 0; i < _materialPropertyLookups.Length; i++)
		{
			for (int j = 0; j < _materialPropertyLookups[i].propertyLookups.Length; j++)
			{
				int propertyID = Shader.PropertyToID(_materialPropertyLookups[i].propertyLookups[j].propertyName);
				_materialPropertyLookups[i].propertyLookups[j].propertyID = propertyID;
			}
		}
		_propIDsCached = true;
	}

	public void OnTexturesLoaded(StreamingTextureAssetBundle streamingTextureAssetBundle)
	{
		if (!_propIDsCached)
		{
			CachePropertyIDs();
		}
		for (int i = 0; i < _materialPropertyLookups.Length; i++)
		{
			for (int j = 0; j < _materialPropertyLookups[i].propertyLookups.Length; j++)
			{
				int propertyID = _materialPropertyLookups[i].propertyLookups[j].propertyID;
				Texture textureByID = streamingTextureAssetBundle.GetTextureByID(_materialPropertyLookups[i].propertyLookups[j].textureIndex);
				_materialPropertyLookups[i].material.SetTexture(propertyID, textureByID);
			}
		}
	}

	public void OnTexturesUnloaded()
	{
		if (!_propIDsCached)
		{
			CachePropertyIDs();
		}
		for (int i = 0; i < _materialPropertyLookups.Length; i++)
		{
			for (int j = 0; j < _materialPropertyLookups[i].propertyLookups.Length; j++)
			{
				int propertyID = _materialPropertyLookups[i].propertyLookups[j].propertyID;
				Texture lodTexture = _materialPropertyLookups[i].propertyLookups[j].lodTexture;
				_materialPropertyLookups[i].material.SetTexture(propertyID, lodTexture);
			}
		}
	}
}
