using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixedEarlyUpdateManager : UpdateManagerBase, PreUpdateManager.IPreFixedUpdateListener
{
	private const int kMaxNumLightSensors = 512;

	private static MonoBehaviourGroup<OWRigidbody> _owRigidbodies = new MonoBehaviourGroup<OWRigidbody>(1024);

	private static MonoBehaviourGroup<SingleLightSensor> _lightSensors = new MonoBehaviourGroup<SingleLightSensor>(512);

	private static MonoBehaviourGroup<ForceDetector> _forceDetectors = new MonoBehaviourGroup<ForceDetector>(1024);

	private static MonoBehaviourGroup<FluidDetector> _fluidDetectors = new MonoBehaviourGroup<FluidDetector>(1024);

	public static void Register(OWRigidbody owRigidbody)
	{
		_owRigidbodies.Add(owRigidbody);
	}

	public static void Register(SingleLightSensor lightSensor)
	{
		_lightSensors.Add(lightSensor);
	}

	public static void Register(ForceDetector forceDetector)
	{
		_forceDetectors.Add(forceDetector);
	}

	public static void Register(FluidDetector fluidDetector)
	{
		_fluidDetectors.Add(fluidDetector);
	}

	public static void Unregister(OWRigidbody owRigidbody)
	{
		_owRigidbodies.Remove(owRigidbody);
	}

	public static void Unregister(SingleLightSensor lightSensor)
	{
		_lightSensors.Remove(lightSensor);
	}

	public static void Unregister(ForceDetector forceDetector)
	{
		_forceDetectors.Remove(forceDetector);
	}

	public static void Unregister(FluidDetector fluidDetector)
	{
		_fluidDetectors.Remove(fluidDetector);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		UpdateManagerBase.InstantiateManager<FixedEarlyUpdateManager>();
	}

	protected override void OnSceneUnloaded(Scene scene)
	{
		_owRigidbodies.RemoveDestroyedElements();
		_lightSensors.RemoveDestroyedElements();
		_forceDetectors.RemoveDestroyedElements();
		_fluidDetectors.RemoveDestroyedElements();
	}

	public void PreFixedUpdate()
	{
		_owRigidbodies.ProcessAdditionsAndRemovals();
		_lightSensors.ProcessAdditionsAndRemovals();
		_forceDetectors.ProcessAdditionsAndRemovals();
		_fluidDetectors.ProcessAdditionsAndRemovals();
	}

	private void FixedUpdate()
	{
		float invFixedDeltaTime = 1f / Time.fixedDeltaTime;
		Vector3 cotuStaticFrameVel = Vector3.zero;
		if (Locator.GetCenterOfTheUniverse() != null)
		{
			cotuStaticFrameVel = Locator.GetCenterOfTheUniverse().GetStaticFrameVelocity_Internal();
		}
		for (int i = 0; i < _owRigidbodies.Count; i++)
		{
			if (!_owRigidbodies.IsPendingAdditionOrRemoval(i))
			{
				try
				{
					_owRigidbodies[i].ManagedFixedUpdate(invFixedDeltaTime, cotuStaticFrameVel);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, _owRigidbodies[i]);
				}
			}
		}
		for (int j = 0; j < _lightSensors.Count; j++)
		{
			if (!_lightSensors.IsPendingAdditionOrRemoval(j))
			{
				try
				{
					_lightSensors[j].ManagedFixedUpdate();
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2, _lightSensors[j]);
				}
			}
		}
		for (int k = 0; k < _forceDetectors.Count; k++)
		{
			if (!_forceDetectors.IsPendingAdditionOrRemoval(k))
			{
				try
				{
					_forceDetectors[k].ManagedFixedUpdate();
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3, _forceDetectors[k]);
				}
			}
		}
		for (int l = 0; l < _fluidDetectors.Count; l++)
		{
			if (!_fluidDetectors.IsPendingAdditionOrRemoval(l))
			{
				try
				{
					_fluidDetectors[l].ManagedFixedUpdate();
				}
				catch (Exception exception4)
				{
					Debug.LogException(exception4, _fluidDetectors[l]);
				}
			}
		}
	}
}
