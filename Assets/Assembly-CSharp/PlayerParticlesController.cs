using UnityEngine;

public class PlayerParticlesController : MonoBehaviour
{
	private const int _kParticlePoolSize = 8;

	private PlayerCharacterController _playerCharacterController;

	private PlayerAnimController _playerAnimController;

	private ImpactSensor _impactSensor;

	private ForceDetector _playerForceDetector;

	private FluidDetector _playerFluidDetector;

	[SerializeField]
	private GameObject[] _footstepParticlePrefabs = new GameObject[24];

	[SerializeField]
	private float _impactParticleMinSpeed = 30f;

	[SerializeField]
	private GameObject[] _impactParticlePrefabs = new GameObject[24];

	private Transform _particleSystemPoolRoot;

	private ParticleSystemPool[] _footstepParticleSystems;

	private Transform _leftFootTransform;

	private Transform _rightFootTransform;

	private ParticleSystemPool[] _impactParticleSystems;

	private void Awake()
	{
		OWRigidbody attachedOWRigidbody = this.GetAttachedOWRigidbody();
		_playerCharacterController = attachedOWRigidbody.GetRequiredComponentInChildren<PlayerCharacterController>();
		_playerAnimController = attachedOWRigidbody.GetRequiredComponentInChildren<PlayerAnimController>();
		_impactSensor = attachedOWRigidbody.GetRequiredComponentInChildren<ImpactSensor>();
		_playerForceDetector = attachedOWRigidbody.GetRequiredComponentInChildren<ForceDetector>();
		_playerFluidDetector = attachedOWRigidbody.GetRequiredComponentInChildren<FluidDetector>();
		_particleSystemPoolRoot = new GameObject("ParticleSystemPool").transform;
		_particleSystemPoolRoot.SetParent(base.transform.parent);
		_particleSystemPoolRoot.transform.localPosition = Vector3.zero;
		_particleSystemPoolRoot.transform.localRotation = Quaternion.identity;
		_footstepParticleSystems = new ParticleSystemPool[24];
		_impactParticleSystems = new ParticleSystemPool[24];
		for (int i = 0; i < 24; i++)
		{
			if (_footstepParticlePrefabs[i] != null)
			{
				_footstepParticleSystems[i] = new ParticleSystemPool(_footstepParticlePrefabs[i], 8, _particleSystemPoolRoot);
			}
			if (_impactParticlePrefabs[i] != null)
			{
				_impactParticleSystems[i] = new ParticleSystemPool(_impactParticlePrefabs[i], 8, _particleSystemPoolRoot);
			}
		}
		Animator component = _playerAnimController.GetComponent<Animator>();
		_leftFootTransform = component.GetBoneTransform(HumanBodyBones.LeftFoot);
		_rightFootTransform = component.GetBoneTransform(HumanBodyBones.RightFoot);
		_playerAnimController.OnLeftFootGrounded += OnLeftFootGrounded;
		_playerAnimController.OnRightFootGrounded += OnRightFootGrounded;
		_impactSensor.OnImpact += OnImpact;
	}

	private void OnDestroy()
	{
		if (_playerAnimController != null)
		{
			_playerAnimController.OnLeftFootGrounded -= OnLeftFootGrounded;
			_playerAnimController.OnRightFootGrounded -= OnRightFootGrounded;
		}
		if (_impactSensor != null)
		{
			_impactSensor.OnImpact -= OnImpact;
		}
	}

	private void Update()
	{
		for (int i = 0; i < 24; i++)
		{
			if (_footstepParticleSystems[i] != null)
			{
				_footstepParticleSystems[i].Update();
			}
			if (_impactParticleSystems[i] != null)
			{
				_impactParticleSystems[i].Update();
			}
		}
	}

	private void PlayFootstepParticles(Transform groundBody, Vector3 position, SurfaceType surfaceType)
	{
		if (_footstepParticleSystems[(int)surfaceType] != null)
		{
			ParticleSystem particleSystem = _footstepParticleSystems[(int)surfaceType].Instantiate(groundBody, position, _playerCharacterController.transform.rotation);
			if (particleSystem != null && particleSystem.forceOverLifetime.enabled)
			{
				Vector3 vector = _playerForceDetector.GetForceAcceleration() + _playerFluidDetector.GetLinearFluidAcceleration();
				Vector3 vector2 = particleSystem.transform.InverseTransformVector(vector);
				ParticleSystem.ForceOverLifetimeModule forceOverLifetime = particleSystem.forceOverLifetime;
				forceOverLifetime.x = vector2.x;
				forceOverLifetime.y = vector2.y;
				forceOverLifetime.z = vector2.z;
			}
		}
	}

	private void OnLeftFootGrounded()
	{
		if (_leftFootTransform == null)
		{
			_leftFootTransform = _playerAnimController.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftFoot);
		}
		OWRigidbody groundBody = _playerCharacterController.GetGroundBody();
		if (groundBody != null)
		{
			PlayFootstepParticles(groundBody.transform, _leftFootTransform.position, _playerCharacterController.GetGroundSurface());
		}
	}

	private void OnRightFootGrounded()
	{
		if (_rightFootTransform == null)
		{
			_rightFootTransform = _playerAnimController.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightFoot);
		}
		OWRigidbody groundBody = _playerCharacterController.GetGroundBody();
		if (groundBody != null)
		{
			PlayFootstepParticles(groundBody.transform, _rightFootTransform.position, _playerCharacterController.GetGroundSurface());
		}
	}

	private void OnImpact(ImpactData impact)
	{
		if (impact.speed < _impactParticleMinSpeed)
		{
			return;
		}
		SurfaceType surfaceType = impact.contactSurfaceTypes[0];
		if (_impactParticleSystems[(int)surfaceType] != null)
		{
			ParticleSystem particleSystem = _impactParticleSystems[(int)surfaceType].Instantiate(impact.otherBody.transform, impact.point, Quaternion.LookRotation(impact.normal));
			if (particleSystem.forceOverLifetime.enabled)
			{
				Vector3 vector = _playerForceDetector.GetForceAcceleration() + _playerFluidDetector.GetLinearFluidAcceleration();
				Vector3 vector2 = particleSystem.transform.InverseTransformVector(vector);
				ParticleSystem.ForceOverLifetimeModule forceOverLifetime = particleSystem.forceOverLifetime;
				forceOverLifetime.x = vector2.x;
				forceOverLifetime.y = vector2.y;
				forceOverLifetime.z = vector2.z;
			}
		}
	}
}
