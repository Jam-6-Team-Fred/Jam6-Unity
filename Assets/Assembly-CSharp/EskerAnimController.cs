using UnityEngine;

public class EskerAnimController : CharacterAnimController
{
	[Space]
	[SerializeField]
	private OWAudioSource _whistleSource;

	[SerializeField]
	private AnimationCurve _whistleBlendCurve = new AnimationCurve();

	[SerializeField]
	private float _whistleBlendSpeed = 3f;

	[Space]
	[SerializeField]
	private TravelerEyeController _travelerEyeController;

	private bool _isWhistling = true;

	private float _whistleWeight;

	protected override void Awake()
	{
		base.Awake();
		if (_travelerEyeController != null)
		{
			_isWhistling = false;
			_travelerEyeController.OnStartPlaying += OnStartWhistling;
			_travelerEyeController.OnStopPlaying += OnStopWhistling;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_travelerEyeController != null)
		{
			_travelerEyeController.OnStartPlaying -= OnStartWhistling;
			_travelerEyeController.OnStopPlaying -= OnStopWhistling;
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if ((bool)_skinRenderer && _skinRenderer.sharedMesh != null)
		{
			float target = ((_inConversation || !_isWhistling) ? 0f : Mathf.Max(_whistleBlendCurve.Evaluate(_whistleSource.time + 0.33f), 0.5f));
			_whistleWeight = Mathf.MoveTowards(_whistleWeight, target, Time.deltaTime * _whistleBlendSpeed);
			_skinRenderer.SetBlendShapeWeight(8, _whistleWeight * 100f);
		}
	}

	private void OnRockChair()
	{
	}

	private void OnStartWhistling()
	{
		_isWhistling = true;
	}

	private void OnStopWhistling()
	{
		_isWhistling = false;
	}
}
