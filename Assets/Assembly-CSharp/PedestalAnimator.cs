using UnityEngine;

[RequireComponent(typeof(Animation))]
public class PedestalAnimator : MonoBehaviour
{
	public enum PedestalState
	{
		Open = 0,
		Opening = 1,
		Closed = 2,
		Closing = 3
	}

	public delegate void PedestalEvent();

	private Animation _animation;

	private AnimationState _animState;

	private PedestalState _pedestalState;

	private bool _pedestalContact;

	public PedestalEvent OnPedestalClosed;

	public PedestalEvent OnPedestalOpened;

	public PedestalEvent OnPedestalContact;

	private OWAudioSource _audioSource;

	private void Awake()
	{
		_animation = GetComponent<Animation>();
		_audioSource = GetComponentInChildren<OWAudioSource>();
		_animState = _animation["NomaiSharedPedestal_Move"];
		_pedestalState = PedestalState.Open;
		_pedestalContact = false;
	}

	private void Start()
	{
	}

	private void Update()
	{
		switch (_pedestalState)
		{
		case PedestalState.Open:
			base.enabled = false;
			break;
		case PedestalState.Opening:
			if (_pedestalContact && _animState.time < 2f)
			{
				_pedestalContact = false;
			}
			if (!_animation.isPlaying)
			{
				_pedestalState = PedestalState.Open;
				if (OnPedestalOpened != null)
				{
					OnPedestalOpened();
				}
			}
			break;
		case PedestalState.Closed:
			base.enabled = false;
			break;
		case PedestalState.Closing:
			if (!_pedestalContact && _animState.time >= 2f)
			{
				_pedestalContact = true;
				if (_audioSource != null)
				{
					_audioSource.PlayOneShot(AudioType.NomaiPedestalContact);
					_audioSource.FadeOut(0.2f);
				}
				if (OnPedestalContact != null)
				{
					OnPedestalContact();
				}
			}
			if (!_animation.isPlaying)
			{
				_pedestalState = PedestalState.Closed;
				if (OnPedestalClosed != null)
				{
					OnPedestalClosed();
				}
			}
			break;
		}
	}

	public bool IsPlaying()
	{
		return _animation.isPlaying;
	}

	public PedestalState GetPedestalState()
	{
		return _pedestalState;
	}

	public bool HasMadeContact()
	{
		return _pedestalContact;
	}

	public void PlayClose()
	{
		_animState.speed = 1f;
		_pedestalState = PedestalState.Closing;
		_animation.Play();
		base.enabled = true;
	}

	public void PlayOpen()
	{
		_animState.speed = -1f;
		if (_pedestalState == PedestalState.Closed)
		{
			_animState.normalizedTime = 1f;
		}
		_pedestalState = PedestalState.Opening;
		_animation.Play();
		base.enabled = true;
	}

	public void SetClosed()
	{
		_animation.Stop();
		_animState.normalizedTime = 1f;
		_pedestalState = PedestalState.Closed;
		if (!_pedestalContact && OnPedestalContact != null)
		{
			OnPedestalContact();
		}
		if (OnPedestalClosed != null)
		{
			OnPedestalClosed();
		}
	}

	public void SetOpen()
	{
		_animation.Stop();
		_animState.normalizedTime = 0f;
		_pedestalState = PedestalState.Open;
		if (OnPedestalOpened != null)
		{
			OnPedestalOpened();
		}
	}

	public void OnPedestalMovement(float value)
	{
		if (_audioSource == null)
		{
			return;
		}
		if (value == 1f)
		{
			if (_pedestalState == PedestalState.Closing)
			{
				_audioSource.FadeOut(0.2f);
			}
			else if (_pedestalState == PedestalState.Opening && (!_audioSource.isPlaying || _audioSource.IsFadingOut()))
			{
				_audioSource.FadeIn(0.5f);
			}
		}
		if (value == 0f)
		{
			if (_pedestalState == PedestalState.Opening)
			{
				_audioSource.FadeOut(0.2f);
			}
			else if (_pedestalState == PedestalState.Closing && (!_audioSource.isPlaying || _audioSource.IsFadingOut()))
			{
				_audioSource.FadeIn(0.5f);
			}
		}
	}
}
