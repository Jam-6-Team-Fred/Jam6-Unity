using UnityEngine;

[RequireComponent(typeof(SurveyorProbe))]
public class ProbeSeeking : MonoBehaviour
{
	public delegate void ProbeSeekEvent();

	private SurveyorProbe _probe;

	private OWRigidbody _probeBody;

	private OWRigidbody _parentBody;

	private Vector3 _localTargetPos;

	private Vector3 _localProjectedPos;

	private Vector3 _localVelocity;

	private Vector3 _instantWorldVelocity;

	private float _seekLength;

	private float _seekTimer;

	private bool _alignment;

	public event ProbeSeekEvent OnSeekComplete;

	private void Awake()
	{
		_probe = GetComponent<SurveyorProbe>();
		_probeBody = this.GetRequiredComponent<OWRigidbody>();
		_probe.OnRetrieveProbe += StopSeeking;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_probe.OnRetrieveProbe -= StopSeeking;
	}

	private void FixedUpdate()
	{
		if (_parentBody == null)
		{
			StopSeeking();
			return;
		}
		if (!_probeBody.IsKinematic())
		{
			_probeBody.MakeKinematic();
			Physics.SyncTransforms();
			Vector3 pointVelocity = _parentBody.GetPointVelocity(_parentBody.transform.TransformPoint(_localTargetPos));
			Vector3 direction = _probeBody.GetVelocity() - pointVelocity;
			_localVelocity = _parentBody.transform.InverseTransformDirection(direction);
		}
		_seekTimer += Time.deltaTime;
		_localProjectedPos += _localVelocity * Time.deltaTime;
		float num = Mathf.Clamp01(_seekTimer / _seekLength);
		Vector3 position = Vector3.Lerp(_localProjectedPos, _localTargetPos, num);
		Vector3 vector = _parentBody.transform.TransformPoint(position);
		Vector3 vector2 = (vector - _probeBody.GetPosition()) / Time.deltaTime;
		_instantWorldVelocity = _parentBody.GetVelocity() + vector2;
		_probeBody.MoveToPosition(vector);
		if (_alignment)
		{
			_probeBody.MoveToRotation(Quaternion.LookRotation(vector2, _probeBody.transform.up));
		}
		if (num == 1f)
		{
			StopSeeking();
			if (this.OnSeekComplete != null)
			{
				this.OnSeekComplete();
			}
		}
	}

	public void SeekTarget(Transform target, float seekLength, bool alignment = true)
	{
		OWRigidbody attachedOWRigidbody = target.GetAttachedOWRigidbody();
		Vector3 localTargetPos = attachedOWRigidbody.transform.InverseTransformPoint(target.position);
		SeekTarget(attachedOWRigidbody, localTargetPos, seekLength, alignment);
	}

	public void SeekTarget(OWRigidbody targetParentBody, Vector3 localTargetPos, float seekLength, bool alignment = true)
	{
		_parentBody = targetParentBody;
		_localTargetPos = localTargetPos;
		_localProjectedPos = _parentBody.transform.InverseTransformPoint(_probeBody.transform.position);
		_seekLength = seekLength;
		_seekTimer = 0f;
		_alignment = alignment;
		base.enabled = true;
	}

	public void StopSeeking()
	{
		if (IsSeeking())
		{
			_probeBody.MakeNonKinematic();
			_probeBody.SetVelocity(_instantWorldVelocity);
			_parentBody = null;
			base.enabled = false;
		}
	}

	public bool IsSeeking()
	{
		if (base.enabled)
		{
			return _parentBody != null;
		}
		return false;
	}

	private void OnDrawGizmos()
	{
		if (IsSeeking())
		{
			Gizmos.matrix = _parentBody.transform.localToWorldMatrix;
			Gizmos.color = Color.red;
			Gizmos.DrawRay(_localProjectedPos, _localVelocity);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(_localProjectedPos, _localTargetPos);
		}
	}
}
