using UnityEngine;

public class RingRiverFloodSensor : MonoBehaviour
{
	private bool _inserted;

	private RingRiverFloodSensor _prev;

	private RingRiverFloodSensor _next;

	[SerializeField]
	private float _delay;

	private float _floodTime;

	private float _floodImpactTime;

	private bool _fired;

	public OWEvent OnFloodImpact = new OWEvent(4);

	private static RingRiverFloodSensor s_head;

	private static RingRiverFloodSensor s_tail;

	private static uint s_count;

	private static RingRiverController s_riverController;

	private static RingRiverFloodSensor s_lastTriggeredSensor;

	private static float s_lastFloodTime;

	private void Start()
	{
		CalcFloodTime();
		Insert(this);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		Remove(this);
	}

	private void CalcFloodTime()
	{
		Vector3 vector = s_riverController.transform.InverseTransformPoint(base.transform.position);
		float num = OWMath.Angle(Vector3.forward, new Vector3(vector.x, 0f, vector.z), Vector3.up);
		if (num < 0f)
		{
			num += 360f;
		}
		_floodTime = num / 360f;
	}

	private void OnFloodImpact_Internal()
	{
		if (!_fired)
		{
			_floodImpactTime = Time.timeSinceLevelLoad;
			if (_delay > 0f)
			{
				base.enabled = true;
				return;
			}
			_fired = true;
			OnFloodImpact.Invoke();
		}
	}

	private void FixedUpdate()
	{
		if (_fired)
		{
			base.enabled = false;
		}
		else if (!_fired && Time.timeSinceLevelLoad >= _floodImpactTime + _delay)
		{
			_fired = true;
			OnFloodImpact.Invoke();
			base.enabled = false;
		}
	}

	public static void Initialize(RingRiverController riverController)
	{
		s_riverController = riverController;
		s_lastTriggeredSensor = null;
		s_lastFloodTime = 0f;
	}

	public static void Teardown()
	{
		RingRiverFloodSensor ringRiverFloodSensor = s_head;
		while ((object)ringRiverFloodSensor != null)
		{
			RingRiverFloodSensor next = ringRiverFloodSensor._next;
			ringRiverFloodSensor._inserted = false;
			ringRiverFloodSensor._prev = null;
			ringRiverFloodSensor._next = null;
			ringRiverFloodSensor = next;
		}
		s_head = null;
		s_tail = null;
		s_count = 0u;
		s_riverController = null;
		s_lastTriggeredSensor = null;
		s_lastFloodTime = 0f;
	}

	public static void UpdateFloodTime(float floodTime)
	{
		if (floodTime <= s_lastFloodTime)
		{
			return;
		}
		s_lastFloodTime = floodTime;
		if (s_count != 0 && !(s_lastFloodTime < s_head._floodTime))
		{
			RingRiverFloodSensor ringRiverFloodSensor = (((object)s_lastTriggeredSensor == null) ? s_head : s_lastTriggeredSensor._next);
			while ((object)ringRiverFloodSensor != null && !(s_lastFloodTime < ringRiverFloodSensor._floodTime))
			{
				ringRiverFloodSensor.OnFloodImpact_Internal();
				s_lastTriggeredSensor = ringRiverFloodSensor;
				ringRiverFloodSensor = ringRiverFloodSensor._next;
			}
		}
	}

	private static void Insert(RingRiverFloodSensor node)
	{
		if (node._inserted)
		{
			return;
		}
		node._next = null;
		node._prev = null;
		if (s_count == 0)
		{
			s_head = node;
			s_tail = node;
		}
		else if (node._floodTime < s_head._floodTime)
		{
			node._next = s_head;
			s_head._prev = node;
			s_head = node;
		}
		else if (node._floodTime >= s_tail._floodTime)
		{
			node._prev = s_tail;
			s_tail._next = node;
			s_tail = node;
		}
		else
		{
			RingRiverFloodSensor next = s_head;
			while ((object)next != null)
			{
				if (node._floodTime < next._floodTime)
				{
					node._next = next;
					node._prev = next._prev;
					node._next._prev = node;
					node._prev._next = node;
					break;
				}
				next = next._next;
			}
		}
		node._inserted = true;
		s_count++;
	}

	private static void Remove(RingRiverFloodSensor node)
	{
		if (node._inserted)
		{
			if ((object)node == s_lastTriggeredSensor)
			{
				s_lastTriggeredSensor = node._prev;
			}
			if (s_count == 1)
			{
				s_head = null;
				s_tail = null;
			}
			else if ((object)node == s_head)
			{
				s_head = node._next;
				s_head._prev = null;
			}
			else if ((object)node == s_tail)
			{
				s_tail = node._prev;
				s_tail._next = null;
			}
			else
			{
				node._next._prev = node._prev;
				node._prev._next = node._next;
			}
			node._inserted = false;
			node._next = null;
			node._prev = null;
			s_count--;
		}
	}
}
