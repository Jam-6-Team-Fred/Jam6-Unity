using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ChildTriggerVolume : MonoBehaviour
{
	public delegate void EnterChildTriggerEvent(Collider hitCollider);

	public delegate void ExitChildTriggerEvent(Collider hitCollider);

	[SerializeField]
	private bool _entryTrigger = true;

	[SerializeField]
	private bool _exitTrigger = true;

	private Collider _collider;

	public event EnterChildTriggerEvent OnEnterChildTrigger;

	public event ExitChildTriggerEvent OnExitChildTrigger;

	private void OnValidate()
	{
		Collider component = GetComponent<Collider>();
		if (!component.isTrigger)
		{
			component.isTrigger = true;
		}
		if (base.gameObject.layer != LayerMask.NameToLayer("BasicEffectVolume"))
		{
			base.gameObject.layer = LayerMask.NameToLayer("BasicEffectVolume");
		}
	}

	private void Awake()
	{
		_collider = GetComponent<Collider>();
	}

	private void OnEnable()
	{
		_collider.enabled = true;
	}

	private void OnDisable()
	{
		_collider.enabled = false;
	}

	public bool IsEntryTrigger()
	{
		return _entryTrigger;
	}

	public bool IsExitTrigger()
	{
		return _exitTrigger;
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		if (this.OnEnterChildTrigger != null)
		{
			this.OnEnterChildTrigger(hitCollider);
		}
	}

	private void OnTriggerExit(Collider hitCollider)
	{
		if (this.OnExitChildTrigger != null)
		{
			this.OnExitChildTrigger(hitCollider);
		}
	}
}
