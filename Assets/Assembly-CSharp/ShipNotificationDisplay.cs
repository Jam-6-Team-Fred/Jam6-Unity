using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ShipNotificationDisplay : NotificationDisplayTextLayout
{
	private ShipAudioController _audioController;

	[SerializeField]
	private FontAndLanguageController _fontController;

	[SerializeField]
	private Text _testText;

	[SerializeField]
	private VerticalLayoutGroup _verticalLayoutGroup;

	[SerializeField]
	private float _defaultNotificationSpacing;

	[SerializeField]
	private float _jpNotificationSpacing = 5f;

	private bool _textTemplatesContainCustomSpacing;

	private TextStyleApplier _testTestStyleApplier;

	private Queue<NotificationData> _notificationUntestedQueue;

	private Queue<NotificationData> _notificationReadyQueue;

	private Queue<NotificationData> _notificationToMoveToTopQueue;

	private NotificationData _fitTestingData;

	private int _spaceReplacementTries;

	private bool _shipDestroyed;

	private bool _modifiedTestVerts;

	private bool _isLargeUiEnabled;

	protected override void Awake()
	{
		base.Awake();
		_notificationTargetType = NotificationTarget.Ship;
		GlobalMessenger.AddListener("ShipSystemFailure", OnShipSystemFailure);
	}

	protected override void Start()
	{
		base.Start();
		_isLargeUiEnabled = PlayerData.IsUILargeTextSize();
		_audioController = Locator.GetShipTransform().GetComponentInChildren<ShipAudioController>();
		_testTestStyleApplier = _testText.GetRequiredComponent<TextStyleApplier>();
		Text requiredComponentInChildren = _textDisplayTemplate.GetRequiredComponentInChildren<Text>();
		requiredComponentInChildren.text = "";
		TextStyleApplier component = requiredComponentInChildren.GetComponent<TextStyleApplier>();
		if (component != null)
		{
			_testTestStyleApplier.spacing = component.spacing;
			_textTemplatesContainCustomSpacing = true;
			_notificationUntestedQueue = new Queue<NotificationData>();
			_notificationReadyQueue = new Queue<NotificationData>();
			_notificationToMoveToTopQueue = new Queue<NotificationData>();
			Canvas.willRenderCanvases += OnWillRenderCanvases;
		}
		for (int i = 0; i < _textItemPool.Count; i++)
		{
			_fontController.AddTextElement(_textItemPool[i].GetRequiredComponentInChildren<Text>(), rescale: false);
		}
		if (_verticalLayoutGroup != null)
		{
			if (TextTranslation.Get().GetLanguage() == TextTranslation.Language.JAPANESE)
			{
				_verticalLayoutGroup.spacing = _jpNotificationSpacing;
			}
			else
			{
				_verticalLayoutGroup.spacing = _defaultNotificationSpacing;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_textTemplatesContainCustomSpacing)
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases;
		}
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipSystemFailure);
	}

	protected override void ExpandPool()
	{
		int num = _textItemPool.Count * 2;
		while (_textItemPool.Count < num)
		{
			GameObject gameObject = Object.Instantiate(_textDisplayTemplate, _textDisplayRoot);
			_textItemPool.Add(gameObject);
			_fontController.AddTextElement(gameObject.GetRequiredComponentInChildren<Text>(), rescale: false);
			gameObject.SetActive(value: false);
		}
	}

	public override void PushNotification(NotificationData data)
	{
		if (_textTemplatesContainCustomSpacing)
		{
			int num = FindDuplicateIndex(data);
			if (num == -1)
			{
				_notificationUntestedQueue.Enqueue(data);
				return;
			}
			NotificationDisplayData notificationDisplayData = _listDisplayData[num];
			notificationDisplayData.ResetDisplay();
			notificationDisplayData.Data = data;
			notificationDisplayData.TextScrollEffect.Init(notificationDisplayData.Data.displayMessage, _textAnchor);
		}
		else
		{
			base.PushNotification(data);
		}
	}

	private void OnWillRenderCanvases()
	{
		if (_shipDestroyed)
		{
			return;
		}
		if (PlayerData.IsUILargeTextSize() != _isLargeUiEnabled)
		{
			for (int i = 0; i < _listDisplayData.Count; i++)
			{
				_listDisplayData[i].TextDisplay.SetLayoutDirty();
			}
			_isLargeUiEnabled = PlayerData.IsUILargeTextSize();
		}
		if (_fitTestingData != null && !_modifiedTestVerts)
		{
			if (_testTestStyleApplier.widthOffset + _testText.preferredWidth > _testText.rectTransform.rect.width)
			{
				if (!InsertLineBreakOnCurrentTestNotification())
				{
					_spaceReplacementTries = 0;
					_notificationReadyQueue.Enqueue(_fitTestingData);
					_fitTestingData = null;
				}
				if (_modifiedTestVerts)
				{
					_testText.SetVerticesDirty();
					return;
				}
			}
			else
			{
				_spaceReplacementTries = 0;
				_notificationReadyQueue.Enqueue(_fitTestingData);
				_fitTestingData = null;
			}
		}
		if (_notificationReadyQueue.Count > 0)
		{
			NotificationData data = _notificationReadyQueue.Dequeue();
			base.PushNotification(data);
		}
		if (_notificationUntestedQueue.Count > 0 && _fitTestingData == null)
		{
			NotificationData fitTestingData = _notificationUntestedQueue.Dequeue();
			_fitTestingData = fitTestingData;
			_testText.text = _fitTestingData.displayMessage;
		}
		if (_notificationToMoveToTopQueue.Count > 0)
		{
			BringNotificationToTop(_notificationToMoveToTopQueue.Dequeue());
		}
		if (_modifiedTestVerts)
		{
			_modifiedTestVerts = false;
		}
	}

	private bool InsertLineBreakOnCurrentTestNotification()
	{
		char[] array = _fitTestingData.displayMessage.ToCharArray();
		int num = 0;
		bool result = false;
		for (int num2 = array.Length - 1; num2 >= 0; num2--)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(array[num2]) == UnicodeCategory.SpaceSeparator)
			{
				num++;
				if (_spaceReplacementTries < num)
				{
					_spaceReplacementTries++;
					array[num2] = '\n';
					result = true;
					_modifiedTestVerts = true;
					break;
				}
			}
			else if (array[num2] == '\n')
			{
				num++;
				array[num2] = ' ';
				_modifiedTestVerts = true;
			}
		}
		_fitTestingData.displayMessage = new string(array);
		_testText.text = _fitTestingData.displayMessage;
		return result;
	}

	protected override void PlayNotificationAudio()
	{
		if (!_audioController.IsCockpitTextSFXPlaying() && OWInput.GetInputMode() != InputMode.LandingCam)
		{
			_audioController.StartCockpitTextAudio();
		}
	}

	protected override void StopNotificationAudio()
	{
		if (_audioController != null)
		{
			_audioController.StopCockpitTextAudio();
		}
	}

	private void OnShipSystemFailure()
	{
		_shipDestroyed = true;
		base.gameObject.SetActive(value: false);
		NotificationManager.SharedInstance.UnregisterNotifiable(this);
	}

	public override void BringNotificationToTop(NotificationData data)
	{
		if (_notificationUntestedQueue.Count > 0 || _notificationReadyQueue.Count > 0 || _fitTestingData != null)
		{
			_notificationToMoveToTopQueue.Enqueue(data);
		}
		else
		{
			base.BringNotificationToTop(data);
		}
	}
}
