using OWML.Utils;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AudioType))]
public class AudioTypeDrawer : PropertyDrawer
{
	private static readonly string[] _names;

	static AudioTypeDrawer()
	{
		var categories = EnumUtils.GetValues<AudioTypeCategory>();
		_names = EnumUtils.GetValues<AudioType>()
			.Select(value =>
			{
				if (value == AudioType.None)
				{
					return ObjectNames.NicifyVariableName(value.ToString());
				}
				for (var i = 0; i < categories.Length; i++)
				{
					if (i == categories.Length - 1 ||
						(int)categories[i] <= (int)value && (int)value < (int)categories[i + 1])
					{
						return $"{ObjectNames.NicifyVariableName(categories[i].ToString())}/{ObjectNames.NicifyVariableName(value.ToString())}";
					}
				}
				return ObjectNames.NicifyVariableName(value.ToString());
			})
			.ToArray();
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
		var index = EditorGUI.Popup(position, label.text, property.enumValueIndex, _names);
		EditorGUI.showMixedValue = false;
		if (EditorGUI.EndChangeCheck())
		{
			property.enumValueIndex = index;
		}
		EditorGUI.EndProperty();
	}
}
