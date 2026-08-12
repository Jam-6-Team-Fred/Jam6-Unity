using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Detector : MonoBehaviour
{
	[Flags]
	public enum Name
	{
		Unnamed = 0,
		Player = 1,
		PlayerCamera = 2,
		Probe = 4,
		Ship = 8,
		Raft = 0x10
	}

	protected List<EffectVolume> _activeVolumes;

	protected Collider _collider;

	protected Shape _shape;

	protected Name _name;

	protected virtual void Awake()
	{
		InitializeCollections();
		SetupColliders();
		if (base.gameObject.CompareTag("PlayerDetector"))
		{
			_name = Name.Player;
		}
		else if (base.gameObject.CompareTag("PlayerCameraDetector"))
		{
			_name = Name.PlayerCamera;
		}
		else if (base.gameObject.CompareTag("ProbeDetector"))
		{
			_name = Name.Probe;
		}
		else if (base.gameObject.CompareTag("ShipDetector"))
		{
			_name = Name.Ship;
		}
		else if (base.gameObject.CompareTag("RaftDetector"))
		{
			_name = Name.Raft;
		}
		else
		{
			_name = Name.Unnamed;
		}
	}

	public bool CompareName(Name name)
	{
		return _name == name;
	}

	public bool CompareNameMask(Name nameMask)
	{
		return (_name & nameMask) != 0;
	}

	public Name GetName()
	{
		return _name;
	}

	public Shape GetShape()
	{
		return _shape;
	}

	public Collider GetCollider()
	{
		return _collider;
	}

	protected bool IsDetectorEnabled()
	{
		if (_collider != null)
		{
			return _collider.enabled;
		}
		if (_shape != null)
		{
			return _shape.active;
		}
		return true;
	}

	protected virtual void InitializeCollections()
	{
		_activeVolumes = new List<EffectVolume>(32);
	}

	protected virtual void SetupColliders()
	{
		_collider = GetComponent<Collider>();
		_shape = GetComponent<Shape>();
		if (_collider != null)
		{
			if (_collider.isTrigger)
			{
				Debug.LogError("Colliders attached to detectors should not be triggers!", base.gameObject);
				Debug.Break();
			}
			if (!OWLayerMask.IsLayerInMask(base.gameObject.layer, OWLayerMask.detectorMask))
			{
				Debug.Log("This detector is not on a detection layer!!!", base.gameObject);
				Debug.Break();
			}
		}
		if (_shape != null && _shape.collisionMode == Shape.CollisionMode.Volume)
		{
			Debug.LogError("Shapes attached to detectors should not be volumes!", base.gameObject);
		}
	}

	protected virtual void OnDestroy()
	{
	}

	public virtual void AddVolume(EffectVolume effectVolume)
	{
		if (!_activeVolumes.Contains(effectVolume))
		{
			_activeVolumes.Add(effectVolume);
			OnVolumeAdded(effectVolume);
		}
	}

	public virtual void RemoveVolume(EffectVolume effectVolume)
	{
		if (_activeVolumes.Contains(effectVolume))
		{
			_activeVolumes.Remove(effectVolume);
			OnVolumeRemoved(effectVolume);
		}
	}

	protected virtual void OnVolumeAdded(EffectVolume effectVolume)
	{
	}

	protected virtual void OnVolumeRemoved(EffectVolume effectVolume)
	{
	}
}
