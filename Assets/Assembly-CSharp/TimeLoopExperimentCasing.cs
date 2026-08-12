using UnityEngine;

public class TimeLoopExperimentCasing : MonoBehaviour
{
	private float _movingTimer;

	[SerializeField]
	private float _movingLength = 5f;

	[SerializeField]
	private GameObject _lowerPosition;

	[SerializeField]
	private GameObject _highPosition;

	[SerializeField]
	private AnimationCurve _movingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private NomaiInterfaceSlot[] _lowerSwitches = new NomaiInterfaceSlot[0];

	[SerializeField]
	private NomaiInterfaceSlot[] _higherSwitches = new NomaiInterfaceSlot[0];

	private int _currentLevel;

	private int _targetLevel;

	private void Awake()
	{
		_movingTimer = 0f;
		_currentLevel = 0;
		_targetLevel = 0;
		for (int i = 0; i < _lowerSwitches.Length; i++)
		{
			if (_lowerSwitches[i] != null)
			{
				_lowerSwitches[i].OnSlotActivated += OnMovingSwitchToLowerTriggered;
			}
		}
		for (int j = 0; j < _higherSwitches.Length; j++)
		{
			if (_higherSwitches[j] != null)
			{
				_higherSwitches[j].OnSlotActivated += OnMovingSwitchToHigherTriggered;
			}
		}
	}

	private void FixedUpdate()
	{
		if (_movingTimer > 0f)
		{
			_movingTimer = Mathf.Clamp01(_movingTimer - Time.deltaTime / _movingLength);
			Vector3 vector = Vector3.zero;
			switch (_currentLevel)
			{
			case 0:
				vector = _lowerPosition.transform.position;
				break;
			case 1:
				vector = _highPosition.transform.position;
				break;
			}
			switch (_targetLevel)
			{
			case 0:
				vector += (_lowerPosition.transform.position - vector) * _movingCurve.Evaluate(1f - _movingTimer);
				break;
			case 1:
				vector += (_highPosition.transform.position - vector) * _movingCurve.Evaluate(1f - _movingTimer);
				break;
			}
			base.transform.position = vector;
			if (_movingTimer <= 0f)
			{
				_currentLevel = _targetLevel;
			}
		}
	}

	private void OnMovingSwitchToLowerTriggered(NomaiInterfaceSlot slot)
	{
		if (_movingTimer <= 0f && _currentLevel == 1)
		{
			_targetLevel = 0;
			_movingTimer = 1f;
		}
	}

	private void OnMovingSwitchToHigherTriggered(NomaiInterfaceSlot slot)
	{
		if (_movingTimer <= 0f && _currentLevel == 0)
		{
			_targetLevel = 1;
			_movingTimer = 1f;
		}
	}
}
