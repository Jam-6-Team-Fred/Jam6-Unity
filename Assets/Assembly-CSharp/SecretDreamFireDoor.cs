using UnityEngine;

public class SecretDreamFireDoor : MonoBehaviour
{
	[SerializeField]
	private SlidingDoor _door;

	[SerializeField]
	private int _secretIndex;

	[SerializeField]
	private LightSensor[] _lightSensors;

	[SerializeField]
	private OWLightController[] _lightControllers;

	[SerializeField]
	private InteractReceiver _interactReceiver;

	private bool[] _lightState;

	private void Awake()
	{
		if (_lightSensors.Length != _lightControllers.Length)
		{
			Debug.LogError("Must have equal number of light sensors and light controllers");
			Debug.Break();
		}
		_lightState = new bool[_lightSensors.Length];
		for (int i = 0; i < _lightSensors.Length; i++)
		{
			_lightSensors[i].OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
			_lightState[i] = false;
		}
		_interactReceiver.OnPressInteract += OnPressInteract;
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _lightSensors.Length; i++)
		{
			_lightSensors[i].OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		}
		_interactReceiver.OnPressInteract -= OnPressInteract;
	}

	private void Update()
	{
	}

	private void OnPressInteract()
	{
		for (int i = 0; i < _lightControllers.Length; i++)
		{
			_lightState[i] = false;
			_lightControllers[i].FadeTo(0f, 1f);
		}
		_door.Close();
	}

	private void OnDetectLight()
	{
		for (int i = 0; i < _lightSensors.Length; i++)
		{
			if (_lightSensors[i].IsIlluminated() && !_lightState[i])
			{
				_lightControllers[i].FadeTo(1f, 1f);
				_lightState[i] = true;
			}
		}
		if (!_lightState[_secretIndex])
		{
			int num = 0;
			for (int j = 0; j < _lightState.Length; j++)
			{
				if (_lightState[j])
				{
					num++;
				}
			}
			if (num == _lightState.Length - 1)
			{
				_door.Open();
			}
		}
		else
		{
			_door.Close();
		}
		_interactReceiver.ResetInteraction();
	}
}
