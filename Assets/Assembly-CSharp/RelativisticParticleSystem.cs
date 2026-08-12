using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RelativisticParticleSystem : MonoBehaviour
{
	private struct ModuleVector
	{
		public ParticleSystem.MinMaxCurve origX;

		public ParticleSystem.MinMaxCurve origY;

		public ParticleSystem.MinMaxCurve origZ;

		public ModuleVector(ParticleSystem.MinMaxCurve x, ParticleSystem.MinMaxCurve y, ParticleSystem.MinMaxCurve z)
		{
			origX = x;
			origY = y;
			origZ = z;
		}

		public void GetRotatedVector(Quaternion rotation, out ParticleSystem.MinMaxCurve x, out ParticleSystem.MinMaxCurve y, out ParticleSystem.MinMaxCurve z)
		{
			if (origX.mode == ParticleSystemCurveMode.Constant)
			{
				Vector3 vector = rotation * new Vector3(origX.constant, origY.constant, origZ.constant);
				x = new ParticleSystem.MinMaxCurve(vector.x);
				y = new ParticleSystem.MinMaxCurve(vector.y);
				z = new ParticleSystem.MinMaxCurve(vector.z);
			}
			else if (origX.mode == ParticleSystemCurveMode.TwoConstants)
			{
				Vector3 vector2 = rotation * new Vector3(origX.constantMin, origY.constantMin, origZ.constantMin);
				Vector3 vector3 = rotation * new Vector3(origX.constantMax, origY.constantMax, origZ.constantMax);
				x = new ParticleSystem.MinMaxCurve(vector2.x, vector3.x);
				y = new ParticleSystem.MinMaxCurve(vector2.y, vector3.y);
				z = new ParticleSystem.MinMaxCurve(vector2.z, vector3.z);
			}
			else
			{
				Debug.LogWarning("Cannot properly rotate Module Curves! Use Constants mode instead, dummy.");
				x = origX;
				y = origY;
				z = origZ;
			}
		}
	}

	private ParticleSystem _particleSystem;

	private Transform _simulationSpace;

	private Quaternion _rotation;

	private ParticleSystem.MainModule _mainModule;

	private ParticleSystem.VelocityOverLifetimeModule _velocityOverLifetimeModule;

	private ModuleVector _velocityOverLifetimeVector;

	private ParticleSystem.LimitVelocityOverLifetimeModule _limitVelocityOverLifetimeModule;

	private ModuleVector _limitVelocityOverLifetimeVector;

	private ParticleSystem.ForceOverLifetimeModule _forceOverLifetimeModule;

	private ModuleVector _forceOverLifetimeVector;

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		_rotation = base.transform.rotation;
		_simulationSpace = new GameObject(base.name + "_ReferenceFrame").transform;
		_simulationSpace.SetParent(this.GetAttachedOWRigidbody().transform, worldPositionStays: false);
		_mainModule = _particleSystem.main;
		_mainModule.simulationSpace = ParticleSystemSimulationSpace.Custom;
		_mainModule.customSimulationSpace = _simulationSpace;
		_velocityOverLifetimeModule = _particleSystem.velocityOverLifetime;
		_velocityOverLifetimeVector = new ModuleVector(_velocityOverLifetimeModule.x, _velocityOverLifetimeModule.y, _velocityOverLifetimeModule.z);
		_limitVelocityOverLifetimeModule = _particleSystem.limitVelocityOverLifetime;
		_limitVelocityOverLifetimeVector = new ModuleVector(_limitVelocityOverLifetimeModule.limitX, _limitVelocityOverLifetimeModule.limitY, _limitVelocityOverLifetimeModule.limitZ);
		_forceOverLifetimeModule = _particleSystem.forceOverLifetime;
		_forceOverLifetimeVector = new ModuleVector(_forceOverLifetimeModule.x, _forceOverLifetimeModule.y, _forceOverLifetimeModule.z);
	}

	private void FixedUpdate()
	{
		_simulationSpace.rotation = _rotation;
		if (_velocityOverLifetimeModule.enabled || (_limitVelocityOverLifetimeModule.enabled && _limitVelocityOverLifetimeModule.separateAxes) || _forceOverLifetimeModule.enabled)
		{
			Quaternion rotation = Quaternion.Inverse(_rotation) * base.transform.rotation;
			ParticleSystem.MinMaxCurve x;
			ParticleSystem.MinMaxCurve y;
			ParticleSystem.MinMaxCurve z;
			if (_velocityOverLifetimeModule.enabled)
			{
				_velocityOverLifetimeVector.GetRotatedVector(rotation, out x, out y, out z);
				_velocityOverLifetimeModule.x = x;
				_velocityOverLifetimeModule.y = y;
				_velocityOverLifetimeModule.z = z;
			}
			if (_limitVelocityOverLifetimeModule.enabled)
			{
				_limitVelocityOverLifetimeVector.GetRotatedVector(rotation, out x, out y, out z);
				_limitVelocityOverLifetimeModule.limitX = x;
				_limitVelocityOverLifetimeModule.limitY = y;
				_limitVelocityOverLifetimeModule.limitZ = z;
			}
			if (_forceOverLifetimeModule.enabled)
			{
				_forceOverLifetimeVector.GetRotatedVector(rotation, out x, out y, out z);
				_forceOverLifetimeModule.x = x;
				_forceOverLifetimeModule.y = y;
				_forceOverLifetimeModule.z = z;
			}
		}
	}
}
