using UnityEngine;

public class PrisonerInstrumentToggle : MonoBehaviour
{
	[SerializeField]
	private TravelerEyeController _travelerEyeController;

	[SerializeField]
	private RotateTransform _musicBoxRotator;

	[SerializeField]
	private TransformAnimator _bowTransformAnimator;

	[SerializeField]
	private Transform _idleBowTransform;

	[SerializeField]
	private Transform _playingBowTransform;

	private void Awake()
	{
		_travelerEyeController.OnStartPlaying += OnStartPlayingInstrument;
		_travelerEyeController.OnStopPlaying += OnStopPlayingInstrument;
	}

	private void OnDestroy()
	{
		_travelerEyeController.OnStartPlaying -= OnStartPlayingInstrument;
		_travelerEyeController.OnStopPlaying -= OnStopPlayingInstrument;
	}

	private void OnStartPlayingInstrument()
	{
		_musicBoxRotator.enabled = true;
		_bowTransformAnimator.TranslateToLocalPosition(_playingBowTransform.localPosition, 0.25f);
		_bowTransformAnimator.RotateToLocalRotation(_playingBowTransform.localRotation, 0.25f);
	}

	private void OnStopPlayingInstrument()
	{
		_musicBoxRotator.enabled = false;
		_bowTransformAnimator.TranslateToLocalPosition(_idleBowTransform.localPosition, 0.25f);
		_bowTransformAnimator.RotateToLocalRotation(_idleBowTransform.localRotation, 0.25f);
	}
}
