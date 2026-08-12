using UnityEngine;

public class ScrollItem : OWItem
{
	[SerializeField]
	private TransformAnimator _animator;

	private NomaiWallText _nomaiWallText;

	private const float _animDuration = 0.9f;

	private const float _animDegrees = -105f;

	private const float _animOffsetZ = 0.8f;

	protected override void Awake()
	{
		_type = ItemType.Scroll;
		_nomaiWallText = GetComponentInChildren<NomaiWallText>();
		if (_nomaiWallText == null)
		{
			Debug.LogError("No NomaiWallText found!", this);
			Debug.Break();
		}
		_nomaiWallText.InitializeAsWhiteboardText();
		base.Awake();
		for (int i = 0; i < _colliders.Length; i++)
		{
			if (_colliders[i].GetComponent<NomaiWallText>() != null)
			{
				_colliders[i] = null;
			}
		}
	}

	public override void PlaySocketAnimation()
	{
		_animator.transform.localPosition = Vector3.forward * 0.8f;
		_animator.TranslateToOriginalLocalPosition(0.9f);
		_animator.transform.localEulerAngles = Vector3.forward * -105f;
		_animator.RotateToOriginalLocalRotation(0.9f);
	}

	public override void PlayUnsocketAnimation()
	{
		_animator.TranslateToLocalPosition(Vector3.forward * 0.8f, 0.9f);
		_animator.RotateToLocalEulerAngles(Vector3.forward * -105f, 0.9f);
	}

	public override void OnCompleteUnsocket()
	{
		_animator.ResetToOriginalPositionRotation();
	}

	public void ShowNomaiTextImmediate()
	{
		_nomaiWallText.ShowImmediate();
	}

	public void ShowNomaiText()
	{
		_nomaiWallText.Show();
	}

	public void HideNomaiText()
	{
		_nomaiWallText.Hide(0.9f);
	}

	public override string GetDisplayName()
	{
		return UITextLibrary.GetString(UITextType.ItemScrollPrompt);
	}

	public override bool IsAnimationPlaying()
	{
		if (!_nomaiWallText.IsAnimationPlaying())
		{
			return _animator.IsAnimating();
		}
		return true;
	}
}
