using System.Collections.Generic;
using UnityEngine;

public class OWTriggerVolume : MonoBehaviour
{
	public delegate void OWTriggerObjectEvent(GameObject hitObj);

	[SerializeField]
	private GameObject[] _initialOccupants = new GameObject[0];

	[SerializeField]
	private EntrywayTrigger[] _sharedEntryways = new EntrywayTrigger[0];

	[SerializeField]
	private bool _ignoreDuplicateOccupantWarning;

	protected OWCollider _owCollider;

	protected OWCustomCollider _owCustomCollider;

	protected CompoundTriggerVolume _compoundTrigger;

	protected Shape _shape;

	protected List<EntrywayTrigger> _childEntryways;

	protected List<GameObject> _trackedObjects;

	protected bool _active = true;

	public event OWTriggerObjectEvent OnEntry;

	public event OWTriggerObjectEvent OnExit;

	public List<GameObject> getTrackedObjects()
	{
		return _trackedObjects;
	}

	protected virtual void Reset()
	{
		Collider component = GetComponent<Collider>();
		if (component != null)
		{
			component.isTrigger = true;
			if (!OWLayerMask.IsLayerInMask(base.gameObject.layer, OWLayerMask.effectVolumeMask))
			{
				base.gameObject.layer = LayerMask.NameToLayer("BasicEffectVolume");
			}
		}
		Shape component2 = GetComponent<Shape>();
		if (component2 != null)
		{
			component2.SetCollisionMode(Shape.CollisionMode.Volume);
		}
	}

	protected virtual void Awake()
	{
		_trackedObjects = new List<GameObject>(8);
		Collider component = GetComponent<Collider>();
		if (component != null)
		{
			component.Assert(OWLayerMask.effectVolumeMask, isTrigger: true);
			_owCollider = base.gameObject.GetAddComponent<OWCollider>();
			_owCollider.OnColliderDisabled += OnAttachedColliderDisabled;
		}
		_owCustomCollider = GetComponent<OWCustomCollider>();
		if (_owCustomCollider != null)
		{
			_owCustomCollider.OnEntry += AddObjectToVolume;
			_owCustomCollider.OnExit += RemoveObjectFromVolume;
		}
		_compoundTrigger = GetComponent<CompoundTriggerVolume>();
		if (_compoundTrigger != null)
		{
			_compoundTrigger.OnEntry += AddObjectToVolume;
			_compoundTrigger.OnExit += RemoveObjectFromVolume;
		}
		_shape = GetComponent<Shape>();
		if (_shape != null)
		{
			_shape.OnCollisionEnter += OnShapeCollisionEnter;
			_shape.OnCollisionExit += OnShapeCollisionExit;
			_shape.OnShapeDeactivated += OnShapeDeactivated;
		}
		_childEntryways = new List<EntrywayTrigger>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			EntrywayTrigger component2 = base.transform.GetChild(i).GetComponent<EntrywayTrigger>();
			if (component2 != null)
			{
				_childEntryways.Add(component2);
				component2.Register();
				component2.OnEntry += AddObjectToVolume;
				component2.OnExit += RemoveObjectFromVolume;
			}
		}
		for (int j = 0; j < _sharedEntryways.Length; j++)
		{
			_sharedEntryways[j].Register();
			_sharedEntryways[j].OnEntry += AddObjectToVolume;
			_sharedEntryways[j].OnExit += RemoveObjectFromVolume;
		}
	}

	protected virtual void Start()
	{
		if (_initialOccupants != null)
		{
			for (int i = 0; i < _initialOccupants.Length; i++)
			{
				AddObjectToVolume(_initialOccupants[i]);
			}
		}
	}

	private void OnDisable()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			RemoveAllObjectsFromVolume();
		}
	}

	protected virtual void OnDestroy()
	{
		if (_owCollider != null)
		{
			_owCollider.OnColliderDisabled -= OnAttachedColliderDisabled;
		}
		if (_owCustomCollider != null)
		{
			_owCustomCollider.OnEntry -= AddObjectToVolume;
			_owCustomCollider.OnExit -= RemoveObjectFromVolume;
		}
		if (_compoundTrigger != null)
		{
			_compoundTrigger.OnEntry -= AddObjectToVolume;
			_compoundTrigger.OnExit -= RemoveObjectFromVolume;
		}
		if (_shape != null)
		{
			_shape.OnCollisionEnter -= OnShapeCollisionEnter;
			_shape.OnCollisionExit -= OnShapeCollisionExit;
			_shape.OnShapeDeactivated -= OnShapeDeactivated;
		}
		for (int i = 0; i < _childEntryways.Count; i++)
		{
			_childEntryways[i].OnEntry -= AddObjectToVolume;
			_childEntryways[i].OnExit -= RemoveObjectFromVolume;
		}
		for (int j = 0; j < _sharedEntryways.Length; j++)
		{
			_sharedEntryways[j].OnEntry -= AddObjectToVolume;
			_sharedEntryways[j].OnExit -= RemoveObjectFromVolume;
		}
	}

	public void SetTriggerActivation(bool active)
	{
		if (_active != active)
		{
			_active = active;
			if (!_active)
			{
				RemoveAllObjectsFromVolume();
			}
			if (_owCollider != null)
			{
				_owCollider.SetActivation(active);
			}
			if (_compoundTrigger != null)
			{
				_compoundTrigger.SetActivation(active);
			}
			if (_shape != null)
			{
				_shape.SetActivation(active);
			}
			for (int i = 0; i < _childEntryways.Count; i++)
			{
				_childEntryways[i].SetActivation(active);
			}
			for (int j = 0; j < _sharedEntryways.Length; j++)
			{
				_sharedEntryways[j].SetActivation(active);
			}
		}
	}

	public float GetPenetrationDistance(Vector3 worldPos)
	{
		if (_childEntryways.Count > 0 || _sharedEntryways.Length != 0)
		{
			Debug.LogError("Cannot get penetration distance into OWTriggerVolumes using EntrywayTriggers!");
			return 0f;
		}
		if (_owCollider != null)
		{
			Collider collider = _owCollider.GetCollider();
			if (collider is SphereCollider)
			{
				return OWPhysics.GetDistToSurface(collider as SphereCollider, worldPos);
			}
			if (collider is CapsuleCollider)
			{
				return OWPhysics.GetDistToSurface(collider as CapsuleCollider, worldPos);
			}
			Debug.LogError("Cannot get penetration distance into an OWCollider that isn't a SphereCollider or CapsuleCollider!");
			return 0f;
		}
		if (_compoundTrigger != null)
		{
			Debug.LogError("Cannot get penetration distance into CompoundTriggers!");
			return 0f;
		}
		if (_shape != null)
		{
			return _shape.PenetrationDistance(worldPos);
		}
		return 0f;
	}

	public float GetDistanceToBoundary(Vector3 worldPos)
	{
		float num = float.PositiveInfinity;
		if (_owCollider != null)
		{
			num = ((_owCustomCollider != null) ? _owCustomCollider.GetDistToSurface(worldPos) : ((!(_owCollider.GetCollider().GetType() == typeof(SphereCollider))) ? Vector3.Distance(worldPos, base.transform.position) : Mathf.Max(0f, Vector3.Distance(worldPos, base.transform.position) - ((SphereCollider)_owCollider.GetCollider()).radius)));
		}
		if (_compoundTrigger != null)
		{
			Debug.LogError("Cannot calculate distance to compound triggers yet!");
		}
		if (_shape != null)
		{
			num = Mathf.Min(num, Mathf.Max(0f, _shape.PenetrationDistance(worldPos)));
		}
		for (int i = 0; i < _childEntryways.Count; i++)
		{
			num = Mathf.Min(num, Vector3.Distance(worldPos, _childEntryways[i].transform.position));
		}
		for (int j = 0; j < _sharedEntryways.Length; j++)
		{
			num = Mathf.Min(num, Vector3.Distance(worldPos, _sharedEntryways[j].transform.position));
		}
		return num;
	}

	public OWCollider GetOWCollider()
	{
		return _owCollider;
	}

	public OWCustomCollider GetOWCustomCollider()
	{
		return _owCustomCollider;
	}

	public Shape GetShape()
	{
		return _shape;
	}

	public bool IsTrackingObject(GameObject obj)
	{
		return _trackedObjects.Contains(obj);
	}

	public virtual void AddObjectToVolume(GameObject hitObj)
	{
		if (!_active)
		{
			return;
		}
		if (_trackedObjects.SafeAdd(hitObj))
		{
			AddHitObjectListeners(hitObj);
			FireEntryEvent(hitObj);
			return;
		}
		Debug.LogWarning("OWTriggerVolume " + base.gameObject.name + " already contains " + hitObj.name, this);
		if (!_ignoreDuplicateOccupantWarning && _childEntryways.Count == 0 && _sharedEntryways.Length == 0)
		{
			Debug.Break();
		}
	}

	public virtual void RemoveObjectFromVolume(GameObject hitObj)
	{
		if (_trackedObjects.Remove(hitObj))
		{
			RemoveHitObjectListeners(hitObj);
			FireExitEvent(hitObj);
		}
	}

	protected void AddHitObjectListeners(GameObject hitObj)
	{
		OWCollider component = hitObj.GetComponent<OWCollider>();
		if (component != null)
		{
			component.OnColliderDisabled += OnTrackedColliderDisabled;
		}
		else if (hitObj.GetComponent<Collider>() != null)
		{
			component = hitObj.AddComponent<OWCollider>();
			component.OnColliderDisabled += OnTrackedColliderDisabled;
		}
		if (hitObj.CompareTag("PlayerDetector"))
		{
			if (_childEntryways.Count > 0 || _sharedEntryways.Length != 0)
			{
				GlobalMessenger<SurveyorProbe>.AddListener("LaunchProbe", OnLaunchProbe);
			}
			GlobalMessenger.AddListener("WarpPlayer", OnWarpPlayer);
		}
		else if (hitObj.CompareTag("DreamLanternDetector"))
		{
			hitObj.GetComponent<LanternFluidDetector>().GetShape().OnShapeDeactivated += OnTrackedDreamLanternShapeDeactivated;
		}
	}

	protected void RemoveHitObjectListeners(GameObject hitObj)
	{
		if (hitObj == null)
		{
			return;
		}
		OWCollider component = hitObj.GetComponent<OWCollider>();
		if (component != null)
		{
			component.OnColliderDisabled -= OnTrackedColliderDisabled;
		}
		if (hitObj.CompareTag("PlayerDetector"))
		{
			if (_childEntryways.Count > 0 || _sharedEntryways.Length != 0)
			{
				GlobalMessenger<SurveyorProbe>.RemoveListener("LaunchProbe", OnLaunchProbe);
			}
			GlobalMessenger.RemoveListener("WarpPlayer", OnWarpPlayer);
		}
		else if (hitObj.CompareTag("DreamLanternDetector"))
		{
			hitObj.GetComponent<LanternFluidDetector>().GetShape().OnShapeDeactivated -= OnTrackedDreamLanternShapeDeactivated;
		}
	}

	protected void FireEntryEvent(GameObject hitObj)
	{
		if (this.OnEntry != null)
		{
			this.OnEntry(hitObj);
		}
	}

	protected void FireExitEvent(GameObject hitObj)
	{
		if (this.OnExit != null)
		{
			this.OnExit(hitObj);
		}
	}

	protected void OnShapeCollisionEnter(Shape hitShape)
	{
		AddObjectToVolume(hitShape.gameObject);
	}

	protected void OnShapeCollisionExit(Shape hitShape)
	{
		RemoveObjectFromVolume(hitShape.gameObject);
	}

	private void RemoveAllObjectsFromVolume()
	{
		for (int num = _trackedObjects.Count - 1; num >= 0; num--)
		{
			RemoveObjectFromVolume(_trackedObjects[num]);
		}
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		if (_owCollider != null && _owCustomCollider == null)
		{
			AddObjectToVolume(hitCollider.gameObject);
		}
	}

	private void OnTriggerExit(Collider hitCollider)
	{
		if (_owCollider != null && _owCustomCollider == null)
		{
			RemoveObjectFromVolume(hitCollider.gameObject);
		}
	}

	private void OnTrackedColliderDisabled(OWCollider collider)
	{
		if (_compoundTrigger != null)
		{
			_compoundTrigger.UntrackCollider(collider.GetCollider());
		}
		if (_owCustomCollider != null)
		{
			_owCustomCollider.UntrackTransform(collider.transform);
		}
		RemoveObjectFromVolume(collider.gameObject);
	}

	private void OnWarpPlayer()
	{
		if (_childEntryways.Count > 0 || _sharedEntryways.Length != 0)
		{
			GameObject hitObj = Locator.GetPlayerDetector().gameObject;
			GameObject hitObj2 = Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>().gameObject;
			RemoveObjectFromVolume(hitObj);
			RemoveObjectFromVolume(hitObj2);
		}
	}

	private void OnLaunchProbe(SurveyorProbe probe)
	{
		if (!PlayerState.AtFlightConsole())
		{
			AddObjectToVolume(probe.GetDetectorObject());
		}
	}

	private void OnAttachedColliderDisabled(OWCollider collider)
	{
		RemoveAllObjectsFromVolume();
	}

	private void OnShapeDeactivated(Shape shape)
	{
		RemoveAllObjectsFromVolume();
	}

	private void OnTrackedDreamLanternShapeDeactivated(Shape shape)
	{
		RemoveObjectFromVolume(shape.gameObject);
	}
}
