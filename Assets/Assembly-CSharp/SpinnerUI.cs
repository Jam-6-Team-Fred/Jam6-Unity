using UnityEngine;

public class SpinnerUI : MonoBehaviour
{
	private static SpinnerUI s_instance;

	private static int s_spinnerActiveCount;

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private RectTransform _spinnerTransform;

	[SerializeField]
	private float _spinnerSpeed = -180f;

	private float _rotation;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Instantiate()
	{
		if (!(s_instance == null))
		{
			return;
		}
		GameObject gameObject = Resources.Load<GameObject>("Prefabs/UI/SpinnerUI");
		if (gameObject == null)
		{
			Debug.LogError("Unable to load spinner prefab!");
			return;
		}
		GameObject gameObject2 = Object.Instantiate(gameObject);
		s_instance = gameObject2.GetComponent<SpinnerUI>();
		if (s_instance == null)
		{
			Debug.LogError("Unable to find SpinnerUI Component on spinner prefab!");
			Object.Destroy(gameObject2);
		}
		else
		{
			Object.DontDestroyOnLoad(gameObject2);
			gameObject2.hideFlags = HideFlags.NotEditable;
		}
	}

	private void Awake()
	{
		base.enabled = s_spinnerActiveCount > 0;
	}

	private void OnEnable()
	{
		_canvas.enabled = true;
		_rotation = 0f;
		Canvas.willRenderCanvases += OnWillRenderCanvases;
	}

	private void OnDisable()
	{
		_canvas.enabled = false;
		_rotation = 0f;
		Canvas.willRenderCanvases -= OnWillRenderCanvases;
	}

	private void LateUpdate()
	{
		_rotation += _spinnerSpeed * Mathf.Min(Time.unscaledDeltaTime, 0.0333333f);
		_rotation = Mathf.Repeat(_rotation, 360f);
	}

	public static void Show()
	{
		if (s_instance != null && s_spinnerActiveCount == 0)
		{
			s_instance.enabled = true;
		}
		s_spinnerActiveCount++;
	}

	public static void Hide()
	{
		if (s_spinnerActiveCount != 0)
		{
			s_spinnerActiveCount--;
			if (s_instance != null && s_spinnerActiveCount == 0)
			{
				s_instance.enabled = false;
			}
		}
	}

	private void OnWillRenderCanvases()
	{
		_spinnerTransform.localRotation = Quaternion.Euler(0f, 0f, _rotation);
	}
}
