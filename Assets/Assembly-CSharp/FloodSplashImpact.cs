using UnityEngine;

public class FloodSplashImpact : MonoBehaviour
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[SerializeField]
	private OWRenderer _renderer;

	[Space]
	[SerializeField]
	private float _lifetime = 1f;

	[SerializeField]
	private float _delay;

	[SerializeField]
	private AnimationCurve _yOffsetOverLife = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _cutoffOverLife = AnimationCurve.Linear(0f, 0.5f, 1f, 1f);

	private static int s_propID_MainTex_ST = Shader.PropertyToID("_MainTex_ST");

	private static int s_propID_Cutoff = Shader.PropertyToID("_Cutoff");

	private bool _playing;

	private float _startTime;

	private Vector4 _baseScaleOffset;

	private void Reset()
	{
		_sector = GetComponentInParent<Sector>();
	}

	private void Awake()
	{
		if (_sector == null)
		{
			_sector = GetComponentInParent<Sector>();
		}
		_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
		_renderer.SetActivation(active: false);
		_baseScaleOffset = _renderer.sharedMaterial.GetVector(s_propID_MainTex_ST);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
	}

	private void OnFloodImpact()
	{
		if (_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			_startTime = Time.time + _delay;
			base.enabled = true;
		}
	}

	private void Update()
	{
		float time = Time.time;
		if (!_playing && time >= _startTime)
		{
			_renderer.SetActivation(active: true);
			_playing = true;
		}
		if (_playing)
		{
			float num = Mathf.InverseLerp(_startTime, _startTime + _lifetime, time);
			_renderer.SetMaterialProperty(s_propID_MainTex_ST, _baseScaleOffset + new Vector4(0f, 0f, 0f, _yOffsetOverLife.Evaluate(num)));
			_renderer.SetMaterialProperty(s_propID_Cutoff, _cutoffOverLife.Evaluate(num));
			if (num >= 1f)
			{
				_renderer.SetActivation(active: false);
				_playing = false;
				base.enabled = false;
			}
		}
	}
}
