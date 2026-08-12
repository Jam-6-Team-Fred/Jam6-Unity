using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CanvasMarker : MonoBehaviour
{
	public enum SecondaryLabelType
	{
		NONE = 0,
		DANGER = 1,
		DUPLICATE = 2
	}

	public delegate void MarkerDestroyedEvent(CanvasMarker marker);

	public delegate void MarkerResetPositionEvent(CanvasMarker marker);

	public delegate void MarkerVisiblityChangedEvent(bool value);

	public delegate SecondaryLabelType MarkerSecondaryLabelUpdateEvent(SecondaryLabelType labelTypeToSet);

	public delegate void MarkerSecondaryLabelChangedEvent();

	[SerializeField]
	protected Text _mainTextField;

	[SerializeField]
	protected RectTransform _attachPoint;

	[SerializeField]
	protected RectTransform _secondaryLabelRectTransform;

	[SerializeField]
	protected GameObject _dangerIndicatorRootObj;

	[SerializeField]
	protected GameObject _markerWarningImageObj;

	[SerializeField]
	protected Text _secondaryTextField;

	[SerializeField]
	protected RectTransform _dangerIndicatorAttachPoint;

	[SerializeField]
	protected RectTransform _onScreenMarkerRoot;

	[SerializeField]
	protected MeshRenderer _marker;

	[SerializeField]
	protected OffScreenIndicator _offScreenIndicator;

	[Space(10f)]
	[SerializeField]
	protected Transform _arrowScaleRoot;

	[SerializeField]
	protected Transform _offScreenArrowScaleRoot;

	[SerializeField]
	protected float _arrowScaleNormal = 10000f;

	[SerializeField]
	protected float _arrowScaleLarge = 15000f;

	[SerializeField]
	protected RectTransform _fullTextBlockRect;

	[SerializeField]
	protected int _textFontSizeNormal = 16;

	[SerializeField]
	protected int _textFontSizeLarge = 32;

	protected Canvas _canvas;

	protected Transform _visualTarget;

	protected OWRigidbody _rigidbodyTarget;

	protected bool _visible;

	protected bool _onScreen;

	protected bool _fogVisibility = true;

	protected float _warpDistance;

	protected string _label = string.Empty;

	protected float _markerOffsetRadius;

	protected SecondaryLabelType _secondaryLabelType;

	protected FogWarpDetector _targetWarpDetector;

	protected int _fogMarkerCount;

	protected CanvasMarker _prevMarker;

	protected CanvasMarker _nextMarker;

	protected StringBuilder _stringBuilder;

	protected OWCamera _playerCamera;

	protected PlayerFogWarpDetector _playerFogWarpDetector;

	private OuterFogWarpVolume _outerFogWarpVolume;

	public event MarkerDestroyedEvent OnMarkerDestroyed;

	public event MarkerResetPositionEvent OnMarkerResetPosition;

	public event MarkerVisiblityChangedEvent OnMarkerChangeVisibility;

	public event MarkerSecondaryLabelUpdateEvent OnMarkerSecondaryLabelUpdate;

	public event MarkerSecondaryLabelChangedEvent OnMarkerSecondaryLabelChangedUpdate;

	protected virtual void Awake()
	{
		_stringBuilder = new StringBuilder();
	}

	public void Init(Canvas canvas, OWRigidbody rigidbodyTarget, string markerLabel, float markerOffsetRadius = 0f)
	{
		_rigidbodyTarget = rigidbodyTarget;
		rigidbodyTarget.OnDestroyOWRigidbody += OnDestroyOWRigidbody;
		Init(canvas, rigidbodyTarget.transform, markerLabel, markerOffsetRadius);
	}

	public void Init(Canvas canvas, Transform markerTarget, string markerLabel, float markerOffsetRadius = 0f)
	{
		_playerFogWarpDetector = Locator.GetPlayerDetector().GetRequiredComponent<PlayerFogWarpDetector>();
		if (_playerFogWarpDetector.GetOuterFogWarpVolume() == GetOuterFogWarpVolume())
		{
			_fogVisibility = true;
		}
		else
		{
			_fogVisibility = false;
		}
		_label = markerLabel;
		_visualTarget = markerTarget;
		_markerOffsetRadius = markerOffsetRadius;
		Init(canvas);
	}

	public void Init(Canvas canvas)
	{
		_canvas = canvas;
		base.transform.SetParent(_canvas.transform);
		_offScreenIndicator.SetCanvas(_canvas);
		_playerCamera = Locator.GetPlayerCamera();
		base.transform.localPosition = Vector3.zero;
		base.transform.localScale = Vector3.one;
		base.transform.localRotation = Quaternion.identity;
		RectTransform requiredComponent = this.GetRequiredComponent<RectTransform>();
		requiredComponent.anchorMin = Vector2.zero;
		requiredComponent.anchorMax = Vector2.one;
		requiredComponent.offsetMin = Vector2.zero;
		requiredComponent.offsetMax = Vector2.zero;
		GlobalMessenger.AddListener("ChangeGUIMode", OnChangeGUIMode);
		SetSecondaryLabel(_secondaryLabelType);
		base.gameObject.SetActive(value: true);
	}

	public void ResetArrowAnchoredPosition()
	{
		_onScreenMarkerRoot.anchoredPosition = Vector2.zero;
	}

	public Vector2 GetArrowAnchoredPosition()
	{
		return _onScreenMarkerRoot.anchoredPosition;
	}

	public virtual bool HasDuplicateMarkers()
	{
		bool result = false;
		if (GetFogMarkerCount() > 1)
		{
			result = true;
		}
		return result;
	}

	public void SetSecondaryLabel(SecondaryLabelType labelType)
	{
		if (_dangerIndicatorRootObj != null)
		{
			if (labelType == SecondaryLabelType.NONE)
			{
				if (_dangerIndicatorRootObj.activeSelf)
				{
					_dangerIndicatorRootObj.SetActive(value: false);
				}
			}
			else if (!_dangerIndicatorRootObj.activeSelf)
			{
				_dangerIndicatorRootObj.SetActive(value: true);
			}
			switch (labelType)
			{
			case SecondaryLabelType.DANGER:
				_markerWarningImageObj.SetActive(value: true);
				_secondaryTextField.text = UITextLibrary.GetString(UITextType.ProbeDangerUI);
				break;
			case SecondaryLabelType.DUPLICATE:
				_markerWarningImageObj.SetActive(value: false);
				_secondaryTextField.text = UITextLibrary.GetString(UITextType.DuplicateSignalUI);
				break;
			}
			_secondaryTextField.SetAllDirty();
		}
		if (_secondaryLabelType != labelType)
		{
			_secondaryLabelType = labelType;
			if (this.OnMarkerSecondaryLabelChangedUpdate != null)
			{
				this.OnMarkerSecondaryLabelChangedUpdate();
			}
		}
	}

	public SecondaryLabelType GetSecondaryLabelType()
	{
		return _secondaryLabelType;
	}

	public bool IsSecondaryLabelEnabled()
	{
		if (_dangerIndicatorRootObj != null)
		{
			return _dangerIndicatorRootObj.activeSelf;
		}
		return false;
	}

	public void DestroyMarker()
	{
		if (_nextMarker != null)
		{
			_nextMarker.SetPreviousMarker(_prevMarker);
			if (_prevMarker != null)
			{
				_prevMarker.SetNextMarker(_nextMarker);
			}
		}
		if (_prevMarker != null)
		{
			_prevMarker.SetNextMarker(_nextMarker);
			if (_nextMarker != null)
			{
				_nextMarker.SetPreviousMarker(_prevMarker);
			}
		}
		if (Locator.GetMarkerManager() != null)
		{
			Locator.GetMarkerManager().UnregisterMarker(this);
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("ChangeGUIMode", OnChangeGUIMode);
		if (_targetWarpDetector != null)
		{
			_targetWarpDetector.OnTrackFogWarpVolume -= OnTrackFogWarpVolume;
			_targetWarpDetector.OnUntrackFogWarpVolume -= OnUntrackFogWarpVolume;
		}
		if (_rigidbodyTarget != null)
		{
			_rigidbodyTarget.OnDestroyOWRigidbody -= OnDestroyOWRigidbody;
		}
		if (this.OnMarkerDestroyed != null)
		{
			this.OnMarkerDestroyed(this);
		}
	}

	public string GetMarkerLabelName()
	{
		return _label;
	}

	public Transform GetMarkerTarget()
	{
		return _visualTarget;
	}

	public void SetMarkerTarget(Transform target)
	{
		_visualTarget = target;
	}

	public void SetOuterFogWarpVolume(OuterFogWarpVolume outerFogWarpVolume)
	{
		_outerFogWarpVolume = outerFogWarpVolume;
	}

	public void SetFogDetector(FogWarpDetector detector)
	{
		if (detector != _targetWarpDetector && _targetWarpDetector != null)
		{
			_targetWarpDetector.OnTrackFogWarpVolume -= OnTrackFogWarpVolume;
			_targetWarpDetector.OnUntrackFogWarpVolume -= OnUntrackFogWarpVolume;
		}
		_targetWarpDetector = detector;
		if (detector != null)
		{
			_targetWarpDetector.OnTrackFogWarpVolume += OnTrackFogWarpVolume;
			_targetWarpDetector.OnUntrackFogWarpVolume += OnUntrackFogWarpVolume;
		}
	}

	public FogWarpDetector GetFogDetector()
	{
		return _targetWarpDetector;
	}

	public void SetLabel(string label)
	{
		_label = label;
	}

	public void SetWarpDistance(float distance)
	{
		_warpDistance = distance;
	}

	public float GetWarpDistance()
	{
		return _warpDistance;
	}

	protected int GetFogMarkerCount()
	{
		if (!IsVisible())
		{
			return _fogMarkerCount;
		}
		return _fogMarkerCount + 1;
	}

	public int GetRawFogMarkerCount()
	{
		return _fogMarkerCount;
	}

	public void SetFogVisibility(bool value)
	{
		if (_fogVisibility != value)
		{
			_fogVisibility = value;
		}
	}

	public virtual void SetVisibility(bool value)
	{
		if (_visible != value)
		{
			_visible = value;
			if (this.OnMarkerChangeVisibility != null)
			{
				this.OnMarkerChangeVisibility(_visible);
			}
		}
	}

	public virtual bool IsUndefinedDistance()
	{
		return false;
	}

	public bool IsVisible()
	{
		if (_fogVisibility)
		{
			return _visible;
		}
		return false;
	}

	public bool IsVisibleIgnoreFog()
	{
		return _visible;
	}

	public void OnDestroyOWRigidbody(OWRigidbody destroyedBody)
	{
		DestroyMarker();
	}

	public bool IsOnScreen()
	{
		return _onScreen;
	}

	protected virtual void Update()
	{
		if (_visualTarget == null)
		{
			return;
		}
		if (!UpdateIsVisible())
		{
			if (_onScreenMarkerRoot.gameObject.activeSelf)
			{
				_onScreenMarkerRoot.gameObject.SetActive(value: false);
			}
			if (_offScreenIndicator.gameObject.activeSelf)
			{
				_offScreenIndicator.gameObject.SetActive(value: false);
			}
			return;
		}
		if (_nextMarker != null && _nextMarker.GetArrowAnchoredPosition() != Vector2.zero)
		{
			_nextMarker.ResetArrowAnchoredPosition();
		}
		bool flag = false;
		Vector3 position = _visualTarget.position;
		Vector2 onScreenPos = new Vector2(0f, 0f);
		flag = IsOnScreen(position, ref onScreenPos);
		if (flag)
		{
			_onScreenMarkerRoot.anchoredPosition = onScreenPos;
			if (!_onScreenMarkerRoot.gameObject.activeSelf)
			{
				_onScreenMarkerRoot.gameObject.SetActive(value: true);
			}
			if (_offScreenIndicator.gameObject.activeSelf)
			{
				_offScreenIndicator.gameObject.SetActive(value: false);
			}
		}
		else
		{
			_offScreenIndicator.SetCanvasPosition(position);
			if (_onScreenMarkerRoot.gameObject.activeSelf)
			{
				_onScreenMarkerRoot.gameObject.SetActive(value: false);
			}
			if (!_offScreenIndicator.gameObject.activeSelf)
			{
				_offScreenIndicator.gameObject.SetActive(value: true);
			}
		}
		_onScreen = flag;
		UpdateDistanceText();
		UpdateSecondaryLabel();
	}

	private bool UpdateIsVisible()
	{
		DetermineFogVisibilityToPlayer();
		if (_visible && _fogVisibility)
		{
			return GUIMode.AreHUDMarkersVisible();
		}
		return false;
	}

	private bool IsOnScreen(Vector3 targetWorldPos, ref Vector2 onScreenPos)
	{
		onScreenPos.x = 0f;
		onScreenPos.y = 0f;
		bool result = false;
		if (_prevMarker == null)
		{
			Camera camera = _canvas.worldCamera;
			if (camera == null)
			{
				camera = Locator.GetActiveCamera().mainCamera;
			}
			Vector3 vector = _canvas.WorldToCanvasPosition(camera, targetWorldPos);
			Rect rect = _canvas.pixelRect;
			if (_canvas.GetComponent<CanvasScaler>() != null)
			{
				rect = _canvas.GetComponent<RectTransform>().rect;
			}
			float a = GetMarkerTargetScreenSize(rect.height) * 0.5f;
			a = Mathf.Min(a, rect.height - GetTotalMarkerHeight() - vector.y);
			a = Mathf.Max(0f, a);
			vector.y += a;
			onScreenPos.x = vector.x * rect.width / rect.width;
			onScreenPos.y = vector.y * rect.height / rect.height;
			if (vector.x >= 0f && vector.x <= rect.width && vector.y >= 0f && vector.y <= rect.height - GetTotalMarkerHeight() && vector.z > 0f)
			{
				result = true;
			}
		}
		else
		{
			result = _prevMarker.IsOnScreen() && IsVisible();
		}
		return result;
	}

	protected virtual void UpdateSecondaryLabel()
	{
		SecondaryLabelType secondaryLabelType = _secondaryLabelType;
		if (HasDuplicateMarkers() && secondaryLabelType != SecondaryLabelType.DUPLICATE)
		{
			secondaryLabelType = SecondaryLabelType.DUPLICATE;
		}
		else if (!HasDuplicateMarkers() && secondaryLabelType == SecondaryLabelType.DUPLICATE)
		{
			secondaryLabelType = SecondaryLabelType.NONE;
		}
		if (this.OnMarkerSecondaryLabelUpdate != null)
		{
			secondaryLabelType = this.OnMarkerSecondaryLabelUpdate(secondaryLabelType);
		}
		if (secondaryLabelType != _secondaryLabelType)
		{
			SetSecondaryLabel(secondaryLabelType);
		}
	}

	protected virtual void UpdateDistanceText()
	{
		if (_stringBuilder != null)
		{
			_stringBuilder.Length = 0;
			_stringBuilder.Append(_label);
			_stringBuilder.Append(" ");
			float markerDistance = GetMarkerDistance();
			if (markerDistance < 1000f)
			{
				_stringBuilder.Append(Mathf.Round(markerDistance));
				_stringBuilder.Append("m");
			}
			else if (markerDistance < 99999f)
			{
				_stringBuilder.Append(Mathf.Round(markerDistance / 100f) / 10f);
				_stringBuilder.Append("km");
			}
			else
			{
				_stringBuilder.Append(" ");
				_stringBuilder.Append("ERROR");
			}
			_mainTextField.text = _stringBuilder.ToString();
			bool flag = false;
			if (_nextMarker != null && _nextMarker.IsVisible())
			{
				flag = true;
			}
			if (flag)
			{
				_offScreenIndicator.SetText(UITextLibrary.GetString(UITextType.MultipleSignal));
			}
			else
			{
				_offScreenIndicator.SetText(_stringBuilder.ToString());
			}
		}
	}

	protected virtual float GetMarkerDistance()
	{
		return Vector3.Distance(_playerCamera.transform.position, _visualTarget.position);
	}

	private void OnTrackFogWarpVolume(FogWarpVolume warpVolume)
	{
		if (warpVolume.IsOuterWarpVolume())
		{
			warpVolume.PropagateCanvasMarkerOutwards(this, IsVisibleIgnoreFog());
			DetermineFogVisibilityToPlayer();
		}
		Locator.GetMarkerManager().RequestFogMarkerUpdate();
	}

	private void OnUntrackFogWarpVolume(FogWarpVolume warpVolume)
	{
		if (warpVolume.IsOuterWarpVolume())
		{
			warpVolume.PropagateCanvasMarkerOutwards(this, addMarker: false);
		}
	}

	public virtual OuterFogWarpVolume GetOuterFogWarpVolume()
	{
		if (_outerFogWarpVolume != null)
		{
			return _outerFogWarpVolume;
		}
		if (_targetWarpDetector != null)
		{
			return _targetWarpDetector.GetOuterFogWarpVolume();
		}
		return null;
	}

	public void SetFogMarkerCount(int count)
	{
		_fogMarkerCount = count;
		UpdateSecondaryLabel();
	}

	public void PlayerWarpVolumeUpdated()
	{
		if (_targetWarpDetector != null || _outerFogWarpVolume != null)
		{
			DetermineFogVisibilityToPlayer();
		}
	}

	public void DetermineFogVisibilityToPlayer()
	{
		if (_playerFogWarpDetector == null)
		{
			_playerFogWarpDetector = Locator.GetPlayerDetector().GetRequiredComponent<PlayerFogWarpDetector>();
		}
		if (_playerFogWarpDetector.GetOuterFogWarpVolume() == GetOuterFogWarpVolume())
		{
			SetFogVisibility(value: true);
		}
		else
		{
			SetFogVisibility(value: false);
		}
	}

	public void SetPreviousMarker(CanvasMarker marker)
	{
		_prevMarker = marker;
		if (_prevMarker == null)
		{
			base.transform.SetParent(_canvas.transform);
			RectTransform requiredComponent = this.GetRequiredComponent<RectTransform>();
			requiredComponent.anchorMin = Vector2.zero;
			requiredComponent.anchorMax = Vector2.one;
			requiredComponent.offsetMin = Vector2.zero;
			requiredComponent.offsetMax = Vector2.zero;
		}
		else
		{
			base.transform.SetParent(_prevMarker.GetAttachPoint().transform);
			RectTransform requiredComponent2 = this.GetRequiredComponent<RectTransform>();
			requiredComponent2.anchorMin = Vector2.zero;
			requiredComponent2.anchorMax = Vector2.one;
			requiredComponent2.offsetMin = Vector2.zero;
			requiredComponent2.offsetMax = Vector2.zero;
			requiredComponent2.anchoredPosition = Vector2.zero;
		}
	}

	public void SetNextMarker(CanvasMarker marker)
	{
		_nextMarker = marker;
	}

	public CanvasMarker GetNextMarker()
	{
		return _nextMarker;
	}

	public CanvasMarker GetPreviousMarker()
	{
		return _prevMarker;
	}

	public RectTransform GetAttachPoint()
	{
		if (IsSecondaryLabelEnabled())
		{
			return _dangerIndicatorAttachPoint;
		}
		return _attachPoint;
	}

	public void NotifyResetPosition()
	{
		if (this.OnMarkerResetPosition != null)
		{
			this.OnMarkerResetPosition(this);
		}
	}

	protected virtual void OnChangeGUIMode()
	{
		if (!GUIMode.AreHUDMarkersVisible())
		{
			_onScreenMarkerRoot.gameObject.SetActive(value: false);
			_offScreenIndicator.gameObject.SetActive(value: false);
		}
		else
		{
			SetVisibility(IsVisible());
		}
	}

	protected float GetTotalMarkerHeight()
	{
		float num = _onScreenMarkerRoot.rect.height + _mainTextField.rectTransform.rect.height;
		if (IsSecondaryLabelEnabled())
		{
			num += _secondaryLabelRectTransform.rect.height;
		}
		return num;
	}

	protected float GetMarkerTargetScreenSize(float height)
	{
		float markerDistance = GetMarkerDistance();
		return Mathf.Clamp(_markerOffsetRadius * (1f / Mathf.Tan(_playerCamera.fieldOfView * ((float)Math.PI / 180f) * 0.5f) / markerDistance), 0f, 0.8f) * height;
	}
}
