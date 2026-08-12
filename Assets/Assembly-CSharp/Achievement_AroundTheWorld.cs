using UnityEngine;

public class Achievement_AroundTheWorld : MonoBehaviour
{
	private class AroundTheWorldTrigger
	{
		public delegate void AroundTheWorldEvent(int ID);

		private OWTriggerVolume triggerVolume;

		private bool _reached;

		private float _timeReached;

		private float _lastTimeReached;

		private int _ID;

		public event AroundTheWorldEvent OnTriggered;

		public bool reached()
		{
			return _reached;
		}

		public float timeReached()
		{
			return _timeReached;
		}

		public float lastTimeReached()
		{
			return _lastTimeReached;
		}

		public AroundTheWorldTrigger(OWTriggerVolume trigger, int ID)
		{
			triggerVolume = trigger;
			_reached = false;
			triggerVolume.OnEntry += OnEntry;
			_ID = ID;
			_timeReached = -1f;
			_lastTimeReached = -1f;
		}

		public void OnDestroy()
		{
			triggerVolume.OnEntry -= OnEntry;
		}

		private void OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				_lastTimeReached = _timeReached;
				_timeReached = Time.time;
				if (_reached && this.OnTriggered != null)
				{
					this.OnTriggered(_ID);
				}
				_reached = true;
			}
		}
	}

	[SerializeField]
	private OWTriggerVolume[] _triggerVolume = new OWTriggerVolume[0];

	private AroundTheWorldTrigger[] _triggers;

	private void Start()
	{
		_triggers = new AroundTheWorldTrigger[_triggerVolume.Length];
		for (int i = 0; i < _triggerVolume.Length; i++)
		{
			_triggers[i] = new AroundTheWorldTrigger(_triggerVolume[i], i);
			_triggers[i].OnTriggered += OnTriggered;
		}
	}

	protected virtual void OnDestroy()
	{
		for (int i = 0; i < _triggerVolume.Length; i++)
		{
			_triggers[i].OnTriggered -= OnTriggered;
			_triggers[i].OnDestroy();
		}
	}

	private void OnTriggered(int TriggeredID)
	{
		for (int i = 0; i < _triggers.Length; i++)
		{
			if (!_triggers[i].reached())
			{
				return;
			}
		}
		float num = _triggers[TriggeredID].lastTimeReached();
		for (int j = 1; j < _triggers.Length; j++)
		{
			int num2 = (j + TriggeredID) % _triggers.Length;
			if (_triggers[num2].timeReached() < num)
			{
				return;
			}
			num = _triggers[num2].timeReached();
		}
		Debug.Log("Reaching " + _triggers[TriggeredID].timeReached() + " - " + _triggers[TriggeredID].lastTimeReached());
		if (_triggers[TriggeredID].timeReached() - _triggers[TriggeredID].lastTimeReached() < 90f)
		{
			Achievements.Earn(Achievements.Type.AROUND_THE_WORLD);
		}
	}
}
