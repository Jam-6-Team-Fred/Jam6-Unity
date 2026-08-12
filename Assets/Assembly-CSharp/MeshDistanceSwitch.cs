using UnityEngine;

public class MeshDistanceSwitch : MonoBehaviour
{
	[SerializeField]
	private DistanceTracker _tracker;

	[SerializeField]
	private float _distanceLimit;

	[SerializeField]
	private bool _switchOnWhenUnderLimit;

	[SerializeField]
	private MeshCollider _collider;

	[SerializeField]
	private MeshRenderer _renderer;

	private bool _meshEnabled;

	private float _distanceLimitSqr;

	private void Start()
	{
		GlobalMessenger<int>.AddListener("StartOfTimeLoop", OnStartOfTimeLoop);
		if (_collider == null && GetComponent<MeshCollider>() != null)
		{
			_collider = GetComponent<MeshCollider>();
		}
		if (_renderer == null && GetComponent<MeshRenderer>() != null)
		{
			_renderer = GetComponent<MeshRenderer>();
		}
	}

	private void OnStartOfTimeLoop(int loop)
	{
		Init();
		_distanceLimitSqr = _distanceLimit * _distanceLimit;
	}

	private void Init()
	{
		float vectorSquareMagnitude = _tracker.GetVectorSquareMagnitude();
		if (_switchOnWhenUnderLimit)
		{
			if (vectorSquareMagnitude < _distanceLimitSqr)
			{
				if (_collider != null)
				{
					_collider.enabled = true;
				}
				if (_renderer != null)
				{
					_renderer.enabled = true;
				}
				_meshEnabled = true;
			}
			else if (vectorSquareMagnitude >= _distanceLimitSqr)
			{
				if (_collider != null)
				{
					_collider.enabled = false;
				}
				if (_renderer != null)
				{
					_renderer.enabled = false;
				}
				_meshEnabled = false;
			}
		}
		else if (vectorSquareMagnitude >= _distanceLimitSqr)
		{
			if (_collider != null)
			{
				_collider.enabled = true;
			}
			if (_renderer != null)
			{
				_renderer.enabled = true;
			}
			_meshEnabled = true;
		}
		else if (vectorSquareMagnitude < _distanceLimitSqr)
		{
			if (_collider != null)
			{
				_collider.enabled = false;
			}
			if (_renderer != null)
			{
				_renderer.enabled = false;
			}
			_meshEnabled = false;
		}
	}

	private void FixedUpdate()
	{
		RefreshDistances();
	}

	private void RefreshDistances()
	{
		float vectorMagnitude = _tracker.GetVectorMagnitude();
		if (_switchOnWhenUnderLimit)
		{
			if (vectorMagnitude < _distanceLimit && !_meshEnabled)
			{
				if (_collider != null)
				{
					_collider.enabled = true;
				}
				if (_renderer != null)
				{
					_renderer.enabled = true;
				}
				_meshEnabled = true;
			}
			else if (vectorMagnitude >= _distanceLimit && _meshEnabled)
			{
				if (_collider != null)
				{
					_collider.enabled = false;
				}
				if (_renderer != null)
				{
					_renderer.enabled = false;
				}
				_meshEnabled = false;
			}
		}
		else if (vectorMagnitude >= _distanceLimit && !_meshEnabled)
		{
			if (_collider != null)
			{
				_collider.enabled = true;
			}
			if (_renderer != null)
			{
				_renderer.enabled = true;
			}
			_meshEnabled = true;
		}
		else if (vectorMagnitude < _distanceLimit && _meshEnabled)
		{
			if (_collider != null)
			{
				_collider.enabled = false;
			}
			if (_renderer != null)
			{
				_renderer.enabled = false;
			}
			_meshEnabled = false;
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger<int>.RemoveListener("StartOfTimeLoop", OnStartOfTimeLoop);
	}
}
