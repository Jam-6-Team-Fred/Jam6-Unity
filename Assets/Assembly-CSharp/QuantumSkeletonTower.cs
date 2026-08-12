using UnityEngine;

public class QuantumSkeletonTower : QuantumObject
{
	[SerializeField]
	private GameObject[] _towerSkeletons;

	[SerializeField]
	private GameObject[] _towerTrackerObjects;

	[SerializeField]
	private VisibilityObject[] _pointingSkeletons;

	[SerializeField]
	private EyeShuttleController _shuttleController;

	[SerializeField]
	private float _minPlayerOffset;

	[SerializeField]
	private float _maxPlayerOffset;

	[SerializeField]
	private bool _drawOffsets;

	private int _index;

	private bool _waitForPlayerToLookAtTower;

	private bool _waitForFlicker;

	private bool _flickering;

	private float _startFlickerTime;

	private float _flickerOutTime;

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < _towerSkeletons.Length; i++)
		{
			_towerSkeletons[i].SetActive(value: false);
		}
		for (int j = 1; j < _towerTrackerObjects.Length; j++)
		{
			_towerTrackerObjects[j].SetActive(value: false);
		}
	}

	protected override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
	{
		if (_waitForPlayerToLookAtTower)
		{
			return false;
		}
		if (_index < _towerSkeletons.Length)
		{
			bool flag = false;
			for (int i = 0; i < _pointingSkeletons.Length; i++)
			{
				if (_pointingSkeletons[i].gameObject.activeInHierarchy && (!_pointingSkeletons[i].IsVisible() || !_pointingSkeletons[i].IsIlluminated()))
				{
					_pointingSkeletons[i].gameObject.SetActive(value: false);
					flag = true;
					break;
				}
			}
			if (flag)
			{
				_towerSkeletons[_index].SetActive(value: true);
				_index++;
				_waitForPlayerToLookAtTower = true;
				return true;
			}
		}
		return false;
	}

	protected override void Update()
	{
		base.Update();
		if (_waitForPlayerToLookAtTower && IsVisible() && IsIlluminated())
		{
			_waitForPlayerToLookAtTower = false;
			if (_index < _towerSkeletons.Length)
			{
				_towerTrackerObjects[_index].SetActive(value: true);
			}
			else
			{
				_waitForFlicker = true;
				_startFlickerTime = Time.time + 0.5f;
			}
		}
		if (_waitForFlicker && Time.time > _startFlickerTime)
		{
			GlobalMessenger<float, float>.FireEvent("FlickerOffAndOn", 0.5f, 0.5f);
			_waitForFlicker = false;
			_flickerOutTime = Time.time + 0.5f;
			_flickering = true;
		}
		else if (_flickering && Time.time > _flickerOutTime)
		{
			_shuttleController.SpawnShuttle();
			base.gameObject.SetActive(value: false);
			Vector3 position = Locator.GetPlayerTransform().position;
			Vector3 vector = position - base.transform.position;
			vector.y = 0f;
			float magnitude = vector.magnitude;
			if (magnitude < _maxPlayerOffset)
			{
				float num = Mathf.Clamp(magnitude + 8f, _minPlayerOffset, _maxPlayerOffset);
				Vector3 vector2 = new Vector3(base.transform.position.x, position.y, base.transform.position.z);
				Locator.GetPlayerBody().SetPosition(vector2 + vector.normalized * num);
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (_drawOffsets)
		{
			Gizmos.color = Color.yellow;
			OWGizmos.DrawWireCircle(base.transform.position, base.transform.up, _minPlayerOffset);
			OWGizmos.DrawWireCircle(base.transform.position, base.transform.up, _maxPlayerOffset);
		}
	}
}
