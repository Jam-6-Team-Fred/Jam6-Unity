using UnityEngine;

public class IslandDroppingCode : MonoBehaviour
{
	[SerializeField]
	private OWRigidbody _island;

	[SerializeField]
	private GameObject _islandVisuals;

	[SerializeField]
	private int _downSpeed = 10;

	private bool _firstUpdate = true;

	private bool _secondUpdate = true;

	private void Awake()
	{
	}

	private void FixedUpdate()
	{
		if (_firstUpdate)
		{
			_islandVisuals.SetActive(value: false);
			_firstUpdate = false;
		}
		else if (_secondUpdate)
		{
			_island.Suspend();
			_secondUpdate = false;
		}
		if (Input.GetKeyDown(KeyCode.K))
		{
			_island.Unsuspend();
			_island.AddAcceleration(-_islandVisuals.transform.up * _downSpeed);
			_islandVisuals.SetActive(value: true);
		}
	}
}
