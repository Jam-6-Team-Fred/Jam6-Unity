using System;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalMessenger
{
	private class EventData
	{
		public List<Callback> callbacks = new List<Callback>();

		public List<Callback> temp = new List<Callback>();

		public bool isInvoking;
	}

	private static IDictionary<string, EventData> eventTable = new Dictionary<string, EventData>(ComparerLibrary.stringEqComparer);

	public static void AddListener(string eventType, Callback handler)
	{
		lock (eventTable)
		{
			if (!eventTable.TryGetValue(eventType, out var value))
			{
				value = new EventData();
				eventTable.Add(eventType, value);
			}
			value.callbacks.Add(handler);
		}
	}

	public static void RemoveListener(string eventType, Callback handler)
	{
		lock (eventTable)
		{
			if (eventTable.TryGetValue(eventType, out var value))
			{
				int num = value.callbacks.IndexOf(handler);
				if (num >= 0)
				{
					value.callbacks[num] = value.callbacks[value.callbacks.Count - 1];
					value.callbacks.RemoveAt(value.callbacks.Count - 1);
				}
			}
		}
	}

	public static void FireEvent(string eventType)
	{
		lock (eventTable)
		{
			if (!eventTable.TryGetValue(eventType, out var value))
			{
				return;
			}
			if (value.isInvoking)
			{
				throw new InvalidOperationException("GlobalMessenger does not support recursive FireEvent calls to the same eventType.");
			}
			value.isInvoking = true;
			value.temp.AddRange(value.callbacks);
			for (int i = 0; i < value.temp.Count; i++)
			{
				try
				{
					value.temp[i]();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			value.temp.Clear();
			value.isInvoking = false;
		}
	}
}
public static class GlobalMessenger<T>
{
	private class EventData
	{
		public List<Callback<T>> callbacks = new List<Callback<T>>();

		public List<Callback<T>> temp = new List<Callback<T>>();

		public bool isInvoking;
	}

	private static IDictionary<string, EventData> eventTable = new Dictionary<string, EventData>(ComparerLibrary.stringEqComparer);

	public static void AddListener(string eventType, Callback<T> handler)
	{
		lock (eventTable)
		{
			if (!eventTable.TryGetValue(eventType, out var value))
			{
				value = new EventData();
				eventTable.Add(eventType, value);
			}
			value.callbacks.Add(handler);
		}
	}

	public static void RemoveListener(string eventType, Callback<T> handler)
	{
		lock (eventTable)
		{
			if (eventTable.TryGetValue(eventType, out var value))
			{
				int num = value.callbacks.IndexOf(handler);
				if (num >= 0)
				{
					value.callbacks[num] = value.callbacks[value.callbacks.Count - 1];
					value.callbacks.RemoveAt(value.callbacks.Count - 1);
				}
			}
		}
	}

	public static void FireEvent(string eventType, T arg1)
	{
		lock (eventTable)
		{
			if (!eventTable.TryGetValue(eventType, out var value))
			{
				return;
			}
			if (value.isInvoking)
			{
				throw new InvalidOperationException("GlobalMessenger does not support recursive FireEvent calls to the same eventType.");
			}
			value.isInvoking = true;
			value.temp.AddRange(value.callbacks);
			for (int i = 0; i < value.temp.Count; i++)
			{
				try
				{
					value.temp[i](arg1);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			value.temp.Clear();
			value.isInvoking = false;
		}
	}
}
public static class GlobalMessenger<T, U>
{
	private class EventData
	{
		public List<Callback<T, U>> callbacks = new List<Callback<T, U>>();

		public List<Callback<T, U>> temp = new List<Callback<T, U>>();

		public bool isInvoking;
	}

	private static IDictionary<string, EventData> eventTable = new Dictionary<string, EventData>(ComparerLibrary.stringEqComparer);

	public static void AddListener(string eventType, Callback<T, U> handler)
	{
		lock (eventTable)
		{
			if (!eventTable.TryGetValue(eventType, out var value))
			{
				value = new EventData();
				eventTable.Add(eventType, value);
			}
			value.callbacks.Add(handler);
		}
	}

	public static void RemoveListener(string eventType, Callback<T, U> handler)
	{
		lock (eventTable)
		{
			if (eventTable.TryGetValue(eventType, out var value))
			{
				int num = value.callbacks.IndexOf(handler);
				if (num >= 0)
				{
					value.callbacks[num] = value.callbacks[value.callbacks.Count - 1];
					value.callbacks.RemoveAt(value.callbacks.Count - 1);
				}
			}
		}
	}

	public static void FireEvent(string eventType, T arg1, U arg2)
	{
		lock (eventTable)
		{
			if (!eventTable.TryGetValue(eventType, out var value))
			{
				return;
			}
			if (value.isInvoking)
			{
				throw new InvalidOperationException("GlobalMessenger does not support recursive FireEvent calls to the same eventType.");
			}
			value.isInvoking = true;
			value.temp.AddRange(value.callbacks);
			for (int i = 0; i < value.temp.Count; i++)
			{
				try
				{
					value.temp[i](arg1, arg2);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			value.temp.Clear();
			value.isInvoking = false;
		}
	}
}
public static class GlobalMessenger<T, U, W>
{
	private class EventData
	{
		public List<Callback<T, U, W>> callbacks = new List<Callback<T, U, W>>();

		public List<Callback<T, U, W>> temp = new List<Callback<T, U, W>>();

		public bool isInvoking;
	}

	private static IDictionary<string, EventData> eventTable = new Dictionary<string, EventData>(ComparerLibrary.stringEqComparer);

	public static void AddListener(string eventType, Callback<T, U, W> handler)
	{
		lock (eventTable)
		{
			if (!eventTable.TryGetValue(eventType, out var value))
			{
				value = new EventData();
				eventTable.Add(eventType, value);
			}
			value.callbacks.Add(handler);
		}
	}

	public static void RemoveListener(string eventType, Callback<T, U, W> handler)
	{
		lock (eventTable)
		{
			if (eventTable.TryGetValue(eventType, out var value))
			{
				int num = value.callbacks.IndexOf(handler);
				if (num >= 0)
				{
					value.callbacks[num] = value.callbacks[value.callbacks.Count - 1];
					value.callbacks.RemoveAt(value.callbacks.Count - 1);
				}
			}
		}
	}

	public static void FireEvent(string eventType, T arg1, U arg2, W arg3)
	{
		lock (eventTable)
		{
			if (!eventTable.TryGetValue(eventType, out var value))
			{
				return;
			}
			if (value.isInvoking)
			{
				throw new InvalidOperationException("GlobalMessenger does not support recursive FireEvent calls to the same eventType.");
			}
			value.isInvoking = true;
			value.temp.AddRange(value.callbacks);
			for (int i = 0; i < value.temp.Count; i++)
			{
				try
				{
					value.temp[i](arg1, arg2, arg3);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			value.temp.Clear();
			value.isInvoking = false;
		}
	}
}
