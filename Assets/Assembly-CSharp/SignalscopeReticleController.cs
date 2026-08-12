using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignalscopeReticleController : MonoBehaviour
{
	[SerializeField]
	private Color _fullStrengthColor;

	[SerializeField]
	private Color _longDistanceColor;

	[SerializeField]
	private Sprite _fixedReticuleSprite;

	[SerializeField]
	private Sprite _dowsingBracketSprite;

	[SerializeField]
	private Transform _reticuleBracketsTransform;

	[SerializeField]
	private SkinnedMeshRenderer _dowsingBracketRTemplate;

	[SerializeField]
	private SkinnedMeshRenderer _dowsingBracketLTemplate;

	[SerializeField]
	private Text _scopeDistanceText;

	private float _minScopeReticleDistanceFromCenter;

	private float _maxScopeReticleDistanceFromCenter = 1f;

	private Color _defaultColor;

	private string _longDistanceHexColor;

	private List<SkinnedMeshRenderer> _clonedRightBrackets;

	private List<SkinnedMeshRenderer> _clonedLeftBrackets;

	private List<Material> _materialInstances;

	private Signalscope _sigScopeTool;

	[SerializeField]
	private Canvas _canvas;

	private const float LONG_DISTANCE_THRESHOLD = 1000f;

	private void Awake()
	{
		_clonedRightBrackets = new List<SkinnedMeshRenderer>();
		_clonedLeftBrackets = new List<SkinnedMeshRenderer>();
		_materialInstances = new List<Material>();
		_dowsingBracketRTemplate.gameObject.SetActive(value: false);
		_dowsingBracketLTemplate.gameObject.SetActive(value: false);
		_scopeDistanceText.enabled = false;
		_defaultColor = _dowsingBracketRTemplate.sharedMaterial.color;
		_longDistanceHexColor = ColorUtility.ToHtmlStringRGBA(_longDistanceColor);
		_canvas.enabled = false;
	}

	private void Start()
	{
		Locator.GetPlayerTransform().GetComponentInChildren<HUDCanvas>().RegisterSignalscopeReticuleController(this);
		if (Locator.GetShipTransform() != null)
		{
			Locator.GetShipTransform().GetComponentInChildren<ShipCockpitUI>().RegisterSignalscopeReticuleController(this);
		}
	}

	public void UpdateSignalscopeReticuleCanvases()
	{
		if (_sigScopeTool == null)
		{
			_sigScopeTool = Locator.GetToolModeSwapper().GetSignalScope();
		}
		_scopeDistanceText.enabled = true;
		List<AudioSignal> audioSignals = Locator.GetAudioSignals();
		UpdateText(audioSignals);
	}

	public void UpdateSignalscopeReticule()
	{
		if (_sigScopeTool == null)
		{
			_sigScopeTool = Locator.GetToolModeSwapper().GetSignalScope();
		}
		List<AudioSignal> audioSignals = Locator.GetAudioSignals();
		UpdateBrackets(audioSignals);
	}

	private void UpdateBrackets(List<AudioSignal> listSignals)
	{
		while (_clonedLeftBrackets.Count < listSignals.Count)
		{
			GameObject gameObject = Object.Instantiate(_dowsingBracketLTemplate.gameObject, _reticuleBracketsTransform);
			GameObject obj = Object.Instantiate(_dowsingBracketRTemplate.gameObject, _reticuleBracketsTransform);
			gameObject.SetActive(value: true);
			obj.SetActive(value: true);
			SkinnedMeshRenderer component = gameObject.GetComponent<SkinnedMeshRenderer>();
			SkinnedMeshRenderer component2 = obj.GetComponent<SkinnedMeshRenderer>();
			Material item = (component2.material = (component.material = gameObject.GetComponent<SkinnedMeshRenderer>().material));
			_clonedLeftBrackets.Add(component);
			_clonedRightBrackets.Add(component2);
			_materialInstances.Add(item);
		}
		for (int i = 0; i < listSignals.Count; i++)
		{
			float signalStrength = listSignals[i].GetSignalStrength();
			if (signalStrength > 0f)
			{
				float value = Mathf.Clamp(signalStrength * 100f, 0f, 100f);
				float num = Mathf.Lerp(_minScopeReticleDistanceFromCenter, _maxScopeReticleDistanceFromCenter, 1f - signalStrength);
				_clonedLeftBrackets[i].transform.localPosition = new Vector3(num * -1f, 0f, 0f);
				_clonedRightBrackets[i].transform.localPosition = new Vector3(num, 0f, 0f);
				_clonedLeftBrackets[i].SetBlendShapeWeight(0, value);
				_clonedRightBrackets[i].SetBlendShapeWeight(0, value);
				Color color = _defaultColor;
				float num2 = Mathf.InverseLerp(1000f, 500f, listSignals[i].GetDistanceFromScope());
				float num3 = Mathf.Lerp(0.5f, 0.8f, num2 * num2);
				if (num2 <= 0f)
				{
					color = _longDistanceColor;
				}
				if (signalStrength == 1f)
				{
					color = _fullStrengthColor;
					color.a = 1f;
				}
				else
				{
					color.a = Mathf.Lerp(0.2f, 0.7f, signalStrength);
				}
				_clonedLeftBrackets[i].transform.localScale = new Vector3(num3, num3, num3);
				_clonedRightBrackets[i].transform.localScale = new Vector3(0f - num3, num3, num3);
				_materialInstances[i].color = color;
				_clonedLeftBrackets[i].enabled = true;
				_clonedRightBrackets[i].enabled = true;
			}
			else
			{
				_clonedLeftBrackets[i].enabled = false;
				_clonedRightBrackets[i].enabled = false;
			}
		}
	}

	private void UpdateText(List<AudioSignal> listSignals)
	{
		string text = string.Empty;
		if (!PlayerState.AtFlightConsole())
		{
			AudioSignal strongestSignal = _sigScopeTool.GetStrongestSignal();
			if (strongestSignal != null && strongestSignal.GetSignalStrength() > SignalscopeUI.s_distanceTextThreshold)
			{
				float distanceFromScope = strongestSignal.GetDistanceFromScope();
				string text2 = "m";
				string text3 = ((distanceFromScope > 1000f) ? _longDistanceHexColor : "ffffffff");
				string text4 = (PlayerData.KnowsSignal(strongestSignal.GetName()) ? AudioSignal.SignalNameToString(strongestSignal.GetName()) : UITextLibrary.GetString(UITextType.UnknownSignal));
				text = text4 + ": <color=#" + text3 + ">" + Mathf.Round(distanceFromScope) + text2 + "</color>";
			}
		}
		_scopeDistanceText.text = text;
	}

	public void SetUIActive(bool value)
	{
		_reticuleBracketsTransform.gameObject.SetActive(value);
		_scopeDistanceText.gameObject.SetActive(value);
		_canvas.enabled = value;
	}
}
