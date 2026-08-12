using System;
using System.Collections.Generic;
using UnityEngine;

public class ElectroBarrier : SectoredMonoBehaviour
{
	[Serializable]
	private class ElectroTarget
	{
		public enum Type
		{
			General = 0,
			Player = 1,
			Ship = 2,
			Probe = 3,
			Jellyfish = 4
		}

		public OWRigidbody body;

		public Type type;

		public ParticleSystem auraParticleSystem;

		public ParticleSystem staticParticleSystem;
	}

	private const int _kMaxTargets = 16;

	[Space]
	[SerializeField]
	private float _barrierRadius = 100f;

	[SerializeField]
	private float _auraDist = 50f;

	[SerializeField]
	private float _staticDist = 10f;

	[Space]
	[SerializeField]
	private GameObject _auraPrefab;

	[SerializeField]
	private GameObject _staticPrefab;

	private OWRigidbody _parentBody;

	private OWTriggerVolume _triggerVolume;

	private List<ElectroTarget> _targetsPool;

	private List<ElectroTarget> _activeTargets;

	protected override void Awake()
	{
		base.Awake();
		_parentBody = this.GetAttachedOWRigidbody();
		_triggerVolume = GetComponent<OWTriggerVolume>();
		_targetsPool = new List<ElectroTarget>(16);
		_activeTargets = new List<ElectroTarget>(16);
		for (int i = 0; i < 16; i++)
		{
			ElectroTarget electroTarget = new ElectroTarget();
			electroTarget.auraParticleSystem = UnityEngine.Object.Instantiate(_auraPrefab, base.transform).GetComponent<ParticleSystem>();
			electroTarget.staticParticleSystem = UnityEngine.Object.Instantiate(_staticPrefab, base.transform).GetComponent<ParticleSystem>();
			_targetsPool.Add(electroTarget);
		}
		_triggerVolume.OnEntry += OnEnterBarrierZone;
		_triggerVolume.OnExit += OnExitBarrierZone;
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_triggerVolume.OnEntry -= OnEnterBarrierZone;
		_triggerVolume.OnExit -= OnExitBarrierZone;
	}

	private void Update()
	{
		for (int i = 0; i < _activeTargets.Count; i++)
		{
			ElectroTarget electroTarget = _activeTargets[i];
			Vector3 forward = electroTarget.body.GetWorldCenterOfMass() - base.transform.position;
			float num = Mathf.Abs(forward.magnitude - _barrierRadius);
			Quaternion rotation = Quaternion.LookRotation(forward);
			electroTarget.auraParticleSystem.transform.rotation = rotation;
			electroTarget.staticParticleSystem.transform.rotation = rotation;
			float auraDist = _auraDist;
			float staticDist = _staticDist;
			if (electroTarget.type == ElectroTarget.Type.Player)
			{
				auraDist *= 0.2f;
				staticDist *= 0.1f;
			}
			else if (electroTarget.type == ElectroTarget.Type.Probe)
			{
				auraDist *= 0.1f;
				staticDist *= 0.05f;
			}
			if (num <= _auraDist && !electroTarget.auraParticleSystem.isPlaying)
			{
				electroTarget.auraParticleSystem.Play();
			}
			else if (num > _auraDist && electroTarget.auraParticleSystem.isPlaying)
			{
				electroTarget.auraParticleSystem.Stop();
			}
			if (num <= _staticDist && !electroTarget.staticParticleSystem.isPlaying)
			{
				electroTarget.staticParticleSystem.Play();
			}
			else if (num > _staticDist && electroTarget.staticParticleSystem.isPlaying)
			{
				electroTarget.staticParticleSystem.Stop();
			}
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (base.enabled && !flag)
		{
			for (int i = 0; i < _activeTargets.Count; i++)
			{
				_activeTargets[i].auraParticleSystem.Stop();
				_activeTargets[i].staticParticleSystem.Stop();
			}
		}
		base.enabled = flag;
	}

	private void OnEnterBarrierZone(GameObject hitObj)
	{
		OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody();
		if (attachedOWRigidbody == null || attachedOWRigidbody == _parentBody)
		{
			return;
		}
		for (int i = 0; i < _activeTargets.Count; i++)
		{
			if (_activeTargets[i].body == attachedOWRigidbody)
			{
				return;
			}
		}
		ElectroTarget electroTarget = _targetsPool[_targetsPool.Count - 1];
		_targetsPool.RemoveAt(_targetsPool.Count - 1);
		electroTarget.body = attachedOWRigidbody;
		if (attachedOWRigidbody.CompareTag("Player"))
		{
			electroTarget.type = ElectroTarget.Type.Player;
		}
		else if (attachedOWRigidbody.CompareTag("Ship"))
		{
			electroTarget.type = ElectroTarget.Type.Ship;
		}
		else if (attachedOWRigidbody.CompareTag("Probe"))
		{
			electroTarget.type = ElectroTarget.Type.Probe;
		}
		else if (attachedOWRigidbody.GetComponent<JellyfishController>() != null)
		{
			electroTarget.type = ElectroTarget.Type.Jellyfish;
		}
		else
		{
			electroTarget.type = ElectroTarget.Type.General;
		}
		_activeTargets.Add(electroTarget);
		base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}

	private void OnExitBarrierZone(GameObject hitObj)
	{
		OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody();
		if (attachedOWRigidbody == null || attachedOWRigidbody == _parentBody)
		{
			return;
		}
		for (int i = 0; i < _activeTargets.Count; i++)
		{
			if (_activeTargets[i].body == attachedOWRigidbody)
			{
				_activeTargets[i].body = null;
				_activeTargets[i].auraParticleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
				_activeTargets[i].staticParticleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
				_targetsPool.Add(_activeTargets[i]);
				_activeTargets.QuickRemoveAt(i);
				return;
			}
		}
		if (_activeTargets.Count == 0)
		{
			base.enabled = false;
		}
	}
}
