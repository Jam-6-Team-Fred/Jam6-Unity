using UnityEngine;

public class SandLevelController : MonoBehaviour
{
	public delegate void SandColliderInitializeEvent();

	[SerializeField]
	private AnimationCurve _scaleCurve;

	private int _reactivateSandOnFirstFrameFlag;

	private GameObject _sandColliderObj;

	private Collider _sandCollider;

	private bool _playerInsideSandSphere;

	private bool _probeInsideSandSphere;

	private bool _isTowerTwin;

	public event SandColliderInitializeEvent OnSandColliderInitialized;

	private void Awake()
	{
		_sandCollider = GetComponentInChildren<SphereCollider>();
		_sandColliderObj = _sandCollider.gameObject;
		_sandColliderObj.SetActive(value: false);
		AstroObject componentInParent = GetComponentInParent<AstroObject>();
		if (componentInParent != null && componentInParent.GetAstroObjectName() == AstroObject.Name.TowerTwin)
		{
			_isTowerTwin = true;
			GlobalMessenger<OWRigidbody>.AddListener("EnterTimeLoopCentral", OnEnterTimeLoopCentral);
			GlobalMessenger<OWRigidbody>.AddListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
		}
	}

	private void OnDestroy()
	{
		if (_isTowerTwin)
		{
			GlobalMessenger<OWRigidbody>.RemoveListener("EnterTimeLoopCentral", OnEnterTimeLoopCentral);
			GlobalMessenger<OWRigidbody>.RemoveListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
		}
	}

	public bool IsPointBuried(Vector3 point)
	{
		return (point - base.transform.position).magnitude < GetRadius();
	}

	public Collider GetSandCollider()
	{
		return _sandCollider;
	}

	public float GetRadius()
	{
		return base.transform.localScale.x * 0.5f;
	}

	private void FixedUpdate()
	{
		if (_reactivateSandOnFirstFrameFlag == 1)
		{
			_sandColliderObj.SetActive(value: true);
		}
		_reactivateSandOnFirstFrameFlag++;
		float num = _scaleCurve.Evaluate(TimeLoop.GetMinutesElapsed());
		base.transform.localScale = new Vector3(num, num, num);
		if (_reactivateSandOnFirstFrameFlag == 1 && this.OnSandColliderInitialized != null)
		{
			this.OnSandColliderInitialized();
		}
		if (_playerInsideSandSphere && Vector3.Distance(Locator.GetPlayerTransform().position, base.transform.position) > GetRadius() + 1.1f)
		{
			MonoBehaviour.print("FIRE PLAYER EXIT TIME LOOP CENTRAL EVENT " + base.gameObject.name);
			GlobalMessenger<OWRigidbody>.FireEvent("ExitTimeLoopCentral", Locator.GetPlayerBody());
		}
		if (_probeInsideSandSphere && Vector3.Distance(Locator.GetProbe().transform.position, base.transform.position) > GetRadius() + 1.1f)
		{
			GlobalMessenger<OWRigidbody>.FireEvent("ExitTimeLoopCentral", Locator.GetProbe().GetOWRigidbody());
		}
	}

	public bool IsInitialized()
	{
		return _reactivateSandOnFirstFrameFlag >= 1;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(base.transform.position, _scaleCurve.Evaluate(22f) * 0.5f);
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _scaleCurve.Evaluate(22f) * 0.5f);
			Gizmos.DrawWireSphere(base.transform.position, _scaleCurve.Evaluate(0f) * 0.5f);
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _scaleCurve.Evaluate(0f) * 0.5f);
		}
	}

	private void OnEnterTimeLoopCentral(OWRigidbody body)
	{
		if (body.CompareTag("Player"))
		{
			MonoBehaviour.print("PLAYER ENTER SAND SPHERE " + base.gameObject.name + " AT " + Time.time);
			_playerInsideSandSphere = true;
			_sandColliderObj.layer = LayerMask.NameToLayer("ProxyPrimitive");
		}
		else if (body.CompareTag("Probe"))
		{
			_probeInsideSandSphere = true;
			Collider[] componentsInChildren = body.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Physics.IgnoreCollision(_sandCollider, componentsInChildren[i]);
			}
		}
	}

	private void OnExitTimeLoopCentral(OWRigidbody body)
	{
		if (body.CompareTag("Player"))
		{
			MonoBehaviour.print("PLAYER EXIT SAND SPHERE " + base.gameObject.name + " AT " + Time.time);
			_playerInsideSandSphere = false;
			_sandColliderObj.layer = LayerMask.NameToLayer("Primitive");
		}
		else if (body.CompareTag("Probe"))
		{
			_probeInsideSandSphere = false;
			Collider[] componentsInChildren = body.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Physics.IgnoreCollision(_sandCollider, componentsInChildren[i], ignore: false);
			}
		}
	}
}
