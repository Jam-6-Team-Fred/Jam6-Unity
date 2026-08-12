using System;
using System.Text;
using UnityEngine;

public class ReferenceFrameGUI : MonoBehaviour
{
	private enum ReferenceFrameGUIType
	{
		PLAYER = 0,
		SHIP = 1,
		LANDING_CAM = 2,
		MAP = 3
	}

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private Canvas _offScreenIndicatorCanvas;

	[SerializeField]
	private Canvas _referenceCanvas;

	private ReferenceFrame _currentReferenceFrame;

	private ReferenceFrame _possibleReferenceFrame;

	[SerializeField]
	private OWRigidbody _activeBody;

	[SerializeField]
	private OWCamera _activeCam;

	private bool _isMapView;

	[SerializeField]
	private float _lockOnGuiPlaneDistance = 1f;

	[SerializeField]
	private float _offScreenIndicatorPlaneDistance = 0.5f;

	[SerializeField]
	private LockOnReticule _reticule1;

	[SerializeField]
	private LockOnReticule _reticule2;

	private Vector3 _relativeVelocity;

	private Vector3 _orientedRelativeVelocity;

	private bool _isDirectTrajectory;

	private Color _reticuleColor;

	[SerializeField]
	private Color _departingColor = new Color(1f, 0f, 0f, 1f);

	[SerializeField]
	private Color _approachingColor = new Color(0f, 1f, 0.333f, 1f);

	[SerializeField]
	private Color _staticColor = Color.white;

	[SerializeField]
	private float _possLockReticuleAlpha = 0.25f;

	[SerializeField]
	private OffScreenIndicator _offScreenIndicator;

	private StringBuilder _stringBuilder;

	[SerializeField]
	private ReferenceFrameGUIType _type;

	private bool _showVisuals;

	private GameObject[] _guiObjects;

	private ReferenceFrameTracker _referenceFrameTracker;

	private ShipCockpitController _shipController;

	private bool _hasPlayerLockedOn;

	private bool _hasPlayerLockedOnMap;

	private void Awake()
	{
		if (_referenceCanvas == null)
		{
			_referenceCanvas = _canvas;
		}
		_reticuleColor = Color.white;
		GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", OnSwitchActiveCamera);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.AddListener("ChangeGUIMode", OnChangeGUIMode);
		_guiObjects = new GameObject[3] { _reticule1.gameObject, _reticule2.gameObject, _offScreenIndicator.gameObject };
		_stringBuilder = new StringBuilder();
		SetVisibility(visible: false);
	}

	private void Start()
	{
		_referenceFrameTracker = Locator.GetPlayerTransform().GetComponent<ReferenceFrameTracker>();
		if (_activeCam == null)
		{
			_activeCam = Locator.GetPlayerCamera();
		}
		if (_activeBody == null)
		{
			_activeBody = Locator.GetPlayerTransform().GetRequiredComponent<OWRigidbody>();
		}
		if (_reticule2 != null)
		{
			_reticule2.SetArrowState(LockOnReticule.ArrowState.OFF);
			_reticule2.SetPossibleLockAlpha(_possLockReticuleAlpha);
			_reticule2.SetResetColor(_staticColor);
			_reticule2.Init();
			_reticule2.gameObject.SetActive(value: false);
		}
		if (_reticule1 != null)
		{
			_reticule1.SetArrowState(LockOnReticule.ArrowState.OFF);
			_reticule1.SetPossibleLockAlpha(_possLockReticuleAlpha);
			_reticule1.SetResetColor(_staticColor);
			_reticule1.Init();
			_reticule1.gameObject.SetActive(value: false);
		}
		if (_offScreenIndicator != null)
		{
			Canvas canvas = ((!(_offScreenIndicatorCanvas != null)) ? _canvas : _offScreenIndicatorCanvas);
			_offScreenIndicator.SetCanvas(canvas);
			_offScreenIndicator.gameObject.SetActive(value: false);
		}
		if (_type == ReferenceFrameGUIType.SHIP || _type == ReferenceFrameGUIType.LANDING_CAM)
		{
			_shipController = Locator.GetShipTransform().GetComponentInChildren<ShipCockpitController>();
		}
		_hasPlayerLockedOn = PlayerData.GetPersistentCondition("HAS_PLAYER_LOCKED_ON");
		_hasPlayerLockedOnMap = PlayerData.GetPersistentCondition("HAS_PLAYER_LOCKED_ON_MAP");
	}

	private void OnEnable()
	{
		UpdateVisibility();
	}

	private void OnDisable()
	{
		UpdateVisibility();
	}

	private void OnDestroy()
	{
		GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.RemoveListener("ChangeGUIMode", OnChangeGUIMode);
	}

	private void UpdateReferenceFrameState(ReferenceFrame possibleFrame, ReferenceFrame currentFrame)
	{
		if (currentFrame != null && currentFrame != _currentReferenceFrame)
		{
			LockOnReticule reticuleWithState = GetReticuleWithState(LockOnReticule.LockState.LOCK);
			if (reticuleWithState != null)
			{
				reticuleWithState.SetLockState(LockOnReticule.LockState.OFF);
			}
			reticuleWithState = GetReticuleWithState(LockOnReticule.LockState.POSSIBLE_LOCK);
			if (reticuleWithState != null)
			{
				reticuleWithState.SetLockState(LockOnReticule.LockState.LOCK);
			}
			else
			{
				reticuleWithState = GetReticuleWithState(LockOnReticule.LockState.OFF);
				if (reticuleWithState != null)
				{
					reticuleWithState.SetLockState(LockOnReticule.LockState.LOCK);
				}
			}
		}
		if (currentFrame == null && _currentReferenceFrame != null)
		{
			LockOnReticule reticuleWithState = GetReticuleWithState(LockOnReticule.LockState.LOCK);
			if (reticuleWithState != null)
			{
				reticuleWithState.SetLockState(LockOnReticule.LockState.OFF);
			}
		}
		if (_possibleReferenceFrame != possibleFrame)
		{
			LockOnReticule reticuleWithState = GetReticuleWithState(LockOnReticule.LockState.POSSIBLE_LOCK);
			if (reticuleWithState != null)
			{
				reticuleWithState.SetLockState(LockOnReticule.LockState.OFF);
			}
			if (possibleFrame != null)
			{
				reticuleWithState = GetReticuleWithState(LockOnReticule.LockState.OFF);
				reticuleWithState.SetLockState(LockOnReticule.LockState.POSSIBLE_LOCK);
				if (_type == ReferenceFrameGUIType.MAP)
				{
					bool flag = possibleFrame.GetAstroObject() != null && possibleFrame.GetAstroObject().GetAstroObjectName() == AstroObject.Name.MapSatellite;
					reticuleWithState.ShowFullPrompt(!_hasPlayerLockedOnMap || (flag && !Locator.GetShipLogManager().IsFactRevealed("IP_RING_WORLD_X1")));
				}
				else
				{
					reticuleWithState.ShowFullPrompt(PlayerState.InZeroGTraining() || !_hasPlayerLockedOn);
				}
			}
		}
		_possibleReferenceFrame = possibleFrame;
		_currentReferenceFrame = currentFrame;
	}

	private LockOnReticule GetReticuleWithState(LockOnReticule.LockState lockState)
	{
		if (_reticule1.GetLockState() == lockState)
		{
			return _reticule1;
		}
		if (_reticule2.GetLockState() == lockState)
		{
			return _reticule2;
		}
		return null;
	}

	private void Update()
	{
		UpdateVisibility();
		if (!GUIMode.IsHiddenMode() && !(_activeCam == null) && _showVisuals)
		{
			UpdateReferenceFrameState(_referenceFrameTracker.GetPossibleReferenceFrame(), _referenceFrameTracker.GetReferenceFrame());
			if (_currentReferenceFrame != null)
			{
				UpdateRelativeMotion();
			}
			UpdateLockOnGUI();
		}
	}

	private void UpdateVisibility()
	{
		if (!(_referenceFrameTracker == null))
		{
			bool flag = false;
			switch (_type)
			{
			case ReferenceFrameGUIType.PLAYER:
				flag = _referenceFrameTracker.IsPlayerTargetingActive();
				break;
			case ReferenceFrameGUIType.SHIP:
				flag = _referenceFrameTracker.IsShipTargetingActive() && !_shipController.UsingLandingCam();
				break;
			case ReferenceFrameGUIType.LANDING_CAM:
				flag = _referenceFrameTracker.IsShipTargetingActive() && _shipController.UsingLandingCam();
				break;
			case ReferenceFrameGUIType.MAP:
				flag = _referenceFrameTracker.IsMapTargetingActive();
				break;
			}
			if (_showVisuals != flag)
			{
				SetVisibility(flag);
			}
		}
	}

	private void UpdateLockOnGUI()
	{
		bool flag = false;
		LockOnReticule reticuleWithState = GetReticuleWithState(LockOnReticule.LockState.POSSIBLE_LOCK);
		if (reticuleWithState != null)
		{
			if (_possibleReferenceFrame != null && !GUIMode.IsCaptureMode())
			{
				Vector3 vector = _canvas.WorldToCanvasPosition(_activeCam, _possibleReferenceFrame.GetPosition());
				reticuleWithState.SetScreenSize(GetScreenSize(_possibleReferenceFrame, _activeCam), _canvas.scaleFactor);
				reticuleWithState.GetRequiredComponent<RectTransform>().anchoredPosition = vector;
				if (vector.x >= 0f && vector.x <= _canvas.pixelRect.width && vector.y >= 0f && vector.y <= _canvas.pixelRect.height && vector.z > 0f)
				{
					flag = true;
				}
			}
			if (!reticuleWithState.gameObject.activeSelf && flag)
			{
				reticuleWithState.gameObject.SetActive(value: true);
			}
			else if (reticuleWithState.gameObject.activeSelf && !flag)
			{
				reticuleWithState.gameObject.SetActive(value: false);
			}
		}
		bool value = false;
		LockOnReticule.ArrowState arrowState = LockOnReticule.ArrowState.OFF;
		LockOnReticule reticuleWithState2 = GetReticuleWithState(LockOnReticule.LockState.LOCK);
		if (reticuleWithState2 == null)
		{
			reticuleWithState2 = GetReticuleWithState(LockOnReticule.LockState.POSSIBLE_LOCK);
		}
		if (_currentReferenceFrame == null && _offScreenIndicator.gameObject.activeSelf)
		{
			_offScreenIndicator.gameObject.SetActive(value: false);
		}
		if (_currentReferenceFrame == null || !(reticuleWithState2 != null))
		{
			return;
		}
		Vector3 position = _currentReferenceFrame.GetPosition();
		bool flag2 = false;
		Vector3 vector2 = _referenceCanvas.WorldToCanvasPosition(_activeCam, position);
		reticuleWithState2.SetScreenSize(GetScreenSize(_currentReferenceFrame, _activeCam), _canvas.scaleFactor);
		reticuleWithState2.GetRequiredComponent<RectTransform>().anchoredPosition = new Vector2(vector2.x * _canvas.pixelRect.width / _referenceCanvas.pixelRect.width, vector2.y * _canvas.pixelRect.height / _referenceCanvas.pixelRect.height);
		if (vector2.x >= 0f && vector2.x <= _canvas.pixelRect.width / _canvas.scaleFactor && vector2.y >= 0f && vector2.y <= _canvas.pixelRect.height / _canvas.scaleFactor && vector2.z > 0f)
		{
			flag2 = true;
		}
		if (flag2)
		{
			value = ((!_isDirectTrajectory) ? true : false);
			if (!_isMapView)
			{
				arrowState = ((_orientedRelativeVelocity.z < -10f) ? LockOnReticule.ArrowState.IN : ((!(_orientedRelativeVelocity.z > 10f)) ? LockOnReticule.ArrowState.OFF : LockOnReticule.ArrowState.OUT));
			}
			if (_currentReferenceFrame != null && !_isDirectTrajectory && !_isMapView && flag2)
			{
				reticuleWithState2.SetMotionLines(_orientedRelativeVelocity);
			}
			else
			{
				reticuleWithState2.HideMotionLines();
			}
			if (_offScreenIndicator != null)
			{
				_offScreenIndicator.gameObject.SetActive(value: false);
			}
			if (!reticuleWithState2.gameObject.activeSelf)
			{
				reticuleWithState2.gameObject.SetActive(value: true);
			}
		}
		else
		{
			if (_offScreenIndicator != null)
			{
				_offScreenIndicator.gameObject.SetActive(value: true);
				_offScreenIndicator.SetCanvasPosition(position);
			}
			if (reticuleWithState2.gameObject.activeSelf)
			{
				reticuleWithState2.gameObject.SetActive(value: false);
			}
		}
		reticuleWithState2.SetArrowState(arrowState);
		reticuleWithState2.EnableMotionLines(value);
	}

	private float GetScreenSize(ReferenceFrame referenceFrame, OWCamera camera)
	{
		float num = Vector3.Distance(referenceFrame.GetPosition(), camera.transform.position);
		return Mathf.Clamp(referenceFrame.GetBracketsRadius() * (1f / Mathf.Tan(camera.fieldOfView * ((float)Math.PI / 180f) * 0.5f) / num), 0.125f, 0.5f) * (float)camera.pixelHeight;
	}

	private void UpdateRelativeMotion()
	{
		_relativeVelocity = -_activeBody.GetRelativeVelocity(_currentReferenceFrame);
		Vector3 vector = _activeBody.GetWorldCenterOfMass() - _currentReferenceFrame.GetPosition();
		float magnitude = vector.magnitude;
		Vector3 normalized = (_currentReferenceFrame.GetPosition() - _activeBody.GetPosition()).normalized;
		Vector3 rhs = Vector3.Cross(_activeBody.transform.up, normalized);
		Vector3 rhs2 = Vector3.Cross(normalized, rhs);
		_orientedRelativeVelocity.x = Vector3.Dot(_relativeVelocity, rhs);
		_orientedRelativeVelocity.y = Vector3.Dot(_relativeVelocity, rhs2);
		_orientedRelativeVelocity.z = Vector3.Dot(_relativeVelocity, normalized);
		if (_orientedRelativeVelocity.z < -1f)
		{
			_reticuleColor = _departingColor;
		}
		else if (_orientedRelativeVelocity.z > 1f)
		{
			_reticuleColor = _approachingColor;
		}
		else
		{
			_reticuleColor = _staticColor;
		}
		float num = 1f;
		if (magnitude > 100f)
		{
			num = 10f;
		}
		else if (magnitude > 1000f)
		{
			num = 100f;
		}
		if (new Vector2(_orientedRelativeVelocity.x, _orientedRelativeVelocity.y).sqrMagnitude < num * num)
		{
			_isDirectTrajectory = true;
		}
		else
		{
			_isDirectTrajectory = false;
		}
		_stringBuilder.Length = 0;
		LockOnReticule reticuleWithState = GetReticuleWithState(LockOnReticule.LockState.LOCK);
		if (!(reticuleWithState != null))
		{
			return;
		}
		if (!_isMapView)
		{
			_stringBuilder.Append(_currentReferenceFrame.GetHUDDisplayName());
			if (_stringBuilder.Length > 0)
			{
				_stringBuilder.AppendLine();
			}
			float num2 = vector.magnitude;
			string value = "m";
			if (num2 >= 5000f)
			{
				num2 /= 1000f;
				value = "km";
			}
			_stringBuilder.Append(Mathf.Round(num2));
			_stringBuilder.Append(value);
			_stringBuilder.AppendLine();
			_stringBuilder.Append(Mathf.Round(_orientedRelativeVelocity.z));
			_stringBuilder.Append("m/s");
			reticuleWithState.SetReadoutText(_stringBuilder.ToString());
			reticuleWithState.SetColorWithoutAlpha(_reticuleColor);
			if (_offScreenIndicator != null)
			{
				_offScreenIndicator.SetText(_stringBuilder.ToString());
			}
		}
		else
		{
			reticuleWithState.SetReadoutText(string.Empty);
			_offScreenIndicator.SetText(string.Empty);
		}
	}

	public void SetActiveCamera(OWCamera activeCam)
	{
		_activeCam = activeCam;
		_canvas.renderMode = RenderMode.ScreenSpaceCamera;
		_canvas.worldCamera = activeCam.mainCamera;
		_canvas.planeDistance = _lockOnGuiPlaneDistance;
		if (_offScreenIndicatorCanvas != null && _offScreenIndicatorCanvas.renderMode != RenderMode.WorldSpace)
		{
			_offScreenIndicatorCanvas.renderMode = RenderMode.ScreenSpaceCamera;
			_offScreenIndicatorCanvas.worldCamera = activeCam.mainCamera;
			_offScreenIndicatorCanvas.planeDistance = _offScreenIndicatorPlaneDistance;
		}
	}

	private void OnSwitchActiveCamera(OWCamera activeCam)
	{
		_isMapView = false;
		if (activeCam.CompareTag("MapCamera"))
		{
			_isMapView = true;
		}
		_activeCam = activeCam;
	}

	private void OnPlayerDeath(DeathType type)
	{
		base.enabled = false;
		_currentReferenceFrame = null;
		_possibleReferenceFrame = null;
		_activeCam = null;
	}

	private void SetVisibility(bool visible)
	{
		_showVisuals = visible;
		if (!_showVisuals)
		{
			LockOnReticule reticuleWithState = GetReticuleWithState(LockOnReticule.LockState.POSSIBLE_LOCK);
			if (reticuleWithState != null)
			{
				reticuleWithState.gameObject.SetActive(value: false);
			}
			LockOnReticule reticuleWithState2 = GetReticuleWithState(LockOnReticule.LockState.LOCK);
			if (reticuleWithState2 != null)
			{
				reticuleWithState2.gameObject.SetActive(value: false);
			}
			_offScreenIndicator.gameObject.SetActive(value: false);
		}
		_canvas.enabled = visible;
	}

	private void OnChangeGUIMode()
	{
		if (GUIMode.IsHiddenMode() && _guiObjects != null)
		{
			for (int i = 0; i < _guiObjects.Length; i++)
			{
				_guiObjects[i].SetActive(value: false);
			}
		}
	}
}
