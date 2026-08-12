using UnityEditor;
using UnityEngine;

public class TexCompactDrawer : MaterialPropertyDrawer
{
	public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) => 0;

	public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
	{
		editor.TexturePropertySingleLine(label, prop);
	}
}
