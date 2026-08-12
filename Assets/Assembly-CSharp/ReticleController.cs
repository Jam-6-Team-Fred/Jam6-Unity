using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ReticleController : MonoBehaviour
{
	private Image _image;

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private Sprite _defaultReticle;

	[SerializeField]
	private Sprite _zeroGReticle;

	[SerializeField]
	private Sprite _probeLauncherReticle;

	[SerializeField]
	private Sprite _translatorReticle;

	private static bool s_hideReticle;

	private void Awake()
	{
		_image = GetComponent<Image>();
		_image.sprite = _defaultReticle;
		_image.enabled = false;
		s_hideReticle = false;
	}

	public static void Hide()
	{
		s_hideReticle = true;
	}

	public static void Show()
	{
		s_hideReticle = false;
	}

	private void LateUpdate()
	{
		if (s_hideReticle || Locator.GetPromptManager().IsCenterPromptDisplayed() || PlayerState.IsDead() || PlayerState.InConversation() || PlayerState.UsingShipComputer() || PlayerState.InLandingView() || OWTime.IsPaused(OWTime.PauseType.Menu) || !GUIMode.IsReticleVisible() || PlayerState.IsPlayerCameraLockingOn() || PlayerState.IsViewingProjector())
		{
			if (_canvas.enabled)
			{
				_canvas.enabled = false;
			}
			return;
		}
		if (!_canvas.enabled)
		{
			_canvas.enabled = true;
		}
		bool flag = true;
		Vector3 localScale = Vector3.one;
		if (PlayerState.InMapView())
		{
			_image.sprite = _zeroGReticle;
			_image.rectTransform.localScale = localScale;
			return;
		}
		switch (Locator.GetToolModeSwapper().GetToolMode())
		{
		case ToolMode.Probe:
			flag = true;
			_image.sprite = _probeLauncherReticle;
			break;
		case ToolMode.SignalScope:
			flag = false;
			break;
		case ToolMode.Translator:
			flag = true;
			_image.sprite = _translatorReticle;
			localScale = Vector3.one * Mathf.Lerp(1f, 3f, Mathf.Clamp01(NomaiTranslator.distToClosestTextCenter));
			break;
		default:
			flag = true;
			if (PlayerState.InZeroG())
			{
				_image.sprite = _zeroGReticle;
			}
			else
			{
				_image.sprite = _defaultReticle;
			}
			break;
		}
		if (_image.enabled != flag)
		{
			_image.enabled = flag;
		}
		_image.rectTransform.localScale = localScale;
		float t = Mathf.InverseLerp(1f, 5f, Time.timeSinceLevelLoad);
		Color color = _image.color;
		color.a = Mathf.Lerp(0f, 1f, t);
		_image.color = color;
	}
}
