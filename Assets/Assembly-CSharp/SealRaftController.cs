using UnityEngine;

public class SealRaftController : MonoBehaviour
{
	[SerializeField]
	private AbstractGhostDoorInterface _codeInterface;

	[Space]
	[SerializeField]
	private Transform _nearNode;

	[SerializeField]
	private Transform _farNode;

	[Space]
	[SerializeField]
	private LightSensor _nearSensor;

	[SerializeField]
	private LightSensor _farSensor;

	[Space]
	[SerializeField]
	private OWCollider[] _colliders = new OWCollider[0];

	[SerializeField]
	private Shape[] _shapes = new Shape[0];

	[SerializeField]
	private AlignToSurfaceFluidDetector _fluidDetector;

	[Header("Audio")]
	[SerializeField]
	private float _minSpeed = 1f;

	[SerializeField]
	private float _maxSpeed = 5f;

	[SerializeField]
	private OWAudioSource _audioSource;

	private OWRigidbody _raftBody;

	private Vector3 _origDrag;

	private Transform _autoTargetNode;

	private Transform _anchorNode;

	private void Awake()
	{
		_raftBody = GetComponent<OWRigidbody>();
		_raftBody.OnSuspendOWRigidbody += OnSuspendBody;
		_raftBody.OnUnsuspendOWRigidbody += OnUnsuspendBody;
		if (_codeInterface != null)
		{
			_codeInterface.OnOpen += OnOpen;
			_codeInterface.OnClose += OnClose;
		}
	}

	private void Start()
	{
		_origDrag = _fluidDetector.GetDragFactor();
		_audioSource.SetLocalVolume(0f);
	}

	private void OnDestroy()
	{
		_raftBody.OnSuspendOWRigidbody -= OnSuspendBody;
		_raftBody.OnUnsuspendOWRigidbody -= OnUnsuspendBody;
		if (_codeInterface != null)
		{
			_codeInterface.OnOpen -= OnOpen;
			_codeInterface.OnClose -= OnClose;
		}
	}

	private void FixedUpdate()
	{
		_fluidDetector.SetDragFactor(_origDrag);
		Vector3 forward = _farNode.forward;
		Vector3 vector = OWPhysics.FromToAngularVelocity(Vector3.ProjectOnPlane(base.transform.forward, _raftBody.GetOrigParent().up), forward);
		_raftBody.AddAngularVelocityChange(vector.normalized * Time.deltaTime * 0.1f);
		int num = 0;
		if (_nearSensor.IsIlluminated())
		{
			num--;
		}
		if (_farSensor.IsIlluminated())
		{
			num++;
		}
		float num2 = 3f;
		float num3 = 5f;
		float b = 10f;
		if (_autoTargetNode != null)
		{
			Vector3 vector2 = _autoTargetNode.position - base.transform.position;
			vector2.y = 0f;
			float magnitude = vector2.magnitude;
			float num4 = Mathf.Min(magnitude, num3);
			_raftBody.AddAcceleration(vector2.normalized * num4);
			if (magnitude < num2)
			{
				_anchorNode = _autoTargetNode;
				_autoTargetNode = null;
			}
		}
		else if (num != 0)
		{
			Vector3 vector3 = ((num > 0) ? _farNode : _nearNode).position - base.transform.position;
			vector3.y = 0f;
			_raftBody.AddAcceleration(vector3.normalized * num3);
		}
		else if (_anchorNode != null)
		{
			_fluidDetector.SetDragFactor(new Vector3(5f, _origDrag.y, 5f));
			Vector3 vector4 = _anchorNode.position - base.transform.position;
			vector4.y = 0f;
			float magnitude2 = vector4.magnitude;
			if (magnitude2 > num2)
			{
				_anchorNode = null;
			}
			else
			{
				float num5 = Mathf.Min(magnitude2, b);
				_raftBody.AddAcceleration(vector4.normalized * num5);
			}
		}
		else
		{
			Vector3 vector5 = _nearNode.position - base.transform.position;
			vector5.y = 0f;
			Vector3 vector6 = _farNode.position - base.transform.position;
			vector6.y = 0f;
			if (vector5.sqrMagnitude < num2 * num2)
			{
				_anchorNode = _nearNode;
			}
			else if (vector6.sqrMagnitude < num2 * num2)
			{
				_anchorNode = _farNode;
			}
		}
		float magnitude3 = (_raftBody.GetVelocity() - _raftBody.GetOrigParentBody().GetVelocity()).magnitude;
		float num6 = Mathf.InverseLerp(_minSpeed, _maxSpeed, magnitude3);
		_audioSource.SetLocalVolume(num6);
		bool flag = num6 > 0f;
		if (!_audioSource.isPlaying && flag)
		{
			_audioSource.Play();
		}
		else if (_audioSource.isPlaying && !flag)
		{
			_audioSource.Stop();
		}
	}

	private void OnOpen()
	{
		_autoTargetNode = _nearNode;
	}

	private void OnClose()
	{
		_autoTargetNode = _farNode;
	}

	private void OnSuspendBody(OWRigidbody body)
	{
		for (int i = 0; i < _colliders.Length; i++)
		{
			_colliders[i].SetActivation(active: false);
		}
		for (int j = 0; j < _shapes.Length; j++)
		{
			_shapes[j].enabled = false;
		}
		_audioSource.FadeOut(0.5f);
		base.enabled = false;
	}

	private void OnUnsuspendBody(OWRigidbody body)
	{
		for (int i = 0; i < _colliders.Length; i++)
		{
			_colliders[i].SetActivation(active: true);
		}
		for (int j = 0; j < _shapes.Length; j++)
		{
			_shapes[j].enabled = true;
		}
		base.enabled = true;
	}

	private void OnDrawGizmosSelected()
	{
		if (_nearNode != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(_nearNode.position, new Vector3(6f, 1f, 6f));
		}
		if (_farNode != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(_farNode.position, new Vector3(6f, 1f, 6f));
		}
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(_nearSensor.transform.position, 30f);
	}
}
