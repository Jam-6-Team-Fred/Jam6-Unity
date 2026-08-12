using System.Collections.Generic;
using UnityEngine;

public class CenterOfTheUniverse : MonoBehaviour
{
	private static CenterOfTheUniverse s_instance = null;

	private static List<OWRigidbody> s_rigidbodies = new List<OWRigidbody>(256);

	private static bool s_universeDeactivated = true;

	[SerializeField]
	private OWRigidbody _staticReferenceFrame;

	private OWRigidbody _centerBody;

	private bool _recenterUniverseNextUpdate;

	private Vector3 _cachedOffsetVelocity = Vector3.zero;

	private void Awake()
	{
		s_instance = this;
		s_universeDeactivated = false;
		_recenterUniverseNextUpdate = true;
	}

	private void Start()
	{
		if (_staticReferenceFrame == null)
		{
			Debug.LogError("No static reference frame set!", this);
			Debug.Break();
		}
		if (Locator.GetPlayerTransform() != null)
		{
			_centerBody = Locator.GetPlayerTransform().GetComponent<OWRigidbody>();
			GlobalMessenger.AddListener("PlayerRepositioned", OnPlayerRepositioned);
		}
	}

	private void OnDestroy()
	{
		s_instance = null;
		s_rigidbodies.Clear();
		GlobalMessenger.RemoveListener("PlayerRepositioned", OnPlayerRepositioned);
	}

	private void OnEnable()
	{
		if (!_recenterUniverseNextUpdate)
		{
			RecenterUniverseAroundPlayer();
		}
	}

	private void OnDisable()
	{
		Vector3 velocityChange = ((_staticReferenceFrame != null) ? (-_staticReferenceFrame.GetRigidbody().velocity) : Vector3.zero);
		for (int i = 0; i < s_rigidbodies.Count; i++)
		{
			OWRigidbody oWRigidbody = s_rigidbodies[i];
			if (oWRigidbody != null && oWRigidbody.transform.parent == null && oWRigidbody.gameObject.activeInHierarchy)
			{
				oWRigidbody.AddVelocityChange(velocityChange);
			}
		}
	}

	public static bool IsUniverseDeactivated()
	{
		return s_universeDeactivated;
	}

	public static void TrackRigidbody(OWRigidbody owRigidbody)
	{
		s_rigidbodies.Add(owRigidbody);
	}

	public static void UntrackRigidbody(OWRigidbody owRigidbody)
	{
		s_rigidbodies.QuickRemove(owRigidbody);
	}

	public static void DeactivateUniverse()
	{
		s_universeDeactivated = true;
		for (int i = 0; i < s_rigidbodies.Count; i++)
		{
			if (s_rigidbodies[i] != null)
			{
				s_rigidbodies[i].gameObject.SetActive(value: false);
			}
		}
		s_instance.gameObject.SetActive(value: false);
	}

	public Vector3 GetStaticFrameVelocity_Internal()
	{
		if (!base.enabled || !(_staticReferenceFrame != null))
		{
			return Vector3.zero;
		}
		return _staticReferenceFrame.GetVelocity_Internal();
	}

	public OWRigidbody GetStaticReferenceFrame()
	{
		return _staticReferenceFrame;
	}

	public void SetStaticReferenceFrame(OWRigidbody staticReferenceFrame)
	{
		_staticReferenceFrame = staticReferenceFrame;
	}

	public Vector3 GetOffsetVelocity()
	{
		if (!base.enabled)
		{
			return Vector3.zero;
		}
		return _cachedOffsetVelocity;
	}

	public Vector3 GetOffsetPosition()
	{
		if (!(_staticReferenceFrame != null))
		{
			return Vector3.zero;
		}
		return _staticReferenceFrame.GetPosition();
	}

	private void FixedUpdate()
	{
		_cachedOffsetVelocity = (base.enabled ? (-_centerBody.GetVelocity_Internal()) : Vector3.zero);
	}

	private void LateUpdate()
	{
		if (_recenterUniverseNextUpdate)
		{
			RecenterUniverseAroundPlayer();
			_recenterUniverseNextUpdate = false;
		}
	}

	private void OnPlayerRepositioned()
	{
		_recenterUniverseNextUpdate = true;
	}

	private void RecenterUniverseAroundPlayer()
	{
		Vector3 position = _centerBody.transform.position;
		for (int i = 0; i < s_rigidbodies.Count; i++)
		{
			OWRigidbody oWRigidbody = s_rigidbodies[i];
			if (oWRigidbody != null && oWRigidbody.transform.parent == null)
			{
				oWRigidbody.transform.position -= position;
			}
		}
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
		MonoBehaviour.print("UNIVERSE RECENTERED");
	}
}
