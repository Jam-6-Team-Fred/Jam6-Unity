using System.Collections.Generic;

public class DialogueConditionManager
{
	private static DialogueConditionManager s_instance;

	private IDictionary<string, bool> _dictConditions;

	public static DialogueConditionManager SharedInstance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new DialogueConditionManager();
			}
			return s_instance;
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger<int>.RemoveListener("StartOfTimeLoop", OnStartOfTimeLoop);
	}

	public DialogueConditionManager()
	{
		GlobalMessenger<int>.AddListener("StartOfTimeLoop", OnStartOfTimeLoop);
		_dictConditions = new Dictionary<string, bool>(ComparerLibrary.stringEqComparer);
		_dictConditions.Add("DEFAULT", value: true);
	}

	public void ReadPlayerData()
	{
		ShipLogManager shipLogManager = Locator.GetShipLogManager();
		if (shipLogManager != null)
		{
			if (shipLogManager.IsFactRevealed("DB_VESSEL_X1"))
			{
				SetConditionState("DB_VESSEL_X1", conditionState: true);
			}
			if (shipLogManager.IsFactRevealed("IP_RING_WORLD_X1"))
			{
				SetConditionState("IP_RING_WORLD_X1", conditionState: true);
			}
			if (!PlayerData.GetPersistentCondition("COMPLETED_SHIPLOG_TUTORIAL") && shipLogManager.IsFactRevealed("IP_RING_WORLD_X1"))
			{
				SetConditionState("SLATE_SHIPLOG_TUTORIAL_PREREQ", conditionState: true);
			}
		}
		if (PlayerData.GetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE_MULTIPLE"))
		{
			SetConditionState("ParadoxLoopGOE2", conditionState: true);
		}
		if (PlayerData.GetPersistentCondition("DB_VESSEL_X1"))
		{
			SetConditionState("DB_VESSEL_X1", conditionState: true);
		}
		if (PlayerData.GetPersistentCondition("IP_RING_WORLD_X1"))
		{
			SetConditionState("IP_RING_WORLD_X1", conditionState: true);
		}
		int num = PlayerData.LoadLoopCount();
		bool conditionState = false;
		if (num >= 1)
		{
			conditionState = true;
		}
		if (!AddCondition("LOOP_COUNT_GOE_1", conditionState))
		{
			SetConditionState("LOOP_COUNT_GOE_1", conditionState);
		}
		conditionState = false;
		if (num == 1)
		{
			conditionState = true;
		}
		if (!AddCondition("LOOP_COUNT_EQ_1", conditionState))
		{
			SetConditionState("LOOP_COUNT_EQ_1", conditionState);
		}
		conditionState = false;
		if (num <= 2)
		{
			conditionState = true;
		}
		if (!AddCondition("LOOP_COUNT_LOE_2", conditionState))
		{
			SetConditionState("LOOP_COUNT_LOE_2", conditionState);
		}
		conditionState = false;
		if (num >= 2)
		{
			conditionState = true;
		}
		if (!AddCondition("LOOP_COUNT_GOE_2", conditionState))
		{
			SetConditionState("LOOP_COUNT_GOE_2", conditionState);
		}
		conditionState = false;
		if (num == 2)
		{
			conditionState = true;
		}
		if (!AddCondition("LOOP_COUNT_EQ_2", conditionState))
		{
			SetConditionState("LOOP_COUNT_EQ_2", conditionState);
		}
		conditionState = false;
		if (num >= 3)
		{
			conditionState = true;
		}
		if (!AddCondition("LOOP_COUNT_GOE_3", conditionState))
		{
			SetConditionState("LOOP_COUNT_GOE_3", conditionState);
		}
		conditionState = false;
		if (num == 3)
		{
			conditionState = true;
		}
		if (!AddCondition("LOOP_COUNT_EQ_3", conditionState))
		{
			SetConditionState("LOOP_COUNT_EQ_3", conditionState);
		}
	}

	public bool AddCondition(string conditionName, bool conditionState = false)
	{
		if (!ConditionExists(conditionName))
		{
			_dictConditions.Add(conditionName, conditionState);
			return true;
		}
		return false;
	}

	public bool ConditionExists(string conditionName)
	{
		return _dictConditions.ContainsKey(conditionName);
	}

	public void SetConditionState(string conditionName, bool conditionState = false)
	{
		bool flag = true;
		if (ConditionExists(conditionName))
		{
			if (_dictConditions[conditionName] == conditionState)
			{
				flag = false;
			}
			_dictConditions[conditionName] = conditionState;
		}
		else
		{
			AddCondition(conditionName, conditionState);
		}
		if (flag)
		{
			GlobalMessenger<string, bool>.FireEvent("DialogueConditionChanged", conditionName, conditionState);
		}
		if (conditionName == "LAUNCH_CODES_GIVEN")
		{
			PlayerData.LearnLaunchCodes();
		}
	}

	public bool GetConditionState(string conditionName)
	{
		if (_dictConditions.ContainsKey(conditionName))
		{
			return _dictConditions[conditionName];
		}
		return false;
	}

	private void OnStartOfTimeLoop(int loopCount)
	{
		_dictConditions.Clear();
		_dictConditions.Add("DEFAULT", value: true);
		ReadPlayerData();
		GlobalMessenger.FireEvent("DialogueConditionsReset");
	}
}
