using System.Collections.Generic;
using UnityEngine;

public class TapeMeasure : MonoBehaviour
{
	[SerializeField]
	private Transform _toTransform;

	[SerializeField]
	private bool _printDistanceFromTarget = true;

	[SerializeField]
	private bool _printDeltaFromStart;

	[SerializeField]
	private bool _printRateOfChange;

	[SerializeField]
	private bool _useCentersOfMass = true;

	[SerializeField]
	private bool _recordPositions;

	[SerializeField]
	private float _positionRecordingInterval = 10f;

	[SerializeField]
	private bool _drawPositionTrail;

	private OWRigidbody _toBody;

	private OWRigidbody _thisBody;

	private float _firstDist = -1f;

	private float _lastDist;

	private List<Vector3> _positionHistory;

	private float _lastPositionRecordTime;

	private void Awake()
	{
		_thisBody = this.GetRequiredComponent<OWRigidbody>();
		if (_toTransform != null)
		{
			_toBody = _toTransform.GetRequiredComponent<OWRigidbody>();
		}
		_positionHistory = new List<Vector3>();
	}

	private void FixedUpdate()
	{
		if (_toTransform == null)
		{
			return;
		}
		float num = 0f;
		num = (_useCentersOfMass ? (_toBody.GetWorldCenterOfMass() - _thisBody.GetWorldCenterOfMass()).magnitude : (base.transform.position - _toTransform.position).magnitude);
		if (_firstDist < 0f)
		{
			_firstDist = num;
		}
		float num2 = (num - _lastDist) / Time.fixedDeltaTime;
		string text = "";
		if (_printDistanceFromTarget)
		{
			text = text + "Distance to " + _toBody.name + ": " + num + "m;  ";
		}
		if (_printDeltaFromStart)
		{
			text = text + "Delta from start distance: " + (num - _firstDist) + "m;  ";
		}
		if (_printRateOfChange)
		{
			text = text + "Rate of change: " + num2 + "m/s;";
		}
		if (_printDistanceFromTarget || _printDeltaFromStart || _printRateOfChange)
		{
			Debug.Log(text);
		}
		_lastDist = num;
		if (_recordPositions)
		{
			float num3 = Time.time - _lastPositionRecordTime;
			if (num3 >= _positionRecordingInterval || num3 == 0f)
			{
				if (_useCentersOfMass)
				{
					_positionHistory.Add(_thisBody.GetWorldCenterOfMass());
				}
				else
				{
					_positionHistory.Add(base.transform.position);
				}
				_lastPositionRecordTime = Time.time;
			}
		}
		if (_drawPositionTrail && _positionHistory.Count > 0)
		{
			for (int i = 0; i < _positionHistory.Count - 1; i++)
			{
				Debug.DrawLine(_positionHistory[i], _positionHistory[i + 1], Color.red);
			}
		}
	}
}
