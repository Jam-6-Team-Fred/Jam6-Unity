using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class LightsCullGroup : MonoBehaviour
{
	[Serializable]
	public struct LightData
	{
		public Light light;

		public float originalIntensity;

		public LightData(Light l, float i)
		{
			light = l;
			originalIntensity = i;
		}
	}

	[SerializeField]
	[HideInInspector]
	private bool _prebuilt;

	[SerializeField]
	[HideInInspector]
	private List<LightData> _staticLights;

	[SerializeField]
	[HideInInspector]
	private List<OWLight2> _dynamicLights;

	[SerializeField]
	[HideInInspector]
	private List<LightLOD> _lightLODs;

	private bool _shining = true;

	private bool _lightsEnabled = true;

	private bool _fadeComplete = true;

	private float _fadeTimerLength = 1f;

	private float _fadeTimer = 1f;

	protected virtual void Awake()
	{
		if (!_prebuilt)
		{
			BuildLightsCullGroup();
		}
		base.enabled = false;
	}

	protected void BuildLightsCullGroup()
	{
		_staticLights = new List<LightData>();
		_dynamicLights = new List<OWLight2>();
		_lightLODs = new List<LightLOD>();
		RecursivelyAddLights(base.transform);
		if (_lightLODs.Count == 0)
		{
			_lightLODs = null;
		}
	}

	protected void RecursivelyAddLights(Transform parent, bool addOWLight = false)
	{
		if (!ShouldIncludeObject(parent))
		{
			return;
		}
		if (!addOWLight)
		{
			IGroupController component = parent.GetComponent<IGroupController>();
			if (component != null)
			{
				addOWLight = (component.groupControlMask & 4) > 0;
			}
		}
		Light component2 = parent.GetComponent<Light>();
		if (component2 != null)
		{
			OWLight2 component3 = parent.GetComponent<OWLight2>();
			if (component3 != null)
			{
				_dynamicLights.Add(component3);
			}
			else if (addOWLight)
			{
				_dynamicLights.Add(component2.gameObject.AddComponent<OWLight2>());
			}
			else
			{
				_staticLights.Add(new LightData(component2, component2.intensity));
			}
			LightLOD component4 = component2.GetComponent<LightLOD>();
			if (component4 != null)
			{
				_lightLODs.Add(component4);
			}
		}
		foreach (Transform item in parent)
		{
			RecursivelyAddLights(item);
		}
	}

	protected virtual bool ShouldIncludeObject(Transform transform)
	{
		if (transform == null)
		{
			return false;
		}
		LightsCullGroupExcluder component = transform.GetComponent<LightsCullGroupExcluder>();
		if (component != null && component.gameObject != base.gameObject)
		{
			return false;
		}
		LightsCullGroup component2 = transform.GetComponent<LightsCullGroup>();
		if (component2 != null && component2.gameObject != base.gameObject)
		{
			return false;
		}
		if (transform.GetComponent<OWItem>() != null)
		{
			return false;
		}
		return true;
	}

	protected virtual void LateUpdate()
	{
		if (!_fadeComplete)
		{
			_fadeTimer = Mathf.Clamp(_shining ? (_fadeTimer + Time.deltaTime) : (_fadeTimer - Time.deltaTime), 0f, _fadeTimerLength);
			float num = _fadeTimer / _fadeTimerLength;
			SetLightsFade(num);
			if ((_shining && OWMath.ApproxEquals(num, 1f)) || (!_shining && OWMath.ApproxEquals(num, 0f)))
			{
				if (!_shining && _lightsEnabled)
				{
					SetLightsEnabled(newEnabled: false);
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

	protected void SetLightsEnabled(bool newEnabled)
	{
		if (_lightsEnabled == newEnabled)
		{
			return;
		}
		for (int i = 0; i < _staticLights.Count; i++)
		{
			if (_staticLights[i].light != null)
			{
				_staticLights[i].light.enabled = newEnabled;
			}
		}
		for (int j = 0; j < _dynamicLights.Count; j++)
		{
			if (_dynamicLights[j] != null)
			{
				_dynamicLights[j].SetLODActivation(newEnabled);
			}
		}
		if (_lightLODs != null)
		{
			for (int k = 0; k < _lightLODs.Count; k++)
			{
				if (_lightLODs[k] != null)
				{
					_lightLODs[k].enabled = newEnabled;
				}
			}
		}
		_lightsEnabled = newEnabled;
	}

	protected void SetLightsFade(float fadeFactor)
	{
		for (int i = 0; i < _staticLights.Count; i++)
		{
			if (_staticLights[i].light != null)
			{
				_staticLights[i].light.intensity = _staticLights[i].originalIntensity * fadeFactor;
			}
		}
		for (int j = 0; j < _dynamicLights.Count; j++)
		{
			if (_dynamicLights[j] != null)
			{
				_dynamicLights[j].SetLODFade(1f - fadeFactor);
			}
		}
	}

	public void SetShining(bool shining, float fadeLength = 0f)
	{
		if (fadeLength <= 0f)
		{
			_fadeComplete = true;
			_fadeTimerLength = 0f;
			SetLightsFade(shining ? 1f : 0f);
			SetLightsEnabled(shining);
		}
		else
		{
			_fadeComplete = false;
			if (_fadeTimerLength == 0f)
			{
				_fadeTimer = (_shining ? fadeLength : 0f);
			}
			else
			{
				_fadeTimer = _fadeTimer / _fadeTimerLength * fadeLength;
			}
			_fadeTimerLength = fadeLength;
			if (shining && !_lightsEnabled)
			{
				SetLightsEnabled(newEnabled: true);
			}
			base.enabled = true;
		}
		_shining = shining;
	}

	public bool IsShining()
	{
		return _shining;
	}

	public bool IsCrossfading()
	{
		return !_fadeComplete;
	}
}
