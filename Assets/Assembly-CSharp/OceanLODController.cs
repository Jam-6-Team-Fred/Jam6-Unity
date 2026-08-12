using UnityEngine;

public class OceanLODController : SectoredMonoBehaviour
{
	[SerializeField]
	private Light _ambientLight;

	[SerializeField]
	private Texture2D _ambientLightLookup;

	[Space]
	[SerializeField]
	private int _maxLOD_High = 8;

	[SerializeField]
	private int _LODBias_High;

	[SerializeField]
	private int _maxLOD_Medium = 5;

	[SerializeField]
	private int _LODBias_Medium;

	[SerializeField]
	private int _maxLOD_Low = 3;

	[SerializeField]
	private int _LODBias_Low;

	private int _propID_OceanLOD_AmbientLightTex;

	private int _propID_OceanLOD_AmbientLightPosRange;

	private int _propID_OceanLOD_AmbientLightParams;

	private TessellatedRenderer _tessellatedRenderer;

	protected override void Awake()
	{
		base.Awake();
		_propID_OceanLOD_AmbientLightTex = Shader.PropertyToID("_OceanLOD_AmbientLightTex");
		_propID_OceanLOD_AmbientLightPosRange = Shader.PropertyToID("_OceanLOD_AmbientLightPosRange");
		_propID_OceanLOD_AmbientLightParams = Shader.PropertyToID("_OceanLOD_AmbientLightParams");
		_tessellatedRenderer = GetComponent<TessellatedRenderer>();
		OnGraphicSettingsUpdated(PlayerData.GetGraphicSettings());
		GlobalMessenger<GraphicSettings>.AddListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger<GraphicSettings>.RemoveListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (_sector.GetName() == Sector.Name.QuantumMoon)
		{
			base.enabled = _sector.ContainsOccupant(DynamicOccupant.Player);
		}
		else
		{
			base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe) && !PlayerState.OnQuantumMoon();
		}
	}

	private void LateUpdate()
	{
		if (_ambientLight != null)
		{
			Vector3 position = _ambientLight.transform.position;
			float range = _ambientLight.range;
			Color color = _ambientLight.color;
			float x = color.r * color.r;
			float g = color.g;
			float intensity = _ambientLight.intensity;
			float a = color.a;
			Shader.SetGlobalTexture(_propID_OceanLOD_AmbientLightTex, _ambientLightLookup);
			Shader.SetGlobalVector(_propID_OceanLOD_AmbientLightPosRange, new Vector4(position.x, position.y, position.z, range));
			Shader.SetGlobalVector(_propID_OceanLOD_AmbientLightParams, new Vector4(x, g, intensity, a));
		}
	}

	private void OnGraphicSettingsUpdated(GraphicSettings graphicsSettings)
	{
		switch (graphicsSettings.oceanQuality)
		{
		case GenericQuality.HIGH:
			_tessellatedRenderer.maxLOD = _maxLOD_High;
			_tessellatedRenderer.LODBias = _LODBias_High;
			break;
		case GenericQuality.MEDIUM:
			_tessellatedRenderer.maxLOD = _maxLOD_Medium;
			_tessellatedRenderer.LODBias = _LODBias_Medium;
			break;
		case GenericQuality.LOW:
			_tessellatedRenderer.maxLOD = _maxLOD_Low;
			_tessellatedRenderer.LODBias = _LODBias_Low;
			break;
		}
	}
}
