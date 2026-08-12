using System.Text;
using UnityEngine;

public class TranslatorWord
{
	private float _textRefreshRate;

	private float _updateTime;

	private float _totalTime;

	private float _translateTime;

	private bool _startTranslating;

	private bool _isTranslated;

	private StringBuilder _strBuilder;

	public string DisplayText { get; set; }

	public string TranslatedText { get; set; }

	public int StartPosition { get; set; }

	public int EndPosition { get; set; }

	public int Length { get; set; }

	public int DisplayOrder { get; set; }

	public TranslatorWord(string translatedText, int startPos, int endPos, bool previouslyTransated, float translationTime)
	{
		_strBuilder = new StringBuilder();
		TranslatedText = translatedText.Replace("\\\\n", "\n");
		if (TranslatedText.Contains("<NbTimeloops>"))
		{
			int num = (TimeLoop.GetLoopCount() + 53) % 1000;
			int num2 = (int)Mathf.Floor((TimeLoop.GetLoopCount() + 318053) / 1000) % 1000;
			int num3 = (int)Mathf.Floor((TimeLoop.GetLoopCount() + 9318053) / 1000000);
			string newValue = ((TextTranslation.Get().GetLanguage() != 0 && TextTranslation.Get().GetLanguage() != TextTranslation.Language.JAPANESE && TextTranslation.Get().GetLanguage() != TextTranslation.Language.CHINESE_SIMPLE) ? ((TextTranslation.Get().GetLanguage() != TextTranslation.Language.GERMAN && TextTranslation.Get().GetLanguage() != TextTranslation.Language.ITALIAN && TextTranslation.Get().GetLanguage() != TextTranslation.Language.PORTUGUESE_BR) ? (num3 + " " + num2.ToString("D3") + " " + num.ToString("D3")) : (num3 + "." + num2.ToString("D3") + "." + num.ToString("D3"))) : (num3 + "," + num2.ToString("D3") + "," + num.ToString("D3")));
			TranslatedText = TranslatedText.Replace("<NbTimeloops>", newValue);
		}
		if (TranslatedText.Contains("<FirstLoop>"))
		{
			int num4 = 54;
			int num5 = 318;
			int num6 = 9;
			string newValue = ((TextTranslation.Get().GetLanguage() != 0 && TextTranslation.Get().GetLanguage() != TextTranslation.Language.JAPANESE) ? ((TextTranslation.Get().GetLanguage() != TextTranslation.Language.GERMAN && TextTranslation.Get().GetLanguage() != TextTranslation.Language.ITALIAN && TextTranslation.Get().GetLanguage() != TextTranslation.Language.PORTUGUESE_BR) ? (num6 + " " + num5.ToString("D3") + " " + num4.ToString("D3")) : (num6 + "." + num5.ToString("D3") + "." + num4.ToString("D3"))) : (num6 + "," + num5.ToString("D3") + "," + num4.ToString("D3")));
			TranslatedText = TranslatedText.Replace("<FirstLoop>", newValue);
		}
		if (TranslatedText.Contains("<"))
		{
			string newValue = string.Concat(Mathf.Floor(TimeLoop.GetMinutesElapsed()));
			TranslatedText = TranslatedText.Replace("<TimeMinutes>", newValue);
			newValue = string.Concat(22f - Mathf.Floor(TimeLoop.GetMinutesElapsed()));
			TranslatedText = TranslatedText.Replace("<TimeMinutesRemaining>", newValue);
			newValue = string.Concat(Mathf.Floor((TimeLoop.GetSecondsElapsed() + 2501f) / 60f));
			TranslatedText = TranslatedText.Replace("<TimeMinutesSolarActivity>", newValue);
			newValue = string.Concat((int)TimeLoop.GetSecondsElapsed() % 60);
			TranslatedText = TranslatedText.Replace("<TimeSeconds>", newValue);
			newValue = string.Concat(Mathf.Max(0f, Mathf.Floor((690f - TimeLoop.GetSecondsElapsed()) / 60f)));
			TranslatedText = TranslatedText.Replace("<RemainingMinutes>", newValue);
			newValue = string.Concat(Mathf.Max(0f, (690f - Mathf.Floor(TimeLoop.GetSecondsElapsed())) % 60f));
			TranslatedText = TranslatedText.Replace("<RemainingSeconds>", newValue);
			newValue = string.Concat(22f - Mathf.Floor(TimeLoop.GetMinutesElapsed()));
			TranslatedText = TranslatedText.Replace("<MinutesToRedGiant>", newValue);
			newValue = string.Concat((1320f - Mathf.Floor(TimeLoop.GetSecondsElapsed())) % 60f);
			TranslatedText = TranslatedText.Replace("<SecondsToRedGiant>", newValue);
			newValue = string.Concat(Mathf.Floor((TimeLoop.GetSecondsElapsed() - 690f) / 60f));
			TranslatedText = TranslatedText.Replace("<MinutesSinceRedGiant>", newValue);
			newValue = string.Concat((Mathf.Floor(TimeLoop.GetSecondsElapsed()) - 690f) % 60f);
			TranslatedText = TranslatedText.Replace("<SecondsSinceRedGiant>", newValue);
		}
		StartPosition = startPos;
		EndPosition = endPos;
		Length = endPos - startPos;
		_updateTime = 0f;
		DisplayText = "";
		_startTranslating = false;
		_isTranslated = false;
		_translateTime = translationTime;
	}

	public bool IsTranslated()
	{
		return _isTranslated;
	}

	public void BeginTranslation(float perWordTranslateTime)
	{
		_startTranslating = true;
		_translateTime = perWordTranslateTime;
		_textRefreshRate = _translateTime * 0.1f;
	}

	public void UpdateDisplayText(float dt)
	{
		if (_isTranslated)
		{
			return;
		}
		if (_totalTime >= _translateTime)
		{
			DisplayText = TranslatedText;
			_isTranslated = true;
		}
		else if (_updateTime >= _textRefreshRate)
		{
			_strBuilder.Length = 0;
			for (int i = 0; i < Length; i++)
			{
				string value = char.ConvertFromUtf32(Random.Range(48, 126));
				_strBuilder.Append(value);
			}
			_updateTime = 0f;
			DisplayText = _strBuilder.ToString();
		}
		_updateTime += dt;
		if (_startTranslating)
		{
			_totalTime += dt;
		}
	}
}
