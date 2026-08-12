using System.Collections.Generic;

public class TranslatorWordLengthComparer : IComparer<TranslatorWord>
{
	public int Compare(TranslatorWord x, TranslatorWord y)
	{
		if (x.Length < y.Length)
		{
			return 1;
		}
		if (x.Length == y.Length)
		{
			return 0;
		}
		return -1;
	}
}
