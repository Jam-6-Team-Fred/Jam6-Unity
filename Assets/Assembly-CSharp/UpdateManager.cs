using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpdateManager : UpdateManagerBase, PreUpdateManager.IPreUpdateListener
{
	private const int kMaxNumLightFlickerers = 512;

	private static MonoBehaviourGroup<LightFlicker2> _lightFlickerers = new MonoBehaviourGroup<LightFlicker2>(512);

	public static void Register(LightFlicker2 lightFlicker)
	{
		_lightFlickerers.Add(lightFlicker);
	}

	public static void Unregister(LightFlicker2 lightFlicker)
	{
		_lightFlickerers.Remove(lightFlicker);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		UpdateManagerBase.InstantiateManager<UpdateManager>();
	}

	protected override void OnSceneUnloaded(Scene scene)
	{
		_lightFlickerers.RemoveDestroyedElements();
	}

	public void PreUpdate()
	{
		_lightFlickerers.ProcessAdditionsAndRemovals();
	}

	private void Update()
	{
		for (int i = 0; i < _lightFlickerers.Count; i++)
		{
			if (!_lightFlickerers.IsPendingAdditionOrRemoval(i))
			{
				try
				{
					_lightFlickerers[i].ManagedUpdate();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, _lightFlickerers[i]);
				}
			}
		}
	}
}
