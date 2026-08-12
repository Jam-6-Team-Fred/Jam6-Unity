using UnityEngine;

public class TextToTexture
{
	private const int ASCII_START_OFFSET = 32;

	private Font customFont;

	private Texture2D fontTexture;

	private int fontCountX;

	private int fontCountY;

	private float[] kerningValues;

	private bool supportSpecialCharacters;

	public TextToTexture(Font customFont, int fontCountX, int fontCountY)
	{
		this.customFont = customFont;
		fontTexture = (Texture2D)customFont.material.mainTexture;
		this.fontCountX = fontCountX;
		this.fontCountY = fontCountY;
		kerningValues = DefaultCharacterKerning();
		supportSpecialCharacters = true;
	}

	public Texture2D CreateTextToTexture(string text, int textPlacementX, int textPlacementY, int textureSize, float characterSize, float lineSpacing)
	{
		Texture2D texture2D = CreatefillTexture2D(Color.clear, textureSize, textureSize);
		int num = fontTexture.width / fontCountX;
		int num2 = fontTexture.height / fontCountY;
		int num3 = (int)((float)num * characterSize);
		int num4 = (int)((float)num2 * characterSize);
		float num5 = textPlacementX;
		float num6 = textPlacementY;
		bool flag = false;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			flag = false;
			if (c == '\r')
			{
				num6 -= (float)num4 * lineSpacing;
				num5 = textPlacementX;
				if (i + 1 < text.Length && text[i + 1] == ' ')
				{
					i++;
				}
			}
			if (c == '\\' && supportSpecialCharacters)
			{
				flag = true;
				if (i + 1 < text.Length)
				{
					i++;
					c = text[i];
					switch (c)
					{
					case 'n':
					case 'r':
						num6 -= (float)num4 * lineSpacing;
						num5 = textPlacementX;
						if (i + 1 < text.Length && text[i + 1] == ' ')
						{
							i++;
						}
						break;
					case 't':
						num5 += (float)num3 * GetKerningValue(' ') * 5f;
						break;
					case '\\':
						flag = false;
						break;
					}
				}
			}
			if (!flag && customFont.HasCharacter(c))
			{
				Vector2 characterGridPosition = GetCharacterGridPosition(c);
				characterGridPosition.x *= num;
				characterGridPosition.y *= num2;
				Color[] pixels = fontTexture.GetPixels((int)characterGridPosition.x, fontTexture.height - (int)characterGridPosition.y - num2, num, num2);
				pixels = changeDimensions(pixels, num, num2, num3, num4);
				texture2D = AddPixelsToTextureIfClear(texture2D, pixels, (int)num5, (int)num6, num3, num4);
				float kerningValue = GetKerningValue(c);
				num5 += (float)num3 * kerningValue;
			}
			else if (!flag)
			{
				Debug.Log("Letter Not Found:" + c);
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public int CalcTextWidthPlusTrailingBuffer(string text, int decalTextureSize, float characterSize)
	{
		float num = 0f;
		int num2 = (int)((float)(fontTexture.width / fontCountX) * characterSize);
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			num = ((i >= text.Length - 1) ? (num + (float)num2) : (num + (float)num2 * GetKerningValue(c)));
		}
		return (int)num;
	}

	private Color[] changeDimensions(Color[] originalColors, int originalWidth, int originalHeight, int newWidth, int newHeight)
	{
		Color[] array;
		if (originalWidth == newWidth && originalHeight == newHeight)
		{
			array = originalColors;
		}
		else
		{
			array = new Color[newWidth * newHeight];
			Texture2D texture2D = new Texture2D(originalWidth, originalHeight);
			texture2D.SetPixels(originalColors);
			for (int i = 0; i < newHeight; i++)
			{
				for (int j = 0; j < newWidth; j++)
				{
					int num = j + i * newWidth;
					float u = (float)j / (float)newWidth;
					float v = (float)i / (float)newHeight;
					array[num] = texture2D.GetPixelBilinear(u, v);
				}
			}
		}
		return array;
	}

	private float[] DefaultCharacterKerning()
	{
		double[] array = new double[95]
		{
			0.201, 0.201, 0.256, 0.401, 0.401, 0.641, 0.481, 0.138, 0.24, 0.24,
			0.281, 0.421, 0.201, 0.24, 0.201, 0.201, 0.401, 0.353, 0.401, 0.401,
			0.401, 0.401, 0.401, 0.401, 0.401, 0.401, 0.201, 0.201, 0.421, 0.421,
			0.421, 0.401, 0.731, 0.481, 0.481, 0.52, 0.481, 0.481, 0.44, 0.561,
			0.52, 0.201, 0.36, 0.481, 0.401, 0.6, 0.52, 0.561, 0.481, 0.561,
			0.52, 0.481, 0.44, 0.52, 0.481, 0.68, 0.481, 0.481, 0.44, 0.201,
			0.201, 0.201, 0.338, 0.401, 0.24, 0.401, 0.401, 0.36, 0.401, 0.401,
			0.189, 0.401, 0.401, 0.16, 0.16, 0.36, 0.16, 0.6, 0.401, 0.401,
			0.401, 0.401, 0.24, 0.36, 0.201, 0.401, 0.36, 0.52, 0.36, 0.36,
			0.36, 0.241, 0.188, 0.241, 0.421
		};
		float[] array2 = new float[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = (float)array[i];
		}
		return array2;
	}

	private Texture2D AddPixelsToTextureIfClear(Texture2D texture, Color[] newPixels, int placementX, int placementY, int placementWidth, int placementHeight)
	{
		int num = 0;
		if (placementX + placementWidth < texture.width)
		{
			Color[] pixels = texture.GetPixels(placementX, placementY, placementWidth, placementHeight);
			for (int i = 0; i < placementHeight; i++)
			{
				for (int j = 0; j < placementWidth; j++)
				{
					num = j + i * placementWidth;
					if (pixels[num] != Color.clear)
					{
						newPixels[num] = pixels[num];
					}
				}
			}
			texture.SetPixels(placementX, placementY, placementWidth, placementHeight, newPixels);
		}
		else
		{
			Debug.Log("Letter Falls Outside Bounds of Texture" + (placementX + placementWidth));
		}
		return texture;
	}

	private Vector2 GetCharacterGridPosition(char c)
	{
		int num = c - 32;
		return new Vector2(num % fontCountX, num / fontCountX);
	}

	private float GetKerningValue(char c)
	{
		return kerningValues[c - 32];
	}

	private Texture2D CreatefillTexture2D(Color color, int textureWidth, int textureHeight)
	{
		Texture2D texture2D = new Texture2D(textureWidth, textureHeight);
		int num = texture2D.width * texture2D.height;
		Color[] array = new Color[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = color;
		}
		texture2D.SetPixels(array);
		return texture2D;
	}

	private float[] GetCharacterKerningValuesFromPerCharacterKerning(PerCharacterKerning[] perCharacterKerning)
	{
		float[] array = new float[96];
		foreach (PerCharacterKerning perCharacterKerning2 in perCharacterKerning)
		{
			if (perCharacterKerning2.First != "")
			{
				int @char = perCharacterKerning2.GetChar();
				if (@char >= 0 && @char - 32 < array.Length)
				{
					array[@char - 32] = perCharacterKerning2.GetKerningValue();
				}
			}
		}
		return array;
	}
}
