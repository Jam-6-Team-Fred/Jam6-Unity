using System;
using UnityEngine;

public class ConstantForceDetector : ForceDetector
{
	[SerializeField]
	private ForceVolume[] _detectableFields;

	[SerializeField]
	private ForceDetector _inheritDetector;

	[SerializeField]
	private bool _inheritElement0;

	private void Start()
	{
		if (_detectableFields != null && _detectableFields.Length != 0)
		{
			_activeVolumes.AddRange(_detectableFields);
			if (_inheritElement0 && _detectableFields[0].GetAttachedForceDetector() != null)
			{
				_activeInheritedDetector = _detectableFields[0].GetAttachedForceDetector();
			}
		}
		if (_inheritDetector != null)
		{
			_activeInheritedDetector = _inheritDetector;
		}
	}

	protected override void SetupColliders()
	{
	}

	public void ClearAllFields()
	{
		_activeVolumes.Clear();
		_activeInheritedDetector = null;
	}

	public void AddConstantVolume(ForceVolume newField, bool inheritForceAcceleration = true, bool clearOtherFields = false)
	{
		if (clearOtherFields)
		{
			ClearAllFields();
		}
		_activeVolumes.Add(newField);
		if (inheritForceAcceleration && newField.GetAttachedForceDetector() != null)
		{
			_activeInheritedDetector = newField.GetAttachedForceDetector();
		}
	}

	public void AddConstantVolumeInEditor(ForceVolume newField)
	{
		Array.Resize(ref _detectableFields, _detectableFields.Length + 1);
		_detectableFields[_detectableFields.Length - 1] = newField;
	}

	public override void AddVolume(EffectVolume volume)
	{
	}

	public override void RemoveVolume(EffectVolume volume)
	{
	}
}
