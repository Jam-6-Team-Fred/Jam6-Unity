using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SkyboxRenderer : MonoBehaviour
{
	private static List<SkyboxRenderer> s_active = new List<SkyboxRenderer>(32);

	private Renderer _renderer;

	[SerializeField]
	private bool _useLocalMaterial;

	public static List<SkyboxRenderer> activeSkyboxRenderers => s_active;

	public Renderer renderer => _renderer;

	public Material material
	{
		get
		{
			if (!_useLocalMaterial)
			{
				return _renderer.sharedMaterial;
			}
			return _renderer.material;
		}
	}

	public bool shouldRender
	{
		get
		{
			if (_renderer.enabled)
			{
				return material != null;
			}
			return false;
		}
	}

	private void Awake()
	{
		_renderer = GetComponent<Renderer>();
	}

	private void OnEnable()
	{
		s_active.Add(this);
	}

	private void OnDisable()
	{
		s_active.QuickRemove(this);
	}
}
