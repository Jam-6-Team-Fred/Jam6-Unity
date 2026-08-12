using UnityEngine;

public static class IconGenerator
{
	public static Texture2D GenerateSolidRect(int width, int height, Color color)
	{
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				texture2D.SetPixel(i, j, color);
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D GenerateCrosshair(int width, int height, Color color, int lineWidth = 1)
	{
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if ((i <= width / 2 && i > width / 2 - lineWidth) || (j <= height / 2 && j > height / 2 - lineWidth))
				{
					texture2D.SetPixel(i, j, color);
				}
				else
				{
					texture2D.SetPixel(i, j, Color.clear);
				}
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D GenerateSquareBracket(int width, int height, Color color, int lineWidth = 1, float bracketRatio = 0.25f)
	{
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);
		for (int i = 0; i < width; i++)
		{
			if (i < lineWidth || i >= width - lineWidth)
			{
				for (int j = 0; j < height; j++)
				{
					if ((float)j < (float)height * bracketRatio || (float)j > (float)height * (1f - bracketRatio))
					{
						texture2D.SetPixel(i, j, color);
					}
					else
					{
						texture2D.SetPixel(i, j, Color.clear);
					}
				}
			}
			else if ((float)i < (float)width * bracketRatio || (float)i > (float)width * (1f - bracketRatio))
			{
				for (int k = 0; k < height; k++)
				{
					if (k < lineWidth || k >= height - lineWidth)
					{
						texture2D.SetPixel(i, k, color);
					}
					else
					{
						texture2D.SetPixel(i, k, Color.clear);
					}
				}
			}
			else
			{
				texture2D.SetPixel(i, 0, Color.clear);
				texture2D.SetPixel(i, height - 1, Color.clear);
				for (int l = 0; l < height; l++)
				{
					texture2D.SetPixel(i, l, Color.clear);
				}
			}
		}
		texture2D.Apply();
		return texture2D;
	}
}
