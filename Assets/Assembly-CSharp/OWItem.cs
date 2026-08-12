using UnityEngine;

public abstract class OWItem : SectoredMonoBehaviour
{
	[SerializeField]
	protected float _interactRange = 2f;

	[SerializeField]
	private Vector3 _localDropOffset = Vector3.zero;

	[SerializeField]
	private Vector3 _localDropNormal = Vector3.up;

	protected ItemType _type;

	[SerializeField]
	[HideInInspector]
	private bool _prebuilt;

	[SerializeField]
	[HideInInspector]
	protected OWCollider[] _colliders;

	[SerializeField]
	[HideInInspector]
	protected OWRenderer[] _renderers;

	[SerializeField]
	[HideInInspector]
	protected ParticleSystem[] _particleSystems;

	[SerializeField]
	[HideInInspector]
	protected OWLight2[] _lights;

	protected bool _visible = true;

	protected bool _interactable = true;

	protected DetachableFragment _parentFragment;

	public OWEvent<OWItem> onPickedUp;

	[ContextMenu("Clear Built State")]
	private void ClearBuiltState()
	{
		_prebuilt = false;
		_colliders = null;
		_renderers = null;
		_particleSystems = null;
		_lights = null;
	}

	protected override void Awake()
	{
		base.Awake();
		if (!_prebuilt)
		{
			FindComponentsInHierarchy();
		}
		_parentFragment = GetComponentInParent<DetachableFragment>();
		if (_parentFragment != null)
		{
			_parentFragment.OnChangeSector += OnParentFragmentChangeSector;
		}
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_parentFragment != null)
		{
			_parentFragment.OnChangeSector -= OnParentFragmentChangeSector;
			_parentFragment = null;
		}
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
	}

	private void FindComponentsInHierarchy()
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		_colliders = new OWCollider[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			_colliders[i] = componentsInChildren[i].gameObject.GetAddComponent<OWCollider>();
		}
		Renderer[] componentsInChildren2 = GetComponentsInChildren<Renderer>();
		_renderers = new OWRenderer[componentsInChildren2.Length];
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			_renderers[j] = componentsInChildren2[j].gameObject.GetAddComponent<OWRenderer>();
		}
		_particleSystems = GetComponentsInChildren<ParticleSystem>();
		_lights = GetComponentsInChildren<OWLight2>();
	}

	public ItemType GetItemType()
	{
		return _type;
	}

	public float GetInteractRange()
	{
		return _interactRange;
	}

	public virtual bool IsInteractable()
	{
		return _interactable;
	}

	public virtual void EnableInteraction(bool value)
	{
		_interactable = value;
	}

	public abstract string GetDisplayName();

	public void SetColliderActivation(bool active)
	{
		for (int i = 0; i < _colliders.Length; i++)
		{
			if (_colliders[i] != null)
			{
				_colliders[i].SetActivation(active);
			}
		}
	}

	public void MoveAndChildToTransform(Transform socketTransform)
	{
		base.transform.parent = socketTransform;
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		base.transform.localScale = Vector3.one;
		SetColliderActivation(active: false);
	}

	public virtual void SocketItem(Transform socketTransform, Sector sector)
	{
		SetSector(sector);
		MoveAndChildToTransform(socketTransform);
	}

	public virtual void PickUpItem(Transform holdTranform)
	{
		if (_parentFragment != null)
		{
			_parentFragment.OnChangeSector -= OnParentFragmentChangeSector;
		}
		for (int i = 0; i < _colliders.Length; i++)
		{
			if (_colliders[i] != null)
			{
				_colliders[i].ClearParentBody();
			}
		}
		_parentFragment = null;
		SetSector(null);
		MoveAndChildToTransform(holdTranform);
		onPickedUp.Invoke(this);
	}

	public virtual bool CheckIsDroppable()
	{
		return true;
	}

	public virtual void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
	{
		base.transform.SetParent(parent);
		base.transform.localScale = Vector3.one;
		Quaternion quaternion = Quaternion.FromToRotation(base.transform.TransformDirection(_localDropNormal), normal);
		base.transform.rotation = quaternion * base.transform.rotation;
		base.transform.position = position + base.transform.TransformDirection(_localDropOffset);
		if (_parentFragment != null)
		{
			_parentFragment.OnChangeSector -= OnParentFragmentChangeSector;
		}
		_parentFragment = customDropTarget as DetachableFragment;
		if (_parentFragment != null)
		{
			_parentFragment.OnChangeSector += OnParentFragmentChangeSector;
		}
		SetSector(sector);
		SetColliderActivation(active: true);
	}

	public virtual bool IsAnimationPlaying()
	{
		return false;
	}

	public virtual void PlaySocketAnimation()
	{
	}

	public virtual void PlayUnsocketAnimation()
	{
	}

	public virtual void OnCompleteUnsocket()
	{
	}

	private void SetVisible(bool visible)
	{
		if (_visible == visible)
		{
			return;
		}
		_visible = visible;
		for (int i = 0; i < _renderers.Length; i++)
		{
			if (_renderers[i] != null)
			{
				_renderers[i].SetLODActivation(visible);
			}
		}
		for (int j = 0; j < _particleSystems.Length; j++)
		{
			if (_particleSystems[j] != null)
			{
				if (visible)
				{
					_particleSystems[j].Play(withChildren: true);
				}
				else
				{
					_particleSystems[j].Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
				}
			}
		}
		for (int k = 0; k < _lights.Length; k++)
		{
			if (_lights[k] != null)
			{
				_lights[k].SetLODActivation(visible);
			}
		}
	}

	protected virtual void UpdateCollisionLOD()
	{
		for (int i = 0; i < _colliders.Length; i++)
		{
			if (_colliders[i] != null)
			{
				if (_sector != null)
				{
					_colliders[i].CheckLODActivation(_sector.GetOccupantMask());
				}
				else
				{
					_colliders[i].SetLODLevel(0);
				}
			}
		}
	}

	private void UpdateVisualsLOD()
	{
		bool visible = _sector == null || _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		SetVisible(visible);
	}

	protected override void OnChangeSector(Sector oldSector, Sector newSector)
	{
		UpdateVisualsLOD();
		UpdateCollisionLOD();
	}

	protected override void OnSectorOccupantsUpdated()
	{
		UpdateVisualsLOD();
		UpdateCollisionLOD();
	}

	protected virtual void OnParentFragmentChangeSector(Sector newParentSector)
	{
		if (_sector != _parentFragment.GetSector())
		{
			SetSector(newParentSector);
		}
	}

	private void OnEnterMapView()
	{
		SetVisible(visible: false);
	}

	private void OnExitMapView()
	{
		UpdateVisualsLOD();
	}
}
