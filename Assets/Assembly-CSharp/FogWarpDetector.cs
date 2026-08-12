using System.Collections.Generic;
using UnityEngine;

public class FogWarpDetector : MonoBehaviour
{
	public enum Name
	{
		None = 0,
		Player = 1,
		Ship = 2,
		Probe = 3
	}

	public delegate void TrackFogWarpVolumeEvent(FogWarpVolume volume);

	public delegate void UntrackFogWarpVolumeEvent(FogWarpVolume volume);

	public delegate void OuterFogWarpVolumeChangeEvent(OuterFogWarpVolume volume);

	public delegate void FogWarpDetectorDestroyedEvent();

	[SerializeField]
	protected Name _name;

	protected SectorDetector _sectorDetector;

	protected OWRigidbody _attachedBody;

	protected OuterFogWarpVolume _outerWarpVolume;

	protected List<FogWarpVolume> _warpVolumes;

	protected float _targetFogFraction;

	protected FogWarpVolume _closestFogWarp;

	private NoiseMaker _noiseMaker;

	public event TrackFogWarpVolumeEvent OnTrackFogWarpVolume;

	public event UntrackFogWarpVolumeEvent OnUntrackFogWarpVolume;

	public event OuterFogWarpVolumeChangeEvent OnOuterFogWarpVolumeChange;

	public event FogWarpDetectorDestroyedEvent OnFogWarpDetectorDestroyed;

	private void Awake()
	{
		_warpVolumes = new List<FogWarpVolume>(8);
		_attachedBody = this.GetAttachedOWRigidbody();
		_sectorDetector = this.GetRequiredComponent<SectorDetector>();
		_noiseMaker = GetComponent<NoiseMaker>();
	}

	protected virtual void Start()
	{
		if (_warpVolumes.Count == 0)
		{
			base.enabled = false;
		}
	}

	private void OnDestroy()
	{
		if (this.OnFogWarpDetectorDestroyed != null)
		{
			this.OnFogWarpDetectorDestroyed();
		}
	}

	public bool CompareName(Name name)
	{
		return _name == name;
	}

	public float GetTargetFogFraction()
	{
		return _targetFogFraction;
	}

	public SectorDetector GetSectorDetector()
	{
		return _sectorDetector;
	}

	public OWRigidbody GetOWRigidbody()
	{
		return _attachedBody;
	}

	public OuterFogWarpVolume GetOuterFogWarpVolume()
	{
		return _outerWarpVolume;
	}

	public virtual void OnFogWarp()
	{
		if (_noiseMaker != null)
		{
			_noiseMaker.OnFogWarp();
		}
	}

	public void TrackFogWarpVolume(FogWarpVolume volume)
	{
		bool flag = false;
		if (!_warpVolumes.SafeAdd(volume))
		{
			return;
		}
		base.enabled = true;
		if (volume.IsOuterWarpVolume())
		{
			if (_outerWarpVolume != null)
			{
				Debug.LogError("Entering an outer warp volume before leaving the old one!");
				Debug.Break();
			}
			if (_outerWarpVolume != volume)
			{
				flag = true;
			}
			_outerWarpVolume = (OuterFogWarpVolume)volume;
		}
		if (this.OnTrackFogWarpVolume != null)
		{
			this.OnTrackFogWarpVolume(volume);
		}
		if (flag && this.OnOuterFogWarpVolumeChange != null)
		{
			this.OnOuterFogWarpVolumeChange(_outerWarpVolume);
		}
	}

	public void UntrackFogWarpVolume(FogWarpVolume volume)
	{
		if (!_warpVolumes.Remove(volume))
		{
			return;
		}
		if (this.OnUntrackFogWarpVolume != null)
		{
			this.OnUntrackFogWarpVolume(volume);
		}
		if (volume.IsOuterWarpVolume())
		{
			_outerWarpVolume = null;
			if (this.OnOuterFogWarpVolumeChange != null)
			{
				this.OnOuterFogWarpVolumeChange(_outerWarpVolume);
			}
		}
		if (_warpVolumes.Count == 0)
		{
			base.enabled = false;
			_targetFogFraction = 0f;
		}
	}

	protected virtual void FixedUpdate()
	{
		_targetFogFraction = 0f;
		_closestFogWarp = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _warpVolumes.Count; i++)
		{
			if (!_warpVolumes[i].IsProbeOnly() || _name == Name.Probe)
			{
				FogWarpVolume fogWarpVolume = _warpVolumes[i];
				float num2 = Mathf.Abs(fogWarpVolume.CheckWarpProximity(this));
				float b = Mathf.Clamp01(1f - Mathf.Abs(num2) / fogWarpVolume.GetFogThickness());
				_targetFogFraction = Mathf.Max(_targetFogFraction, b);
				if (num2 < num)
				{
					num = num2;
					_closestFogWarp = fogWarpVolume;
				}
			}
		}
	}
}
