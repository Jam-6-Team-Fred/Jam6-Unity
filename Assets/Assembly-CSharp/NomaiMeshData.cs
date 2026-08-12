using System.IO;
using UnityEngine;

public class NomaiMeshData
{
	private int _meshSizeX;

	private int _meshSizeY;

	private Glyph[,] _meshGlyphs;

	private int[,] _meshNomaiTextIds;

	private string _texturePath;

	private int _textureSlicesX;

	private int _textureSlicesY;

	public int MeshNRow => _meshSizeX;

	public int MeshNCol => _meshSizeY;

	public string TexturePath => _texturePath;

	public Glyph[,] MeshGlyphs => _meshGlyphs;

	public int[,] NomaiTextIDs => _meshNomaiTextIds;

	public int TextureNRow => _textureSlicesX;

	public int TextureNCol => _textureSlicesY;

	public bool LoadFromFile(TextAsset asset)
	{
		StringReader stringReader = new StringReader(asset.text);
		string text = stringReader.ReadLine();
		string[] array = text.Split(',');
		if (!int.TryParse(array[0], out _meshSizeX))
		{
			return false;
		}
		if (!int.TryParse(array[1], out _meshSizeY))
		{
			return false;
		}
		_meshGlyphs = new Glyph[_meshSizeX, _meshSizeY];
		_meshNomaiTextIds = new int[_meshSizeX, _meshSizeY];
		for (int i = 0; i < _meshSizeX; i++)
		{
			for (int j = 0; j < _meshSizeY; j++)
			{
				_meshNomaiTextIds[i, j] = -1;
			}
		}
		text = stringReader.ReadLine();
		int result;
		int result2;
		while (text != "BREAK")
		{
			array = text.Split(',');
			int.TryParse(array[0], out result);
			int.TryParse(array[1], out result2);
			Glyph glyph = new Glyph();
			int.TryParse(array[2], out glyph.x);
			int.TryParse(array[3], out glyph.y);
			int.TryParse(array[4], out var result3);
			switch (result3)
			{
			case 0:
				glyph.rotation = Glyph.GlyphRotation.NO_ROTATION;
				break;
			case 1:
				glyph.rotation = Glyph.GlyphRotation.ROT_CW_90;
				break;
			case 2:
				glyph.rotation = Glyph.GlyphRotation.ROT_180;
				break;
			case 3:
				glyph.rotation = Glyph.GlyphRotation.ROT_CW_270;
				break;
			default:
				glyph.rotation = Glyph.GlyphRotation.NO_ROTATION;
				break;
			}
			float.TryParse(array[5], out var result4);
			float.TryParse(array[6], out var result5);
			glyph.MinUV1Coord = new Vector2(result4, result5);
			float.TryParse(array[7], out result4);
			float.TryParse(array[8], out result5);
			glyph.MaxUV1Coord = new Vector2(result4, result5);
			float.TryParse(array[9], out result4);
			float.TryParse(array[10], out result5);
			glyph.IDUVCoord = new Vector2(result4, result5);
			_meshGlyphs[result, _meshSizeY - 1 - result2] = glyph;
			text = stringReader.ReadLine();
		}
		text = stringReader.ReadLine();
		if (!int.TryParse(text, out var _))
		{
			return false;
		}
		text = stringReader.ReadLine();
		while (text != "BREAK")
		{
			array = text.Split(',');
			int.TryParse(array[0], out result);
			int.TryParse(array[1], out result2);
			int.TryParse(array[2], out var result7);
			_meshNomaiTextIds[result, result2] = result7;
			text = stringReader.ReadLine();
		}
		text = stringReader.ReadLine();
		array = text.Split(',');
		_texturePath = array[0];
		if (!int.TryParse(array[1], out _textureSlicesX))
		{
			return false;
		}
		if (!int.TryParse(array[2], out _textureSlicesY))
		{
			return false;
		}
		stringReader.Dispose();
		return true;
	}
}
