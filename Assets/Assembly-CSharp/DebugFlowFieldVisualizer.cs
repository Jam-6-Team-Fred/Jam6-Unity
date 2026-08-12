using System;
using UnityEngine;

public class DebugFlowFieldVisualizer : MonoBehaviour
{
	[SerializeField]
	private bool _drawFlowVectors;

	[SerializeField]
	private bool _drawFlowMarkers;

	[SerializeField]
	private int _xResolution = 100;

	[SerializeField]
	private int _yResolution = 30;

	[SerializeField]
	private float _velocityScale = 2f;

	[Space]
	[SerializeField]
	private bool _updateFlowVectors;

	[SerializeField]
	private bool _debugToggleFlood;

	[Space]
	[SerializeField]
	private RingRiverFluidVolume _riverFluid;

	[SerializeField]
	private OWRingRiverCollider _riverCollider;

	[SerializeField]
	private RingRiverController _riverController;

	[SerializeField]
	private DebugFlowVector[,] _debugFlowVectors;

	private void OnValidate()
	{
		if (_velocityScale < 0f)
		{
			_velocityScale = 0f;
		}
		if (_updateFlowVectors)
		{
			_updateFlowVectors = false;
			if (_riverFluid == null || _riverCollider == null)
			{
				return;
			}
			_riverFluid.UpdateMarkers();
			_debugFlowVectors = new DebugFlowVector[_xResolution, _yResolution];
			for (int i = 0; i < _xResolution; i++)
			{
				for (int j = 0; j < _yResolution; j++)
				{
					_debugFlowVectors[i, j] = default(DebugFlowVector);
					float y = (0.5f - ((float)j + 0.5f) / (float)_yResolution) * 300f;
					float num = (float)i / (float)_xResolution * 360f;
					float num2 = _riverCollider.GetInnerRadiusAtDegrees(num) - 0.2f;
					float z = Mathf.Cos((float)Math.PI / 180f * num) * num2;
					float x = Mathf.Sin((float)Math.PI / 180f * num) * num2;
					Vector3 vector = new Vector3(x, y, z);
					Vector3 worldPosition = _riverFluid.transform.TransformPoint(vector);
					_riverFluid.CalcFlowFromClosestMarkers(worldPosition, out var flowDirection, out var flowSpeed);
					_debugFlowVectors[i, j].localPosition = vector;
					_debugFlowVectors[i, j].localVelocity = _riverFluid.transform.InverseTransformDirection(flowDirection) * flowSpeed;
				}
			}
		}
		if (_debugToggleFlood)
		{
			_debugToggleFlood = false;
			_riverController.DebugToggleFlood();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_drawFlowMarkers && _riverFluid != null)
		{
			_riverFluid.DebugDrawFlowMarkers();
		}
	}

	private void OnDrawGizmos()
	{
		if (_riverFluid == null || _riverCollider == null || !_drawFlowVectors || _debugFlowVectors == null || _debugFlowVectors.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < _debugFlowVectors.GetLength(0); i++)
		{
			for (int j = 0; j < _debugFlowVectors.GetLength(1); j++)
			{
				_debugFlowVectors[i, j].Draw(_riverFluid.transform, _velocityScale);
			}
		}
	}
}
