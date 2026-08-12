using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace CameraGravityAlign
{
	/// <summary>
	/// patches the camera rotation code to do our own custom aligned version
	///
	/// this is so fucking jank LOL
	/// </summary>
	[InitializeOnLoad]
	public static class EditorCamera
	{
		static Vector3? GetUp() //Edit this function to get up from gravity instead of world origin
		{
			return CustomTransformEditor.GetUp(Pivot);
		}

		static SceneView View => SceneView.lastActiveSceneView;
		static Vector3 Pivot => SceneView.lastActiveSceneView.pivot;

		static Vector2 eulers;

		static Quaternion oldBaseRot;
		static bool initialized = false;

		static EditorCamera()
		{
			SceneView.duringSceneGui -= OnSceneGUI;
			SceneView.duringSceneGui += OnSceneGUI;
			initialized = false;

			Harmony.CreateAndPatchAll(typeof(EditorCamera));
		}

		static void OnSceneGUI(SceneView _)
		{
			if (!alignEnabled) return;

			if (!initialized)
			{
				initialized = true;
				oldBaseRot = View.rotation;
				eulers = new Vector2(0f, 40f); //Start look down a bit -> could change to get more accurate value?
			}

			var lookRot = Quaternion.Euler(eulers.y, eulers.x, 0f);

			//---------------- Align ----------------//
			var maybeUp = GetUp();
			if (maybeUp == null) return;
			var up = maybeUp.Value;

			var baseRot = Quaternion.FromToRotation(oldBaseRot * CustomTransformEditor.GetAlignAxis(), up) * oldBaseRot;
			oldBaseRot = baseRot;

			//---------------- Rotate ----------------//
			var newRot = baseRot.TransformRotation(lookRot);
			// check diff since it causes repaint
			if (View.rotation.eulerAngles != newRot.eulerAngles) View.rotation = newRot;

			//---------------- Draw Pivot ----------------//
			if (!showPivot) return;
			float radius = 0.1f;
			Vector3 camPos = View.camera.transform.position;
			Vector3 vec = Pivot - camPos;

			// copied from HandleUtility.RaySnap
			PhysicsScene physicsScene = Physics.defaultPhysicsScene;
			Scene scene = Camera.current.scene;
			if (scene.IsValid())
				physicsScene = scene.GetPhysicsScene();

			Handles.DrawWireDisc(Pivot, up, radius);
			if (!physicsScene.Raycast(camPos, vec, out RaycastHit hitInfo, vec.magnitude, OWLayerMask.physicalMask))
			{
				if (physicsScene.Raycast(Pivot, -up, out RaycastHit hitInfo2, 10000f, OWLayerMask.physicalMask))
				{
					Handles.DrawWireDisc(hitInfo2.point, hitInfo2.normal, radius * 5f);
					Handles.color *= 0.3f;
					Handles.DrawLine(Pivot, hitInfo2.point);
					Handles.color = Color.white;
				}
			}
		}

		[HarmonyTargetMethod]
		public static MethodBase TargetMethod() => AccessTools.Method(AccessTools.TypeByName("SceneViewMotion"), "HandleMouseDrag");

		/// <summary>
		/// euler angles tracking here
		/// </summary>
		[HarmonyPrefix]
		public static bool Prefix()
		{
			if (!alignEnabled) return true;

			var e = Event.current;

			if (!((!e.alt && e.RightMouse()) || (e.alt && e.LeftMouse()))) return true;
			if (View.camera.orthographic) return true; //Only for perspective.

			var maybeUp = GetUp();
			if (maybeUp == null) return true;

			//---------------- Mouse Input ----------------//
			eulers += e.delta * .003f * Mathf.Rad2Deg;

			return false;
		}

		//--------------------------------------------- Settings ---------------------------------------------//
		static bool alignEnabled { get; set; } = false;
		static bool showPivot { get; set; } = true;

		//--------------------------------------------- Menu Items ---------------------------------------------//
		[MenuItem("Streetlights/Camera Align/Toggle Align #a")]
		public static void Align()
		{
			alignEnabled = !alignEnabled;
			if (!alignEnabled)
			{
				View.rotation = Quaternion.LookRotation(View.rotation.Forward());
				initialized = false;
			}
			SceneView.RepaintAll();
		}

		[MenuItem("Streetlights/Camera Align/Toggle Pivot")]
		public static void ShowPivot()
		{
			showPivot = !showPivot;
			SceneView.RepaintAll();
		}
	}

	public static class CamExtensions
	{
		public static Vector3 Up(this Quaternion rotation) => rotation * Vector3.up;
		public static Vector3 Forward(this Quaternion rotation) => rotation * Vector3.forward;
		public static Quaternion TransformRotation(this Quaternion reference, Quaternion localRotation) => reference * localRotation;

		//--------------------------------------------- Input ---------------------------------------------//
		public static bool LeftMouse(this Event e) => e.button == 0;
		public static bool RightMouse(this Event e) => e.button == 1;
		public static bool MiddleMouse(this Event e) => e.button == 2;

		public static bool MouseDown(this Event e) => e.type == EventType.MouseDown;
		public static bool MouseDrag(this Event e) => e.type == EventType.MouseDrag;
		public static bool MouseUp(this Event e) => e.type == EventType.MouseUp;
	}
}
