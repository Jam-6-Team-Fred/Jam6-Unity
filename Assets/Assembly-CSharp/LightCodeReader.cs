using System.Collections.Generic;
using UnityEngine;

public class LightCodeReader : MonoBehaviour
{
	[SerializeField]
	private LightCodeName _codeName;

	[SerializeField]
	private bool _reverses;

	[SerializeField]
	private float _pauseDuration;

	[Space]
	[SerializeField]
	private GameObject _lightsRoot;

	[SerializeField]
	private List<NomaiLamp> _lights;

	private float _lastChangeTime;

	private LightCode _lightCode;

	private int _index;

	private bool _reversing;

	private bool _isPaused;

	private void OnValidate()
	{
		if (_lightsRoot != null)
		{
			ChangeLightsRoot(_lightsRoot);
			_lightsRoot = null;
		}
	}

	public void ChangeLightsRoot(GameObject lightsRoot)
	{
		_lights.Clear();
		NomaiLamp[] componentsInChildren = lightsRoot.GetComponentsInChildren<NomaiLamp>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			_lights.Add(componentsInChildren[i]);
		}
	}

	private void Start()
	{
		_lightCode = LightCode.GetLightCode(_codeName);
		_index = 0;
		_reversing = false;
		_lastChangeTime = Time.time;
	}

	public void ChangeLightCode(LightCodeName codeName, bool reverses)
	{
		_lightCode = LightCode.GetLightCode(codeName);
		_reverses = reverses;
		_index = 0;
		_reversing = false;
		_lastChangeTime = Time.time;
	}

	public void ChangePauseTime(float pauseDuration)
	{
		_pauseDuration = pauseDuration;
	}

	private void Update()
	{
		if (_isPaused)
		{
			if (Time.time - _lastChangeTime >= _pauseDuration)
			{
				_isPaused = false;
				_lastChangeTime = Time.time;
				for (int i = 0; i < _lights.Count; i++)
				{
					_lights[i].FadeTo(_lightCode.isLight(_index) ? 1f : 0f, 0f);
				}
			}
		}
		else
		{
			if (!(Time.time - _lastChangeTime >= _lightCode.PulseLength(_index)))
			{
				return;
			}
			_lastChangeTime = Time.time;
			if (_reversing)
			{
				_index--;
				if (_index == -1)
				{
					_reversing = false;
					_index = 0;
					_isPaused = true;
				}
			}
			else
			{
				_index++;
				if (_index == _lightCode.Count())
				{
					_isPaused = true;
					if (_reverses)
					{
						_index--;
						_reversing = true;
					}
					else
					{
						_index = 0;
					}
				}
			}
			for (int j = 0; j < _lights.Count; j++)
			{
				_lights[j].FadeTo((_isPaused || !_lightCode.isLight(_index)) ? 0f : 1f, 0f);
			}
		}
	}
}
