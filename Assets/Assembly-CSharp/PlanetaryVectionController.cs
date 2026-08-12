using UnityEngine;

[RequireComponent(typeof(VectionFieldEmitter))]
public class PlanetaryVectionController : MonoBehaviour
{
	private enum FollowTarget
	{
		Player = 0,
		Probe = 1
	}

	private VectionFieldEmitter _vectionFieldEmitter;

	[SerializeField]
	private FollowTarget _followTarget;

	[SerializeField]
	private AnimationCurve _densityByHeight = new AnimationCurve(new Keyframe(100f, 10f), new Keyframe(150f, 0f));

	[SerializeField]
	private Sector _activeInSector;

	[SerializeField]
	private Sector[] _exclusionSectors = new Sector[0];

	private bool _inActiveSector;

	private bool _inExclusionSector;

	private void Awake()
	{
		_vectionFieldEmitter = GetComponent<VectionFieldEmitter>();
		if ((bool)_activeInSector)
		{
			_activeInSector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		if (_exclusionSectors != null)
		{
			for (int i = 0; i < _exclusionSectors.Length; i++)
			{
				_exclusionSectors[i].OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnExclusionSectorOccupantsUpdated);
			}
		}
		_inActiveSector = false;
		_inExclusionSector = false;
	}

	private void Start()
	{
		if (_followTarget == FollowTarget.Player)
		{
			_vectionFieldEmitter.emitterTransform = Locator.GetPlayerCamera().transform;
		}
		else if (_followTarget == FollowTarget.Probe)
		{
			_vectionFieldEmitter.emitterTransform = Locator.GetProbe().transform;
		}
	}

	private void OnDestroy()
	{
		if ((bool)_activeInSector)
		{
			_activeInSector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		if (_exclusionSectors == null)
		{
			return;
		}
		for (int i = 0; i < _exclusionSectors.Length; i++)
		{
			if (_exclusionSectors[i] != null)
			{
				_exclusionSectors[i].OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnExclusionSectorOccupantsUpdated);
			}
		}
	}

	private void OnSectorOccupantsUpdated()
	{
		if (_followTarget == FollowTarget.Player)
		{
			_inActiveSector = _activeInSector.ContainsOccupant(DynamicOccupant.Player);
		}
		else if (_followTarget == FollowTarget.Probe)
		{
			_inActiveSector = _activeInSector.ContainsOccupant(DynamicOccupant.Probe);
		}
		base.enabled = _inActiveSector && !_inExclusionSector;
		_vectionFieldEmitter.enabled = base.enabled;
	}

	private void OnExclusionSectorOccupantsUpdated()
	{
		if (_followTarget == FollowTarget.Player)
		{
			_inExclusionSector = false;
			for (int i = 0; i < _exclusionSectors.Length; i++)
			{
				if (_exclusionSectors[i].ContainsOccupant(DynamicOccupant.Player))
				{
					_inExclusionSector = true;
					break;
				}
			}
		}
		else if (_followTarget == FollowTarget.Probe)
		{
			_inExclusionSector = false;
			for (int j = 0; j < _exclusionSectors.Length; j++)
			{
				if (_exclusionSectors[j].ContainsOccupant(DynamicOccupant.Probe))
				{
					_inExclusionSector = true;
					break;
				}
			}
		}
		base.enabled = _inActiveSector && !_inExclusionSector;
		_vectionFieldEmitter.enabled = base.enabled;
	}

	private void FixedUpdate()
	{
		Transform emitterTransform = _vectionFieldEmitter.emitterTransform;
		if (emitterTransform != null)
		{
			float magnitude = (base.transform.position - emitterTransform.position).magnitude;
			int num = Mathf.RoundToInt(_densityByHeight.Evaluate(magnitude));
			_vectionFieldEmitter.particleCount = num;
			if (num > 0 && !_vectionFieldEmitter.enabled)
			{
				_vectionFieldEmitter.enabled = true;
			}
			else if (num == 0 && !_vectionFieldEmitter.hasAliveParticles && _vectionFieldEmitter.enabled)
			{
				_vectionFieldEmitter.enabled = false;
			}
		}
		else if (_vectionFieldEmitter.enabled)
		{
			_vectionFieldEmitter.enabled = false;
		}
	}
}
