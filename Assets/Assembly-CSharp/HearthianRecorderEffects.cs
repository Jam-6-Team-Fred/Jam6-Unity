using UnityEngine;

[RequireComponent(typeof(CharacterDialogueTree))]
public class HearthianRecorderEffects : MonoBehaviour
{
	private CharacterDialogueTree _characterDialogueTree;

	[SerializeField]
	private Transform _reel1Transform;

	[SerializeField]
	private float _reel1Speed = 180f;

	[SerializeField]
	private Transform _reel2Transform;

	[SerializeField]
	private float _reel2Speed = 180f;

	[SerializeField]
	private float _reelAcceleration = 1f;

	private bool _playing;

	private float _reelSpeedFactor;

	private void Awake()
	{
		_characterDialogueTree = GetComponent<CharacterDialogueTree>();
		_characterDialogueTree.OnStartConversation += OnPlayRecorder;
		_characterDialogueTree.OnEndConversation += OnStopRecorder;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_characterDialogueTree.OnStartConversation -= OnPlayRecorder;
		_characterDialogueTree.OnEndConversation -= OnStopRecorder;
	}

	private void OnPlayRecorder()
	{
		_playing = true;
		base.enabled = true;
	}

	private void OnStopRecorder()
	{
		_playing = false;
	}

	private void Update()
	{
		_reelSpeedFactor = Mathf.MoveTowards(_reelSpeedFactor, _playing ? 1f : 0f, _reelAcceleration * Time.deltaTime);
		_reel1Transform.Rotate(Vector3.up, _reel1Speed * (_reelSpeedFactor * _reelSpeedFactor) * Time.deltaTime, Space.Self);
		_reel2Transform.Rotate(Vector3.up, _reel2Speed * (_reelSpeedFactor * _reelSpeedFactor) * Time.deltaTime, Space.Self);
		if (!_playing && _reelSpeedFactor <= 0f)
		{
			base.enabled = false;
		}
	}
}
