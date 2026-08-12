using UnityEngine;

public class PlayerDistanceTrigger : DistanceTrigger
{
	protected Transform _thisTransform;

	protected Transform _playerTransform;

	protected bool _triggered;

	protected override void Awake()
	{
		base.Awake();
		_triggered = false;
		_thisTransform = base.transform;
		base.enabled = false;
	}

	private void Update()
	{
		if (_triggered)
		{
			if ((_playerTransform.position - _thisTransform.position).magnitude > _triggerRadius)
			{
				TriggerExit();
			}
		}
		else if (!_triggered && (_playerTransform.position - _thisTransform.position).magnitude < _triggerRadius)
		{
			TriggerEnter();
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			if (_playerTransform == null)
			{
				_playerTransform = Locator.GetPlayerTransform();
			}
			base.enabled = true;
		}
		else
		{
			base.enabled = false;
		}
	}

	public override void TriggerEnter()
	{
		if (OnTriggerEnter != null)
		{
			OnTriggerEnter.Invoke();
		}
		_triggered = true;
	}

	public override void TriggerExit()
	{
		if (OnTriggerExit != null)
		{
			OnTriggerExit.Invoke();
		}
		_triggered = false;
	}
}
