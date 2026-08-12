using UnityEngine;
using UnityEngine.Rendering;

public class IllusoryWall : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _triggerVolume;

	[SerializeField]
	private float _fadeOutDuration;

	[SerializeField]
	private float _fadeInDuration;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private bool _toggleShadowCasting = true;

	private OWRenderer[] _renderers;

	private float _fadeStartTime;

	private bool _visible = true;

	private float _visibleFraction = 1f;

	private void Awake()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		_renderers = new OWRenderer[componentsInChildren.Length];
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i] = componentsInChildren[i].GetComponent<OWRenderer>();
			if (_renderers[i] == null)
			{
				_renderers[i] = componentsInChildren[i].gameObject.AddComponent<OWRenderer>();
			}
		}
		_triggerVolume.OnEntry += OnEnterVolume;
		_triggerVolume.OnExit += OnExitVolume;
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEnterVolume;
		_triggerVolume.OnExit -= OnExitVolume;
	}

	private void Update()
	{
		float num = (_visible ? 1f : 0f);
		float num2 = (_visible ? _fadeInDuration : _fadeOutDuration);
		float visibleFraction = _visibleFraction;
		_visibleFraction = (Time.time - _fadeStartTime) / num2;
		_visibleFraction = (_visible ? _visibleFraction : (1f - _visibleFraction));
		if (Mathf.Sign(visibleFraction - num) != Mathf.Sign(_visibleFraction - num))
		{
			_visibleFraction = num;
			base.enabled = false;
			if (_visible)
			{
				UpdateShadowCasting();
			}
		}
		UpdateDithering();
	}

	private void UpdateShadowCasting()
	{
		if (_toggleShadowCasting)
		{
			for (int i = 0; i < _renderers.Length; i++)
			{
				_renderers[i].GetRenderer().shadowCastingMode = (_visible ? ShadowCastingMode.On : ShadowCastingMode.Off);
			}
		}
	}

	private void UpdateDithering()
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			if (_renderers[i] != null)
			{
				_renderers[i].SetDitherFade(1f - _visibleFraction);
			}
		}
	}

	private void SetVisible(bool visible)
	{
		if (_visible != visible)
		{
			_visible = visible;
			if (!_visible)
			{
				UpdateShadowCasting();
			}
			_fadeStartTime = Time.time;
			base.enabled = true;
		}
	}

	private void PlayEffectSound()
	{
		AudioType type = (_visible ? AudioType.IllusoryWall_Exit : AudioType.IllusoryWall_Enter);
		_oneShotSource.PlayOneShot(type);
	}

	private void OnEnterVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerCameraDetector"))
		{
			SetVisible(visible: false);
			PlayEffectSound();
		}
	}

	private void OnExitVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerCameraDetector"))
		{
			SetVisible(visible: true);
			PlayEffectSound();
		}
	}
}
