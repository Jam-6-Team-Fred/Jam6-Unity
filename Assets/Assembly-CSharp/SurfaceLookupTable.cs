using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SurfaceLookupTable : ScriptableObject
{
	[Serializable]
	public struct SurfaceTypeGroup
	{
		public SurfaceType type;

		public Material[] materials;
	}

	public SurfaceTypeGroup[] surfaceTypeGroups;

	public Dictionary<Material, SurfaceType> BuildDictionary()
	{
		Dictionary<Material, SurfaceType> dictionary = new Dictionary<Material, SurfaceType>();
		for (int i = 0; i < surfaceTypeGroups.Length; i++)
		{
			for (int j = 0; j < surfaceTypeGroups[i].materials.Length; j++)
			{
				dictionary.Add(surfaceTypeGroups[i].materials[j], surfaceTypeGroups[i].type);
			}
		}
		return dictionary;
	}
}
