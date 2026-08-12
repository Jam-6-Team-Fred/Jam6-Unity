using UnityEngine;

[ExecuteInEditMode]
public class HourglassTwinsShaderController : MonoBehaviour
{
	[SerializeField]
	private Transform _towerTwin;

	[SerializeField]
	private Transform _caveTwin;

	[SerializeField]
	private SandLevelController _towerTwinSand;

	[SerializeField]
	private SandLevelController _caveTwinSand;

	private int _propID_InvWorldMatTT;

	private int _propID_InvWorldMatCT;

	private int _propID_SandRadiusTT;

	private int _propID_SandRadiusCT;

	private void Start()
	{
		_propID_InvWorldMatTT = Shader.PropertyToID("_InvWorldMatTT");
		_propID_InvWorldMatCT = Shader.PropertyToID("_InvWorldMatCT");
		_propID_SandRadiusTT = Shader.PropertyToID("_SandRadiusTT");
		_propID_SandRadiusCT = Shader.PropertyToID("_SandRadiusCT");
		UpdateShaderGlobals();
	}

	private void LateUpdate()
	{
		UpdateShaderGlobals();
	}

	private void UpdateShaderGlobals()
	{
		Shader.SetGlobalMatrix(_propID_InvWorldMatTT, _towerTwin.worldToLocalMatrix);
		Shader.SetGlobalMatrix(_propID_InvWorldMatCT, _caveTwin.worldToLocalMatrix);
		Shader.SetGlobalFloat(_propID_SandRadiusTT, _towerTwinSand.GetRadius());
		Shader.SetGlobalFloat(_propID_SandRadiusCT, _caveTwinSand.GetRadius());
	}
}
