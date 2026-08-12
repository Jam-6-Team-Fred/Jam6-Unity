using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Sectors/Sector Proxy", 400)]
public class SectorProxy : MonoBehaviour, ISectorGroup
{
	[Serializable]
	private struct LightData
	{
		public Light light;

		public float originalIntensity;

		public LightData(Light l, float i)
		{
			light = l;
			originalIntensity = i;
		}
	}

	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_unityLODFade;

	[SerializeField]
	[HideInInspector]
	private bool _prebuilt;

	[SerializeField]
	[HideInInspector]
	private List<Renderer> _renderers;

	[SerializeField]
	[HideInInspector]
	private List<LightData> _lights;

	private List<CullGroup> _controlledCullGroups;

	[SerializeField]
	protected Sector _sector;

	[SerializeField]
	private bool _crossfade = true;

	[SerializeField]
	private float _crossfadeLength = 1f;

	[SerializeField]
	protected Sector _exclusiveSector;

	private bool _firstUpdate = true;

	protected bool _proxyActive;

	protected bool _proxyHidden;

	protected bool _waitingForCullGroups;

	private bool _inMapView;

	private bool _isFastForwarding;

	private bool _fadeComplete = true;

	private float _fadeTimer;

	protected virtual void Awake()
	{
		if (!_prebuilt)
		{
			FindComponentsInHierarchy();
		}
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_unityLODFade = Shader.PropertyToID("unity_LODFade");
		}
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else if (!(this is CloakingFieldProxy))
		{
			Debug.LogWarning("SectorProxy has no specified Sector!", this);
		}
		if ((bool)_exclusiveSector)
		{
			_exclusiveSector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnExclusiveSectorOccupantsUpdated);
		}
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
		base.enabled = false;
	}

	protected virtual void OnDestroy()
	{
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		if ((bool)_exclusiveSector)
		{
			_exclusiveSector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnExclusiveSectorOccupantsUpdated);
		}
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
	}

	private void LateUpdate()
	{
		if (_waitingForCullGroups)
		{
			if (CanSwitch())
			{
				SetProxyActive(proxyActive: false);
				_waitingForCullGroups = false;
			}
			return;
		}
		if (_crossfade && !_fadeComplete)
		{
			_fadeTimer = Mathf.MoveTowards(_fadeTimer, _proxyActive ? _crossfadeLength : 0f, Time.deltaTime);
			float num = _fadeTimer / _crossfadeLength;
			float num2 = Mathf.Floor(num * 16f) / 16f;
			if ((_proxyActive && num == 1f) || (!_proxyActive && num == 0f))
			{
				_fadeComplete = true;
			}
			s_matPropBlock.SetVector(s_propID_unityLODFade, new Vector4(num - 1f, num2 - 1f, 0f, 0f));
			for (int i = 0; i < _renderers.Count; i++)
			{
				if (_renderers[i] != null)
				{
					_renderers[i].SetPropertyBlock(s_matPropBlock);
					if (_fadeComplete)
					{
						_renderers[i].enabled = _proxyActive && !_proxyHidden;
					}
					else if (!_proxyHidden && !_renderers[i].enabled && num > 0f)
					{
						_renderers[i].enabled = true;
					}
				}
			}
			for (int j = 0; j < _lights.Count; j++)
			{
				if (_lights[j].light != null)
				{
					_lights[j].light.intensity = _lights[j].originalIntensity * num;
					if (_fadeComplete)
					{
						_lights[j].light.enabled = _proxyActive && !_proxyHidden;
					}
					else if (!_proxyHidden && !_lights[j].light.enabled && num > 0f)
					{
						_lights[j].light.enabled = true;
					}
				}
			}
		}
		if (!_crossfade || _fadeComplete)
		{
			base.enabled = false;
		}
	}

	private void FindComponentsInHierarchy()
	{
		_renderers = new List<Renderer>();
		_lights = new List<LightData>();
		RecursivelyAddComponents(base.transform);
	}

	private void RecursivelyAddComponents(Transform parent)
	{
		if ((parent != base.transform && parent.GetComponent<SectorProxy>() != null) || parent.GetComponent<SectorLightsLODGroup>() != null || parent.GetComponent<SectorRendererLODGroup>() != null)
		{
			return;
		}
		Renderer component = parent.GetComponent<Renderer>();
		Light component2 = parent.GetComponent<Light>();
		if ((bool)component)
		{
			_renderers.Add(component);
		}
		if ((bool)component2)
		{
			_lights.Add(new LightData(component2, component2.intensity));
		}
		foreach (Transform item in parent)
		{
			RecursivelyAddComponents(item);
		}
	}

	private void OnSectorOccupantsUpdated()
	{
		if (_firstUpdate)
		{
			SetProxyActive(ShouldBeActive(), instant: true);
			_fadeComplete = true;
			_fadeTimer = (_proxyActive ? _crossfadeLength : 0f);
			_firstUpdate = false;
		}
		else
		{
			if (_inMapView || _isFastForwarding)
			{
				return;
			}
			if (!ShouldBeActive())
			{
				if (_proxyActive)
				{
					if (CanSwitch())
					{
						SetProxyActive(proxyActive: false);
						return;
					}
					_waitingForCullGroups = true;
					base.enabled = true;
				}
			}
			else
			{
				if (!_proxyActive)
				{
					SetProxyActive(proxyActive: true);
				}
				_waitingForCullGroups = false;
			}
		}
	}

	private void OnExclusiveSectorOccupantsUpdated()
	{
		if (!ShouldBeHidden())
		{
			if (_proxyHidden)
			{
				SetProxyHidden(hidden: false);
			}
		}
		else if (!_proxyHidden)
		{
			SetProxyHidden(hidden: true);
		}
	}

	private void OnEnterMapView()
	{
		_inMapView = true;
		if (!_proxyActive)
		{
			SetProxyActive(proxyActive: true, instant: true);
		}
	}

	private void OnExitMapView()
	{
		_inMapView = false;
		if (!ShouldBeActive())
		{
			SetProxyActive(proxyActive: false, CanSwitch());
		}
	}

	protected virtual void OnEnterDreamWorld()
	{
		ForceInstantUpdate();
	}

	protected virtual void OnExitDreamWorld()
	{
		ForceInstantUpdate();
	}

	protected void ForceInstantUpdate()
	{
		bool flag = ShouldBeActive();
		if (flag || CanSwitch())
		{
			SetProxyActive(flag, instant: true);
			_fadeComplete = true;
			_fadeTimer = (_proxyActive ? _crossfadeLength : 0f);
		}
	}

	private void OnStartFastForward()
	{
		_isFastForwarding = true;
		if (!_proxyHidden)
		{
			SetProxyHidden(hidden: true);
		}
		if (!_proxyActive)
		{
			SetProxyActive(proxyActive: true, instant: true);
		}
	}

	private void OnEndFastForward()
	{
		_isFastForwarding = false;
		if (!ShouldBeHidden())
		{
			SetProxyHidden(hidden: false);
		}
		if (!ShouldBeActive())
		{
			SetProxyActive(proxyActive: false, CanSwitch());
		}
	}

	protected virtual bool ShouldBeActive()
	{
		if ((bool)_sector && _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			return false;
		}
		return true;
	}

	protected virtual bool ShouldBeHidden()
	{
		if ((bool)_exclusiveSector && !_exclusiveSector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			return true;
		}
		return false;
	}

	protected bool CanSwitch()
	{
		if (_controlledCullGroups == null)
		{
			return true;
		}
		for (int i = 0; i < _controlledCullGroups.Count; i++)
		{
			if (_controlledCullGroups[i].IsWaitingForStreaming())
			{
				return false;
			}
		}
		return true;
	}

	public void SetProxyActive(bool proxyActive, bool instant = false)
	{
		_proxyActive = proxyActive;
		if (_crossfade && !instant)
		{
			_fadeComplete = false;
			base.enabled = true;
		}
		else
		{
			s_matPropBlock.SetVector(s_propID_unityLODFade, _proxyActive ? new Vector4(0f, 0f, 0f, 0f) : new Vector4(-1f, -1f, 0f, 0f));
			for (int i = 0; i < _renderers.Count; i++)
			{
				if (_renderers[i] != null)
				{
					_renderers[i].enabled = _proxyActive && !_proxyHidden;
					_renderers[i].SetPropertyBlock(s_matPropBlock);
				}
			}
			for (int j = 0; j < _lights.Count; j++)
			{
				if (_lights[j].light != null)
				{
					_lights[j].light.enabled = _proxyActive && !_proxyHidden;
				}
			}
			_fadeComplete = true;
			_fadeTimer = (_proxyActive ? _crossfadeLength : 0f);
		}
		if (_controlledCullGroups != null)
		{
			for (int k = 0; k < _controlledCullGroups.Count; k++)
			{
				_controlledCullGroups[k].SetVisible(!_proxyActive, instant);
			}
		}
	}

	public void SetProxyHidden(bool hidden)
	{
		_proxyHidden = hidden;
		for (int i = 0; i < _renderers.Count; i++)
		{
			if (_renderers[i] != null)
			{
				_renderers[i].enabled = _proxyActive && !_proxyHidden;
			}
		}
		for (int j = 0; j < _lights.Count; j++)
		{
			if (_lights[j].light != null)
			{
				_lights[j].light.enabled = _proxyActive && !_proxyHidden;
			}
		}
	}

	public bool IsProxyActive()
	{
		return _proxyActive;
	}

	public Sector GetSector()
	{
		return _sector;
	}

	public void SetSector(Sector sector)
	{
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		_sector = sector;
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		OnSectorOccupantsUpdated();
	}

	public void AddControlledCullGroup(CullGroup cullGroup)
	{
		if (_controlledCullGroups == null)
		{
			_controlledCullGroups = new List<CullGroup>(16);
		}
		_controlledCullGroups.Add(cullGroup);
	}

	public void RemoveControlledCullGroup(CullGroup cullGroup)
	{
		if (_controlledCullGroups != null)
		{
			_controlledCullGroups.QuickRemove(cullGroup);
		}
	}
}
