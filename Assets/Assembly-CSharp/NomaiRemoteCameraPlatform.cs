using System;
using System.Collections.Generic;
using UnityEngine;

public class NomaiRemoteCameraPlatform : NomaiShared
{
	public enum ID
	{
		None = 0,
		SunStation = 1,
		HGT_TimeLoop = 2,
		THM_EyeLocator = 4,
		TH_Mine = 5,
		BH_Observatory = 6,
		BH_GravityCannon = 7,
		BH_QuantumFragment = 8,
		BH_BlackHoleForge = 9,
		GD_ConstructionYardIsland1 = 10,
		GD_ConstructionYardIsland2 = 11,
		GD_StatueIsland = 12,
		GD_ProbeCannonSunkenModule = 13,
		GD_ProbeCannonDamagedModule = 14,
		GD_ProbeCannonIntactModule = 15,
		VM_Interior = 16,
		HGT_TLE = 17,
		BH_NorthPole = 18
	}

	public enum State
	{
		Connecting_FadeIn = 0,
		Connecting_FadeOut = 1,
		Connected = 2,
		Disconnecting_FadeIn = 3,
		Disconnecting_FadeOut = 4,
		Disconnected = 5
	}

	private static List<NomaiRemoteCameraPlatform> s_platforms;

	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_Fade;

	private static int s_propID_HeightMaskScale;

	private static int s_propID_WaveScale;

	private static int s_propID_Ripple2Position;

	private static int s_propID_Ripple2Params;

	[SerializeField]
	private string _dataPointID;

	[SerializeField]
	private Sector _visualSector;

	[SerializeField]
	private Sector _visualSector2;

	[SerializeField]
	private Shape _connectionBounds;

	[Space]
	[SerializeField]
	private MeshRenderer _poolRenderer;

	[SerializeField]
	private float _poolFillLength = 3f;

	[SerializeField]
	private float _poolEmptyLength = 0.5f;

	[SerializeField]
	private AnimationCurve _poolHeightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _poolMaskCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	[SerializeField]
	private AnimationCurve _poolWaveHeightCurve = AnimationCurve.EaseInOut(0f, 0.01f, 1f, 0.05f);

	[Space]
	[SerializeField]
	private Renderer[] _transitionRenderers;

	[SerializeField]
	private PedestalAnimator _transitionPedestalAnimator;

	[SerializeField]
	private GameObject _transitionStone;

	[Space]
	[SerializeField]
	private GameObject _hologramGroup;

	[SerializeField]
	private Transform _playerHologram;

	[SerializeField]
	private Transform _stoneHologram;

	[Space]
	[SerializeField]
	private float _fadeInLength = 1.5f;

	[SerializeField]
	private float _fadeOutLength = 0.25f;

	[Space]
	[SerializeField]
	private OWAudioSource _ambientAudioSource;

	[SerializeField]
	private OWAudioSource _oneShotAudioSource;

	[Space]
	[SerializeField]
	private DarkZone _darkZone;

	private OWCamera _playerCamera;

	private NomaiRemoteCamera _ownedCamera;

	private SharedStoneSocket _socket;

	private PedestalAnimator _pedestalAnimator;

	private State _platformState;

	private bool _active;

	private float _poolT;

	private bool _showPlayerRipples;

	private Transform _activePlayerHolo;

	private float _transitionFade;

	private NomaiRemoteCameraPlatform _slavePlatform;

	private List<Sector> _alreadyOccupiedSectors;

	public static string IDToPlanetString(ID id)
	{
		switch (id)
		{
		case ID.None:
			return "None";
		case ID.SunStation:
			return UITextLibrary.GetString(UITextType.LocationSS);
		case ID.HGT_TimeLoop:
			return UITextLibrary.GetString(UITextType.LocationTT);
		case ID.HGT_TLE:
			return UITextLibrary.GetString(UITextType.LocationCT);
		case ID.TH_Mine:
			return UITextLibrary.GetString(UITextType.LocationTH);
		case ID.BH_Observatory:
		case ID.BH_GravityCannon:
		case ID.BH_QuantumFragment:
		case ID.BH_BlackHoleForge:
			return UITextLibrary.GetString(UITextType.LocationBH);
		case ID.GD_ConstructionYardIsland1:
		case ID.GD_ConstructionYardIsland2:
		case ID.GD_StatueIsland:
			return UITextLibrary.GetString(UITextType.LocationGD);
		case ID.GD_ProbeCannonSunkenModule:
			return UITextLibrary.GetString(UITextType.LocationOPC_Module3);
		case ID.GD_ProbeCannonDamagedModule:
			return UITextLibrary.GetString(UITextType.LocationOPC_Module2);
		case ID.GD_ProbeCannonIntactModule:
			return UITextLibrary.GetString(UITextType.LocationOPC_Module1);
		case ID.VM_Interior:
			return UITextLibrary.GetString(UITextType.LocationBHMoon);
		default:
			return "";
		}
	}

	private void Awake()
	{
		_ownedCamera = GetComponentInChildren<NomaiRemoteCamera>();
		_alreadyOccupiedSectors = new List<Sector>(16);
		_platformState = State.Disconnected;
		_active = false;
		_poolT = 0f;
		_showPlayerRipples = false;
		_activePlayerHolo = null;
		_transitionFade = 0f;
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_Fade = Shader.PropertyToID("_Fade");
			s_propID_HeightMaskScale = Shader.PropertyToID("_HeightMaskScale");
			s_propID_WaveScale = Shader.PropertyToID("_WaveScale");
			s_propID_Ripple2Position = Shader.PropertyToID("_Ripple2Position");
			s_propID_Ripple2Params = Shader.PropertyToID("_Ripple2Params");
		}
		_socket = GetComponentInChildren<SharedStoneSocket>();
		if (_socket != null)
		{
			_pedestalAnimator = _socket.GetPedestalAnimator();
		}
		else
		{
			Debug.LogWarning("SharedStoneSocket not found!", this);
		}
		UpdatePoolRenderer();
		_hologramGroup.SetActive(value: false);
		UpdateRendererFade();
		_transitionStone.SetActive(value: false);
	}

	private void Start()
	{
		if (s_platforms == null)
		{
			s_platforms = new List<NomaiRemoteCameraPlatform>(32);
		}
		s_platforms.Add(this);
		_playerCamera = Locator.GetPlayerCamera();
		if (_socket != null)
		{
			SharedStoneSocket socket = _socket;
			socket.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(socket.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSocketableRemoved));
			SharedStoneSocket socket2 = _socket;
			socket2.OnSocketableDonePlacing = (OWItemSocket.SocketEvent)Delegate.Combine(socket2.OnSocketableDonePlacing, new OWItemSocket.SocketEvent(OnSocketableDonePlacing));
		}
		if (_pedestalAnimator != null)
		{
			PedestalAnimator pedestalAnimator = _pedestalAnimator;
			pedestalAnimator.OnPedestalContact = (PedestalAnimator.PedestalEvent)Delegate.Combine(pedestalAnimator.OnPedestalContact, new PedestalAnimator.PedestalEvent(OnPedestalContact));
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_socket != null)
		{
			SharedStoneSocket socket = _socket;
			socket.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(socket.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSocketableRemoved));
			SharedStoneSocket socket2 = _socket;
			socket2.OnSocketableDonePlacing = (OWItemSocket.SocketEvent)Delegate.Remove(socket2.OnSocketableDonePlacing, new OWItemSocket.SocketEvent(OnSocketableDonePlacing));
		}
		if (_pedestalAnimator != null)
		{
			PedestalAnimator pedestalAnimator = _pedestalAnimator;
			pedestalAnimator.OnPedestalContact = (PedestalAnimator.PedestalEvent)Delegate.Remove(pedestalAnimator.OnPedestalContact, new PedestalAnimator.PedestalEvent(OnPedestalContact));
		}
		if (s_platforms != null)
		{
			s_platforms.Remove(this);
		}
	}

	private void Update()
	{
		if (_active)
		{
			if (_slavePlatform != null && !_slavePlatform.gameObject.activeInHierarchy)
			{
				Disconnect();
				if (_pedestalAnimator != null)
				{
					_pedestalAnimator.PlayOpen();
				}
				if (_transitionPedestalAnimator != null)
				{
					_transitionPedestalAnimator.PlayOpen();
				}
				_active = false;
			}
			else if (CheckSlavePlatformInsideSupernova())
			{
				OnLeaveBounds();
			}
			else if (!_connectionBounds.PointInside(_playerCamera.transform.position))
			{
				OnLeaveBounds();
			}
		}
		if (_active)
		{
			_poolT = Mathf.MoveTowards(_poolT, 1f, Time.deltaTime / _poolFillLength);
		}
		else
		{
			_poolT = Mathf.MoveTowards(_poolT, 0f, Time.deltaTime / _poolEmptyLength);
		}
		UpdatePoolRenderer();
		switch (_platformState)
		{
		case State.Connecting_FadeIn:
			_transitionFade = Mathf.MoveTowards(_transitionFade, 1f, Time.deltaTime / _fadeInLength);
			UpdateRendererFade();
			if (_transitionFade == 1f)
			{
				_slavePlatform._poolT = _poolT;
				_slavePlatform._showPlayerRipples = true;
				_slavePlatform._activePlayerHolo = _playerHologram.GetChild(0);
				_slavePlatform.UpdatePoolRenderer();
				_transitionFade = 0f;
				UpdateRendererFade();
				_slavePlatform._transitionFade = 1f;
				_slavePlatform.UpdateRendererFade();
				SwitchToRemoteCamera();
				_hologramGroup.SetActive(value: true);
				UpdateHologramTransforms();
				_ambientAudioSource.FadeIn(3f, fadeFromNothing: true);
				Locator.GetAudioMixer().MixRemoteCameraPlatform(_fadeInLength);
				_platformState = State.Connecting_FadeOut;
			}
			break;
		case State.Connecting_FadeOut:
			_slavePlatform._transitionFade = Mathf.MoveTowards(_slavePlatform._transitionFade, 0f, Time.deltaTime / _fadeInLength);
			_slavePlatform.UpdateRendererFade();
			UpdateHologramTransforms();
			_slavePlatform._poolT = _poolT;
			_slavePlatform.UpdatePoolRenderer();
			if (_slavePlatform._transitionFade == 0f)
			{
				_platformState = State.Connected;
			}
			break;
		case State.Connected:
			VerifySectorOccupancy();
			UpdateHologramTransforms();
			_slavePlatform._poolT = _poolT;
			_slavePlatform.UpdatePoolRenderer();
			break;
		case State.Disconnecting_FadeIn:
			_slavePlatform._transitionFade = Mathf.MoveTowards(_slavePlatform._transitionFade, 1f, Time.deltaTime / _fadeOutLength);
			_slavePlatform.UpdateRendererFade();
			UpdateHologramTransforms();
			_slavePlatform._poolT = _poolT;
			_slavePlatform.UpdatePoolRenderer();
			if (_slavePlatform._transitionFade == 1f)
			{
				_slavePlatform._poolT = ((_slavePlatform._sharedStone == null) ? 0f : 1f);
				_slavePlatform._showPlayerRipples = false;
				_slavePlatform._activePlayerHolo = null;
				_slavePlatform.UpdatePoolRenderer();
				_slavePlatform._transitionFade = 0f;
				_slavePlatform.UpdateRendererFade();
				_transitionFade = 1f;
				UpdateRendererFade();
				SwitchToPlayerCamera();
				_hologramGroup.SetActive(value: false);
				_platformState = State.Disconnecting_FadeOut;
			}
			break;
		case State.Disconnecting_FadeOut:
			_transitionFade = Mathf.MoveTowards(_transitionFade, 0f, Time.deltaTime / _fadeOutLength);
			UpdateRendererFade();
			if (_transitionFade == 0f)
			{
				if (_sharedStone == null)
				{
					_slavePlatform = null;
				}
				_platformState = State.Disconnected;
			}
			break;
		}
		if (_platformState == State.Disconnected && !_active && _poolT == 0f)
		{
			base.enabled = false;
		}
	}

	private void Connect()
	{
		switch (_platformState)
		{
		case State.Disconnecting_FadeOut:
		case State.Disconnected:
			_platformState = State.Connecting_FadeIn;
			break;
		case State.Disconnecting_FadeIn:
			_platformState = State.Connecting_FadeOut;
			break;
		}
		_oneShotAudioSource.PlayOneShot(AudioType.NomaiRemoteCameraEntry);
		base.enabled = true;
	}

	private void Disconnect()
	{
		switch (_platformState)
		{
		case State.Connecting_FadeOut:
		case State.Connected:
			_platformState = State.Disconnecting_FadeIn;
			_ambientAudioSource.FadeOut(0.5f);
			_oneShotAudioSource.PlayOneShot(AudioType.NomaiRemoteCameraExit);
			Locator.GetAudioMixer().UnmixRemoteCameraPlatform(_fadeOutLength);
			break;
		case State.Connecting_FadeIn:
			_platformState = State.Disconnecting_FadeOut;
			break;
		}
	}

	private void SwitchToRemoteCamera()
	{
		GlobalMessenger.FireEvent("EnterNomaiRemoteCamera");
		_slavePlatform.RevealFactID();
		_slavePlatform._ownedCamera.Activate(this, _playerCamera);
		_slavePlatform._ownedCamera.SetImageEffectFade(0f);
		_alreadyOccupiedSectors.Clear();
		if (_slavePlatform._visualSector != null)
		{
			if (_visualSector.ContainsOccupant(DynamicOccupant.Player))
			{
				_alreadyOccupiedSectors.Add(_visualSector);
			}
			_slavePlatform._visualSector.AddOccupant(Locator.GetPlayerSectorDetector());
			Sector parentSector = _slavePlatform._visualSector.GetParentSector();
			while (parentSector != null)
			{
				if (parentSector.ContainsOccupant(DynamicOccupant.Player))
				{
					_alreadyOccupiedSectors.Add(parentSector);
				}
				parentSector.AddOccupant(Locator.GetPlayerSectorDetector());
				parentSector = parentSector.GetParentSector();
			}
		}
		if (_slavePlatform._visualSector2 != null)
		{
			_slavePlatform._visualSector2.AddOccupant(Locator.GetPlayerSectorDetector());
		}
		if (_slavePlatform._darkZone != null)
		{
			_slavePlatform._darkZone.AddPlayerToZone(instant: true);
		}
	}

	private void SwitchToPlayerCamera()
	{
		if (_slavePlatform._visualSector != null)
		{
			if (!_alreadyOccupiedSectors.Contains(_slavePlatform._visualSector))
			{
				_slavePlatform._visualSector.RemoveOccupant(Locator.GetPlayerSectorDetector());
			}
			Sector parentSector = _slavePlatform._visualSector.GetParentSector();
			while (parentSector != null)
			{
				if (!_alreadyOccupiedSectors.Contains(parentSector))
				{
					parentSector.RemoveOccupant(Locator.GetPlayerSectorDetector());
				}
				parentSector = parentSector.GetParentSector();
			}
		}
		if (_slavePlatform._visualSector2 != null)
		{
			_slavePlatform._visualSector2.RemoveOccupant(Locator.GetPlayerSectorDetector());
		}
		if (_slavePlatform._darkZone != null)
		{
			_slavePlatform._darkZone.RemovePlayerFromZone(instant: true);
		}
		GlobalMessenger.FireEvent("ExitNomaiRemoteCamera");
		_slavePlatform._ownedCamera.Deactivate();
		_slavePlatform._ownedCamera.SetImageEffectFade(0f);
	}

	private void VerifySectorOccupancy()
	{
		if (_slavePlatform._visualSector != null && !_slavePlatform._visualSector.ContainsOccupant(DynamicOccupant.Player))
		{
			Debug.LogWarning("Player was somehow removed from the NomaiRemoteCameraPlatform's visual sectors!  Re-adding...");
			_slavePlatform._visualSector.AddOccupant(Locator.GetPlayerSectorDetector());
			Sector parentSector = _slavePlatform._visualSector.GetParentSector();
			while (parentSector != null)
			{
				if (!parentSector.ContainsOccupant(DynamicOccupant.Player))
				{
					parentSector.AddOccupant(Locator.GetPlayerSectorDetector());
				}
				parentSector = parentSector.GetParentSector();
			}
		}
		if (_slavePlatform._visualSector2 != null && !_slavePlatform._visualSector2.ContainsOccupant(DynamicOccupant.Player))
		{
			_slavePlatform._visualSector2.AddOccupant(Locator.GetPlayerSectorDetector());
		}
	}

	private void UpdatePoolRenderer()
	{
		_poolRenderer.transform.localPosition = new Vector3(0f, _poolHeightCurve.Evaluate(_poolT), 0f);
		_poolRenderer.material.SetFloat(s_propID_HeightMaskScale, _poolMaskCurve.Evaluate(_poolT));
		Vector4 vector = _poolRenderer.sharedMaterial.GetVector(s_propID_WaveScale);
		vector.y = _poolWaveHeightCurve.Evaluate(_poolT);
		_poolRenderer.material.SetVector(s_propID_WaveScale, vector);
		Vector4 vector2 = _poolRenderer.sharedMaterial.GetVector(s_propID_Ripple2Position);
		Vector4 vector3 = _poolRenderer.sharedMaterial.GetVector(s_propID_Ripple2Params);
		if (_showPlayerRipples)
		{
			Vector3 vector4 = _poolRenderer.transform.InverseTransformPoint(_activePlayerHolo.position);
			vector2.x = vector4.x;
			vector2.y = vector4.z;
			vector3.x = 0.5f;
		}
		_poolRenderer.material.SetVector(s_propID_Ripple2Position, vector2);
		_poolRenderer.material.SetVector(s_propID_Ripple2Params, vector3);
	}

	private void UpdateRendererFade()
	{
		s_matPropBlock.SetFloat(s_propID_Fade, 1f - _transitionFade);
		for (int i = 0; i < _transitionRenderers.Length; i++)
		{
			_transitionRenderers[i].SetPropertyBlock(s_matPropBlock);
			if (_transitionRenderers[i].enabled && _transitionFade == 0f)
			{
				_transitionRenderers[i].enabled = false;
			}
			else if (!_transitionRenderers[i].enabled && _transitionFade > 0f)
			{
				_transitionRenderers[i].enabled = true;
			}
		}
		_ownedCamera.SetImageEffectFade(1f - _transitionFade);
	}

	private void UpdateHologramTransforms()
	{
		_hologramGroup.transform.position = _slavePlatform.transform.position;
		_hologramGroup.transform.rotation = _slavePlatform.transform.rotation;
		Transform playerTransform = Locator.GetPlayerTransform();
		_playerHologram.position = TransformPoint(playerTransform.position, this, _slavePlatform);
		_playerHologram.rotation = TransformRotation(playerTransform.rotation, this, _slavePlatform);
		if (_sharedStone != null)
		{
			Transform transform = _sharedStone.transform;
			_stoneHologram.position = TransformPoint(transform.position, this, _slavePlatform);
			_stoneHologram.rotation = TransformRotation(transform.rotation, this, _slavePlatform);
		}
		else
		{
			_stoneHologram.SetPositionAndRotation(new Vector3(0f, -2f, 0f), Quaternion.identity);
		}
	}

	private void OnSocketableRemoved(OWItem socketable)
	{
		if (!(_slavePlatform == null))
		{
			Disconnect();
			_transitionStone.SetActive(value: false);
			_slavePlatform._transitionStone.SetActive(value: false);
			_socket.GetPedestalAnimator().PlayOpen();
			if (_transitionPedestalAnimator != null)
			{
				_transitionPedestalAnimator.PlayOpen();
			}
			if (_slavePlatform._pedestalAnimator != null)
			{
				_slavePlatform._pedestalAnimator.PlayOpen();
			}
			if (_slavePlatform._transitionPedestalAnimator != null)
			{
				_slavePlatform._transitionPedestalAnimator.PlayOpen();
			}
			_sharedStone = null;
			_active = false;
			if (_platformState == State.Disconnected)
			{
				_slavePlatform = null;
			}
		}
	}

	private void OnSocketableDonePlacing(OWItem socketable)
	{
		_sharedStone = socketable as SharedStone;
		if (_sharedStone == null)
		{
			Debug.LogError("Placed an empty item or a non SharedStone in a NomaiRemoteCameraPlatform");
		}
		_slavePlatform = GetPlatform(_sharedStone.GetRemoteCameraID());
		if (_slavePlatform == null)
		{
			Debug.LogError(string.Concat("Shared stone with Remote Camera ID: ", _sharedStone.GetRemoteCameraID(), " has no registered camera platform!"));
		}
		if (_slavePlatform == this || !_slavePlatform.gameObject.activeInHierarchy)
		{
			_sharedStone = null;
			_slavePlatform = null;
			return;
		}
		_transitionStone.SetActive(value: true);
		_slavePlatform._transitionStone.SetActive(value: true);
		_socket.GetPedestalAnimator().PlayClose();
		if (_transitionPedestalAnimator != null)
		{
			_transitionPedestalAnimator.PlayClose();
		}
		if (_slavePlatform._pedestalAnimator != null)
		{
			_slavePlatform._pedestalAnimator.PlayClose();
		}
		if (_slavePlatform._transitionPedestalAnimator != null)
		{
			_slavePlatform._transitionPedestalAnimator.PlayClose();
		}
		_active = true;
		base.enabled = true;
		if (_pedestalAnimator == null || _pedestalAnimator.HasMadeContact())
		{
			OnPedestalContact();
		}
	}

	private void OnPedestalContact()
	{
		if (_slavePlatform != null && _connectionBounds.PointInside(_playerCamera.transform.position) && !CheckSlavePlatformInsideSupernova())
		{
			Connect();
		}
	}

	private void OnLeaveBounds()
	{
		Disconnect();
		if (_pedestalAnimator != null)
		{
			_pedestalAnimator.PlayOpen();
		}
		if (_transitionPedestalAnimator != null)
		{
			_transitionPedestalAnimator.PlayOpen();
		}
		if (_slavePlatform != null)
		{
			if (_slavePlatform._pedestalAnimator != null)
			{
				_slavePlatform._pedestalAnimator.PlayOpen();
			}
			if (_slavePlatform._transitionPedestalAnimator != null)
			{
				_slavePlatform._transitionPedestalAnimator.PlayOpen();
			}
		}
		_active = false;
	}

	protected void RevealFactID()
	{
		if (_dataPointID.Length > 0)
		{
			Locator.GetShipLogManager().RevealFact(_dataPointID);
		}
	}

	public NomaiRemoteCamera GetOwnedCamera()
	{
		return _ownedCamera;
	}

	public static Vector3 TransformPoint(Vector3 worldPos, NomaiRemoteCameraPlatform from, NomaiRemoteCameraPlatform to)
	{
		Vector3 position = from.transform.InverseTransformPoint(worldPos);
		return to.transform.TransformPoint(position);
	}

	public static Quaternion TransformRotation(Quaternion worldRot, NomaiRemoteCameraPlatform from, NomaiRemoteCameraPlatform to)
	{
		Quaternion quaternion = from.transform.InverseTransformRotation(worldRot);
		return to.transform.rotation * quaternion;
	}

	public static NomaiRemoteCameraPlatform GetPlatform(ID platformID)
	{
		if (s_platforms != null)
		{
			for (int i = 0; i < s_platforms.Count; i++)
			{
				if (s_platforms[i]._id == platformID)
				{
					return s_platforms[i];
				}
			}
		}
		return null;
	}

	public State GetPlatformState()
	{
		return _platformState;
	}

	public SharedStone GetSocketedStone()
	{
		return _sharedStone;
	}

	public bool IsPlatformActive()
	{
		return _active;
	}

	public bool CheckSlavePlatformInsideSupernova()
	{
		if (_slavePlatform != null && _slavePlatform._id != ID.HGT_TimeLoop && Locator.GetSunController() != null)
		{
			return Locator.GetSunController().IsPointInsideSupernova(_slavePlatform.transform.position);
		}
		return false;
	}
}
