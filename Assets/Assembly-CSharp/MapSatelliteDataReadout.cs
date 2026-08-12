using UnityEngine;
using UnityEngine.UI;

public class MapSatelliteDataReadout : SectoredMonoBehaviour
{
	[SerializeField]
	private Canvas _readoutCanvas;

	[SerializeField]
	private Text _planeAngleText;

	[SerializeField]
	private MapSatelliteStateController _satelliteStateController;

	private OWRigidbody _satelliteBody;

	private OWRigidbody _primaryBody;

	private Vector3 _startVec;

	private Vector3 _crossAxis;

	private float _angle;

	private MapSatelliteStateController.MapSatelliteState _lastRecordedState;

	protected override void Awake()
	{
		base.Awake();
		_satelliteBody = this.GetAttachedOWRigidbody();
		_primaryBody = _satelliteBody.GetComponent<InitialMotion>().GetPrimaryBody();
		Vector3 vector = _satelliteBody.transform.position - _primaryBody.transform.position;
		_startVec = new Vector3(vector.x, 0f, vector.z).normalized;
		_crossAxis = Vector3.Cross(_startVec, Vector3.up).normalized;
		_lastRecordedState = _satelliteStateController.GetState();
		_satelliteStateController.OnSatelliteStateChange += OnSatelliteStateChange;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_satelliteStateController != null)
		{
			_satelliteStateController.OnSatelliteStateChange -= OnSatelliteStateChange;
		}
	}

	private void OnEnable()
	{
		_readoutCanvas.enabled = true;
	}

	private void OnDisable()
	{
		_readoutCanvas.enabled = false;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsOccupant(DynamicOccupant.Player) || _sector.ContainsOccupant(DynamicOccupant.Probe);
	}

	private void LateUpdate()
	{
		if (_lastRecordedState <= MapSatelliteStateController.MapSatelliteState.NORMAL)
		{
			Vector3 to = _satelliteBody.transform.position - _primaryBody.transform.position;
			_angle = Vector3.SignedAngle(_startVec, to, _crossAxis);
			if (_angle < 0f)
			{
				_angle = 360f + _angle;
			}
			int num = (int)(_angle * 10f) % 10;
			_planeAngleText.text = Mathf.FloorToInt(_angle).ToString() + "." + num + "°";
		}
	}

	private void OnSatelliteStateChange(MapSatelliteStateController.MapSatelliteState newState)
	{
		_lastRecordedState = newState;
		if (_lastRecordedState > MapSatelliteStateController.MapSatelliteState.NORMAL)
		{
			_planeAngleText.text = UITextLibrary.GetString(UITextType.MapOfflineMessage);
		}
	}
}
