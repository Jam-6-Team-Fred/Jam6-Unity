using UnityEngine;

public class ShipCanvasGroup : MonoBehaviour, IShipGroup
{
	[SerializeField]
	private ShipLODTrigger _lodTrigger;

	[SerializeField]
	private Canvas[] _canvases = new Canvas[0];

	private bool _firstUpdateComplete;

	private bool _initialized;

	private bool _requestRefreshCanvas;

	private bool _visible = true;

	private bool _inMapView;

	private bool _gameplayActive = true;

	private void Awake()
	{
		if (_lodTrigger != null)
		{
			_lodTrigger.OnTriggerUpdated += new OWEvent.OWCallback(OnTriggerUpdated);
		}
		else
		{
			Debug.LogWarning("ShipCanvasGroup has no specificed ShipLODTrigger!");
		}
		_firstUpdateComplete = false;
		Canvas.willRenderCanvases += OnWillRenderCanvases;
		_initialized = true;
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		ButtonPromptLibrary.OnUpdateButtonPromptConfig += OnButtonImagesChanged;
	}

	private void OnDestroy()
	{
		if (_lodTrigger != null)
		{
			_lodTrigger.OnTriggerUpdated -= new OWEvent.OWCallback(OnTriggerUpdated);
		}
		if (_initialized)
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases;
			GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
			GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
			ButtonPromptLibrary.OnUpdateButtonPromptConfig -= OnButtonImagesChanged;
		}
	}

	private void FlagForRefreshCanvas()
	{
		if (ShouldBeVisible())
		{
			_requestRefreshCanvas = true;
			_firstUpdateComplete = false;
		}
	}

	private void OnButtonImagesChanged()
	{
		FlagForRefreshCanvas();
	}

	private void OnTriggerUpdated()
	{
		FlagForRefreshCanvas();
	}

	private void OnEnterMapView()
	{
		_inMapView = true;
		FlagForRefreshCanvas();
	}

	private void OnExitMapView()
	{
		_inMapView = false;
		FlagForRefreshCanvas();
	}

	public bool IsGameplayActive()
	{
		return _gameplayActive;
	}

	public void SetGameplayActive(bool active)
	{
		_gameplayActive = active;
		FlagForRefreshCanvas();
	}

	private bool ShouldBeVisible()
	{
		if (!_gameplayActive || _inMapView)
		{
			return false;
		}
		if (_lodTrigger != null)
		{
			if (!_lodTrigger.isPlayerInTrigger)
			{
				return _lodTrigger.isProbeInTrigger;
			}
			return true;
		}
		return true;
	}

	private void SetVisible(bool visible)
	{
		_visible = visible;
		for (int i = 0; i < _canvases.Length; i++)
		{
			if (_canvases[i] != null)
			{
				_canvases[i].enabled = _visible;
			}
		}
	}

	private void RefreshCanvases()
	{
		bool flag = ShouldBeVisible();
		if (_visible != flag)
		{
			SetVisible(flag);
		}
	}

	private void OnWillRenderCanvases()
	{
		if (_requestRefreshCanvas)
		{
			if (_firstUpdateComplete)
			{
				RefreshCanvases();
				_requestRefreshCanvas = false;
			}
			else
			{
				_firstUpdateComplete = true;
			}
		}
	}

	public ShipLODTrigger GetLODTrigger()
	{
		return _lodTrigger;
	}

	public void SetLODTrigger(ShipLODTrigger lodTrigger)
	{
		if (_lodTrigger != null)
		{
			_lodTrigger.OnTriggerUpdated -= new OWEvent.OWCallback(OnTriggerUpdated);
		}
		_lodTrigger = lodTrigger;
		if (_lodTrigger != null)
		{
			_lodTrigger.OnTriggerUpdated += new OWEvent.OWCallback(OnTriggerUpdated);
		}
		OnTriggerUpdated();
	}
}
