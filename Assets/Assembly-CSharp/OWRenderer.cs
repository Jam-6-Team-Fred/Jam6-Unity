using System.Collections.Generic;
using UnityEngine;

public class OWRenderer : MonoBehaviour
{
	private static List<Material> s_tempMaterialList = new List<Material>(32);

	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_unityLODFade;

	private static int s_propID_OWDitherFade;

	private static int s_propID_Color;

	private static int s_propID_EmissionColor;

	private static int s_propID_Cutoff;

	private static int s_propID_Fade;

	private bool _initialized;

	private Renderer _renderer;

	private bool _gameplayActive;

	private bool _lodActive = true;

	public Material sharedMaterial
	{
		get
		{
			Initialize();
			return _renderer.sharedMaterial;
		}
		set
		{
			Initialize();
			_renderer.sharedMaterial = value;
		}
	}

	public Material[] sharedMaterials
	{
		get
		{
			Initialize();
			return _renderer.sharedMaterials;
		}
		set
		{
			Initialize();
			_renderer.sharedMaterials = value;
		}
	}

	protected void Initialize()
	{
		if (!_initialized)
		{
			_renderer = this.GetRequiredComponent<Renderer>();
			_gameplayActive = _renderer.enabled;
			if (s_matPropBlock == null)
			{
				s_matPropBlock = new MaterialPropertyBlock();
				s_propID_unityLODFade = Shader.PropertyToID("unity_LODFade");
				s_propID_OWDitherFade = Shader.PropertyToID("_OWDitherFade");
				s_propID_Color = Shader.PropertyToID("_Color");
				s_propID_EmissionColor = Shader.PropertyToID("_EmissionColor");
				s_propID_Cutoff = Shader.PropertyToID("_Cutoff");
				s_propID_Fade = Shader.PropertyToID("_Fade");
			}
			_initialized = true;
		}
	}

	public Renderer GetRenderer()
	{
		Initialize();
		return _renderer;
	}

	public bool IsActive()
	{
		Initialize();
		return _gameplayActive;
	}

	public void SetActivation(bool active)
	{
		Initialize();
		if (_gameplayActive != active)
		{
			_gameplayActive = active;
			if (_lodActive)
			{
				_renderer.enabled = _gameplayActive;
			}
		}
	}

	public void SetLODActivation(bool active)
	{
		Initialize();
		if (_lodActive != active)
		{
			_lodActive = active;
			if (_gameplayActive)
			{
				_renderer.enabled = _lodActive;
			}
		}
	}

	public Color GetOriginalColor()
	{
		Initialize();
		return _renderer.sharedMaterial.GetColor(s_propID_Color);
	}

	public Color GetOriginalColor(int materialIndex)
	{
		Initialize();
		_renderer.GetSharedMaterials(s_tempMaterialList);
		return s_tempMaterialList[materialIndex].GetColor(s_propID_Color);
	}

	public Color GetOriginalEmissionColor()
	{
		Initialize();
		return GetOriginalEmissionColor_Internal(_renderer.sharedMaterial);
	}

	public Color GetOriginalEmissionColor(int materialIndex)
	{
		Initialize();
		_renderer.GetSharedMaterials(s_tempMaterialList);
		return GetOriginalEmissionColor_Internal(s_tempMaterialList[materialIndex]);
	}

	private Color GetOriginalEmissionColor_Internal(Material emissiveMaterial)
	{
		if (!emissiveMaterial.HasProperty(s_propID_EmissionColor))
		{
			return Color.white;
		}
		return emissiveMaterial.GetColor(s_propID_EmissionColor);
	}

	public void SetLODFade(float lodFade)
	{
		Initialize();
		lodFade = Mathf.Clamp01(lodFade);
		float y = Mathf.Floor(lodFade * 16f) / 16f;
		SetMaterialProperty(s_propID_unityLODFade, new Vector4(lodFade, y, 0f, 0f));
	}

	public void SetDitherFade(float ditherFade)
	{
		Initialize();
		ditherFade = Mathf.Clamp01(ditherFade);
		SetMaterialProperty(s_propID_OWDitherFade, ditherFade);
	}

	public void SetColor(Color value)
	{
		Initialize();
		SetMaterialProperty(s_propID_Color, value);
	}

	public void SetEmissionColor(Color value)
	{
		Initialize();
		SetMaterialProperty(s_propID_EmissionColor, value);
	}

	public void SetCutoff(float value)
	{
		Initialize();
		SetMaterialProperty(s_propID_Cutoff, value);
	}

	public void SetFade(float value)
	{
		Initialize();
		SetMaterialProperty(s_propID_Fade, value);
	}

	public void SetMaterialProperty(int id, float value)
	{
		Initialize();
		_renderer.GetPropertyBlock(s_matPropBlock);
		s_matPropBlock.SetFloat(id, value);
		_renderer.SetPropertyBlock(s_matPropBlock);
	}

	public void SetMaterialProperty(int id, Vector4 value)
	{
		Initialize();
		_renderer.GetPropertyBlock(s_matPropBlock);
		s_matPropBlock.SetVector(id, value);
		_renderer.SetPropertyBlock(s_matPropBlock);
	}

	public void SetMaterialProperty(int id, Color value)
	{
		Initialize();
		_renderer.GetPropertyBlock(s_matPropBlock);
		s_matPropBlock.SetColor(id, value);
		_renderer.SetPropertyBlock(s_matPropBlock);
	}

	public void SetMaterialProperty(int id, Texture value)
	{
		Initialize();
		_renderer.GetPropertyBlock(s_matPropBlock);
		s_matPropBlock.SetTexture(id, value);
		_renderer.SetPropertyBlock(s_matPropBlock);
	}
}
