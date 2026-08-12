using UnityEngine;

public class Glyph
{
	public enum GlyphRotation
	{
		NO_ROTATION = 0,
		ROT_CW_90 = 1,
		ROT_180 = 2,
		ROT_CW_270 = 3
	}

	public int x;

	public int y;

	public GlyphRotation rotation;

	public Vector2 MaxUV1Coord { get; set; }

	public Vector2 MinUV1Coord { get; set; }

	public Vector2 IDUVCoord { get; set; }

	public Glyph()
	{
		x = 0;
		y = 0;
		rotation = GlyphRotation.NO_ROTATION;
		MinUV1Coord = Vector2.zero;
		MaxUV1Coord = Vector2.zero;
		IDUVCoord = Vector2.zero;
	}

	public Glyph(int newX, int newY)
	{
		x = newX;
		y = newY;
		rotation = GlyphRotation.NO_ROTATION;
		MinUV1Coord = Vector2.zero;
		MaxUV1Coord = Vector2.zero;
		IDUVCoord = Vector2.zero;
	}

	public Glyph(Glyph other)
	{
		x = other.x;
		y = other.y;
		rotation = other.rotation;
		MinUV1Coord = other.MinUV1Coord;
		MaxUV1Coord = other.MaxUV1Coord;
		IDUVCoord = other.IDUVCoord;
	}

	public bool Equals(Glyph other)
	{
		if (other == null)
		{
			return false;
		}
		if (x == other.x && y == other.y)
		{
			return rotation == other.rotation;
		}
		return false;
	}

	public Rect TexCoord(int totalX, int totalY)
	{
		return new Rect((float)x / (float)totalX, (float)y / (float)totalY, 1f / (float)totalX, 1f / (float)totalY);
	}
}
