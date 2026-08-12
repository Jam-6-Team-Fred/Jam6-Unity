using System.Text;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
	private const int kHistoryLength = 240;

	private const float kGraphScale = 30f;

	private static Color kColor_line_frameTime = new Color(0.2f, 0.53f, 0.65f, 1f);

	private static Color kColor_label_frameTime = new Color(0.2f, 0.53f, 0.65f, 0.5f);

	private static Color kColor_line_stepCount = new Color(0.8f, 0.44f, 0f, 0.5f);

	private static Color kColor_label_stepCount = new Color(0.8f, 0.44f, 0f, 0.5f);

	private const string kLabel_FPS = " fps";

	private const string kLabel_ms16 = "16 ms";

	private const string kLabel_ms33 = "33 ms";

	private const string kLabel_step1 = "1 step";

	private const string kLabel_step2 = "2 steps";

	private int _numPhysicsStepsThisFrame;

	private int _curFrameIndex;

	private float[] _frameTimeHistory;

	private int[] _physicsStepsHistory;

	private float[] _physicsTimeHistory;

	private int _fps;

	private string _fpsLabel;

	private Material _lineMaterial;

	private StringBuilder _stringBuilder;

	private GUIStyle _boldGUIStyle;

	private GUIStyle _rightAlignedGUIStyle;

	private GUIStyle _boldRightAlignedGUIStyle;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Instantiate()
	{
	}

	private void Awake()
	{
		_numPhysicsStepsThisFrame = 0;
		_curFrameIndex = 0;
		_frameTimeHistory = new float[240];
		_physicsStepsHistory = new int[240];
		_physicsTimeHistory = new float[240];
		for (int i = 0; i < 240; i++)
		{
			_frameTimeHistory[i] = 0f;
			_physicsStepsHistory[i] = 0;
			_physicsTimeHistory[i] = 0f;
		}
		_lineMaterial = new Material(Shader.Find("Hidden/Lines"));
		_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
		_stringBuilder = new StringBuilder(16);
	}

	private void OnDestroy()
	{
		_frameTimeHistory = null;
		_physicsStepsHistory = null;
		_physicsTimeHistory = null;
		Object.Destroy(_lineMaterial);
		_lineMaterial = null;
		_stringBuilder = null;
	}

	private void FixedUpdate()
	{
		_numPhysicsStepsThisFrame++;
	}

	private void LateUpdate()
	{
		_frameTimeHistory[_curFrameIndex] = Time.unscaledDeltaTime;
		_physicsStepsHistory[_curFrameIndex] = _numPhysicsStepsThisFrame;
		_physicsTimeHistory[_curFrameIndex] = (float)_numPhysicsStepsThisFrame * Time.fixedUnscaledDeltaTime;
		_fps = 0;
		int num = _curFrameIndex;
		float num2 = 0f;
		while (num2 < 1f && _fps < 240)
		{
			num2 += _frameTimeHistory[num];
			_fps++;
			num--;
			if (num < 0)
			{
				num = 239;
			}
		}
		_curFrameIndex++;
		if (_curFrameIndex >= 240)
		{
			_curFrameIndex = 0;
		}
		_numPhysicsStepsThisFrame = 0;
	}

	private void OnGUI()
	{
		if (!GUIMode.IsFPSMode() && !GUIMode.IsDebugMode())
		{
			return;
		}
		if (_boldGUIStyle == null)
		{
			GUIStyle style = GUI.skin.GetStyle("Label");
			_boldGUIStyle = new GUIStyle(style);
			_boldGUIStyle.fontStyle = FontStyle.Bold;
			_rightAlignedGUIStyle = new GUIStyle(style);
			_rightAlignedGUIStyle.alignment = TextAnchor.UpperRight;
			_boldRightAlignedGUIStyle = new GUIStyle(style);
			_boldRightAlignedGUIStyle.fontStyle = FontStyle.Bold;
			_boldRightAlignedGUIStyle.alignment = TextAnchor.UpperRight;
		}
		if (Event.current.type == EventType.Layout)
		{
			_stringBuilder.Length = 0;
			_stringBuilder.Append(_fps);
			if (GUIMode.IsDebugMode())
			{
				_stringBuilder.Append(" fps");
			}
			_fpsLabel = _stringBuilder.ToString();
		}
		if (GUIMode.IsFPSMode())
		{
			Rect position = new Rect(16f, 16f, 32f, 24f);
			Rect position2 = new Rect(position.x + 4f, position.y, 24f, 24f);
			GUI.color = new Color(0f, 0f, 0f, 0.5f);
			GUI.DrawTexture(position, Texture2D.whiteTexture);
			GUI.color = Color.white;
			GUI.Label(position2, _fpsLabel, _boldRightAlignedGUIStyle);
		}
		else
		{
			if (!GUIMode.IsDebugMode())
			{
				return;
			}
			Rect position3 = new Rect(16f, (float)Screen.height - 166f, 480f, 150f);
			Rect position4 = new Rect(position3.x + 4f, position3.y, 64f, 64f);
			Rect rect = new Rect(position3.x, 16f, position3.width, position3.height - 20f);
			Rect position5 = new Rect(position3.xMax - 64f, position3.y + 80f, 64f, 64f);
			Rect position6 = new Rect(position3.xMax - 64f, position3.y + 16f, 64f, 64f);
			GUI.color = new Color(0f, 0f, 0f, 0.5f);
			GUI.DrawTexture(position3, Texture2D.whiteTexture);
			GUI.color = kColor_label_stepCount;
			GUI.Label(position5, "1 step", _rightAlignedGUIStyle);
			GUI.Label(position6, "2 steps", _rightAlignedGUIStyle);
			position5.y -= 16f;
			position6.y -= 16f;
			GUI.color = kColor_label_frameTime;
			GUI.Label(position5, "16 ms", _rightAlignedGUIStyle);
			GUI.Label(position6, "33 ms", _rightAlignedGUIStyle);
			if (Event.current.type == EventType.Repaint)
			{
				_lineMaterial.SetPass(0);
				GL.PushMatrix();
				GL.LoadPixelMatrix(0f, Screen.width, 0f, Screen.height);
				GL.Begin(1);
				GL.Color(new Color(1f, 1f, 1f, 0.25f));
				GL.Vertex3(rect.xMin, rect.y, 0f);
				GL.Vertex3(rect.xMax, rect.y, 0f);
				GL.Vertex3(rect.xMin, rect.y + rect.height * 0.5f, 0f);
				GL.Vertex3(rect.xMax, rect.y + rect.height * 0.5f, 0f);
				GL.Vertex3(rect.xMin, rect.yMax, 0f);
				GL.Vertex3(rect.xMax, rect.yMax, 0f);
				GL.End();
				GL.Begin(2);
				GL.Color(kColor_line_stepCount);
				int num = _curFrameIndex;
				int i = 0;
				int num2 = _physicsStepsHistory[num];
				float num3 = _physicsTimeHistory[num] * 30f;
				GL.Vertex3(rect.xMin, rect.y + num3 * rect.height, 0f);
				for (; i < 240; i++)
				{
					if (_physicsStepsHistory[num] != num2)
					{
						float num4 = (float)i / 240f;
						float num5 = (float)_physicsStepsHistory[num] * 0.5f;
						GL.Vertex3(rect.x + num4 * rect.width, rect.y + num3 * rect.height, 0f);
						GL.Vertex3(rect.x + num4 * rect.width, rect.y + num5 * rect.height, 0f);
						num2 = _physicsStepsHistory[num];
						num3 = num5;
					}
					num++;
					if (num >= 240)
					{
						num = 0;
					}
				}
				GL.Vertex3(rect.xMax, rect.y + num3 * rect.height, 0f);
				GL.End();
				GL.Begin(2);
				GL.Color(kColor_line_frameTime);
				num = _curFrameIndex;
				for (i = 0; i < 240; i++)
				{
					float num6 = (float)i / 240f;
					float num7 = _frameTimeHistory[num] * 30f;
					GL.Vertex3(rect.x + num6 * rect.width, rect.y + num7 * rect.height, 0f);
					num++;
					if (num >= 240)
					{
						num = 0;
					}
				}
				GL.End();
				GL.PopMatrix();
			}
			GUI.color = Color.white;
			GUI.Label(position4, _fpsLabel, _boldGUIStyle);
		}
	}
}
