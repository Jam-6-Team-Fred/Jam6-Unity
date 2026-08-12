using UnityEngine;

public class ChertTravelerController : TravelerController
{
	[Space]
	[SerializeField]
	private float _nervousTime = 11f;

	[SerializeField]
	private float _panicTime = 17f;

	[SerializeField]
	private float _catatonicTime = 20.5f;

	[SerializeField]
	private float _crossfadeTime = 1f;

	private ChertMood _mood;

	private float _moodWeight;

	protected override void Awake()
	{
		base.Awake();
		base.enabled = false;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.OnSectorOccupantsUpdated();
		if (_animator != null)
		{
			base.enabled = _animator.enabled;
		}
	}

	private void LateUpdate()
	{
		if (!_talking)
		{
			float minutesElapsed = TimeLoop.GetMinutesElapsed();
			if (minutesElapsed >= _catatonicTime)
			{
				_mood = ChertMood.Catatonia;
			}
			else if (minutesElapsed >= _panicTime)
			{
				_mood = ChertMood.Panicked;
			}
			else if (minutesElapsed >= _nervousTime)
			{
				_mood = ChertMood.Nervous;
			}
			else
			{
				_mood = ChertMood.Chipper;
			}
			_moodWeight = Mathf.MoveTowards(_moodWeight, (float)_mood, Time.deltaTime / _crossfadeTime);
			_animator.SetFloat("Mood", _moodWeight);
		}
	}

	protected override void StartConversation()
	{
		base.StartConversation();
		_moodWeight = (float)_mood;
	}

	protected override void OnEnableBigHeadMode()
	{
		Transform boneTransform = _animator.GetBoneTransform(HumanBodyBones.UpperChest);
		Transform boneTransform2 = _animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
		Transform boneTransform3 = _animator.GetBoneTransform(HumanBodyBones.RightShoulder);
		boneTransform.localScale = new Vector3(2f, 2f, 2f);
		boneTransform2.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		boneTransform3.localScale = new Vector3(0.5f, 0.5f, 0.5f);
	}
}
