using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CanEditMultipleObjects]
[CustomEditor(typeof(Transform))]
public class CustomTransformEditor : Editor
{
	//Unity's built-in editor
	private Editor defaultEditor;

	private void OnEnable()
	{
		//When this inspector is created, also create the built-in inspector
		defaultEditor = CreateEditor(targets, Type.GetType("UnityEditor.TransformInspector, UnityEditor"));

		SceneView.duringSceneGui += OnSceneGUI;
		Selection.selectionChanged += OnSelectionChanged;
	}

	private void OnDisable()
	{
		//When OnDisable is called, the default editor we created should be destroyed to avoid memory leakage.
		//Also, make sure to call any required methods like OnDisable
		var disableMethod = defaultEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		if (disableMethod != null)
			disableMethod.Invoke(defaultEditor, null);
		DestroyImmediate(defaultEditor);

		SceneView.duringSceneGui -= OnSceneGUI;
		Selection.selectionChanged -= OnSelectionChanged;

		_align = _stick = Tools.hidden = false;
	}

	private static bool _stick;
	private enum StickMode { Surface, Radius }
	private static StickMode _stickMode = StickMode.Surface;
	private static bool _align;
	private enum AlignAxis { PosX, PosY, PosZ, NegX, NegY, NegZ }
	private static AlignAxis _alignAxis = AlignAxis.PosY;
	private enum AlignMode { Gravity, Surface, Radius }
	private static AlignMode _alignMode = AlignMode.Gravity;
	// private static bool _alignSmoothing;
	private static Transform _reference;
	private static float _radius = 10f;

	private static ForceVolume _forceVolume;
	private static bool _picking;

	private void OnSelectionChanged()
	{
		if (_picking) _reference = Selection.activeTransform;
		OnReferenceOrTargetChanged();
	}

	// stops picking and updates force volume
	private void OnReferenceOrTargetChanged()
	{
		if (_picking) _picking = false;

		var target = (Transform)this.target;
		// very thorough, but probably very slow. oh well
		var transform = _reference ? _reference : target;
		while (transform)
		{
			_forceVolume = transform.GetComponentInChildren<ForceVolume>();
			if (_forceVolume) break;
			transform = transform.parent;
		}
		if (_forceVolume)
		{
			// super jank, probably breaks shit, lol
			_forceVolume.Invoke("Awake");
		}
	}

	public override void OnInspectorGUI()
	{
		EditorGUIUtility.labelWidth = 70;

		defaultEditor.OnInspectorGUI();

		var smol = EditorGUIUtility.currentViewWidth < 350;

		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		_stick = GUILayout.Toggle(_stick, new GUIContent("Stick", Resources_Load<Texture2D>($"Editor/Textures/Stick_{(_stick ? "On" : "Off")}")), EditorStyles.miniButtonLeft, GUILayout.MinWidth(60), GUILayout.MaxWidth(1000));
		if (GUILayout.Button(new GUIContent(smol ? null : _stickMode.ToString(), Resources_Load<Texture2D>($"Editor/Textures/StickMode_{_stickMode}"), _stickMode.ToString()), EditorStyles.miniButtonRight, GUILayout.MinWidth(30), GUILayout.MaxWidth(2000)))
			_stickMode = (StickMode)(((int)_stickMode + 1) % 2);

		_align = GUILayout.Toggle(_align, new GUIContent("Align", Resources_Load<Texture2D>($"Editor/Textures/Align_{(_align ? "On" : "Off")}")), EditorStyles.miniButtonLeft, GUILayout.MinWidth(60), GUILayout.MaxWidth(1000));
		_alignAxis = (AlignAxis)EditorGUILayout.Popup((int)_alignAxis, new[] { "+X", "+Y", "+Z", "-X", "-Y", "-Z" }, EditorStyles.miniButtonMid, GUILayout.Width(30));
		if (GUILayout.Button(new GUIContent(smol ? null : _alignMode.ToString(), Resources_Load<Texture2D>($"Editor/Textures/AlignMode_{_alignMode}"), _alignMode.ToString()), EditorStyles.miniButtonMid, GUILayout.MinWidth(30), GUILayout.MaxWidth(2000)))
			_alignMode = (AlignMode)(((int)_alignMode + 1) % 3);
		// no clue how this works or why its disabled sometimes
		// _alignSmoothing = GUILayout.Toggle(_alignSmoothing, new GUIContent(smol ? null : "Smoothing", Resources_Load<Texture2D>($"Editor/Textures/Smoothing_{(_alignSmoothing ? "On" : "Off")}"), "Smoothing"), EditorStyles.miniButtonRight, GUILayout.MinWidth(30), GUILayout.MaxWidth(2000));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		_reference = (Transform)EditorGUILayout.ObjectField(new GUIContent("Reference", "Used for Gravity and Radius"), _reference, typeof(Transform), true);
		if (EditorGUI.EndChangeCheck()) OnReferenceOrTargetChanged();
		_picking = GUILayout.Toggle(_picking, "Pick", EditorStyles.miniButtonLeft, GUILayout.Width(40));
		GUI.enabled = _reference;
		if (GUILayout.Button("Clear", EditorStyles.miniButtonRight, GUILayout.Width(40)))
		{
			_reference = null;
			OnReferenceOrTargetChanged();
		}
		EditorGUILayout.EndHorizontal();

		_radius = EditorGUILayout.FloatField("Radius", _radius);
		GUI.enabled = true;

		EditorGUILayout.Space();
		GUI.enabled = false;
		EditorGUILayout.ObjectField(_forceVolume, typeof(ForceVolume), true);
		GUI.enabled = true;

		EditorGUILayout.Space();
		if (_stickMode == StickMode.Radius && !_reference)
			EditorGUILayout.HelpBox("Stick Mode is Radius, but no Reference was set", MessageType.Warning);
		if (_alignMode == AlignMode.Gravity && !_forceVolume)
			EditorGUILayout.HelpBox("Align Mode is Gravity, but no ForceVolume was found", MessageType.Warning);
		else if (_alignMode == AlignMode.Radius && !_reference)
			EditorGUILayout.HelpBox("Align Mode is Radius, but no Reference was set", MessageType.Warning);
	}

	public static Vector3 GetAlignAxis()
	{
		switch (_alignAxis)
		{
			case AlignAxis.PosX: return new Vector3(1, 0, 0);
			case AlignAxis.PosY: return new Vector3(0, 1, 0);
			case AlignAxis.PosZ: return new Vector3(0, 0, 1);
			case AlignAxis.NegX: return new Vector3(-1, 0, 0);
			case AlignAxis.NegY: return new Vector3(0, -1, 0);
			case AlignAxis.NegZ: return new Vector3(0, 0, -1);
			default: throw new ArgumentOutOfRangeException();
		}
	}

	private void OnSceneGUI(SceneView _)
	{
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
		{
			_stick = true;
			Repaint();
		}
		else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.S)
		{
			_stick = false;
			Repaint();
		}
		else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A)
		{
			_align = true;
			Repaint();
		}
		else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.A)
		{
			_align = false;
			Repaint();
		}

		var target = (Transform)this.target;
		EditorGUI.BeginChangeCheck();

		var position = Vector3.zero;
		Tools.hidden = _stick || _align;
		if (_stick)
		{
			var size = HandleUtility.GetHandleSize(target.position);
			Handles.color = Handles.zAxisColor;
			Handles.DrawLine(target.position, target.position + target.forward * size);
			Handles.color = Handles.xAxisColor;
			Handles.DrawLine(target.position, target.position + target.right * size);
			Handles.color = Handles.centerColor;
			position = Handles.FreeMoveHandle(target.position, Quaternion.identity, 1 / 4f * size, Vector3.zero, Handles.CircleHandleCap);
		}
		else if (_align)
		{
			position = Handles.PositionHandle(target.position, Quaternion.identity);
		}

		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(target, "Move Transform");
			target.position = position;
			if (_stick)
			{
				// edit position
				switch (_stickMode)
				{
					case StickMode.Surface:
						var active = target.gameObject.activeSelf;
						var cullingMask = Camera.current.cullingMask;
						target.gameObject.SetActive(false);
						Camera.current.cullingMask = OWLayerMask.groundMask;
						var obj = HandleUtility.RaySnap(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition));
						if (obj is RaycastHit hit)
						{
							target.position = hit.point;
						}
						target.gameObject.SetActive(active);
						Camera.current.cullingMask = cullingMask;
						break;
					case StickMode.Radius:
						if (_reference)
						{
							// do ray sphere intersection instead. current method sucks
							var up = (target.position - _reference.position).normalized;
							target.position = _reference.position + up * _radius;
						}
						break;
				}
			}
			if (_align)
			{
				// edit rotation
				switch (_alignMode)
				{
					case AlignMode.Gravity:
						if (_forceVolume)
						{
							var up = -_forceVolume.CalculateForceAccelerationAtPoint(target.position).normalized;
							var alignAxis = target.rotation * GetAlignAxis();
							target.rotation = Quaternion.FromToRotation(alignAxis, up) * target.rotation;
						}
						break;
					case AlignMode.Surface:
						// maybe dont raycast twice if you have both stick and align on
						var active = target.gameObject.activeSelf;
						var cullingMask = Camera.current.cullingMask;
						target.gameObject.SetActive(false);
						Camera.current.cullingMask = OWLayerMask.groundMask;
						var obj = HandleUtility.RaySnap(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition));
						if (obj is RaycastHit hit)
						{
							var up = hit.normal;
							var alignAxis = target.rotation * GetAlignAxis();
							target.rotation = Quaternion.FromToRotation(alignAxis, up) * target.rotation;
						}
						target.gameObject.SetActive(active);
						Camera.current.cullingMask = cullingMask;
						break;
					case AlignMode.Radius:
						if (_reference)
						{
							var up = (target.position - _reference.position).normalized;
							var alignAxis = target.rotation * GetAlignAxis();
							target.rotation = Quaternion.FromToRotation(alignAxis, up) * target.rotation;
						}
						break;
				}
			}
		}
	}

	// for EditorCamera
	public static Vector3? GetUp(Vector3 targetPosition)
	{
		Vector3? up = null;
		switch (_alignMode)
		{
			case AlignMode.Gravity:
				if (_forceVolume)
				{
					up = -_forceVolume.CalculateForceAccelerationAtPoint(targetPosition).normalized;
				}
				break;
			case AlignMode.Surface:
				// surface is jank so use gravity or radius
				if (_forceVolume)
				{
					up = -_forceVolume.CalculateForceAccelerationAtPoint(targetPosition).normalized;
				}
				else if (_reference)
				{
					up = (targetPosition - _reference.position).normalized;
				}
				break;
			case AlignMode.Radius:
				if (_reference)
				{
					up = (targetPosition - _reference.position).normalized;
				}
				break;
		}
		return up;
	}


	// apparently if you dont cache this unity at some point overflows some counter and dies (hawkbar told me this)
	private static readonly Dictionary<string, Object> _resourceCache = new Dictionary<string, Object>();

	private static T Resources_Load<T>(string path) where T : Object
	{
		if (!_resourceCache.TryGetValue(path, out var resource))
		{
			resource = Resources.Load<T>(path);
			_resourceCache.Add(path, resource);
		}
		return (T)resource;
	}
}
