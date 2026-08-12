using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ScreenPromptElement : MonoBehaviour
{
	public const string IMAGE_MARKER_STRING = "<CMD>";

	public const string IMAGE_MARKER_STRING_N1 = "<CMD1>";

	public const string IMAGE_MARKER_STRING_N2 = "<CMD2>";

	public const string EYE_COORDS_IMAGE_MARKER_STRING = "<EYE>";

	public const string c_textObjectName = "Text";

	private string _textStr;

	public Color _textColor;

	private ScreenPrompt _promptData;

	private TextAnchor _textAlignment;

	private Font _font;

	private float _fontSize;

	private float _customLineSpacing = float.MinValue;

	private List<GameObject> _children;

	private bool _initialized;

	private bool _isDisplayed;

	private bool _displayedLastFrame;

	private bool _firstTimeDisplay = true;

	private float _preferredIconSize;

	private const float _minimumAttentionStateScale = 1f;

	private const float _maximumAttentionStateScale = 1.1f;

	private const float _attentionScaleLoopTime = 1f;

	private bool _isTextScrolling;

	private bool _displayTextInReverse;

	private int _iPromptCharacterLength;

	private bool _isWasdPrompt;

	private LayoutElement _layoutWasdBlock;

	private LayoutElement[] _layoutWasdImages;

	private LayoutElement _eyeCoordsImgLayoutElement;

	private float _eyeCoordsAspectRatio;

	private bool _requiresTextRebuild;

	private bool _requiresFullRebuild;

	private static float s_timeOfLastRebuild;

	public static GameObject CreateNewScreenPrompt(ScreenPrompt data, int fontSize, Font font, Transform parent = null, TextAnchor textAlignment = TextAnchor.MiddleLeft)
	{
		GameObject gameObject = new GameObject("ScreenPrompt", typeof(RectTransform));
		if (parent != null)
		{
			gameObject.transform.SetParent(parent, worldPositionStays: true);
		}
		ScreenPromptElement screenPromptElement = gameObject.AddComponent<ScreenPromptElement>();
		Vector2 pivotPoint = Vector2.zero;
		switch (textAlignment)
		{
		case TextAnchor.LowerCenter:
			pivotPoint = new Vector2(0.5f, 0f);
			break;
		case TextAnchor.LowerRight:
			pivotPoint = new Vector2(1f, 0f);
			break;
		case TextAnchor.MiddleLeft:
			pivotPoint = new Vector2(0f, 0.5f);
			break;
		case TextAnchor.MiddleCenter:
			pivotPoint = new Vector2(0.5f, 0.5f);
			break;
		case TextAnchor.MiddleRight:
			pivotPoint = new Vector2(1f, 0.5f);
			break;
		case TextAnchor.UpperLeft:
			pivotPoint = new Vector2(0f, 1f);
			break;
		case TextAnchor.UpperCenter:
			pivotPoint = new Vector2(0.5f, 1f);
			break;
		case TextAnchor.UpperRight:
			pivotPoint = Vector2.one;
			break;
		}
		screenPromptElement.InitializeScreenPrompt(data, fontSize, font, textAlignment, pivotPoint);
		return gameObject;
	}

	public ScreenPrompt GetPromptData()
	{
		return _promptData;
	}

	public void InitializeScreenPrompt(ScreenPrompt data, int fontSize, Font font, TextAnchor textAlignment, Vector2 pivotPoint)
	{
		_isDisplayed = false;
		_displayedLastFrame = false;
		_children = new List<GameObject>();
		_promptData = data;
		Color textColor = OWUtilities.GetDefaultColor();
		if (_promptData != null)
		{
			textColor = _promptData.GetTextColor();
			_textStr = _promptData.GetText();
		}
		_font = font;
		_textAlignment = textAlignment;
		_fontSize = fontSize;
		_textColor = textColor;
		HorizontalLayoutGroup horizontalLayoutGroup = base.gameObject.AddComponent<HorizontalLayoutGroup>();
		horizontalLayoutGroup.childAlignment = _textAlignment;
		horizontalLayoutGroup.childForceExpandHeight = false;
		horizontalLayoutGroup.childForceExpandWidth = false;
		horizontalLayoutGroup.spacing = 5f;
		ZeroTransformValues(base.gameObject);
		this.GetRequiredComponent<RectTransform>().pivot = pivotPoint;
		_requiresFullRebuild = true;
		s_timeOfLastRebuild = Time.unscaledTime;
		BuildScreenPrompt();
		_requiresFullRebuild = false;
		_initialized = true;
	}

	private void Awake()
	{
		Canvas.willRenderCanvases += OnWillRenderCanvases;
	}

	private void OnDestroy()
	{
		Canvas.willRenderCanvases -= OnWillRenderCanvases;
	}

	private void Update()
	{
		if (_initialized)
		{
			if (_requiresFullRebuild && Time.unscaledTime - s_timeOfLastRebuild > ButtonPromptLibrary.BUTTONIMAGE_MIN_REFRESH_TIME)
			{
				DoRebuildUIElementFromPrompt();
				s_timeOfLastRebuild = Time.unscaledTime;
			}
			if (_requiresTextRebuild && Time.unscaledTime - s_timeOfLastRebuild > ButtonPromptLibrary.BUTTONIMAGE_MIN_REFRESH_TIME)
			{
				DoRebuildUIElementFromString();
				s_timeOfLastRebuild = Time.unscaledTime;
			}
		}
	}

	private void OnWillRenderCanvases()
	{
		if (!_initialized)
		{
			return;
		}
		if (base.gameObject.activeSelf && !_isDisplayed)
		{
			_isDisplayed = true;
		}
		if (_isDisplayed)
		{
			if (_firstTimeDisplay)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetRequiredComponent<RectTransform>());
				MaskableGraphic[] componentsInChildren = GetComponentsInChildren<MaskableGraphic>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = true;
				}
				_firstTimeDisplay = false;
			}
			if (_promptData.IsDisplayState(ScreenPrompt.DisplayState.Attention))
			{
				_promptData.SetTextColor((Mathf.Sin(Time.unscaledTime * 10f) > 0f) ? _promptData.GetDefaultColor() : new Color(1f, 1f, 1f, 0.1f));
			}
			else if (_promptData.IsDisplayState(ScreenPrompt.DisplayState.GrayedOut))
			{
				for (int j = 0; j < _children.Count; j++)
				{
					Image component = _children[j].GetComponent<Image>();
					if (component != null)
					{
						component.color = new Color(1f, 1f, 1f, 0.2f);
					}
				}
				_promptData.SetTextColor(new Color(1f, 1f, 1f, 0.2f));
			}
			else
			{
				base.transform.localScale = Vector3.one;
				for (int k = 0; k < _children.Count; k++)
				{
					Image component2 = _children[k].GetComponent<Image>();
					if (component2 != null)
					{
						component2.color = new Color(1f, 1f, 1f, 1f);
					}
				}
				_promptData.SetTextColor(_promptData.GetDefaultColor());
			}
		}
		if (_isTextScrolling)
		{
			bool isTextScrolling = false;
			for (int l = 0; l < _children.Count; l++)
			{
				GameObject gameObject = ((!_displayTextInReverse) ? _children[l] : _children[_children.Count - 1 - l]);
				TypeEffectText component3 = gameObject.GetComponent<TypeEffectText>();
				Image component4 = gameObject.GetComponent<Image>();
				if (gameObject.activeSelf)
				{
					if (component3 != null && component3.IsTextEffectRunning())
					{
						isTextScrolling = true;
						break;
					}
					continue;
				}
				isTextScrolling = true;
				gameObject.SetActive(value: true);
				if (component4 != null)
				{
					component4.SetAllDirty();
				}
				if (component3 != null && !component3.IsTextEffectComplete())
				{
					component3.StartTextEffect();
				}
				break;
			}
			_isTextScrolling = isTextScrolling;
		}
		_displayedLastFrame = _isDisplayed;
	}

	public void SetPreferredIconHeightAndWidth(float preferredHeightAndWidth)
	{
		_preferredIconSize = preferredHeightAndWidth;
		if (_eyeCoordsImgLayoutElement != null)
		{
			_eyeCoordsImgLayoutElement.preferredHeight = preferredHeightAndWidth;
			_eyeCoordsImgLayoutElement.preferredWidth = _eyeCoordsAspectRatio * preferredHeightAndWidth;
		}
		if (_isWasdPrompt)
		{
			float num = ((_preferredIconSize != 0f) ? ((float)Mathf.RoundToInt(_preferredIconSize * 2f)) : ((float)Mathf.RoundToInt(this.GetRequiredComponent<RectTransform>().rect.width * 0.2f)));
			if (_layoutWasdBlock != null)
			{
				_layoutWasdBlock.preferredWidth = num;
				float num2 = Mathf.RoundToInt(num / 3f);
				for (int i = 0; i < _layoutWasdImages.Length; i++)
				{
					_layoutWasdImages[i].preferredWidth = num2;
					_layoutWasdImages[i].preferredHeight = num2;
				}
			}
		}
		else
		{
			Image[] componentsInChildren = base.gameObject.GetComponentsInChildren<Image>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				Vector2 vector = ButtonPromptLibrary.AdjustButtonImageSize(componentsInChildren[j].sprite.texture, preferredHeightAndWidth);
				LayoutElement component = componentsInChildren[j].GetComponent<LayoutElement>();
				component.preferredWidth = vector.x;
				component.preferredHeight = vector.y;
			}
		}
	}

	public void SetCustomLineSpacing(float lineSpacing)
	{
		_customLineSpacing = lineSpacing;
		float num = 1f;
		num = ((_customLineSpacing != float.MinValue) ? _customLineSpacing : TextTranslation.GetDefaultFontSpacing());
		Text[] componentsInChildren = base.transform.GetComponentsInChildren<Text>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].lineSpacing = num;
		}
		RefreshUIElementLayout();
	}

	public void ResetCustomLineSpacing()
	{
		SetCustomLineSpacing(float.MinValue);
	}

	public float GetAttentionStateMaxScale()
	{
		return 1.1f;
	}

	private void RefreshUIElementLayout()
	{
		Graphic[] componentsInChildren = base.transform.GetComponentsInChildren<Graphic>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetLayoutDirty();
		}
	}

	public void RefreshUIElement()
	{
		if (_promptData.GetTextColor() != _textColor)
		{
			_textColor = _promptData.GetTextColor();
			Text[] componentsInChildren = base.transform.GetComponentsInChildren<Text>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].color = _textColor;
			}
		}
		OnPromptDataVisibilityUpdate();
		Graphic[] componentsInChildren2 = base.transform.GetComponentsInChildren<Graphic>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].SetAllDirty();
		}
	}

	private void OnPromptDataVisibilityUpdate()
	{
		bool flag = false;
		if (_promptData.IsVisible())
		{
			ScreenPromptList componentInParent = base.transform.GetComponentInParent<ScreenPromptList>();
			if (componentInParent != null && _promptData.GetPriority() == componentInParent.GetHighestVisiblePriority())
			{
				flag = true;
			}
		}
		if (!_displayedLastFrame && flag && _promptData.GetScrollsOnVisble())
		{
			_isTextScrolling = true;
			for (int i = 0; i < _children.Count; i++)
			{
				_children[i].SetActive(value: false);
				TypeEffectText component = _children[i].GetComponent<TypeEffectText>();
				if (component != null)
				{
					component.StopAndClearText();
				}
			}
		}
		if (_firstTimeDisplay && flag)
		{
			MaskableGraphic[] componentsInChildren = GetComponentsInChildren<MaskableGraphic>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enabled = false;
			}
		}
		base.gameObject.SetActive(flag);
		if (!flag)
		{
			_displayedLastFrame = false;
			_isDisplayed = false;
		}
	}

	private void ZeroTransformValues(GameObject go)
	{
		go.transform.localScale = Vector3.one;
		go.transform.localRotation = Quaternion.identity;
		go.GetRequiredComponent<RectTransform>().SetLocalPositionZ(0f);
	}

	private List<string> PartitionTextForScreenPrompt(string promptText, ScreenPrompt.MultiCommandType commandType)
	{
		if (commandType == ScreenPrompt.MultiCommandType.CUSTOM_BOTH)
		{
			return BuildTwoCommandScreenPrompt(promptText);
		}
		return BuildDefaultScreenPrompt(promptText);
	}

	private List<string> BuildTwoCommandScreenPrompt(string promptText)
	{
		int num = promptText.IndexOf("<CMD1>");
		int num2 = promptText.IndexOf("<CMD2>");
		List<Sprite> spriteList = _promptData.GetSpriteList(_requiresFullRebuild);
		if (num == -1 || num2 == -1)
		{
			Debug.LogError("Markup tags <CMD1> and <CMD2> are required for this prompt");
		}
		if (spriteList.Count < 2)
		{
			Debug.LogError("Not enough sprites for 2 command prompt");
		}
		string text;
		string text2;
		if (num2 < num)
		{
			int num3 = num;
			num = num2;
			num2 = num3;
			text = "<CMD2>";
			text2 = "<CMD1>";
		}
		else
		{
			text = "<CMD1>";
			text2 = "<CMD2>";
		}
		List<string> list = new List<string>();
		string text3 = promptText.Substring(0, num);
		if (text3 != string.Empty)
		{
			list.Add(text3);
		}
		list.Add(text);
		text3 = promptText.Substring(num + text.Length, num2 - num - text.Length);
		if (text3 != string.Empty)
		{
			list.Add(text3);
		}
		list.Add(text2);
		text3 = promptText.Substring(num2 + text2.Length);
		if (text3 != string.Empty)
		{
			list.Add(text3);
		}
		return list;
	}

	private List<string> BuildDefaultScreenPrompt(string promptText)
	{
		List<string> stringList = new List<string>();
		ScreenPrompt.MultiCommandType multiCommandType = _promptData.GetMultiCommandType();
		List<Sprite> spriteList = _promptData.GetSpriteList(_requiresFullRebuild);
		bool twoCommandImages = spriteList.Count == 2;
		int num = promptText.IndexOf("<CMD>");
		if (num == -1)
		{
			int num2 = promptText.IndexOf("<EYE>");
			if (num2 == -1)
			{
				if (spriteList.Count > 0)
				{
					if (_textAlignment == TextAnchor.LowerRight || _textAlignment == TextAnchor.MiddleRight || _textAlignment == TextAnchor.UpperRight)
					{
						stringList.Add(promptText);
						BuildInCommandImage(ref stringList, twoCommandImages, multiCommandType);
					}
					else
					{
						BuildInCommandImage(ref stringList, twoCommandImages, multiCommandType);
						stringList.Add(promptText);
					}
				}
				else
				{
					stringList.Add(promptText);
				}
			}
			else
			{
				string text = promptText.Substring(0, num2);
				if (text != string.Empty)
				{
					stringList.Add(text);
				}
				stringList.Add("<EYE>");
				text = promptText.Substring(num2 + "<EYE>".Length);
				if (text != string.Empty)
				{
					stringList.Add(text);
				}
			}
		}
		else
		{
			string text2 = promptText.Substring(0, num);
			if (text2 != string.Empty)
			{
				stringList.Add(text2);
			}
			BuildInCommandImage(ref stringList, twoCommandImages, multiCommandType);
			text2 = promptText.Substring(num + "<CMD>".Length);
			if (text2 != string.Empty)
			{
				stringList.Add(text2);
			}
		}
		return stringList;
	}

	private void BuildInCommandImage(ref List<string> stringList, bool twoCommandImages, ScreenPrompt.MultiCommandType commandType)
	{
		if (twoCommandImages)
		{
			switch (commandType)
			{
			case ScreenPrompt.MultiCommandType.HOLD_BOTH:
				stringList.Add("<CMD>");
				stringList.Add("+");
				stringList.Add("<CMD>");
				break;
			case ScreenPrompt.MultiCommandType.POS_NEG:
				stringList.Add("<CMD>");
				stringList.Add("<CMD>");
				break;
			case ScreenPrompt.MultiCommandType.NONE:
				stringList.Add("<CMD>");
				stringList.Add("<CMD>");
				break;
			case ScreenPrompt.MultiCommandType.HOLD_ONE_AND_PRESS_2ND:
				stringList.Add(UITextLibrary.GetString(UITextType.HoldPrompt));
				stringList.Add("<CMD>");
				stringList.Add("+" + UITextLibrary.GetString(UITextType.PressPrompt));
				stringList.Add("<CMD>");
				break;
			case ScreenPrompt.MultiCommandType.CUSTOM_BOTH:
				break;
			}
		}
		else
		{
			stringList.Add("<CMD>");
		}
	}

	private void BuildScreenPromptGfx(List<string> stringStuff, bool commandIsWASD = false)
	{
		_iPromptCharacterLength = 0;
		List<Sprite> spriteList = _promptData.GetSpriteList(_requiresFullRebuild);
		int num = 0;
		if (_textAlignment == TextAnchor.UpperRight || _textAlignment == TextAnchor.MiddleRight || _textAlignment == TextAnchor.LowerRight)
		{
			_displayTextInReverse = true;
		}
		else
		{
			_displayTextInReverse = false;
		}
		for (int i = 0; i < stringStuff.Count; i++)
		{
			if (stringStuff[i] == "<CMD1>" || stringStuff[i] == "<CMD>")
			{
				_iPromptCharacterLength++;
				if (commandIsWASD)
				{
					_isWasdPrompt = true;
					_layoutWasdImages = new LayoutElement[4];
					GameObject gameObject = new GameObject("ImageBlock", typeof(RectTransform));
					gameObject.transform.SetParent(base.transform);
					VerticalLayoutGroup verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
					verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
					verticalLayoutGroup.childForceExpandHeight = false;
					verticalLayoutGroup.childForceExpandWidth = false;
					ZeroTransformValues(gameObject);
					_layoutWasdBlock = gameObject.AddComponent<LayoutElement>();
					float num2 = ((_preferredIconSize != 0f) ? ((float)Mathf.RoundToInt(_preferredIconSize * 2f)) : ((float)Mathf.RoundToInt(this.GetRequiredComponent<RectTransform>().rect.width * 0.2f)));
					_layoutWasdBlock.preferredWidth = num2;
					_children.Add(gameObject);
					float num3 = Mathf.RoundToInt(num2 / 3f);
					GameObject gameObject2 = new GameObject("Top", typeof(RectTransform));
					gameObject2.transform.SetParent(gameObject.transform);
					HorizontalLayoutGroup horizontalLayoutGroup = gameObject2.AddComponent<HorizontalLayoutGroup>();
					horizontalLayoutGroup.childAlignment = TextAnchor.LowerCenter;
					horizontalLayoutGroup.childForceExpandHeight = false;
					horizontalLayoutGroup.childForceExpandWidth = false;
					ZeroTransformValues(gameObject2);
					LayoutElement layoutElement = gameObject2.AddComponent<LayoutElement>();
					GameObject gameObject3 = new GameObject("UpImage", typeof(RectTransform));
					gameObject3.transform.SetParent(gameObject2.transform);
					Image image = gameObject3.AddComponent<Image>();
					image.sprite = spriteList[0];
					image.preserveAspect = true;
					ZeroTransformValues(gameObject3);
					layoutElement = gameObject3.AddComponent<LayoutElement>();
					layoutElement.preferredWidth = num3;
					layoutElement.preferredHeight = num3;
					_layoutWasdImages[0] = layoutElement;
					GameObject gameObject4 = new GameObject("Bottom", typeof(RectTransform));
					gameObject4.transform.SetParent(gameObject.transform);
					HorizontalLayoutGroup horizontalLayoutGroup2 = gameObject4.AddComponent<HorizontalLayoutGroup>();
					horizontalLayoutGroup2.childAlignment = TextAnchor.UpperCenter;
					horizontalLayoutGroup2.childForceExpandHeight = false;
					horizontalLayoutGroup2.childForceExpandWidth = false;
					ZeroTransformValues(gameObject4);
					layoutElement = gameObject4.AddComponent<LayoutElement>();
					GameObject gameObject5 = new GameObject("LeftImage", typeof(RectTransform));
					gameObject5.transform.SetParent(gameObject4.transform);
					Image image2 = gameObject5.AddComponent<Image>();
					image2.sprite = spriteList[2];
					image2.preserveAspect = true;
					ZeroTransformValues(gameObject5);
					layoutElement = gameObject5.AddComponent<LayoutElement>();
					layoutElement.preferredWidth = num3;
					layoutElement.preferredHeight = num3;
					_layoutWasdImages[1] = layoutElement;
					GameObject gameObject6 = new GameObject("DownImage", typeof(RectTransform));
					gameObject6.transform.SetParent(gameObject4.transform);
					Image image3 = gameObject6.AddComponent<Image>();
					image3.sprite = spriteList[1];
					image3.preserveAspect = true;
					ZeroTransformValues(gameObject6);
					layoutElement = gameObject6.AddComponent<LayoutElement>();
					layoutElement.preferredWidth = num3;
					layoutElement.preferredHeight = num3;
					_layoutWasdImages[2] = layoutElement;
					GameObject gameObject7 = new GameObject("RightImage", typeof(RectTransform));
					gameObject7.transform.SetParent(gameObject4.transform);
					Image image4 = gameObject7.AddComponent<Image>();
					image4.sprite = spriteList[3];
					image4.preserveAspect = true;
					ZeroTransformValues(gameObject7);
					layoutElement = gameObject7.AddComponent<LayoutElement>();
					layoutElement.preferredWidth = num3;
					layoutElement.preferredHeight = num3;
					_layoutWasdImages[3] = layoutElement;
				}
				else
				{
					GameObject gameObject8 = new GameObject("CommandImage", typeof(RectTransform));
					gameObject8.transform.SetParent(base.transform);
					Image image5 = gameObject8.AddComponent<Image>();
					LayoutElement layoutElement = gameObject8.AddComponent<LayoutElement>();
					if (_preferredIconSize != 0f)
					{
						layoutElement.preferredHeight = _preferredIconSize;
					}
					image5.SetLayoutDirty();
					if (stringStuff[i] == "<CMD>")
					{
						image5.sprite = spriteList[num];
						num++;
					}
					else
					{
						image5.sprite = spriteList[0];
					}
					image5.preserveAspect = true;
					Vector2 vector = ButtonPromptLibrary.AdjustButtonImageSize(image5.sprite.texture, _preferredIconSize);
					layoutElement.preferredHeight = vector.y;
					layoutElement.preferredWidth = vector.x;
					ApplySteamdeckJank(image5, image5.sprite.texture.name);
					ZeroTransformValues(gameObject8);
					_children.Add(gameObject8);
				}
			}
			else if (stringStuff[i] == "<CMD2>")
			{
				_iPromptCharacterLength++;
				GameObject gameObject9 = new GameObject("CommandImage", typeof(RectTransform));
				gameObject9.transform.SetParent(base.transform);
				Image image6 = gameObject9.AddComponent<Image>();
				LayoutElement layoutElement = gameObject9.AddComponent<LayoutElement>();
				if (_preferredIconSize != 0f)
				{
					layoutElement.preferredHeight = _preferredIconSize;
				}
				image6.SetLayoutDirty();
				image6.sprite = spriteList[1];
				image6.preserveAspect = true;
				Vector2 vector2 = ButtonPromptLibrary.AdjustButtonImageSize(image6.sprite.texture, _preferredIconSize);
				layoutElement.preferredHeight = vector2.y;
				layoutElement.preferredWidth = vector2.x;
				ApplySteamdeckJank(image6, spriteList[1].texture.name);
				ZeroTransformValues(gameObject9);
				_children.Add(gameObject9);
			}
			else if (stringStuff[i] == "<EYE>")
			{
				GameObject gameObject10 = new GameObject("EyeCoords", typeof(RectTransform));
				gameObject10.transform.SetParent(base.transform);
				Image image7 = gameObject10.AddComponent<Image>();
				image7.SetLayoutDirty();
				image7.sprite = spriteList[0];
				image7.preserveAspect = true;
				ZeroTransformValues(gameObject10);
				_children.Add(gameObject10);
				_eyeCoordsImgLayoutElement = gameObject10.AddComponent<LayoutElement>();
				_eyeCoordsAspectRatio = image7.sprite.rect.width / image7.sprite.rect.height;
				_eyeCoordsImgLayoutElement.preferredHeight = _preferredIconSize;
				_eyeCoordsImgLayoutElement.preferredWidth = _eyeCoordsAspectRatio * _preferredIconSize;
			}
			else if (stringStuff[i] != string.Empty)
			{
				GameObject gameObject11 = new GameObject("Text", typeof(RectTransform));
				gameObject11.transform.SetParent(base.transform);
				Text text = gameObject11.AddComponent<Text>();
				TypeEffectText typeEffectText = gameObject11.AddComponent<TypeEffectText>();
				typeEffectText.Init(stringStuff[i], _textAlignment, TextSpeed.Fast);
				if (!_promptData.GetScrollsOnVisble())
				{
					typeEffectText.SetTextWithNoEffect();
				}
				text.font = _font;
				text.fontSize = Mathf.RoundToInt(_fontSize);
				if (_customLineSpacing == float.MinValue)
				{
					text.lineSpacing = TextTranslation.GetDefaultFontSpacing();
				}
				else
				{
					text.lineSpacing = _customLineSpacing;
				}
				text.color = _textColor;
				text.horizontalOverflow = HorizontalWrapMode.Overflow;
				ZeroTransformValues(gameObject11);
				_children.Add(gameObject11);
				_iPromptCharacterLength += typeEffectText.GetTotalVisibleTextLength();
			}
		}
	}

	private void ApplySteamdeckJank(Image img, string imgName)
	{
		switch (imgName)
		{
		default:
			if (!imgName.StartsWith("SteamDeck_Left_Stick") && !imgName.StartsWith("SteamDeck_Right_Stick"))
			{
				break;
			}
			goto case "SteamDeck_R1";
		case "SteamDeck_R1":
		case "SteamDeck_R2":
		case "SteamDeck_L1":
		case "SteamDeck_L2":
		{
			LayoutElement addComponent = img.gameObject.GetAddComponent<LayoutElement>();
			addComponent.minWidth = _preferredIconSize * 1.5f;
			addComponent.minHeight = addComponent.minWidth;
			LayoutValueSetter component = img.gameObject.GetComponent<LayoutValueSetter>();
			if (component != null)
			{
				component.enabled = false;
				Object.Destroy(component);
			}
			break;
		}
		}
	}

	private ScreenPrompt.MultiCommandType DetermineCommandType()
	{
		List<Sprite> spriteList = _promptData.GetSpriteList(_requiresFullRebuild);
		ScreenPrompt.MultiCommandType result = _promptData.GetMultiCommandType();
		if (spriteList.Count == 4)
		{
			result = ScreenPrompt.MultiCommandType.WASD;
		}
		return result;
	}

	private void BuildScreenPrompt()
	{
		ScreenPrompt.MultiCommandType multiCommandType = DetermineCommandType();
		List<string> list = PartitionTextForScreenPrompt(_textStr, multiCommandType);
		BuildScreenPromptGfx(list, multiCommandType == ScreenPrompt.MultiCommandType.WASD);
		if (_textAlignment == TextAnchor.UpperRight || _textAlignment == TextAnchor.MiddleRight || _textAlignment == TextAnchor.LowerRight)
		{
			list.Reverse();
		}
		_firstTimeDisplay = true;
	}

	public void UpdateText(string updatedText)
	{
		ScreenPrompt.MultiCommandType commandType = DetermineCommandType();
		List<string> list = PartitionTextForScreenPrompt(_textStr, commandType);
		List<string> list2 = PartitionTextForScreenPrompt(updatedText, commandType);
		bool flag = false;
		if (list.Count != list2.Count)
		{
			flag = true;
		}
		else if (list.Count != _children.Count)
		{
			flag = true;
		}
		else
		{
			bool flag2 = false;
			bool flag3 = false;
			for (int i = 0; i < list.Count; i++)
			{
				flag2 = false;
				flag3 = false;
				if (list[i] == "<CMD>" || list[i] == "<CMD1>" || list[i] == "<CMD2>" || list[i] == "<EYE>")
				{
					flag2 = true;
				}
				if (list2[i] == "<CMD>" || list2[i] == "<CMD1>" || list2[i] == "<CMD2>" || list2[i] == "<EYE>")
				{
					flag3 = true;
				}
				if (flag2 != flag3)
				{
					flag = true;
					break;
				}
				if (!flag2)
				{
					Text component = _children[i].GetComponent<Text>();
					if (component != null)
					{
						component.text = list2[i];
					}
				}
			}
		}
		if (flag)
		{
			RebuildUIElementFromString(updatedText);
		}
		else
		{
			RefreshUIElement();
		}
	}

	public void RebuildUIElementFromString(string updatedText)
	{
		_textStr = updatedText;
		_requiresTextRebuild = true;
	}

	private void DoRebuildUIElementFromString()
	{
		for (int i = 0; i < _children.Count; i++)
		{
			Object.Destroy(_children[i]);
		}
		_children.Clear();
		OnPromptDataVisibilityUpdate();
		BuildScreenPrompt();
		_requiresTextRebuild = false;
	}

	public void RebuildUIElementFromPrompt()
	{
		_requiresFullRebuild = true;
	}

	public void RefreshUIElementSize(float updatedPreferredHeightAndWidth, int updatedFontSize)
	{
		SetPreferredIconHeightAndWidth(updatedPreferredHeightAndWidth);
		_fontSize = updatedFontSize;
		int fontSize = Mathf.RoundToInt(_fontSize);
		Text[] componentsInChildren = base.transform.GetComponentsInChildren<Text>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].fontSize = fontSize;
		}
		RefreshUIElementLayout();
	}

	private void DoRebuildUIElementFromPrompt()
	{
		for (int i = 0; i < _children.Count; i++)
		{
			Object.Destroy(_children[i]);
		}
		_children.Clear();
		OnPromptDataVisibilityUpdate();
		if (_promptData != null)
		{
			_textStr = _promptData.GetText();
		}
		BuildScreenPrompt();
		_requiresFullRebuild = false;
	}

	public bool IsDisplayed()
	{
		return _isDisplayed;
	}

	public int GetPriorityValue()
	{
		return _promptData.GetPriority();
	}
}
