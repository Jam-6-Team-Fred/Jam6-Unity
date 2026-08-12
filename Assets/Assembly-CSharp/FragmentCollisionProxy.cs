using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class FragmentCollisionProxy : MonoBehaviour
{
	[Serializable]
	public struct MeteorData
	{
		public MeteorController meteor;

		public Vector3 prevLocalPosition;
	}

	[Serializable]
	public struct MeteorImpact
	{
		public MeteorController meteor;

		public GameObject impactedObject;

		public Vector3 impactPosition;

		public Vector3 impactVelocity;
	}

	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private float _boundsRadius = 500f;

	[Space]
	[Space]
	[SerializeField]
	private Transform _testRaycaster;

	[SerializeField]
	private float _testRaycastLength;

	private static FragmentCollisionProxy _instance;

	private Transform _transform;

	private MeshCollider _meshCollider;

	private OWRigidbody _owRigidbody;

	private List<MeteorData> _meteors;

	private List<MeteorImpact> _meteorImpacts;

	private bool _proxyActive;

	private Vector3 _testHitPos;

	[SerializeField]
	[HideInInspector]
	private Mesh _proxyColliderMesh;

	[SerializeField]
	[HideInInspector]
	private FragmentIntegrity[] _fragmentArray;

	[SerializeField]
	[HideInInspector]
	private int[] _submeshIndices;

	private void Awake()
	{
		_instance = this;
		_transform = base.transform;
		_meshCollider = GetComponent<MeshCollider>();
		_owRigidbody = this.GetAttachedOWRigidbody();
		_meteors = new List<MeteorData>(64);
		_meteorImpacts = new List<MeteorImpact>(64);
		_proxyActive = false;
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else
		{
			Debug.LogWarning("FragmentCollisionProxy has no specified Sector!", this);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		_instance = null;
	}

	public static void TrackMeteor(MeteorController meteor)
	{
		if (_instance != null)
		{
			MeteorData item = default(MeteorData);
			item.meteor = meteor;
			item.prevLocalPosition = _instance._transform.InverseTransformPoint(meteor.owRigidbody.GetPosition());
			_instance._meteors.Add(item);
		}
	}

	public static void UntrackMeteor(MeteorController meteor)
	{
		if (!(_instance != null))
		{
			return;
		}
		for (int i = 0; i < _instance._meteors.Count; i++)
		{
			if (_instance._meteors[i].meteor == meteor)
			{
				_instance._meteors.QuickRemoveAt(i);
				break;
			}
		}
	}

	private void OnSectorOccupantsUpdated()
	{
		if ((bool)_sector)
		{
			_proxyActive = !_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Ship);
		}
	}

	private void FixedUpdate()
	{
		if (!_proxyActive)
		{
			for (int i = 0; i < _meteors.Count; i++)
			{
				MeteorData value = _meteors[i];
				value.prevLocalPosition = _transform.InverseTransformPoint(value.meteor.owRigidbody.GetPosition());
				_meteors[i] = value;
			}
			return;
		}
		float num = _boundsRadius * _boundsRadius;
		for (int j = 0; j < _meteors.Count; j++)
		{
			MeteorData value2 = _meteors[j];
			Vector3 position = value2.meteor.owRigidbody.GetPosition();
			Vector3 vector = _transform.TransformPoint(value2.prevLocalPosition);
			if ((position - _transform.position).sqrMagnitude <= num)
			{
				if (Vector3.Distance(position, vector) <= 0f)
				{
					Debug.LogError("Meteor delta position is zero", _meteors[j].meteor);
					continue;
				}
				if (_meshCollider.Raycast(new Ray(vector, position - vector), out var hitInfo, Vector3.Distance(position, vector)))
				{
					FragmentIntegrity fragmentFromRaycastHit = GetFragmentFromRaycastHit(hitInfo);
					if (fragmentFromRaycastHit == null || fragmentFromRaycastHit.GetIntegrity() <= 0f)
					{
						continue;
					}
					MeteorImpact item = default(MeteorImpact);
					item.meteor = value2.meteor;
					item.impactedObject = ((fragmentFromRaycastHit != null) ? fragmentFromRaycastHit.gameObject : _owRigidbody.gameObject);
					item.impactPosition = position;
					item.impactVelocity = value2.meteor.owRigidbody.GetVelocity() - _owRigidbody.GetPointVelocity(position);
					_meteorImpacts.Add(item);
				}
			}
			value2.prevLocalPosition = _transform.InverseTransformPoint(position);
			_meteors[j] = value2;
		}
		for (int k = 0; k < _meteorImpacts.Count; k++)
		{
			_meteorImpacts[k].meteor.Impact(_meteorImpacts[k].impactedObject, _meteorImpacts[k].impactPosition, _meteorImpacts[k].impactVelocity);
		}
		_meteorImpacts.Clear();
	}

	private void UpdateTestRaycast()
	{
		if (_meshCollider.Raycast(new Ray(_testRaycaster.position, _testRaycaster.forward), out var hitInfo, _testRaycastLength))
		{
			FragmentIntegrity fragmentFromRaycastHit = GetFragmentFromRaycastHit(hitInfo);
			if (fragmentFromRaycastHit != null)
			{
				MonoBehaviour.print("TEST HIT FRAGMENT: " + fragmentFromRaycastHit.name);
			}
			else
			{
				MonoBehaviour.print("TEST HIT SOMETHING ELSE");
			}
			_testHitPos = base.transform.InverseTransformPoint(hitInfo.point);
		}
		else
		{
			MonoBehaviour.print("TEST HIT NOTHING");
		}
	}

	private FragmentIntegrity GetFragmentFromRaycastHit(RaycastHit hitInfo)
	{
		int num = -1;
		for (int i = 0; i < _submeshIndices.Length; i++)
		{
			if (hitInfo.triangleIndex < _submeshIndices[i])
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return null;
		}
		return _fragmentArray[num];
	}

	private Vector3 GetFragmentWorldCenter(FragmentIntegrity fragment)
	{
		Vector3 vector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		Vector3 vector2 = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		Collider[] componentsInChildren = fragment.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!componentsInChildren[i].isTrigger && OWLayerMask.IsLayerInMask(componentsInChildren[i].gameObject.layer, OWLayerMask.physicalMask))
			{
				vector.x = Mathf.Min(vector.x, componentsInChildren[i].bounds.min.x);
				vector.y = Mathf.Min(vector.y, componentsInChildren[i].bounds.min.y);
				vector.z = Mathf.Min(vector.z, componentsInChildren[i].bounds.min.z);
				vector2.x = Mathf.Max(vector2.x, componentsInChildren[i].bounds.max.x);
				vector2.y = Mathf.Max(vector2.y, componentsInChildren[i].bounds.max.y);
				vector2.z = Mathf.Max(vector2.z, componentsInChildren[i].bounds.max.z);
			}
		}
		return (vector + vector2) * 0.5f;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, _boundsRadius);
		}
	}

	private void OnDrawGizmos()
	{
		if (_proxyActive && Application.isPlaying)
		{
			Gizmos.color = Color.red;
			if (_testRaycaster != null)
			{
				Gizmos.DrawLine(_testRaycaster.position, _testRaycaster.position + _testRaycaster.forward * _testRaycastLength);
				Gizmos.DrawSphere(base.transform.TransformPoint(_testHitPos), 2f);
			}
		}
	}
}
