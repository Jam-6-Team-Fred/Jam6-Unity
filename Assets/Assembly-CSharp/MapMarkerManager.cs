using System.Collections.Generic;
using UnityEngine;

public class MapMarkerManager : MonoBehaviour
{
	[SerializeField]
	private GameObject _canvasMarkerTemplate;

	[SerializeField]
	private GameObject _locatorMarkerTemplate;

	[SerializeField]
	private Camera _mapCamera;

	[SerializeField]
	private FontAndLanguageController _markerFontController;

	private List<CanvasMapMarker> _activeMarkers;

	private Canvas _canvas;

	private void Awake()
	{
		_canvas = base.gameObject.GetRequiredComponent<Canvas>();
		_activeMarkers = new List<CanvasMapMarker>(32);
		_canvas.renderMode = RenderMode.ScreenSpaceCamera;
		_canvas.worldCamera = _mapCamera;
		_canvasMarkerTemplate.SetActive(value: false);
		_locatorMarkerTemplate.SetActive(value: false);
		_canvas.enabled = false;
	}

	public void SetVisible(bool value)
	{
		_canvas.enabled = value;
		if (value)
		{
			for (int i = 0; i < _activeMarkers.Count; i++)
			{
				_activeMarkers[i].NotifyRefreshOnMapOpen();
			}
		}
	}

	public void RegisterMarker(CanvasMapMarker marker)
	{
		marker.Init(_canvas);
		_markerFontController.AddTextElement(marker.GetTextElement(), rescale: false);
		_activeMarkers.Add(marker);
	}

	public void RegisterMarker(CanvasMapMarker marker, OWRigidbody rigidbodyTarget)
	{
		marker.Init(_canvas, rigidbodyTarget, marker.GetMarkerLabelName());
		_markerFontController.AddTextElement(marker.GetTextElement(), rescale: false);
		_activeMarkers.Add(marker);
	}

	public void RegisterMarker(CanvasMapMarker marker, Transform markerTarget)
	{
		marker.Init(_canvas, markerTarget, marker.GetMarkerLabelName());
		_markerFontController.AddTextElement(marker.GetTextElement(), rescale: false);
		_activeMarkers.Add(marker);
	}

	public void RegisterMarker(CanvasMapMarker marker, OWRigidbody rigidbodyTarget, UITextType label)
	{
		marker.Init(_canvas, rigidbodyTarget, UITextLibrary.GetString(label));
		_markerFontController.AddTextElement(marker.GetTextElement(), rescale: false);
		_activeMarkers.Add(marker);
	}

	public void RegisterMarker(CanvasMapMarker marker, Transform markerTarget, UITextType label)
	{
		marker.Init(_canvas, markerTarget, UITextLibrary.GetString(label));
		_markerFontController.AddTextElement(marker.GetTextElement(), rescale: false);
		_activeMarkers.Add(marker);
	}

	public void UnregisterMarker(CanvasMapMarker marker)
	{
		_markerFontController.RemoveTextElement(marker.GetTextElement());
		_activeMarkers.Remove(marker);
		Object.Destroy(marker.gameObject);
	}

	public CanvasMapMarker InstantiateNewMarker(bool hasPointer)
	{
		GameObject obj = ((!hasPointer) ? Object.Instantiate(_canvasMarkerTemplate, _canvas.transform) : Object.Instantiate(_locatorMarkerTemplate, _canvas.transform));
		return obj.GetRequiredComponent<CanvasMapMarker>();
	}
}
