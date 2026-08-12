using UnityEngine;

public class MultiStateQuantumObject : QuantumObject
{
	[SerializeField]
	private QuantumState[] _states;

	[SerializeField]
	private int _initialState = -1;

	[SerializeField]
	private bool _sequential;

	[SerializeField]
	private bool _loop;

	[SerializeField]
	private bool _reverse;

	[SerializeField]
	private MultiStateQuantumObject[] _prerequisiteObjects;

	[SerializeField]
	private GameObject _prerequisiteRoot;

	private int _stateIndex = -1;

	private int[] _probabilities;

	private void OnValidate()
	{
		if (!_sequential)
		{
			_loop = (_reverse = false);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_probabilities = new int[_states.Length];
		if (_prerequisiteRoot != null)
		{
			MultiStateQuantumObject[] componentsInChildren = _prerequisiteRoot.GetComponentsInChildren<MultiStateQuantumObject>();
			MultiStateQuantumObject[] array = new MultiStateQuantumObject[_prerequisiteObjects.Length + componentsInChildren.Length];
			_prerequisiteObjects.CopyTo(array, 0);
			componentsInChildren.CopyTo(array, _prerequisiteObjects.Length);
			_prerequisiteObjects = array;
		}
	}

	protected override void Start()
	{
		for (int i = 0; i < _states.Length; i++)
		{
			_states[i].SetVisible(visible: false);
		}
		base.Start();
	}

	public bool HasCollapsed()
	{
		return _stateIndex != -1;
	}

	public void DebugForceToFinalQuantumState()
	{
		if (_sequential)
		{
			int stateIndex = _stateIndex;
			_stateIndex = _states.Length - 1;
			if (stateIndex != _stateIndex && stateIndex >= 0 && stateIndex < _states.Length)
			{
				_states[stateIndex].SetVisible(visible: false);
			}
			_states[_stateIndex].SetVisible(visible: true);
		}
		else
		{
			Debug.LogError("Cannot collapse to 'final' state if quantum object is non-sequential.");
		}
	}

	protected override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
	{
		for (int i = 0; i < _prerequisiteObjects.Length; i++)
		{
			if (!_prerequisiteObjects[i].HasCollapsed())
			{
				return false;
			}
		}
		int stateIndex = _stateIndex;
		if (_stateIndex == -1 && _initialState != -1)
		{
			_stateIndex = _initialState;
		}
		else if (_sequential)
		{
			_stateIndex = (_reverse ? (_stateIndex - 1) : (_stateIndex + 1));
			if (_loop)
			{
				if (_stateIndex < 0)
				{
					_stateIndex = _states.Length - 1;
				}
				else if (_stateIndex > _states.Length - 1)
				{
					_stateIndex = 0;
				}
			}
			else
			{
				_stateIndex = Mathf.Clamp(_stateIndex, 0, _states.Length - 1);
			}
		}
		else
		{
			int num = 0;
			for (int j = 0; j < _states.Length; j++)
			{
				if (j != stateIndex)
				{
					_probabilities[j] = _states[j].GetProbability();
					num += _probabilities[j];
				}
			}
			int num2 = Random.Range(0, num);
			int num3 = 0;
			int num4 = 0;
			for (int k = 0; k < _states.Length; k++)
			{
				if (k != stateIndex)
				{
					num3 = num4;
					num4 += _probabilities[k];
					if (_probabilities[k] > 0 && num2 >= num3 && num2 < num4)
					{
						_stateIndex = k;
						break;
					}
				}
			}
		}
		if (stateIndex != _stateIndex && stateIndex >= 0 && stateIndex < _states.Length)
		{
			_states[stateIndex].SetVisible(visible: false);
		}
		_states[_stateIndex].SetVisible(visible: true);
		if (_sequential && !_loop && _stateIndex == _states.Length - 1)
		{
			SetActivation(active: false);
		}
		return true;
	}
}
