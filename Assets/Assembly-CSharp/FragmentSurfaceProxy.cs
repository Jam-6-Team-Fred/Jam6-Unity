using System;
using System.Collections.Generic;
using UnityEngine;

public class FragmentSurfaceProxy : MonoBehaviour
{
	private static FragmentSurfaceProxy _instance;

	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private float _innerRadius = 400f;

	[SerializeField]
	private float _outerRadius = 500f;

	[SerializeField]
	private float _northConeDegrees;

	[SerializeField]
	private float _southConeDegrees;

	private OWRigidbody _owRigidbody;

	private List<MeteorController> _meteors;

	private FragmentProxy[] _fragmentProxies;

	private Vector3[] _localCenters;

	private void Awake()
	{
		_instance = this;
		_owRigidbody = this.GetAttachedOWRigidbody();
		_meteors = new List<MeteorController>(64);
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else
		{
			Debug.LogWarning("FragmentSurfaceProxy has no specified Sector!", this);
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

	private void Start()
	{
		_fragmentProxies = UnityEngine.Object.FindObjectsOfType<FragmentProxy>();
		_localCenters = new Vector3[_fragmentProxies.Length];
		for (int i = 0; i < _fragmentProxies.Length; i++)
		{
			_localCenters[i] = base.transform.InverseTransformPoint(_fragmentProxies[i].worldCenter);
		}
		base.enabled = false;
	}

	public static void TrackMeteor(MeteorController meteor)
	{
		if (_instance != null)
		{
			_instance._meteors.Add(meteor);
		}
	}

	public static void UntrackMeteor(MeteorController meteor)
	{
		if (_instance != null)
		{
			_instance._meteors.QuickRemove(meteor);
		}
	}

	private void OnSectorOccupantsUpdated()
	{
		if ((bool)_sector)
		{
			base.enabled = !_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Ship);
		}
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < _meteors.Count; i++)
		{
			Vector3 position = _meteors[i].GetAttachedOWRigidbody().GetPosition();
			Vector3 from = position - base.transform.position;
			if (!(from.sqrMagnitude < _outerRadius * _outerRadius) || !(from.sqrMagnitude > _innerRadius * _innerRadius))
			{
				continue;
			}
			float num = Vector3.Angle(from, base.transform.up);
			if (num < _northConeDegrees || num > 180f - _southConeDegrees)
			{
				Vector3 impactVel = _owRigidbody.GetPointVelocity(position) - _meteors[i].owRigidbody.GetVelocity();
				_meteors[i].Impact(base.gameObject, position, impactVel);
				MonoBehaviour.print("meteor hit pole " + num);
				continue;
			}
			Vector3 vector = base.transform.InverseTransformPoint(position);
			FragmentProxy fragmentProxy = null;
			float num2 = float.PositiveInfinity;
			for (int j = 0; j < _fragmentProxies.Length; j++)
			{
				float sqrMagnitude = (vector - _localCenters[j]).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					fragmentProxy = _fragmentProxies[j];
					num2 = sqrMagnitude;
				}
			}
			if (fragmentProxy != null && fragmentProxy.IsIntact())
			{
				MonoBehaviour.print("meteor hit fragment " + fragmentProxy.name);
				Vector3 impactVel2 = _owRigidbody.GetPointVelocity(position) - _meteors[i].owRigidbody.GetVelocity();
				_meteors[i].Impact(fragmentProxy.gameObject, position, impactVel2);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!OWGizmos.IsDirectlySelected(base.gameObject))
		{
			return;
		}
		Gizmos.color = new Color(1f, 0f, 1f, 1f);
		Gizmos.DrawWireSphere(base.transform.position, _innerRadius);
		Gizmos.DrawWireSphere(base.transform.position, _outerRadius);
		if (_localCenters != null)
		{
			for (int i = 0; i < _localCenters.Length; i++)
			{
				Vector3 vector = base.transform.TransformPoint(_localCenters[i]);
				Vector3 vector2 = vector - base.transform.position;
				Gizmos.DrawRay(vector, vector2.normalized * ((_outerRadius - vector2.magnitude) * 2f));
			}
		}
		float num = _outerRadius * Mathf.Cos(_northConeDegrees * ((float)Math.PI / 180f));
		float radius = _outerRadius * Mathf.Sin(_northConeDegrees * ((float)Math.PI / 180f));
		OWGizmos.DrawWireCircle(base.transform.position + base.transform.up * num, base.transform.up, radius);
		float num2 = _outerRadius * Mathf.Cos(_southConeDegrees * ((float)Math.PI / 180f));
		float radius2 = _outerRadius * Mathf.Sin(_southConeDegrees * ((float)Math.PI / 180f));
		OWGizmos.DrawWireCircle(base.transform.position + -base.transform.up * num2, -base.transform.up, radius2);
	}
}
