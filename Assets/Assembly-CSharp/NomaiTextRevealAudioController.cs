using UnityEngine;

public class NomaiTextRevealAudioController : MonoBehaviour
{
	private OWAudioSource _audioSource;

	private NomaiWallText _wallText;

	private Transform _origParent;

	private void Start()
	{
		_origParent = base.transform.parent;
		_audioSource = GetComponent<OWAudioSource>();
		_audioSource.MakeVelocityUpdateDynamicOnPause();
		base.enabled = false;
	}

	private void OnDisable()
	{
		ReturnToPlayer();
	}

	public bool IsAvailable()
	{
		if (_wallText == null)
		{
			return !_audioSource.isPlaying;
		}
		return false;
	}

	public void PlayTextReveal(NomaiWallText wallText)
	{
		_wallText = wallText;
		base.transform.parent = wallText.transform;
		base.transform.localPosition = Vector3.zero;
		_audioSource.FadeIn(0.1f, fadeFromNothing: true, randomizePlayhead: true);
		base.enabled = true;
	}

	private void ReturnToPlayer()
	{
		_wallText = null;
		_audioSource.Stop();
		base.transform.parent = _origParent;
		base.transform.localPosition = Vector3.zero;
		base.enabled = false;
	}

	private void Update()
	{
		if (_wallText != null && !_wallText.IsAnyTextAnimating())
		{
			_audioSource.FadeOut(0.1f);
			_wallText = null;
		}
		else if (_wallText == null && !_audioSource.isPlaying)
		{
			base.enabled = false;
		}
	}
}
