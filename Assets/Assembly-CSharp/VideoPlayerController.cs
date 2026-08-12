using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoPlayerController : SectoredMonoBehaviour
{
	private VideoPlayer _videoPlayer;

	[SerializeField]
	private AudioSource _audioSource;

	[SerializeField]
	private string _streamingVideoFileName;

	[SerializeField]
	private AudioType _splitAudioType;

	private OWAudioSource _owSplitAudioSource;

	private double _videoTime;

	private bool _checkingForLoopPoint;

	private bool _isPlaying;

	private bool _isPreparing;

	protected override void Awake()
	{
		base.Awake();
		_videoPlayer = GetComponent<VideoPlayer>();
		_videoPlayer.waitForFirstFrame = true;
		_videoPlayer.errorReceived += OnVideoPlayerError;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (flag && !_videoPlayer.isPlaying)
		{
			if (!_isPreparing)
			{
				_isPreparing = true;
				_videoPlayer.prepareCompleted += OnVideoPrepareCompleted;
				PrepareVideo();
			}
		}
		else if (!flag && _videoPlayer.isPlaying)
		{
			_isPlaying = false;
			_videoPlayer.Stop();
			if (_owSplitAudioSource != null)
			{
				_owSplitAudioSource.Stop();
			}
			if (_checkingForLoopPoint)
			{
				_videoPlayer.loopPointReached -= OnVideoLoopPointReached;
				_checkingForLoopPoint = false;
			}
		}
	}

	protected void PrepareVideo()
	{
		Debug.Log("START VideoPlayerController.PrepareVideo: " + _streamingVideoFileName);
		_videoPlayer.source = VideoSource.VideoClip;
		_videoPlayer.clip = null;
		_videoPlayer.url = Application.streamingAssetsPath + "/Video/" + _streamingVideoFileName;
		if (_splitAudioType != 0)
		{
			if (_videoPlayer.audioOutputMode != 0)
			{
				_videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
			}
			if (_audioSource == null)
			{
				Debug.LogError("VideoPlayerController: Split Audio/Video playback requires an AudioSource!");
			}
			else
			{
				_owSplitAudioSource = _audioSource.GetComponent<OWAudioSource>();
				if (_owSplitAudioSource == null)
				{
					Debug.LogError("VideoPlayerController: Split Audio/Video playback requires an OWAudioSource!");
				}
				else
				{
					_owSplitAudioSource.AssignAudioLibraryClip(_splitAudioType);
				}
			}
		}
		if (_videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
		{
			_videoPlayer.SetTargetAudioSource(0, _audioSource);
		}
		_videoPlayer.timeReference = VideoTimeReference.ExternalTime;
		_videoPlayer.externalReferenceTime = 0.0;
		_videoPlayer.timeReference = VideoTimeReference.Freerun;
		_videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		if (_videoPlayer.targetTexture == null)
		{
			Debug.LogError("VideoPlayerController.PrepareVideo--Player target texture is null!");
		}
		_videoPlayer.Prepare();
		Debug.Log("END VideoPlayerController.PrepareVideo: " + _streamingVideoFileName);
	}

	protected void OnVideoPrepareCompleted(VideoPlayer source)
	{
		Debug.Log("OnVideoPrepareCompleted " + _streamingVideoFileName);
		_videoPlayer.prepareCompleted -= OnVideoPrepareCompleted;
		_isPreparing = false;
		if (!_checkingForLoopPoint && _videoPlayer.isLooping)
		{
			_videoPlayer.loopPointReached += OnVideoLoopPointReached;
			_checkingForLoopPoint = true;
		}
		_isPlaying = true;
		_videoPlayer.Play();
		if (_owSplitAudioSource != null)
		{
			_owSplitAudioSource.Play();
		}
	}

	protected virtual void Update()
	{
		if (_videoPlayer.timeReference == VideoTimeReference.Freerun)
		{
			_videoPlayer.timeReference = VideoTimeReference.ExternalTime;
		}
		else if (_videoPlayer.timeReference == VideoTimeReference.ExternalTime && _isPlaying)
		{
			_videoTime += Time.deltaTime;
			_videoPlayer.externalReferenceTime = _videoTime;
		}
	}

	private void OnVideoLoopPointReached(VideoPlayer source)
	{
		_videoTime = 0.0;
		if (_owSplitAudioSource != null)
		{
			_owSplitAudioSource.Play();
		}
	}

	private void OnVideoPlayerError(VideoPlayer source, string message)
	{
		Debug.LogError(message);
		if (_isPreparing)
		{
			Debug.LogError("Failed while preparing video");
			_isPreparing = false;
			_videoPlayer.prepareCompleted -= OnVideoPrepareCompleted;
		}
	}
}
