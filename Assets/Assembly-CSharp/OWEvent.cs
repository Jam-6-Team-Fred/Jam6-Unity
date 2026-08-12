using System;
using System.Collections.Generic;
using UnityEngine;

public struct OWEvent
{
	public delegate void OWCallback();

	private List<OWCallback> _callbacks;

	private List<OWCallback> _activeCallbacks;

	private bool _isInvoking;

	public OWEvent(int capacity)
	{
		_callbacks = new List<OWCallback>(capacity);
		_activeCallbacks = new List<OWCallback>(capacity);
		_isInvoking = false;
	}

	public void AddListener(OWCallback handler)
	{
		if (_callbacks == null)
		{
			_callbacks = new List<OWCallback>();
		}
		_callbacks.Add(handler);
	}

	public void RemoveListener(OWCallback handler)
	{
		if (_callbacks != null)
		{
			int num = _callbacks.IndexOf(handler);
			if (num >= 0)
			{
				_callbacks[num] = _callbacks[_callbacks.Count - 1];
				_callbacks.RemoveAt(_callbacks.Count - 1);
			}
		}
	}

	public int GetNumListeners()
	{
		if (_callbacks == null)
		{
			return 0;
		}
		return _callbacks.Count;
	}

	public OWCallback[] GetCallbacks()
	{
		if (_callbacks == null)
		{
			return new OWCallback[0];
		}
		return _callbacks.ToArray();
	}

	public void Invoke()
	{
		if (_callbacks == null)
		{
			return;
		}
		if (_activeCallbacks == null)
		{
			_activeCallbacks = new List<OWCallback>(_callbacks.Capacity);
		}
		if (_isInvoking)
		{
			throw new InvalidOperationException("OWEvent does not support recursive Invoke calls.");
		}
		_isInvoking = true;
		_activeCallbacks.AddRange(_callbacks);
		for (int i = 0; i < _activeCallbacks.Count; i++)
		{
			try
			{
				_activeCallbacks[i]();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_activeCallbacks.Clear();
		_isInvoking = false;
	}

	public static OWEvent operator +(OWEvent owEvent, OWCallback owCallback)
	{
		owEvent.AddListener(owCallback);
		return owEvent;
	}

	public static OWEvent operator -(OWEvent owEvent, OWCallback owCallback)
	{
		owEvent.RemoveListener(owCallback);
		return owEvent;
	}
}
public struct OWEvent<T>
{
	public delegate void OWCallback(T arg1);

	private List<OWCallback> _callbacks;

	private List<OWCallback> _activeCallbacks;

	private bool _isInvoking;

	public OWEvent(int capacity)
	{
		_callbacks = new List<OWCallback>(capacity);
		_activeCallbacks = new List<OWCallback>(capacity);
		_isInvoking = false;
	}

	public void AddListener(OWCallback handler)
	{
		if (_callbacks == null)
		{
			_callbacks = new List<OWCallback>();
		}
		_callbacks.Add(handler);
	}

	public void RemoveListener(OWCallback handler)
	{
		if (_callbacks != null)
		{
			int num = _callbacks.IndexOf(handler);
			if (num >= 0)
			{
				_callbacks[num] = _callbacks[_callbacks.Count - 1];
				_callbacks.RemoveAt(_callbacks.Count - 1);
			}
		}
	}

	public int GetNumListeners()
	{
		if (_callbacks == null)
		{
			return 0;
		}
		return _callbacks.Count;
	}

	public OWCallback[] GetCallbacks()
	{
		if (_callbacks == null)
		{
			return new OWCallback[0];
		}
		return _callbacks.ToArray();
	}

	public void Invoke(T arg1)
	{
		if (_callbacks == null)
		{
			return;
		}
		if (_activeCallbacks == null)
		{
			_activeCallbacks = new List<OWCallback>(_callbacks.Capacity);
		}
		if (_isInvoking)
		{
			throw new InvalidOperationException("OWEvent does not support recursive Invoke calls.");
		}
		_isInvoking = true;
		_activeCallbacks.AddRange(_callbacks);
		for (int i = 0; i < _activeCallbacks.Count; i++)
		{
			try
			{
				_activeCallbacks[i](arg1);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_activeCallbacks.Clear();
		_isInvoking = false;
	}

	public static OWEvent<T>operator +(OWEvent<T> owEvent, OWCallback owCallback)
	{
		owEvent.AddListener(owCallback);
		return owEvent;
	}

	public static OWEvent<T>operator -(OWEvent<T> owEvent, OWCallback owCallback)
	{
		owEvent.RemoveListener(owCallback);
		return owEvent;
	}
}
public struct OWEvent<T, U>
{
	public delegate void OWCallback(T arg1, U arg2);

	private List<OWCallback> _callbacks;

	private List<OWCallback> _activeCallbacks;

	private bool _isInvoking;

	public OWEvent(int capacity)
	{
		_callbacks = new List<OWCallback>(capacity);
		_activeCallbacks = new List<OWCallback>(capacity);
		_isInvoking = false;
	}

	public void AddListener(OWCallback handler)
	{
		if (_callbacks == null)
		{
			_callbacks = new List<OWCallback>();
		}
		_callbacks.Add(handler);
	}

	public void RemoveListener(OWCallback handler)
	{
		if (_callbacks != null)
		{
			int num = _callbacks.IndexOf(handler);
			if (num >= 0)
			{
				_callbacks[num] = _callbacks[_callbacks.Count - 1];
				_callbacks.RemoveAt(_callbacks.Count - 1);
			}
		}
	}

	public int GetNumListeners()
	{
		if (_callbacks == null)
		{
			return 0;
		}
		return _callbacks.Count;
	}

	public OWCallback[] GetCallbacks()
	{
		if (_callbacks == null)
		{
			return new OWCallback[0];
		}
		return _callbacks.ToArray();
	}

	public void Invoke(T arg1, U arg2)
	{
		if (_callbacks == null)
		{
			return;
		}
		if (_activeCallbacks == null)
		{
			_activeCallbacks = new List<OWCallback>(_callbacks.Capacity);
		}
		if (_isInvoking)
		{
			throw new InvalidOperationException("OWEvent does not support recursive Invoke calls.");
		}
		_isInvoking = true;
		_activeCallbacks.AddRange(_callbacks);
		for (int i = 0; i < _activeCallbacks.Count; i++)
		{
			try
			{
				_activeCallbacks[i](arg1, arg2);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_activeCallbacks.Clear();
		_isInvoking = false;
	}

	public static OWEvent<T, U>operator +(OWEvent<T, U> owEvent, OWCallback owCallback)
	{
		owEvent.AddListener(owCallback);
		return owEvent;
	}

	public static OWEvent<T, U>operator -(OWEvent<T, U> owEvent, OWCallback owCallback)
	{
		owEvent.RemoveListener(owCallback);
		return owEvent;
	}
}
public struct OWEvent<T, U, V>
{
	public delegate void OWCallback(T arg1, U arg2, V arg3);

	private List<OWCallback> _callbacks;

	private List<OWCallback> _activeCallbacks;

	private bool _isInvoking;

	public OWEvent(int capacity)
	{
		_callbacks = new List<OWCallback>(capacity);
		_activeCallbacks = new List<OWCallback>(capacity);
		_isInvoking = false;
	}

	public void AddListener(OWCallback handler)
	{
		if (_callbacks == null)
		{
			_callbacks = new List<OWCallback>();
		}
		_callbacks.Add(handler);
	}

	public void RemoveListener(OWCallback handler)
	{
		if (_callbacks != null)
		{
			int num = _callbacks.IndexOf(handler);
			if (num >= 0)
			{
				_callbacks[num] = _callbacks[_callbacks.Count - 1];
				_callbacks.RemoveAt(_callbacks.Count - 1);
			}
		}
	}

	public int GetNumListeners()
	{
		if (_callbacks == null)
		{
			return 0;
		}
		return _callbacks.Count;
	}

	public OWCallback[] GetCallbacks()
	{
		if (_callbacks == null)
		{
			return new OWCallback[0];
		}
		return _callbacks.ToArray();
	}

	public void Invoke(T arg1, U arg2, V arg3)
	{
		if (_callbacks == null)
		{
			return;
		}
		if (_activeCallbacks == null)
		{
			_activeCallbacks = new List<OWCallback>(_callbacks.Capacity);
		}
		if (_isInvoking)
		{
			throw new InvalidOperationException("OWEvent does not support recursive Invoke calls.");
		}
		_isInvoking = true;
		_activeCallbacks.AddRange(_callbacks);
		for (int i = 0; i < _activeCallbacks.Count; i++)
		{
			try
			{
				_activeCallbacks[i](arg1, arg2, arg3);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_activeCallbacks.Clear();
		_isInvoking = false;
	}

	public static OWEvent<T, U, V>operator +(OWEvent<T, U, V> owEvent, OWCallback owCallback)
	{
		owEvent.AddListener(owCallback);
		return owEvent;
	}

	public static OWEvent<T, U, V>operator -(OWEvent<T, U, V> owEvent, OWCallback owCallback)
	{
		owEvent.RemoveListener(owCallback);
		return owEvent;
	}
}
public struct OWEvent<T, U, V, W>
{
	public delegate void OWCallback(T arg1, U arg2, V arg3, W arg4);

	private List<OWCallback> _callbacks;

	private List<OWCallback> _activeCallbacks;

	private bool _isInvoking;

	public OWEvent(int capacity)
	{
		_callbacks = new List<OWCallback>(capacity);
		_activeCallbacks = new List<OWCallback>(capacity);
		_isInvoking = false;
	}

	public void AddListener(OWCallback handler)
	{
		if (_callbacks == null)
		{
			_callbacks = new List<OWCallback>();
		}
		_callbacks.Add(handler);
	}

	public void RemoveListener(OWCallback handler)
	{
		if (_callbacks != null)
		{
			int num = _callbacks.IndexOf(handler);
			if (num >= 0)
			{
				_callbacks[num] = _callbacks[_callbacks.Count - 1];
				_callbacks.RemoveAt(_callbacks.Count - 1);
			}
		}
	}

	public int GetNumListeners()
	{
		if (_callbacks == null)
		{
			return 0;
		}
		return _callbacks.Count;
	}

	public OWCallback[] GetCallbacks()
	{
		if (_callbacks == null)
		{
			return new OWCallback[0];
		}
		return _callbacks.ToArray();
	}

	public void Invoke(T arg1, U arg2, V arg3, W arg4)
	{
		if (_callbacks == null)
		{
			return;
		}
		if (_activeCallbacks == null)
		{
			_activeCallbacks = new List<OWCallback>(_callbacks.Capacity);
		}
		if (_isInvoking)
		{
			throw new InvalidOperationException("OWEvent does not support recursive Invoke calls.");
		}
		_isInvoking = true;
		_activeCallbacks.AddRange(_callbacks);
		for (int i = 0; i < _activeCallbacks.Count; i++)
		{
			try
			{
				_activeCallbacks[i](arg1, arg2, arg3, arg4);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_activeCallbacks.Clear();
		_isInvoking = false;
	}

	public static OWEvent<T, U, V, W>operator +(OWEvent<T, U, V, W> owEvent, OWCallback owCallback)
	{
		owEvent.AddListener(owCallback);
		return owEvent;
	}

	public static OWEvent<T, U, V, W>operator -(OWEvent<T, U, V, W> owEvent, OWCallback owCallback)
	{
		owEvent.RemoveListener(owCallback);
		return owEvent;
	}
}
