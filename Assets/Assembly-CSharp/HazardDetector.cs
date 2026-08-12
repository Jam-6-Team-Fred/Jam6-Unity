using System.Collections.Generic;
using UnityEngine;

public class HazardDetector : Detector
{
	public delegate void FirstContactDamageEvent(HazardVolume volume);

	public delegate void HazardUpdatedEvent();

	private OWRigidbody _owRigidbody;

	[SerializeField]
	private GameObject _darkMatterEntryEffect;

	[SerializeField]
	private float _darkMatterMinEntrySpeed = 1f;

	[Space]
	[SerializeField]
	private ElectricityEffect[] _electricityEffects = new ElectricityEffect[0];

	private List<InsulatingVolume> _insulatingVolumes;

	private int _hazardMask;

	private float _genericDamagePerSecond;

	private float _darkMatterDamagePerSecond;

	private float _heatDamagePerSecond;

	private float _fireDamagePerSecond;

	private float _sandDamagePerSecond;

	private float _electricityDamagePerSecond;

	private bool _isPlayerDetector;

	public event FirstContactDamageEvent OnFirstContactDamage;

	public event HazardUpdatedEvent OnHazardsUpdated;

	protected override void Awake()
	{
		base.Awake();
		_owRigidbody = this.GetAttachedOWRigidbody();
		_insulatingVolumes = new List<InsulatingVolume>(4);
		_isPlayerDetector = base.gameObject.CompareTag("PlayerDetector");
	}

	public float GetNetDamagePerSecond()
	{
		return _genericDamagePerSecond + _darkMatterDamagePerSecond + _fireDamagePerSecond + _sandDamagePerSecond + _heatDamagePerSecond + _electricityDamagePerSecond;
	}

	public bool GetDisplayDangerMarker()
	{
		if (_activeVolumes.Count > 0)
		{
			if (GetNetDamagePerSecond() > 0f)
			{
				return true;
			}
			for (int i = 0; i < _activeVolumes.Count; i++)
			{
				HazardVolume hazardVolume = _activeVolumes[i] as HazardVolume;
				if (hazardVolume.GetHazardType() != HazardVolume.HazardType.ELECTRICITY || !IsInsulated())
				{
					if (hazardVolume.GetHazardType() == HazardVolume.HazardType.RAPIDS)
					{
						return true;
					}
					if (hazardVolume.GetFirstContactDamage() > 0f)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool InHazardType(HazardVolume.HazardType type)
	{
		if (IsInvulnerable())
		{
			return false;
		}
		if (type == HazardVolume.HazardType.NONE)
		{
			return _hazardMask == 0;
		}
		return (int)((uint)_hazardMask & (uint)type) > 0;
	}

	public HazardVolume.HazardType GetLatestHazardType()
	{
		for (int num = _activeVolumes.Count - 1; num >= 0; num--)
		{
			HazardVolume.HazardType hazardType = (_activeVolumes[num] as HazardVolume).GetHazardType();
			if (hazardType != HazardVolume.HazardType.ELECTRICITY || !IsInsulated())
			{
				return hazardType;
			}
		}
		return HazardVolume.HazardType.NONE;
	}

	public DarkMatterVolume GetDarkMatterVolume()
	{
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			if (_activeVolumes[i] is DarkMatterVolume)
			{
				return _activeVolumes[i] as DarkMatterVolume;
			}
		}
		return null;
	}

	public void PlayElectricityEffects()
	{
		for (int i = 0; i < _electricityEffects.Length; i++)
		{
			_electricityEffects[i].Play();
		}
	}

	public void AddInsulatingVolume(InsulatingVolume insulatingVolume)
	{
		if (!_insulatingVolumes.Contains(insulatingVolume))
		{
			_insulatingVolumes.Add(insulatingVolume);
			_hazardMask &= -33;
		}
	}

	public void RemoveInsulatingVolume(InsulatingVolume insulatingVolume)
	{
		if (!_insulatingVolumes.Contains(insulatingVolume))
		{
			return;
		}
		_insulatingVolumes.Remove(insulatingVolume);
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			if ((_activeVolumes[i] as HazardVolume).GetHazardType() == HazardVolume.HazardType.ELECTRICITY)
			{
				_hazardMask |= 32;
			}
		}
	}

	protected override void OnVolumeAdded(EffectVolume eVolume)
	{
		HazardVolume hazardVolume = eVolume as HazardVolume;
		if (hazardVolume == null)
		{
			return;
		}
		HazardVolume.HazardType hazardType = hazardVolume.GetHazardType();
		if (hazardType == HazardVolume.HazardType.ELECTRICITY && IsInsulated())
		{
			return;
		}
		if (hazardVolume.GetFirstContactDamage() > 0f && this.OnFirstContactDamage != null && !IsInvulnerable())
		{
			this.OnFirstContactDamage(hazardVolume);
		}
		_hazardMask |= (int)hazardType;
		if (hazardType == HazardVolume.HazardType.DARKMATTER && _darkMatterEntryEffect != null)
		{
			OWRigidbody attachedOWRigidbody = hazardVolume.GetAttachedOWRigidbody();
			Vector3 relativeVelocity = attachedOWRigidbody.GetRelativeVelocity(_owRigidbody);
			if (relativeVelocity.sqrMagnitude >= _darkMatterMinEntrySpeed * _darkMatterMinEntrySpeed)
			{
				Object.Instantiate(_darkMatterEntryEffect, base.transform.position, Quaternion.LookRotation(relativeVelocity), attachedOWRigidbody.transform);
			}
		}
		if (this.OnHazardsUpdated != null)
		{
			this.OnHazardsUpdated();
		}
	}

	protected override void OnVolumeRemoved(EffectVolume eVolume)
	{
		_hazardMask = 0;
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			_hazardMask |= (int)(_activeVolumes[i] as HazardVolume).GetHazardType();
		}
		if (IsInsulated())
		{
			_hazardMask &= -33;
		}
		if (this.OnHazardsUpdated != null)
		{
			this.OnHazardsUpdated();
		}
	}

	private bool IsInvulnerable()
	{
		if (_isPlayerDetector)
		{
			if (!PlayerState.IsInsideShip())
			{
				return PlayerState.IsInsideTheEye();
			}
			return true;
		}
		return false;
	}

	public bool IsInsulated()
	{
		if (_insulatingVolumes.Count <= 0)
		{
			if (_isPlayerDetector)
			{
				return PlayerState.IsInsideShip();
			}
			return false;
		}
		return true;
	}

	private void Update()
	{
		_genericDamagePerSecond = 0f;
		_darkMatterDamagePerSecond = 0f;
		_fireDamagePerSecond = 0f;
		_heatDamagePerSecond = 0f;
		_sandDamagePerSecond = 0f;
		_electricityDamagePerSecond = 0f;
		if (IsInvulnerable())
		{
			return;
		}
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			HazardVolume obj = _activeVolumes[i] as HazardVolume;
			float num = obj.GetDamagePerSecond(this);
			if (obj.GetHazardType() == HazardVolume.HazardType.ELECTRICITY && IsInsulated())
			{
				num = 0f;
			}
			switch (obj.GetHazardType())
			{
			case HazardVolume.HazardType.GENERAL:
			case HazardVolume.HazardType.RAPIDS:
				_genericDamagePerSecond += num;
				break;
			case HazardVolume.HazardType.DARKMATTER:
				_darkMatterDamagePerSecond += num;
				break;
			case HazardVolume.HazardType.FIRE:
				_fireDamagePerSecond += num;
				break;
			case HazardVolume.HazardType.SANDFALL:
				_sandDamagePerSecond += num;
				break;
			case HazardVolume.HazardType.HEAT:
				_heatDamagePerSecond += num;
				break;
			case HazardVolume.HazardType.ELECTRICITY:
				_electricityDamagePerSecond += num;
				break;
			}
		}
	}
}
