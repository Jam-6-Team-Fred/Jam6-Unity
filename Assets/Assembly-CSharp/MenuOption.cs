using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuOption : MonoBehaviour, IEventSystemHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField]
	protected SettingsID _settingId;

	[SerializeField]
	protected Text _label;

	[SerializeField]
	protected Text _secondaryTextField;

	[SerializeField]
	protected UITextType _tooltipTextType;

	[FormerlySerializedAs("_testText")]
	[TextArea]
	[SerializeField]
	protected string _overrideTooltipText;

	protected TooltipDisplay _menuTooltipDisplay;

	[Space(10f)]
	[FormerlySerializedAs("_enableXbox")]
	[SerializeField]
	private bool _enableXboxOne = true;

	[SerializeField]
	private bool _enableXboxSeriesSX = true;

	[SerializeField]
	private bool _enablePS4 = true;

	[SerializeField]
	private bool _enablePS5 = true;

	[SerializeField]
	private bool _enableSwitch = true;

	[SerializeField]
	private bool _enablePC = true;

	[SerializeField]
	private bool _enableSteamDeck = true;

	[SerializeField]
	private bool _enableInGame = true;

	[SerializeField]
	private bool _dlcOnly;

	protected Selectable _selectable;

	[SerializeField]
	private UITextType _overrideLabelTextPS5Only;

	public virtual void Initialize()
	{
		_selectable = this.GetRequiredComponent<Selectable>();
		_selectable.gameObject.SetActive(ShouldEnable());
	}

	public virtual bool ShouldEnable()
	{
		bool result = true;
		if (!_enablePC)
		{
			result = false;
		}
		if (SteamUtils.IsSteamRunningOnSteamDeck() && !_enableSteamDeck)
		{
			result = false;
		}
		if (!_enableInGame && (LoadManager.GetCurrentScene() == OWScene.SolarSystem || LoadManager.GetCurrentScene() == OWScene.EyeOfTheUniverse))
		{
			result = false;
		}
		if (_dlcOnly && EntitlementsManager.IsDlcOwned() != EntitlementsManager.AsyncOwnershipStatus.Owned)
		{
			result = false;
		}
		return result;
	}

	public Selectable GetSelectable()
	{
		if (_selectable == null)
		{
			_selectable = this.GetRequiredComponent<Selectable>();
			if (!_enablePC)
			{
				_selectable.gameObject.SetActive(value: false);
			}
			if (!_enableInGame && (LoadManager.GetCurrentScene() == OWScene.SolarSystem || LoadManager.GetCurrentScene() == OWScene.EyeOfTheUniverse))
			{
				_selectable.gameObject.SetActive(value: false);
			}
		}
		return _selectable;
	}

	public virtual SettingsID GetSettingsID()
	{
		return _settingId;
	}

	public virtual Text GetLabelField()
	{
		return _label;
	}

	public UITextType GetOverrideUITextType()
	{
		return UITextType.None;
	}

	public virtual Text GetSecondaryTextField()
	{
		return _secondaryTextField;
	}

	public void SetSelectable(Selectable s)
	{
		_selectable = s;
	}

	public void SetTooltipDisplay(TooltipDisplay display)
	{
		_menuTooltipDisplay = display;
	}

	public void SetTooltipText(UITextType textType)
	{
		_tooltipTextType = textType;
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		if (_menuTooltipDisplay != null && _tooltipTextType != 0)
		{
			_menuTooltipDisplay.SetTooltipText(UITextLibrary.GetString(_tooltipTextType));
		}
		else if (_menuTooltipDisplay != null && _overrideTooltipText != "")
		{
			_menuTooltipDisplay.SetTooltipText(_overrideTooltipText);
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		if (_menuTooltipDisplay != null && _tooltipTextType != 0)
		{
			_menuTooltipDisplay.SetTooltipText("");
		}
		else if (_menuTooltipDisplay != null && _overrideTooltipText != "")
		{
			_menuTooltipDisplay.SetTooltipText("");
		}
	}
}
