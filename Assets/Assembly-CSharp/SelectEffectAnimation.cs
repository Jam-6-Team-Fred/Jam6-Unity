using UnityEngine;
using UnityEngine.EventSystems;

public class SelectEffectAnimation : SelectEffect
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private string _animatorOnSelectTrigger;

	[SerializeField]
	private string _animatorOnDeselectTrigger;

	public override void OnSelect(BaseEventData eventData)
	{
		_animator.SetTrigger(_animatorOnSelectTrigger);
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		_animator.SetTrigger(_animatorOnDeselectTrigger);
	}
}
