using UnityEngine;

public class BlackHoleForgeMovement : MonoBehaviour
{
	private float _movingTimer;

	[SerializeField]
	private float _movingLength = 5f;

	[SerializeField]
	private GameObject _lowerPosition;

	[SerializeField]
	private GameObject _midPosition;

	[SerializeField]
	private GameObject _highPosition;

	[SerializeField]
	private float _lowerPositionOffset;

	[SerializeField]
	private float _midPositionOffset;

	[SerializeField]
	private float _highPositionOffset;

	[SerializeField]
	private AnimationCurve _movingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private NomaiInterfaceSlot[] _lowerSwitches = new NomaiInterfaceSlot[0];

	[SerializeField]
	private NomaiInterfaceSlot[] _middleSwitches = new NomaiInterfaceSlot[0];

	[SerializeField]
	private NomaiInterfaceSlot[] _higherSwitches = new NomaiInterfaceSlot[0];

	[SerializeField]
	private float _lowerRotation;

	[SerializeField]
	private float _midRotation;

	[SerializeField]
	private float _highRotation;

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
		for (int j = 0; j < _middleSwitches.Length; j++)
		{
			if (_middleSwitches[j] != null)
			{
				_middleSwitches[j].OnSlotActivated += OnMovingSwitchToMiddleTriggered;
			}
		}
		for (int k = 0; k < _higherSwitches.Length; k++)
		{
			if (_higherSwitches[k] != null)
			{
				_higherSwitches[k].OnSlotActivated += OnMovingSwitchToHigherTriggered;
			}
		}
	}

	private void FixedUpdate()
	{
		if (_movingTimer > 0f)
		{
			_movingTimer = Mathf.Clamp01(_movingTimer - Time.deltaTime / _movingLength);
			Vector3 vector = Vector3.zero;
			float num = 0f;
			switch (_currentLevel)
			{
			case 0:
				vector = _lowerPosition.transform.position + new Vector3(0f, 1f, 0f) * _lowerPositionOffset;
				num = _lowerRotation;
				break;
			case 1:
				vector = _midPosition.transform.position + new Vector3(0f, 1f, 0f) * _midPositionOffset;
				num = _midRotation;
				break;
			case 2:
				vector = _highPosition.transform.position + new Vector3(0f, 1f, 0f) * _highPositionOffset;
				num = _highRotation;
				break;
			}
			switch (_targetLevel)
			{
			case 0:
				vector += (_lowerPosition.transform.position + new Vector3(0f, 1f, 0f) * _lowerPositionOffset - vector) * _movingCurve.Evaluate(1f - _movingTimer);
				num += (_lowerRotation - num) * _rotationCurve.Evaluate(1f - _movingTimer);
				break;
			case 1:
				vector += (_midPosition.transform.position + new Vector3(0f, 1f, 0f) * _midPositionOffset - vector) * _movingCurve.Evaluate(1f - _movingTimer);
				num += (_midRotation - num) * _rotationCurve.Evaluate(1f - _movingTimer);
				break;
			case 2:
				vector += (_highPosition.transform.position + new Vector3(0f, 1f, 0f) * _highPositionOffset - vector) * _movingCurve.Evaluate(1f - _movingTimer);
				num += (_highRotation - num) * _rotationCurve.Evaluate(1f - _movingTimer);
				break;
			}
			base.transform.position = vector;
			Quaternion rotation = Quaternion.AngleAxis(num, new Vector3(0f, 1f, 0f)) * _lowerPosition.transform.rotation;
			base.transform.rotation = rotation;
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

	private void OnMovingSwitchToMiddleTriggered(NomaiInterfaceSlot slot)
	{
		if (_movingTimer <= 0f && _currentLevel != 1)
		{
			_targetLevel = 1;
			_movingTimer = 1f;
		}
	}

	private void OnMovingSwitchToHigherTriggered(NomaiInterfaceSlot slot)
	{
		if (_movingTimer <= 0f && _currentLevel == 1)
		{
			_targetLevel = 2;
			_movingTimer = 1f;
		}
	}
}
