using UnityEngine;

public class LightFlicker2 : SectoredMonoBehaviour
{
	[SerializeField]
	private OWLight2[] _lights;

	[SerializeField]
	private OWEmissiveRenderer[] _renderers;

	[Space(10f)]
	[SerializeField]
	private float _range = 0.1f;

	[SerializeField]
	private float _rate = 0.2f;

	private float _flickerScale = 1f;

	private float _targetFlickerScale;

	private Color _baseEmissionColor;

	private float _startEmissionIntensity;

	private float _targetEmissionIntensity;

	protected override void Awake()
	{
		base.Awake();
		UpdateTargetFlickerScale();
		if (_sector != null)
		{
			base.enabled = false;
		}
	}

	private void OnEnable()
	{
		UpdateManager.Register(this);
	}

	private void OnDisable()
	{
		UpdateManager.Unregister(this);
	}

	public void ManagedUpdate()
	{
		if (Mathf.Abs(_flickerScale - _targetFlickerScale) < 0.01f)
		{
			UpdateTargetFlickerScale();
		}
		_flickerScale = Mathf.Lerp(_flickerScale, _targetFlickerScale, _rate);
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].SetFlickerScale(_flickerScale);
		}
		for (int j = 0; j < _renderers.Length; j++)
		{
			_renderers[j].SetFlickerScale(_flickerScale);
		}
	}

	private void UpdateTargetFlickerScale()
	{
		_targetFlickerScale = 1f + Random.Range(0f - _range, _range);
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}
}
