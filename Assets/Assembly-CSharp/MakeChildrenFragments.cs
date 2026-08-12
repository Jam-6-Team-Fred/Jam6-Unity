using UnityEngine;

public class MakeChildrenFragments : MonoBehaviour
{
	[SerializeField]
	private float _integrity = 100f;

	[SerializeField]
	private float _propagateToChildFraction = 1f;

	[SerializeField]
	private Material _fractureMaterial;

	[SerializeField]
	private float _fractionDetachable = 1f;

	[SerializeField]
	private float _drag = 10f;

	[SerializeField]
	private DetachableFragment.ForceMask _forceDetection = DetachableFragment.ForceMask.ParentOnly;

	[Space]
	[SerializeField]
	private bool _addCullGroup;

	[SerializeField]
	private Sector _cullGroupSector;

	[SerializeField]
	private bool _addCollisionGroup;

	[SerializeField]
	private Sector _collisionGroupSector;

	[SerializeField]
	private bool _addLightGroup;

	[SerializeField]
	private Sector _lightGroupSector;

	private void Awake()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			MakeFragment(base.transform.GetChild(i).gameObject);
		}
		Object.Destroy(this);
	}

	private void MakeFragment(GameObject childObject)
	{
		FragmentIntegrity fragmentIntegrity = childObject.AddComponent<FragmentIntegrity>();
		childObject.AddComponent<FragmentProxy>();
		if (_addCullGroup)
		{
			childObject.AddComponent<SectorCullGroup>().SetSector(_cullGroupSector);
		}
		if (_addCollisionGroup)
		{
			childObject.AddComponent<SectorCollisionGroup>().SetSector(_collisionGroupSector);
		}
		if (_addLightGroup)
		{
			childObject.AddComponent<SectorLightsCullGroup>().SetSector(_lightGroupSector);
		}
		fragmentIntegrity.Init(_integrity, _propagateToChildFraction, _fractureMaterial);
		if (Random.Range(0f, 1f) < _fractionDetachable)
		{
			childObject.AddComponent<DetachableFragment>().Init(_drag, _forceDetection);
		}
		else
		{
			childObject.AddComponent<DestructibleFragment>();
		}
	}
}
