using System.Collections.Generic;
using UnityEngine;

public class ElectricalSystem : MonoBehaviour
{
	private class SystemEvent
	{
		public enum Type
		{
			None = 0,
			PowerOn = 1,
			PowerOff = 2,
			Disrupt = 3
		}

		public Type type;

		public float timestamp;

		public float data;

		public SystemEvent(Type eventType, float eventTimestamp)
		{
			type = eventType;
			timestamp = eventTimestamp;
			data = 0f;
		}

		public SystemEvent(Type eventType, float eventTimestamp, float eventData)
		{
			type = eventType;
			timestamp = eventTimestamp;
			data = eventData;
		}
	}

	[SerializeField]
	private ElectricalSystem[] _connectedSystems = new ElectricalSystem[0];

	[SerializeField]
	private ElectricalComponent[] _connectedComponents = new ElectricalComponent[0];

	[SerializeField]
	private float _systemDelay;

	private List<SystemEvent> _events;

	private bool _powered;

	private float _disruptionTimer;

	private void Awake()
	{
		_events = new List<SystemEvent>(16);
		_powered = false;
	}

	private void Update()
	{
		if (_events.Count > 0 && Time.timeSinceLevelLoad >= _events[0].timestamp + _systemDelay)
		{
			ProcessEvent(_events[0]);
			_events.RemoveAt(0);
		}
		if (_disruptionTimer > 0f)
		{
			_disruptionTimer -= Time.deltaTime;
			for (int i = 0; i < _connectedComponents.Length; i++)
			{
				bool flag = _powered && Random.value > 0.5f;
				if (_connectedComponents[i] != null)
				{
					_connectedComponents[i].SetPowered((_disruptionTimer > 0f) ? flag : _powered);
				}
			}
		}
		if (_events.Count == 0 && _disruptionTimer <= 0f)
		{
			base.enabled = false;
		}
	}

	private void ProcessEvent(SystemEvent systemEvent)
	{
		switch (systemEvent.type)
		{
		case SystemEvent.Type.PowerOn:
		case SystemEvent.Type.PowerOff:
		{
			_powered = systemEvent.type == SystemEvent.Type.PowerOn;
			for (int j = 0; j < _connectedSystems.Length; j++)
			{
				if (_connectedSystems[j] != null)
				{
					_connectedSystems[j].SetPowered(_powered);
				}
			}
			for (int k = 0; k < _connectedComponents.Length; k++)
			{
				if (_connectedComponents[k] != null)
				{
					_connectedComponents[k].SetPowered(_powered);
				}
			}
			break;
		}
		case SystemEvent.Type.Disrupt:
		{
			_disruptionTimer = systemEvent.data;
			for (int i = 0; i < _connectedSystems.Length; i++)
			{
				if (_connectedSystems[i] != null)
				{
					_connectedSystems[i].Disrupt(systemEvent.data);
				}
			}
			break;
		}
		}
	}

	public bool IsPowered()
	{
		return _powered;
	}

	public bool IsDisrupted()
	{
		return _disruptionTimer > 0f;
	}

	public void SetPowered(bool powered)
	{
		SystemEvent systemEvent = new SystemEvent(powered ? SystemEvent.Type.PowerOn : SystemEvent.Type.PowerOff, Time.timeSinceLevelLoad);
		if (_systemDelay <= 0f)
		{
			ProcessEvent(systemEvent);
			return;
		}
		_events.Add(systemEvent);
		base.enabled = true;
	}

	public void Disrupt(float disruptionLength)
	{
		SystemEvent systemEvent = new SystemEvent(SystemEvent.Type.Disrupt, Time.timeSinceLevelLoad, disruptionLength);
		if (_systemDelay <= 0f)
		{
			ProcessEvent(systemEvent);
			return;
		}
		_events.Add(systemEvent);
		base.enabled = true;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		for (int i = 0; i < _connectedSystems.Length; i++)
		{
			if (_connectedSystems[i] != null)
			{
				Gizmos.DrawLine(base.transform.position, _connectedSystems[i].transform.position);
			}
		}
		Gizmos.color = Color.red;
		for (int j = 0; j < _connectedComponents.Length; j++)
		{
			if (_connectedComponents[j] != null)
			{
				Gizmos.DrawLine(base.transform.position, _connectedComponents[j].transform.position);
			}
		}
	}
}
