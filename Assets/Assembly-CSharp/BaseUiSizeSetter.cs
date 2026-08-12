using System;
using UnityEngine;

public abstract class BaseUiSizeSetter : MonoBehaviour, IUiSizeSetter
{
	[Serializable]
	protected struct BoolBlock
	{
		public bool normalVal;

		public bool largeVal;
	}

	[Serializable]
	protected struct IntBlock
	{
		public int normalVal;

		public int largeVal;
	}

	[Serializable]
	protected struct FloatBlock
	{
		public float normalVal;

		public float largeVal;
	}

	[Serializable]
	protected struct Vector2Block
	{
		public Vector2 normalVal;

		public Vector2 largeVal;
	}

	[Serializable]
	protected struct Vector3Block
	{
		public Vector3 normalVal;

		public Vector3 largeVal;
	}

	[Serializable]
	protected struct TextAnchorBlock
	{
		public TextAnchor normalVal;

		public TextAnchor largeVal;
	}

	[SerializeField]
	protected GameObject _userFriendlyParentObject;

	[SerializeField]
	protected bool _requiresExternalInitialization;

	protected bool _readyForResize;

	protected bool _isRegistered;

	public GameObject userFriendlyParentIdObj
	{
		get
		{
			return _userFriendlyParentObject;
		}
		set
		{
			_userFriendlyParentObject = value;
		}
	}

	public bool readyForResize => _readyForResize;

	public event ReadyForResizeEvent OnReadyForResize;

	public abstract void DoResizeAction(UITextSize textSizeSetting);

	protected virtual void Awake()
	{
		_readyForResize = true;
		if (_requiresExternalInitialization)
		{
			_readyForResize = false;
		}
	}

	protected virtual void Start()
	{
		Locator.GetUISizeManager().RegisterUiSizeSetter(this);
		_isRegistered = true;
	}

	protected virtual void OnDestroy()
	{
		if (_isRegistered && Locator.GetUISizeManager() != null)
		{
			Locator.GetUISizeManager().UnregisterUiSizeSetter(this);
			_isRegistered = false;
		}
	}

	public virtual void MarkReadyForInitialization()
	{
		_readyForResize = true;
		this.OnReadyForResize?.Invoke(this);
	}
}
