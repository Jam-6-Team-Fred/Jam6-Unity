using System.Collections.Generic;
using UnityEngine;

public class EyeOrbitHologram : Hologram
{
	[SerializeField]
	private Transform _solarSystemRoot;

	[SerializeField]
	private HologramOrbit[] _orbits;

	[SerializeField]
	private GameObject _eyeOrbitPrefab;

	[SerializeField]
	private bool _rotatingEyeMode;

	private List<HologramOrbit> _eyeOrbitList;

	private float _targetScale = 1f;

	private bool _spawnEyeOrbits;

	private float _lastEyeSpawnTime;

	private void Start()
	{
		_solarSystemRoot.localScale = Vector3.one;
		_eyeOrbitList = new List<HologramOrbit>(16);
	}

	private void OnDestroy()
	{
	}

	public void SetOrbitVisibility(int index, bool visible)
	{
		if (_orbits.Length > index)
		{
			_orbits[index].SetVisible(visible);
			return;
		}
		if (visible)
		{
			_targetScale = 0.1f;
			return;
		}
		for (int i = 0; i < _eyeOrbitList.Count; i++)
		{
			_eyeOrbitList[i].SetVisible(visible: false);
		}
		_eyeOrbitList.Clear();
		_targetScale = 1f;
		_spawnEyeOrbits = false;
	}

	protected override void OnActivation()
	{
	}

	protected override void OnFinishActivation()
	{
	}

	protected override void OnDeactivation()
	{
	}

	protected override void UpdateHologram()
	{
		_solarSystemRoot.localScale = Vector3.MoveTowards(_solarSystemRoot.localScale, _targetScale * Vector3.one, Time.deltaTime);
		if (_targetScale < 1f && !_spawnEyeOrbits && _solarSystemRoot.localScale.x == _targetScale)
		{
			_spawnEyeOrbits = true;
		}
		else if (_spawnEyeOrbits && ((_rotatingEyeMode && _eyeOrbitList.Count == 0) || (!_rotatingEyeMode && Time.time > _lastEyeSpawnTime + 1f)))
		{
			GameObject obj = Object.Instantiate(_eyeOrbitPrefab, _solarSystemRoot.position, _solarSystemRoot.rotation);
			obj.transform.parent = _solarSystemRoot;
			HologramOrbit component = obj.GetComponent<HologramOrbit>();
			component.SetVisible(visible: true, _rotatingEyeMode);
			_eyeOrbitList.Add(component);
			_lastEyeSpawnTime = Time.time;
			if (_eyeOrbitList.Count > 10)
			{
				_eyeOrbitList[0].SetVisible(visible: false);
				_eyeOrbitList.RemoveAt(0);
			}
		}
	}
}
