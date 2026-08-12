using UnityEngine;

public class DreamRaftController : MonoBehaviour
{
	[SerializeField]
	private float _turboSpeed = 10f;

	[SerializeField]
	private LightSensor _turboSensor;

	[SerializeField]
	private OWAudioSource _audioSource;

	[Space]
	[SerializeField]
	private DreamRiverFluidVolume _riverFluid;

	[SerializeField]
	private DreamRaftFluidDetector _fluidDetector;

	[Space]
	[SerializeField]
	private OWCollider[] _colliders = new OWCollider[0];

	[SerializeField]
	private Shape[] _shapes = new Shape[0];

	private OWRigidbody _raftBody;

	private void Awake()
	{
		_raftBody = GetComponent<OWRigidbody>();
		_raftBody.OnSuspendOWRigidbody += OnSuspendBody;
		_raftBody.OnUnsuspendOWRigidbody += OnUnsuspendBody;
	}

	private void Start()
	{
		if (_raftBody.GetSimulateInSector() != null)
		{
			Debug.LogWarning("DreamRaftControllers should not have _suspendInSector set!", this);
		}
		_audioSource.SetLocalVolume(0f);
	}

	private void OnDestroy()
	{
		_raftBody.OnSuspendOWRigidbody -= OnSuspendBody;
		_raftBody.OnUnsuspendOWRigidbody -= OnUnsuspendBody;
	}

	public bool IsBoosting()
	{
		return _turboSensor.IsIlluminated();
	}

	public float GetTurboSpeed()
	{
		return _turboSpeed;
	}

	private void FixedUpdate()
	{
		Vector3 pointFlowOnlyVelocity = _riverFluid.GetPointFlowOnlyVelocity(_fluidDetector.transform.position);
		Vector3 up = _riverFluid.transform.up;
		Vector3 vector = OWPhysics.FromToAngularVelocity(Vector3.ProjectOnPlane(base.transform.forward, up), pointFlowOnlyVelocity);
		_raftBody.AddAngularVelocityChange(vector.normalized * Time.deltaTime * 0.1f);
		bool flag = IsBoosting() && !Locator.GetDreamWorldAudioController().GetRiverPathAudioController().IsMuted();
		bool flag2 = _audioSource.isPlaying && !_audioSource.IsFadingOut();
		if (flag && !flag2)
		{
			_audioSource.FadeIn(1f);
		}
		else if (!flag && flag2)
		{
			_audioSource.FadeOut(1f);
		}
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
}
