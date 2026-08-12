using UnityEngine;

public class NomaiDrillController : MonoBehaviour
{
	[SerializeField]
	private TractorBeamController _tractorBeam;

	[SerializeField]
	private NomaiInterfaceSlot _rotateRightSlot;

	[SerializeField]
	private NomaiInterfaceSlot _rotateLeftSlot;

	private Quaternion _targetLocalRotation;

	private void Awake()
	{
		if (_rotateRightSlot != null)
		{
			_rotateRightSlot.OnSlotActivated += OnRotateRight;
		}
		if (_rotateLeftSlot != null)
		{
			_rotateLeftSlot.OnSlotActivated += OnRotateLeft;
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_rotateRightSlot != null)
		{
			_rotateRightSlot.OnSlotActivated -= OnRotateRight;
		}
		if (_rotateLeftSlot != null)
		{
			_rotateLeftSlot.OnSlotActivated -= OnRotateLeft;
		}
	}

	private void OnEnable()
	{
		RotateByDegrees(60f);
	}

	private bool IsRotating()
	{
		return base.enabled;
	}

	private void OnToggleTractorBeam(NomaiInterfaceSlot slot)
	{
		_tractorBeam.SetActivation(_tractorBeam.IsActive());
	}

	private void OnRotateRight(NomaiInterfaceSlot slot)
	{
		if (!IsRotating())
		{
			RotateByDegrees(60f);
		}
	}

	private void OnRotateLeft(NomaiInterfaceSlot slot)
	{
		if (!IsRotating())
		{
			RotateByDegrees(-60f);
		}
	}

	private void RotateByDegrees(float degrees)
	{
		base.enabled = true;
		Vector3 axis = base.transform.parent.InverseTransformDirection(base.transform.up);
		_targetLocalRotation = Quaternion.AngleAxis(degrees, axis) * base.transform.localRotation;
	}

	private void FixedUpdate()
	{
		base.transform.localRotation = Quaternion.RotateTowards(base.transform.localRotation, _targetLocalRotation, 10f * Time.deltaTime);
		if (base.transform.localRotation == _targetLocalRotation)
		{
			base.enabled = false;
		}
	}
}
