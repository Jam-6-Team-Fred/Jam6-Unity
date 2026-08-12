using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[PreferBinarySerialization]
public class SceneMeshData : ScriptableObject
{
	[SerializeField]
	public List<Mesh> data;
}
