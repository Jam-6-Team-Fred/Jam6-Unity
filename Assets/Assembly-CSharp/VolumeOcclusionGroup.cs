using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class VolumeOcclusionGroup : MonoBehaviour
{
	[Serializable]
	public struct OcclusionVolumeData
	{
		public VolumeOcclusionRenderer occlusionVolume;

		public float originalStrength;

		public OcclusionVolumeData(VolumeOcclusionRenderer vor, float s)
		{
			occlusionVolume = vor;
			originalStrength = s;
		}
	}

	[SerializeField]
	[HideInInspector]
	private bool _prebuilt;

	[SerializeField]
	[HideInInspector]
	private List<OcclusionVolumeData> _occlusionVolumes;

	[SerializeField]
	[HideInInspector]
	private List<VolumeOcclusionLight> _staticOcclusionLights;

	[SerializeField]
	[HideInInspector]
	private List<OWVolumeOcclusionLight> _dynamicOcclusionLights;

	private bool _visible = true;

	private bool _occlusionEnabled = true;

	private bool _fadeComplete = true;

	private float _fadeTimerLength = 1f;

	private float _fadeTimer = 1f;

	protected virtual void Awake()
	{
		if (!_prebuilt)
		{
			BuildGroup();
		}
		base.enabled = false;
	}

	protected void BuildGroup()
	{
		_occlusionVolumes = new List<OcclusionVolumeData>();
		_staticOcclusionLights = new List<VolumeOcclusionLight>();
		_dynamicOcclusionLights = new List<OWVolumeOcclusionLight>();
		RecursivelyAddOcclusion(base.transform);
	}

	protected void RecursivelyAddOcclusion(Transform parent)
	{
		VolumeOcclusionRenderer component = parent.GetComponent<VolumeOcclusionRenderer>();
		if (component != null)
		{
			_occlusionVolumes.Add(new OcclusionVolumeData(component, component.occlusionStrength));
		}
		VolumeOcclusionLight component2 = parent.GetComponent<VolumeOcclusionLight>();
		if (component2 != null)
		{
			OWVolumeOcclusionLight component3 = parent.GetComponent<OWVolumeOcclusionLight>();
			if (component3 != null)
			{
				_dynamicOcclusionLights.Add(component3);
			}
			else
			{
				_staticOcclusionLights.Add(component2);
			}
		}
		foreach (Transform item in parent)
		{
			RecursivelyAddOcclusion(item);
		}
	}

	protected virtual void LateUpdate()
	{
		if (!_fadeComplete)
		{
			if (_visible)
			{
				_fadeTimer += Time.deltaTime;
			}
			else
			{
				_fadeTimer -= Time.deltaTime;
			}
			_fadeTimer = Mathf.Clamp(_fadeTimer, 0f, _fadeTimerLength);
			float num = _fadeTimer / _fadeTimerLength;
			SetOcclusionFade(num);
			if ((_visible && OWMath.ApproxEquals(num, 1f)) || (!_visible && OWMath.ApproxEquals(num, 0f)))
			{
				if (!_visible && _occlusionEnabled)
				{
					SetOcclusionEnabled(newEnabled: false);
				}
				_fadeComplete = true;
				base.enabled = false;
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	protected void SetOcclusionEnabled(bool newEnabled)
	{
		if (_occlusionEnabled == newEnabled)
		{
			return;
		}
		for (int i = 0; i < _occlusionVolumes.Count; i++)
		{
			if (_occlusionVolumes[i].occlusionVolume != null)
			{
				_occlusionVolumes[i].occlusionVolume.enabled = newEnabled;
			}
		}
		for (int j = 0; j < _staticOcclusionLights.Count; j++)
		{
			if (_staticOcclusionLights[j] != null)
			{
				_staticOcclusionLights[j].enabled = newEnabled;
			}
		}
		for (int k = 0; k < _dynamicOcclusionLights.Count; k++)
		{
			if (_dynamicOcclusionLights[k] != null)
			{
				_dynamicOcclusionLights[k].SetLODActivation(newEnabled);
			}
		}
		_occlusionEnabled = newEnabled;
	}

	protected void SetOcclusionFade(float fadeFactor)
	{
		for (int i = 0; i < _occlusionVolumes.Count; i++)
		{
			if (_occlusionVolumes[i].occlusionVolume != null)
			{
				_occlusionVolumes[i].occlusionVolume.occlusionStrength = _occlusionVolumes[i].originalStrength * fadeFactor;
			}
		}
	}

	public void SetVisible(bool newVisible, float newFadeLength = 0f)
	{
		if (newFadeLength <= 0f)
		{
			_fadeComplete = true;
			_fadeTimerLength = 0f;
			SetOcclusionFade(newVisible ? 1f : 0f);
			SetOcclusionEnabled(newVisible);
		}
		else
		{
			_fadeComplete = false;
			if (_fadeTimerLength == 0f)
			{
				_fadeTimer = (_visible ? newFadeLength : 0f);
			}
			else
			{
				_fadeTimer = _fadeTimer / _fadeTimerLength * newFadeLength;
			}
			_fadeTimerLength = newFadeLength;
			if (newVisible && !_occlusionEnabled)
			{
				SetOcclusionEnabled(newEnabled: true);
			}
			base.enabled = true;
		}
		_visible = newVisible;
	}

	public bool IsVisible()
	{
		return _visible;
	}

	public bool IsCrossfading()
	{
		return !_fadeComplete;
	}
}
