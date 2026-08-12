using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixedUpdateManager : UpdateManagerBase, PreUpdateManager.IPreFixedUpdateListener
{
	private static MonoBehaviourGroup<CustomCollisionChecker> _customCollisionCheckers = new MonoBehaviourGroup<CustomCollisionChecker>();

	private static MonoBehaviourGroup<ReferenceFrameVolume> _referenceFrameVolumes = new MonoBehaviourGroup<ReferenceFrameVolume>();

	private static MonoBehaviourGroup<AlignWithDirection> _alignWithDirections = new MonoBehaviourGroup<AlignWithDirection>();

	public static void Register(CustomCollisionChecker customCollisionChecker)
	{
		_customCollisionCheckers.Add(customCollisionChecker);
	}

	public static void Register(ReferenceFrameVolume referenceFrameVolume)
	{
		_referenceFrameVolumes.Add(referenceFrameVolume);
	}

	public static void Register(AlignWithDirection alignWithDirection)
	{
		_alignWithDirections.Add(alignWithDirection);
	}

	public static void Unregister(CustomCollisionChecker customCollisionChecker)
	{
		_customCollisionCheckers.Remove(customCollisionChecker);
	}

	public static void Unregister(ReferenceFrameVolume referenceFrameVolume)
	{
		_referenceFrameVolumes.Remove(referenceFrameVolume);
	}

	public static void Unregister(AlignWithDirection alignWithDirection)
	{
		_alignWithDirections.Remove(alignWithDirection);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		UpdateManagerBase.InstantiateManager<FixedUpdateManager>();
	}

	protected override void OnSceneUnloaded(Scene scene)
	{
		_customCollisionCheckers.RemoveDestroyedElements();
		_referenceFrameVolumes.RemoveDestroyedElements();
		_alignWithDirections.RemoveDestroyedElements();
	}

	public void PreFixedUpdate()
	{
		_customCollisionCheckers.ProcessAdditionsAndRemovals();
		_referenceFrameVolumes.ProcessAdditionsAndRemovals();
		_alignWithDirections.ProcessAdditionsAndRemovals();
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < _customCollisionCheckers.Count; i++)
		{
			if (!_customCollisionCheckers.IsPendingAdditionOrRemoval(i))
			{
				try
				{
					_customCollisionCheckers[i].ManagedFixedUpdate();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, _customCollisionCheckers[i]);
				}
			}
		}
		Vector3 activeCameraPos = Vector3.zero;
		if (Locator.GetActiveCamera() != null)
		{
			activeCameraPos = Locator.GetActiveCamera().transform.position;
		}
		for (int j = 0; j < _referenceFrameVolumes.Count; j++)
		{
			if (!_referenceFrameVolumes.IsPendingAdditionOrRemoval(j))
			{
				try
				{
					_referenceFrameVolumes[j].ManagedFixedUpdate(activeCameraPos);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2, _referenceFrameVolumes[j]);
				}
			}
		}
		for (int k = 0; k < _alignWithDirections.Count; k++)
		{
			if (!_alignWithDirections.IsPendingAdditionOrRemoval(k))
			{
				try
				{
					_alignWithDirections[k].ManagedFixedUpdate();
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3, _alignWithDirections[k]);
				}
			}
		}
	}
}
