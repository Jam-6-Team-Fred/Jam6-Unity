using OWML.Utils;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class EditSceneView
{
	private static SceneView.CameraMode _rgbMode;
	private static SceneView.CameraMode _aMode;
	private static SceneView.CameraMode _debugMode;

	[MenuItem("Visuals/Edit Scene View")]
	[InitializeOnLoadMethod]
	public static async void Init()
	{
		// sceneview doesnt exist until next frame
		await Task.Yield();

		SceneView.ClearUserDefinedCameraModes();
		_rgbMode = SceneView.AddCameraMode("Vertex Color (RGB)", "Outer Wilds");
		_aMode = SceneView.AddCameraMode("Vertex Color (A)", "Outer Wilds");
		_debugMode = SceneView.AddCameraMode("DEBUG show attribute", "Outer Wilds");

		if (SceneView.lastActiveSceneView != null){
			SceneView.lastActiveSceneView.onCameraModeChanged -= OnCameraModeChanged;
			SceneView.lastActiveSceneView.onCameraModeChanged += OnCameraModeChanged;

			// fix unlit mode
			foreach (var light in SceneView.lastActiveSceneView.GetValue<Light[]>("m_Light"))
				light.range = float.MaxValue;
		}

		SceneView.duringSceneGui -= OnSceneGUI;
		SceneView.duringSceneGui += OnSceneGUI;
	}

	private static void OnCameraModeChanged(SceneView.CameraMode mode)
	{
		if (mode == _rgbMode)
			SceneView.lastActiveSceneView.SetSceneViewShaderReplace(Shader.Find("SceneView/VertexColorRGB"), null);
		else if (mode == _aMode)
			SceneView.lastActiveSceneView.SetSceneViewShaderReplace(Shader.Find("SceneView/VertexColorA"), null);
		else if (mode == _debugMode)
			SceneView.lastActiveSceneView.SetSceneViewShaderReplace(Shader.Find("SceneView/DebugAttrib"), null);
		else
			SceneView.lastActiveSceneView.SetSceneViewShaderReplace(null, null);
	}

	private static void OnSceneGUI(SceneView sceneView)
	{
		Handles.BeginGUI();
		var go = Selection.activeGameObject;
		if (go)
		{
			var style = new GUIStyle(GUI.skin.box) { richText = true, fontSize = 10, margin = new RectOffset() };
			{
				var group = go.GetCullGroup();
				if (group)
				{
					if (group is ISectorGroup sectorGroup && sectorGroup.GetSector())
						GUILayout.Label($"Part of CullGroup <b>{group.name}</b> belonging to Sector <b>{sectorGroup.GetSector().name}</b>", style);
					else
						GUILayout.Label($"Part of CullGroup <b>{group.name}</b>", style);
				}
			}
			{
				var group = go.GetCollisionGroup();
				if (group)
				{
					if (group is ISectorGroup sectorGroup && sectorGroup.GetSector())
						GUILayout.Label($"Part of CollisionGroup <b>{group.name}</b> belonging to Sector <b>{sectorGroup.GetSector().name}</b>", style);
					else
						GUILayout.Label($"Part of CollisionGroup <b>{group.name}</b>", style);
				}
			}
			{
				var group = go.GetLightsCullGroup();
				if (group)
				{
					if (group is ISectorGroup sectorGroup && sectorGroup.GetSector())
						GUILayout.Label($"Part of LightsCullGroup <b>{group.name}</b> belonging to Sector <b>{sectorGroup.GetSector().name}</b>", style);
					else
						GUILayout.Label($"Part of LightsCullGroup <b>{group.name}</b>", style);
				}
			}
		}

		Handles.EndGUI();
	}
}
