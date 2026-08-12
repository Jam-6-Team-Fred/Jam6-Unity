using System;
using System.Collections.Generic;
using UnityEngine;

public class PreUpdateManager : UpdateManagerBase
{
	public interface IPreUpdateListener
	{
		void PreUpdate();
	}

	public interface IPreFixedUpdateListener
	{
		void PreFixedUpdate();
	}

	public interface IPreLateUpdateListener
	{
		void PreLateUpdate();
	}

	private static List<IPreUpdateListener> s_preUpdateListeners = new List<IPreUpdateListener>(8);

	private static List<IPreFixedUpdateListener> s_preFixedUpdateListeners = new List<IPreFixedUpdateListener>(8);

	private static List<IPreLateUpdateListener> s_preLateUpdateListeners = new List<IPreLateUpdateListener>(8);

	public static void Register(IPreUpdateListener preUpdateListener)
	{
		s_preUpdateListeners.Add(preUpdateListener);
	}

	public static void Register(IPreFixedUpdateListener preFixedUpdateListener)
	{
		s_preFixedUpdateListeners.Add(preFixedUpdateListener);
	}

	public static void Register(IPreLateUpdateListener preLateUpdateListener)
	{
		s_preLateUpdateListeners.Add(preLateUpdateListener);
	}

	public static void Unregister(IPreUpdateListener preUpdateListener)
	{
		s_preUpdateListeners.Remove(preUpdateListener);
	}

	public static void Unregister(IPreFixedUpdateListener preFixedUpdateListener)
	{
		s_preFixedUpdateListeners.Remove(preFixedUpdateListener);
	}

	public static void Unregister(IPreLateUpdateListener preLateUpdateListener)
	{
		s_preLateUpdateListeners.Remove(preLateUpdateListener);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		UpdateManagerBase.InstantiateManager<PreUpdateManager>();
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < s_preFixedUpdateListeners.Count; i++)
		{
			try
			{
				s_preFixedUpdateListeners[i].PreFixedUpdate();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}
	}

	private void Update()
	{
		for (int i = 0; i < s_preUpdateListeners.Count; i++)
		{
			try
			{
				s_preUpdateListeners[i].PreUpdate();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < s_preLateUpdateListeners.Count; i++)
		{
			try
			{
				s_preLateUpdateListeners[i].PreLateUpdate();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}
	}
}
