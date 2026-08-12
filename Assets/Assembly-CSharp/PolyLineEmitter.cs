using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PolyLineEmitter : MonoBehaviour
{
	private ParticleSystem _particleSystem;

	[SerializeField]
	private Vector3[] _vertices = new Vector3[2]
	{
		Vector3.left,
		Vector3.right
	};

	[SerializeField]
	private bool _looped;

	[SerializeField]
	private int _burstCountMin = 30;

	[SerializeField]
	private int _burstCountMax = 40;

	[SerializeField]
	private float _burstLength;

	private float[] _segmentLengths;

	private float _totalLength;

	private float[] _normalizedDists;

	private int _emitCount;

	private float _emitRate;

	private float _emitAccumulator;

	private int _numEmitted;

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		CacheLineData();
		base.enabled = false;
	}

	private void CacheLineData()
	{
		int num = (_looped ? _vertices.Length : Mathf.Max(_vertices.Length - 1, 0));
		_segmentLengths = new float[num];
		_totalLength = 0f;
		_normalizedDists = new float[_vertices.Length];
		if (num <= 0)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			if (_looped && i == num - 1)
			{
				_segmentLengths[i] = Vector3.Distance(_vertices[i], _vertices[0]);
			}
			else
			{
				_segmentLengths[i] = Vector3.Distance(_vertices[i], _vertices[i + 1]);
			}
			_totalLength += _segmentLengths[i];
		}
		float num2 = 0f;
		for (int j = 0; j < _vertices.Length; j++)
		{
			_normalizedDists[j] = num2 / _totalLength;
			if (j < _segmentLengths.Length)
			{
				num2 += _segmentLengths[j];
			}
		}
	}

	public Vector3 GetPointNormalizedDistanceAlongLine(float d)
	{
		if (_vertices.Length == 0)
		{
			return base.transform.position;
		}
		if (_vertices.Length == 1)
		{
			return _vertices[0];
		}
		int num = _vertices.Length - 1;
		for (int i = 0; i < num; i++)
		{
			if (d < _normalizedDists[i + 1])
			{
				return Vector3.Lerp(_vertices[i], _vertices[i + 1], Mathf.InverseLerp(_normalizedDists[i], _normalizedDists[i + 1], d));
			}
		}
		if (_looped)
		{
			return Vector3.Lerp(_vertices[num], _vertices[0], Mathf.InverseLerp(_normalizedDists[num], 1f, d));
		}
		return _vertices[num];
	}

	public Vector3 GetPointDistanceAlongLine(float d)
	{
		return GetPointNormalizedDistanceAlongLine(d / _totalLength);
	}

	public void Emit(int count)
	{
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		emitParams.applyShapeToPosition = true;
		for (int i = 0; i < count; i++)
		{
			emitParams.position = GetPointNormalizedDistanceAlongLine(Random.value);
			_particleSystem.Emit(emitParams, 1);
		}
	}

	public void Play()
	{
		if (_burstLength <= 0f)
		{
			Emit(Random.Range(_burstCountMin, _burstCountMax));
			return;
		}
		_emitCount = Random.Range(_burstCountMin, _burstCountMax);
		_emitRate = (float)_emitCount / _burstLength;
		_emitAccumulator = 0f;
		_numEmitted = 0;
	}

	private void Update()
	{
		if (_numEmitted >= _emitCount)
		{
			base.enabled = false;
			return;
		}
		_emitAccumulator += _emitRate * Time.deltaTime;
		int num = Mathf.FloorToInt(_emitAccumulator);
		if (num > 0)
		{
			if (_numEmitted + num > _emitCount)
			{
				num = _emitCount - _numEmitted;
			}
			Emit(num);
			_numEmitted += num;
			_emitAccumulator -= num;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_vertices.Length >= 2)
		{
			Gizmos.color = Color.red;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			for (int i = 0; i < _vertices.Length - 1; i++)
			{
				Gizmos.DrawLine(_vertices[i], _vertices[i + 1]);
			}
			if (_looped)
			{
				Gizmos.DrawLine(_vertices[_vertices.Length - 1], _vertices[0]);
			}
		}
	}
}
