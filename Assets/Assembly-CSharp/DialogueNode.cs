using System.Collections.Generic;

public class DialogueNode
{
	private string _name;

	private List<string> _listPagesToDisplay;

	private List<DialogueOption> _listDialogueOptions;

	private List<string> _listEntryCondition;

	private List<string> _conditionsToSet;

	private string _persistentConditionToSet;

	private string _persistentConditionToDisable;

	private string[] _dbEntriesToSet;

	private string _targetName;

	private DialogueNode _target;

	private List<string> _listTargetCondition;

	private DialogueText _displayTextData;

	private int _currentPage;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public List<DialogueOption> ListDialogueOptions
	{
		get
		{
			return _listDialogueOptions;
		}
		set
		{
			_listDialogueOptions = value;
		}
	}

	public List<string> ListEntryCondition
	{
		get
		{
			return _listEntryCondition;
		}
		set
		{
			_listEntryCondition = value;
		}
	}

	public List<string> ConditionsToSet
	{
		get
		{
			return _conditionsToSet;
		}
		set
		{
			_conditionsToSet = value;
		}
	}

	public string PersistentConditionToSet
	{
		get
		{
			return _persistentConditionToSet;
		}
		set
		{
			_persistentConditionToSet = value;
		}
	}

	public string PersistentConditionToDisable
	{
		get
		{
			return _persistentConditionToDisable;
		}
		set
		{
			_persistentConditionToDisable = value;
		}
	}

	public string[] DBEntriesToSet
	{
		get
		{
			return _dbEntriesToSet;
		}
		set
		{
			_dbEntriesToSet = value;
		}
	}

	public DialogueText DisplayTextData
	{
		get
		{
			return _displayTextData;
		}
		set
		{
			_displayTextData = value;
		}
	}

	public string TargetName
	{
		get
		{
			return _targetName;
		}
		set
		{
			_targetName = value;
		}
	}

	public List<string> ListTargetCondition
	{
		get
		{
			return _listTargetCondition;
		}
		set
		{
			_listTargetCondition = value;
		}
	}

	public DialogueNode Target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
		}
	}

	public int CurrentPage => _currentPage;

	public DialogueNode()
	{
		_name = "";
		_currentPage = -1;
		_targetName = "";
		_target = null;
		_listEntryCondition = new List<string>();
		_listPagesToDisplay = new List<string>();
		_listTargetCondition = new List<string>();
		_listDialogueOptions = new List<DialogueOption>();
		_conditionsToSet = new List<string>();
		_persistentConditionToSet = string.Empty;
		_persistentConditionToDisable = string.Empty;
		_dbEntriesToSet = new string[0];
	}

	public void RefreshConditionsData()
	{
		_listPagesToDisplay = _displayTextData.GetDisplayStringList();
	}

	public void GetNextPage(out string mainText, ref List<DialogueOption> options)
	{
		if (HasNext())
		{
			_currentPage++;
		}
		string key = _name + _listPagesToDisplay[_currentPage];
		mainText = TextTranslation.Translate(key).Trim();
		if (!HasNext())
		{
			for (int i = 0; i < _listDialogueOptions.Count; i++)
			{
				options.Add(_listDialogueOptions[i]);
			}
		}
	}

	public bool HasNext()
	{
		if (_currentPage + 1 < _listPagesToDisplay.Count)
		{
			return true;
		}
		return false;
	}

	public string GetOptionsText(out int count)
	{
		string text = "";
		count = 0;
		for (int i = 0; i < _listDialogueOptions.Count; i++)
		{
			text += _listDialogueOptions[i].Text;
			text += "\n";
			count++;
		}
		return text;
	}

	public bool EntryConditionsSatisfied()
	{
		bool result = true;
		if (_listEntryCondition.Count == 0)
		{
			return false;
		}
		DialogueConditionManager sharedInstance = DialogueConditionManager.SharedInstance;
		for (int i = 0; i < _listEntryCondition.Count; i++)
		{
			string text = _listEntryCondition[i];
			if (sharedInstance.ConditionExists(text))
			{
				if (!sharedInstance.GetConditionState(text))
				{
					result = false;
				}
			}
			else if (!PlayerData.PersistentConditionExists(text))
			{
				if (!PlayerData.GetPersistentCondition(text))
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	public bool TargetNodeConditionsSatisfied()
	{
		bool result = true;
		if (_listTargetCondition.Count == 0)
		{
			return result;
		}
		for (int i = 0; i < _listTargetCondition.Count; i++)
		{
			string id = _listTargetCondition[i];
			if (!Locator.GetShipLogManager().IsFactRevealed(id))
			{
				result = false;
			}
		}
		return result;
	}

	public void SetNodeCompleted()
	{
		_currentPage = -1;
		DialogueConditionManager sharedInstance = DialogueConditionManager.SharedInstance;
		for (int i = 0; i < _conditionsToSet.Count; i++)
		{
			sharedInstance.SetConditionState(_conditionsToSet[i], conditionState: true);
		}
		if (_persistentConditionToSet != string.Empty)
		{
			PlayerData.SetPersistentCondition(_persistentConditionToSet, state: true);
			sharedInstance.SetConditionState(_persistentConditionToSet, conditionState: true);
		}
		if (_persistentConditionToDisable != string.Empty)
		{
			PlayerData.SetPersistentCondition(_persistentConditionToDisable, state: false);
			sharedInstance.SetConditionState(_persistentConditionToDisable);
		}
		for (int j = 0; j < _dbEntriesToSet.Length; j++)
		{
			Locator.GetShipLogManager().RevealFact(_dbEntriesToSet[j]);
		}
	}
}
