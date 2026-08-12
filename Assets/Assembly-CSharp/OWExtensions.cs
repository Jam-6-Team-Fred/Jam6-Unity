using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public static class OWExtensions
{
	public static Texture2D ToTexture2D(this RenderTexture rTex)
	{
		Texture2D texture2D = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, mipChain: false);
		RenderTexture.active = rTex;
		texture2D.ReadPixels(new Rect(0f, 0f, rTex.width, rTex.height), 0, 0);
		texture2D.Apply();
		return texture2D;
	}

	public static bool SafeAdd<T>(this List<T> list, T value)
	{
		if (!list.Contains(value))
		{
			list.Add(value);
			return true;
		}
		return false;
	}

	public static bool SafeAdd<T>(this T[] arr, T item)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			if (arr[i].Equals(item))
			{
				return false;
			}
		}
		Array.Resize(ref arr, arr.Length + 1);
		arr[arr.Length - 1] = item;
		return true;
	}

	public static bool SafeAdd<T, K>(this IDictionary<T, K> dict, T key, K value)
	{
		if (!dict.ContainsKey(key))
		{
			dict.Add(key, value);
			return true;
		}
		return false;
	}

	public static bool QuickRemove<T>(this List<T> list, T item)
	{
		int num = list.IndexOf(item);
		if (num >= 0)
		{
			list.QuickRemoveAt(num);
			return true;
		}
		return false;
	}

	public static bool QuickRemove<T>(this T[] arr, T item)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			if (arr[i].Equals(item))
			{
				arr.QuickRemoveAt(i);
				return true;
			}
		}
		return false;
	}

	public static void QuickRemoveAt<T>(this List<T> list, int index)
	{
		list[index] = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
	}

	public static void QuickRemoveAt<T>(this T[] arr, int index)
	{
		arr[index] = arr[arr.Length - 1];
		arr[arr.Length - 1] = default(T);
		Array.Resize(ref arr, arr.Length - 1);
	}

	public static bool ApproxContains(this List<Vector3> list, Vector3 vector, float epsilon = 0.001f)
	{
		bool result = false;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].ApproxEquals(vector, epsilon))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static bool ApproxEquals(this Vector3 v1, Vector3 v2, float epsilon = 0.001f)
	{
		if (Mathf.Abs(v1.x - v2.x) < epsilon && Mathf.Abs(v1.y - v2.y) < epsilon && Mathf.Abs(v1.z - v2.z) < epsilon)
		{
			return true;
		}
		return false;
	}

	public static void SetLocalPositionX(this Transform trans, float x)
	{
		Vector3 localPosition = trans.localPosition;
		localPosition.x = x;
		trans.localPosition = localPosition;
	}

	public static void SetLocalPositionY(this Transform trans, float y)
	{
		Vector3 localPosition = trans.localPosition;
		localPosition.y = y;
		trans.localPosition = localPosition;
	}

	public static void SetLocalPositionZ(this Transform trans, float z)
	{
		Vector3 localPosition = trans.localPosition;
		localPosition.z = z;
		trans.localPosition = localPosition;
	}

	public static Quaternion InverseTransformRotation(this Transform t, Quaternion q)
	{
		return Quaternion.Inverse(t.rotation) * q;
	}

	public static void Assert(this OWCollider owCollider, string layerName, bool isTrigger)
	{
		owCollider.GetCollider().Assert(1 << LayerMask.NameToLayer(layerName), isTrigger);
	}

	public static void Assert(this OWCollider owCollider, LayerMask layerMask, bool isTrigger)
	{
		owCollider.GetCollider().Assert(layerMask, isTrigger);
	}

	public static void Assert(this Collider collider, string layerName, bool isTrigger)
	{
		collider.Assert(1 << LayerMask.NameToLayer(layerName), isTrigger);
	}

	public static void Assert(this Collider collider, LayerMask layerMask, bool isTrigger)
	{
		if (!OWLayerMask.IsLayerInMask(collider.gameObject.layer, layerMask))
		{
			Debug.LogError("This collider is not in the " + layerMask.ToString() + " LayerMask!", collider);
			Debug.Break();
		}
		if (collider.isTrigger != isTrigger)
		{
			Debug.LogError("isTrigger should be set to " + isTrigger, collider);
			Debug.Break();
		}
	}

	public static void SetAlpha(this Material mat, float a)
	{
		Color color = mat.color;
		color.a = a;
		mat.color = color;
	}

	public static void SetColorIndependentOfAlpha(this Material mat, Color c)
	{
		float a = mat.color.a;
		Color color = c;
		color.a = a;
		mat.color = color;
	}

	public static float GetAlpha(this Material mat)
	{
		return mat.color.a;
	}

	public static OWRigidbody GetOWRigidbodyInParents(this GameObject obj)
	{
		return obj.GetAttachedOWRigidbody(ignoreThisTransform: true);
	}

	public static OWRigidbody GetAttachedOWRigidbody(this GameObject obj, bool ignoreThisTransform = false)
	{
		OWRigidbody oWRigidbody = null;
		Transform transform = obj.transform;
		if (ignoreThisTransform)
		{
			transform = obj.transform.parent;
		}
		while (oWRigidbody == null)
		{
			oWRigidbody = transform.GetComponent<OWRigidbody>();
			if (oWRigidbody != null && !oWRigidbody.gameObject.activeInHierarchy)
			{
				oWRigidbody = null;
			}
			if ((transform == obj.transform.root && oWRigidbody == null) || oWRigidbody != null)
			{
				break;
			}
			transform = transform.parent;
		}
		return oWRigidbody;
	}

	public static OWRigidbody GetAttachedOWRigidbody(this Component cmpt, bool ignoreThisTransform = false)
	{
		return cmpt.gameObject.GetAttachedOWRigidbody(ignoreThisTransform);
	}

	public static CullGroup GetCullGroup(this GameObject obj)
	{
		Transform transform = obj.transform;
		while (transform != null)
		{
			CullGroup component = transform.GetComponent<CullGroup>();
			if (transform.GetComponent<CullGroupExcluder>() != null && component == null)
			{
				return null;
			}
			if (component != null)
			{
				return component;
			}
			transform = transform.parent;
		}
		return null;
	}

	public static CollisionGroup GetCollisionGroup(this GameObject obj)
	{
		Transform transform = obj.transform;
		while (transform != null)
		{
			CollisionGroup component = transform.GetComponent<CollisionGroup>();
			if (transform.GetComponent<CollisionGroupExcluder>() != null && component == null)
			{
				return null;
			}
			if (component != null)
			{
				return component;
			}
			transform = transform.parent;
		}
		return null;
	}

	public static LightsCullGroup GetLightsCullGroup(this GameObject obj)
	{
		Transform transform = obj.transform;
		while (transform != null)
		{
			LightsCullGroup component = transform.GetComponent<LightsCullGroup>();
			if (transform.GetComponent<LightsCullGroupExcluder>() != null && component == null)
			{
				return null;
			}
			if (component != null)
			{
				return component;
			}
			transform = transform.parent;
		}
		return null;
	}

	public static T GetAddComponent<T>(this GameObject obj) where T : Component
	{
		T val = obj.GetComponent<T>();
		if ((UnityEngine.Object)val == (UnityEngine.Object)null)
		{
			val = obj.AddComponent<T>();
		}
		return val;
	}

	public static T GetTaggedComponent<T>(this GameObject obj, string tag) where T : Component
	{
		T componentInChildren = obj.FindWithRequiredTag(tag).GetComponentInChildren<T>();
		if ((UnityEngine.Object)componentInChildren == (UnityEngine.Object)null)
		{
			Debug.LogError(string.Concat("Expected to find component of type ", typeof(T), " but found none"), obj);
			Debug.Break();
		}
		return componentInChildren;
	}

	public static T GetTaggedComponent<T>(this Component cmpt, string tag) where T : Component
	{
		T componentInChildren = cmpt.gameObject.FindWithRequiredTag(tag).GetComponentInChildren<T>();
		if ((UnityEngine.Object)componentInChildren == (UnityEngine.Object)null)
		{
			Debug.LogError(string.Concat("Expected to find component of type ", typeof(T), " but found none"), cmpt.gameObject);
			Debug.Break();
		}
		return componentInChildren;
	}

	public static T GetRequiredComponent<T>(this GameObject obj) where T : Component
	{
		T component = obj.GetComponent<T>();
		if ((UnityEngine.Object)component == (UnityEngine.Object)null)
		{
			Debug.LogError(string.Concat("Expected to find component of type ", typeof(T), " but found none"), obj);
			Debug.Break();
		}
		return component;
	}

	public static T GetRequiredComponent<T>(this Component cpnt) where T : Component
	{
		return cpnt.gameObject.GetRequiredComponent<T>();
	}

	public static T GetRequiredComponentInChildren<T>(this GameObject obj) where T : Component
	{
		T componentInChildren = obj.GetComponentInChildren<T>();
		if ((UnityEngine.Object)componentInChildren == (UnityEngine.Object)null)
		{
			Debug.LogError(string.Concat("Expected to find component of type ", typeof(T), " but found none"), obj);
			Debug.Break();
		}
		return componentInChildren;
	}

	public static T GetRequiredComponentInChildren<T>(this Component cpnt) where T : Component
	{
		return cpnt.gameObject.GetRequiredComponentInChildren<T>();
	}

	public static GameObject FindWithRequiredTag(this GameObject obj, string tag)
	{
		GameObject gameObject = GameObject.FindWithTag(tag);
		if (gameObject == null)
		{
			Debug.LogError("Failed to find a GameObject tagged " + tag, obj);
			Debug.Break();
		}
		return gameObject;
	}

	public static void DestroyAllComponents<T>(this GameObject obj) where T : Component
	{
		T[] components = obj.GetComponents<T>();
		for (int i = 0; i < components.Length; i++)
		{
			UnityEngine.Object.Destroy(components[i]);
		}
	}

	public static void DestroyAllComponentsImmediate<T>(this GameObject obj) where T : Component
	{
		T[] components = obj.GetComponents<T>();
		for (int i = 0; i < components.Length; i++)
		{
			UnityEngine.Object.DestroyImmediate(components[i]);
		}
	}

	public static void DestroyAllChildren(this Transform t)
	{
		for (int num = t.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(t.GetChild(num).gameObject);
		}
		t.DetachChildren();
	}

	public static void DestroyAllChildrenImmediate(this Transform t)
	{
		for (int num = t.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.DestroyImmediate(t.GetChild(num).gameObject);
		}
		t.DetachChildren();
	}

	public static void RecalculateTangents(this Mesh mesh)
	{
		int vertexCount = mesh.vertexCount;
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		Vector2[] uv = mesh.uv;
		int[] triangles = mesh.triangles;
		int num = triangles.Length / 3;
		Vector4[] array = new Vector4[vertexCount];
		Vector3[] array2 = new Vector3[vertexCount];
		Vector3[] array3 = new Vector3[vertexCount];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			long num3 = triangles[num2];
			long num4 = triangles[num2 + 1];
			long num5 = triangles[num2 + 2];
			Vector3 vector = vertices[num3];
			Vector3 vector2 = vertices[num4];
			Vector3 vector3 = vertices[num5];
			Vector2 vector4 = uv[num3];
			Vector2 vector5 = uv[num4];
			Vector2 vector6 = uv[num5];
			float num6 = vector2.x - vector.x;
			float num7 = vector3.x - vector.x;
			float num8 = vector2.y - vector.y;
			float num9 = vector3.y - vector.y;
			float num10 = vector2.z - vector.z;
			float num11 = vector3.z - vector.z;
			float num12 = vector5.x - vector4.x;
			float num13 = vector6.x - vector4.x;
			float num14 = vector5.y - vector4.y;
			float num15 = vector6.y - vector4.y;
			float num16 = 1f / (num12 * num15 - num13 * num14);
			Vector3 vector7 = new Vector3((num15 * num6 - num14 * num7) * num16, (num15 * num8 - num14 * num9) * num16, (num15 * num10 - num14 * num11) * num16);
			Vector3 vector8 = new Vector3((num12 * num7 - num13 * num6) * num16, (num12 * num9 - num13 * num8) * num16, (num12 * num11 - num13 * num10) * num16);
			array2[num3] += vector7;
			array2[num4] += vector7;
			array2[num5] += vector7;
			array3[num3] += vector8;
			array3[num4] += vector8;
			array3[num5] += vector8;
			num2 += 3;
		}
		for (int j = 0; j < vertexCount; j++)
		{
			Vector3 normal = normals[j];
			Vector3 tangent = array2[j];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			array[j].x = tangent.x;
			array[j].y = tangent.y;
			array[j].z = tangent.z;
			array[j].w = ((Vector3.Dot(Vector3.Cross(normal, tangent), array3[j]) < 0f) ? (-1f) : 1f);
		}
		mesh.tangents = array;
	}

	public static Vector3 WorldToCanvasPosition(this Canvas canvas, OWCamera owCamera, Vector3 worldPosition)
	{
		return canvas.WorldToCanvasPosition(owCamera.mainCamera, worldPosition);
	}

	public static Vector3 WorldToCanvasPosition(this Canvas canvas, Camera camera, Vector3 worldPosition)
	{
		RectTransform requiredComponent = canvas.GetRequiredComponent<RectTransform>();
		Vector3 vector = camera.WorldToViewportPoint(worldPosition);
		return new Vector3(vector.x * requiredComponent.sizeDelta.x, vector.y * requiredComponent.sizeDelta.y, vector.z);
	}

	public static float CanvasToWorldRatio(this Canvas canvas, OWCamera owCamera)
	{
		return canvas.CanvasToWorldRatio(owCamera.mainCamera);
	}

	public static float CanvasToWorldRatio(this Canvas canvas, Camera camera)
	{
		Vector3 vector = camera.ViewportToWorldPoint(new Vector3(0f, 0f, canvas.planeDistance));
		return (camera.ViewportToWorldPoint(new Vector3(1f, 0f, canvas.planeDistance)) - vector).magnitude / canvas.pixelRect.width;
	}

	public static bool StartsWith(this byte[] thisArray, byte[] otherArray)
	{
		if (otherArray == null)
		{
			Debug.LogWarning("Null array argument passed in. Returning false.");
			return false;
		}
		if (thisArray == otherArray)
		{
			return true;
		}
		if (otherArray.Length == 0)
		{
			return true;
		}
		if (thisArray.Length < otherArray.Length)
		{
			return false;
		}
		for (int i = 0; i < otherArray.Length; i++)
		{
			if (thisArray[i] != otherArray[i])
			{
				return false;
			}
		}
		return true;
	}

	public static string GetInnerXml(this XmlNode xmlNode)
	{
		return xmlNode.InnerXml;
	}

	public static string GetOuterXml(this XmlNode xmlNode)
	{
		return xmlNode.OuterXml;
	}

	public static void SetValue(this XmlNode xmlNode, string value)
	{
		if (xmlNode.NodeType == XmlNodeType.Element)
		{
			xmlNode.InnerText = value;
		}
		else
		{
			xmlNode.Value = value;
		}
	}

	public static string GetValue(this XmlNode xmlNode)
	{
		return xmlNode.Value;
	}

	public static XmlNode CreateNode(this XmlDocument xmlDoc, string tagName)
	{
		return xmlDoc.CreateNode(XmlNodeType.Element, tagName, string.Empty);
	}
}
