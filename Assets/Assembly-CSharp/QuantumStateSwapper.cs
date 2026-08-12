using UnityEngine;

[RequireComponent(typeof(QuantumObject))]
public class QuantumStateSwapper : MonoBehaviour
{
	[SerializeField]
	private bool _ignoreFailedCollapses;

	private QuantumObject _quantumObject;

	private QuantumState[] _states;

	private int[] _probabilities;

	private void Awake()
	{
		_states = GetComponentsInChildren<QuantumState>();
		_probabilities = new int[_states.Length];
		_quantumObject = GetComponent<QuantumObject>();
		_quantumObject.OnPostCollapse += OnPostCollapse;
	}

	private void OnDestroy()
	{
		_quantumObject.OnPostCollapse -= OnPostCollapse;
	}

	private void OnPostCollapse(QuantumObject quantumObject, bool collapsed)
	{
		if (collapsed || !_ignoreFailedCollapses)
		{
			int num = 0;
			for (int i = 0; i < _states.Length; i++)
			{
				_probabilities[i] = _states[i].GetProbability();
				num += _probabilities[i];
			}
			int num2 = Random.Range(0, num);
			int num3 = 0;
			int num4 = 0;
			for (int j = 0; j < _states.Length; j++)
			{
				num3 = num4;
				num4 += _probabilities[j];
				_states[j].gameObject.SetActive(_probabilities[j] > 0 && num2 >= num3 && num2 < num4);
			}
		}
	}
}
