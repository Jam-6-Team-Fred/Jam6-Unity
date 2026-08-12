using UnityEngine;

[RequireComponent(typeof(TractorBeamController))]
public class SafetyTractorBeamController : SectoredMonoBehaviour
{
	[SerializeField]
	private ForceVolume _alignmentForceVolume;

	[SerializeField]
	private Renderer[] _renderers;

	[SerializeField]
	private int _materialIndex;

	[SerializeField]
	private float _fadeDuration = 1f;

	private TractorBeamController _beamController;

	private float _beamFraction;

	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_Color;

	private Color[] _color;

	protected override void Awake()
	{
		base.Awake();
		_color = new Color[_renderers.Length];
		for (int i = 0; i < _renderers.Length; i++)
		{
			if (_renderers[i] == null)
			{
				Debug.LogWarning("SafetyTractorBeam: Emissive renderer is null or missing!", this);
				continue;
			}
			if (s_matPropBlock == null)
			{
				s_matPropBlock = new MaterialPropertyBlock();
				s_propID_Color = Shader.PropertyToID("_Color");
			}
			_color[i] = _renderers[i].sharedMaterials[_materialIndex].GetColor(s_propID_Color);
			_color[i] = _color[i].gamma;
			s_matPropBlock.SetColor(s_propID_Color, _color[i] * 0f);
			_renderers[i].SetPropertyBlock(s_matPropBlock);
		}
		_beamController = GetComponent<TractorBeamController>();
		_beamFraction = 0f;
		base.enabled = false;
	}

	public void SetActivation(bool active)
	{
		_beamController.SetActivation(active);
		_alignmentForceVolume.SetVolumeActivation(active);
		if (_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			base.enabled = true;
			_beamFraction = (active ? 0f : 1f);
		}
		else
		{
			float beamFraction = (active ? 1f : 0f);
			SetBeamFraction(beamFraction);
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].enabled = flag;
		}
		if (!base.enabled && flag)
		{
			float beamFraction = (_beamController.IsActive() ? 1f : 0f);
			SetBeamFraction(beamFraction);
		}
	}

	private void Update()
	{
		float maxDelta = Time.deltaTime / _fadeDuration;
		float num = (_beamController.IsActive() ? 1f : 0f);
		_beamFraction = Mathf.MoveTowards(_beamFraction, num, maxDelta);
		float num2 = ((Random.value > 0.5f) ? 1f : 0f);
		if (OWMath.ApproxEquals(_beamFraction, num))
		{
			_beamFraction = num;
			num2 = 1f;
			base.enabled = false;
		}
		SetBeamFraction(_beamFraction * num2);
	}

	private void SetBeamFraction(float fraction)
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			if (_renderers[i] != null)
			{
				s_matPropBlock.SetColor(s_propID_Color, _color[i] * fraction);
				_renderers[i].SetPropertyBlock(s_matPropBlock);
			}
		}
	}
}
