using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class VisionSensor : MonoBehaviour
{
	public delegate void DetectionEvent(VisionDetector obj);

	private List<VisionDetector> _visionDetectors;

	private OWTriggerVolume _trigger;

	public event DetectionEvent OnDetectObject;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		_visionDetectors = new List<VisionDetector>(64);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void Update()
	{
		for (int num = _visionDetectors.Count - 1; num >= 0; num--)
		{
			if (_visionDetectors[num].CheckIllumination())
			{
				this.OnDetectObject(_visionDetectors[num]);
				_visionDetectors.RemoveAt(num);
			}
		}
		if (_visionDetectors.Count <= 0)
		{
			base.enabled = false;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		VisionDetector component = hitObj.GetComponent<VisionDetector>();
		if (component != null)
		{
			_visionDetectors.SafeAdd(component);
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		VisionDetector component = hitObj.GetComponent<VisionDetector>();
		if (component != null)
		{
			_visionDetectors.Remove(component);
		}
	}
}
