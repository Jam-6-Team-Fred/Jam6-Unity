using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RepairReceiver : MonoBehaviour
{
	public enum Type
	{
		ShipComponent = 0,
		ShipHull = 1,
		SatelliteNode = 2
	}

	private OWCollider _owCollider;

	[SerializeField]
	private Type _type;

	[SerializeField]
	private ShipComponent _targetComponent;

	[SerializeField]
	private ShipHull _targetHull;

	[SerializeField]
	private SatelliteNode _targetSatNode;

	[SerializeField]
	private float _repairDistance = 3f;

	private ShipDamageController _shipDamageController;

	public Type type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	public ShipComponent targetComponent
	{
		get
		{
			return _targetComponent;
		}
		set
		{
			_targetComponent = value;
		}
	}

	public ShipHull targetHull
	{
		get
		{
			return _targetHull;
		}
		set
		{
			_targetHull = value;
		}
	}

	public SatelliteNode targetSatNode
	{
		get
		{
			return _targetSatNode;
		}
		set
		{
			_targetSatNode = value;
		}
	}

	public float repairDistance
	{
		get
		{
			return _repairDistance;
		}
		set
		{
			_repairDistance = value;
		}
	}

	private void Awake()
	{
		_owCollider = base.gameObject.GetAddComponent<OWCollider>();
	}

	private void Start()
	{
		if (_type == Type.ShipComponent || _type == Type.ShipHull)
		{
			_shipDamageController = Locator.GetShipBody().GetComponent<ShipDamageController>();
		}
	}

	public bool IsRepairable()
	{
		switch (_type)
		{
		case Type.ShipComponent:
			if (_targetComponent.isDamaged && PlayerState.IsInsideShip() == _targetComponent.isInternalRepairPoint)
			{
				return !_shipDamageController.IsDestroyed();
			}
			return false;
		case Type.ShipHull:
			if (_targetHull.isDamaged && !PlayerState.IsInsideShip())
			{
				return !_shipDamageController.IsDestroyed();
			}
			return false;
		case Type.SatelliteNode:
			return _targetSatNode.isDamaged;
		default:
			return false;
		}
	}

	public void RepairTick()
	{
		switch (_type)
		{
		case Type.ShipComponent:
			_targetComponent.RepairTick();
			break;
		case Type.ShipHull:
			_targetHull.RepairTick();
			break;
		case Type.SatelliteNode:
			_targetSatNode.RepairTick();
			break;
		}
	}

	public bool IsDamaged()
	{
		switch (_type)
		{
		case Type.ShipComponent:
			return _targetComponent.isDamaged;
		case Type.ShipHull:
			return _targetHull.isDamaged;
		case Type.SatelliteNode:
			return _targetSatNode.isDamaged;
		default:
			return false;
		}
	}

	public UITextType GetRepairableName()
	{
		switch (_type)
		{
		case Type.ShipComponent:
			return _targetComponent.componentName;
		case Type.ShipHull:
			return _targetHull.hullName;
		case Type.SatelliteNode:
			return UITextType.SatelliteNodeName;
		default:
			return UITextType.ShipPartUnknown;
		}
	}

	public float GetRepairFraction()
	{
		switch (_type)
		{
		case Type.ShipComponent:
			return _targetComponent.repairFraction;
		case Type.ShipHull:
			return _targetHull.integrity;
		case Type.SatelliteNode:
			return _targetSatNode.repairFraction;
		default:
			return -1f;
		}
	}

	public void EnableCollider()
	{
		_owCollider.SetActivation(active: true);
	}

	public void DisableCollider()
	{
		_owCollider.SetActivation(active: false);
	}
}
