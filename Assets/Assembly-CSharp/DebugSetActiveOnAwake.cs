using UnityEngine;

public class DebugSetActiveOnAwake : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _targets = new GameObject[0];

	private void Awake()
	{
		for (int i = 0; i < _targets.Length; i++)
		{
			if (_targets != null)
			{
				_targets[i].SetActive(value: true);
			}
		}
	}
}
