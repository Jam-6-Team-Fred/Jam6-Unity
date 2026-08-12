using UnityEngine;

public class TravelerController : SectoredMonoBehaviour, ITimelineObliterator
{
	[SerializeField]
	protected CharacterDialogueTree _dialogueSystem;

	[SerializeField]
	protected Animator _animator;

	[SerializeField]
	protected AudioSource _audioSource;

	[SerializeField]
	protected float _delayToRestartAudio;

	[SerializeField]
	private VoidShadowEffectController _voidShadowEffect;

	protected bool _talking;

	protected int _playingAnimID;

	protected override void Awake()
	{
		base.Awake();
		if (_dialogueSystem != null)
		{
			_dialogueSystem.OnStartConversation += OnStartConversation;
			_dialogueSystem.OnEndConversation += OnEndConversation;
			GlobalMessenger.AddListener("StartTravelerConversation", StartConversation);
			GlobalMessenger<float>.AddListener("EndTravelerConversation", EndConversation);
			GlobalMessenger.AddListener("GameUnpaused", OnUnpause);
			GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
			GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
		}
		GlobalMessenger.AddListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_dialogueSystem != null)
		{
			_dialogueSystem.OnStartConversation -= OnStartConversation;
			_dialogueSystem.OnEndConversation -= OnEndConversation;
			GlobalMessenger.RemoveListener("StartTravelerConversation", StartConversation);
			GlobalMessenger<float>.RemoveListener("EndTravelerConversation", EndConversation);
			GlobalMessenger.RemoveListener("GameUnpaused", OnUnpause);
			GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
			GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
		}
		GlobalMessenger.RemoveListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (_animator != null)
		{
			bool num = _animator.enabled;
			_animator.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
			if (!num && _animator.enabled && _audioSource != null)
			{
				_animator.PlayInFixedTime(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, _audioSource.time);
			}
		}
	}

	private void OnStartConversation()
	{
		_talking = true;
		GlobalMessenger.FireEvent("StartTravelerConversation");
	}

	private void OnEndConversation()
	{
		GlobalMessenger<float>.FireEvent("EndTravelerConversation", _delayToRestartAudio);
		_talking = false;
	}

	protected virtual void StartConversation()
	{
		if (_animator != null && _animator.enabled)
		{
			if (_animator.IsInTransition(0))
			{
				_playingAnimID = _animator.GetNextAnimatorStateInfo(0).fullPathHash;
			}
			else
			{
				_playingAnimID = _animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
			}
			_animator.SetTrigger("Talking");
		}
		Locator.GetTravelerAudioManager().StopAllTravelerAudio();
	}

	protected virtual void EndConversation(float audioDelay)
	{
		if (_animator != null && _animator.enabled)
		{
			if (audioDelay > 0f)
			{
				_animator.CrossFadeInFixedTime(_playingAnimID, audioDelay, -1, 0f - audioDelay);
			}
			else
			{
				_animator.SetTrigger("Playing");
			}
		}
		Locator.GetTravelerAudioManager().PlayAllTravelerAudio(audioDelay);
	}

	protected virtual void OnUnpause()
	{
		if (!_talking && _animator != null && _animator.enabled)
		{
			_animator.SetTrigger("Playing");
		}
	}

	protected virtual void OnStartFastForward()
	{
		if (_animator != null)
		{
			_animator.enabled = false;
		}
	}

	protected virtual void OnEndFastForward()
	{
		if (_animator != null)
		{
			_animator.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
			if (_animator.enabled)
			{
				_animator.SetTrigger("Playing");
			}
		}
	}

	protected virtual void OnEnableBigHeadMode()
	{
		if (_animator != null)
		{
			_animator.GetBoneTransform(HumanBodyBones.Neck).localScale = new Vector3(2.5f, 2.5f, 2.5f);
		}
	}

	public VoidShadowEffectController GetVoidShadowEffect()
	{
		return _voidShadowEffect;
	}
}
