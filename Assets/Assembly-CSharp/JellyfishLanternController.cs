using UnityEngine;

public class JellyfishLanternController : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _trigger;

	private OWLight[] _lights;

	private Vector3 _startPos;

	private Vector3 _targetPos;

	private float _triggerTime;

	private void Awake()
	{
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		_targetPos = base.transform.localPosition;
		_startPos = new Vector3(_targetPos.x, -0.5f, _targetPos.y);
		base.transform.localPosition = _startPos;
	}

	private void Start()
	{
		_lights = GetComponentsInChildren<OWLight>();
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].SetIntensity(0f);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void FixedUpdate()
	{
		float t = Mathf.InverseLerp(_triggerTime, _triggerTime + 5f, Time.time);
		t = Mathf.SmoothStep(0f, 1f, t);
		base.transform.localPosition = Vector3.Lerp(_startPos, _targetPos, t);
		if (t >= 1f)
		{
			base.enabled = false;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			for (int i = 0; i < _lights.Length; i++)
			{
				_lights[i].FadeTo(1f, 5f);
			}
			_triggerTime = Time.time;
			_trigger.OnEntry -= OnEntry;
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		hitObj.CompareTag("PlayerDetector");
	}
}
