using UnityEngine;

public class SetActiveOnAwake : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _targets = new GameObject[0];

	[SerializeField]
	private bool _active;

	private void Awake()
	{
		for (int i = 0; i < _targets.Length; i++)
		{
			_targets[i].SetActive(_active);
		}
	}
}
