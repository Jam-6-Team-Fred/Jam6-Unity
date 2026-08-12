using UnityEngine;

public class ProbePathAnimator : MonoBehaviour
{
	[SerializeField]
	private GameObject _eyeHologramPrefab;

	[SerializeField]
	private GameObject _probeHologramPrefab;

	private LineRenderer _lineRenderer;

	private Vector3 _originPos;

	private Vector3 _currentPos = Vector3.zero;

	private Vector3 _targetPos;

	private bool _isEyePath;

	private bool _isLatestPath;

	private float _initTime;

	private float _duration;

	private Transform _probeTransform;

	private void Awake()
	{
		_lineRenderer = GetComponent<LineRenderer>();
	}

	public void Init(Vector3 origin, Vector3 direction, float dist, float duration, bool isEyePath = false, bool isLatestPath = false)
	{
		_currentPos = (_originPos = origin);
		_targetPos = _originPos + direction * dist;
		_isEyePath = isEyePath;
		_isLatestPath = isLatestPath;
		_initTime = Time.time;
		_duration = duration;
		_lineRenderer.SetPosition(0, origin);
		if (_isLatestPath)
		{
			GameObject gameObject = Object.Instantiate(_probeHologramPrefab);
			_probeTransform = gameObject.transform;
			_probeTransform.parent = base.transform.parent;
			_probeTransform.position = base.transform.TransformPoint(_currentPos);
			_probeTransform.forward = base.transform.TransformDirection(direction);
		}
	}

	private void Update()
	{
		float num = Time.time - _initTime;
		float num2 = Mathf.Clamp01(num / _duration);
		_currentPos = Vector3.Lerp(_originPos, _targetPos, num2);
		_lineRenderer.SetPosition(1, _currentPos);
		if (!_isEyePath && !_isLatestPath && num > _duration + 1f)
		{
			Object.Destroy(base.gameObject);
		}
		else if (_isEyePath && num2 == 1f)
		{
			base.enabled = false;
			GameObject obj = Object.Instantiate(_eyeHologramPrefab);
			obj.transform.parent = base.transform.parent;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.position = base.transform.TransformPoint(_currentPos);
		}
		if (_isLatestPath)
		{
			_probeTransform.position = base.transform.TransformPoint((_targetPos - _originPos) * TimeLoop.GetFractionElapsed() * num2 + _originPos);
		}
	}
}
