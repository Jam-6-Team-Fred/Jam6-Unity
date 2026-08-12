using System;
using UnityEngine;

public class RoastingStickController : MonoBehaviour, ILateInitializer
{
	private const float MAX_TILT_DEGREES = 45f;

	private const float ACTIVATION_RATE = 90f;

	private const float ROCK_COLLISION_HEIGHT = 0.5f;

	[SerializeField]
	private Transform _stickPivotTransform;

	[SerializeField]
	private Transform _stickTransform;

	[SerializeField]
	private Marshmallow _marshmallow;

	[SerializeField]
	private GameObject _mallowBodyPrefab;

	[SerializeField]
	private GameObject _unsuitedRoastingArm;

	[SerializeField]
	private GameObject _suitedRoastingArm;

	private Campfire _campfire;

	private float _stickMaxZ;

	private float _stickMinZ = 0.75f;

	private bool _active;

	private Quaternion _startRotation;

	private Quaternion _targetRotation;

	private Quaternion _inactiveRotation;

	private float _initRotationTime;

	private float _rotationDuration;

	private Vector2 _cursorPos = Vector2.zero;

	private float _extendFraction;

	private bool _screenPromptsInitialized;

	private ScreenPrompt _tiltPrompt;

	private ScreenPrompt _extendPrompt;

	private ScreenPrompt _mallowPrompt;

	private ScreenPrompt _removePrompt;

	private ScreenPrompt _exitPrompt;

	private bool _showMallowPrompt;

	private bool _showRemovePrompt;

	private PlayerAudioController _audioController;

	private Vector3 _debugPos;

	private Vector3 _debugPos2;

	private string _promptText;

	private void Awake()
	{
		_stickPivotTransform.gameObject.SetActive(value: false);
		_stickMaxZ = _stickTransform.localPosition.z;
		_inactiveRotation = Quaternion.AngleAxis(90f, Vector3.up);
		_stickTransform.localPosition = new Vector3(0f, 0f, _stickMinZ);
		base.transform.localRotation = _inactiveRotation;
		_screenPromptsInitialized = false;
		LateInitializerManager.RegisterLateInitializer(this);
		_tiltPrompt = new ScreenPrompt(InputLibrary.look, UITextLibrary.GetString(UITextType.RoastingMovePrompt));
		_extendPrompt = new ScreenPrompt(InputLibrary.extendStick, UITextLibrary.GetString(UITextType.RoastingExtendPrompt));
		_mallowPrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.RoastingEatPrompt));
		_removePrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.RoastingTossPrompt));
		_exitPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.RoastingExitPrompt));
		GlobalMessenger<Campfire>.AddListener("EnterRoastingMode", OnEnterRoastingMode);
		GlobalMessenger.AddListener("ExitRoastingMode", OnExitRoastingMode);
		GlobalMessenger.AddListener("SuitUp", OnPutOnSuit);
		GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
	}

	private void Start()
	{
		if (Locator.GetPlayerSuit().IsWearingSuit())
		{
			OnPutOnSuit();
		}
		else
		{
			OnRemoveSuit();
		}
		_audioController = Locator.GetPlayerTransform().GetComponentInChildren<PlayerAudioController>();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_screenPromptsInitialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
		GlobalMessenger<Campfire>.RemoveListener("EnterRoastingMode", OnEnterRoastingMode);
		GlobalMessenger.RemoveListener("ExitRoastingMode", OnExitRoastingMode);
		GlobalMessenger.RemoveListener("SuitUp", OnPutOnSuit);
		GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
	}

	public void LateInitialize()
	{
		_screenPromptsInitialized = true;
		Locator.GetPromptManager().AddScreenPrompt(_mallowPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_removePrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_exitPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_tiltPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_extendPrompt, PromptPosition.UpperRight);
	}

	private void Update()
	{
		float t = (Time.time - _initRotationTime) / _rotationDuration;
		t = Mathf.SmoothStep(0f, 1f, t);
		base.transform.localRotation = Quaternion.Slerp(_startRotation, _targetRotation, t);
		if (!_active && t >= 1f)
		{
			base.enabled = false;
			_stickPivotTransform.gameObject.SetActive(value: false);
		}
		UpdateExtension();
		UpdateRotation();
		if (_active)
		{
			UpdateMarshmallowInput();
		}
		_marshmallow.UpdateRoast(_campfire);
		if (OWInput.GetInputMode() == InputMode.Roasting)
		{
			_extendPrompt.SetVisibility(isVisible: true);
			_tiltPrompt.SetVisibility(isVisible: true);
			_exitPrompt.SetVisibility(isVisible: true);
			_mallowPrompt.SetVisibility(_showMallowPrompt);
			_removePrompt.SetVisibility(_showRemovePrompt);
		}
		else
		{
			_mallowPrompt.SetVisibility(isVisible: false);
			_extendPrompt.SetVisibility(isVisible: false);
			_tiltPrompt.SetVisibility(isVisible: false);
			_exitPrompt.SetVisibility(isVisible: false);
			_removePrompt.SetVisibility(isVisible: false);
		}
	}

	private void UpdateMarshmallowInput()
	{
		bool flag = false;
		bool showRemovePrompt = false;
		string text = "";
		if (_extendFraction == 0f)
		{
			if (_marshmallow.IsEdible())
			{
				text = UITextLibrary.GetString(UITextType.RoastingEatPrompt);
				flag = true;
				if (_marshmallow.IsBurned())
				{
					showRemovePrompt = true;
					if (OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.Roasting))
					{
						InputLibrary.cancel.ConsumeInput();
						_marshmallow.Remove();
						Locator.GetPlayerAudioController().PlayMarshmallowToss();
						GameObject obj = UnityEngine.Object.Instantiate(_mallowBodyPrefab, _stickTransform.position, _stickTransform.rotation);
						OWRigidbody component = obj.GetComponent<OWRigidbody>();
						component.SetVelocity(_campfire.GetAttachedOWRigidbody().GetPointVelocity(_stickTransform.position) + _stickTransform.forward * 3f);
						component.SetAngularVelocity(_stickTransform.right * 10f);
						obj.GetComponentInChildren<MeshRenderer>().material.color = _marshmallow.GetBurntColor();
					}
				}
				if (OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.Roasting) && _marshmallow.IsEdible())
				{
					_marshmallow.Eat();
				}
			}
			else if (_marshmallow.GetState() == Marshmallow.MallowState.Burning)
			{
				text = UITextLibrary.GetString(UITextType.RoastingExtinguishPrompt);
				flag = true;
				if (OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.Roasting))
				{
					_marshmallow.Extinguish();
				}
			}
			else if (_marshmallow.GetState() == Marshmallow.MallowState.Gone)
			{
				text = UITextLibrary.GetString(UITextType.RoastingReplacePrompt);
				flag = true;
				if (OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.Roasting))
				{
					_marshmallow.SpawnMallow(playSFX: true);
				}
			}
			if (flag && _promptText != text)
			{
				_promptText = text;
				_mallowPrompt.SetText(_promptText);
			}
			if (OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.Roasting))
			{
				_campfire.StopRoasting();
				return;
			}
		}
		_showMallowPrompt = flag;
		_showRemovePrompt = showRemovePrompt;
	}

	private void UpdateExtension()
	{
		if (_active)
		{
			if (OWInput.UsingGamepad())
			{
				_extendFraction = Mathf.Clamp01(OWInput.GetValue(InputLibrary.extendStick, InputMode.Roasting));
			}
			else if (OWInput.GetValue(InputLibrary.extendStick, InputMode.Roasting) > 0f)
			{
				_extendFraction += 0.25f;
				_extendFraction = Mathf.Max(0f, Mathf.Min(1f, _extendFraction));
			}
			else
			{
				_extendFraction -= 0.25f;
				if (_extendFraction < 0f)
				{
					_extendFraction = 0f;
				}
				_extendFraction = Mathf.Max(0f, Mathf.Min(1f, _extendFraction));
			}
		}
		else
		{
			_extendFraction = 0f;
		}
		float b = _stickMinZ + (CalculateMaxStickExtension() - _stickMinZ) * _extendFraction;
		_stickTransform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(_stickTransform.localPosition.z, b, 0.2f));
	}

	private float CalculateMaxStickExtension()
	{
		float num = _stickMaxZ;
		Vector3 position = new Vector3(0f, 0f, num);
		Vector3 position2 = _stickPivotTransform.TransformPoint(position);
		float num2 = _campfire.GetRockHeight() - _campfire.transform.InverseTransformPoint(position2).y;
		if (num2 > 0f)
		{
			float f = Vector3.Angle(_stickPivotTransform.forward, _campfire.transform.up) * ((float)Math.PI / 180f);
			float num3 = num2 / Mathf.Sin(f);
			num -= num3;
		}
		if (_campfire.CheckStickIntersection(_stickPivotTransform.position, _stickPivotTransform.forward, out var intersectPoint))
		{
			float z = _stickPivotTransform.InverseTransformPoint(intersectPoint).z;
			num = Mathf.Min(num, z);
		}
		return num;
	}

	private void UpdateRotation()
	{
		Vector2 axisValue = OWInput.GetAxisValue(InputLibrary.look, InputMode.Roasting);
		float num = (OWInput.UsingGamepad() ? Mathf.Lerp(5f, 1f, _extendFraction) : Mathf.Lerp(2f, 1f, _extendFraction));
		_cursorPos += axisValue * num * Time.deltaTime;
		if (_cursorPos.magnitude > 1f)
		{
			_cursorPos = _cursorPos.normalized;
		}
		float angle = _cursorPos.magnitude * 45f;
		Vector3 axis = Vector3.Cross(Vector3.forward, _cursorPos);
		Quaternion b = Quaternion.AngleAxis(angle, axis);
		_stickPivotTransform.localRotation = OWUtilities.GetWobbleRotation(0.2f, 0.1f) * Quaternion.Slerp(_stickPivotTransform.localRotation, b, 0.5f) * Quaternion.identity;
	}

	private void TriggerRotation(Quaternion targetRotation)
	{
		_initRotationTime = Time.time;
		_targetRotation = targetRotation;
		_startRotation = base.transform.localRotation;
		_rotationDuration = Quaternion.Angle(_startRotation, _targetRotation) / 90f;
	}

	private void OnEnterRoastingMode(Campfire campfire)
	{
		base.enabled = true;
		_audioController.PlayMarshmallowStickEquip();
		_stickPivotTransform.gameObject.SetActive(value: true);
		_active = true;
		_cursorPos = new Vector2(0.5f, 0f);
		_stickTransform.localRotation = Quaternion.identity;
		_marshmallow.SpawnMallow(playSFX: false);
		TriggerRotation(Quaternion.identity);
		_campfire = campfire;
		_extendPrompt.SetVisibility(isVisible: true);
		_tiltPrompt.SetVisibility(isVisible: true);
		_exitPrompt.SetVisibility(isVisible: true);
	}

	private void OnExitRoastingMode()
	{
		_active = false;
		_audioController.PlayMarshmallowStickEquip();
		_marshmallow.Disable();
		TriggerRotation(_inactiveRotation);
		_mallowPrompt.SetVisibility(isVisible: false);
		_extendPrompt.SetVisibility(isVisible: false);
		_tiltPrompt.SetVisibility(isVisible: false);
		_exitPrompt.SetVisibility(isVisible: false);
		_removePrompt.SetVisibility(isVisible: false);
	}

	private void OnPutOnSuit()
	{
		_unsuitedRoastingArm.SetActive(value: false);
		_suitedRoastingArm.SetActive(value: true);
	}

	private void OnRemoveSuit()
	{
		_unsuitedRoastingArm.SetActive(value: true);
		_suitedRoastingArm.SetActive(value: false);
	}
}
