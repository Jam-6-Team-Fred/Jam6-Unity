using UnityEngine;

public class PlayerSpacesuit : MonoBehaviour
{
	private bool _isWearingSuit;

	private bool _isWearingHelmet;

	private bool _isTrainingSuit;

	private bool _waitingToPutOnHelmet;

	private float _putOnHelmetTime;

	private static bool s_instantSuitUp;

	private static bool s_instantRemoveSuit;

	private void Start()
	{
		base.enabled = false;
	}

	public static bool GetInstantSuitUp()
	{
		return s_instantSuitUp;
	}

	public static bool GetInstantRemoveSuit()
	{
		return s_instantRemoveSuit;
	}

	public bool IsTrainingSuit()
	{
		return _isTrainingSuit;
	}

	public bool IsWearingSuit(bool includeTrainingSuit = true)
	{
		if (_isWearingSuit)
		{
			if (!includeTrainingSuit)
			{
				return !_isTrainingSuit;
			}
			return true;
		}
		return false;
	}

	public bool IsWearingHelmet()
	{
		return _isWearingHelmet;
	}

	public void SuitUp(bool isTrainingSuit = false, bool instantSuitUp = false, bool putOnHelmet = true)
	{
		if (!_isWearingSuit)
		{
			if (Locator.GetToolModeSwapper().GetToolMode() != ToolMode.Item)
			{
				Locator.GetToolModeSwapper().UnequipTool();
			}
			s_instantSuitUp = instantSuitUp;
			s_instantRemoveSuit = false;
			_isWearingSuit = true;
			_isTrainingSuit = isTrainingSuit;
			GlobalMessenger.FireEvent("SuitUp");
			if (putOnHelmet)
			{
				PutOnHelmet();
			}
		}
		else
		{
			Debug.LogWarning("Already wearing suit");
		}
	}

	public void RemoveSuit(bool instantRemoveSuit = false)
	{
		if (_isWearingSuit)
		{
			if (Locator.GetToolModeSwapper().GetToolMode() != ToolMode.Item)
			{
				Locator.GetToolModeSwapper().UnequipTool();
			}
			s_instantSuitUp = false;
			s_instantRemoveSuit = instantRemoveSuit;
			RemoveHelmet();
			_isWearingSuit = (_isTrainingSuit = false);
			GlobalMessenger.FireEvent("RemoveSuit");
		}
		else
		{
			Debug.LogWarning("Not wearing suit");
		}
	}

	public void PutOnHelmet()
	{
		if (_isWearingSuit && !_isWearingHelmet)
		{
			s_instantRemoveSuit = false;
			_isWearingHelmet = true;
			Locator.GetPlayerAudioController().PlayWearHelmet();
			GlobalMessenger.FireEvent("PutOnHelmet");
		}
		else
		{
			Debug.LogWarning("Cannot put on helmet if not wearing suit or already wearing helmet");
		}
	}

	public void PutOnHelmetAfterDelay(float delay)
	{
		if (_isWearingSuit && !_isWearingHelmet)
		{
			base.enabled = true;
			_waitingToPutOnHelmet = true;
			_putOnHelmetTime = Time.time + delay;
			s_instantSuitUp = false;
		}
		else
		{
			Debug.LogWarning("Cannot put on helmet if not wearing suit or already wearing helmet");
		}
	}

	public void RemoveHelmet()
	{
		if (_waitingToPutOnHelmet)
		{
			Debug.Log("Cancelled delayed putting on helmet");
			_waitingToPutOnHelmet = false;
			base.enabled = false;
		}
		else if (_isWearingHelmet && _isWearingSuit)
		{
			s_instantSuitUp = false;
			_isWearingHelmet = false;
			Locator.GetPlayerAudioController().PlayRemoveHelmet();
			GlobalMessenger.FireEvent("RemoveHelmet");
		}
		else
		{
			Debug.LogError("Cannot remove helmet if not wearing suit or not wearing helmet");
		}
	}

	private void Update()
	{
		if (_waitingToPutOnHelmet && Time.time > _putOnHelmetTime)
		{
			PutOnHelmet();
			_waitingToPutOnHelmet = false;
			base.enabled = false;
		}
	}
}
