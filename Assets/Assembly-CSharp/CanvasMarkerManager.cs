using System.Collections.Generic;
using UnityEngine;

public class CanvasMarkerManager : MonoBehaviour
{
	[SerializeField]
	private GameObject _canvasMarkerN1;

	[SerializeField]
	private GameObject _canvasFogMarker;

	[SerializeField]
	private FontAndLanguageController _markerFontController;

	private List<CanvasMarker> _activeMarkers;

	private Canvas _canvas;

	protected PlayerFogWarpDetector _playerWarpDetector;

	protected FogWarpDetector _probeWarpDetector;

	protected FogWarpDetector _shipWarpDetector;

	private List<CanvasMarker> _nonFogMarkers;

	private bool _requestUpdateAllFogMarkers;

	private void Awake()
	{
		_nonFogMarkers = new List<CanvasMarker>(32);
		_canvas = base.gameObject.GetRequiredComponent<Canvas>();
		_activeMarkers = new List<CanvasMarker>(64);
	}

	private void Start()
	{
		_playerWarpDetector = Locator.GetPlayerDetector().GetRequiredComponent<PlayerFogWarpDetector>();
		_playerWarpDetector.OnOuterFogWarpVolumeChange += OnUpdateFogVisibilities;
		_playerWarpDetector.OnFogWarpDetectorDestroyed += OnPlayerDetectorDestroyed;
		_probeWarpDetector = Locator.GetProbe().GetRequiredComponentInChildren<FogWarpDetector>();
		_probeWarpDetector.OnOuterFogWarpVolumeChange += OnUpdateFogVisibilities;
		_probeWarpDetector.OnFogWarpDetectorDestroyed += OnProbeDetectorDestroyed;
		if (Locator.GetShipDetector() != null)
		{
			_shipWarpDetector = Locator.GetShipDetector().GetRequiredComponent<FogWarpDetector>();
			_shipWarpDetector.OnOuterFogWarpVolumeChange += OnUpdateFogVisibilities;
			_shipWarpDetector.OnFogWarpDetectorDestroyed += OnShipDetectorDestroyed;
		}
		_canvas.renderMode = RenderMode.ScreenSpaceCamera;
		_canvas.worldCamera = Locator.GetActiveCamera().mainCamera;
		_canvas.planeDistance = 0.2f;
		_canvasMarkerN1.SetActive(value: false);
		_canvasFogMarker.SetActive(value: false);
	}

	private void Update()
	{
		if (_requestUpdateAllFogMarkers)
		{
			UpdateAllFogMarkers();
			_requestUpdateAllFogMarkers = false;
		}
	}

	public void RegisterMarker(CanvasMarker marker)
	{
		marker.Init(_canvas);
		if (marker as CanvasFogMarker == null)
		{
			_nonFogMarkers.Add(marker);
			marker.OnMarkerChangeVisibility += OnSourceMarkerChangeVisibility;
		}
		_activeMarkers.Add(marker);
	}

	public void RegisterMarker(CanvasMarker marker, OWRigidbody rigidbodyTarget, string label, float screenSizeRadius = 0f)
	{
		marker.Init(_canvas, rigidbodyTarget, label, screenSizeRadius);
		if (marker as CanvasFogMarker == null)
		{
			_nonFogMarkers.Add(marker);
			marker.OnMarkerChangeVisibility += OnSourceMarkerChangeVisibility;
		}
		_activeMarkers.Add(marker);
	}

	public void RegisterMarker(CanvasMarker marker, Transform transformTarget, string label, float screenSizeRadius = 0f)
	{
		marker.Init(_canvas, transformTarget, label, screenSizeRadius);
		if (marker as CanvasFogMarker == null)
		{
			_nonFogMarkers.Add(marker);
			marker.OnMarkerChangeVisibility += OnSourceMarkerChangeVisibility;
		}
		_activeMarkers.Add(marker);
	}

	public void RegisterMarker(CanvasFogMarker fogMarker, InnerFogWarpVolume volumeTarget)
	{
		fogMarker.Init(_canvas, volumeTarget);
		_activeMarkers.Add(fogMarker);
	}

	public void UnregisterMarker(CanvasMarker marker)
	{
		_activeMarkers.Remove(marker);
		if (_nonFogMarkers.Contains(marker))
		{
			marker.OnMarkerChangeVisibility -= OnSourceMarkerChangeVisibility;
			_nonFogMarkers.Remove(marker);
		}
		Object.Destroy(marker.gameObject);
	}

	public CanvasMarker InstantiateNewMarker()
	{
		return Object.Instantiate(_canvasMarkerN1).GetRequiredComponent<CanvasMarker>();
	}

	public CanvasFogMarker InstantiateNewFogMarker()
	{
		return Object.Instantiate(_canvasFogMarker).GetRequiredComponent<CanvasFogMarker>();
	}

	private void OnSourceMarkerChangeVisibility(bool visibility)
	{
		RequestFogMarkerUpdate();
	}

	private void OnUpdateFogVisibilities(OuterFogWarpVolume warpVolume)
	{
		RequestFogMarkerUpdate();
	}

	public void RequestFogMarkerUpdate()
	{
		_requestUpdateAllFogMarkers = true;
	}

	private void UpdateAllFogMarkers()
	{
		for (int i = 0; i < _nonFogMarkers.Count; i++)
		{
			OuterFogWarpVolume outerFogWarpVolume = _nonFogMarkers[i].GetOuterFogWarpVolume();
			if (outerFogWarpVolume != null)
			{
				outerFogWarpVolume.PropagateCanvasMarkerOutwards(_nonFogMarkers[i], _nonFogMarkers[i].IsVisibleIgnoreFog());
			}
		}
		for (int j = 0; j < _activeMarkers.Count; j++)
		{
			_activeMarkers[j].PlayerWarpVolumeUpdated();
		}
		UpdateFogMarkerCounts();
	}

	private void UpdateFogMarkerCounts()
	{
		for (int i = 0; i < _activeMarkers.Count; i++)
		{
			CanvasMarker canvasMarker = _activeMarkers[i];
			CanvasFogMarker canvasFogMarker = canvasMarker as CanvasFogMarker;
			if (canvasFogMarker == null)
			{
				canvasMarker.SetFogMarkerCount(0);
			}
		}
		for (int j = 0; j < _activeMarkers.Count; j++)
		{
			CanvasFogMarker canvasFogMarker = _activeMarkers[j] as CanvasFogMarker;
			if (canvasFogMarker != null)
			{
				CanvasMarker canvasMarker = canvasFogMarker.GetSourceMarker();
				if (canvasMarker != null && canvasFogMarker.IsVisible())
				{
					canvasMarker.SetFogMarkerCount(canvasMarker.GetRawFogMarkerCount() + 1);
				}
			}
		}
	}

	private void OnShipDetectorDestroyed()
	{
		if (_shipWarpDetector != null)
		{
			_shipWarpDetector.OnOuterFogWarpVolumeChange -= OnUpdateFogVisibilities;
			_shipWarpDetector.OnFogWarpDetectorDestroyed -= OnShipDetectorDestroyed;
		}
	}

	private void OnProbeDetectorDestroyed()
	{
		if (_probeWarpDetector != null)
		{
			_probeWarpDetector.OnOuterFogWarpVolumeChange -= OnUpdateFogVisibilities;
			_probeWarpDetector.OnFogWarpDetectorDestroyed -= OnShipDetectorDestroyed;
		}
	}

	private void OnPlayerDetectorDestroyed()
	{
		if (_playerWarpDetector != null)
		{
			_playerWarpDetector.OnOuterFogWarpVolumeChange -= OnUpdateFogVisibilities;
			_playerWarpDetector.OnFogWarpDetectorDestroyed -= OnShipDetectorDestroyed;
		}
	}
}
