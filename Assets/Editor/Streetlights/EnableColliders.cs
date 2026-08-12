using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// colliders and shapes get disabled at build time. this undoes that
/// </summary>
public static class EnableColliders
{
	[MenuItem("Streetlights/Enable Colliders Under Selected", true)]
	public static bool V_EnableCollidersUnderSelected() =>
		Selection.activeGameObject && (
			Selection.activeGameObject.GetComponentsInChildren<Collider>(true).Any(x => !x.enabled) ||
			Selection.activeGameObject.GetComponentsInChildren<Shape>(true).Any(x => !x.enabled)
		);

	[MenuItem("Streetlights/Enable Colliders Under Selected")]
	public static void EnableCollidersUnderSelected()
	{
		foreach (var collider in Selection.activeGameObject.GetComponentsInChildren<Collider>(true))
		{
			Undo.RecordObject(collider, "Enable Colliders");
			collider.enabled = true;
		}
		foreach (var collider in Selection.activeGameObject.GetComponentsInChildren<Shape>(true))
		{
			Undo.RecordObject(collider, "Enable Colliders");
			collider.enabled = true;
		}
	}
}
