using System.Collections.Generic;

public class DialogueOption
{
	private string _textID;

	private string _text;

	private string _targetName;

	private string _requiredCondition;

	private List<string> _requiredPersistentCondition;

	private string _cancelledCondition;

	private List<string> _cancelledPersistentCondition;

	private string _setCondition;

	private string _cancelCondition;

	private List<string> _requiredLogCondition;

	public string Text
	{
		get
		{
			return TextTranslation.Translate(_textID);
		}
		set
		{
			_text = value;
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

	public string ConditionRequirement
	{
		get
		{
			return _requiredCondition;
		}
		set
		{
			_requiredCondition = value;
		}
	}

	public List<string> PersistentCondition
	{
		get
		{
			return _requiredPersistentCondition;
		}
		set
		{
			_requiredPersistentCondition = value;
		}
	}

	public string CancelledRequirement
	{
		get
		{
			return _cancelledCondition;
		}
		set
		{
			_cancelledCondition = value;
		}
	}

	public List<string> CancelledPersistentRequirement
	{
		get
		{
			return _cancelledPersistentCondition;
		}
		set
		{
			_cancelledPersistentCondition = value;
		}
	}

	public List<string> LogConditionRequirement
	{
		get
		{
			return _requiredLogCondition;
		}
		set
		{
			_requiredLogCondition = value;
		}
	}

	public string ConditionToSet
	{
		get
		{
			return _setCondition;
		}
		set
		{
			_setCondition = value;
		}
	}

	public string ConditionToCancel
	{
		get
		{
			return _cancelCondition;
		}
		set
		{
			_cancelCondition = value;
		}
	}

	public void SetNodeId(string nodeName, string charName)
	{
		_textID = charName + nodeName + _text;
	}

	public DialogueOption()
	{
		_requiredCondition = string.Empty;
		_requiredPersistentCondition = new List<string>(0);
		_cancelledCondition = string.Empty;
		_cancelledPersistentCondition = new List<string>(0);
		_setCondition = string.Empty;
		_cancelCondition = string.Empty;
		_targetName = string.Empty;
		_text = string.Empty;
		_textID = string.Empty;
		_requiredLogCondition = new List<string>(0);
	}
}
