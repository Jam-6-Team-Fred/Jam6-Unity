using UnityEngine;

public class EyeStateActivationController : MonoBehaviour
{
	[SerializeField]
	private GameObject _object;

	[SerializeField]
	private EyeState[] _activeStates;

	private void Awake()
	{
		if (_object == null)
		{
			Debug.LogError("Eye state object is NULL!");
			Debug.Break();
		}
		GlobalMessenger<EyeState>.AddListener("EyeStateChanged", OnEyeStateChanged);
	}

	private void OnDestroy()
	{
		GlobalMessenger<EyeState>.RemoveListener("EyeStateChanged", OnEyeStateChanged);
	}

	private void OnEyeStateChanged(EyeState state)
	{
		bool active = false;
		for (int i = 0; i < _activeStates.Length; i++)
		{
			if (state == _activeStates[i])
			{
				active = true;
				break;
			}
		}
		_object.SetActive(active);
	}
}
