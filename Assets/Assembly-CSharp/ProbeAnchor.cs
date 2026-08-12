using System.Text;
using UnityEngine;

public class ProbeAnchor : MonoBehaviour
{
	public delegate void ProbeAnchorEvent();

	private const float ACTIVATION_DELAY = 0.2f;

	[SerializeField]
	private SphereCollider _collider;

	private SurveyorProbe _probe;

	private OWRigidbody _probeBody;

	private AlignWithDirection _probeAlignment;

	private FragmentIntegrity _breakableFragment;

	private SandLevelController _sandLevelController;

	private DamDestructionController _ringworldDam;

	private Vector3 _localImpactPos;

	private float _anchorTime;

	private bool _anchored;

	private float _launchTime;

	private float _unanchoredTime;

	private NotificationData _probeNotification;

	private bool _notificationPosted;

	private StringBuilder _strBuilder;

	public event ProbeAnchorEvent OnAnchorToSurface;

	public event ProbeAnchorEvent OnUnanchorFromSurface;

	private void Awake()
	{
		_collider.enabled = false;
		_probeBody = this.GetAttachedOWRigidbody();
		_probe = _probeBody.GetRequiredComponent<SurveyorProbe>();
		_probeAlignment = _probeBody.GetRequiredComponent<AlignWithDirection>();
		_probe.OnRetrieveProbe += OnRetrieveProbe;
		_probeNotification = new NotificationData(NotificationTarget.All, string.Empty);
		_strBuilder = new StringBuilder();
		_anchored = false;
	}

	private void OnDestroy()
	{
		_probe.OnRetrieveProbe -= OnRetrieveProbe;
	}

	public Collider GetCollider()
	{
		return _collider;
	}

	public SandLevelController GetSandLevelController()
	{
		return _sandLevelController;
	}

	public FragmentIntegrity GetFragmentIntegrity()
	{
		return _breakableFragment;
	}

	public DamDestructionController GetDamIntegrity()
	{
		return _ringworldDam;
	}

	private string BuildIntegrityString()
	{
		_strBuilder.Length = 0;
		if (_breakableFragment != null)
		{
			_strBuilder.Append(UITextLibrary.GetString(UITextType.NotificationIntegrity));
			_strBuilder.Append(_breakableFragment.GetIntegrityPercent());
			_strBuilder.Append("%");
		}
		else if (_ringworldDam != null)
		{
			_strBuilder.Append(UITextLibrary.GetString(UITextType.NotificationIntegrity));
			_strBuilder.Append(_ringworldDam.GetIntegrityPercent());
			_strBuilder.Append("%");
		}
		return _strBuilder.ToString();
	}

	public bool IsAnchored()
	{
		return _anchored;
	}

	public float GetSecondsAnchored()
	{
		if (!_anchored)
		{
			return 0f;
		}
		return Time.time - _anchorTime;
	}

	public void AnchorToSocket(Transform socket)
	{
		if (!_anchored)
		{
			AnchorToObject(socket.gameObject, socket.up, socket.position);
		}
	}

	public void UnanchorFromSurface()
	{
		_probeBody.transform.parent = null;
		_collider.enabled = true;
		_probeBody.MakeNonKinematic();
		_probeBody.GetAttachedForceDetector().enabled = true;
		_probeBody.GetAttachedFluidDetector().enabled = true;
		_probeAlignment.enabled = true;
		_anchored = false;
		_unanchoredTime = Time.time;
		if (_breakableFragment != null || _ringworldDam != null)
		{
			NotificationManager.SharedInstance.UnpinNotification(_probeNotification);
			_notificationPosted = false;
			_breakableFragment = null;
			_ringworldDam = null;
		}
		if (this.OnUnanchorFromSurface != null)
		{
			this.OnUnanchorFromSurface();
		}
	}

	public void OnLaunch()
	{
		base.enabled = true;
		_launchTime = Time.time;
		_unanchoredTime = 0f;
		RaycastCollisionCheck();
	}

	private void FixedUpdate()
	{
		if (!_anchored)
		{
			if (!_collider.enabled && Time.time > _launchTime + 0.2f)
			{
				ActivateCollider();
			}
			RaycastCollisionCheck();
		}
	}

	private void Update()
	{
		if (!_anchored)
		{
			return;
		}
		base.transform.localPosition = _localImpactPos;
		if ((_breakableFragment != null || _ringworldDam != null) && _notificationPosted)
		{
			string text = BuildIntegrityString();
			if (text != _probeNotification.displayMessage)
			{
				_probeNotification.displayMessage = text;
				NotificationManager.SharedInstance.RepostNotifcation(_probeNotification);
			}
		}
	}

	private void ActivateCollider()
	{
		_collider.enabled = true;
		GlobalMessenger<Collider>.FireEvent("IgnoreProbeCollider", _collider);
	}

	public void AnchorToObject(GameObject hitObject, Vector3 hitNormal, Vector3 hitPoint)
	{
		OWRigidbody attachedOWRigidbody = hitObject.GetAttachedOWRigidbody();
		if (attachedOWRigidbody.GetMass() < 0.001f)
		{
			return;
		}
		if (attachedOWRigidbody.GetMass() < 100f)
		{
			Vector3 vector = _probeBody.GetVelocity() - attachedOWRigidbody.GetPointVelocity(_probeBody.GetPosition());
			attachedOWRigidbody.GetRigidbody().AddForceAtPosition(vector.normalized * 0.005f, hitPoint, ForceMode.Impulse);
		}
		_collider.enabled = false;
		_breakableFragment = hitObject.GetComponentInParent<FragmentIntegrity>();
		_sandLevelController = null;
		AstroObject component = attachedOWRigidbody.GetComponent<AstroObject>();
		if (component != null)
		{
			_sandLevelController = component.GetSandLevelController();
		}
		_ringworldDam = hitObject.GetComponentInParent<DamDestructionController>();
		if (_breakableFragment != null || _ringworldDam != null)
		{
			_probeNotification.displayMessage = BuildIntegrityString();
			if (_notificationPosted)
			{
				NotificationManager.SharedInstance.RepostNotifcation(_probeNotification);
			}
			else
			{
				NotificationManager.SharedInstance.PostNotification(_probeNotification, pin: true);
				_notificationPosted = true;
			}
		}
		_probeBody.MakeKinematic();
		_probeBody.transform.parent = hitObject.transform;
		_probeBody.transform.rotation = Quaternion.FromToRotation(_probeBody.transform.forward, -hitNormal) * _probeBody.transform.rotation;
		_probeBody.transform.position = hitPoint;
		_probeBody.GetAttachedForceDetector().enabled = false;
		_probeBody.GetAttachedFluidDetector().enabled = false;
		_probeAlignment.enabled = false;
		_localImpactPos = base.transform.localPosition;
		_anchorTime = Time.time;
		_anchored = true;
		if (this.OnAnchorToSurface != null)
		{
			this.OnAnchorToSurface();
		}
	}

	private void OnRetrieveProbe()
	{
		base.enabled = false;
		_collider.enabled = false;
		if (_anchored)
		{
			if (_breakableFragment != null || _ringworldDam != null)
			{
				NotificationManager.SharedInstance.UnpinNotification(_probeNotification);
				_notificationPosted = false;
				_breakableFragment = null;
				_ringworldDam = null;
			}
			_probeBody.MakeNonKinematic();
			_probeBody.transform.parent = null;
			_probeBody.GetAttachedForceDetector().enabled = true;
			_probeBody.GetAttachedFluidDetector().enabled = true;
			_probeAlignment.enabled = true;
			_anchored = false;
		}
	}

	private void RaycastCollisionCheck()
	{
		OWRigidbody oWRigidbody = null;
		if (_probe.GetRulesetDetector().GetPlanetoidRuleset() != null)
		{
			oWRigidbody = _probe.GetRulesetDetector().GetPlanetoidRuleset().GetAttachedOWRigidbody();
		}
		else if (_probe.GetSectorDetector() != null && _probe.GetSectorDetector().GetPassiveReferenceFrame() != null)
		{
			oWRigidbody = _probe.GetSectorDetector().GetPassiveReferenceFrame().GetOWRigidBody();
		}
		else if (Vector3.Distance(Locator.GetPlayerTransform().position, base.transform.position) < 100f)
		{
			oWRigidbody = Locator.GetPlayerBody();
		}
		else if (Locator.GetShipTransform() != null && Vector3.Distance(Locator.GetShipTransform().position, base.transform.position) < 100f)
		{
			oWRigidbody = Locator.GetShipBody();
		}
		else if (_probe.GetSectorDetector() != null && _probe.GetSectorDetector().GetLastEnteredSector() != null)
		{
			oWRigidbody = _probe.GetSectorDetector().GetLastEnteredSector().GetOWRigidbody();
		}
		else if (Locator.GetSunTransform() != null)
		{
			oWRigidbody = Locator.GetSunTransform().GetComponent<OWRigidbody>();
		}
		Vector3 vector = ((oWRigidbody != null) ? (_probeBody.GetVelocity() - oWRigidbody.GetPointVelocity(base.transform.position)) : _probeBody.GetVelocity());
		if (!Physics.Raycast(base.transform.position, vector, out var hitInfo, 100f, OWLayerMask.physicalMask) || !(hitInfo.collider.attachedRigidbody != null))
		{
			return;
		}
		Vector3 vector2 = Vector3.Project(hitInfo.collider.attachedRigidbody.GetComponent<OWRigidbody>().GetPointVelocity(hitInfo.point) - _probeBody.GetVelocity(), vector);
		if (!(vector2.magnitude * Time.deltaTime > hitInfo.distance))
		{
			return;
		}
		bool flag = _probe.GetRulesetDetector().GetProbeRuleSet() != null && _probe.GetRulesetDetector().GetProbeRuleSet().GetIgnoreAnchor();
		IgnoreCollision component = hitInfo.collider.GetComponent<IgnoreCollision>();
		if (component == null || !component.IgnoresProbe())
		{
			if (!flag && (component == null || !component.IgnoresProbeAnchor()) && hitInfo.collider.material.dynamicFriction > 0f)
			{
				AnchorToObject(hitInfo.collider.gameObject, hitInfo.normal, hitInfo.point);
				return;
			}
			base.transform.position = hitInfo.point - vector2.normalized * 0.15f;
			ActivateCollider();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (_anchored || !(collision.collider.attachedRigidbody != null) || !(_probeBody != null) || !(Time.time > _unanchoredTime + 0.5f))
		{
			return;
		}
		Vector3 direction = collision.contacts[0].point - base.transform.position;
		if (Physics.Raycast(base.transform.position, direction, out var hitInfo, 5f, OWLayerMask.physicalMask))
		{
			bool num = _probe.GetRulesetDetector().GetProbeRuleSet() != null && _probe.GetRulesetDetector().GetProbeRuleSet().GetIgnoreAnchor();
			IgnoreCollision component = hitInfo.collider.GetComponent<IgnoreCollision>();
			if (!num && (component == null || !component.IgnoresProbeAnchor()) && hitInfo.collider.material.dynamicFriction > 0f)
			{
				OWRigidbody component2 = collision.collider.attachedRigidbody.GetComponent<OWRigidbody>();
				Vector3 to = _probeBody.GetVelocity() - component2.GetPointVelocity(hitInfo.point);
				float num2 = Vector3.Angle(-hitInfo.normal, to);
				if (num2 > 90f)
				{
					Debug.Log("Probe contact angle too high, ignoring collision: " + num2);
				}
				else
				{
					AnchorToObject(hitInfo.collider.gameObject, hitInfo.normal, hitInfo.point);
				}
			}
		}
		else
		{
			Debug.LogError("Probe raycast failed during collision event against " + collision.collider.name, collision.collider.gameObject);
		}
	}
}
