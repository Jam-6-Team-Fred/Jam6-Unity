using UnityEngine;

public class TravelerEyeController : MonoBehaviour
{
	public delegate void TravelerEyeEvent();

	[SerializeField]
	private CharacterDialogueTree _dialogueTree;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Animator _rockingChairAnimator;

	[SerializeField]
	private AudioSignal _signal;

	[SerializeField]
	private string _startPlayingCondition;

	private bool _isPlaying;

	public event TravelerEyeEvent OnStartPlaying;

	public event TravelerEyeEvent OnStopPlaying;

	private void Awake()
	{
		if (_dialogueTree != null)
		{
			_dialogueTree.OnStartConversation += OnStartConversation;
			_dialogueTree.OnEndConversation += OnEndConversation;
		}
		GlobalMessenger.AddListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	private void OnEnable()
	{
		if (_animator != null)
		{
			_animator.SetBool("Playing", value: false);
		}
		if (_rockingChairAnimator != null)
		{
			_rockingChairAnimator.SetBool("Playing", value: false);
		}
	}

	private void OnDestroy()
	{
		if (_dialogueTree != null)
		{
			_dialogueTree.OnStartConversation -= OnStartConversation;
			_dialogueTree.OnEndConversation -= OnEndConversation;
		}
		GlobalMessenger.RemoveListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	public void OnStartCosmicJamSession()
	{
		_signal.GetOWAudioSource().SetLocalVolume(0f);
		_signal.GetOWAudioSource().Play();
	}

	public void OnCrossfadeToFinale(float fadeOutDuration)
	{
		_signal.SetSignalActivation(active: false, fadeOutDuration);
	}

	public float GetSecondsUntilCrossfadeToFinale()
	{
		float num = _signal.GetOWAudioSource().clip.length / 4f;
		float time = _signal.GetOWAudioSource().time;
		float num2 = Mathf.Floor(time / num) * num;
		float num3 = num2 + num;
		MonoBehaviour.print("segment duration: " + num);
		MonoBehaviour.print("current time: " + time);
		MonoBehaviour.print("segment time: " + num2);
		MonoBehaviour.print("next time: " + num3);
		return num3 - time;
	}

	public void OnStopCosmicJamSession()
	{
		if (_animator != null)
		{
			_animator.SetBool("Playing", value: false);
		}
		if (_rockingChairAnimator != null)
		{
			_rockingChairAnimator.SetBool("Playing", value: false);
		}
		if (this.OnStopPlaying != null)
		{
			this.OnStopPlaying();
		}
		_dialogueTree.GetComponent<InteractReceiver>().EnableInteraction();
	}

	private void OnStartConversation()
	{
	}

	private void OnEndConversation()
	{
		if (!_isPlaying && DialogueConditionManager.SharedInstance.GetConditionState(_startPlayingCondition))
		{
			if (this.OnStartPlaying != null)
			{
				this.OnStartPlaying();
			}
			MonoBehaviour.print("start playing " + base.gameObject.name);
			_isPlaying = true;
			if (_animator != null)
			{
				_animator.SetBool("Playing", value: true);
				_animator.CrossFadeInFixedTime("PlayingInstrument", 0.25f, -1, _signal.GetOWAudioSource().time);
			}
			if (_rockingChairAnimator != null)
			{
				_rockingChairAnimator.SetBool("Playing", value: true);
			}
			_signal.SetSignalActivation(active: true);
			_dialogueTree.GetComponent<InteractReceiver>().DisableInteraction();
		}
	}

	protected virtual void OnEnableBigHeadMode()
	{
		if (_animator != null && !_animator.name.Contains("Esker"))
		{
			if (_animator.name.Contains("Chert"))
			{
				Transform boneTransform = _animator.GetBoneTransform(HumanBodyBones.UpperChest);
				Transform boneTransform2 = _animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
				Transform boneTransform3 = _animator.GetBoneTransform(HumanBodyBones.RightShoulder);
				boneTransform.localScale = new Vector3(2f, 2f, 2f);
				boneTransform2.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				boneTransform3.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			}
			else if (_animator.name.Contains("Gabbro") || _animator.name.Contains("Solanum"))
			{
				Transform boneTransform4 = _animator.GetBoneTransform(HumanBodyBones.Neck);
				Transform boneTransform5 = _animator.GetBoneTransform(HumanBodyBones.Head);
				boneTransform4.localScale = new Vector3(1.58f, 1.58f, 1.58f);
				boneTransform5.localScale = new Vector3(1.58f, 1.58f, 1.58f);
			}
			else if (_animator.name.Contains("Ghostbird"))
			{
				_animator.GetBoneTransform(HumanBodyBones.Head).localScale = new Vector3(2.5f, 2.5f, 2.5f);
			}
			else
			{
				_animator.GetBoneTransform(HumanBodyBones.Neck).localScale = new Vector3(2.5f, 2.5f, 2.5f);
			}
		}
	}
}
