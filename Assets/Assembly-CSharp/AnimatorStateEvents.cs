using UnityEngine;

public class AnimatorStateEvents : StateMachineBehaviour
{
	public delegate void StateEvent(AnimatorStateInfo stateInfo, int layerIndex);

	public event StateEvent OnEnterState;

	public event StateEvent OnExitState;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.OnEnterState != null)
		{
			this.OnEnterState(stateInfo, layerIndex);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.OnExitState != null)
		{
			this.OnExitState(stateInfo, layerIndex);
		}
	}
}
