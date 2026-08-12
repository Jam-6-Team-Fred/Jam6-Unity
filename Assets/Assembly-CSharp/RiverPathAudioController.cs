using UnityEngine;

public abstract class RiverPathAudioController : MonoBehaviour
{
	[SerializeField]
	private BaseRiverAudioPath _path;

	[SerializeField]
	private OWTriggerVolume _activationVolume;

	protected OWRigidbody _parentBody;

	protected Transform _playerTransform;

	protected RiverPathAudioSource _activeSource;

	protected RiverPathAudioSource[] _riverAudioSources;

	private int _muffleCount;

	private int _muteCount;

	private bool _audioModifiersDirty;

	private Vector3 _cachedTargetPosition = Vector3.zero;

	private int _currentTriangleIdx = -1;

	private int _currentSegmentIdx;

	private Vector3 _currentDesiredPoint;

	private BaseRiverAudioPath.Triangle _currentTriangle;

	protected virtual void Awake()
	{
		_activationVolume.OnEntry += OnEnterActivationVolume;
		_activationVolume.OnExit += OnExitActivationVolume;
	}

	protected virtual void Start()
	{
		_parentBody = this.GetAttachedOWRigidbody();
		_playerTransform = Locator.GetPlayerController().transform;
		base.enabled = false;
	}

	protected virtual void OnDestroy()
	{
		_activationVolume.OnEntry -= OnEnterActivationVolume;
		_activationVolume.OnExit -= OnExitActivationVolume;
	}

	public bool IsMuted()
	{
		return _muteCount > 0;
	}

	public void AddAudioModifier(AudioModifier riverModifer)
	{
		switch (riverModifer)
		{
		case AudioModifier.Muffle:
			_muffleCount++;
			_audioModifiersDirty = true;
			break;
		case AudioModifier.Mute:
			_muteCount++;
			_audioModifiersDirty = true;
			break;
		}
	}

	public void RemoveAudioModifier(AudioModifier riverModifer)
	{
		switch (riverModifer)
		{
		case AudioModifier.Muffle:
			_muffleCount = Mathf.Max(_muffleCount - 1, 0);
			_audioModifiersDirty = true;
			break;
		case AudioModifier.Mute:
			_muteCount = Mathf.Max(_muteCount - 1, 0);
			_audioModifiersDirty = true;
			break;
		}
	}

	protected abstract RiverPathAudioSource GetDefaultRiverSource();

	protected abstract void OnActivationUpdated(bool active);

	protected void SetActiveRiverSource(RiverPathAudioSource riverSource)
	{
		if (_activeSource != null)
		{
			_activeSource.SetPlaying(play: false);
			_activeSource.SetMuffled(muffle: false);
		}
		_activeSource = riverSource;
		_activeSource.SetMuffled(_muffleCount > 0);
		_activeSource.SetPlaying(_muteCount == 0);
	}

	protected virtual void FixedUpdate()
	{
		if (_audioModifiersDirty)
		{
			_audioModifiersDirty = false;
			if (_activeSource != null)
			{
				_activeSource.SetMuffled(_muffleCount > 0);
				_activeSource.SetPlaying(_muteCount == 0);
			}
		}
		Vector3 normal;
		bool inside;
		Vector3 waterPoint = _path.GetWaterPoint(_playerTransform.position, ref _currentTriangleIdx, ref _currentSegmentIdx, out _currentTriangle, out normal, out _currentDesiredPoint, out inside);
		for (int i = 0; i < _riverAudioSources.Length; i++)
		{
			_riverAudioSources[i].UpdatePosition(waterPoint, inside, _playerTransform.position, _parentBody);
		}
		_cachedTargetPosition = waterPoint;
	}

	private void OnEnterActivationVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = true;
			OnActivationUpdated(active: true);
			if (_activeSource == null)
			{
				SetActiveRiverSource(GetDefaultRiverSource());
			}
		}
	}

	private void OnExitActivationVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = false;
			_activeSource = null;
			_audioModifiersDirty = false;
			for (int i = 0; i < _riverAudioSources.Length; i++)
			{
				_riverAudioSources[i].SetMuffled(muffle: false);
				_riverAudioSources[i].SetPlaying(play: false);
			}
			OnActivationUpdated(active: false);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(_cachedTargetPosition, 0.5f);
	}
}
