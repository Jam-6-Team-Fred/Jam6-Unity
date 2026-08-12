using UnityEngine;

public class TimeLoopLightController : MonoBehaviour
{
	[SerializeField]
	private OWLightController _fillLightController;

	[SerializeField]
	private OWLightController[] _lightControllers;

	private bool _lightsOn;

	private float _nextLightTime;

	private float _interval;

	private int _index;

	private void Awake()
	{
		GlobalMessenger<OWRigidbody>.AddListener("EnterTimeLoopCentral", OnEnterTimeLoopCentral);
		GlobalMessenger<OWRigidbody>.AddListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
	}

	private void Start()
	{
		_fillLightController.SetIntensity(0f);
		for (int i = 0; i < _lightControllers.Length; i++)
		{
			_lightControllers[i].SetIntensity(0f);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterTimeLoopCentral", OnEnterTimeLoopCentral);
		GlobalMessenger<OWRigidbody>.RemoveListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
	}

	public void SetLightsOn(bool lightsOn, float interval, float delay, float fillFadeDuration)
	{
		if (_lightsOn != lightsOn)
		{
			_lightsOn = lightsOn;
			_interval = interval;
			_nextLightTime = Time.time + delay;
			_index = 0;
			base.enabled = true;
			_fillLightController.FadeTo(lightsOn ? 1f : 0f, fillFadeDuration);
		}
	}

	private void Update()
	{
		if (Time.time > _nextLightTime)
		{
			_lightControllers[_index].FadeTo(_lightsOn ? 1f : 0f, 1.4f);
			_index++;
			_nextLightTime += _interval;
			if (_index > _lightControllers.Length - 1)
			{
				base.enabled = false;
			}
		}
	}

	private void OnEnterTimeLoopCentral(OWRigidbody body)
	{
		if (body.CompareTag("Player") && TimeLoop.IsTimeLoopEnabled())
		{
			SetLightsOn(lightsOn: true, 0.7f, 1.6f, 6f);
		}
	}

	private void OnExitTimeLoopCentral(OWRigidbody body)
	{
		if (body.CompareTag("Player") && TimeLoop.IsTimeLoopEnabled())
		{
			SetLightsOn(lightsOn: false, 0f, 0f, 0f);
		}
	}
}
