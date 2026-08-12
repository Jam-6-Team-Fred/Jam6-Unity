using OWML.Utils;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BRDFRegistry))]
public class BRDFRegistryEditor : Editor
{
	private string[] _names;

	private void OnEnable()
	{
		// NicifyVariableName yells if in ctor
		_names = EnumUtils.GetNames<BRDFRegistry.BRDFLookupID>().Select(ObjectNames.NicifyVariableName).ToArray();
	}

	public override void OnInspectorGUI()
	{
		var target = (BRDFRegistry)this.target;
		for (var i = 0; i < BRDFRegistry.kNumBRDFs; i++)
		{
			EditorGUILayout.LabelField($"{_names[i]} BRDF", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Specular Color");
			GUILayout.FlexibleSpace();
			var specularColor = EditorGUILayout.ColorField(target.brdfSpecColors[i], GUILayout.Width(80));
			EditorGUILayout.EndHorizontal();
			var lookupTexture = (Texture2D)EditorGUILayout.ObjectField("Lookup Texture", target.brdfLookups[i], typeof(Texture2D), false);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "Change BRDF Registry");
				target.brdfSpecColors[i] = specularColor;
				target.brdfLookups[i] = lookupTexture;
				EditorUtility.SetDirty(target);
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
		}

		if (GUILayout.Button("Reload BRDFs")) ReloadBRDFs();
	}

	[MenuItem("Visuals/Reload BRDFs")]
	public static void ReloadBRDFs()
	{
		var target = FindObjectOfType<BRDFManager>()._brdfRegistryAsset;
		for (var i = 0; i < BRDFRegistry.kNumBRDFs; i++)
		{
			if (target.brdfLookups[i] == null) continue;
			target.brdfLookupArray.SetPixels(target.brdfLookups[i].GetPixels(), i);
		}
		EditorUtility.SetDirty(target.brdfLookupArray);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		target.UpdateBRDFs();
	}
}
