public class UITextLibrary
{
	public static string GetString(UITextType TextID)
	{
		return TextTranslation.Translate_UI((int)TextID);
	}
}
