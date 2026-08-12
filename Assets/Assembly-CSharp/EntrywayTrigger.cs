using UnityEngine;

public class EntrywayTrigger : MonoBehaviour
{
	public delegate void EntrywayEvent(GameObject hitObj);

	private OWCollider _owCollider;

	private Shape _shape;

	private int _registerCount;

	public event EntrywayEvent OnEntry;

	public event EntrywayEvent OnExit;

	private void Awake()
	{
		if (GetComponent<Collider>() != null)
		{
			_owCollider = base.gameObject.GetAddComponent<OWCollider>();
			_owCollider.Assert(OWLayerMask.effectVolumeMask, isTrigger: true);
			_owCollider.SetLODActivationMask(DynamicOccupant.Player | DynamicOccupant.Probe);
			_owCollider.IgnorePhysicsSwapDelay();
			return;
		}
		_shape = GetComponent<Shape>();
		if (_shape != null)
		{
			_shape.OnCollisionExit += OnShapeExit;
			return;
		}
		Debug.LogError("Entryway trigger has no attached collider or shape", this);
		Debug.Break();
	}

	private void Start()
	{
		if (_registerCount == 0)
		{
			Debug.LogError("Entryway trigger is not being used", this);
			Debug.Break();
		}
	}

	public void Register()
	{
		_registerCount++;
	}

	public void ForceEntry(GameObject obj)
	{
		if (this.OnEntry != null)
		{
			this.OnEntry(obj);
		}
	}

	public void SetActivation(bool active)
	{
		if (_owCollider != null)
		{
			_owCollider.SetActivation(active);
		}
		if (_shape != null)
		{
			_shape.SetActivation(active);
		}
	}

	private void OnShapeExit(Shape hitShape)
	{
		if (hitShape.CompareTag("PlayerDetector") || hitShape.CompareTag("ProbeDetector") || hitShape.CompareTag("PlayerCameraDetector") || hitShape.CompareTag("DynamicPropDetector") || hitShape.CompareTag("DreamLanternDetector"))
		{
			Vector3 vector = base.transform.InverseTransformPoint(hitShape.transform.position);
			if (vector.z > 0f && this.OnExit != null)
			{
				this.OnExit(hitShape.gameObject);
			}
			else if (vector.z <= 0f && this.OnEntry != null)
			{
				this.OnEntry(hitShape.gameObject);
			}
		}
	}

	private void OnTriggerExit(Collider hitCollider)
	{
		if (hitCollider.CompareTag("PlayerDetector") || hitCollider.CompareTag("ProbeDetector") || hitCollider.CompareTag("PlayerCameraDetector") || hitCollider.CompareTag("DynamicPropDetector"))
		{
			Vector3 vector = base.transform.InverseTransformPoint(hitCollider.transform.position);
			if (vector.z > 0f && this.OnExit != null)
			{
				this.OnExit(hitCollider.gameObject);
			}
			else if (vector.z <= 0f && this.OnEntry != null)
			{
				this.OnEntry(hitCollider.gameObject);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawRay(base.transform.position, base.transform.forward * 2f);
	}
}
