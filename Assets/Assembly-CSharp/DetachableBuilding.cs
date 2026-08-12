using UnityEngine;

public class DetachableBuilding : MonoBehaviour, IItemDropTarget
{
	[SerializeField]
	private float _mass = 10f;

	[Space]
	[SerializeField]
	private ProxyShadowCaster[] _proxyShadowCasters = new ProxyShadowCaster[0];

	private OWRigidbody _buildingBody;

	private bool _detached;

	public OWEvent OnDetachBuilding;

	public OWEvent OnReattachBuilding;

	public OWRigidbody buildingBody => _buildingBody;

	public bool isDetached => _detached;

	public ProxyShadowCaster[] proxyShadowCasters => _proxyShadowCasters;

	public void Detach(bool kinematicSimulation)
	{
		if (!_detached)
		{
			base.gameObject.AddComponent<Rigidbody>();
			_buildingBody = base.gameObject.AddComponent<OWRigidbody>();
			_buildingBody.SetMass(_mass);
			_buildingBody.GetRigidbody().angularDrag = 0f;
			if (kinematicSimulation)
			{
				_buildingBody.MakeKinematic();
				_buildingBody.EnableKinematicSimulation();
			}
			_buildingBody.SetVelocity(_buildingBody.GetOrigParentBody().GetPointVelocity(_buildingBody.GetPosition()));
			_buildingBody.SetAngularVelocity(_buildingBody.GetOrigParentBody().GetAngularVelocity());
			for (int i = 0; i < _proxyShadowCasters.Length; i++)
			{
				_proxyShadowCasters[i].SetDynamic(dynamic: true);
			}
			_detached = true;
			OnDetachBuilding.Invoke();
		}
	}

	public void Reattach()
	{
		if (_detached)
		{
			_buildingBody.transform.parent = _buildingBody.GetOrigParent();
			Rigidbody rigidbody = _buildingBody.GetRigidbody();
			KinematicRigidbody component = _buildingBody.GetComponent<KinematicRigidbody>();
			Object.Destroy(_buildingBody);
			_buildingBody = null;
			if (component != null)
			{
				Object.Destroy(component);
			}
			Object.Destroy(rigidbody);
			for (int i = 0; i < _proxyShadowCasters.Length; i++)
			{
				_proxyShadowCasters[i].SetDynamic(dynamic: false);
			}
			_detached = false;
			OnReattachBuilding.Invoke();
		}
	}

	public Transform GetItemDropTargetTransform(GameObject raycastTarget)
	{
		return base.transform;
	}

	public void AddDroppedItem(GameObject dropTarget, OWItem item)
	{
	}
}
