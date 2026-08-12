using UnityEngine;

public class DetachableFragment : MonoBehaviour, IItemDropTarget
{
	public enum ForceMask
	{
		Everything = 0,
		SunOnly = 1,
		ParentOnly = 2
	}

	public delegate void DetachFragmentEvent(OWRigidbody fragmentBody, OWRigidbody parentBody);

	public delegate void ComeToRestEvent(OWRigidbody anchorBody);

	public delegate void WarpFragmentEvent(Sector newSector);

	public delegate void WarpScalingEvent();

	private static GameObject s_detachAudioSourcePrefab;

	private static float s_lastDefaultAudioPlayTime;

	[SerializeField]
	private GameObject _destructibleRoot;

	[SerializeField]
	private float _destructibleDelay = 2f;

	[SerializeField]
	private Vector3 _localCenterOfMass = Vector3.zero;

	[SerializeField]
	private Transform _centerOfMassOverride;

	[SerializeField]
	private float _mass = 100f;

	[SerializeField]
	private float _drag = 10f;

	[SerializeField]
	private ForceMask _forceDetection = ForceMask.ParentOnly;

	[SerializeField]
	[HideInInspector]
	private Vector3 _fragmentBoundSize = Vector3.zero;

	[SerializeField]
	private bool _makeKinematic = true;

	[SerializeField]
	private bool _addShapeToDetector;

	[SerializeField]
	private float _detectorRadius = 0.5f;

	[SerializeField]
	private bool _drawBounds;

	[SerializeField]
	private bool _muteAudio;

	[SerializeField]
	private AudioType _overrideDefaultAudio;

	[SerializeField]
	private OWAudioSource _audioSource;

	private FragmentIntegrity _fragmentIntegrity;

	private OWRigidbody _attachedBody;

	private float _detachTime;

	private bool _isDetached;

	private Sector _sector;

	private ISectorGroup[] _sectorGroups;

	private EffectVolume[] _effectVolumes;

	private ProxyShadowCaster[] _proxyShadowCasters;

	public event DetachFragmentEvent OnDetachFragment;

	public event ComeToRestEvent OnComeToRest;

	public event WarpFragmentEvent OnChangeSector;

	public event WarpScalingEvent OnBeginWarpScaling;

	public event WarpScalingEvent OnEndWarpScaling;

	public void Init(float drag = 0f, ForceMask forceDetection = ForceMask.ParentOnly)
	{
		CalculateMassFromBounds();
		_drag = drag;
		_forceDetection = forceDetection;
	}

	private void Reset()
	{
		CalculateMassFromBounds();
	}

	private void Awake()
	{
		_attachedBody = this.GetAttachedOWRigidbody();
		_fragmentIntegrity = GetComponent<FragmentIntegrity>();
		_sector = GetComponent<Sector>();
		if (!_muteAudio && _audioSource == null)
		{
			if (s_detachAudioSourcePrefab == null)
			{
				s_detachAudioSourcePrefab = Resources.Load<GameObject>("Prefabs/FloatingAudio/DetachableFragmentAudioSource");
			}
			Vector3 position = ((_centerOfMassOverride == null) ? base.transform.TransformPoint(_localCenterOfMass) : _centerOfMassOverride.position);
			GameObject obj = Object.Instantiate(s_detachAudioSourcePrefab, position, Quaternion.identity, base.transform);
			_audioSource = obj.GetRequiredComponent<OWAudioSource>();
		}
		AddEventListeners();
	}

	private void Start()
	{
		_sectorGroups = GetComponentsInChildren<ISectorGroup>();
		_effectVolumes = GetComponentsInChildren<EffectVolume>();
		_proxyShadowCasters = GetComponentsInChildren<ProxyShadowCaster>();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		RemoveEventListeners();
	}

	public Vector3 GetWorldCenterOfMass()
	{
		return base.transform.TransformPoint(_localCenterOfMass);
	}

	protected virtual void AddEventListeners()
	{
		if (_fragmentIntegrity != null)
		{
			_fragmentIntegrity.OnTakeDamage += OnTakeDamage;
		}
	}

	protected virtual void RemoveEventListeners()
	{
		if (_fragmentIntegrity != null)
		{
			_fragmentIntegrity.OnTakeDamage -= OnTakeDamage;
		}
	}

	public bool HasDestructibleRoot()
	{
		return _destructibleRoot != null;
	}

	public Sector GetSector()
	{
		return _sector;
	}

	public OWRigidbody Detach()
	{
		if (_isDetached)
		{
			return null;
		}
		_isDetached = true;
		RemoveEventListeners();
		_fragmentIntegrity = null;
		if (_destructibleRoot != null)
		{
			_detachTime = Time.time;
			base.enabled = true;
		}
		GameObject gameObject = new GameObject();
		if (base.name.StartsWith("Fragment"))
		{
			gameObject.name = base.name + "_Body";
		}
		else if (base.name.Contains("Sector_"))
		{
			gameObject.name = "Fragment_" + base.name.Replace("Sector_", "") + "_Body";
		}
		else
		{
			gameObject.name = "Fragment_" + base.name + "_Body";
		}
		gameObject.transform.parent = null;
		gameObject.transform.position = ((_centerOfMassOverride == null) ? base.transform.TransformPoint(_localCenterOfMass) : _centerOfMassOverride.position);
		gameObject.transform.rotation = base.transform.rotation;
		gameObject.tag = "DetachedFragment";
		GameObject gameObject2 = new GameObject();
		gameObject2.transform.parent = gameObject.transform;
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localRotation = Quaternion.identity;
		gameObject2.name = "ScaleRoot";
		base.transform.parent = gameObject2.transform;
		gameObject.AddComponent<Rigidbody>();
		OWRigidbody oWRigidbody = gameObject.AddComponent<OWRigidbody>();
		oWRigidbody.SetScaleRoot(gameObject2.transform);
		gameObject.AddComponent<ImpactSensor>();
		if (_makeKinematic)
		{
			oWRigidbody.MakeKinematic();
			oWRigidbody.EnableKinematicSimulation();
		}
		oWRigidbody.GetRigidbody().mass = _mass;
		if (!_makeKinematic)
		{
			oWRigidbody.SetCenterOfMass(Vector3.zero);
		}
		for (int i = 0; i < _effectVolumes.Length; i++)
		{
			_effectVolumes[i].SetAttachedBody(oWRigidbody);
		}
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(oWRigidbody.GetWorldCenterOfMass());
		oWRigidbody.SetVelocity(pointVelocity);
		oWRigidbody.SetAngularVelocity(_attachedBody.GetAngularVelocity());
		GameObject gameObject3 = new GameObject();
		gameObject3.name = "DetectorVolume";
		gameObject3.layer = LayerMask.NameToLayer("BasicDetector");
		gameObject3.transform.parent = gameObject.transform;
		gameObject3.transform.localPosition = Vector3.zero;
		gameObject3.AddComponent<SphereCollider>().radius = _detectorRadius;
		if (_addShapeToDetector)
		{
			SphereShape sphereShape = gameObject3.AddComponent<SphereShape>();
			sphereShape.SetCollisionMode(Shape.CollisionMode.Detector);
			sphereShape.radius = _detectorRadius;
		}
		DynamicFluidDetector dynamicFluidDetector = gameObject3.AddComponent<DynamicFluidDetector>();
		dynamicFluidDetector.SetDragFactor(_drag);
		FragmentDragAnimator component = GetComponent<FragmentDragAnimator>();
		if (component != null)
		{
			component.StartAnimation(dynamicFluidDetector);
		}
		if (_forceDetection == ForceMask.SunOnly)
		{
			gameObject3.AddComponent<ConstantForceDetector>().AddConstantVolume(Locator.GetSunTransform().GetComponent<OWRigidbody>().GetAttachedGravityVolume(), inheritForceAcceleration: false);
		}
		else if (_forceDetection == ForceMask.ParentOnly)
		{
			gameObject3.AddComponent<ConstantForceDetector>().AddConstantVolume(_attachedBody.GetAttachedGravityVolume());
		}
		else
		{
			gameObject3.AddComponent<DynamicForceDetector>();
		}
		if (_sector != null)
		{
			Sector rootSector = _sector.GetRootSector();
			_sector.SetParentSector(rootSector);
			_sector.SetOWRigidbody(oWRigidbody);
			for (int j = 0; j < _sectorGroups.Length; j++)
			{
				Sector sector = _sectorGroups[j].GetSector();
				if (sector != null && sector != _sector && sector != rootSector && !sector.IsSubsector(_sector))
				{
					_sectorGroups[j].SetSector(rootSector);
				}
			}
		}
		for (int k = 0; k < _proxyShadowCasters.Length; k++)
		{
			_proxyShadowCasters[k].SetDynamic(dynamic: true);
		}
		if (!_muteAudio)
		{
			if (_overrideDefaultAudio != 0)
			{
				_audioSource.PlayOneShot(_overrideDefaultAudio);
			}
			else if (Time.time > s_lastDefaultAudioPlayTime + 1f)
			{
				_audioSource.PlayOneShot(AudioType.BH_BreakawayFragment);
				s_lastDefaultAudioPlayTime = Time.time;
			}
		}
		if (this.OnDetachFragment != null)
		{
			this.OnDetachFragment(oWRigidbody, _attachedBody);
		}
		return oWRigidbody;
	}

	public void BeginWarpScaling()
	{
		if (this.OnBeginWarpScaling != null)
		{
			this.OnBeginWarpScaling();
		}
		for (int i = 0; i < _sectorGroups.Length; i++)
		{
			if (_sectorGroups[i] is CollisionGroup)
			{
				(_sectorGroups[i] as CollisionGroup).BeginScalingGroup();
			}
		}
	}

	public void ChangeFragmentSector(Sector newParentSector, ProxyShadowCasterSuperGroup newProxyShadowSuperGroup)
	{
		if (_sector != null)
		{
			_sector.SetParentSector(newParentSector);
		}
		for (int i = 0; i < _sectorGroups.Length; i++)
		{
			Sector sector = _sectorGroups[i].GetSector();
			if (sector != _sector || sector == null)
			{
				_sectorGroups[i].SetSector(newParentSector);
			}
		}
		for (int j = 0; j < _proxyShadowCasters.Length; j++)
		{
			_proxyShadowCasters[j].SetSuperGroup(newProxyShadowSuperGroup);
		}
		if (this.OnChangeSector != null)
		{
			this.OnChangeSector(newParentSector);
		}
	}

	public void EndWarpScaling()
	{
		for (int i = 0; i < _sectorGroups.Length; i++)
		{
			if (_sectorGroups[i] is CollisionGroup)
			{
				(_sectorGroups[i] as CollisionGroup).EndScalingGroup();
			}
		}
		if (this.OnEndWarpScaling != null)
		{
			this.OnEndWarpScaling();
		}
	}

	public void ComeToRest(OWRigidbody anchorBody)
	{
		for (int i = 0; i < _proxyShadowCasters.Length; i++)
		{
			_proxyShadowCasters[i].SetDynamic(dynamic: false);
		}
		if (this.OnComeToRest != null)
		{
			this.OnComeToRest(anchorBody);
		}
	}

	protected virtual void OnTakeDamage(float integrity)
	{
		if (integrity <= 0f)
		{
			Invoke("Detach", 0.1f);
		}
	}

	public void CalculateMassFromBounds()
	{
		Vector3 zero = Vector3.zero;
		Vector3 vector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		Vector3 vector2 = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		_makeKinematic = false;
		int num = 0;
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			bool flag = collider.enabled;
			collider.enabled = true;
			if (!collider.isTrigger && OWLayerMask.IsLayerInMask(collider.gameObject.layer, OWLayerMask.physicalMask))
			{
				zero += collider.bounds.center;
				vector.x = Mathf.Min(vector.x, collider.bounds.min.x);
				vector.y = Mathf.Min(vector.y, collider.bounds.min.y);
				vector.z = Mathf.Min(vector.z, collider.bounds.min.z);
				vector2.x = Mathf.Max(vector2.x, collider.bounds.max.x);
				vector2.y = Mathf.Max(vector2.y, collider.bounds.max.y);
				vector2.z = Mathf.Max(vector2.z, collider.bounds.max.z);
				num++;
				if (collider is MeshCollider && !((MeshCollider)collider).convex)
				{
					_makeKinematic = true;
				}
			}
			collider.enabled = flag;
		}
		_fragmentBoundSize = vector2 - vector;
		_mass = _fragmentBoundSize.x * _fragmentBoundSize.y * _fragmentBoundSize.z / 10000f;
		_localCenterOfMass = base.transform.InverseTransformPoint((vector + vector2) * 0.5f);
	}

	public Transform GetItemDropTargetTransform(GameObject raycastTarget)
	{
		return base.transform;
	}

	public void AddDroppedItem(GameObject dropTarget, OWItem item)
	{
	}

	private void FixedUpdate()
	{
		if (Time.time > _detachTime + _destructibleDelay)
		{
			if (_destructibleRoot != null)
			{
				_destructibleRoot.SetActive(value: false);
			}
			base.enabled = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_drawBounds)
		{
			DrawBounds();
		}
	}

	public void DrawBounds()
	{
		Gizmos.color = Color.red;
		Vector3 vector = base.transform.TransformPoint(_localCenterOfMass);
		Gizmos.DrawLine(vector + base.transform.up * 10f, vector - base.transform.up * 10f);
		Gizmos.DrawLine(vector + base.transform.right * 10f, vector - base.transform.right * 10f);
		Gizmos.DrawLine(vector + base.transform.forward * 10f, vector - base.transform.forward * 10f);
		Gizmos.DrawWireCube(vector, _fragmentBoundSize);
		if (_centerOfMassOverride != null)
		{
			Gizmos.DrawSphere(_centerOfMassOverride.position, 5f);
		}
	}
}
