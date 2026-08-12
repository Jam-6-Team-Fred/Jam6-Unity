using UnityEngine;

public class PlaytestDiscoveryPrompt : MonoBehaviour
{
	[SerializeField]
	private CloakFieldController _cloakFieldController;

	[SerializeField]
	private GameObject _popupPrefab;

	[Tooltip("whether to actually record some values that need to be recorded. To be turned off when not doing playtest build.")]
	private PopupMenu _menu;

	private bool _subscribed;

	private static PlaytestDiscoveryPrompt _instance;

	private int _enterSatStationLoop = -1;

	private int _enterSatelliteLoop = -1;

	public static PlaytestDiscoveryPrompt instance => _instance;

	public int enterSatStationLoop
	{
		get
		{
			return _enterSatStationLoop;
		}
		set
		{
			_enterSatStationLoop = value;
		}
	}

	public int enterSatelliteLoop
	{
		get
		{
			return _enterSatelliteLoop;
		}
		set
		{
			_enterSatelliteLoop = value;
		}
	}

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		_cloakFieldController.OnPlayerEnter += new OWEvent.OWCallback(OnPlayerEnter);
		_subscribed = true;
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		if (_subscribed)
		{
			_cloakFieldController.OnPlayerEnter -= new OWEvent.OWCallback(OnPlayerEnter);
		}
	}

	private void LoadValues()
	{
	}

	private void OnPlayerEnter()
	{
		if (_menu == null)
		{
			GameObject gameObject = Object.Instantiate(_popupPrefab);
			_menu = gameObject.GetComponentInChildren<PopupMenu>(includeInactive: true);
			_menu.OnPopupConfirm += OnPopupConfirm;
		}
		ScreenPrompt okPrompt = new ScreenPrompt(InputLibrary.select, "Send");
		ScreenPrompt cancelPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.MenuCancel));
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		int loopCount = TimeLoop.GetLoopCount();
		string message = $"You've found something new! (sorry, it's not ready to be explored just yet)\n\nPlease notify us at support@mobiusdigitalgames.com\nand include the following info:\n\n- elapsed playtime so far: {realtimeSinceStartup:N2} seconds\n- number of loops so far: {loopCount}\n- enter satellite loop: {enterSatelliteLoop}\n- enter radio tower loop: {enterSatStationLoop}\n- did you stumble on this message by accident or were you investigating a mysterious anomaly?\n- a screenshot of the whole ship log in rumor mode";
		OWTime.Pause(OWTime.PauseType.Menu);
		_menu.EnableMenu(value: true);
		_menu.SetUpPopup(message, InputLibrary.select, InputLibrary.cancel, okPrompt, cancelPrompt, closeMenuOnOk: true, setCancelButtonActive: false);
		GraphicSettings graphicSettings = PlayerData.GetGraphicSettings();
		graphicSettings.fullScreen = false;
		PlayerData.SetGraphicSettings(graphicSettings);
		PlayerData.SaveSettings();
	}

	private void OnPopupConfirm()
	{
		_menu.EnableMenu(value: false);
		OWTime.Unpause(OWTime.PauseType.Menu);
		GraphicSettings graphicSettings = PlayerData.GetGraphicSettings();
		graphicSettings.fullScreen = true;
		PlayerData.SetGraphicSettings(graphicSettings);
		PlayerData.SaveSettings();
		Locator.GetDeathManager().KillPlayer(DeathType.Supernova);
	}
}
