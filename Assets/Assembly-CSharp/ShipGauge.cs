using UnityEngine;

public class ShipGauge : MonoBehaviour
{
	[SerializeField]
	protected ShipLODTrigger _lodTrigger;

	[Space]
	[SerializeField]
	protected Transform _needleTransform;

	[SerializeField]
	protected float _needleAngleMin;

	[SerializeField]
	protected float _needleAngleMax = 180f;

	[Space]
	[SerializeField]
	protected OWRenderer _indicatorLight;

	[SerializeField]
	protected float _indicatorWarningThreshold = 0.3f;

	[ColorUsage(false, true)]
	[SerializeField]
	protected Color _indicatorWarningColor = new Color(1.5f, 1f, 0.5f);

	[ColorUsage(false, true)]
	[SerializeField]
	protected Color _indicatorCriticalColor = new Color(1.3f, 0.55f, 0.55f);

	private bool _shipDestroyed;

	private Quaternion _currentNeedleRotation;

	private bool _lightActive;

	protected virtual void Awake()
	{
		_currentNeedleRotation = _needleTransform.localRotation;
		_lightActive = false;
		base.enabled = false;
		_lodTrigger.OnTriggerUpdated += new OWEvent.OWCallback(OnTriggerUpdated);
		GlobalMessenger.AddListener("ShipSystemFailure", OnShipSystemFailure);
	}

	protected virtual void OnDestroy()
	{
		_lodTrigger.OnTriggerUpdated -= new OWEvent.OWCallback(OnTriggerUpdated);
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipSystemFailure);
	}

	protected virtual void OnTriggerUpdated()
	{
		base.enabled = _lodTrigger.isPlayerInTrigger || _lodTrigger.isProbeInTrigger;
	}

	protected virtual void OnShipSystemFailure()
	{
		_shipDestroyed = true;
		_indicatorLight.SetEmissionColor(Color.black);
		_lightActive = false;
	}

	protected virtual void UpdateVisuals(float t)
	{
		Quaternion quaternion = Quaternion.AngleAxis(Mathf.LerpUnclamped(_needleAngleMin, _needleAngleMax, t), Vector3.right);
		if (Quaternion.Angle(_currentNeedleRotation, quaternion) >= 0.1f)
		{
			_needleTransform.localRotation = quaternion;
			_currentNeedleRotation = quaternion;
		}
		if (t <= _indicatorWarningThreshold && !_shipDestroyed)
		{
			if (t > 0f)
			{
				if (!_lightActive)
				{
					_indicatorLight.SetEmissionColor(_indicatorWarningColor);
					_lightActive = true;
				}
			}
			else
			{
				_lightActive = true;
				bool flag = Time.timeSinceLevelLoad * 2f % 2f < 1.33f;
				_indicatorLight.SetEmissionColor(flag ? _indicatorCriticalColor : Color.black);
			}
		}
		else if (_lightActive)
		{
			_indicatorLight.SetEmissionColor(Color.black);
			_lightActive = false;
		}
	}
}
