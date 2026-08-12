using System.Collections.Generic;
using UnityEngine;

public class LandingCameraDashboardRenderer : MonoBehaviour
{
	private enum RenderTime
	{
		Early = 0,
		Mid = 1,
		Late = 2
	}

	public static List<LandingCameraDashboardRenderer> s_earlyRenderers = new List<LandingCameraDashboardRenderer>(8);

	public static List<LandingCameraDashboardRenderer> s_midRenderers = new List<LandingCameraDashboardRenderer>(8);

	public static List<LandingCameraDashboardRenderer> s_lateRenderers = new List<LandingCameraDashboardRenderer>(8);

	[SerializeField]
	private RenderTime _renderTime = RenderTime.Mid;

	[SerializeField]
	private Material[] _materials = new Material[1];

	private Renderer _renderer;

	public Renderer renderer => _renderer;

	public Material[] materials => _materials;

	private void Awake()
	{
		_renderer = GetComponent<Renderer>();
		if (_renderTime == RenderTime.Early)
		{
			s_earlyRenderers.Add(this);
		}
		else if (_renderTime == RenderTime.Mid)
		{
			s_midRenderers.Add(this);
		}
		else
		{
			s_lateRenderers.Add(this);
		}
	}

	private void OnDestroy()
	{
		if (_renderTime == RenderTime.Early)
		{
			s_earlyRenderers.Remove(this);
		}
		else if (_renderTime == RenderTime.Mid)
		{
			s_midRenderers.Remove(this);
		}
		else
		{
			s_lateRenderers.Remove(this);
		}
	}
}
