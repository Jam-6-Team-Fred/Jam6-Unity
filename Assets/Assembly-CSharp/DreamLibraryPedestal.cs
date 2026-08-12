using System;
using UnityEngine;

public class DreamLibraryPedestal : MonoBehaviour
{
	[SerializeField]
	private float _simulationRadiusBuffer;

	[SerializeField]
	private bool _debugDrawFilledSphere;

	[Space]
	[SerializeField]
	private DreamLanternSocket _socket;

	[SerializeField]
	private OWRendererFadeController _lightBeamController;

	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private Transform _pedestal;

	[SerializeField]
	private Transform[] _flaps;

	[SerializeField]
	private float _maxFlapDegrees = 90f;

	[Space]
	[SerializeField]
	private DreamSlideProjector _projector;

	[SerializeField]
	private DreamObjectProjection[] _projections;

	[SerializeField]
	private OWAudioSource _projectionAudio;

	[SerializeField]
	private AbstractDoor[] _doorsToOpen;

	[SerializeField]
	private DreamLibraryFlame[] _flames;

	private const float _animationLength = 0.7f;

	private bool _powered;

	private bool _lanternPlaced;

	private float _animStartTime;

	private float _minFlapDegrees = 30f;

	private float _flapStartDegrees;

	private float _pedestalMaxHeight;

	private float _pedestalStartHeight;

	public OWEvent OnPowerOn = new OWEvent(1);

	public OWEvent OnPowerOff = new OWEvent(1);

	private void Awake()
	{
		DreamLanternSocket socket = _socket;
		socket.OnSocketableDonePlacing = (OWItemSocket.SocketEvent)Delegate.Combine(socket.OnSocketableDonePlacing, new OWItemSocket.SocketEvent(OnSocketableDonePlacing));
		DreamLanternSocket socket2 = _socket;
		socket2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(socket2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSocketableRemoved));
	}

	private void Start()
	{
		_minFlapDegrees = _flaps[0].localEulerAngles.z;
		_pedestalMaxHeight = _pedestal.localPosition.y;
		_lightBeamController.SetFade(0f);
		_lightController.SetIntensity(0f);
		for (int i = 0; i < _projections.Length; i++)
		{
			_projections[i].SetVisibleImmediate(visible: false, forceUpdate: true);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		DreamLanternSocket socket = _socket;
		socket.OnSocketableDonePlacing = (OWItemSocket.SocketEvent)Delegate.Remove(socket.OnSocketableDonePlacing, new OWItemSocket.SocketEvent(OnSocketableDonePlacing));
		DreamLanternSocket socket2 = _socket;
		socket2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(socket2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSocketableRemoved));
	}

	public float GetSimulationRadiusBuffer()
	{
		return _simulationRadiusBuffer;
	}

	public bool IsPowered()
	{
		return _powered;
	}

	public DreamLanternSocket GetSocket()
	{
		return _socket;
	}

	private void Update()
	{
		if (!_powered && _lanternPlaced && Time.time > _animStartTime + 0.7f - 0.1f)
		{
			_powered = true;
			if (_projector != null)
			{
				_projector.SetLit(lit: true);
			}
			for (int i = 0; i < _doorsToOpen.Length; i++)
			{
				_doorsToOpen[i].Open();
			}
			for (int j = 0; j < _projections.Length; j++)
			{
				_projections[j].SetVisible(visible: true);
			}
			for (int k = 0; k < _flames.Length; k++)
			{
				_flames[k].SetLit(lit: true);
			}
			if (_projectionAudio != null)
			{
				_projectionAudio.PlayOneShot(AudioType.ProjectorTotem_Light);
			}
			OnPowerOn.Invoke();
		}
		float t = Mathf.InverseLerp(_animStartTime, _animStartTime + 0.7f, Time.time);
		t = Mathf.SmoothStep(0f, 1f, t);
		float b = (_lanternPlaced ? (_pedestalMaxHeight - 0.2f) : _pedestalMaxHeight);
		_pedestal.localPosition = Vector3.up * Mathf.Lerp(_pedestalStartHeight, b, t);
		float b2 = (_lanternPlaced ? _maxFlapDegrees : _minFlapDegrees);
		for (int l = 0; l < _flaps.Length; l++)
		{
			Vector3 localEulerAngles = _flaps[l].localEulerAngles;
			localEulerAngles.z = Mathf.Lerp(_flapStartDegrees, b2, t);
			_flaps[l].localEulerAngles = localEulerAngles;
		}
		if ((_powered || !_lanternPlaced) && t >= 1f)
		{
			base.enabled = false;
		}
	}

	private void OnSocketableDonePlacing(OWItem item)
	{
		Activate(item);
	}

	private void OnSocketableRemoved(OWItem item)
	{
		Deactivate(item);
	}

	public void Activate(OWItem item = null)
	{
		base.enabled = true;
		_lanternPlaced = true;
		_animStartTime = Time.time;
		_flapStartDegrees = _flaps[0].localEulerAngles.z;
		_pedestalStartHeight = _pedestal.localPosition.y;
		_lightBeamController.FadeTo(1f, 0.7f);
		_lightController.FadeTo(1f, 0.7f);
		if (item != null)
		{
			Locator.GetDreamWorldController().UpdateSimulationSphereRadius(_simulationRadiusBuffer);
		}
	}

	public void Deactivate(OWItem item = null)
	{
		base.enabled = true;
		_powered = false;
		_lanternPlaced = false;
		_animStartTime = Time.time;
		_flapStartDegrees = _flaps[0].localEulerAngles.z;
		_pedestalStartHeight = _pedestal.localPosition.y;
		_lightBeamController.FadeTo(0f, 0.7f);
		_lightController.FadeTo(0f, 0.7f);
		if (_projector != null)
		{
			_projector.SetLit(lit: false);
		}
		for (int i = 0; i < _doorsToOpen.Length; i++)
		{
			_doorsToOpen[i].Close();
		}
		for (int j = 0; j < _projections.Length; j++)
		{
			_projections[j].SetVisible(visible: false);
		}
		for (int k = 0; k < _flames.Length; k++)
		{
			_flames[k].SetLit(lit: false);
		}
		if (_projectionAudio != null)
		{
			_projectionAudio.PlayOneShot(AudioType.Artifact_Extinguish, 0.2f);
		}
		if (item != null)
		{
			Locator.GetDreamWorldController().UpdateSimulationSphereRadius(0f);
		}
		OnPowerOff.Invoke();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		if (_debugDrawFilledSphere)
		{
			Gizmos.DrawSphere(_socket.transform.position, 20f + _simulationRadiusBuffer);
		}
		else
		{
			Gizmos.DrawWireSphere(_socket.transform.position, 20f + _simulationRadiusBuffer);
		}
	}
}
