public class SleepAction : GhostAction
{
	private enum WakeState
	{
		Sleeping = 0,
		WakingUp = 1,
		Awake = 2
	}

	private WakeState _state;

	public override Name GetName()
	{
		return Name.Sleep;
	}

	public override float CalculateUtility()
	{
		if (!_data.hasWokenUp)
		{
			return 100f;
		}
		return -100f;
	}

	public override bool IsInterruptible()
	{
		return false;
	}

	protected override void OnEnterAction()
	{
		EnterSleepState();
	}

	protected override void OnExitAction()
	{
	}

	public override bool Update_Action()
	{
		if (_state == WakeState.Sleeping)
		{
			if (_data.hasWokenUp || _data.sensor.isIlluminatedByPlayer)
			{
				_state = WakeState.Awake;
				_effects.PlayDefaultAnimation();
			}
		}
		else if (_state != WakeState.WakingUp && _state == WakeState.Awake)
		{
			return false;
		}
		return true;
	}

	private void OnHeadRaiseComplete()
	{
		_state = WakeState.Awake;
		_data.hasWokenUp = true;
	}

	private void EnterSleepState()
	{
		_controller.SetLanternConcealed(concealed: true);
		_effects.PlaySleepAnimation();
		_state = WakeState.Sleeping;
	}
}
