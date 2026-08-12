using System;

[Serializable]
public class PerCharacterKerning
{
	public string First = "";

	public float Second;

	public PerCharacterKerning(string character, float kerning)
	{
		First = character;
		Second = kerning;
	}

	public PerCharacterKerning(char character, float kerning)
	{
		First = character.ToString() ?? "";
		Second = kerning;
	}

	public char GetChar()
	{
		return First[0];
	}

	public float GetKerningValue()
	{
		return Second;
	}
}
