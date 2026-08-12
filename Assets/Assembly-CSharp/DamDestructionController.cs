using UnityEngine;

public class DamDestructionController : MonoBehaviour
{
	[SerializeField]
	private RingWorldController _ringworldController;

	[SerializeField]
	private Sector _interiorSector;

	[SerializeField]
	private float _startIntegrity = 100f;

	[SerializeField]
	private float _damagedIntegrity = 84f;

	[Space]
	[SerializeField]
	private OWRenderer _leakRenderer;

	[SerializeField]
	private AnimationCurve _leakProgressionCurve;

	[SerializeField]
	private float _leakFadeOutLength = 1f;

	[Space]
	[SerializeField]
	private GameObject _intactDamRoot;

	[SerializeField]
	private GameObject _destroyedDamRoot;

	[SerializeField]
	private Collider _destroyedDamAnchorCollider;

	[SerializeField]
	private DamFragmentController[] _damFragments = new DamFragmentController[0];

	[SerializeField]
	private OWRigidbody[] _centerFragments = new OWRigidbody[0];

	[SerializeField]
	private OWRigidbody[] _topFragments = new OWRigidbody[0];

	[SerializeField]
	private Transform _destructionForceCenter;

	[SerializeField]
	private float _destructionForceRadius = 50f;

	[SerializeField]
	private float _destructionForceLinearAccel = 10f;

	[SerializeField]
	private float _destructionForceAngularAccel = 1f;

	[SerializeField]
	private float _destructionForceDuration = 1f;

	[SerializeField]
	private float _topFragmentsDelay = 1f;

	[Space]
	[SerializeField]
	private ParticleSystem[] _destructionParticles = new ParticleSystem[0];

	[Space]
	[SerializeField]
	private Vector3 _probeEjectLocalDir = new Vector3(0f, 0f, 1f);

	[SerializeField]
	private float _probeEjectSpeed = 10f;

	private bool _leaking;

	private bool _collapsed;

	private static int _propID_Progression = Shader.PropertyToID("_Progression");

	private float _damageProgression;

	private Color _leakColor;

	private OWRigidbody _ringworldBody;

	private float _collapseStartTime;

	private float _destructionForceStopTime;

	private bool _topFragmentsDetached;

	private float _topFragmentDetachTime;

	public RingWorldController ringworldController => _ringworldController;

	public float GetLeakProgression()
	{
		return _leakProgressionCurve.Evaluate(_damageProgression);
	}

	public float GetIntegrityPercent()
	{
		if (_ringworldController.isDamBroken)
		{
			return 0f;
		}
		if (_ringworldController.isDamDamaged)
		{
			return Mathf.Round(Mathf.Lerp(_damagedIntegrity, 0f, _damageProgression));
		}
		return _startIntegrity;
	}

	private void Awake()
	{
		_ringworldBody = this.GetAttachedOWRigidbody();
		if (_leakRenderer != null)
		{
			_leakRenderer.SetActivation(active: false);
			_leakRenderer.SetMaterialProperty(_propID_Progression, 0f);
			_leakColor = _leakRenderer.GetOriginalColor();
		}
		for (int i = 0; i < _damFragments.Length; i++)
		{
			OWCollider[] dynamicColliders = _damFragments[i].dynamicColliders;
			for (int j = 0; j < dynamicColliders.Length; j++)
			{
				Physics.IgnoreCollision(dynamicColliders[j].GetCollider(), _destroyedDamAnchorCollider);
				for (int k = 0; k < _damFragments.Length; k++)
				{
					if (_damFragments[k].highResCollider != null)
					{
						Physics.IgnoreCollision(dynamicColliders[j].GetCollider(), _damFragments[k].highResCollider.GetCollider());
					}
				}
			}
			for (int l = i + 1; l < _damFragments.Length; l++)
			{
				OWCollider[] dynamicColliders2 = _damFragments[l].dynamicColliders;
				for (int m = 0; m < dynamicColliders.Length; m++)
				{
					for (int n = 0; n < dynamicColliders2.Length; n++)
					{
						Physics.IgnoreCollision(dynamicColliders[m].GetCollider(), dynamicColliders2[n].GetCollider());
					}
				}
			}
		}
		base.enabled = false;
	}

	public void StartLeak()
	{
		_leaking = true;
		if (_leakRenderer != null)
		{
			_leakRenderer.SetActivation(active: true);
		}
		base.enabled = true;
	}

	public void StartCollapse()
	{
		if (_collapsed)
		{
			return;
		}
		_collapsed = true;
		_collapseStartTime = Time.time;
		SurveyorProbe probe = Locator.GetProbe();
		if (probe != null && probe.IsAnchored() && probe.transform.IsChildOf(base.transform))
		{
			probe.Unanchor();
			Vector3 normalized = base.transform.TransformDirection(_probeEjectLocalDir).normalized;
			probe.GetOWRigidbody().AddVelocityChange(normalized * _probeEjectSpeed);
		}
		if (_intactDamRoot != null)
		{
			_intactDamRoot.SetActive(value: false);
		}
		if (_destroyedDamRoot != null)
		{
			_destroyedDamRoot.SetActive(value: true);
		}
		for (int i = 0; i < _centerFragments.Length; i++)
		{
			if (!(_centerFragments[i] == null))
			{
				_centerFragments[i].SetVelocity(_ringworldBody.GetPointVelocity(_centerFragments[i].GetPosition()));
				_centerFragments[i].SetAngularVelocity(_ringworldBody.GetAngularVelocity());
			}
		}
		for (int j = 0; j < _topFragments.Length; j++)
		{
			if (!(_topFragments[j] == null))
			{
				_topFragments[j].SetVelocity(_ringworldBody.GetPointVelocity(_topFragments[j].GetPosition()));
				_topFragments[j].SetAngularVelocity(_ringworldBody.GetAngularVelocity());
			}
		}
		_destructionForceStopTime = Time.time + _destructionForceDuration;
		_topFragmentDetachTime = Time.time + _topFragmentsDelay;
		if (_interiorSector.ContainsOccupant(DynamicOccupant.Player))
		{
			for (int k = 0; k < _destructionParticles.Length; k++)
			{
				_destructionParticles[k].Play();
			}
		}
		base.enabled = true;
	}

	private void Update()
	{
		if (!_leaking)
		{
			return;
		}
		_damageProgression = Mathf.InverseLerp(_ringworldController.damDamageTime, _ringworldController.damBreakTime, TimeLoop.GetSecondsElapsed());
		if (!(_leakRenderer != null))
		{
			return;
		}
		_leakRenderer.SetMaterialProperty(_propID_Progression, GetLeakProgression());
		if (_collapsed && _leakRenderer.IsActive())
		{
			float num = Time.time - _collapseStartTime;
			float num2 = 1f - Mathf.Clamp01(num / _leakFadeOutLength);
			if (_leakRenderer != null)
			{
				_leakRenderer.SetColor(new Color(_leakColor.r, _leakColor.g, _leakColor.b, num2));
			}
			if (num2 <= 0f)
			{
				_leakRenderer.SetActivation(active: false);
			}
		}
	}

	private void FixedUpdate()
	{
		if (!_collapsed)
		{
			return;
		}
		if (Time.time < _destructionForceStopTime)
		{
			for (int i = 0; i < _centerFragments.Length; i++)
			{
				if (!(_centerFragments[i] == null))
				{
					Vector3 rhs = _centerFragments[i].GetPosition() - _destructionForceCenter.position;
					if (rhs.sqrMagnitude < _destructionForceRadius * _destructionForceRadius)
					{
						_centerFragments[i].AddAcceleration(rhs.normalized * _destructionForceLinearAccel);
						Vector3 normalized = Vector3.Cross(_destructionForceCenter.forward, rhs).normalized;
						_centerFragments[i].AddAngularAcceleration(normalized * _destructionForceAngularAccel);
					}
				}
			}
			if (_topFragmentsDetached)
			{
				for (int j = 0; j < _topFragments.Length; j++)
				{
					if (!(_topFragments[j] == null))
					{
						Vector3 rhs2 = _topFragments[j].GetPosition() - _destructionForceCenter.position;
						if (rhs2.sqrMagnitude < _destructionForceRadius * _destructionForceRadius)
						{
							_topFragments[j].AddAcceleration(rhs2.normalized * _destructionForceLinearAccel);
							Vector3 normalized2 = Vector3.Cross(_destructionForceCenter.forward, rhs2).normalized;
							_topFragments[j].AddAngularAcceleration(normalized2 * _destructionForceAngularAccel);
						}
					}
				}
			}
		}
		if (!_topFragmentsDetached)
		{
			if (Time.time >= _topFragmentDetachTime)
			{
				_topFragmentsDetached = true;
			}
			else
			{
				for (int k = 0; k < _topFragments.Length; k++)
				{
					if (!(_topFragments[k] == null))
					{
						_topFragments[k].SetVelocity(_ringworldBody.GetPointVelocity(_topFragments[k].GetPosition()));
						_topFragments[k].SetAngularVelocity(_ringworldBody.GetAngularVelocity());
					}
				}
			}
		}
		if (Time.time >= _destructionForceStopTime && _topFragmentsDetached)
		{
			base.enabled = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!OWGizmos.IsDirectlySelected(base.gameObject) || _destructionForceCenter == null)
		{
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(_destructionForceCenter.position, _destructionForceRadius);
		if (_centerFragments == null)
		{
			return;
		}
		for (int i = 0; i < _centerFragments.Length; i++)
		{
			if (!(_centerFragments[i] == null))
			{
				Gizmos.DrawLine(_destructionForceCenter.position, _centerFragments[i].transform.position);
			}
		}
	}
}
