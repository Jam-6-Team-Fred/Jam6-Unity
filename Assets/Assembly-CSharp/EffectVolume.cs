using UnityEngine;

public abstract class EffectVolume : MonoBehaviour
{
	public delegate void OWEffectVolumeEvent(bool active);

	protected OWRigidbody _attachedBody;

	protected OWTriggerVolume _triggerVolume;

	public event OWEffectVolumeEvent OnActivate;

	protected virtual void Reset()
	{
		Collider component = GetComponent<Collider>();
		if (component != null)
		{
			component.isTrigger = true;
			if (!OWLayerMask.IsLayerInMask(base.gameObject.layer, OWLayerMask.effectVolumeMask))
			{
				base.gameObject.layer = LayerMask.NameToLayer("BasicEffectVolume");
			}
		}
		Shape component2 = GetComponent<Shape>();
		if (component2 != null)
		{
			component2.SetCollisionMode(Shape.CollisionMode.Volume);
		}
	}

	protected virtual void Awake()
	{
		ResetAttachedBody();
		if (_triggerVolume == null)
		{
			_triggerVolume = base.gameObject.GetAddComponent<OWTriggerVolume>();
		}
		_triggerVolume.OnEntry += OnEffectVolumeEnter;
		_triggerVolume.OnExit += OnEffectVolumeExit;
	}

	protected virtual void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEffectVolumeEnter;
		_triggerVolume.OnExit -= OnEffectVolumeExit;
	}

	public void ResetAttachedBody()
	{
		_attachedBody = base.gameObject.GetAttachedOWRigidbody();
	}

	public void SetAttachedBody(OWRigidbody body)
	{
		_attachedBody = body;
	}

	public OWRigidbody GetAttachedOWRigidbody()
	{
		return _attachedBody;
	}

	public void SetVolumeActivation(bool active)
	{
		if (this.OnActivate != null)
		{
			this.OnActivate(active);
		}
		_triggerVolume.SetTriggerActivation(active);
		base.enabled = active;
	}

	public bool IsVolumeActive()
	{
		return base.enabled;
	}

	public OWTriggerVolume GetOWTriggerVolume()
	{
		return _triggerVolume;
	}

	protected abstract void OnEffectVolumeEnter(GameObject hitObj);

	protected abstract void OnEffectVolumeExit(GameObject hitObj);
}
