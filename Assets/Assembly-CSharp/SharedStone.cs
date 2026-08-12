using UnityEngine;

public class SharedStone : OWItem
{
	[SerializeField]
	private NomaiRemoteCameraPlatform.ID _connectedPlatform;

	[SerializeField]
	private TransformAnimator _animator;

	private float _animDuration = 0.4f;

	private float _animOffsetY = 0.08f;

	protected override void Awake()
	{
		base.Awake();
		_type = ItemType.SharedStone;
	}

	public NomaiRemoteCameraPlatform.ID GetRemoteCameraID()
	{
		return _connectedPlatform;
	}

	public override string GetDisplayName()
	{
		return "<color=orange>" + NomaiRemoteCameraPlatform.IDToPlanetString(_connectedPlatform) + "</color> " + UITextLibrary.GetString(UITextType.ItemSharedStonePrompt);
	}

	public override void PlaySocketAnimation()
	{
		_animator.transform.localPosition = Vector3.up * _animOffsetY;
		_animator.TranslateToOriginalLocalPosition(_animDuration);
	}

	public override void PlayUnsocketAnimation()
	{
		_animator.TranslateToLocalPosition(Vector3.up * _animOffsetY, _animDuration);
	}

	public override void OnCompleteUnsocket()
	{
		_animator.ResetToOriginalPositionRotation();
	}

	public override bool IsAnimationPlaying()
	{
		return _animator.IsAnimating();
	}
}
