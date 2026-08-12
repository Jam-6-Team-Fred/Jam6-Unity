using UnityEngine;

public class MenuValueOption : MenuOption
{
	public delegate void OptionValueChangedEvent(SettingsID id, MenuValueOption option);

	[SerializeField]
	protected MenuValueOption _dependentMenuOption;

	protected int _value;

	public event OptionValueChangedEvent OnMenuOptionValueChanged;

	public virtual void Initialize(bool inputBool)
	{
		if (inputBool)
		{
			Initialize(1);
		}
		else
		{
			Initialize(0);
		}
	}

	public virtual void Initialize(int startingValue)
	{
		base.Initialize();
		_value = startingValue;
	}

	public virtual bool GetValueAsBool()
	{
		if (_value > 0)
		{
			return true;
		}
		return false;
	}

	public virtual int GetValue()
	{
		return _value;
	}

	public virtual MenuValueOption GetDependentMenuOption()
	{
		return _dependentMenuOption;
	}

	public virtual void OnOptionValueChanged()
	{
		if (this.OnMenuOptionValueChanged != null)
		{
			this.OnMenuOptionValueChanged(_settingId, this);
		}
	}
}
