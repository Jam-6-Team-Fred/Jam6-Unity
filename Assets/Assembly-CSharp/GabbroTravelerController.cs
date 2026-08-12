using UnityEngine;

public class GabbroTravelerController : TravelerController
{
	[SerializeField]
	private Animator _hammockAnimator;

	[SerializeField]
	private IslandController _islandController;

	[SerializeField]
	private float _floatDelay;

	[SerializeField]
	private float _dropDelay;

	private bool _waitingToFloat;

	private bool _waitingToDrop;

	private bool _dropping;

	private float _floatTime;

	private float _dropTime;

	protected override void Awake()
	{
		base.Awake();
		if (_islandController != null)
		{
			_islandController.OnIslandApexEvent += OnIslandApexEvent;
			_islandController.OnIslandSplashEvent += OnIslandSplashEvent;
		}
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_islandController != null)
		{
			_islandController.OnIslandApexEvent -= OnIslandApexEvent;
			_islandController.OnIslandSplashEvent -= OnIslandSplashEvent;
		}
	}

	private void Update()
	{
		if (_waitingToFloat && Time.time > _floatTime)
		{
			if (_animator.enabled)
			{
				_animator.CrossFadeInFixedTime("Gabbro_Float", 20f, -1, _audioSource.time);
				_hammockAnimator.CrossFadeInFixedTime("GabbroHammock_Float", 20f, -1, _audioSource.time);
			}
			_waitingToFloat = false;
		}
		if (_waitingToDrop && Time.time > _dropTime)
		{
			if (_animator.enabled)
			{
				_animator.Play("Gabbro_Drop");
				_hammockAnimator.Play("GabbroHammock_Drop");
				_dropping = true;
			}
			_waitingToDrop = false;
		}
		if (_dropping && _animator.GetCurrentAnimatorStateInfo(0).IsName("Gabbro_Playing"))
		{
			if (_animator.enabled)
			{
				_animator.PlayInFixedTime("Gabbro_Playing", -1, _audioSource.time);
				_hammockAnimator.PlayInFixedTime("GabbroHammock_Playing", -1, _audioSource.time);
			}
			_dropping = false;
		}
		if (!_waitingToFloat && !_waitingToDrop && !_dropping)
		{
			base.enabled = false;
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool num = _animator.enabled;
		bool flag = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (num != flag)
		{
			_animator.enabled = flag;
			_hammockAnimator.enabled = flag;
			if (flag)
			{
				_animator.PlayInFixedTime("Gabbro_Playing", -1, _audioSource.time);
				_hammockAnimator.PlayInFixedTime("GabbroHammock_Playing", -1, _audioSource.time);
			}
			else
			{
				_dropping = false;
			}
		}
	}

	protected override void StartConversation()
	{
		if (_animator.enabled)
		{
			_animator.CrossFadeInFixedTime("Gabbro_Talking", 1.8f);
			_hammockAnimator.CrossFadeInFixedTime("GabbroHammock_Talking", 1.8f);
		}
		Locator.GetTravelerAudioManager().StopAllTravelerAudio();
	}

	protected override void EndConversation(float audioDelay)
	{
		if (_animator.enabled)
		{
			_animator.CrossFadeInFixedTime("Gabbro_Playing", audioDelay, -1, 0f - audioDelay);
			_hammockAnimator.CrossFadeInFixedTime("GabbroHammock_Playing", audioDelay, -1, 0f - audioDelay);
		}
		Locator.GetTravelerAudioManager().PlayAllTravelerAudio(audioDelay);
		if (DialogueConditionManager.SharedInstance.GetConditionState("MAP_PROMPT_REMINDER") || DialogueConditionManager.SharedInstance.GetConditionState("MAP_PROMPT_ATTENTION"))
		{
			bool conditionState = DialogueConditionManager.SharedInstance.GetConditionState("MAP_PROMPT_ATTENTION");
			DialogueConditionManager.SharedInstance.SetConditionState("MAP_PROMPT_REMINDER");
			DialogueConditionManager.SharedInstance.SetConditionState("MAP_PROMPT_ATTENTION");
			GlobalMessenger<bool>.FireEvent("TriggerMapPromptReminder", conditionState);
		}
	}

	protected override void OnUnpause()
	{
		if (!_talking && _animator.enabled && _animator.GetCurrentAnimatorStateInfo(0).IsName("Gabbro_Playing") && !_animator.IsInTransition(0))
		{
			_animator.CrossFadeInFixedTime("Gabbro_Playing", 1f);
			_hammockAnimator.CrossFadeInFixedTime("GabbroHammock_Playing", 1f);
		}
	}

	protected override void OnStartFastForward()
	{
	}

	protected override void OnEndFastForward()
	{
		OnUnpause();
	}

	private void OnIslandSplashEvent()
	{
		if (_dropDelay <= 0f)
		{
			if (_animator.enabled)
			{
				_animator.Play("Gabbro_Drop");
				_hammockAnimator.Play("GabbroHammock_Drop");
				_dropping = true;
				base.enabled = true;
			}
		}
		else
		{
			_waitingToDrop = true;
			_dropTime = Time.time + _dropDelay;
			base.enabled = true;
		}
	}

	private void OnIslandApexEvent()
	{
		if (_floatDelay <= 0f)
		{
			if (_animator.enabled)
			{
				_animator.CrossFadeInFixedTime("Gabbro_Float", 20f, -1, _audioSource.time);
				_hammockAnimator.CrossFadeInFixedTime("GabbroHammock_Float", 20f, -1, _audioSource.time);
			}
		}
		else
		{
			_waitingToFloat = true;
			_floatTime = Time.time + _floatDelay;
			base.enabled = true;
		}
	}

	protected override void OnEnableBigHeadMode()
	{
		Transform boneTransform = _animator.GetBoneTransform(HumanBodyBones.Neck);
		Transform boneTransform2 = _animator.GetBoneTransform(HumanBodyBones.Head);
		boneTransform.localScale = new Vector3(1.58f, 1.58f, 1.58f);
		boneTransform2.localScale = new Vector3(1.58f, 1.58f, 1.58f);
	}
}
