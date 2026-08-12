using UnityEngine;

public struct ColorHSV
{
	public float h;

	public float s;

	public float v;

	public float a;

	public ColorHSV(float h, float s, float v, float a = 1f)
	{
		this.h = h;
		this.s = s;
		this.v = v;
		this.a = a;
	}

	public ColorHSV(Color colorRGB)
	{
		float num = Mathf.Min(Mathf.Min(colorRGB.r, colorRGB.g), colorRGB.b);
		float num2 = Mathf.Max(Mathf.Max(colorRGB.r, colorRGB.g), colorRGB.b);
		float num3 = num2 - num;
		v = num2;
		a = colorRGB.a;
		if (!Mathf.Approximately(num2, 0f))
		{
			s = num3 / num2;
			if (Mathf.Approximately(num, num2))
			{
				v = num2;
				s = 0f;
				h = -1f;
				return;
			}
			if (colorRGB.r == num2)
			{
				h = (colorRGB.g - colorRGB.b) / num3;
			}
			else if (colorRGB.g == num2)
			{
				h = 2f + (colorRGB.b - colorRGB.r) / num3;
			}
			else
			{
				h = 4f + (colorRGB.r - colorRGB.g) / num3;
			}
			h *= 60f;
			if (h < 0f)
			{
				h += 360f;
			}
		}
		else
		{
			s = 0f;
			h = -1f;
		}
	}

	public static ColorHSV Lerp(ColorHSV c1, ColorHSV c2, float u)
	{
		return new ColorHSV(Mathf.Lerp(c1.h, c2.h, u), Mathf.Lerp(c1.s, c2.s, u), Mathf.Lerp(c1.v, c2.v, u));
	}

	public Color ToColorRGB()
	{
		if (s == 0f)
		{
			return new Color(v, v, v, a);
		}
		float num = h % 360f / 60f;
		int num2 = (int)num;
		float num3 = num - (float)num2;
		float num4 = v;
		float num5 = num4 * (1f - s);
		float num6 = num4 * (1f - s * num3);
		float num7 = num4 * (1f - s * (1f - num3));
		Color result = new Color(0f, 0f, 0f, a);
		switch (num2)
		{
		case 0:
			result.r = num4;
			result.g = num7;
			result.b = num5;
			break;
		case 1:
			result.r = num6;
			result.g = num4;
			result.b = num5;
			break;
		case 2:
			result.r = num5;
			result.g = num4;
			result.b = num7;
			break;
		case 3:
			result.r = num5;
			result.g = num6;
			result.b = num4;
			break;
		case 4:
			result.r = num7;
			result.g = num5;
			result.b = num4;
			break;
		default:
			result.r = num4;
			result.g = num5;
			result.b = num6;
			break;
		}
		return result;
	}

	public override string ToString()
	{
		return $"h: {h:0.00}, s: {s:0.00}, v: {v:0.00}, a: {a:0.00}";
	}
}
