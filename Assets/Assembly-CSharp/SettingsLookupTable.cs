using System;
using UnityEngine;

public class SettingsLookupTable : ScriptableObject
{
	[Serializable]
	public struct SettingsEntry
	{
		public SettingsID settingsId;

		public RebindableID rebindableId;

		public UITextType labelTextType;

		public UITextType tooltipTextType;

		public SettingsEntry(SettingsID sId, UITextType label, UITextType tooltip)
			: this(sId, RebindableID.UNDEFINED, label, tooltip)
		{
		}

		public SettingsEntry(SettingsID sId, RebindableID rId, UITextType label, UITextType tooltip)
		{
			settingsId = sId;
			rebindableId = rId;
			labelTextType = label;
			tooltipTextType = tooltip;
		}

		public override bool Equals(object obj)
		{
			if (obj is SettingsEntry settingsEntry)
			{
				if (true && settingsEntry.settingsId == settingsId && settingsEntry.rebindableId == rebindableId && settingsEntry.labelTextType == labelTextType)
				{
					return settingsEntry.tooltipTextType == tooltipTextType;
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	public static string s_strAssetPath = "Resources/SettingsLookupTable.asset";

	public SettingsEntry[] settingsEntries;
}
