using UnityEngine;

[ExecuteInEditMode]
public class CharacterLightingRigController : SectoredMonoBehaviour
{
	[SerializeField]
	private OWRenderer[] _characterRenderers = new OWRenderer[0];

	[Space]
	[ColorUsage(false, true)]
	[SerializeField]
	private Color _skyLightColor = new Color(0.36f, 0.49f, 0.71f, 1f);

	[SerializeField]
	private Vector3 _skyLightDir = new Vector3(-1f, 2f, -1f);

	[ColorUsage(false, true)]
	[SerializeField]
	private Color _bounceLightColor = new Color(0.71f, 0.52f, 0.36f, 1f);

	[SerializeField]
	private Vector3 _bounceLightDir = new Vector3(0f, -2f, 1f);

	[ColorUsage(false, true)]
	[SerializeField]
	private Color _rimLightColor = new Color(0.83f, 0.81f, 0.57f, 1f);

	[SerializeField]
	private Vector3 _rimLightDir = new Vector3(1f, 0f, -1f);

	[Space]
	[SerializeField]
	private Vector3 _falloffCenter = new Vector3(0f, 2f, 0f);

	[SerializeField]
	private float _falloffRadius = 1f;

	private int _propID_SkyLightColor;

	private int _propID_SkyLightDir;

	private int _propID_BounceLightColor;

	private int _propID_BounceLightDir;

	private int _propID_RimLightColor;

	private int _propID_RimLightDir;

	private int _propID_LightCenterRadius;

	private Transform _transform;

	protected override void Awake()
	{
		base.Awake();
		_propID_SkyLightColor = Shader.PropertyToID("_SkyLightColor");
		_propID_SkyLightDir = Shader.PropertyToID("_SkyLightDir");
		_propID_BounceLightColor = Shader.PropertyToID("_BounceLightColor");
		_propID_BounceLightDir = Shader.PropertyToID("_BounceLightDir");
		_propID_RimLightColor = Shader.PropertyToID("_RimLightColor");
		_propID_RimLightDir = Shader.PropertyToID("_RimLightDir");
		_propID_LightCenterRadius = Shader.PropertyToID("_LightCenterRadius");
		_transform = base.transform;
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}

	private void LateUpdate()
	{
		Vector3 vector = _transform.TransformDirection(_skyLightDir);
		Vector3 vector2 = _transform.TransformDirection(_bounceLightDir);
		Vector3 vector3 = _transform.TransformDirection(_rimLightDir);
		Vector3 vector4 = _transform.TransformPoint(_falloffCenter);
		float w = _falloffRadius * _transform.lossyScale.x;
		for (int i = 0; i < _characterRenderers.Length; i++)
		{
			_characterRenderers[i].SetMaterialProperty(_propID_SkyLightColor, _skyLightColor);
			_characterRenderers[i].SetMaterialProperty(_propID_SkyLightDir, new Vector4(vector.x, vector.y, vector.z, 0f));
			_characterRenderers[i].SetMaterialProperty(_propID_BounceLightColor, _bounceLightColor);
			_characterRenderers[i].SetMaterialProperty(_propID_BounceLightDir, new Vector4(vector2.x, vector2.y, vector2.z, 0f));
			_characterRenderers[i].SetMaterialProperty(_propID_RimLightColor, _rimLightColor);
			_characterRenderers[i].SetMaterialProperty(_propID_RimLightDir, new Vector4(vector3.x, vector3.y, vector3.z, 0f));
			_characterRenderers[i].SetMaterialProperty(_propID_LightCenterRadius, new Vector4(vector4.x, vector4.y, vector4.z, w));
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(Vector3.zero, _skyLightDir);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(Vector3.zero, _bounceLightDir);
			Gizmos.color = Color.green;
			Gizmos.DrawLine(Vector3.zero, _rimLightDir);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(_falloffCenter, _falloffRadius);
		}
	}
}
