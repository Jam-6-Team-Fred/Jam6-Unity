using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class VectionFieldEmitter : MonoBehaviour
{
	public enum EmitDirection
	{
		Random = 0,
		Directional = 1,
		Radial = 2,
		Gravity = 3
	}

	private ParticleSystem _particleSystem;

	private ParticleSystem.MainModule _particleSystemSettings;

	private ParticleSystem.Particle[] _particles;

	[SerializeField]
	private Transform _emitterTransform;

	[SerializeField]
	private float _fieldRadius = 10f;

	[SerializeField]
	private int _particleCount = 10;

	[SerializeField]
	private bool _emitOnLeadingEdge = true;

	[SerializeField]
	private EmitDirection _emitDirection;

	[SerializeField]
	private Vector3 _directionalDir = Vector3.zero;

	[SerializeField]
	private bool _reverseDir;

	[SerializeField]
	private ForceVolume[] _affectingForces;

	[SerializeField]
	private float _forceMultiplier = 1f;

	[SerializeField]
	private bool _applyForcePerParticle;

	private ParticleSystem.EmitParams _emitParams;

	private Vector3 _emitterLastPos;

	public Transform emitterTransform
	{
		get
		{
			return _emitterTransform;
		}
		set
		{
			_emitterTransform = value;
		}
	}

	public float fieldRadius
	{
		get
		{
			return _fieldRadius;
		}
		set
		{
			_fieldRadius = Mathf.Max(value, 0f);
		}
	}

	public int particleCount
	{
		get
		{
			return _particleCount;
		}
		set
		{
			_particleCount = Mathf.Max(value, 0);
		}
	}

	public bool hasAliveParticles => _particleSystem.particleCount > 0;

	public bool emitOnLeadingEdge
	{
		get
		{
			return _emitOnLeadingEdge;
		}
		set
		{
			_emitOnLeadingEdge = value;
		}
	}

	public EmitDirection emitDirection
	{
		get
		{
			return _emitDirection;
		}
		set
		{
			_emitDirection = value;
		}
	}

	public Vector3 directionalDir
	{
		get
		{
			return _directionalDir;
		}
		set
		{
			_directionalDir = value;
		}
	}

	public bool reverseDir
	{
		get
		{
			return _reverseDir;
		}
		set
		{
			_reverseDir = value;
		}
	}

	private void OnValidate()
	{
		if (_fieldRadius < 0f)
		{
			_fieldRadius = 0f;
		}
		if (_particleCount < 0)
		{
			_particleCount = 0;
		}
	}

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		_particleSystemSettings = _particleSystem.main;
		_particles = new ParticleSystem.Particle[_particleSystemSettings.maxParticles];
		_emitterLastPos = ((_emitterTransform == null) ? base.transform.position : _emitterTransform.position);
		if (_particleSystemSettings.simulationSpace == ParticleSystemSimulationSpace.Local)
		{
			_emitterLastPos = base.transform.InverseTransformPoint(_emitterLastPos);
		}
		_emitParams = default(ParticleSystem.EmitParams);
	}

	private void FixedUpdate()
	{
		if (_particleCount == 0 && _particleSystem.particleCount == 0)
		{
			return;
		}
		Vector3 vector = ((_emitterTransform == null) ? base.transform.position : _emitterTransform.position);
		Vector3 vector2 = ((_particleSystemSettings.simulationSpace == ParticleSystemSimulationSpace.World) ? vector : base.transform.InverseTransformPoint(vector));
		Vector3 moveDist = vector2 - _emitterLastPos;
		int particles = _particleSystem.GetParticles(_particles);
		if (_forceMultiplier != 0f && _affectingForces != null)
		{
			if (_applyForcePerParticle)
			{
				for (int i = 0; i < particles; i++)
				{
					Vector3 worldPos = ((_particleSystemSettings.simulationSpace == ParticleSystemSimulationSpace.World) ? _particles[i].position : base.transform.TransformPoint(_particles[i].position));
					Vector3 vector3 = CalcForceAccel(worldPos);
					_particles[i].velocity += vector3 * _forceMultiplier * Time.deltaTime;
				}
			}
			else
			{
				Vector3 vector4 = CalcForceAccel(vector) * _forceMultiplier * Time.deltaTime;
				for (int j = 0; j < particles; j++)
				{
					_particles[j].velocity += vector4;
				}
			}
		}
		int num = 0;
		for (int k = 0; k < particles; k++)
		{
			if ((vector2 - _particles[k].position).sqrMagnitude > _fieldRadius * _fieldRadius)
			{
				_particles[k].remainingLifetime = 0f;
				num++;
			}
		}
		_particleSystem.SetParticles(_particles, particles);
		int num2 = particles - num;
		for (int l = 0; l < num; l++)
		{
			if (num2 >= _particleCount)
			{
				break;
			}
			bool flag = _emitOnLeadingEdge && moveDist.sqrMagnitude < _fieldRadius * _fieldRadius;
			CalcEmitParams(vector, vector2, flag, moveDist);
			_particleSystem.Emit(_emitParams, 1);
			num2++;
		}
		for (int m = 0; m < _particleCount - num2; m++)
		{
			CalcEmitParams(vector, vector2);
			_particleSystem.Emit(_emitParams, 1);
			num2++;
		}
		_emitterLastPos = vector2;
	}

	private Vector3 CalcForceAccel(Vector3 worldPos)
	{
		int num = 0;
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < _affectingForces.Length; i++)
		{
			if (_affectingForces[i] == null)
			{
				continue;
			}
			Collider component = _affectingForces[i].GetComponent<Collider>();
			if (!(component == null) && (!(component is SphereCollider) || OWPhysics.IsPointContained(component as SphereCollider, worldPos)) && (!(component is CapsuleCollider) || OWPhysics.IsPointContained(component as CapsuleCollider, worldPos)) && (!(component is BoxCollider) || OWPhysics.IsPointContained(component as BoxCollider, worldPos)))
			{
				int priority = _affectingForces[i].GetPriority();
				if (priority > num)
				{
					vector = Vector3.zero;
					num = priority;
				}
				vector += _affectingForces[i].CalculateForceAccelerationAtPoint(worldPos);
			}
		}
		if (_particleSystemSettings.simulationSpace == ParticleSystemSimulationSpace.Local)
		{
			vector = base.transform.InverseTransformDirection(vector);
		}
		return vector;
	}

	private void CalcEmitParams(Vector3 emitterWorldPos, Vector3 emitterCurrentPos, bool emitOnLeadingEdge = false, Vector3 moveDist = default(Vector3))
	{
		if (emitOnLeadingEdge)
		{
			_emitParams.position = Random.onUnitSphere * _fieldRadius * 0.999f;
			if (moveDist.sqrMagnitude > 1.0000001E-06f)
			{
				_emitParams.position *= Mathf.Sign(Vector3.Dot(_emitParams.position, moveDist));
				_emitParams.position -= moveDist * Random.value;
			}
			_emitParams.position += emitterCurrentPos;
		}
		else
		{
			_emitParams.position = emitterCurrentPos + Random.insideUnitSphere * _fieldRadius * 0.999f;
		}
		switch (_emitDirection)
		{
		case EmitDirection.Random:
			_emitParams.velocity = Random.onUnitSphere * _particleSystemSettings.startSpeed.constant;
			if (emitOnLeadingEdge)
			{
				_emitParams.velocity *= Mathf.Sign(Vector3.Dot(_emitParams.velocity, emitterCurrentPos - _emitParams.position));
			}
			break;
		case EmitDirection.Directional:
			_emitParams.velocity = _directionalDir * _particleSystemSettings.startSpeed.constant;
			break;
		case EmitDirection.Radial:
		{
			Vector3 vector2 = ((_particleSystemSettings.simulationSpace == ParticleSystemSimulationSpace.World) ? (_emitParams.position - base.transform.position) : _emitParams.position);
			_emitParams.velocity = ((vector2.sqrMagnitude > 1.0000001E-06f) ? vector2.normalized : Vector3.zero) * _particleSystemSettings.startSpeed.constant * (_reverseDir ? (-1f) : 1f);
			break;
		}
		case EmitDirection.Gravity:
		{
			Vector3 worldPos = ((!_applyForcePerParticle) ? emitterWorldPos : ((_particleSystemSettings.simulationSpace == ParticleSystemSimulationSpace.World) ? _emitParams.position : base.transform.TransformPoint(_emitParams.position)));
			Vector3 vector = CalcForceAccel(worldPos);
			_emitParams.velocity = ((vector.sqrMagnitude > 1.0000001E-06f) ? vector.normalized : Vector3.zero) * _particleSystemSettings.startSpeed.constant * (_reverseDir ? (-1f) : 1f);
			break;
		}
		default:
			_emitParams.velocity = Vector3.zero;
			break;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere((_emitterTransform != null) ? _emitterTransform.position : base.transform.position, _fieldRadius); // CHANGED
		}
	}
}
