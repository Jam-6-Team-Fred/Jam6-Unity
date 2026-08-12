using UnityEngine;

public class HUDHelmetAnimator : MonoBehaviour
{
	private enum SuitState
	{
		PutOnHelmet = 0,
		RemoveHelmet = 1,
		HelmetOn = 2,
		HelmetOff = 3
	}

	private const string _keyword_MARKERS_ACTIVE = "_MARKERS_ACTIVE";

	private const string _keyword_ABERRATION_ACTIVE = "_ABERRATION_ACTIVE";

	private readonly int _propID_OffsetBendR = Shader.PropertyToID("_OffsetBendR");

	private readonly int _propID_OffsetBendG = Shader.PropertyToID("_OffsetBendG");

	private readonly int _propID_OffsetBendB = Shader.PropertyToID("_OffsetBendB");

	private readonly int _propID_ChannelIntensity = Shader.PropertyToID("_ChannelIntensity");

	private readonly int _propID_MarkerIntensity = Shader.PropertyToID("_MarkerIntensity");

	[SerializeField]
	private Transform _helmetRoot;

	[SerializeField]
	private GameObject _helmetVisuals;

	[SerializeField]
	private MeshRenderer _hudRenderer;

	[Space(10f)]
	[SerializeField]
	private float _helmetMoveSpeed = 1f;

	[Space(10f)]
	[SerializeField]
	private DampedSpring3D _helmetOffsetSpring = new DampedSpring3D();

	[SerializeField]
	private float _jumpImpulse = 0.1f;

	[SerializeField]
	private float _impactImpulseScale = 0.01f;

	[SerializeField]
	private Vector3 _jetpackAccelScale = new Vector3(0.01f, 0.01f, 0.01f);

	[SerializeField]
	private Vector3 _boosterShudderFrequency = new Vector3(30f, 30f, 30f);

	[SerializeField]
	private Vector3 _boosterShudderScale = new Vector3(0.2f, 0.2f, 0.2f);

	[SerializeField]
	private Vector3 _offsetMaxRanges = new Vector3(0.1f, 0.1f, 0.03f);

	[Space(10f)]
	[SerializeField]
	private DampedSpringRadial3D _helmetTwistSpring = new DampedSpringRadial3D();

	[SerializeField]
	private Vector2 _lookTwistSensitivity = Vector2.one;

	[SerializeField]
	private Vector3 _twistMaxRanges = new Vector3(30f, 30f, 0f);

	[Space(10f)]
	[SerializeField]
	private float _hudFlickerOnLength = 1f;

	[SerializeField]
	private float _hudFlickerOutLength = 0.25f;

	[SerializeField]
	private float _hudCalibrationLength = 3f;

	[SerializeField]
	private float _hudCrashLength = 1f;

	[SerializeField]
	private float _hudRebootLength = 2f;

	[Space(10f)]
	[SerializeField]
	private DampedSpring _hudDamageWobbleSpring = new DampedSpring();

	[SerializeField]
	private float _hudDamageWobbleScale = 1f;

	[SerializeField]
	private float _hudDamageFlickerScale = 1f;

	[Space(10f)]
	[SerializeField]
	private ElectricalArc[] _electricalArcs = new ElectricalArc[0];

	[SerializeField]
	private float _electricalArcRadius = 0.1f;

	[Space(10f)]
	[SerializeField]
	private float _hudConversationFadeLength = 0.33f;

	private PlayerCharacterController _playerController;

	private JetpackThrusterModel _playerJetpack;

	private ImpactSensor _playerImpactSensor;

	private PlayerResources _playerResources;

	private Material _hudMaterial;

	private SuitState _suitState;

	private Vector3 _initLocalPos;

	private Vector3 _initLocalScale;

	private float _helmetTimer;

	private bool _isTrainingSuit;

	private bool _justBecameGrounded;

	private bool _hudCalibrated;

	private bool _hudCrashing;

	private bool _hudRebooting;

	private bool _inConversation;

	private bool _hasBigBangHappened;

	private bool _keywordActive_Markers;

	private bool _keywordActive_Aberration;

	private float _hudTimer;

	private float _hudDamageTimer;

	private float _hudDamageWobble;

	private float _hudCrashTimer;

	private float _hudConvoFade;

	private void Awake()
	{
		_suitState = SuitState.HelmetOff;
		Vector3 initLocalPos = (_helmetRoot.localPosition = new Vector3(0f, 1f, _helmetRoot.localPosition.z));
		_initLocalPos = initLocalPos;
		_initLocalScale = _helmetRoot.localScale;
		_helmetTimer = 0f;
		_justBecameGrounded = false;
		_hudCalibrated = false;
		_hudCrashing = false;
		_hudRebooting = false;
		_hudTimer = 0f;
		_hudMaterial = _hudRenderer.sharedMaterial;
		GlobalMessenger.AddListener("PutOnHelmet", OnPutOnHelmet);
		GlobalMessenger.AddListener("RemoveHelmet", OnRemoveHelmet);
		GlobalMessenger.AddListener("ChangeGUIMode", OnChangeGUIMode);
		GlobalMessenger<Signalscope>.AddListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.AddListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger.AddListener("EnterConversation", OnEnterConversation);
		GlobalMessenger.AddListener("ExitConversation", OnExitConversation);
		GlobalMessenger.AddListener("CrashHUD", OnCrashHUD);
		GlobalMessenger.AddListener("BigBangHelmetCrack", OnBigBangHelmetCrack);
		GlobalMessenger<GraphicSettings>.AddListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
	}

	private void Start()
	{
		float num = (float)Screen.width / (float)Screen.height;
		if (!OWMath.ApproxEquals(num, 1.777f, 0.01f))
		{
			_helmetRoot.localScale = Vector3.Scale(_initLocalScale, new Vector3(num / 1.7777778f, 1f, 1f));
		}
		Transform playerTransform = Locator.GetPlayerTransform();
		_playerController = playerTransform.GetComponent<PlayerCharacterController>();
		_playerJetpack = playerTransform.GetComponent<JetpackThrusterModel>();
		_playerImpactSensor = playerTransform.GetComponent<ImpactSensor>();
		_playerResources = playerTransform.GetComponent<PlayerResources>();
		for (int i = 0; i < _electricalArcs.Length; i++)
		{
			_electricalArcs[i].OnJump += OnElectricalArcComplete;
			_electricalArcs[i].enabled = false;
		}
		_playerController.OnJump += OnJump;
		_playerController.OnBecomeGrounded += OnBecomeGrounded;
		_playerImpactSensor.OnImpact += OnImpact;
		_playerResources.OnInstantDamage += OnInstantDamage;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("PutOnHelmet", OnPutOnHelmet);
		GlobalMessenger.RemoveListener("RemoveHelmet", OnRemoveHelmet);
		GlobalMessenger.RemoveListener("ChangeGUIMode", OnChangeGUIMode);
		GlobalMessenger<Signalscope>.RemoveListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.RemoveListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger.RemoveListener("EnterConversation", OnEnterConversation);
		GlobalMessenger.RemoveListener("ExitConversation", OnExitConversation);
		GlobalMessenger.RemoveListener("CrashHUD", OnCrashHUD);
		GlobalMessenger.RemoveListener("BigBangHelmetCrack", OnBigBangHelmetCrack);
		GlobalMessenger<GraphicSettings>.RemoveListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
		for (int i = 0; i < _electricalArcs.Length; i++)
		{
			_electricalArcs[i].OnJump -= OnElectricalArcComplete;
		}
		if ((bool)_playerController)
		{
			_playerController.OnJump -= OnJump;
			_playerController.OnBecomeGrounded -= OnBecomeGrounded;
		}
		if ((bool)_playerImpactSensor)
		{
			_playerImpactSensor.OnImpact -= OnImpact;
		}
		if ((bool)_playerResources)
		{
			_playerResources.OnInstantDamage -= OnInstantDamage;
		}
		if (_hudMaterial != null)
		{
			_hudMaterial.DisableKeyword("_MARKERS_ACTIVE");
			_hudMaterial.DisableKeyword("_ABERRATION_ACTIVE");
		}
	}

	private void OnEnterSignalscopeZoom(Signalscope telescopeO)
	{
		_helmetVisuals.gameObject.SetActive(value: false);
	}

	private void OnExitSignalscopeZoom()
	{
		if (!GUIMode.IsHiddenMode())
		{
			_helmetVisuals.SetActive(value: true);
		}
	}

	private void OnEnterConversation()
	{
		_inConversation = true;
	}

	private void OnExitConversation()
	{
		_inConversation = false;
	}

	private void OnChangeGUIMode()
	{
		if (GUIMode.IsHiddenMode())
		{
			_helmetVisuals.SetActive(value: false);
		}
		else if (base.enabled)
		{
			_helmetVisuals.SetActive(value: true);
		}
	}

	private void OnPutOnHelmet()
	{
		base.enabled = true;
		if (!GUIMode.IsHiddenMode())
		{
			_helmetVisuals.SetActive(value: true);
		}
		_hudCrashing = false;
		_hudRebooting = false;
		_suitState = SuitState.PutOnHelmet;
		_isTrainingSuit = Locator.GetPlayerSuit().IsTrainingSuit();
		_helmetTimer = 0f;
		_hudDamageTimer = 0f;
		_hudDamageWobble = 0f;
		_hudDamageWobbleSpring.ResetVelocity();
		if (PlayerSpacesuit.GetInstantSuitUp())
		{
			_suitState = SuitState.HelmetOn;
			_helmetRoot.localPosition = Vector3.zero;
			_hudTimer = _hudCalibrationLength;
			GlobalMessenger.FireEvent("HelmetHUDActivated");
		}
	}

	private void OnRemoveHelmet()
	{
		_suitState = SuitState.RemoveHelmet;
		_helmetTimer = 0f;
		_hudTimer = 0f;
		if (PlayerSpacesuit.GetInstantRemoveSuit())
		{
			_suitState = SuitState.HelmetOff;
			_helmetRoot.localPosition = _initLocalPos;
		}
	}

	private void OnJump()
	{
		_helmetOffsetSpring.velocity += Vector3.down * _jumpImpulse;
	}

	private void OnBecomeGrounded()
	{
		_justBecameGrounded = true;
	}

	private void OnImpact(ImpactData impact)
	{
		if (!_playerController.IsGrounded() || _justBecameGrounded)
		{
			Vector3 vector = base.transform.InverseTransformVector(impact.velocity) * _impactImpulseScale;
			_helmetOffsetSpring.velocity -= vector;
		}
	}

	private void OnInstantDamage(float instantDamage, InstantDamageType damageType)
	{
		_hudDamageTimer += instantDamage * 0.01f;
		_hudDamageWobbleSpring.velocity += instantDamage * 0.01f;
		if (damageType != InstantDamageType.Electrical || _hudCrashing || _hudRebooting)
		{
			return;
		}
		_hudCrashTimer = 0f;
		_hudCrashing = true;
		_hudRebooting = false;
		_hudCalibrated = false;
		int num = 0;
		for (int i = 0; i < _electricalArcs.Length; i++)
		{
			if (!_electricalArcs[i].enabled && (num < 2 || Random.value > 0.5f))
			{
				_electricalArcs[i].startLocalPosition = Random.insideUnitCircle.normalized * _electricalArcRadius;
				_electricalArcs[i].endLocalPosition = Quaternion.AngleAxis(Random.Range(150f, 210f), Vector3.forward) * _electricalArcs[i].startLocalPosition;
				_electricalArcs[i].enabled = true;
				num++;
			}
		}
	}

	private void OnElectricalArcComplete(ElectricalArc arc)
	{
		arc.enabled = false;
	}

	private void OnCrashHUD()
	{
		_hudCrashTimer = 0f;
		_hudCrashing = true;
		_hudRebooting = false;
		_hudCalibrated = false;
	}

	private void OnBigBangHelmetCrack()
	{
		_hudCrashTimer = 0f;
		_hudCrashing = true;
		_hudRebooting = false;
		_hudCalibrated = false;
		_hasBigBangHappened = true;
	}

	private void OnGraphicSettingsUpdated(GraphicSettings graphicSettings)
	{
		float num = (float)graphicSettings.displayResWidth / (float)graphicSettings.displayResHeight;
		if (!OWMath.ApproxEquals(num, 1.777f, 0.01f))
		{
			_helmetRoot.localScale = Vector3.Scale(_initLocalScale, new Vector3(num / 1.7777778f, 1f, 1f));
		}
		else
		{
			_helmetRoot.localScale = _initLocalScale;
		}
	}

	private void Update()
	{
		_justBecameGrounded = false;
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		Vector2 zero3 = Vector2.zero;
		Vector2 zero4 = Vector2.zero;
		Vector2 zero5 = Vector2.zero;
		Vector2 zero6 = Vector2.zero;
		Vector4 one = Vector4.one;
		bool flag = false;
		_hudTimer += Time.deltaTime;
		_hudDamageTimer = Mathf.Max(_hudDamageTimer - Time.deltaTime / _hudDamageFlickerScale, 0f);
		_hudDamageWobble = _hudDamageWobbleSpring.Update(_hudDamageWobble, 0f, Time.deltaTime);
		_hudConvoFade = Mathf.Clamp01(_hudConvoFade + Time.deltaTime / _hudConversationFadeLength * (_inConversation ? (-1f) : 1f));
		if (_suitState == SuitState.HelmetOn)
		{
			_helmetOffsetSpring.velocity -= Vector3.Scale(_playerJetpack.GetLocalAcceleration(), _jetpackAccelScale) * Time.deltaTime;
			if (_playerJetpack.IsBoosterFiring())
			{
				float x = (Mathf.PerlinNoise(Time.time * _boosterShudderFrequency.x, 0f) * 2f - 1f) * _boosterShudderScale.x;
				float y = (Mathf.PerlinNoise(Time.time * _boosterShudderFrequency.y, 0f) * 2f - 1f) * _boosterShudderScale.y;
				float z = (Mathf.PerlinNoise(Time.time * _boosterShudderFrequency.z, 0f) * 2f - 1f) * _boosterShudderScale.z;
				_helmetOffsetSpring.velocity += new Vector3(x, y, z);
			}
			Vector3 localPosition = _helmetOffsetSpring.Update(_helmetRoot.localPosition, Vector3.zero, Time.deltaTime);
			if (Mathf.Abs(localPosition.x) > _offsetMaxRanges.x)
			{
				localPosition.x = Mathf.Clamp(localPosition.x, 0f - _offsetMaxRanges.x, _offsetMaxRanges.x);
				_helmetOffsetSpring.velocity.x *= -0.5f;
			}
			if (Mathf.Abs(localPosition.y) > _offsetMaxRanges.y)
			{
				localPosition.y = Mathf.Clamp(localPosition.y, 0f - _offsetMaxRanges.y, _offsetMaxRanges.y);
				_helmetOffsetSpring.velocity.y *= -0.5f;
			}
			if (Mathf.Abs(localPosition.z) > _offsetMaxRanges.z)
			{
				localPosition.z = Mathf.Clamp(localPosition.z, 0f - _offsetMaxRanges.z, _offsetMaxRanges.z);
				_helmetOffsetSpring.velocity.z *= -0.5f;
			}
			_helmetRoot.localPosition = localPosition;
			Vector2 axisValue = OWInput.GetAxisValue(InputLibrary.look);
			Vector3 localEulerAngles = _helmetRoot.localEulerAngles;
			localEulerAngles -= new Vector3((0f - axisValue.y) * _lookTwistSensitivity.y, axisValue.x * _lookTwistSensitivity.x, 0f) * Time.timeScale;
			localEulerAngles.x = Mathf.Clamp(OWMath.WrapAngle(localEulerAngles.x), 0f - _twistMaxRanges.x, _twistMaxRanges.x);
			localEulerAngles.y = Mathf.Clamp(OWMath.WrapAngle(localEulerAngles.y), 0f - _twistMaxRanges.y, _twistMaxRanges.y);
			localEulerAngles.z = Mathf.Clamp(OWMath.WrapAngle(localEulerAngles.z), 0f - _twistMaxRanges.z, _twistMaxRanges.z);
			_helmetRoot.localEulerAngles = _helmetTwistSpring.Update(localEulerAngles, Vector3.zero, Time.deltaTime);
			float num = Mathf.Clamp01(_hudTimer / _hudFlickerOnLength);
			if (num < 1f)
			{
				one.x *= ((Mathf.PerlinNoise(Time.timeSinceLevelLoad * 60f, 0f) < num) ? 1f : 0f);
				one.y *= ((Mathf.PerlinNoise(Time.timeSinceLevelLoad * 60f, 1f) < num) ? 1f : 0f);
				one.z *= ((Mathf.PerlinNoise(Time.timeSinceLevelLoad * 60f, 2f) < num) ? 1f : 0f);
				if (_hudCalibrated || _isTrainingSuit)
				{
					float num2 = 1f - num;
					zero4 += new Vector2(Mathf.Sin(Time.timeSinceLevelLoad * 20f), Mathf.Cos(Time.timeSinceLevelLoad * 20f)) * 0.007f * num2;
					zero5 += new Vector2(Mathf.Cos(Time.timeSinceLevelLoad * 4f), Mathf.Sin(Time.timeSinceLevelLoad * -4f)) * 0.003f * num2;
					zero6 += new Vector2(Mathf.Sin(Time.timeSinceLevelLoad * -14f), Mathf.Cos(Time.timeSinceLevelLoad * -14f)) * 0.008f * num2;
				}
			}
			if (_hudCrashing)
			{
				one.x *= ((Mathf.PerlinNoise(Time.timeSinceLevelLoad * 60f, 0f) < 0.5f) ? 1f : 0f);
				one.y *= ((Mathf.PerlinNoise(Time.timeSinceLevelLoad * 60f, 1f) < 0.5f) ? 1f : 0f);
				one.z *= ((Mathf.PerlinNoise(Time.timeSinceLevelLoad * 60f, 2f) < 0.5f) ? 1f : 0f);
				float num3 = Mathf.Floor(Time.timeSinceLevelLoad * 10f) * 0.1f;
				zero4 += new Vector2(Mathf.Sin(num3 * 20f), Mathf.Cos(num3 * 20f)) * 0.07f;
				zero5 += new Vector2(Mathf.Cos(num3 * 4f), Mathf.Sin(num3 * -4f)) * 0.03f;
				zero6 += new Vector2(Mathf.Sin(num3 * -14f), Mathf.Cos(num3 * -14f)) * 0.08f;
				flag = true;
				_hudCrashTimer += Time.deltaTime;
				if (_hudCrashTimer >= _hudCrashLength)
				{
					_hudCrashTimer = 0f;
					_hudCrashing = false;
					_hudRebooting = true;
				}
			}
			else if (_hudRebooting)
			{
				one *= 0f;
				_hudCrashTimer += Time.deltaTime;
				if (_hudCrashTimer >= _hudRebootLength)
				{
					_hudRebooting = false;
					_hudTimer = 0f;
					if (_hasBigBangHappened)
					{
						base.enabled = false;
					}
				}
			}
			else if (!_hudCalibrated && !_isTrainingSuit)
			{
				float num4 = 1f - Mathf.Clamp01(_hudTimer / (_hudCalibrationLength - 0.5f));
				if (num4 > 0f)
				{
					zero += new Vector2(0.015f, 0.05f) * num4;
					zero2 += new Vector2(-0.03f, 0f) * num4;
					zero3 += new Vector2(-0.06f, -0.02f) * num4;
					flag = true;
				}
				if (_hudTimer > _hudCalibrationLength)
				{
					_hudCalibrated = true;
				}
			}
		}
		else if (_suitState == SuitState.PutOnHelmet)
		{
			_helmetTimer += Time.deltaTime;
			_helmetRoot.localPosition = Vector3.Lerp(_initLocalPos, Vector3.zero, Mathf.SmoothStep(0f, 1f, _helmetMoveSpeed * _helmetTimer));
			if (_helmetRoot.localPosition.y <= 0f)
			{
				GlobalMessenger.FireEvent("HelmetHUDActivated");
				_suitState = SuitState.HelmetOn;
				_hudTimer = 0f;
			}
			one *= 0f;
		}
		else if (_suitState == SuitState.RemoveHelmet)
		{
			_helmetTimer += Time.unscaledDeltaTime;
			_helmetRoot.localPosition = Vector3.Lerp(Vector3.zero, _initLocalPos, Mathf.SmoothStep(0f, 1f, _helmetMoveSpeed * _helmetTimer));
			if (_helmetRoot.localPosition.y >= _initLocalPos.y)
			{
				base.enabled = false;
				_helmetRoot.localPosition = _initLocalPos;
				_suitState = SuitState.HelmetOff;
				one *= 0f;
			}
			else
			{
				float num5 = Mathf.Clamp01(_hudTimer / _hudFlickerOutLength);
				zero4 += new Vector2(0.15f, -0.15f) * num5;
				zero5 += new Vector2(-0.15f, -0.15f) * num5;
				zero6 += new Vector2(0f, 0f) * num5;
				flag = true;
			}
		}
		if (_hudDamageTimer > 0f)
		{
			one.y *= ((Mathf.PerlinNoise(Time.timeSinceLevelLoad * 60f, 4f) > _hudDamageTimer) ? 1f : 0f);
			one.z *= ((Mathf.PerlinNoise(Time.timeSinceLevelLoad * 60f, 5f) > _hudDamageTimer) ? 1f : 0f);
		}
		if (Mathf.Abs(_hudDamageWobble) > 0.001f)
		{
			zero4 += new Vector2(Mathf.Sin(Time.timeSinceLevelLoad * 20f), Mathf.Cos(Time.timeSinceLevelLoad * 20f)) * 0.007f * _hudDamageWobble * _hudDamageWobbleScale;
			zero5 += new Vector2(Mathf.Cos(Time.timeSinceLevelLoad * 4f), Mathf.Sin(Time.timeSinceLevelLoad * -4f)) * 0.003f * _hudDamageWobble * _hudDamageWobbleScale;
			zero6 += new Vector2(Mathf.Sin(Time.timeSinceLevelLoad * -14f), Mathf.Cos(Time.timeSinceLevelLoad * -14f)) * 0.008f * _hudDamageWobble * _hudDamageWobbleScale;
			flag = true;
		}
		one.w *= _hudConvoFade;
		if (_isTrainingSuit)
		{
			one.x *= 0f;
			one.z *= 0f;
		}
		bool flag2 = !_hudCalibrated && !_isTrainingSuit;
		bool flag3 = flag;
		if (flag2 && !_keywordActive_Markers)
		{
			_hudMaterial.EnableKeyword("_MARKERS_ACTIVE");
			_keywordActive_Markers = true;
		}
		else if (!flag2 && _keywordActive_Markers)
		{
			_hudMaterial.DisableKeyword("_MARKERS_ACTIVE");
			_keywordActive_Markers = false;
		}
		if (flag3 && !_keywordActive_Aberration)
		{
			_hudMaterial.EnableKeyword("_ABERRATION_ACTIVE");
			_keywordActive_Aberration = true;
		}
		else if (!flag3 && _keywordActive_Aberration)
		{
			_hudMaterial.DisableKeyword("_ABERRATION_ACTIVE");
			_keywordActive_Aberration = false;
		}
		if (_keywordActive_Markers)
		{
			_hudMaterial.SetFloat(_propID_MarkerIntensity, (_hudCalibrated || _isTrainingSuit) ? 0f : 1f);
		}
		if (_keywordActive_Aberration)
		{
			_hudMaterial.SetVector(_propID_OffsetBendR, new Vector4(zero.x, zero.y, zero4.x, zero4.y));
			_hudMaterial.SetVector(_propID_OffsetBendG, new Vector4(zero2.x, zero2.y, zero5.x, zero5.y));
			_hudMaterial.SetVector(_propID_OffsetBendB, new Vector4(zero3.x, zero3.y, zero6.x, zero6.y));
		}
		_hudMaterial.SetVector(_propID_ChannelIntensity, one);
	}
}
