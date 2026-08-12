using UnityEngine;

public abstract class PlayerTool : MonoBehaviour
{
	[SerializeField]
	protected DampedSpringQuat _moveSpring = new DampedSpringQuat();

	[SerializeField]
	protected Transform _stowTransform;

	[SerializeField]
	protected Transform _holdTransform;

	[SerializeField]
	protected float _arrivalDegrees = 5f;

	protected bool _isEquipped;

	protected bool _isCentered;

	protected bool _isPuttingAway;

	protected virtual void Start()
	{
		base.enabled = false;
	}

	public virtual void EquipTool()
	{
		if (!_isEquipped)
		{
			_isEquipped = true;
			_isPuttingAway = false;
			_isCentered = !HasEquipAnimation();
			if (HasEquipAnimation())
			{
				base.transform.localRotation = _stowTransform.localRotation;
			}
			base.enabled = true;
		}
	}

	public virtual void UnequipTool()
	{
		if (_isEquipped)
		{
			_isEquipped = false;
			if (!HasEquipAnimation())
			{
				_isCentered = false;
				base.enabled = false;
			}
			else if (!_isPuttingAway)
			{
				_isPuttingAway = true;
				_isCentered = false;
			}
		}
	}

	public virtual bool IsEquipped()
	{
		return _isEquipped;
	}

	public virtual bool IsCentered()
	{
		return _isCentered;
	}

	public virtual bool IsPuttingAway()
	{
		return _isPuttingAway;
	}

	public virtual bool HasEquipAnimation()
	{
		if (_stowTransform != null)
		{
			return _holdTransform != null;
		}
		return false;
	}

	public void InstantlyCenterTool()
	{
		_isCentered = true;
		base.transform.localRotation = _holdTransform.localRotation;
		_moveSpring.ResetVelocity();
	}

	protected virtual bool AllowEquipAnimation()
	{
		return true;
	}

	protected virtual void Update()
	{
		if (HasEquipAnimation() && AllowEquipAnimation())
		{
			float deltaTime = (_isPuttingAway ? Time.unscaledDeltaTime : Time.deltaTime);
			Quaternion quaternion = (_isPuttingAway ? _stowTransform.localRotation : _holdTransform.localRotation);
			base.transform.localRotation = _moveSpring.Update(base.transform.localRotation, quaternion, deltaTime);
			float num = Quaternion.Angle(base.transform.localRotation, quaternion);
			if (_isEquipped && !_isCentered && num <= _arrivalDegrees)
			{
				_isCentered = true;
			}
			if (_isPuttingAway && num <= _arrivalDegrees)
			{
				_isEquipped = false;
				_isPuttingAway = false;
				base.enabled = false;
				_moveSpring.ResetVelocity();
			}
		}
	}
}
