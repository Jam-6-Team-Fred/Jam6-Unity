using System;
using UnityEngine;

[Serializable]
public class ReferenceFrame
{
	public delegate void ReferenceFrameDestroyedEvent();

	[SerializeField]
	private float _minSuitTargetDistance;

	[SerializeField]
	private float _maxTargetDistance;

	[SerializeField]
	private float _autopilotArrivalDistance;

	[SerializeField]
	private float _autoAlignmentDistance;

	[SerializeField]
	private bool _hideLandingModePrompt;

	[Space(10f)]
	[SerializeField]
	private bool _matchAngularVelocity;

	[SerializeField]
	private float _minMatchAngularVelocityDistance;

	[SerializeField]
	private float _maxMatchAngularVelocityDistance;

	[Space(10f)]
	[SerializeField]
	private float _bracketsRadius;

	private AstroObject _attachedAstroObject;

	private OWRigidbody _attachedOWRigidbody;

	private Vector3 _localPosition;

	private bool _useCenterOfMass;

	public event ReferenceFrameDestroyedEvent OnReferenceFrameDestroyed;

	public ReferenceFrame(OWRigidbody attachedOWRigidbody)
	{
		_attachedOWRigidbody = attachedOWRigidbody;
		_attachedAstroObject = attachedOWRigidbody.GetComponent<AstroObject>();
		_useCenterOfMass = true;
	}

	public void Init(OWRigidbody attachedOWRigidbody, Vector3 worldPosition)
	{
		_attachedOWRigidbody = attachedOWRigidbody;
		_attachedAstroObject = attachedOWRigidbody.GetComponent<AstroObject>();
		_localPosition = attachedOWRigidbody.transform.InverseTransformPoint(worldPosition);
	}

	public void Destroy()
	{
		if (this.OnReferenceFrameDestroyed != null)
		{
			this.OnReferenceFrameDestroyed();
		}
	}

	public OWRigidbody GetOWRigidBody()
	{
		return _attachedOWRigidbody;
	}

	public AstroObject GetAstroObject()
	{
		return _attachedAstroObject;
	}

	public string GetHUDDisplayName()
	{
		if (_attachedAstroObject != null)
		{
			AstroObject.Name astroObjectName = _attachedAstroObject.GetAstroObjectName();
			if ((uint)(astroObjectName - 2) <= 7u || (uint)(astroObjectName - 15) <= 1u)
			{
				return AstroObject.AstroObjectNameToString(_attachedAstroObject.GetAstroObjectName());
			}
			return "";
		}
		return "";
	}

	public Vector3 GetPosition()
	{
		if (_useCenterOfMass)
		{
			return _attachedOWRigidbody.GetWorldCenterOfMass();
		}
		return _attachedOWRigidbody.transform.TransformPoint(_localPosition);
	}

	public Vector3 GetVelocity()
	{
		if (!_useCenterOfMass)
		{
			return _attachedOWRigidbody.GetPointVelocity(_attachedOWRigidbody.transform.TransformPoint(_localPosition));
		}
		return _attachedOWRigidbody.GetVelocity();
	}

	public Vector3 GetPointVelocity(Vector3 worldPoint)
	{
		return _attachedOWRigidbody.GetPointVelocity(worldPoint);
	}

	public Vector3 GetAcceleration()
	{
		if (!_useCenterOfMass)
		{
			return _attachedOWRigidbody.GetPointAcceleration(_attachedOWRigidbody.transform.TransformPoint(_localPosition));
		}
		return _attachedOWRigidbody.GetAcceleration();
	}

	public float GetOrbitSpeed(float orbitRadius)
	{
		if (_attachedOWRigidbody.GetAttachedGravityVolume() != null)
		{
			return Mathf.Sqrt(_attachedOWRigidbody.GetAttachedGravityVolume().CalculateGravityMagnitude(orbitRadius) * orbitRadius);
		}
		return 0f;
	}

	public bool GetHideLandingModePrompt()
	{
		return _hideLandingModePrompt;
	}

	public bool GetAllowAutopilot()
	{
		return _autopilotArrivalDistance > 0f;
	}

	public bool GetAllowAutoAlignment()
	{
		return _autoAlignmentDistance > 0f;
	}

	public bool GetAllowMatchAngularVelocity(float distance)
	{
		if (_matchAngularVelocity && distance > _minMatchAngularVelocityDistance)
		{
			return distance < _maxMatchAngularVelocityDistance;
		}
		return false;
	}

	public float GetAutopilotArrivalDistance()
	{
		return _autopilotArrivalDistance;
	}

	public float GetAutoAlignmentDistance()
	{
		return _autoAlignmentDistance;
	}

	public float GetMinSuitTargetDistance()
	{
		return _minSuitTargetDistance;
	}

	public float GetMaxTargetDistance()
	{
		return _maxTargetDistance;
	}

	public void SetMaxTargetDistance(float distance)
	{
		_maxTargetDistance = distance;
	}

	public bool GetMatchAngularVelocity()
	{
		return _matchAngularVelocity;
	}

	public float GetMinMatchAngularVelocityDistance()
	{
		return _minMatchAngularVelocityDistance;
	}

	public float GetMaxMatchAngularVelocityDistance()
	{
		return _maxMatchAngularVelocityDistance;
	}

	public float GetBracketsRadius()
	{
		return _bracketsRadius;
	}

	public void SetBracketsRadius(float radius)
	{
		_bracketsRadius = radius;
	}
}
