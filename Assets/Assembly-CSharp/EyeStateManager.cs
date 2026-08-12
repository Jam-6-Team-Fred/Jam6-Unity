using UnityEngine;

public class EyeStateManager : MonoBehaviour
{
	[SerializeField]
	private EyeState _initialState;

	private EyeState _state = EyeState.Undefined;

	private bool _initialized;

	private static bool s_paradoxExists;

	public static bool ParadoxExists()
	{
		return s_paradoxExists;
	}

	private void Awake()
	{
		s_paradoxExists = PlayerData.GetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE") || PlayerData.GetPersistentCondition("PROBE_ENTERED_TIMELOOPCORE");
		if (PlayerData.GetWarpedToTheEye())
		{
			_initialState = EyeState.AboardVessel;
			return;
		}
		EyeSpawnPoint[] array = (EyeSpawnPoint[])Resources.FindObjectsOfTypeAll(typeof(EyeSpawnPoint));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetEyeState() == _initialState)
			{
				GameObject gameObject = GameObject.FindWithTag("Player");
				if (gameObject != null)
				{
					gameObject.GetComponent<PlayerSpawner>().SetInitialSpawnPoint(array[i]);
				}
			}
		}
	}

	private void Start()
	{
		_initialized = true;
		BadMarshmallowCan.CheckEotUState();
		SetState(_initialState);
	}

	private void OnDestroy()
	{
	}

	public bool IsInsideTheEye()
	{
		return _state >= EyeState.Observatory;
	}

	public void SetState(EyeState state)
	{
		if (state != _state)
		{
			_state = state;
			GlobalMessenger<EyeState>.FireEvent("EyeStateChanged", _state);
			if (IsInsideTheEye() && Locator.GetToolModeSwapper() != null && Locator.GetToolModeSwapper().GetSignalScope() != null)
			{
				Locator.GetToolModeSwapper().GetSignalScope().SelectFrequency(SignalFrequency.Traveler);
			}
		}
	}

	public EyeState GetState()
	{
		if (!_initialized)
		{
			return _initialState;
		}
		return _state;
	}
}
