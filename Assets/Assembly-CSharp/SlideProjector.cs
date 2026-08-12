using System;
using System.Collections.Generic;
using UnityEngine;

public class SlideProjector : SectoredMonoBehaviour
{
	public struct DisplayCookieRtUsage
	{
		public SlideProjector slideProjector;

		public RenderTexture renderTexture;
	}

	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private Transform _lockOnTransform;

	[SerializeField]
	private SlideProjectorSocket _socket;

	[SerializeField]
	private LightSensor _lightSensor;

	[Header("Flood")]
	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[SerializeField]
	private OWLightController _gearLightController;

	[Space]
	[SerializeField]
	private OWLight2 _light;

	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private OWRendererFadeController _lightShaftController;

	[Space]
	[SerializeField]
	protected OWLightController _houseLightController;

	[SerializeField]
	private OWLightController _bounceLightController;

	[Space]
	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private Texture _lightSensorMask;

	[SerializeField]
	private OWTriggerVolume _slideTextureStreamingTrigger;

	[SerializeField]
	private GearInterfaceEffects _gearInterface;

	private SimpleLanternItem _lanternItem;

	private SlideReelItem _slideItem;

	private Texture _origCookie;

	protected Texture _slideToDisplay;

	protected RenderTexture _displayCookie;

	protected Material _cookieBlitMaterial;

	private ScreenPrompt _forwardPrompt;

	private ScreenPrompt _reversePrompt;

	private ScreenPrompt _leavePrompt;

	private ScreenPrompt _centerForwardPrompt;

	private ScreenPrompt _centerReversePrompt;

	private bool _playerOrProbeInSector;

	private bool _timeFrozen;

	private bool _hasUsedProjector;

	private float _origSpotIntensity;

	public static List<DisplayCookieRtUsage> displayCookiePool;

	protected override void Awake()
	{
		base.Awake();
		InitDisplayCookiePool();
		_origCookie = _light.GetLight().cookie;
		_slideToDisplay = _origCookie;
		_cookieBlitMaterial = new Material(Shader.Find("Hidden/SlideMasking"));
		_cookieBlitMaterial.name = "SlideProjector_CookieBlitMaterial";
		_cookieBlitMaterial.SetTexture("_MaskTex", _lightSensorMask);
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
			_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		}
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract += OnPressInteract;
		}
		if (_socket != null)
		{
			SlideProjectorSocket socket = _socket;
			socket.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Combine(socket.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnSocketablePlaced));
			SlideProjectorSocket socket2 = _socket;
			socket2.OnSocketableDonePlacing = (OWItemSocket.SocketEvent)Delegate.Combine(socket2.OnSocketableDonePlacing, new OWItemSocket.SocketEvent(OnSocketableDonePlacing));
			SlideProjectorSocket socket3 = _socket;
			socket3.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(socket3.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSocketableRemoved));
		}
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
		}
	}

	protected virtual void Start()
	{
		if (_bounceLightController != null)
		{
			_bounceLightController.SetIntensity(0f);
		}
		if (_interactReceiver != null)
		{
			_interactReceiver.SetPromptText(UITextType.UnknownInterfacePrompt);
		}
		_lightController.SetIntensity(0f);
		_lightShaftController.SetFade(0f);
		if (_houseLightController != null)
		{
			_houseLightController.SetIntensity(1f);
		}
		_slideItem = _socket.GetSocketedSlideReel();
		if (_slideItem != null)
		{
			_slideItem.slidesContainer.ResetSlideIndex();
		}
		_ = _slideTextureStreamingTrigger != null;
		_forwardPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.SlideProjectorForwardPrompt) + "   <CMD>");
		_reversePrompt = new ScreenPrompt(InputLibrary.toolActionSecondary, UITextLibrary.GetString(UITextType.SlideProjectorReversePrompt) + "   <CMD>");
		_leavePrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.LeavePrompt) + "   <CMD>");
		_hasUsedProjector = PlayerData.GetPersistentCondition("HAS_USED_SLIDE_PROJECTOR");
		if (!_hasUsedProjector)
		{
			_centerForwardPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, "<CMD>   " + UITextLibrary.GetString(UITextType.SlideProjectorForwardPrompt));
			_centerReversePrompt = new ScreenPrompt(InputLibrary.toolActionSecondary, "<CMD>   " + UITextLibrary.GetString(UITextType.SlideProjectorReversePrompt));
		}
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
			_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
		}
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract -= OnPressInteract;
		}
		if (_lanternItem != null)
		{
			_lanternItem.OnLanternExtinguished -= new OWEvent.OWCallback(OnLanternExtinguished);
		}
		if (_socket != null)
		{
			SlideProjectorSocket socket = _socket;
			socket.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Remove(socket.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnSocketablePlaced));
			SlideProjectorSocket socket2 = _socket;
			socket2.OnSocketableDonePlacing = (OWItemSocket.SocketEvent)Delegate.Remove(socket2.OnSocketableDonePlacing, new OWItemSocket.SocketEvent(OnSocketableDonePlacing));
			SlideProjectorSocket socket3 = _socket;
			socket3.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(socket3.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSocketableRemoved));
		}
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
		}
		if (_slideItem != null)
		{
			_slideItem.slidesContainer.onSlideTextureUpdated -= new OWEvent.OWCallback(OnSlideTextureUpdated);
			_slideItem.slidesContainer.onNeedBounceLightUpdate -= new OWEvent<LightParameters>.OWCallback(OnBounceLightUpdate);
			_slideItem.slidesContainer.onPlayBeatAudio -= new OWEvent<AudioType>.OWCallback(OnPlayBeatAudio);
		}
		_ = _slideTextureStreamingTrigger != null;
		ReleaseDisplayCookiePool();
	}

	private void Update()
	{
		bool flag = OWInput.IsInputMode(InputMode.SatelliteCam);
		if (!_hasUsedProjector)
		{
			_centerForwardPrompt.SetVisibility(flag);
			_centerReversePrompt.SetVisibility(flag);
		}
		else
		{
			_forwardPrompt.SetVisibility(flag);
			_reversePrompt.SetVisibility(flag);
		}
		_leavePrompt.SetVisibility(flag);
		bool flag2 = PlayerData.GetFreezeTimeWhileReadingTranslator() && Locator.GetPlayerController().IsGrounded() && !PlayerState.IsDead() && !Locator.GetGlobalMusicController().IsEndTimesPlaying();
		if (!_timeFrozen && flag2)
		{
			OWTime.Pause(OWTime.PauseType.Reading);
			_timeFrozen = true;
		}
		else if (_timeFrozen && !flag2)
		{
			OWTime.Unpause(OWTime.PauseType.Reading);
			_timeFrozen = false;
		}
		if (flag)
		{
			if (!_hasUsedProjector && (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary) || OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary)))
			{
				_hasUsedProjector = true;
				PlayerData.SetPersistentCondition("HAS_USED_SLIDE_PROJECTOR", state: true);
				Locator.GetPromptManager().RemoveScreenPrompt(_centerForwardPrompt);
				Locator.GetPromptManager().RemoveScreenPrompt(_centerReversePrompt);
			}
			if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary))
			{
				NextSlide();
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary))
			{
				PreviousSlide();
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.cancel))
			{
				CancelInteraction();
			}
		}
	}

	private void InitDisplayCookiePool()
	{
		if (displayCookiePool == null)
		{
			displayCookiePool = new List<DisplayCookieRtUsage>();
			RenderTexture renderTexture = new RenderTexture(1024, 1024, 0);
			renderTexture.name = "SlideProjector_DisplayCookieTexture " + displayCookiePool.Count;
			displayCookiePool.Add(new DisplayCookieRtUsage
			{
				slideProjector = null,
				renderTexture = renderTexture
			});
		}
	}

	protected void GetDisplayCookie()
	{
		for (int i = 0; i < displayCookiePool.Count; i++)
		{
			if (displayCookiePool[i].slideProjector == null)
			{
				_displayCookie = displayCookiePool[i].renderTexture;
				displayCookiePool[i] = new DisplayCookieRtUsage
				{
					slideProjector = this,
					renderTexture = _displayCookie
				};
				_light.GetLight().cookie = _displayCookie;
				BlitCookie();
				return;
			}
		}
		_displayCookie = new RenderTexture(1024, 1024, 0);
		_displayCookie.name = "SlideProjector_DisplayCookieTexture " + displayCookiePool.Count;
		displayCookiePool.Add(new DisplayCookieRtUsage
		{
			slideProjector = this,
			renderTexture = _displayCookie
		});
		_light.GetLight().cookie = _displayCookie;
		BlitCookie();
	}

	protected void FinishUsingDisplayCookie()
	{
		for (int i = 0; i < displayCookiePool.Count; i++)
		{
			if (displayCookiePool[i].renderTexture == _displayCookie)
			{
				displayCookiePool[i] = new DisplayCookieRtUsage
				{
					slideProjector = null,
					renderTexture = _displayCookie
				};
				_displayCookie = null;
				break;
			}
		}
	}

	private void ReleaseDisplayCookiePool()
	{
		if (displayCookiePool != null)
		{
			for (int i = 0; i < displayCookiePool.Count; i++)
			{
				displayCookiePool[i].renderTexture.Release();
			}
			displayCookiePool = null;
		}
	}

	private void NextSlide()
	{
		bool flag = false;
		if (_slideItem != null && _slideItem.slidesContainer.NextSlideAvailable())
		{
			flag = _slideItem.slidesContainer.IncreaseSlideIndex();
			if (flag)
			{
				if (_oneShotSource != null)
				{
					_oneShotSource.PlayOneShot(AudioType.Projector_Next);
				}
				if (IsProjectorFullyLit())
				{
					_slideItem.slidesContainer.SetCurrentRead();
					_slideItem.slidesContainer.TryPlayMusicForCurrentSlideTransition(forward: true);
				}
			}
		}
		if (_gearInterface != null)
		{
			float audioVolume = (flag ? 0f : 0.5f);
			_gearInterface.AddRotation(45f, audioVolume);
		}
	}

	private void PreviousSlide()
	{
		bool flag = false;
		if (_slideItem != null && _slideItem.slidesContainer.PrevSlideAvailable())
		{
			flag = _slideItem.slidesContainer.DecreaseSlideIndex();
			if (flag)
			{
				if (_oneShotSource != null)
				{
					_oneShotSource.PlayOneShot(AudioType.Projector_Prev);
				}
				if (IsProjectorFullyLit())
				{
					_slideItem.slidesContainer.SetCurrentRead();
					_slideItem.slidesContainer.TryPlayMusicForCurrentSlideTransition(forward: false);
				}
			}
		}
		if (_gearInterface != null)
		{
			float audioVolume = (flag ? 0f : 0.5f);
			_gearInterface.AddRotation(-45f, audioVolume);
		}
	}

	protected virtual bool IsProjectorLit()
	{
		if (!(_lanternItem == null) || !_lightSensor.IsIlluminated())
		{
			if (_lanternItem != null)
			{
				return _lanternItem.IsLit();
			}
			return false;
		}
		return true;
	}

	protected virtual bool IsProjectorFullyLit()
	{
		if (_lanternItem != null)
		{
			return _lanternItem.IsLit();
		}
		return false;
	}

	protected virtual void CheckLightStatus()
	{
		bool flag = _lanternItem != null && _lanternItem.IsLit();
		bool flag2 = flag || (_lanternItem == null && _lightSensor.IsIlluminated());
		float num = 1f;
		float num2 = 0f;
		if (flag2 && _slideItem != null)
		{
			SlideAmbientLightModule module = _slideItem.slidesContainer.GetCurrentSlide().GetModule<SlideAmbientLightModule>();
			if (module != null)
			{
				LightParameters lightParameters = module.GetLightParameters();
				num = lightParameters.intensity;
				num2 = lightParameters.spotIntensityMod;
			}
		}
		FadeProjectorLightTo(flag2 ? (1f + num2) : 0f, 0.2f);
		FadeBounceLightTo(flag2 ? num : 0f, 0.2f);
		if (_houseLightController != null)
		{
			_houseLightController.FadeTo(flag ? 0f : 1f, 1f);
		}
		if (flag2 && _displayCookie == null)
		{
			GetDisplayCookie();
		}
		else if (!flag2 && _displayCookie != null)
		{
			FinishUsingDisplayCookie();
		}
		BlitCookie();
	}

	protected void FadeProjectorLightTo(float fade, float duration)
	{
		if (_bounceLightController != null)
		{
			_bounceLightController.FadeTo(fade, duration);
		}
		_lightController.FadeTo(fade, duration);
		_lightShaftController.FadeTo(fade, duration);
	}

	private void FadeBounceLightTo(float fade, float duration)
	{
		if (_bounceLightController != null)
		{
			_bounceLightController.SetIntensity(fade);
		}
	}

	private void SetCookie(Texture cookieTexture)
	{
		if (cookieTexture == null)
		{
			_slideToDisplay = _origCookie;
			return;
		}
		_slideToDisplay = cookieTexture;
		BlitCookie();
	}

	protected virtual void BlitCookie()
	{
		if (!(_displayCookie == null))
		{
			bool flag = _lanternItem != null && _lanternItem.IsLit();
			_cookieBlitMaterial.SetFloat("_isMasked", flag ? 1 : 0);
			Graphics.Blit(_slideToDisplay, _displayCookie, _cookieBlitMaterial);
		}
	}

	private void OnPressInteract()
	{
		Debug.Log($"[SlideProjector] OnPressInteract IsProjectorLit: {IsProjectorLit()}");
		Locator.GetToolModeSwapper().UnequipTool();
		Vector3 localOffset = Vector3.zero;
		if (!(_slideItem != null) || !IsProjectorLit())
		{
			float value = Mathf.Abs(Mathf.Min(_interactReceiver.transform.InverseTransformPoint(Locator.GetPlayerTransform().position).z, 0f));
			float num = 2.2f * Mathf.InverseLerp(6f, 3f, value);
			localOffset = new Vector3(0f, 0f - num, 0f);
		}
		Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(_lockOnTransform, localOffset);
		GlobalMessenger.FireEvent("EnterSatelliteCameraMode");
		GlobalMessenger.FireEvent("StartViewingProjector");
		Locator.GetPromptManager().AddScreenPrompt(_forwardPrompt, PromptPosition.UpperRight, _hasUsedProjector);
		Locator.GetPromptManager().AddScreenPrompt(_reversePrompt, PromptPosition.UpperRight, _hasUsedProjector);
		Locator.GetPromptManager().AddScreenPrompt(_leavePrompt, PromptPosition.UpperRight, makeVisible: true);
		if (!_hasUsedProjector)
		{
			Locator.GetPromptManager().AddScreenPrompt(_centerForwardPrompt, PromptPosition.Center, makeVisible: true);
			Locator.GetPromptManager().AddScreenPrompt(_centerReversePrompt, PromptPosition.Center, makeVisible: true);
		}
		if (_slideItem != null && IsProjectorFullyLit())
		{
			_slideItem.slidesContainer.TryPlayMusicForCurrentSlideInclusive();
		}
		base.enabled = true;
	}

	private void CancelInteraction()
	{
		Locator.GetPromptManager().RemoveScreenPrompt(_forwardPrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_reversePrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_leavePrompt);
		if (!_hasUsedProjector)
		{
			Locator.GetPromptManager().RemoveScreenPrompt(_centerForwardPrompt);
			Locator.GetPromptManager().RemoveScreenPrompt(_centerReversePrompt);
		}
		Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().BreakLock();
		_interactReceiver.ResetInteraction();
		Locator.GetSlideReelMusicManager().OnExitSlideProjector();
		GlobalMessenger.FireEvent("ExitSatelliteCameraMode");
		GlobalMessenger.FireEvent("EndViewingProjector");
		if (_timeFrozen)
		{
			_timeFrozen = false;
			OWTime.Unpause(OWTime.PauseType.Reading);
		}
		base.enabled = false;
	}

	private void OnSocketablePlaced(OWItem item)
	{
		if (item.GetItemType() == ItemType.Lantern)
		{
			_lanternItem = (SimpleLanternItem)item;
			if (_lanternItem.IsLit())
			{
				_lanternItem.OnLanternExtinguished += new OWEvent.OWCallback(OnLanternExtinguished);
				if (_slideItem != null)
				{
					_slideItem.slidesContainer.ResetSlideIndex();
				}
			}
			CheckLightStatus();
		}
		else if (item.GetItemType() == ItemType.SlideReel)
		{
			_slideItem = (SlideReelItem)item;
			_slideItem.slidesContainer.onSlideTextureUpdated += new OWEvent.OWCallback(OnSlideTextureUpdated);
			_slideItem.slidesContainer.onNeedBounceLightUpdate += new OWEvent<LightParameters>.OWCallback(OnBounceLightUpdate);
			_slideItem.slidesContainer.onPlayBeatAudio += new OWEvent<AudioType>.OWCallback(OnPlayBeatAudio);
			_slideItem.slidesContainer.ResetSlideIndex();
			_slideItem.enabled = true;
			if (IsProjectorFullyLit())
			{
				_slideItem.slidesContainer.SetCurrentRead();
			}
			if (_slideItem.slidesContainer.streamingTexturesAvailable)
			{
				_slideItem.slidesContainer.LoadStreamingTextures();
			}
		}
	}

	private void OnSocketableDonePlacing(OWItem item)
	{
		if (item.GetItemType() == ItemType.SlideReel)
		{
			SetCookie(_slideItem.slidesContainer.GetCurrentSlideTexture());
		}
	}

	private void OnSocketableRemoved(OWItem item)
	{
		if (item.GetItemType() == ItemType.Lantern)
		{
			if (item == _lanternItem)
			{
				_lanternItem.OnLanternExtinguished -= new OWEvent.OWCallback(OnLanternExtinguished);
				_lanternItem = null;
			}
			else
			{
				Debug.LogError("Something has gone horribly wrong");
				Debug.Break();
			}
			CheckLightStatus();
		}
		else if (item.GetItemType() == ItemType.SlideReel)
		{
			Debug.Log($"[SlideProjector] OnSocketableRemoved IsProjectorLit: {IsProjectorLit()}");
			_slideItem.slidesContainer.onSlideTextureUpdated -= new OWEvent.OWCallback(OnSlideTextureUpdated);
			_slideItem.slidesContainer.onNeedBounceLightUpdate -= new OWEvent<LightParameters>.OWCallback(OnBounceLightUpdate);
			_slideItem.slidesContainer.onPlayBeatAudio -= new OWEvent<AudioType>.OWCallback(OnPlayBeatAudio);
			_slideItem.slidesContainer.ResetSlideIndex();
			_slideItem.Removed();
			_slideItem = null;
			SetCookie(_origCookie);
			ResetBounceLights();
		}
	}

	private void OnSlideTextureUpdated()
	{
		SetCookie(_slideItem.slidesContainer.GetCurrentSlideTexture());
	}

	private void OnBounceLightUpdate(LightParameters lightParams)
	{
		bool flag = _bounceLightController != null;
		if (flag)
		{
			for (int i = 0; i < _bounceLightController.lights.Length; i++)
			{
				_bounceLightController.lights[i].range = lightParams.range;
				_bounceLightController.lights[i].GetLight().color = lightParams.color;
			}
		}
		if (IsProjectorLit())
		{
			if (flag)
			{
				FadeBounceLightTo(lightParams.intensity, 0.2f);
			}
			_lightController.SetIntensity(1f + lightParams.spotIntensityMod);
		}
	}

	private void ResetBounceLights()
	{
		if (_bounceLightController != null)
		{
			FadeBounceLightTo(1f, 0.2f);
			for (int i = 0; i < _bounceLightController.lights.Length; i++)
			{
				_bounceLightController.lights[i].range = 10f;
				_bounceLightController.lights[i].GetLight().color = Color.white;
			}
		}
		_lightController.SetIntensity(IsProjectorLit() ? 1 : 0);
	}

	private void OnPlayBeatAudio(AudioType audioType)
	{
		if (IsProjectorFullyLit())
		{
			Locator.GetSlideReelMusicManager().PlayBeat(audioType);
		}
	}

	private void OnFloodImpact()
	{
		if (base.enabled)
		{
			CancelInteraction();
		}
		_interactReceiver.SetInteractionEnabled(enable: false);
		if (_gearLightController != null)
		{
			_gearLightController.FadeTo(0f, 0.5f);
		}
		if (_slideItem != null && _slideItem.slidesContainer.streamingTexturesAvailable)
		{
			if (_slideItem.slidesContainer.streamingTexturesAvailable)
			{
				_slideItem.slidesContainer.UnloadStreamingTextures();
			}
			_slideItem.enabled = false;
		}
	}

	private void OnLanternExtinguished()
	{
		CheckLightStatus();
	}

	private void OnDetectLight()
	{
		CheckLightStatus();
	}

	private void OnDetectDarkness()
	{
		CheckLightStatus();
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool playerOrProbeInSector = _playerOrProbeInSector;
		_playerOrProbeInSector = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (playerOrProbeInSector == _playerOrProbeInSector || !(_slideItem != null) || !_slideItem.slidesContainer.streamingTexturesAvailable)
		{
			return;
		}
		if (_playerOrProbeInSector)
		{
			_slideItem.slidesContainer.LoadStreamingTextures();
			_slideItem.enabled = true;
			if (!_slideItem.slidesContainer.IsAutoLoadStreamingSlides())
			{
				_slideItem.slidesContainer.RequestManualStreamSlides();
			}
		}
		else
		{
			_slideItem.enabled = false;
			_slideItem.slidesContainer.UnloadStreamingTextures();
		}
	}
}
