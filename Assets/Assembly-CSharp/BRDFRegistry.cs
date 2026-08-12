using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "BRDFRegistry", menuName = "BRDF Registry", order = 650)]
public class BRDFRegistry : ScriptableObject
{
	public enum BRDFLookupID
	{
		Geode = 0,
		GravityCrystal = 1,
		NomaiGlass = 2,
		Ice = 3,
		DarkMatter = 4,
		Unused6 = 5,
		StrangerGlass = 6,
		Unused8 = 7
	}

	public const int kNumBRDFs = 8;

	public Color[] brdfSpecColors = new Color[8];

	public Texture2D[] brdfLookups = new Texture2D[8];

	public Texture2DArray brdfLookupArray;

	public void UpdateBRDFs()
	{
		Vector4[] array = new Vector4[8];
		for (int i = 0; i < 8; i++)
		{
			array[i] = brdfSpecColors[i];
		}
		Shader.SetGlobalVectorArray("_OW_BRDFSpecColorArray", array);
		Shader.SetGlobalTexture("_OW_BRDFTextureArray", brdfLookupArray);
	}
}
