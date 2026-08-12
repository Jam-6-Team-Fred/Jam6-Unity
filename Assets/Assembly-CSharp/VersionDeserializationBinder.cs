using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;

public sealed class VersionDeserializationBinder : SerializationBinder, ISerializationBinder
{
	private static Regex signalDictRegex = new Regex(".*Dictionary`2\\[\\[.*Int32.*\\[.*Boolean.*");

	private static Regex conditionDictRegex = new Regex(".*Dictionary`2\\[\\[.*String.*\\[.*Boolean.*");

	private static Regex shipLogFactSaveDictRegex = new Regex(".*Dictionary`2\\[\\[.*String.*\\[.*ShipLogFact.*");

	private static Type deserializableSignalDictType = typeof(SerializableDictionary<int, bool>);

	private static Type deserializableConditionDictType = typeof(SerializableDictionary<string, bool>);

	private static Type deserializableShipLogFactSaveDictType = typeof(SerializableDictionary<string, ShipLogFact>);

	public override Type BindToType(string assemblyName, string typeName)
	{
		if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(typeName))
		{
			return null;
		}
		if (typeName.StartsWith("System.Collections.Generic.Dictionary"))
		{
			typeName = typeName.Replace("System.Collections.Generic.Dictionary", "SerializableDictionary");
			assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
		}
		else
		{
			assemblyName = Assembly.GetExecutingAssembly().FullName;
		}
		return Type.GetType($"{typeName}, {assemblyName}");
	}

	private bool TryGetSerializableDictionaryType(string typeName, out Type dictionaryType)
	{
		dictionaryType = null;
		if (!typeName.StartsWith("System.Collections.Generic.Dictionary"))
		{
			return false;
		}
		if (signalDictRegex.IsMatch(typeName))
		{
			dictionaryType = deserializableSignalDictType;
		}
		if (conditionDictRegex.IsMatch(typeName))
		{
			dictionaryType = deserializableConditionDictType;
		}
		if (shipLogFactSaveDictRegex.IsMatch(typeName))
		{
			dictionaryType = deserializableShipLogFactSaveDictType;
		}
		return dictionaryType != null;
	}
}
