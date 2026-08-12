using UnityEngine;

public class CanvasFogMarker : CanvasMarker
{
	private InnerFogWarpVolume _volumeTarget;

	private CanvasMarker _sourceMarker;

	public void Init(Canvas canvas, InnerFogWarpVolume volumeTarget)
	{
		_volumeTarget = volumeTarget;
		_canvas = canvas;
		_visualTarget = volumeTarget.transform;
		base.transform.SetParent(_canvas.transform);
		_offScreenIndicator.SetCanvas(_canvas);
		base.transform.localPosition = Vector3.zero;
		base.transform.localScale = Vector3.one;
		base.transform.localRotation = Quaternion.identity;
		RectTransform requiredComponent = this.GetRequiredComponent<RectTransform>();
		requiredComponent.anchorMin = Vector2.zero;
		requiredComponent.anchorMax = Vector2.one;
		requiredComponent.offsetMin = Vector2.zero;
		requiredComponent.offsetMax = Vector2.zero;
		SetSecondaryLabel(SecondaryLabelType.NONE);
		_playerCamera = Locator.GetPlayerCamera();
		base.gameObject.SetActive(value: true);
	}

	public void SetSourceMarker(CanvasMarker sourceMarker)
	{
		if (sourceMarker != null && sourceMarker != _sourceMarker)
		{
			sourceMarker.OnMarkerChangeVisibility -= OnSourceMarkerChangeVisibility;
			sourceMarker.OnMarkerDestroyed -= OnSourceMarkerDestroyed;
			sourceMarker.OnMarkerResetPosition -= OnSourceMarkerReset;
		}
		_sourceMarker = sourceMarker;
		_label = sourceMarker.GetMarkerLabelName();
		_targetWarpDetector = sourceMarker.GetFogDetector();
		_sourceMarker.OnMarkerChangeVisibility += OnSourceMarkerChangeVisibility;
		_sourceMarker.OnMarkerDestroyed += OnSourceMarkerDestroyed;
		_sourceMarker.OnMarkerResetPosition += OnSourceMarkerReset;
	}

	protected override float GetMarkerDistance()
	{
		float num = Vector3.Distance(Locator.GetPlayerCamera().transform.position, _volumeTarget.transform.position) - _volumeTarget.GetWarpRadius();
		float warpDistance = _warpDistance;
		float num2 = 0f;
		OuterFogWarpVolume outerFogWarpVolume = null;
		FogWarpDetector fogDetector = _sourceMarker.GetFogDetector();
		outerFogWarpVolume = ((!(fogDetector != null)) ? _sourceMarker.GetOuterFogWarpVolume() : fogDetector.GetOuterFogWarpVolume());
		if (outerFogWarpVolume != null)
		{
			num2 = outerFogWarpVolume.GetWarpRadius() - Vector3.Distance(_sourceMarker.GetMarkerTarget().position, outerFogWarpVolume.transform.position);
		}
		return num + warpDistance + num2;
	}

	public override OuterFogWarpVolume GetOuterFogWarpVolume()
	{
		return _volumeTarget.GetContainerWarpVolume();
	}

	public CanvasMarker GetSourceMarker()
	{
		return _sourceMarker;
	}

	private void OnSourceMarkerChangeVisibility(bool visibility)
	{
		SetVisibility(visibility);
	}

	private void OnSourceMarkerReset(CanvasMarker marker)
	{
		_sourceMarker.OnMarkerChangeVisibility -= OnSourceMarkerChangeVisibility;
		_sourceMarker.OnMarkerDestroyed -= OnSourceMarkerDestroyed;
		_sourceMarker.OnMarkerResetPosition -= OnSourceMarkerReset;
		DestroyMarker();
	}

	private void OnSourceMarkerDestroyed(CanvasMarker marker)
	{
		DestroyMarker();
	}

	public override bool HasDuplicateMarkers()
	{
		return _sourceMarker.HasDuplicateMarkers();
	}
}
