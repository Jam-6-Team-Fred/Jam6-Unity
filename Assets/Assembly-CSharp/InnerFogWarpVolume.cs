using System.Collections.Generic;
using UnityEngine;

public class InnerFogWarpVolume : SphericalFogWarpVolume
{
	private struct CanvasMarkerPair
	{
		public CanvasMarker canvasMarker;

		public CanvasFogMarker canvasFogMarker;

		public CanvasMarkerPair(CanvasMarker marker, CanvasFogMarker fogMarker)
		{
			canvasMarker = marker;
			canvasFogMarker = fogMarker;
		}

		public static int CompareByIsVisible(CanvasMarkerPair x, CanvasMarkerPair y)
		{
			if (!x.canvasFogMarker.IsVisible() && y.canvasFogMarker.IsVisible())
			{
				return -1;
			}
			if (x.canvasFogMarker.IsVisible() && !y.canvasFogMarker.IsVisible())
			{
				return 1;
			}
			return 0;
		}
	}

	[SerializeField]
	private OuterFogWarpVolume _containerWarpVolume;

	[SerializeField]
	private OuterFogWarpVolume _linkedOuterWarpVolume;

	[SerializeField]
	private OuterFogWarpVolume.Name _linkedOuterWarpName;

	[Space]
	[SerializeField]
	private OWRenderer _fogSphereRenderer;

	[SerializeField]
	private bool _useFarFogColor;

	[SerializeField]
	[ColorUsage(false, true)]
	private Color _farFogColor = Color.gray;

	private List<CanvasMarkerPair> _canvasMarkers;

	private void OnValidate()
	{
		if (_linkedOuterWarpName != 0 && _linkedOuterWarpVolume != null)
		{
			_linkedOuterWarpVolume = null;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_canvasMarkers = new List<CanvasMarkerPair>(32);
	}

	private void Start()
	{
		if (_linkedOuterWarpVolume == null && _linkedOuterWarpName != 0)
		{
			_linkedOuterWarpVolume = Locator.GetOuterFogWarp(_linkedOuterWarpName);
		}
		if (_linkedOuterWarpVolume == null)
		{
			Debug.LogWarning("Inner fog warp volume not connected to outer fog warp volume.", this);
		}
		else
		{
			_linkedOuterWarpVolume.RegisterSenderWarp(this);
		}
	}

	public OuterFogWarpVolume GetContainerWarpVolume()
	{
		return _containerWarpVolume;
	}

	public override bool IsOuterWarpVolume()
	{
		return false;
	}

	public override float CheckWarpProximity(FogWarpDetector detector)
	{
		float num = base.CheckWarpProximity(detector);
		if (_fogSphereRenderer != null && detector.CompareName(FogWarpDetector.Name.Player) && !IsProbeOnly())
		{
			float t = Mathf.InverseLerp(150f, _warpRadius + 5f, num);
			Color color = GetFogColor();
			if (_useFarFogColor)
			{
				color = Color.Lerp(_farFogColor, color, t);
			}
			else
			{
				float num2 = Mathf.Lerp(2f, 1f, t);
				color *= num2;
			}
			_fogSphereRenderer.SetColor(color);
		}
		return num;
	}

	public override void PropagateCanvasMarkerOutwards(CanvasMarker marker, bool addMarker, float warpDist = 0f)
	{
		CanvasFogMarker canvasFogMarker = null;
		for (int i = 0; i < _canvasMarkers.Count; i++)
		{
			if (_canvasMarkers[i].canvasMarker == marker)
			{
				canvasFogMarker = _canvasMarkers[i].canvasFogMarker;
				break;
			}
		}
		if (canvasFogMarker == null)
		{
			canvasFogMarker = Locator.GetMarkerManager().InstantiateNewFogMarker();
			Locator.GetMarkerManager().RegisterMarker(canvasFogMarker, this);
			_canvasMarkers.Add(new CanvasMarkerPair(marker, canvasFogMarker));
			canvasFogMarker.SetSourceMarker(marker);
			canvasFogMarker.DetermineFogVisibilityToPlayer();
			canvasFogMarker.OnMarkerDestroyed += OnMarkerTargetReset;
			canvasFogMarker.OnMarkerResetPosition += OnMarkerTargetReset;
			canvasFogMarker.OnMarkerChangeVisibility += OnMarkerChangeVisibility;
			canvasFogMarker.OnMarkerSecondaryLabelChangedUpdate += OnMarkerSecondaryLabelChangedUpdate;
		}
		canvasFogMarker.SetVisibility(addMarker);
		if (addMarker)
		{
			if (_containerWarpVolume == _linkedOuterWarpVolume)
			{
				warpDist = float.PositiveInfinity;
			}
			canvasFogMarker.SetWarpDistance(warpDist);
			canvasFogMarker.DetermineFogVisibilityToPlayer();
		}
		else
		{
			canvasFogMarker.SetFogVisibility(value: false);
		}
		RefreshMarkerPositions();
		if (_containerWarpVolume != null && _containerWarpVolume != _linkedOuterWarpVolume)
		{
			float num = _containerWarpVolume.GetWarpRadius() - Vector3.Distance(_containerWarpVolume.transform.position, base.transform.position);
			_containerWarpVolume.PropagateCanvasMarkerOutwards(marker, addMarker, warpDist + num);
		}
	}

	public override FogWarpVolume GetLinkedFogWarpVolume()
	{
		if (_linkedOuterWarpVolume == null && _linkedOuterWarpName != 0)
		{
			_linkedOuterWarpVolume = Locator.GetOuterFogWarp(_linkedOuterWarpName);
		}
		return _linkedOuterWarpVolume;
	}

	private void OnMarkerTargetReset(CanvasMarker fogMarker)
	{
		for (int i = 0; i < _canvasMarkers.Count; i++)
		{
			if (_canvasMarkers[i].canvasFogMarker == fogMarker)
			{
				_canvasMarkers.QuickRemoveAt(i);
				fogMarker.DestroyMarker();
				fogMarker.OnMarkerChangeVisibility -= OnMarkerChangeVisibility;
				fogMarker.OnMarkerDestroyed -= OnMarkerTargetReset;
				fogMarker.OnMarkerResetPosition -= OnMarkerTargetReset;
				fogMarker.OnMarkerSecondaryLabelChangedUpdate -= OnMarkerSecondaryLabelChangedUpdate;
				RefreshMarkerPositions();
				break;
			}
		}
	}

	private void RefreshMarkerPositions()
	{
		_canvasMarkers.Sort(CanvasMarkerPair.CompareByIsVisible);
		int num = _canvasMarkers.Count - 1;
		for (int num2 = num; num2 >= 0; num2--)
		{
			if (num2 == num)
			{
				_canvasMarkers[num2].canvasFogMarker.SetPreviousMarker(null);
			}
			else
			{
				_canvasMarkers[num2].canvasFogMarker.SetPreviousMarker(_canvasMarkers[num2 + 1].canvasFogMarker);
			}
			if (num2 > 0)
			{
				_canvasMarkers[num2].canvasFogMarker.SetNextMarker(_canvasMarkers[num2 - 1].canvasFogMarker);
			}
			else
			{
				_canvasMarkers[num2].canvasFogMarker.SetNextMarker(null);
			}
		}
	}

	private void OnMarkerChangeVisibility(bool visible)
	{
		RefreshMarkerPositions();
	}

	private void OnMarkerSecondaryLabelChangedUpdate()
	{
		RefreshMarkerPositions();
	}
}
