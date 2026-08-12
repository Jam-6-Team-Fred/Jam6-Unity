using System;
using UnityEngine.UI;

[Serializable]
public struct SettingsMenuData
{
	public SettingsID id;

	public MenuOption uiMenuOption;

	public Text labelTextField;

	public Text secondaryTextField;

	public MenuOption dependentMenuOption;
}
