using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SubmitActionLoadScene : SubmitActionConfirm
{
	public enum LoadableScenes
	{
		GAME = 0,
		EYE = 1,
		TITLE = 2,
		CREDITS = 3
	}

	[SerializeField]
	private LoadableScenes _sceneToLoad;

	[SerializeField]
	protected Text _loadingText;

	[SerializeField]
	private TitleScreenStreaming _titleScreenStreaming;

	private StringBuilder _nowLoadingSB;

	private bool _receivedSubmitAction;

	private bool _waitingOnStreaming;

	public void SetSceneToLoad(LoadableScenes scene)
	{
		_sceneToLoad = scene;
	}

	private void Update()
	{
		if (_receivedSubmitAction && (LoadManager.GetLoadingScene() == OWScene.SolarSystem || LoadManager.GetLoadingScene() == OWScene.EyeOfTheUniverse) && _loadingText != null)
		{
			float asyncLoadProgress = LoadManager.GetAsyncLoadProgress();
			asyncLoadProgress = ((!(asyncLoadProgress < 0.1f)) ? (0.9f + Mathf.InverseLerp(0.1f, 1f, asyncLoadProgress) * 0.1f) : (Mathf.InverseLerp(0f, 0.1f, asyncLoadProgress) * 0.9f));
			ResetStringBuilder();
			_nowLoadingSB.Append(UITextLibrary.GetString(UITextType.LoadingMessage));
			_nowLoadingSB.Append(asyncLoadProgress.ToString("P0"));
			_loadingText.text = _nowLoadingSB.ToString();
			if (_waitingOnStreaming && LoadManager.IsAsyncLoadComplete() && _titleScreenStreaming.AreRequiredAssetsLoaded())
			{
				LoadManager.EnableAsyncLoadTransition();
				_waitingOnStreaming = false;
			}
		}
	}

	private void ResetStringBuilder()
	{
		if (_nowLoadingSB == null)
		{
			_nowLoadingSB = new StringBuilder();
		}
		else
		{
			_nowLoadingSB.Length = 0;
		}
	}

	protected override void ConfirmSubmit()
	{
		base.ConfirmSubmit();
		switch (_sceneToLoad)
		{
		case LoadableScenes.GAME:
			LoadManager.LoadSceneAsync(OWScene.SolarSystem, autoTransition: false, LoadManager.FadeType.ToBlack, 1f, pauseDuringFade: false);
			ResetStringBuilder();
			_waitingOnStreaming = true;
			break;
		case LoadableScenes.EYE:
			LoadManager.LoadSceneAsync(OWScene.EyeOfTheUniverse, autoTransition: true, LoadManager.FadeType.ToBlack, 1f, pauseDuringFade: false);
			ResetStringBuilder();
			break;
		case LoadableScenes.TITLE:
			LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 2f);
			break;
		case LoadableScenes.CREDITS:
			LoadManager.LoadScene(OWScene.Credits_Fast, LoadManager.FadeType.ToBlack, 1f, pauseDuringFade: false);
			break;
		}
		_receivedSubmitAction = true;
		Locator.GetMenuInputModule().DisableInputs();
	}

	protected override void SetUpPopupMenu()
	{
		string text = "";
		LoadableScenes sceneToLoad = _sceneToLoad;
		text = ((sceneToLoad != LoadableScenes.TITLE) ? UITextLibrary.GetString(UITextType.MenuAreYouSure) : (PlayerData.GetWarpedToTheEye() ? UITextLibrary.GetString(UITextType.PauseEyeQuitMessage) : ((TimeLoop.GetLoopCount() <= 1) ? UITextLibrary.GetString(UITextType.PauseQuitMessage) : UITextLibrary.GetString(UITextType.PauseQuitLoopMessage))));
		IInputCommands inputCommands;
		switch (_sceneToLoad)
		{
		case LoadableScenes.GAME:
		case LoadableScenes.EYE:
		case LoadableScenes.CREDITS:
			inputCommands = InputLibrary.menuConfirm;
			break;
		case LoadableScenes.TITLE:
			inputCommands = InputLibrary.confirm;
			break;
		default:
			inputCommands = InputLibrary.menuConfirm;
			break;
		}
		_receivedSubmitAction = false;
		_confirmActionPrompt = new ScreenPrompt(inputCommands, UITextLibrary.GetString(UITextType.MenuConfirm));
		_cancelActionPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.MenuCancel));
		_confirmPopup.SetUpPopup(text, inputCommands, InputLibrary.cancel, _confirmActionPrompt, _cancelActionPrompt);
		base.SetUpPopupMenu();
	}
}
