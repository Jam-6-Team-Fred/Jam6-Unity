using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class NomaiWarpPlatform : MonoBehaviour
{
	public enum Frequency
	{
		None = 0,
		SunStation = 1,
		TimeLoop = 2,
		HourglassTwin = 3,
		TimberHearth = 4,
		BrittleHollowPolar = 5,
		BrittleHollowForge = 6,
		GiantsDeep = 7,
		Vessel = 8
	}

	public delegate void WarpEvent(OWRigidbody warpedBody, NomaiWarpPlatform startPlatform, NomaiWarpPlatform targetPlatform);

	[SerializeField]
	private Frequency _frequency;

	[SerializeField]
	private Transform _platformCenter;

	[Space]
	[SerializeField]
	private Vector3 _localWarpPosition = Vector3.zero;

	[SerializeField]
	private float _warpRadius = 0.5f;

	[SerializeField]
	private bool _ignoreRelativeRotation;

	[Space]
	[SerializeField]
	private SingularityController _blackHole;

	[SerializeField]
	private SingularityController _whiteHole;

	protected List<OWRigidbody> _objectsOnPlatform;

	private OWTriggerVolume _triggerVolume;

	private OWRigidbody _owRigidbody;

	private NomaiWarpPlatform _linkedPlatform;

	private float _openBlackHoleTime;

	private bool _isPlayerOnPlatform;

	private bool _isProbeOnPlatform;

	private bool _keepBlackHoleOpen;

	public event WarpEvent OnReceiveWarpedBody;

	protected virtual void Awake()
	{
		if (_platformCenter == null)
		{
			_platformCenter = base.transform;
		}
		_triggerVolume = GetComponent<OWTriggerVolume>();
		_triggerVolume.OnEntry += OnEntry;
		_triggerVolume.OnExit += OnExit;
		_owRigidbody = _platformCenter.GetAttachedOWRigidbody();
		_objectsOnPlatform = new List<OWRigidbody>(4);
		_linkedPlatform = null;
		_blackHole.OnCollapse += OnBlackHoleCollapse;
		_whiteHole.OnCollapse += OnWhiteHoleCollapse;
	}

	protected virtual void OnDestroy()
	{
		_blackHole.OnCollapse -= OnBlackHoleCollapse;
		_whiteHole.OnCollapse -= OnWhiteHoleCollapse;
		_triggerVolume.OnEntry -= OnEntry;
		_triggerVolume.OnExit -= OnExit;
	}

	protected virtual void Start()
	{
		base.enabled = false;
	}

	public Transform GetPlatformCenter()
	{
		return _platformCenter;
	}

	public bool IsPlayerOnPlatform()
	{
		return _isPlayerOnPlatform;
	}

	public bool IsProbeOnPlatform()
	{
		return _isProbeOnPlatform;
	}

	public bool IsBlackHoleOpen()
	{
		return _blackHole.GetState() != SingularityController.State.Collapsed;
	}

	public Frequency GetFrequency()
	{
		return _frequency;
	}

	public void OpenBlackHole(NomaiWarpPlatform linkedPlatform, bool stayOpen = false)
	{
		if (!IsBlackHoleOpen())
		{
			_openBlackHoleTime = Time.time;
			_linkedPlatform = linkedPlatform;
			_blackHole.Create();
			linkedPlatform.OpenWhiteHole();
			_keepBlackHoleOpen = stayOpen;
			base.enabled = true;
		}
	}

	public void CloseBlackHole()
	{
		_blackHole.Collapse();
		if (_linkedPlatform != null)
		{
			_linkedPlatform.CloseWhiteHole();
		}
	}

	protected virtual void FixedUpdate()
	{
		UpdateEnabled();
		if (!IsBlackHoleOpen())
		{
			return;
		}
		Vector3 vector = base.transform.TransformPoint(_localWarpPosition);
		if (_isProbeOnPlatform)
		{
			if (Locator.GetProbe().IsAnchored())
			{
				Locator.GetProbe().Unanchor();
			}
			if (Locator.GetProbe().GetDetectorObject().GetComponent<ForceApplier>()
				.GetApplyFluids())
			{
				Locator.GetProbe().GetDetectorObject().GetComponent<ForceApplier>()
					.SetApplyFluids(applyFluids: false);
			}
		}
		for (int i = 0; i < _objectsOnPlatform.Count; i++)
		{
			Vector3 vector2 = vector - _objectsOnPlatform[i].GetPosition();
			Vector3 vector3 = _objectsOnPlatform[i].GetVelocity() - _owRigidbody.GetPointVelocity(vector);
			Vector3 vector4 = vector2 * 5f;
			Vector3 vector5 = vector4 - vector3;
			Vector3 vector6 = Vector3.MoveTowards(vector3, vector4, Time.deltaTime * vector5.magnitude * 10f);
			_objectsOnPlatform[i].AddVelocityChange(vector6 - vector3);
			if (_objectsOnPlatform[i].CompareTag("Player"))
			{
				Locator.GetPlayerController().SetInWarpField(inWarpField: true);
			}
		}
		if (!_keepBlackHoleOpen && Time.time > _openBlackHoleTime + 3f && _blackHole.GetState() != SingularityController.State.Collapsing)
		{
			CloseBlackHole();
		}
		if (_blackHole.GetState() == SingularityController.State.Creating)
		{
			return;
		}
		for (int j = 0; j < _objectsOnPlatform.Count; j++)
		{
			if (!(_objectsOnPlatform[j] == null) && Vector3.SqrMagnitude(_objectsOnPlatform[j].GetWorldCenterOfMass() - vector) < _warpRadius * _warpRadius)
			{
				TransmitWarpedBody(_objectsOnPlatform[j]);
			}
		}
	}

	protected virtual void UpdateEnabled()
	{
		if (_objectsOnPlatform.Count == 0 && _blackHole.GetState() == SingularityController.State.Collapsed)
		{
			base.enabled = false;
		}
	}

	protected virtual void OnEntry(GameObject hitObj)
	{
		OWRigidbody requiredComponent = hitObj.GetAttachedOWRigidbody().GetRequiredComponent<OWRigidbody>();
		if (!(requiredComponent == _owRigidbody) && !_objectsOnPlatform.Contains(requiredComponent))
		{
			_objectsOnPlatform.Add(requiredComponent);
			base.enabled = true;
			if (requiredComponent.CompareTag("Player"))
			{
				_isPlayerOnPlatform = true;
			}
			else if (requiredComponent.CompareTag("Probe"))
			{
				_isProbeOnPlatform = true;
			}
		}
	}

	protected virtual void OnExit(GameObject hitObj)
	{
		OWRigidbody componentInParent = hitObj.GetComponentInParent<OWRigidbody>();
		if (!(componentInParent == _owRigidbody))
		{
			_objectsOnPlatform.QuickRemove(componentInParent);
			if (componentInParent.CompareTag("Player"))
			{
				_isPlayerOnPlatform = false;
				Locator.GetPlayerController().SetInWarpField(inWarpField: false);
			}
			else if (componentInParent.CompareTag("Probe"))
			{
				_isProbeOnPlatform = false;
				Locator.GetProbe().GetDetectorObject().GetComponent<ForceApplier>()
					.SetApplyFluids(applyFluids: true);
			}
		}
	}

	protected virtual void OnReceivePlayer(NomaiWarpPlatform startPlatform)
	{
	}

	protected virtual void ReceiveWarpedBody(OWRigidbody body, NomaiWarpPlatform startPlatform)
	{
		Vector3 position = startPlatform.GetPlatformCenter().InverseTransformPoint(body.GetPosition());
		Quaternion quaternion = startPlatform.GetPlatformCenter().InverseTransformRotation(body.GetRotation());
		if (_ignoreRelativeRotation)
		{
			position.x = 0f;
			position.z = -0.5f;
		}
		Vector3 vector = _platformCenter.TransformPoint(position);
		Quaternion worldRotation = (_ignoreRelativeRotation ? _platformCenter.rotation : (_platformCenter.rotation * quaternion));
		body.WarpToPositionRotation(vector, worldRotation);
		body.SetVelocity(_owRigidbody.GetPointVelocity(vector));
		bool flag = body.CompareTag("Player");
		_whiteHole.PlayExitAudio(flag);
		if (flag && _frequency == Frequency.Vessel && Locator.GetEyeStateManager() != null)
		{
			Locator.GetEyeStateManager().SetState(EyeState.WarpedToSurface);
		}
		if (body.CompareTag("Probe"))
		{
			Locator.GetProbe().GetDetectorObject().GetComponent<ForceApplier>()
				.SetApplyFluids(applyFluids: true);
		}
		if (this.OnReceiveWarpedBody != null)
		{
			this.OnReceiveWarpedBody(body, startPlatform, this);
		}
	}

	private void TransmitWarpedBody(OWRigidbody body)
	{
		MonoBehaviour.print("TRANSMIT WARPED BODY: " + body.gameObject.name + " TO " + _linkedPlatform.GetFrequency());
		_blackHole.PlayEntryAudio(body.CompareTag("Player"));
		_linkedPlatform.ReceiveWarpedBody(body, this);
		CloseBlackHole();
	}

	private void OnBlackHoleCollapse()
	{
		_linkedPlatform = null;
		if (_isPlayerOnPlatform)
		{
			Locator.GetPlayerController().SetInWarpField(inWarpField: false);
		}
	}

	private void OpenWhiteHole()
	{
		_whiteHole.Create();
	}

	private void CloseWhiteHole()
	{
		_whiteHole.Collapse();
	}

	private void OnWhiteHoleCollapse()
	{
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.green;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireSphere(_localWarpPosition, _warpRadius);
		}
	}
}
