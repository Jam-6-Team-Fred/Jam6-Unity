using UnityEngine;

public class VisibilityObject : SectoredMonoBehaviour
{
	[SerializeField]
	private bool _checkIllumination;

	[SerializeField]
	private float _illuminationRadius;

	[SerializeField]
	private Vector3 _localIlluminationOffset = Vector3.zero;

	protected VisibilityTracker[] _visibilityTrackers;

	private Light[] _lightSources;

	private bool _isVisible;

	private bool _wasVisible;

	private bool _isIlluminated;

	private bool _wasIlluminated;

	private bool _sectorActive;

	private bool _gameplayActive = true;

	public bool IsVisible()
	{
		return _isVisible;
	}

	public bool IsIlluminated()
	{
		return _isIlluminated;
	}

	public bool IsNewlyObscured()
	{
		if (!_wasVisible || _isVisible)
		{
			if (_isVisible && _wasIlluminated)
			{
				return !_isIlluminated;
			}
			return false;
		}
		return true;
	}

	public bool IsNewlyDarkened()
	{
		if (_wasIlluminated)
		{
			return !_isIlluminated;
		}
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		if (_visibilityTrackers == null)
		{
			_visibilityTrackers = GetComponentsInChildren<VisibilityTracker>();
		}
	}

	public void SetActivation(bool active)
	{
		_gameplayActive = active;
		CheckEnabled();
	}

	public void SetLightSources(Light[] lightSources)
	{
		_lightSources = lightSources;
		if (_lightSources.Length != 0)
		{
			_checkIllumination = true;
		}
	}

	public bool CheckPointInside(Vector3 worldPos)
	{
		for (int i = 0; i < _visibilityTrackers.Length; i++)
		{
			if (_visibilityTrackers[i].IsPointInside(worldPos))
			{
				return true;
			}
		}
		return false;
	}

	protected bool CheckVisibility()
	{
		for (int i = 0; i < _visibilityTrackers.Length; i++)
		{
			if (_visibilityTrackers[i].IsVisible())
			{
				return true;
			}
		}
		return false;
	}

	protected bool CheckVisibilityFromProbe(OWCamera camera)
	{
		for (int i = 0; i < _visibilityTrackers.Length; i++)
		{
			if (_visibilityTrackers[i].IsVisibleToProbe(camera))
			{
				return true;
			}
		}
		return false;
	}

	protected bool CheckVisibilityInstantly()
	{
		for (int i = 0; i < _visibilityTrackers.Length; i++)
		{
			if (_visibilityTrackers[i].IsVisibleUsingCameraFrustum())
			{
				return true;
			}
		}
		return false;
	}

	protected virtual bool CheckIllumination()
	{
		if (!_checkIllumination)
		{
			return true;
		}
		Vector3 point = base.transform.TransformPoint(_localIlluminationOffset);
		if (Locator.GetFlashlight().CheckIlluminationAtPoint(point, _illuminationRadius))
		{
			return true;
		}
		if (Locator.GetProbe() != null && Locator.GetProbe().IsLaunched() && Locator.GetProbe().CheckIlluminationAtPoint(point, _illuminationRadius))
		{
			return true;
		}
		if (Locator.GetThrusterLightTracker().CheckIlluminationAtPoint(point, _illuminationRadius))
		{
			return true;
		}
		if (_lightSources != null)
		{
			for (int i = 0; i < _lightSources.Length; i++)
			{
				if (_lightSources[i].intensity > 0f && _lightSources[i].range > 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected virtual void Update()
	{
		_wasVisible = _isVisible;
		_wasIlluminated = _isIlluminated;
		_isIlluminated = CheckIllumination();
		_isVisible = CheckVisibility();
	}

	protected override void OnSectorOccupantsUpdated()
	{
		_sectorActive = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		CheckEnabled();
	}

	protected virtual void CheckEnabled()
	{
		base.enabled = (_sectorActive || _sector == null) && _gameplayActive;
		for (int i = 0; i < _visibilityTrackers.Length; i++)
		{
			_visibilityTrackers[i].enabled = base.enabled;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_checkIllumination)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(base.transform.TransformPoint(_localIlluminationOffset), _illuminationRadius);
		}
	}
}
