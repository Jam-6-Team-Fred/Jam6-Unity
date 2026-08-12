using UnityEngine;

public class BillboardToCamera : SectoredMonoBehaviour
{
	private enum RotationType
	{
		FREE_ROTATION = 0,
		ROTATE_ABOUT_X_AXIS = 1,
		ROTATE_ABOUT_Y_AXIS = 2
	}

	[SerializeField]
	private RotationType _rotationType;

	private bool _callbackAdded;

	private Vector3 _directionToCamera;

	private Vector3 _projectedDirection;

	protected override void Awake()
	{
		if (_sector == null)
		{
			OWCamera.onAnyPreRender += new OWEvent<OWCamera>.OWCallback(OnAnyCameraPreRender);
			_callbackAdded = true;
		}
		else
		{
			_sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantAdded);
			_sector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantRemoved);
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		if (!_callbackAdded)
		{
			OWCamera.onAnyPreRender += new OWEvent<OWCamera>.OWCallback(OnAnyCameraPreRender);
			_callbackAdded = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_callbackAdded)
		{
			OWCamera.onAnyPreRender -= new OWEvent<OWCamera>.OWCallback(OnAnyCameraPreRender);
			_callbackAdded = false;
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (_sector.ContainsOccupant(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			if (!_callbackAdded)
			{
				OWCamera.onAnyPreRender += new OWEvent<OWCamera>.OWCallback(OnAnyCameraPreRender);
				_callbackAdded = true;
			}
		}
		else if (_callbackAdded)
		{
			OWCamera.onAnyPreRender -= new OWEvent<OWCamera>.OWCallback(OnAnyCameraPreRender);
			_callbackAdded = false;
		}
	}

	private void OnAnyCameraPreRender(OWCamera camera)
	{
		Vector3 zero = Vector3.zero;
		switch (_rotationType)
		{
		case RotationType.ROTATE_ABOUT_X_AXIS:
			zero = Vector3.right;
			break;
		case RotationType.ROTATE_ABOUT_Y_AXIS:
			zero = Vector3.up;
			break;
		default:
			base.transform.LookAt(camera.transform.position);
			return;
		}
		_directionToCamera = camera.transform.position - base.transform.position;
		_projectedDirection = _directionToCamera - Vector3.Project(_directionToCamera, base.transform.TransformDirection(zero));
		base.transform.rotation = Quaternion.FromToRotation(base.transform.TransformDirection(Vector3.forward), _projectedDirection) * base.transform.rotation;
	}
}
