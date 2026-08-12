using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixedLateUpdateManager : UpdateManagerBase, PreUpdateManager.IPreFixedUpdateListener
{
	private static MonoBehaviourGroup<ForceApplier> _forceAppliers = new MonoBehaviourGroup<ForceApplier>(1024);

	private static MonoBehaviourGroup<CenterOfTheUniverseOffsetApplier> _cotuOffsetAppliers = new MonoBehaviourGroup<CenterOfTheUniverseOffsetApplier>(1024);

	private static MonoBehaviourGroup<KinematicRigidbody> _kinematicRigidbodies = new MonoBehaviourGroup<KinematicRigidbody>(1024);

	public static void Register(ForceApplier forceApplier)
	{
		_forceAppliers.Add(forceApplier);
	}

	public static void Register(CenterOfTheUniverseOffsetApplier cotuOffsetApplier)
	{
		_cotuOffsetAppliers.Add(cotuOffsetApplier);
	}

	public static void Register(KinematicRigidbody kinematicRigidbody)
	{
		_kinematicRigidbodies.Add(kinematicRigidbody);
	}

	public static void Unregister(ForceApplier forceApplier)
	{
		_forceAppliers.Remove(forceApplier);
	}

	public static void Unregister(CenterOfTheUniverseOffsetApplier cotuOffsetApplier)
	{
		_cotuOffsetAppliers.Remove(cotuOffsetApplier);
	}

	public static void Unregister(KinematicRigidbody kinematicRigidbody)
	{
		_kinematicRigidbodies.Remove(kinematicRigidbody);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		UpdateManagerBase.InstantiateManager<FixedLateUpdateManager>();
	}

	protected override void OnSceneUnloaded(Scene scene)
	{
		_forceAppliers.RemoveDestroyedElements();
		_cotuOffsetAppliers.RemoveDestroyedElements();
		_kinematicRigidbodies.RemoveDestroyedElements();
	}

	public void PreFixedUpdate()
	{
		_forceAppliers.ProcessAdditionsAndRemovals();
		_cotuOffsetAppliers.ProcessAdditionsAndRemovals();
		_kinematicRigidbodies.ProcessAdditionsAndRemovals();
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < _forceAppliers.Count; i++)
		{
			if (!_forceAppliers.IsPendingAdditionOrRemoval(i))
			{
				try
				{
					_forceAppliers[i].ManagedFixedLateUpdate();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, _forceAppliers[i]);
				}
			}
		}
		Vector3 cotuOffsetVelocity = Vector3.zero;
		if (Locator.GetCenterOfTheUniverse() != null)
		{
			cotuOffsetVelocity = Locator.GetCenterOfTheUniverse().GetOffsetVelocity();
		}
		for (int j = 0; j < _cotuOffsetAppliers.Count; j++)
		{
			if (!_cotuOffsetAppliers.IsPendingAdditionOrRemoval(j))
			{
				try
				{
					_cotuOffsetAppliers[j].ManagedFixedLateUpdate(cotuOffsetVelocity);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2, _cotuOffsetAppliers[j]);
				}
			}
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		for (int k = 0; k < _kinematicRigidbodies.Count; k++)
		{
			if (!_kinematicRigidbodies.IsPendingAdditionOrRemoval(k))
			{
				try
				{
					_kinematicRigidbodies[k].Integrate(fixedDeltaTime);
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3, _kinematicRigidbodies[k]);
				}
			}
		}
	}
}
