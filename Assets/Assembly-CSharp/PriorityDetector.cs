using System.Collections.Generic;
using UnityEngine;

public abstract class PriorityDetector : Detector
{
	[SerializeField]
	private bool _printLog;

	private IDictionary<int, TrackedLayer<PriorityVolume>> _trackedLayers;

	private float _lastLogTime;

	protected override void InitializeCollections()
	{
		_activeVolumes = new List<EffectVolume>(32);
		_trackedLayers = new Dictionary<int, TrackedLayer<PriorityVolume>>(16, ComparerLibrary.intEqComparer);
		_trackedLayers.Add(0, new TrackedLayer<PriorityVolume>());
	}

	public override void AddVolume(EffectVolume eVol)
	{
		PriorityVolume priorityVolume = eVol as PriorityVolume;
		int layer = priorityVolume.GetLayer();
		TrackedLayer<PriorityVolume> trackedLayer;
		if (!_trackedLayers.ContainsKey(layer))
		{
			trackedLayer = new TrackedLayer<PriorityVolume>();
			_trackedLayers.Add(layer, trackedLayer);
		}
		else
		{
			trackedLayer = _trackedLayers[layer];
			if (trackedLayer.volumes.Contains(priorityVolume))
			{
				Debug.Log("This volume is already being tracked", priorityVolume);
				Debug.Log("This detector tried to add a volume twice", this);
				Debug.Break();
				return;
			}
		}
		if (trackedLayer.isActive && priorityVolume.GetPriority() == trackedLayer.GetHighestPriority())
		{
			OnVolumeActivated(priorityVolume);
			AddToActives(priorityVolume);
		}
		else if (priorityVolume.GetPriority() > trackedLayer.GetHighestPriority())
		{
			if (layer == 0)
			{
				OnVolumeActivated(priorityVolume);
				DeactivateBelowPriority(priorityVolume.GetPriority());
				AddToActives(priorityVolume);
			}
			else if (!_trackedLayers[0].isActive || priorityVolume.GetPriority() >= _trackedLayers[0].GetHighestPriority())
			{
				OnVolumeActivated(priorityVolume);
				DeactivateLayer(layer);
				AddToActives(priorityVolume);
			}
		}
		trackedLayer.AddVolume(priorityVolume);
		OnVolumeAdded(priorityVolume);
		if (_printLog)
		{
			MonoBehaviour.print("ADDED VOLUME " + priorityVolume);
			PrintLog();
		}
	}

	public override void RemoveVolume(EffectVolume eVol)
	{
		PriorityVolume priorityVolume = eVol as PriorityVolume;
		int layer = priorityVolume.GetLayer();
		TrackedLayer<PriorityVolume> trackedLayer = _trackedLayers[layer];
		if (trackedLayer.RemoveVolume(priorityVolume))
		{
			if (_activeVolumes.Remove(priorityVolume))
			{
				int priority = priorityVolume.GetPriority();
				int highestPriority = trackedLayer.GetHighestPriority();
				if (trackedLayer.isActive && highestPriority < priority)
				{
					if (layer == 0)
					{
						ActivateLayer(0);
						ActivateNonZeroLayersAboveOrEqualToPriority(highestPriority);
					}
					else if (!_trackedLayers[0].isActive || highestPriority >= _trackedLayers[0].GetHighestPriority())
					{
						ActivateLayer(layer);
					}
					else
					{
						trackedLayer.isActive = false;
					}
				}
				OnVolumeDeactivated(priorityVolume);
			}
			if (trackedLayer.volumes.Count == 0)
			{
				trackedLayer.isActive = false;
			}
			OnVolumeRemoved(priorityVolume);
		}
		else
		{
			Debug.Log(string.Concat(this, " tried to remove ", priorityVolume.gameObject.name, " from layer ", layer, "...but it isn't being tracked!"), priorityVolume);
			Debug.Break();
		}
		if (_printLog)
		{
			MonoBehaviour.print("REMOVED VOLUME " + priorityVolume);
			PrintLog();
		}
	}

	protected virtual void RebuildActiveLayers()
	{
		int num = int.MinValue;
		IEnumerator<KeyValuePair<int, TrackedLayer<PriorityVolume>>> enumerator = _trackedLayers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int key = enumerator.Current.Key;
			TrackedLayer<PriorityVolume> value = enumerator.Current.Value;
			value.Sort();
			if (key == 0)
			{
				num = value.GetHighestPriority();
			}
		}
		_activeVolumes.Clear();
		enumerator.Reset();
		while (enumerator.MoveNext())
		{
			TrackedLayer<PriorityVolume> value2 = enumerator.Current.Value;
			int highestPriority = value2.GetHighestPriority();
			value2.isActive = highestPriority >= num;
			if (value2.isActive)
			{
				for (int i = 0; i < value2.volumes.Count && value2.volumes[i].GetPriority() == highestPriority; i++)
				{
					_activeVolumes.Add(value2.volumes[i]);
				}
			}
		}
	}

	protected virtual void OnVolumeActivated(PriorityVolume volume)
	{
	}

	protected virtual void OnVolumeDeactivated(PriorityVolume volume)
	{
	}

	private void AddToActives(PriorityVolume activatedVolume)
	{
		_activeVolumes.Add(activatedVolume);
		_trackedLayers[activatedVolume.GetLayer()].isActive = true;
	}

	private void ActivateNonZeroLayersAboveOrEqualToPriority(int priorityCutoff)
	{
		IEnumerator<KeyValuePair<int, TrackedLayer<PriorityVolume>>> enumerator = _trackedLayers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int key = enumerator.Current.Key;
			TrackedLayer<PriorityVolume> value = enumerator.Current.Value;
			if (!value.isActive && key != 0 && value.GetHighestPriority() >= priorityCutoff)
			{
				ActivateLayer(key);
			}
		}
	}

	private void ActivateLayersWithPriority(int priorityToMatch)
	{
		IEnumerator<KeyValuePair<int, TrackedLayer<PriorityVolume>>> enumerator = _trackedLayers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int key = enumerator.Current.Key;
			if (enumerator.Current.Value.GetHighestPriority() == priorityToMatch)
			{
				ActivateLayer(key);
			}
		}
	}

	private void ActivateLayer(int layerIndex)
	{
		if (!IsDetectorEnabled())
		{
			return;
		}
		for (int i = 0; i < _trackedLayers[layerIndex].volumes.Count; i++)
		{
			TrackedLayer<PriorityVolume> trackedLayer = _trackedLayers[layerIndex];
			if (trackedLayer.volumes[i].GetPriority() == trackedLayer.GetHighestPriority())
			{
				OnVolumeActivated(trackedLayer.volumes[i]);
				AddToActives(trackedLayer.volumes[i]);
				continue;
			}
			break;
		}
	}

	private void DeactivateBelowPriority(int priorityCutoff)
	{
		for (int num = _activeVolumes.Count - 1; num >= 0; num--)
		{
			PriorityVolume priorityVolume = _activeVolumes[num] as PriorityVolume;
			if (priorityVolume.GetPriority() < priorityCutoff)
			{
				_trackedLayers[priorityVolume.GetLayer()].isActive = false;
				OnVolumeDeactivated(_activeVolumes[num] as PriorityVolume);
				_activeVolumes.RemoveAt(num);
			}
		}
	}

	private void DeactivateLayer(int layer)
	{
		_trackedLayers[layer].isActive = false;
		for (int num = _activeVolumes.Count - 1; num >= 0; num--)
		{
			if ((_activeVolumes[num] as PriorityVolume).GetLayer() == layer)
			{
				OnVolumeDeactivated(_activeVolumes[num] as PriorityVolume);
				_activeVolumes.RemoveAt(num);
			}
		}
	}

	private void DeactivateLayers(int layerOne, int layerTwo)
	{
		_trackedLayers[layerOne].isActive = false;
		_trackedLayers[layerTwo].isActive = false;
		for (int num = _activeVolumes.Count - 1; num >= 0; num--)
		{
			PriorityVolume priorityVolume = _activeVolumes[num] as PriorityVolume;
			if (priorityVolume.GetLayer() == layerOne || priorityVolume.GetLayer() == layerTwo)
			{
				OnVolumeDeactivated(priorityVolume);
				_activeVolumes.RemoveAt(num);
			}
		}
	}

	private void PrintLog()
	{
		string text = "Tracked: ";
		foreach (int key in _trackedLayers.Keys)
		{
			if (_trackedLayers[key].volumes.Count <= 0)
			{
				continue;
			}
			text = text + "L" + key + " ";
			if (_trackedLayers[key].isActive)
			{
				text += "(";
			}
			for (int i = 0; i < _trackedLayers[key].volumes.Count; i++)
			{
				text += _trackedLayers[key].volumes[i].GetPriority();
				if (i != _trackedLayers[key].volumes.Count - 1)
				{
					text += ", ";
				}
			}
			if (_trackedLayers[key].isActive)
			{
				text += ")";
			}
			text += " ";
		}
		MonoBehaviour.print(text);
	}
}
