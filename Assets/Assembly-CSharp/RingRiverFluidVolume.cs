using System;
using System.Collections.Generic;
using UnityEngine;

public class RingRiverFluidVolume : FluidVolume
{
	private const float MAX_MARKER_DEGREES = 45f;

	[Space]
	[SerializeField]
	private bool _debugIgnoreCurrents;

	[SerializeField]
	private bool _updateMarkers;

	[Space]
	[SerializeField]
	private float _defaultSpeed;

	[SerializeField]
	private float _densityForPlayer = 60f;

	[SerializeField]
	private float _buoyancyDensity = 1.1f;

	[Space]
	[SerializeField]
	private GameObject _preFloodFlowMarkers;

	[SerializeField]
	private GameObject _postFloodFlowMarkers;

	[Space]
	[SerializeField]
	private GameObject _undertowVolumesRoot;

	private RiverMarkerInfo[] _closestMarkers = new RiverMarkerInfo[4];

	[SerializeField]
	[HideInInspector]
	private bool _flowMarkersCached;

	[SerializeField]
	[HideInInspector]
	private RiverMarkerGroup _preFloodMarkers;

	[SerializeField]
	[HideInInspector]
	private RiverMarkerGroup _postFloodMarkers;

	private OWRingRiverCollider _collider;

	private RingRiverUndertowVolume[] _undertowVolumes;

	private List<RingRiverCalmVolume> _calmVolumes;

	private FluidDetector _playerFluidDetector;

	private bool _playerInUndertow;

	private bool _playerPinnedByUndertow;

	private float _exitDreamTime = float.NegativeInfinity;

	private void OnValidate()
	{
		if (_updateMarkers)
		{
			_updateMarkers = false;
			UpdateMarkers();
		}
	}

	public void UpdateMarkers()
	{
		if (_collider == null)
		{
			_collider = GetComponent<OWRingRiverCollider>();
		}
		RiverFlowMarker[] componentsInChildren = _preFloodFlowMarkers.GetComponentsInChildren<RiverFlowMarker>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			_collider.SnapTransformToInnerRadius(componentsInChildren[i].transform, 0f);
			componentsInChildren[i].postFlood = false;
		}
		RiverFlowMarker[] componentsInChildren2 = _postFloodFlowMarkers.GetComponentsInChildren<RiverFlowMarker>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			_collider.SnapTransformToInnerRadius(componentsInChildren2[j].transform, 1f);
			componentsInChildren2[j].postFlood = true;
		}
	}

	public void DebugDrawFlowMarkers()
	{
		if (_collider == null)
		{
			_collider = GetComponent<OWRingRiverCollider>();
		}
		RiverFlowMarker[] componentsInChildren = ((_collider.GetFloodLerp() > 0f) ? _postFloodFlowMarkers : _preFloodFlowMarkers).GetComponentsInChildren<RiverFlowMarker>();
		if (componentsInChildren == null)
		{
			return;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] != null)
			{
				componentsInChildren[i].DrawLine();
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Locator.RegisterRingRiverFluidVolume(this);
		_collider = GetComponent<OWRingRiverCollider>();
		if (!_flowMarkersCached)
		{
			CacheFloodMarkers();
			_flowMarkersCached = true;
		}
		if (_undertowVolumesRoot == null)
		{
			_undertowVolumesRoot = base.gameObject;
		}
		_undertowVolumes = _undertowVolumesRoot.GetComponentsInChildren<RingRiverUndertowVolume>();
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void CacheFloodMarkers()
	{
		_preFloodMarkers = new RiverMarkerGroup(_preFloodFlowMarkers.GetComponentsInChildren<RiverFlowMarker>(), base.transform, _collider);
		_postFloodMarkers = new RiverMarkerGroup(_postFloodFlowMarkers.GetComponentsInChildren<RiverFlowMarker>(), base.transform, _collider);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_playerFluidDetector != null)
		{
			_playerFluidDetector.OnExitFluid -= OnPlayerExitFluid;
		}
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
	}

	public void RegisterCalmVolume(RingRiverCalmVolume calmVolume)
	{
		if (_calmVolumes == null)
		{
			_calmVolumes = new List<RingRiverCalmVolume>();
		}
		_calmVolumes.Add(calmVolume);
	}

	public bool IsPlayerInUndertow()
	{
		return _playerInUndertow;
	}

	public bool IsPlayerPinnedByUndertow()
	{
		return _playerPinnedByUndertow;
	}

	public OWRingRiverCollider GetCollider()
	{
		return _collider;
	}

	public override Vector3 GetImpactVelocity(OWRigidbody impactingBody)
	{
		if (Time.time - _exitDreamTime < 6f)
		{
			return Vector3.zero;
		}
		return impactingBody.GetVelocity() - _attachedBody.GetPointVelocity(impactingBody.GetPosition());
	}

	public override Vector3 GetSplashAlignment(Vector3 impactPos, Vector3 impactDir)
	{
		return Vector3.ProjectOnPlane(base.transform.position - impactPos, base.transform.up);
	}

	public override bool PreventPlayerGrounded()
	{
		Vector3 localPosition = base.transform.InverseTransformPoint(Locator.GetPlayerDetector().transform.position);
		if (!PlayerState.IsCameraUnderwater())
		{
			return _collider.GetWaveSpeedFraction(localPosition) > 0f;
		}
		return true;
	}

	public void CalcFlowFromClosestMarkers(Vector3 worldPosition, out Vector3 flowDirection, out float flowSpeed)
	{
		RiverMarkerGroup riverMarkerGroup = ((!_collider.HasFloodReachedPosition(worldPosition)) ? _preFloodMarkers : _postFloodMarkers);
		flowDirection = Vector3.zero;
		flowSpeed = 0f;
		if (riverMarkerGroup.Count > 0)
		{
			Vector3 vector = base.transform.InverseTransformPoint(worldPosition);
			float num = _collider.LocalPositionToDegrees(vector);
			for (int i = 0; i < _closestMarkers.Length; i++)
			{
				_closestMarkers[i].Set(-1, 0f);
			}
			int count = riverMarkerGroup.Count;
			for (int j = 0; j < count; j++)
			{
				float num2 = num - riverMarkerGroup.degrees[j];
				num2 = ((num2 < 0f) ? (0f - num2) : num2);
				if (num2 > 180f)
				{
					num2 = 360f - num2;
				}
				if (num2 > 45f)
				{
					continue;
				}
				float sqrMagnitude = (riverMarkerGroup.localPositions[j] - vector).sqrMagnitude;
				for (int k = 0; k < _closestMarkers.Length; k++)
				{
					if (_closestMarkers[k].markerIndex == -1)
					{
						_closestMarkers[k].Set(j, sqrMagnitude);
						break;
					}
					if (_closestMarkers[k].dist > sqrMagnitude)
					{
						RiverMarkerInfo riverMarkerInfo = _closestMarkers[k];
						_closestMarkers[k].Set(j, sqrMagnitude);
						for (int l = k + 1; l < _closestMarkers.Length; l++)
						{
							RiverMarkerInfo riverMarkerInfo2 = _closestMarkers[l];
							_closestMarkers[l] = riverMarkerInfo;
							riverMarkerInfo = riverMarkerInfo2;
						}
						break;
					}
				}
			}
			for (int m = 0; m < _closestMarkers.Length; m++)
			{
				if (_closestMarkers[m].markerIndex != -1)
				{
					_closestMarkers[m].dist = Mathf.Sqrt(_closestMarkers[m].dist);
				}
			}
			float num3 = 0f;
			for (int n = 0; n < _closestMarkers.Length; n++)
			{
				num3 += _closestMarkers[n].dist;
			}
			float num4 = 0f;
			for (int num5 = 0; num5 < _closestMarkers.Length && _closestMarkers[num5].markerIndex != -1; num5++)
			{
				float num6 = num3 / (_closestMarkers[num5].dist * _closestMarkers[num5].dist);
				num4 += num6;
				flowDirection += -riverMarkerGroup.localRightDirs[_closestMarkers[num5].markerIndex] * num6;
			}
			flowDirection = base.transform.TransformDirection(flowDirection);
			for (int num7 = 0; num7 < _closestMarkers.Length && _closestMarkers[num7].markerIndex != -1; num7++)
			{
				float num8 = num3 / (_closestMarkers[num7].dist * _closestMarkers[num7].dist);
				flowSpeed += riverMarkerGroup.magnitudes[_closestMarkers[num7].markerIndex] * num8 / num4;
			}
		}
		else
		{
			flowDirection = base.transform.up;
			flowSpeed = _defaultSpeed;
		}
		Vector3 rhs = Vector3.ProjectOnPlane(worldPosition - base.transform.position, base.transform.up);
		flowDirection = Vector3.Cross(flowDirection, rhs).normalized;
	}

	public Vector3 GetPointFlowOnlyVelocity(Vector3 worldPosition)
	{
		CalcFlowFromClosestMarkers(worldPosition, out var flowDirection, out var flowSpeed);
		return flowDirection * flowSpeed;
	}

	public override Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		CalcFlowFromClosestMarkers(worldPosition, out var flowDirection, out var flowSpeed);
		Vector3 vector = Vector3.zero;
		if (detector != null)
		{
			Vector3 localPosition = base.transform.InverseTransformPoint(worldPosition);
			float waveSpeedFraction = _collider.GetWaveSpeedFraction(localPosition, detector.CompareName(Detector.Name.Player));
			if (waveSpeedFraction > 0f)
			{
				float num = Mathf.Sqrt(localPosition.x * localPosition.x + localPosition.z * localPosition.z);
				float num2 = 2f * num * (float)Math.PI / 60f;
				flowDirection = Vector3.Cross(rhs: Vector3.ProjectOnPlane(worldPosition - base.transform.position, base.transform.up), lhs: base.transform.up).normalized;
				flowSpeed = num2 * waveSpeedFraction;
				if (detector.AffectsRumble())
				{
					float num3 = (detector.CompareName(Detector.Name.Player) ? 0.7f : 0.35f);
					RumbleManager.AddFluidRumble(_fluidType, waveSpeedFraction * num3);
				}
			}
			else
			{
				float downhillSpeed = _collider.GetDownhillSpeed(localPosition, detector.CompareName(Detector.Name.Player));
				if (downhillSpeed > 0f)
				{
					Vector3 rhs2 = Vector3.ProjectOnPlane(worldPosition - base.transform.position, base.transform.up);
					Vector3 normalized = Vector3.Cross(base.transform.up, rhs2).normalized;
					vector = downhillSpeed * normalized;
				}
			}
		}
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(worldPosition);
		Vector3 vector2 = flowDirection * flowSpeed;
		if (detector.CompareNameMask(Detector.Name.Player | Detector.Name.Probe))
		{
			bool flag = detector.CompareName(Detector.Name.Player);
			if (flag)
			{
				_playerInUndertow = false;
				_playerPinnedByUndertow = false;
			}
			if (IsInCalmVolume(detector.GetName()))
			{
				return pointVelocity;
			}
			for (int i = 0; i < _undertowVolumes.Length; i++)
			{
				if (!_undertowVolumes[i].ContainsDetector(detector.GetName()))
				{
					continue;
				}
				if (flag)
				{
					if (PlayerState.IsRidingRaft())
					{
						break;
					}
					_playerInUndertow = true;
					RumbleManager.AddFluidRumble(_fluidType, 0.5f);
				}
				Vector3 vector3 = Vector3.ProjectOnPlane(worldPosition - base.transform.position, base.transform.up);
				Vector3 localPosition2 = base.transform.InverseTransformPoint(worldPosition);
				float innerRadiusAtLocalPosition = _collider.GetInnerRadiusAtLocalPosition(localPosition2);
				float num4 = vector3.magnitude - innerRadiusAtLocalPosition;
				float num5 = _undertowVolumes[i].depth - num4;
				Vector3 vector4 = vector3.normalized * num5 * 2f;
				if (Vector3.Project(detector.GetAttachedOWRigidbody().GetVelocity() - _attachedBody.GetPointVelocity(worldPosition), flowDirection).sqrMagnitude > 1f)
				{
					vector2 = flowDirection * _undertowVolumes[i].speed;
					vector2 += vector4;
				}
				else if (flag)
				{
					_playerPinnedByUndertow = true;
				}
			}
		}
		return pointVelocity + vector2 + vector;
	}

	public bool IsInCalmVolume(Detector.Name name)
	{
		for (int i = 0; i < _calmVolumes.Count; i++)
		{
			if (_calmVolumes[i].ContainsDetector(name))
			{
				return true;
			}
		}
		return false;
	}

	public override float GetPointDensity(Vector3 worldPosition, FluidDetector detector)
	{
		if (!detector.CompareName(Detector.Name.Player))
		{
			return _density;
		}
		return _densityForPlayer;
	}

	public override float GetFractionSubmerged(FluidDetector detector)
	{
		Vector3 localPosition = base.transform.InverseTransformPoint(detector.transform.position);
		float num = Mathf.Sqrt(localPosition.x * localPosition.x + localPosition.z * localPosition.z);
		return detector.GetBuoyancyData().CalculateSubmergedFraction(0f - num, 0f - _collider.GetInnerRadiusAtLocalPosition(localPosition));
	}

	public override Vector3 GetBuoyancy(FluidDetector detector, float fractionSubmerged)
	{
		if (detector.GetAttachedOWRigidbody().GetAttachedForceDetector() != null)
		{
			Vector3 vector = detector.GetAttachedOWRigidbody().GetAttachedForceDetector().GetForceAcceleration() - _attachedBody.GetAttachedForceDetector().GetForceAcceleration();
			return Vector3.Project(onNormal: -Vector3.ProjectOnPlane(detector.transform.position - base.transform.position, base.transform.up), vector: -vector) * fractionSubmerged * _buoyancyDensity / detector.GetBuoyancyData().density;
		}
		return Vector3.zero;
	}

	public override float GetDepthAtPosition(Vector3 worldPosition)
	{
		Vector3 localPosition = _collider.transform.InverseTransformPoint(worldPosition);
		float num = Mathf.Sqrt(localPosition.x * localPosition.x + localPosition.z * localPosition.z);
		float innerRadiusAtLocalPosition = _collider.GetInnerRadiusAtLocalPosition(localPosition);
		return num - innerRadiusAtLocalPosition;
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		FluidDetector component = hitObj.GetComponent<FluidDetector>();
		if (component != null)
		{
			component.AddVolume(this);
			if (component.CompareName(Detector.Name.Player))
			{
				_playerFluidDetector = component;
				component.OnExitFluid += OnPlayerExitFluid;
			}
		}
	}

	private void OnPlayerExitFluid(FluidVolume volume)
	{
		if (volume == this)
		{
			_playerInUndertow = false;
			_playerPinnedByUndertow = false;
			_playerFluidDetector.OnExitFluid -= OnPlayerExitFluid;
		}
	}

	private void OnExitDreamWorld()
	{
		_exitDreamTime = Time.time;
	}
}
