using UnityEngine;

public class KeyInfoPromptController : MonoBehaviour
{
	[SerializeField]
	private Sprite _eyeCoordinatesSprite;

	private float _showPromptTime;

	private ScreenPrompt _codePrompt;

	private ScreenPrompt _eyeCoordinatesPrompt;

	private float _awakeTime;

	private bool _displayCodePrompt;

	private void Awake()
	{
		_codePrompt = new ScreenPrompt(UITextLibrary.GetString(UITextType.LaunchCodes));
		_eyeCoordinatesPrompt = new ScreenPrompt(UITextLibrary.GetString(UITextType.EyeCoordinates) + "<EYE>", _eyeCoordinatesSprite);
		GlobalMessenger.AddListener("SuitUp", OnSuitUp);
		GlobalMessenger.AddListener("LearnLaunchCodes", OnLearnLaunchCodes);
		GlobalMessenger.AddListener("LaunchCodesEntered", OnLaunchCodesEntered);
	}

	private void Start()
	{
		_awakeTime = Time.time;
		Locator.GetPromptManager().AddScreenPrompt(_codePrompt);
		Locator.GetPromptManager().AddScreenPrompt(_eyeCoordinatesPrompt);
		_displayCodePrompt = PlayerData.KnowsLaunchCodes();
		base.enabled = _displayCodePrompt;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
		GlobalMessenger.RemoveListener("LearnLaunchCodes", OnLearnLaunchCodes);
		GlobalMessenger.RemoveListener("LaunchCodesEntered", OnLaunchCodesEntered);
	}

	public void SetEyeCoordinatesVisibility(bool visible)
	{
		_eyeCoordinatesPrompt.SetVisibility(visible);
	}

	private void OnLearnLaunchCodes()
	{
		_displayCodePrompt = true;
		base.enabled = true;
	}

	private void OnSuitUp()
	{
		if (!Locator.GetPlayerSuit().IsTrainingSuit())
		{
			_codePrompt.SetVisibility(isVisible: false);
			_displayCodePrompt = false;
		}
	}

	private void OnLaunchCodesEntered()
	{
		_codePrompt.SetVisibility(isVisible: false);
		_displayCodePrompt = false;
	}

	private void Update()
	{
		if (!(Time.time < _awakeTime + 5f))
		{
			if (_displayCodePrompt)
			{
				_codePrompt.SetVisibility(OWInput.IsInputMode(InputMode.Character));
				return;
			}
			_codePrompt.SetVisibility(isVisible: false);
			base.enabled = false;
		}
	}
}
