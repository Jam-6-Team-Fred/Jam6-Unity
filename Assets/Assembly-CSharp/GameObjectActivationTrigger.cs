using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class GameObjectActivationTrigger : MonoBehaviour
{
	[SerializeField]
	private bool _checkForTag;

	[SerializeField]
	private string _tag = "";

	[SerializeField]
	private GameObject[] _gameObjects = new GameObject[1];

	[SerializeField]
	private bool _fireOnEnter = true;

	[SerializeField]
	private bool _activeOnEnter = true;

	[SerializeField]
	private bool _fireOnExit;

	[SerializeField]
	private bool _activeOnExit;

	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (!_fireOnEnter || (_checkForTag && !hitObj.CompareTag(_tag)))
		{
			return;
		}
		for (int i = 0; i < _gameObjects.Length; i++)
		{
			if (_gameObjects[i] != null)
			{
				_gameObjects[i].SetActive(_activeOnEnter);
			}
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (!_fireOnExit || (_checkForTag && !hitObj.CompareTag(_tag)))
		{
			return;
		}
		for (int i = 0; i < _gameObjects.Length; i++)
		{
			if (_gameObjects[i] != null)
			{
				_gameObjects[i].SetActive(_activeOnExit);
			}
		}
	}
}
