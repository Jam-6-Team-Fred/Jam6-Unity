using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NomaiExperimentBlackHole : MonoBehaviour
{
	[SerializeField]
	private NomaiExperimentWhiteHole _whiteHole;

	[SerializeField]
	private SurveyorProbe _duplicateProbe;

	private OWRigidbody _attachedOWRigidbody;

	private SingularityController _singularityController;

	private GravityVolume _gravityVolume;

	private SurveyorProbe _trackedProbe;

	private ProbeHUDMarker _trackedProbeHUDMarker;

	private OWRigidbody _trackedPlayerBody;

	private bool _duplicateActive;

	private ProbeHUDMarker _duplicateProbeHUDMarker;

	private bool _timeTravel;

	private void Awake()
	{
		_duplicateProbeHUDMarker = _duplicateProbe.GetComponentInChildren<ProbeHUDMarker>();
		_attachedOWRigidbody = this.GetAttachedOWRigidbody();
		_singularityController = GetComponentInChildren<SingularityController>();
		_singularityController.OnCollapse += OnSingularityCollapse;
	}

	private void Start()
	{
		TimelineObliterationController.SetParadoxExperimentProbeActive(value: false);
	}

	private void OnDestroy()
	{
		_singularityController.OnCollapse -= OnSingularityCollapse;
	}

	public void OpenSingularity()
	{
		_singularityController.Create();
		base.enabled = true;
	}

	public void CloseSingularity()
	{
		_singularityController.Collapse();
	}

	public void SetTimeTravel(bool timeTravel)
	{
		_timeTravel = timeTravel;
	}

	private bool IsSingularityOpen()
	{
		return _singularityController.GetState() != SingularityController.State.Collapsed;
	}

	private void FixedUpdate()
	{
		if (_trackedPlayerBody != null && (_trackedPlayerBody.GetPosition() - base.transform.position).sqrMagnitude < 1f)
		{
			_trackedPlayerBody.SetPosition(_whiteHole.transform.position);
			_singularityController.PlayEntryAudio(isPlayer: true);
			_whiteHole.PlayExitAudio(isPlayer: true);
		}
	}

	private void EjectProbeFromWhiteHole(OWRigidbody probeBody, Vector3 relativeProbeVelocity)
	{
		probeBody.SetPosition(_whiteHole.transform.position);
		Vector3 vector = _whiteHole.transform.forward * 5f + _whiteHole.transform.up * 2f;
		probeBody.SetVelocity(_attachedOWRigidbody.GetPointVelocity(_whiteHole.transform.position) + vector);
		probeBody.SetRotation(Quaternion.FromToRotation(_whiteHole.transform.forward, vector) * _whiteHole.transform.rotation);
		MonoBehaviour.print("eject probe: " + Time.time);
		_whiteHole.PlayExitAudio();
	}

	private void OnSingularityCollapse()
	{
		if (_trackedProbe != null)
		{
			_trackedProbe.GetSeeking().StopSeeking();
			_trackedProbe.GetSeeking().OnSeekComplete -= OnOriginalSeekComplete;
		}
		if (_duplicateActive)
		{
			_duplicateProbe.GetSeeking().StopSeeking();
			_duplicateProbe.GetSeeking().OnSeekComplete -= OnDuplicateSeekComplete;
			PlayerData.SetLoopCountOnParadoxStart();
			Locator.GetTimelineObliterationController().BeginTimelineObliteration(TimelineObliterationController.ObliterationType.TIME_LOOP_EXPERIMENT, _trackedProbe);
			PlayerData.SetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE", state: false);
			PlayerData.SetPersistentCondition("PROBE_ENTERED_TIMELOOPCORE", state: false);
			if (PlayerData.GetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE_MULTIPLE"))
			{
				PlayerData.SetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE_MULTIPLE", state: false);
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (collider.GetAttachedOWRigidbody().CompareTag("Player"))
		{
			_trackedPlayerBody = collider.GetAttachedOWRigidbody();
		}
		else if (collider.GetAttachedOWRigidbody().CompareTag("Probe") && IsSingularityOpen() && _trackedProbe == null)
		{
			_trackedProbe = collider.GetAttachedOWRigidbody().GetComponent<SurveyorProbe>();
			_trackedProbeHUDMarker = _trackedProbe.GetComponentInChildren<ProbeHUDMarker>();
			_trackedProbe.OnRetrieveProbe += OnRetrieveTrackedProbe;
			if (_timeTravel)
			{
				OWRigidbody oWRigidbody = _trackedProbe.GetOWRigidbody();
				Vector3 relativeProbeVelocity = oWRigidbody.GetVelocity() - _attachedOWRigidbody.GetPointVelocity(base.transform.position);
				float seekLength = Vector3.Distance(base.transform.position, oWRigidbody.GetWorldCenterOfMass()) / relativeProbeVelocity.magnitude;
				_duplicateActive = true;
				TimelineObliterationController.SetParadoxExperimentProbeActive(_duplicateActive);
				SetMarkerStateDuplicated(_duplicateActive);
				_duplicateProbe.Launch(_trackedProbe.transform, oWRigidbody.GetVelocity());
				_duplicateProbe.GetSeeking().SeekTarget(base.transform, seekLength);
				_duplicateProbe.GetSeeking().OnSeekComplete += OnDuplicateSeekComplete;
				EjectProbeFromWhiteHole(oWRigidbody, relativeProbeVelocity);
			}
			else
			{
				_trackedProbe.GetSeeking().SeekTarget(base.transform, 1f);
				_trackedProbe.GetSeeking().OnSeekComplete += OnOriginalSeekComplete;
			}
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (collider.GetAttachedOWRigidbody().CompareTag("Player"))
		{
			_trackedPlayerBody = null;
		}
	}

	private void OnRetrieveTrackedProbe()
	{
		_trackedProbe.OnRetrieveProbe -= OnRetrieveTrackedProbe;
		_trackedProbe = null;
	}

	private void OnOriginalSeekComplete()
	{
		OWRigidbody oWRigidbody = _trackedProbe.GetOWRigidbody();
		Vector3 relativeProbeVelocity = oWRigidbody.GetVelocity() - _attachedOWRigidbody.GetPointVelocity(base.transform.position);
		_trackedProbe.GetSeeking().StopSeeking();
		_trackedProbe.GetSeeking().OnSeekComplete -= OnOriginalSeekComplete;
		_singularityController.PlayEntryAudio();
		EjectProbeFromWhiteHole(oWRigidbody, relativeProbeVelocity);
	}

	private void OnDuplicateSeekComplete()
	{
		_duplicateProbe.Deactivate();
		_duplicateProbe.GetSeeking().OnSeekComplete -= OnDuplicateSeekComplete;
		_duplicateActive = false;
		TimelineObliterationController.SetParadoxExperimentProbeActive(_duplicateActive);
		SetMarkerStateDuplicated(_duplicateActive);
		_singularityController.PlayEntryAudio();
	}

	private void SetMarkerStateDuplicated(bool value)
	{
		if (_trackedProbeHUDMarker != null)
		{
			_trackedProbeHUDMarker.MarkTLEDuplicatedState(value);
		}
		if (_duplicateProbeHUDMarker != null)
		{
			_duplicateProbeHUDMarker.MarkTLEDuplicatedState(value);
		}
	}
}
