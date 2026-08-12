using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class LockOnReticule : MonoBehaviour, IPromptVisibilityController
{
	public enum LockState
	{
		LOCK = 0,
		POSSIBLE_LOCK = 1,
		OFF = 2
	}

	public enum ArrowState
	{
		OUT = 0,
		IN = 1,
		OFF = 2
	}

	[Header("Arrows")]
	[SerializeField]
	private GameObject _reticuleArrowRoot;

	[SerializeField]
	private Transform[] _reticuleArrowPivots;

	[SerializeField]
	private MeshRenderer[] _reticuleArrowMeshes;

	[Header("LockOn")]
	[SerializeField]
	private float _lockOnScalar = 1.25f;

	[SerializeField]
	private GameObject _reticuleLockOnRoot;

	[SerializeField]
	private MeshRenderer[] _reticuleLockOnMeshes;

	[Header("Highlight")]
	[SerializeField]
	private float _highlightScalar = 1.35f;

	[SerializeField]
	private GameObject _reticuleHighlightRoot;

	[SerializeField]
	private MeshRenderer[] _reticuleHighlightMeshes;

	[Header("Readout")]
	[SerializeField]
	private Transform _readoutScaleRoot;

	[SerializeField]
	private Text _readout;

	[Header("Relative Motion Lines")]
	[SerializeField]
	private UILineRenderer _lineX;

	[SerializeField]
	private UILineRenderer _lineY;

	[SerializeField]
	private ScreenPromptList _promptListBlock;

	private Color _resetColor = Color.white;

	private Material _materialInst;

	private LockState _lockState = LockState.OFF;

	private float _possLockReticuleAlpha = 1f;

	private float _lockReticuleAlpha = 1f;

	private float _bracketScale = 1f;

	private float _possLockScale = 1f;

	private float _lockScale = 1f;

	private bool _lockAnimationActive;

	private bool _unlockAnimationActive;

	private float _screenPixelSize;

	private JetpackPromptController _jetpackPromptController;

	private ScreenPrompt _lockOnPrompt;

	private ScreenPrompt _matchVelocityPrompt;

	private bool _initialized;

	private bool _showFullPrompt;

	private string _lockOnPromptText;

	private string _lockOnPromptTextShortened;

	public event PromptVisibilityChangeEvent OnPromptVisibilityChange
	{
		add
		{
			throw new NotSupportedException();
		}
		remove
		{
		}
	}

	private void Awake()
	{
		if (_reticuleLockOnMeshes.Length != 0)
		{
			_materialInst = _reticuleLockOnMeshes[0].material;
		}
		for (int i = 0; i < _reticuleArrowMeshes.Length; i++)
		{
			_reticuleArrowMeshes[i].sharedMaterial = _materialInst;
		}
		for (int j = 0; j < _reticuleLockOnMeshes.Length; j++)
		{
			_reticuleLockOnMeshes[j].sharedMaterial = _materialInst;
		}
		for (int k = 0; k < _reticuleHighlightMeshes.Length; k++)
		{
			_reticuleHighlightMeshes[k].sharedMaterial = _materialInst;
		}
	}

	private void OnDestroy()
	{
		if (_materialInst != null)
		{
			UnityEngine.Object.Destroy(_materialInst);
		}
		_materialInst = null;
	}

	public void ShowFullPrompt(bool showFullPrompt)
	{
		if (_showFullPrompt != showFullPrompt)
		{
			_showFullPrompt = showFullPrompt;
			_lockOnPrompt.SetText(showFullPrompt ? _lockOnPromptText : _lockOnPromptTextShortened);
		}
	}

	public void Init()
	{
		if (!_initialized)
		{
			_jetpackPromptController = Locator.GetPlayerTransform().GetComponent<JetpackPromptController>();
			_lockOnPromptText = "<CMD>" + UITextLibrary.GetString(UITextType.PressPrompt) + "   " + UITextLibrary.GetString(UITextType.LockOnPrompt);
			_lockOnPromptTextShortened = "<CMD>";
			_lockOnPrompt = new ScreenPrompt(InputLibrary.lockOn, _lockOnPromptTextShortened);
			_matchVelocityPrompt = new ScreenPrompt(InputLibrary.matchVelocity, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.MatchVelocityPrompt));
			_readout.gameObject.SetActive(value: false);
			_promptListBlock.Init();
			Locator.GetPromptManager().AddScreenPrompt(_lockOnPrompt, _promptListBlock, TextAnchor.MiddleLeft);
			Locator.GetPromptManager().AddScreenPrompt(_matchVelocityPrompt, _promptListBlock, TextAnchor.MiddleLeft);
			_initialized = true;
		}
	}

	private void Update()
	{
		if (_initialized)
		{
			UpdateLockUnlockAnimation();
			UpdateScreenPrompts();
		}
	}

	private void UpdateScreenPrompts()
	{
		_lockOnPrompt.SetVisibility(_lockState == LockState.POSSIBLE_LOCK);
		_matchVelocityPrompt.SetVisibility(PlayerState.InZeroGTraining() && _jetpackPromptController.AllowMatchVelocityPrompt());
	}

	private void UpdateLockUnlockAnimation()
	{
		float num = 10f;
		if (_unlockAnimationActive)
		{
			_bracketScale = Mathf.Clamp01(_bracketScale += Time.deltaTime * num);
			if (_bracketScale == 1f)
			{
				_bracketScale = 1f;
				_unlockAnimationActive = false;
				if (_lockState == LockState.OFF)
				{
					base.gameObject.SetActive(value: false);
				}
			}
			float scale = _bracketScale * (_possLockScale - _lockScale) + _lockScale;
			SetScale(scale);
		}
		if (_lockAnimationActive)
		{
			_bracketScale = Mathf.Clamp01(_bracketScale += Time.deltaTime * (0f - num));
			if (_bracketScale == 0f)
			{
				_bracketScale = 0f;
				_lockAnimationActive = false;
				SetAlpha(_lockReticuleAlpha);
				EnableTextReadout(value: true);
			}
			float scale = _bracketScale * (_possLockScale - _lockScale) + _lockScale;
			SetScale(scale);
		}
	}

	public void SetPossibleLockAlpha(float value)
	{
		_possLockReticuleAlpha = value;
	}

	public void SetLockAlpha(float value)
	{
		_lockReticuleAlpha = value;
	}

	public void SetLockState(LockState state)
	{
		if (state == _lockState)
		{
			return;
		}
		switch (state)
		{
		case LockState.LOCK:
			if (!GUIMode.IsHiddenMode())
			{
				base.gameObject.SetActive(value: true);
			}
			_lockAnimationActive = true;
			_unlockAnimationActive = false;
			_bracketScale = 1f;
			EnableTextReadout(value: true);
			SetAlpha(_lockReticuleAlpha);
			EnableMotionLines(value: true);
			EnableHighlight(value: false);
			EnableLockOn(value: true);
			break;
		case LockState.POSSIBLE_LOCK:
			if (!GUIMode.IsHiddenMode())
			{
				base.gameObject.SetActive(value: true);
			}
			if (_lockState == LockState.OFF)
			{
				SetScale(_possLockScale);
			}
			else
			{
				_unlockAnimationActive = true;
				_lockAnimationActive = false;
				_bracketScale = 0f;
			}
			SetAlpha(_possLockReticuleAlpha);
			EnableTextReadout(value: false);
			EnableMotionLines(value: false);
			EnableHighlight(value: true);
			EnableLockOn(value: false);
			SetArrowState(ArrowState.OFF);
			ResetColor();
			break;
		case LockState.OFF:
			if (_lockState == LockState.LOCK)
			{
				_unlockAnimationActive = true;
				_lockAnimationActive = false;
				_bracketScale = 0f;
				EnableTextReadout(value: false);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
			SetArrowState(ArrowState.OFF);
			EnableMotionLines(value: false);
			EnableHighlight(value: true);
			EnableLockOn(value: false);
			ResetColor();
			break;
		}
		_lockState = state;
	}

	public LockState GetLockState()
	{
		return _lockState;
	}

	public void SetScreenSize(float pixelSize, float canvasScaleFactor)
	{
		_screenPixelSize = pixelSize;
		_lockScale = _screenPixelSize / (this.GetRequiredComponent<RectTransform>().rect.width * canvasScaleFactor) * _lockOnScalar;
		_possLockScale = _lockScale * _highlightScalar;
		if (_lockState == LockState.LOCK)
		{
			SetScale(_lockScale);
		}
		if (_lockState == LockState.POSSIBLE_LOCK)
		{
			SetScale(_possLockScale);
		}
	}

	private void SetScale(float scale)
	{
		base.transform.localScale = Vector3.one * scale;
		Vector3 localScale = new Vector3(1f / base.transform.localScale.x, 1f / base.transform.localScale.y, 1f / base.transform.localScale.z);
		_readoutScaleRoot.localScale = localScale;
		_promptListBlock.GetRequiredComponent<RectTransform>().localScale = localScale;
	}

	public void EnableHighlight(bool value)
	{
		if ((bool)_reticuleHighlightRoot)
		{
			_reticuleHighlightRoot.gameObject.SetActive(value);
		}
	}

	public void EnableLockOn(bool value)
	{
		if ((bool)_reticuleLockOnRoot)
		{
			_reticuleLockOnRoot.gameObject.SetActive(value);
		}
	}

	public void EnableMotionLines(bool value)
	{
		if ((bool)_lineX)
		{
			_lineX.gameObject.SetActive(value);
		}
		if ((bool)_lineY)
		{
			_lineY.gameObject.SetActive(value);
		}
	}

	public void SetMotionLines(Vector2 relativeVel)
	{
		relativeVel *= -1f;
		if ((bool)_lineX)
		{
			_lineX.gameObject.SetActive(value: true);
			_lineX.Points[1] = ((relativeVel.x > 0f) ? new Vector2(45f, 0f) : new Vector2(-45f, 0f));
			_lineX.Points[2] = _lineX.Points[1] + new Vector2(relativeVel.x, 0f);
			_lineX.uvRect = new Rect(0f, 0f, Mathf.Abs(relativeVel.x) / 60f, 1f);
			_lineX.SetVerticesDirty();
			_lineX.transform.GetChild(0).localPosition = _lineX.Points[2];
			_lineX.transform.GetChild(0).localEulerAngles = new Vector3((relativeVel.x > 0f) ? 180f : 0f, -90f, 90f);
		}
		if ((bool)_lineY)
		{
			_lineY.gameObject.SetActive(value: true);
			_lineY.Points[1] = ((relativeVel.y > 0f) ? new Vector2(0f, 45f) : new Vector2(0f, -45f));
			_lineY.Points[2] = _lineY.Points[1] + new Vector2(0f, relativeVel.y);
			_lineY.uvRect = new Rect(0f, 0f, Mathf.Abs(relativeVel.y) / 60f, 1f);
			_lineY.SetVerticesDirty();
			_lineY.transform.GetChild(0).localPosition = _lineY.Points[2];
			_lineY.transform.GetChild(0).localEulerAngles = new Vector3((relativeVel.y > 0f) ? (-90f) : 90f, -90f, 90f);
		}
	}

	public void HideMotionLines()
	{
		if ((bool)_lineX)
		{
			_lineX.gameObject.SetActive(value: false);
		}
		if ((bool)_lineY)
		{
			_lineY.gameObject.SetActive(value: false);
		}
	}

	public void SetArrowState(ArrowState arrowState)
	{
		if (_lockAnimationActive || _unlockAnimationActive)
		{
			arrowState = ArrowState.OFF;
		}
		Vector3 localEulerAngles = Vector3.zero;
		bool active = false;
		switch (arrowState)
		{
		case ArrowState.OFF:
			active = false;
			break;
		case ArrowState.OUT:
			localEulerAngles = new Vector3(0f, 180f, 0f);
			active = true;
			break;
		case ArrowState.IN:
			active = true;
			break;
		}
		_reticuleArrowRoot.SetActive(active);
		for (int i = 0; i < _reticuleArrowPivots.Length; i++)
		{
			_reticuleArrowPivots[i].localEulerAngles = localEulerAngles;
		}
	}

	public void SetAlpha(float a)
	{
		if (_materialInst != null)
		{
			_materialInst.SetAlpha(a);
		}
	}

	public void EnableTextReadout(bool value)
	{
		_readout.gameObject.SetActive(value);
	}

	public void SetReadoutText(string s)
	{
		_readout.text = s;
	}

	public void SetColor(Color color)
	{
		if (_materialInst != null)
		{
			_materialInst.color = color;
		}
		_readout.color = new Color(color.r, color.g, color.b, 1f);
	}

	public void SetColorWithoutAlpha(Color color)
	{
		if (_materialInst != null)
		{
			_materialInst.SetColorIndependentOfAlpha(color);
		}
		_readout.color = new Color(color.r, color.g, color.b, 1f);
	}

	public void ResetColor()
	{
		SetColor(_resetColor);
	}

	public void SetResetColor(Color c)
	{
		_resetColor = c;
	}
}
