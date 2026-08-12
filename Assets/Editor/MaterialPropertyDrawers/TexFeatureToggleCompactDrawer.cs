using UnityEditor;
using UnityEngine;

public class TexFeatureToggleCompactDrawer : MaterialPropertyDrawer
{
	private readonly string keyword;

	public TexFeatureToggleCompactDrawer(string keyword) => this.keyword = keyword;

	public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) => 0;

	public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
	{
		EditorGUI.BeginChangeCheck();
		editor.TexturePropertySingleLine(label, prop);
		if (EditorGUI.EndChangeCheck())
		{
			foreach (Material target in prop.targets)
			{
				if (prop.textureValue)
					target.EnableKeyword(keyword);
				else
					target.DisableKeyword(keyword);
			}
		}
	}
}
