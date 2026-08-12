using UnityEngine;

public class SolarFlareEmitter : MonoBehaviour
{
	private SolarFlareController[] _streamers;

	private SolarFlareController[] _loops;

	private SolarFlareController[] _domes;

	[SerializeField]
	private GameObject _streamerPrefab;

	[SerializeField]
	private GameObject _loopPrefab;

	[SerializeField]
	private GameObject _domePrefab;

	[Space]
	[SerializeField]
	private float _lifeLength = 15f;

	[SerializeField]
	private Color _tint = Color.white;

	[Space]
	[SerializeField]
	private float _minTimeBetweenFlares = 5f;

	[SerializeField]
	private float _maxTimeBetweenFlares = 30f;

	[SerializeField]
	private AnimationCurve _flareFrequencyTimeloopScale = AnimationCurve.EaseInOut(0f, 1f, 20f, 5f);

	[Space]
	[SerializeField]
	private int _minStreamersPerFlare = 1;

	[SerializeField]
	private int _maxStreamersPerFlare = 3;

	[Space]
	[SerializeField]
	private float _startScale = 0.01f;

	[SerializeField]
	private float _endScale = 0.1f;

	private float _flareTimer;

	public GameObject streamerPrefab => _streamerPrefab;

	public GameObject loopPrefab => _loopPrefab;

	public GameObject domePrefab => _domePrefab;

	public float lifeLength
	{
		get
		{
			return _lifeLength;
		}
		set
		{
			_lifeLength = value;
		}
	}

	public Color tint
	{
		get
		{
			return _tint;
		}
		set
		{
			_tint = value;
		}
	}

	private void Awake()
	{
		_streamers = new SolarFlareController[30];
		_loops = new SolarFlareController[10];
		_domes = new SolarFlareController[10];
		for (int i = 0; i < _streamers.Length; i++)
		{
			_streamers[i] = Object.Instantiate(_streamerPrefab, base.transform).GetComponent<SolarFlareController>();
		}
		for (int j = 0; j < _loops.Length; j++)
		{
			_loops[j] = Object.Instantiate(_loopPrefab, base.transform).GetComponent<SolarFlareController>();
		}
		for (int k = 0; k < _domes.Length; k++)
		{
			_domes[k] = Object.Instantiate(_domePrefab, base.transform).GetComponent<SolarFlareController>();
		}
	}

	public void Emit(Vector3 localPosition)
	{
		Vector3 normalized = localPosition.normalized;
		Quaternion quaternion = Quaternion.LookRotation(Vector3.Cross((Mathf.Abs(Vector3.Dot(localPosition, base.transform.up)) > 0.999f) ? base.transform.right : base.transform.up, normalized), normalized);
		int num = Random.Range(_minStreamersPerFlare, _maxStreamersPerFlare);
		for (int i = 0; i < _streamers.Length; i++)
		{
			if (!_streamers[i].enabled)
			{
				Quaternion quaternion2 = Quaternion.Lerp(quaternion, Random.rotation, 0.025f);
				Vector3 localPosition2 = quaternion2 * Vector3.up;
				_streamers[i].Spawn(localPosition2, quaternion2, _startScale, _endScale, _lifeLength, _tint);
				num--;
				if (num == 0)
				{
					break;
				}
			}
		}
		for (int j = 0; j < _loops.Length; j++)
		{
			if (!_loops[j].enabled)
			{
				Quaternion quaternion3 = Quaternion.Lerp(quaternion, Random.rotation, 0.025f);
				Vector3 localPosition3 = quaternion3 * Vector3.up;
				_loops[j].Spawn(localPosition3, quaternion3, _startScale, _endScale, _lifeLength, _tint);
				break;
			}
		}
		for (int k = 0; k < _domes.Length; k++)
		{
			if (!_domes[k].enabled)
			{
				_domes[k].Spawn(normalized, quaternion, _startScale, _endScale, _lifeLength, _tint);
				break;
			}
		}
	}

	public void Clear()
	{
		for (int i = 0; i < _streamers.Length; i++)
		{
			_streamers[i].Despawn();
		}
		for (int j = 0; j < _loops.Length; j++)
		{
			_loops[j].Despawn();
		}
		for (int k = 0; k < _domes.Length; k++)
		{
			_domes[k].Despawn();
		}
	}

	private void Update()
	{
		_flareTimer -= Time.deltaTime;
		if (_flareTimer <= 0f)
		{
			Emit(Random.onUnitSphere);
			_flareTimer = Random.Range(_minTimeBetweenFlares, _maxTimeBetweenFlares) / _flareFrequencyTimeloopScale.Evaluate(TimeLoop.GetMinutesElapsed());
		}
	}

	public void SetRenderingEnabled(bool enable)
	{
		for (int i = 0; i < _streamers.Length; i++)
		{
			_streamers[i].SetRenderingEnabled(enable);
		}
		for (int j = 0; j < _loops.Length; j++)
		{
			_loops[j].SetRenderingEnabled(enable);
		}
		for (int k = 0; k < _domes.Length; k++)
		{
			_domes[k].SetRenderingEnabled(enable);
		}
		base.enabled = enable;
	}
}
