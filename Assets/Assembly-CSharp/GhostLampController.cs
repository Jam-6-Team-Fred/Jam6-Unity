using UnityEngine;

public class GhostLampController : MonoBehaviour
{
	[SerializeField]
	private OWLight _light;

	[SerializeField]
	private OWTriggerVolume _trigger;

	private OWRenderer[] _lampRenderers;

	private float _fade = 1f;

	private float _targetFade = 1f;

	private bool _ghostNearby;

	private void Awake()
	{
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		_lampRenderers = GetComponentsInChildren<OWRenderer>();
		GlobalMessenger.AddListener("GhostKillPlayer", OnGhostKillPlayer);
		GlobalMessenger.AddListener("TurnOnFlashlight", OnTurnOnFlashlight);
		GlobalMessenger.AddListener("TurnOffFlashlight", OnTurnOffFlashlight);
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
		GlobalMessenger.RemoveListener("GhostKillPlayer", OnGhostKillPlayer);
		GlobalMessenger.RemoveListener("TurnOnFlashlight", OnTurnOnFlashlight);
		GlobalMessenger.RemoveListener("TurnOffFlashlight", OnTurnOffFlashlight);
	}

	private void Update()
	{
		_fade = Mathf.MoveTowards(_fade, _targetFade, Time.deltaTime);
		if (OWMath.ApproxEquals(_fade, _targetFade))
		{
			_fade = _targetFade;
			base.enabled = false;
		}
		for (int i = 0; i < _lampRenderers.Length; i++)
		{
			_lampRenderers[i].SetEmissionColor(_lampRenderers[i].GetOriginalEmissionColor() * _fade);
			_light.SetIntensity(_fade);
		}
	}

	private void CheckFade()
	{
		bool flag = !_ghostNearby && !Locator.GetFlashlight().IsFlashlightOn();
		_targetFade = (flag ? 1f : 0f);
		if (!OWMath.ApproxEquals(_fade, _targetFade))
		{
			base.enabled = true;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("GhostDetector"))
		{
			_ghostNearby = true;
			CheckFade();
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("GhostDetector"))
		{
			_ghostNearby = false;
			CheckFade();
		}
	}

	private void OnTurnOnFlashlight()
	{
		CheckFade();
	}

	private void OnTurnOffFlashlight()
	{
		CheckFade();
	}

	private void OnGhostKillPlayer()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
		_targetFade = 0f;
		_light.FadeTo(_targetFade, 1f);
		base.enabled = true;
	}
}
