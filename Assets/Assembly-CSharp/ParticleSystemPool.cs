using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemPool
{
	public delegate void PoolEvent(ParticleSystem particleSystem);

	private int _poolSize;

	private List<ParticleSystem> _reserved;

	private List<ParticleSystem> _active;

	private GameObject _prefab;

	private Transform _inactiveRoot;

	public event PoolEvent OnReturnToPool;

	public ParticleSystemPool(GameObject particleSystemPrefab, int poolSize, Transform inactiveRoot = null)
	{
		_poolSize = poolSize;
		_reserved = new List<ParticleSystem>(_poolSize);
		_active = new List<ParticleSystem>(_poolSize);
		_prefab = particleSystemPrefab;
		_inactiveRoot = inactiveRoot;
		if (_prefab.GetComponent<ParticleSystem>() == null)
		{
			Debug.LogError("Tried to populate a ParticleSystemPool with a Prefab that has no ParticleSystem attached", particleSystemPrefab);
			return;
		}
		for (int i = 0; i < _poolSize; i++)
		{
			GameObject gameObject = Object.Instantiate(_prefab);
			gameObject.SetActive(value: false);
			gameObject.transform.SetParent(_inactiveRoot);
			_reserved.Add(gameObject.GetComponent<ParticleSystem>());
		}
	}

	public ParticleSystem Instantiate(Transform parent, Vector3 position, Quaternion rotation)
	{
		if (_reserved.Count == 0)
		{
			Debug.LogWarning("Could not instantiate ParticleSystem Prefab " + _prefab.name + "; pool empty");
			return null;
		}
		ParticleSystem particleSystem = _reserved[_reserved.Count - 1];
		_reserved.RemoveAt(_reserved.Count - 1);
		_active.Add(particleSystem);
		particleSystem.gameObject.SetActive(value: true);
		particleSystem.transform.SetParent(parent);
		particleSystem.transform.SetPositionAndRotation(position, rotation);
		particleSystem.Play();
		return particleSystem;
	}

	public void Update()
	{
		for (int num = _active.Count - 1; num >= 0; num--)
		{
			if (!_active[num].IsAlive(withChildren: true))
			{
				ParticleSystem particleSystem = _active[num];
				_active.QuickRemoveAt(num);
				_reserved.Add(particleSystem);
				particleSystem.gameObject.SetActive(value: false);
				particleSystem.transform.SetParent(_inactiveRoot);
				if (this.OnReturnToPool != null)
				{
					this.OnReturnToPool(particleSystem);
				}
			}
		}
	}

	public void StopAndReturnAll()
	{
		for (int i = 0; i < _active.Count; i++)
		{
			ParticleSystem particleSystem = _active[i];
			particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
			_reserved.Add(particleSystem);
			particleSystem.gameObject.SetActive(value: false);
			particleSystem.transform.SetParent(_inactiveRoot);
			if (this.OnReturnToPool != null)
			{
				this.OnReturnToPool(particleSystem);
			}
		}
		_active.Clear();
	}
}
