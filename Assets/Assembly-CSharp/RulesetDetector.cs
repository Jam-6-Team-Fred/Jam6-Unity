using System.Collections.Generic;
using UnityEngine;

public class RulesetDetector : Detector
{
	public delegate void RulesetEvent();

	private PlanetoidRuleset _closestPlanetoidRuleset;

	private List<PlanetoidRuleset> _planetoidRulesets;

	private List<ProbeRuleset> _probeRulesets;

	private List<ThrustRuleset> _thrustRulesets;

	private List<EffectRuleset> _effectRulesets;

	private List<ShockLayerRuleset> _shockLayerRulesets;

	private List<AntiTravelMusicRuleset> _antiTravelMusicRulesets;

	private RingRiverRuleset _ringRiverRuleset;

	private DreamLanternRuleset _dreamLanternRuleset;

	private PlayerImpactRuleset _impactRuleset;

	public event RulesetEvent OnChangeRuleset;

	protected override void Awake()
	{
		base.Awake();
		_planetoidRulesets = new List<PlanetoidRuleset>(8);
		_thrustRulesets = new List<ThrustRuleset>(8);
		_effectRulesets = new List<EffectRuleset>(8);
		_shockLayerRulesets = new List<ShockLayerRuleset>(8);
		_probeRulesets = new List<ProbeRuleset>(8);
		_antiTravelMusicRulesets = new List<AntiTravelMusicRuleset>(8);
	}

	public override void AddVolume(EffectVolume volume)
	{
		if (volume as RulesetVolume != null)
		{
			base.AddVolume(volume);
			if (volume.GetType() == typeof(PlanetoidRuleset))
			{
				_planetoidRulesets.Add((PlanetoidRuleset)volume);
				UpdateClosestPlanetoidRuleset();
			}
			else if (volume.GetType() == typeof(ThrustRuleset))
			{
				_thrustRulesets.Add((ThrustRuleset)volume);
			}
			else if (volume.GetType() == typeof(ProbeRuleset))
			{
				_probeRulesets.Add((ProbeRuleset)volume);
			}
			else if (volume.GetType() == typeof(EffectRuleset))
			{
				_effectRulesets.Add((EffectRuleset)volume);
			}
			else if (volume.GetType() == typeof(ShockLayerRuleset))
			{
				_shockLayerRulesets.Add((ShockLayerRuleset)volume);
			}
			else if (volume.GetType() == typeof(AntiTravelMusicRuleset))
			{
				_antiTravelMusicRulesets.Add((AntiTravelMusicRuleset)volume);
			}
			else if (volume.GetType() == typeof(RingRiverRuleset))
			{
				_ringRiverRuleset = (RingRiverRuleset)volume;
			}
			else if (volume.GetType() == typeof(DreamLanternRuleset))
			{
				_dreamLanternRuleset = (DreamLanternRuleset)volume;
			}
			else if (volume.GetType() == typeof(PlayerImpactRuleset))
			{
				_impactRuleset = (PlayerImpactRuleset)volume;
			}
			if (this.OnChangeRuleset != null)
			{
				this.OnChangeRuleset();
			}
		}
	}

	public override void RemoveVolume(EffectVolume volume)
	{
		if (volume as RulesetVolume != null)
		{
			base.RemoveVolume(volume);
			if (volume.GetType() == typeof(PlanetoidRuleset))
			{
				_planetoidRulesets.Remove((PlanetoidRuleset)volume);
				UpdateClosestPlanetoidRuleset();
			}
			else if (volume.GetType() == typeof(ThrustRuleset))
			{
				_thrustRulesets.Remove((ThrustRuleset)volume);
			}
			else if (volume.GetType() == typeof(ProbeRuleset))
			{
				_probeRulesets.Remove((ProbeRuleset)volume);
			}
			else if (volume.GetType() == typeof(EffectRuleset))
			{
				_effectRulesets.Remove((EffectRuleset)volume);
			}
			else if (volume.GetType() == typeof(ShockLayerRuleset))
			{
				_shockLayerRulesets.Remove((ShockLayerRuleset)volume);
			}
			else if (volume.GetType() == typeof(AntiTravelMusicRuleset))
			{
				_antiTravelMusicRulesets.Remove((AntiTravelMusicRuleset)volume);
			}
			else if (volume.GetType() == typeof(RingRiverRuleset))
			{
				_ringRiverRuleset = null;
			}
			else if (volume.GetType() == typeof(DreamLanternRuleset))
			{
				_dreamLanternRuleset = null;
			}
			else if (volume.GetType() == typeof(PlayerImpactRuleset))
			{
				_impactRuleset = null;
			}
			if (this.OnChangeRuleset != null)
			{
				this.OnChangeRuleset();
			}
		}
	}

	public RingRiverRuleset GetRingRiverRuleset()
	{
		return _ringRiverRuleset;
	}

	public DreamLanternRuleset GetDreamLanternRuleset()
	{
		return _dreamLanternRuleset;
	}

	public PlayerImpactRuleset GetImpactRuleset()
	{
		return _impactRuleset;
	}

	public bool AllowTravelMusic()
	{
		return _antiTravelMusicRulesets.Count <= 0;
	}

	public PlanetoidRuleset GetPlanetoidRuleset()
	{
		return _closestPlanetoidRuleset;
	}

	public ProbeRuleset GetProbeRuleSet()
	{
		if (_probeRulesets.Count != 0)
		{
			return _probeRulesets[_probeRulesets.Count - 1];
		}
		return null;
	}

	public float GetThrustLimit()
	{
		float num = float.PositiveInfinity;
		for (int i = 0; i < _thrustRulesets.Count; i++)
		{
			num = Mathf.Min(num, _thrustRulesets[i].GetThrustLimit());
		}
		return num;
	}

	public bool IsJetpackBoosterNerfed(out float nerfDuration)
	{
		nerfDuration = 0f;
		bool result = false;
		for (int i = 0; i < _thrustRulesets.Count; i++)
		{
			if (_thrustRulesets[i].IsJetpackBoosterNerfed())
			{
				nerfDuration = Mathf.Max(nerfDuration, _thrustRulesets[i].GetJetpackBoosterNerfDuration());
				result = true;
			}
		}
		return result;
	}

	public bool IsOnDaySide()
	{
		if (!(GetPlanetoidRuleset() == null))
		{
			return GetPlanetoidRuleset().IsDayAtPosition(base.transform.position);
		}
		return false;
	}

	public bool GetUseMinimap()
	{
		if (!(GetPlanetoidRuleset() == null))
		{
			return GetPlanetoidRuleset().GetUseMinimap();
		}
		return false;
	}

	public bool GetUseAltimeter(Vector3 fromWorldPosition)
	{
		if (!(GetPlanetoidRuleset() == null))
		{
			return GetPlanetoidRuleset().GetUseAltimeter(fromWorldPosition);
		}
		return false;
	}

	public EffectRuleset.BubbleType GetEffectBubbleType()
	{
		if (_effectRulesets.Count == 0)
		{
			return EffectRuleset.BubbleType.None;
		}
		return _effectRulesets[_effectRulesets.Count - 1].GetEffectBubbleType();
	}

	public Material GetEffectBubbleMaterial()
	{
		if (_effectRulesets.Count != 0)
		{
			return _effectRulesets[_effectRulesets.Count - 1].GetEffectBubbleMaterial();
		}
		return null;
	}

	public Material GetCloudEffectBubbleMaterial()
	{
		if (_effectRulesets.Count != 0)
		{
			return _effectRulesets[_effectRulesets.Count - 1].GetCloudEffectBubbleMaterial();
		}
		return null;
	}

	public Material GetSandEffectBubbleMaterial()
	{
		if (CompareName(Name.Probe))
		{
			for (int i = 0; i < _effectRulesets.Count; i++)
			{
				Sector component = _effectRulesets[i].GetComponent<Sector>();
				if (component != null && component.GetName() == Sector.Name.TimeLoopDevice)
				{
					return _effectRulesets[i].GetSandEffectBubbleMaterial();
				}
			}
		}
		if (_effectRulesets.Count != 0)
		{
			return _effectRulesets[_effectRulesets.Count - 1].GetSandEffectBubbleMaterial();
		}
		return null;
	}

	public EffectRuleset GetCurrentEffectRuleset()
	{
		if (_effectRulesets.Count != 0)
		{
			return _effectRulesets[_effectRulesets.Count - 1];
		}
		return null;
	}

	public bool GetUseShockLayer()
	{
		if (_shockLayerRulesets.Count == 0)
		{
			return false;
		}
		return _shockLayerRulesets[_shockLayerRulesets.Count - 1].UsesShockLayer();
	}

	public ShockLayerRuleset.ShockType GetShockLayerType()
	{
		if (_shockLayerRulesets.Count == 0)
		{
			return ShockLayerRuleset.ShockType.None;
		}
		return _shockLayerRulesets[_shockLayerRulesets.Count - 1].GetShockLayerType();
	}

	public ShockLayerRuleset GetCurrentShockLayerRuleset()
	{
		if (_shockLayerRulesets.Count != 0)
		{
			return _shockLayerRulesets[_shockLayerRulesets.Count - 1];
		}
		return null;
	}

	private void Update()
	{
		if (_planetoidRulesets.Count > 1)
		{
			UpdateClosestPlanetoidRuleset();
		}
	}

	private void UpdateClosestPlanetoidRuleset()
	{
		_closestPlanetoidRuleset = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _planetoidRulesets.Count; i++)
		{
			float num2 = Vector3.SqrMagnitude(_planetoidRulesets[i].transform.position - base.transform.position);
			if (num2 < num)
			{
				_closestPlanetoidRuleset = _planetoidRulesets[i];
				num = num2;
			}
		}
	}
}
