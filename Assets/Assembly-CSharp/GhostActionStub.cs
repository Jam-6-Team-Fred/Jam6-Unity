public class GhostActionStub : GhostAction
{
	public override Name GetName()
	{
		return Name.Wait;
	}

	public override float CalculateUtility()
	{
		return 0f;
	}

	public override bool IsInterruptible()
	{
		return true;
	}

	protected override void OnEnterAction()
	{
	}

	protected override void OnExitAction()
	{
	}

	public override bool Update_Action()
	{
		return true;
	}

	public override void FixedUpdate_Action()
	{
	}

	public override void OnArriveAtPosition()
	{
	}
}
