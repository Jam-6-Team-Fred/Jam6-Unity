using UnityEngine;

public class NomaiInterfaceSlot : MonoBehaviour
{
	public delegate void SlotEvent(NomaiInterfaceSlot slot);

	[SerializeField]
	private float _radius;

	[SerializeField]
	private float _exitRadius;

	[SerializeField]
	private bool _attractive = true;

	[SerializeField]
	private bool _ignoreDraggedOrbs;

	[SerializeField]
	private bool _muteAudio;

	private NomaiInterfaceOrb _occupyingOrb;

	private bool _cancelsDragOnCollision = true;

	public event SlotEvent OnSlotActivated;

	public event SlotEvent OnSlotDeactivated;

	public bool IsActivated()
	{
		return _occupyingOrb != null;
	}

	public bool IsAttractive()
	{
		return _attractive;
	}

	public void SetAttractive(bool attractive)
	{
		_attractive = attractive;
	}

	public void SetCancelsDragOnCollision(bool cancelsDrag)
	{
		_cancelsDragOnCollision = cancelsDrag;
	}

	public bool CancelsDragOnCollision()
	{
		return _cancelsDragOnCollision;
	}

	private void OnValidate()
	{
		if (_exitRadius != Mathf.Max(_exitRadius, _radius))
		{
			_exitRadius = Mathf.Max(_exitRadius, _radius);
		}
	}

	public bool CheckOrbCollision(NomaiInterfaceOrb orb)
	{
		if (_ignoreDraggedOrbs && orb.IsBeingDragged())
		{
			return false;
		}
		float num = Vector3.Distance(orb.transform.position, base.transform.position);
		float num2 = (orb.IsBeingDragged() ? _exitRadius : _radius);
		if (_occupyingOrb == null && num < _radius)
		{
			_occupyingOrb = orb;
			if (Time.timeSinceLevelLoad > 1f && this.OnSlotActivated != null)
			{
				this.OnSlotActivated(this);
			}
			return true;
		}
		if (_occupyingOrb != null && _occupyingOrb == orb)
		{
			if (num > num2)
			{
				_occupyingOrb = null;
				if (this.OnSlotDeactivated != null)
				{
					this.OnSlotDeactivated(this);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool GetPlayActivationAudio()
	{
		if (this.OnSlotActivated != null)
		{
			return !_muteAudio;
		}
		return false;
	}

	public bool HasActivationListeners()
	{
		return this.OnSlotActivated != null;
	}

	public NomaiInterfaceOrb GetOccupyingInterfaceOrb()
	{
		return _occupyingOrb;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new ColorHSV(0f, 0.5f, 1f).ToColorRGB();
		Gizmos.DrawWireSphere(base.transform.position, _radius);
		Gizmos.color = new ColorHSV(0f, 0.5f, 0.5f).ToColorRGB();
		Gizmos.DrawWireSphere(base.transform.position, _exitRadius);
	}
}
