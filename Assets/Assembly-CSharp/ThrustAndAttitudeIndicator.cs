using UnityEngine;

public class ThrustAndAttitudeIndicator : MonoBehaviour
{
	private int _propID_BarPosition;

	[SerializeField]
	private Transform _thrusterArrowRoot;

	[SerializeField]
	private MeshRenderer _rendererForward;

	[SerializeField]
	private MeshRenderer _rendererBack;

	[SerializeField]
	private MeshRenderer _rendererRight;

	[SerializeField]
	private MeshRenderer _rendererLeft;

	[SerializeField]
	private MeshRenderer _rendererUp;

	[SerializeField]
	private MeshRenderer _rendererDown;

	[SerializeField]
	private MeshRenderer _boostArrows;

	[Space(16f)]
	[SerializeField]
	private Light[] _lightsForward;

	[SerializeField]
	private Light[] _lightsBack;

	[SerializeField]
	private Light[] _lightsRight;

	[SerializeField]
	private Light[] _lightsLeft;

	[SerializeField]
	private Light[] _lightsUp;

	[SerializeField]
	private Light[] _lightsDown;

	[Space(16f)]
	[SerializeField]
	private Transform _yawCircle;

	[SerializeField]
	private Transform _pitchCircle;

	[SerializeField]
	private Transform _rollCircle;

	[Space(16f)]
	[SerializeField]
	private bool _reticuleMode;

	[SerializeField]
	private bool _shipIndicatorMode;

	private OWRigidbody _targetBody;

	private PlayerCameraController _playerCameraController;

	private JetpackThrusterModel _jetpackThrusterModel;

	private ThrusterModel _shipThrusterModel;

	private ThrusterModel _activeThrusterModel;

	private ThrusterController _jetpackThrusterController;

	private ThrusterController _shipThrusterController;

	private ThrusterController _activeThrusterController;

	private RulesetDetector _shipRulesetDetector;

	private Vector3 _origLocalEulerAngle;

	private bool _inConversation;

	private void Awake()
	{
		_propID_BarPosition = Shader.PropertyToID("_BarPosition");
		if (_reticuleMode)
		{
			Canvas componentInParent = GetComponentInParent<Canvas>();
			if (componentInParent != null)
			{
				componentInParent.worldCamera = Locator.GetPlayerCamera().mainCamera;
			}
		}
		_origLocalEulerAngle = base.transform.localEulerAngles;
		GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.AddListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger.AddListener("EnterConversation", OnEnterConversation);
		GlobalMessenger.AddListener("ExitConversation", OnExitConversation);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
	}

	private void Start()
	{
		GameObject obj = Locator.GetPlayerTransform().gameObject;
		Transform shipTransform = Locator.GetShipTransform();
		_playerCameraController = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
		_jetpackThrusterModel = obj.GetRequiredComponent<JetpackThrusterModel>();
		_jetpackThrusterController = obj.GetRequiredComponent<ThrusterController>();
		if (_shipIndicatorMode)
		{
			if (shipTransform != null)
			{
				_targetBody = shipTransform.GetRequiredComponent<OWRigidbody>();
				_shipThrusterModel = shipTransform.GetRequiredComponent<ThrusterModel>();
				_shipThrusterController = shipTransform.GetRequiredComponent<ThrusterController>();
				_activeThrusterModel = _shipThrusterModel;
				_activeThrusterController = _shipThrusterController;
				_shipRulesetDetector = Locator.GetShipDetector().GetComponent<RulesetDetector>();
			}
			_thrusterArrowRoot.gameObject.SetActive(value: false);
			base.enabled = false;
		}
		else
		{
			_targetBody = obj.GetRequiredComponent<OWRigidbody>();
			_activeThrusterModel = _jetpackThrusterModel;
			_activeThrusterController = _jetpackThrusterController;
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.RemoveListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger.RemoveListener("EnterConversation", OnEnterConversation);
		GlobalMessenger.RemoveListener("ExitConversation", OnExitConversation);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
	}

	private void OnEnterFlightConsole(OWRigidbody shipBody)
	{
		_activeThrusterModel = shipBody.GetRequiredComponent<ThrusterModel>();
		_activeThrusterController = shipBody.GetRequiredComponent<ThrusterController>();
		if (_shipIndicatorMode)
		{
			_thrusterArrowRoot.gameObject.SetActive(value: true);
			base.enabled = true;
		}
		else
		{
			ResetAllArrows();
			_thrusterArrowRoot.gameObject.SetActive(value: false);
			base.enabled = false;
		}
	}

	private void OnExitFlightConsole()
	{
		_activeThrusterModel = _jetpackThrusterModel;
		_activeThrusterController = _jetpackThrusterController;
		if (_shipIndicatorMode)
		{
			ResetAllArrows();
			_thrusterArrowRoot.gameObject.SetActive(value: false);
			base.enabled = false;
		}
		else
		{
			_thrusterArrowRoot.gameObject.SetActive(value: true);
			base.enabled = true;
		}
	}

	private void LateUpdate()
	{
		if (!(_thrusterArrowRoot != null))
		{
			return;
		}
		if (GUIMode.IsHiddenMode())
		{
			_thrusterArrowRoot.gameObject.SetActive(value: false);
			return;
		}
		if (!_inConversation && !_thrusterArrowRoot.gameObject.activeSelf)
		{
			_thrusterArrowRoot.gameObject.SetActive(value: true);
		}
		if (!_shipIndicatorMode)
		{
			float degreesY = _playerCameraController.GetDegreesY();
			float num = degreesY;
			if (degreesY < 0f)
			{
				float t = Mathf.InverseLerp(0f, _playerCameraController.GetMinDegreesY(), degreesY);
				num = Mathf.Lerp(0f, -50f, t);
			}
			_thrusterArrowRoot.localEulerAngles = _origLocalEulerAngle + Vector3.right * num;
		}
		Vector3 localAcceleration = _activeThrusterModel.GetLocalAcceleration();
		float num2 = _activeThrusterModel.GetMaxTranslationalThrust();
		if (_shipRulesetDetector != null)
		{
			num2 = Mathf.Min(_shipRulesetDetector.GetThrustLimit(), num2);
		}
		if (localAcceleration.magnitude > 0f)
		{
			if (localAcceleration.x < 0f)
			{
				DisplayArrows(localAcceleration.x * -1f, num2, _rendererRight, _lightsRight);
				DisplayArrows(0f, num2, _rendererLeft, _lightsLeft);
			}
			else
			{
				DisplayArrows(localAcceleration.x, num2, _rendererLeft, _lightsLeft);
				DisplayArrows(0f, num2, _rendererRight, _lightsRight);
			}
			if (localAcceleration.y < 0f)
			{
				DisplayArrows(localAcceleration.y * -1f, num2, _rendererUp, _lightsUp);
				DisplayArrows(0f, num2, _rendererDown, _lightsDown);
			}
			else
			{
				DisplayArrows(0f, num2, _rendererUp, _lightsUp);
				if (_jetpackThrusterModel.IsBoosterFiring())
				{
					float value = InputLibrary.thrustUp.GetValue();
					DisplayArrows(value, 1f, _boostArrows, null);
					DisplayArrows(value, 1f, _rendererDown, _lightsDown);
				}
				else
				{
					DisplayArrows(0f, 1f, _boostArrows, null);
					DisplayArrows(localAcceleration.y, num2, _rendererDown, _lightsDown);
				}
			}
			if (localAcceleration.z < 0f)
			{
				DisplayArrows(localAcceleration.z * -1f, num2, _rendererForward, _lightsForward);
				DisplayArrows(0f, num2, _rendererBack, _lightsBack);
			}
			else
			{
				DisplayArrows(localAcceleration.z, num2, _rendererBack, _lightsBack);
				DisplayArrows(0f, num2, _rendererForward, _lightsForward);
			}
		}
		else
		{
			DisplayArrows(0f, num2, _rendererRight, _lightsRight);
			DisplayArrows(0f, num2, _rendererLeft, _lightsLeft);
			DisplayArrows(0f, num2, _rendererUp, _lightsUp);
			DisplayArrows(0f, num2, _rendererDown, _lightsDown);
			DisplayArrows(0f, num2, _rendererForward, _lightsForward);
			DisplayArrows(0f, num2, _rendererBack, _lightsBack);
			DisplayArrows(0f, num2, _boostArrows, null);
		}
		if (_reticuleMode)
		{
			_thrusterArrowRoot.rotation = _targetBody.transform.rotation;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		if (_activeThrusterController.IsRollMode())
		{
			flag = true;
			flag2 = true;
			flag3 = false;
		}
		else if (PlayerState.InZeroG())
		{
			flag2 = true;
		}
		if (_rollCircle != null && _rollCircle.gameObject.activeSelf != flag)
		{
			_rollCircle.gameObject.SetActive(flag);
		}
		if (_pitchCircle != null && _pitchCircle.gameObject.activeSelf != flag2)
		{
			_pitchCircle.gameObject.SetActive(flag2);
		}
		if (_yawCircle != null && _yawCircle.gameObject.activeSelf != flag3)
		{
			_yawCircle.gameObject.SetActive(flag3);
		}
	}

	private void DisplayArrows(float value, float maxValue, MeshRenderer barRenderer, Light[] barLights)
	{
		if (barRenderer == null)
		{
			return;
		}
		float num = value / maxValue;
		barRenderer.material.SetFloat(_propID_BarPosition, num);
		if (barLights != null)
		{
			for (int i = 0; i < barLights.Length; i++)
			{
				barLights[i].enabled = ((float)(i + 1) - 0.5f) / (float)barLights.Length <= num;
			}
		}
	}

	private void ResetAllArrows()
	{
		_rendererForward.material.SetFloat(_propID_BarPosition, 0f);
		_rendererBack.material.SetFloat(_propID_BarPosition, 0f);
		_rendererRight.material.SetFloat(_propID_BarPosition, 0f);
		_rendererLeft.material.SetFloat(_propID_BarPosition, 0f);
		_rendererUp.material.SetFloat(_propID_BarPosition, 0f);
		_rendererDown.material.SetFloat(_propID_BarPosition, 0f);
		if (_boostArrows != null)
		{
			_boostArrows.material.SetFloat(_propID_BarPosition, 0f);
		}
		for (int i = 0; i < _lightsForward.Length; i++)
		{
			_lightsForward[i].enabled = false;
		}
		for (int j = 0; j < _lightsBack.Length; j++)
		{
			_lightsBack[j].enabled = false;
		}
		for (int k = 0; k < _lightsRight.Length; k++)
		{
			_lightsRight[k].enabled = false;
		}
		for (int l = 0; l < _lightsLeft.Length; l++)
		{
			_lightsLeft[l].enabled = false;
		}
		for (int m = 0; m < _lightsUp.Length; m++)
		{
			_lightsUp[m].enabled = false;
		}
		for (int n = 0; n < _lightsDown.Length; n++)
		{
			_lightsDown[n].enabled = false;
		}
	}

	private void OnEnterConversation()
	{
		_thrusterArrowRoot.gameObject.SetActive(value: false);
		_inConversation = true;
	}

	private void OnExitConversation()
	{
		_thrusterArrowRoot.gameObject.SetActive(value: true);
		_inConversation = false;
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		base.enabled = false;
	}
}
