using UnityEngine;
using UnityEngine.UI;

public class CanvasMapMarker : MonoBehaviour
{
	public delegate void MarkerDestroyedEvent(CanvasMapMarker marker);

	public delegate void MarkerResetPositionEvent(CanvasMapMarker marker);

	public delegate void MarkerVisiblityChangedEvent(bool value);

	public delegate string MarkerTextUpdateEvent(string textToAppendTo);

	[SerializeField]
	protected Text _textField;

	[SerializeField]
	protected RectTransform _onScreenMarkerRoot;

	[SerializeField]
	protected Image _pointerImg;

	protected Canvas _canvas;

	protected Canvas _markerCanvas;

	protected Transform _visualTarget;

	protected OWRigidbody _rigidbodyTarget;

	protected bool _visible;

	protected bool _onScreen;

	protected string _label = string.Empty;

	protected bool _linkedToCanvasRenderEvents;

	protected bool _willRefreshOnMapOpen;

	protected OuterFogWarpVolume _outerFogWarpVolume;

	public event MarkerDestroyedEvent OnMarkerDestroyed;

	public event MarkerResetPositionEvent OnMarkerResetPosition;

	public event MarkerVisiblityChangedEvent OnMarkerChangeVisibility;

	public event MarkerTextUpdateEvent OnMarkerWriteText;

	public void Init(Canvas canvas, OWRigidbody rigidbodyTarget, string markerLabel)
	{
		_rigidbodyTarget = rigidbodyTarget;
		rigidbodyTarget.OnDestroyOWRigidbody += OnDestroyOWRigidbody;
		Init(canvas, rigidbodyTarget.transform, markerLabel);
	}

	public void Init(Canvas canvas, Transform markerTarget, string markerLabel)
	{
		_label = markerLabel;
		_visualTarget = markerTarget;
		Init(canvas);
	}

	public void Init(Canvas canvas)
	{
		_canvas = canvas;
		_markerCanvas = this.GetRequiredComponent<Canvas>();
		_markerCanvas.enabled = false;
		base.transform.SetParent(_canvas.transform);
		base.transform.localPosition = Vector3.zero;
		base.transform.localScale = Vector3.one;
		base.transform.localRotation = Quaternion.identity;
		RectTransform requiredComponent = this.GetRequiredComponent<RectTransform>();
		requiredComponent.anchorMin = Vector2.zero;
		requiredComponent.anchorMax = Vector2.one;
		requiredComponent.offsetMin = Vector2.zero;
		requiredComponent.offsetMax = Vector2.zero;
		GlobalMessenger.AddListener("ChangeGUIMode", OnChangeGUIMode);
		base.gameObject.SetActive(value: true);
	}

	public void ResetArrowAnchoredPosition()
	{
		_onScreenMarkerRoot.anchoredPosition = Vector2.zero;
	}

	private void DestroyMarker()
	{
		if (Locator.GetMapController() != null)
		{
			Locator.GetMapController().GetMarkerManager().UnregisterMarker(this);
		}
		if (_linkedToCanvasRenderEvents)
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases;
			_linkedToCanvasRenderEvents = false;
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("ChangeGUIMode", OnChangeGUIMode);
		if (_rigidbodyTarget != null)
		{
			_rigidbodyTarget.OnDestroyOWRigidbody -= OnDestroyOWRigidbody;
		}
		if (this.OnMarkerDestroyed != null)
		{
			this.OnMarkerDestroyed(this);
		}
		if (_linkedToCanvasRenderEvents)
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases;
			_linkedToCanvasRenderEvents = false;
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

	public void SetLabel(string label)
	{
		_label = label;
	}

	public Text GetTextElement()
	{
		return _textField;
	}

	public virtual void SetVisibility(bool value)
	{
		bool flag = value;
		if (flag && !_linkedToCanvasRenderEvents)
		{
			Canvas.willRenderCanvases += OnWillRenderCanvases;
			_linkedToCanvasRenderEvents = true;
		}
		else if (!flag && _linkedToCanvasRenderEvents)
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases;
			_linkedToCanvasRenderEvents = false;
		}
		if (_visible != value)
		{
			_visible = value;
			_markerCanvas.enabled = flag && !GUIMode.IsHiddenMode();
			if (this.OnMarkerChangeVisibility != null)
			{
				this.OnMarkerChangeVisibility(_visible);
			}
		}
	}

	public bool IsVisible()
	{
		return _visible;
	}

	public void SetOuterFogWarpVolume(OuterFogWarpVolume outerFogWarpVolume)
	{
		_outerFogWarpVolume = outerFogWarpVolume;
	}

	private void OnDestroyOWRigidbody(OWRigidbody destroyedBody)
	{
		Debug.Log("Marker target " + GetMarkerLabelName() + " destroyed");
		DestroyMarker();
	}

	public bool IsOnScreen()
	{
		return _onScreen;
	}

	protected virtual void OnWillRenderCanvases()
	{
		if (_visualTarget == null)
		{
			return;
		}
		bool flag = true;
		if (!PlayerState.InMapView())
		{
			_markerCanvas.enabled = false;
			return;
		}
		if (_willRefreshOnMapOpen)
		{
			ContentSizeFitter component = _textField.GetComponent<ContentSizeFitter>();
			if (component != null)
			{
				component.enabled = false;
				component.enabled = true;
			}
			_willRefreshOnMapOpen = false;
		}
		bool flag2 = false;
		Vector3 targetPosition = GetTargetPosition();
		Camera camera = _canvas.worldCamera;
		if (camera == null)
		{
			camera = Locator.GetActiveCamera().mainCamera;
		}
		Vector3 vector = _canvas.WorldToCanvasPosition(camera, targetPosition);
		_onScreenMarkerRoot.anchoredPosition = new Vector2(vector.x * _canvas.pixelRect.width / _canvas.pixelRect.width, vector.y * _canvas.pixelRect.height / _canvas.pixelRect.height);
		if (vector.x >= 0f && vector.x <= _canvas.pixelRect.width && vector.y >= 0f && vector.y <= _canvas.pixelRect.height && vector.z > 0f)
		{
			flag2 = true;
		}
		if (flag2)
		{
			if (!GUIMode.IsHiddenMode())
			{
				flag = true;
			}
		}
		else
		{
			flag = false;
		}
		_markerCanvas.enabled = flag;
		_onScreen = flag2;
		UpdateMarkerText();
	}

	private Vector3 GetTargetPosition()
	{
		if (_outerFogWarpVolume != null)
		{
			return Locator.GetAstroObject(AstroObject.Name.DarkBramble).GetOWRigidbody().GetPosition();
		}
		return _visualTarget.position;
	}

	private void UpdateMarkerText()
	{
		string label = _label;
		label = AppendAdditionalText(label);
		_textField.text = label;
	}

	public virtual string AppendAdditionalText(string input)
	{
		string text = input;
		if (this.OnMarkerWriteText != null)
		{
			text = this.OnMarkerWriteText(text);
		}
		return text;
	}

	protected virtual float GetMarkerDistance()
	{
		return Vector3.Distance(Locator.GetPlayerCamera().transform.position, GetTargetPosition());
	}

	public void NotifyRefreshOnMapOpen()
	{
		_willRefreshOnMapOpen = true;
	}

	public void NotifyResetPosition()
	{
		if (this.OnMarkerResetPosition != null)
		{
			this.OnMarkerResetPosition(this);
		}
	}

	public void SetColor(Color color)
	{
		if ((bool)_pointerImg)
		{
			_pointerImg.color = color;
		}
	}

	protected virtual void OnChangeGUIMode()
	{
		if (GUIMode.IsHiddenMode())
		{
			_markerCanvas.enabled = false;
		}
		else
		{
			SetVisibility(IsVisible());
		}
	}
}
