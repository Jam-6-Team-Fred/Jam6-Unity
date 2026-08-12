using UnityEngine;
using UnityEngine.UI;

public class ResumeGameLocalizedText : LocalizedText
{
	[SerializeField]
	protected UITextType _reloadText;

	private bool _isNowLoading;

	protected override void Start()
	{
		base.Start();
		if (PlayerData.IsLoaded())
		{
			SetText();
		}
	}

	public void SetText()
	{
		if (PlayerData.GetLastDeathType() == DeathType.BigBang || PlayerData.GetPersistentCondition("GAME_OVER_LAST_SAVE"))
		{
			_isNowLoading = true;
		}
		else
		{
			_isNowLoading = false;
		}
		UpdateLanguage();
	}

	protected override void UpdateLanguage()
	{
		if (text == null)
		{
			text = GetComponent<Text>();
		}
		if (_isNowLoading)
		{
			text.text = UITextLibrary.GetString(_reloadText);
		}
		else
		{
			text.text = UITextLibrary.GetString(_textID);
		}
		text.SetAllDirty();
	}
}
