using System;
using UnityEngine;

public class RotatingDoor : AbstractDoor
{
	[SerializeField]
	private Transform[] _doorPannelRight;

	[SerializeField]
	private Transform[] _doorPannelLeft;

	[SerializeField]
	private float _openRotation = 90f;

	[Space]
	[SerializeField]
	private float _openingSpeed = 60f;

	[SerializeField]
	private float _closingSpeed = 60f;

	[Header("Audio")]
	[SerializeField]
	private OWAudioSource _loopingAudio;

	[SerializeField]
	private OWAudioSource _oneShotAudio;

	[SerializeField]
	private AudioType _openStartClip = AudioType.Door_OpenStart;

	[SerializeField]
	private AudioType _openStopClip = AudioType.Door_OpenStop;

	[SerializeField]
	private AudioType _closeStartClip = AudioType.Door_CloseStart;

	[SerializeField]
	private AudioType _closeStopClip = AudioType.Door_CloseStop;

	[Header("Occlusion")]
	[SerializeField]
	private VolumeOcclusionLight _occlusionLight;

	private Vector2 _occlusionLightBaseStartSize;

	private Vector2 _occlusionLightBaseEndSize;

	public OWEvent OnOpenFinish = new OWEvent(1);

	public OWEvent OnCloseFinish = new OWEvent(1);

	protected override void Start()
	{
		base.Start();
		if (_occlusionLight != null)
		{
			_occlusionLightBaseStartSize = _occlusionLight.startSize;
			_occlusionLightBaseEndSize = _occlusionLight.endSize;
			if (!_startOpen)
			{
				_occlusionLight.startSize = new Vector2(0f, _occlusionLightBaseStartSize.y);
				_occlusionLight.endSize = new Vector2(0f, _occlusionLightBaseEndSize.y);
			}
		}
	}

	public override void SetOpenImmediate(bool open)
	{
		if (_open != open)
		{
			base.SetOpenImmediate(open);
			for (int i = 0; i < _doorPannelRight.Length; i++)
			{
				_doorPannelRight[i].localEulerAngles = (open ? new Vector3(0f, _openRotation, 0f) : Vector3.zero);
			}
			for (int j = 0; j < _doorPannelRight.Length; j++)
			{
				_doorPannelLeft[j].localEulerAngles = (open ? new Vector3(0f, 0f - _openRotation, 0f) : Vector3.zero);
			}
			if (_occlusionLight != null)
			{
				_occlusionLight.startSize = (_startOpen ? _occlusionLightBaseStartSize : new Vector2(0f, _occlusionLightBaseStartSize.y));
				_occlusionLight.endSize = (_startOpen ? _occlusionLightBaseEndSize : new Vector2(0f, _occlusionLightBaseEndSize.y));
			}
		}
	}

	public override void Open()
	{
		if (!_open)
		{
			if (!base.enabled && _oneShotAudio != null && _loopingAudio != null)
			{
				_oneShotAudio.PlayOneShot(_openStartClip);
				_loopingAudio.FadeInToLibraryVolume(0.2f);
			}
			base.Open();
		}
	}

	public override void Close()
	{
		if (_open)
		{
			if (!base.enabled && _oneShotAudio != null && _loopingAudio != null)
			{
				_oneShotAudio.PlayOneShot(_closeStartClip);
				_loopingAudio.FadeInToLibraryVolume(0.2f);
			}
			base.Close();
		}
	}

	private void FixedUpdate()
	{
		float num = (_open ? _openingSpeed : (0f - _closingSpeed));
		for (int i = 0; i < _doorPannelRight.Length; i++)
		{
			_doorPannelRight[i].Rotate(new Vector3(0f, num * Time.deltaTime, 0f));
		}
		for (int j = 0; j < _doorPannelRight.Length; j++)
		{
			_doorPannelLeft[j].Rotate(new Vector3(0f, (0f - num) * Time.deltaTime, 0f));
		}
		if (_occlusionLight != null)
		{
			float num2 = Mathf.Clamp01(Mathf.Abs(Mathf.DeltaAngle(0f, _doorPannelRight[0].localEulerAngles.y) / 120f));
			float num3 = 1f - Mathf.Cos(num2 * (float)Math.PI * 0.5f);
			_occlusionLight.startSize = new Vector2(_occlusionLightBaseStartSize.x * num3, _occlusionLightBaseStartSize.y);
			_occlusionLight.endSize = new Vector2(_occlusionLightBaseEndSize.x * num3, _occlusionLightBaseEndSize.y);
		}
		float num4 = _doorPannelRight[0].localRotation.eulerAngles.y % 360f;
		bool flag = false;
		if (!(_open ? ((!(num > 0f)) ? (num4 <= _openRotation && num4 > 1f) : (num4 >= _openRotation && num4 < 359f)) : ((!(num > 0f)) ? (num4 <= 360f && num4 > _openRotation + 1f) : (num4 >= 0f && num4 < _openRotation - 1f))))
		{
			return;
		}
		for (int k = 0; k < _doorPannelRight.Length; k++)
		{
			_doorPannelRight[k].localEulerAngles = (_open ? new Vector3(0f, _openRotation, 0f) : Vector3.zero);
		}
		for (int l = 0; l < _doorPannelRight.Length; l++)
		{
			_doorPannelLeft[l].localEulerAngles = (_open ? new Vector3(0f, 0f - _openRotation, 0f) : Vector3.zero);
		}
		base.enabled = false;
		if (_open)
		{
			OnOpenFinish.Invoke();
		}
		else
		{
			OnCloseFinish.Invoke();
		}
		if (_oneShotAudio != null && _loopingAudio != null)
		{
			if (_open)
			{
				_oneShotAudio.PlayOneShot(_openStopClip);
			}
			else
			{
				_oneShotAudio.PlayOneShot(_closeStopClip);
			}
			_loopingAudio.FadeOut(0.2f);
		}
		if (_occlusionLight != null)
		{
			_occlusionLight.startSize = (_open ? _occlusionLightBaseStartSize : new Vector2(0f, _occlusionLightBaseStartSize.y));
			_occlusionLight.endSize = (_open ? _occlusionLightBaseEndSize : new Vector2(0f, _occlusionLightBaseEndSize.y));
		}
	}
}
