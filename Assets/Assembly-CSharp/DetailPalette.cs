using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "DetailPalette", menuName = "Detail Palette", order = 650)]
public class DetailPalette : ScriptableObject
{
	public DetailPrototype[] detailPrototypes = new DetailPrototype[0];

	public float GetDensitySum()
	{
		float num = 0f;
		for (int i = 0; i < detailPrototypes.Length; i++)
		{
			num += Mathf.Max(detailPrototypes[i].density, 0f);
		}
		return num;
	}

	public float[] GetDensityIndexLookup()
	{
		float densitySum = GetDensitySum();
		float[] array = new float[detailPrototypes.Length];
		for (int i = 0; i < detailPrototypes.Length; i++)
		{
			array[i] = detailPrototypes[i].density / densitySum + ((i == 0) ? 0f : array[i - 1]);
		}
		return array;
	}
}
