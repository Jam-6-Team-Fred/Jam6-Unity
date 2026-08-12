using System;
using UnityEngine;

public abstract class ProxyBody : MonoBehaviour, ILateInitializer
{
	[SerializeField]
	protected Transform _realObjectTransform;

	[SerializeField]
	protected float _realObjectDiameter;

	public const float CUTOFF_DISTANCE = 42000f;

	public const float FAR_PLANE = 46000f;

	private float _proxyAtan;

	protected bool _outOfRange;

	protected bool _renderingEnabled;

	private float _currentScaleFactor;

	private float _logSpaceLength;

	protected bool _initialized;

	public Transform realObjectTransform => _realObjectTransform;

	public float realObjectDiameter => _realObjectDiameter;

	public float currentScaleFactor => _currentScaleFactor;

	protected virtual void Awake()
	{
		LateInitializerManager.RegisterLateInitializer(this);
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
	}

	protected virtual void Start()
	{
		ToggleRendering(on: false);
	}

	protected virtual void Update()
	{
		if (!_initialized)
		{
			return;
		}
		if (IsObjectInSupernova() || _realObjectTransform == null)
		{
			ToggleRendering(on: false);
			base.enabled = false;
			return;
		}
		Vector3 position = Locator.GetActiveCamera().transform.position;
		Vector3 realVector = _realObjectTransform.position - position;
		float sqrMagnitude = realVector.sqrMagnitude;
		_outOfRange = sqrMagnitude > 1.764E+09f;
		if (_outOfRange != _renderingEnabled)
		{
			ToggleRendering(_outOfRange);
		}
		if (_outOfRange)
		{
			sqrMagnitude = Mathf.Sqrt(sqrMagnitude);
			base.transform.position = GetProxyPosition(position, realVector, sqrMagnitude, 42000f, _logSpaceLength, out var _);
			float scaleMultiplier = Mathf.Clamp01(Mathf.Atan(_realObjectDiameter / sqrMagnitude) / _proxyAtan);
			base.transform.rotation = _realObjectTransform.rotation;
			_currentScaleFactor = scaleMultiplier;
			UpdateScale(scaleMultiplier, sqrMagnitude);
		}
	}

	public static Vector3 GetProxyPosition(Vector3 playerCamPos, Vector3 realVector, float realDistance, float cutoffDistance, float logSpaceLength, out float resultDistance)
	{
		float num = (realDistance - cutoffDistance) / logSpaceLength;
		num = Mathf.Clamp(num, 0f, num);
		float num2 = (0f - Mathf.Pow(2f, -10f * num) + 1f) * logSpaceLength;
		resultDistance = cutoffDistance + num2;
		return playerCamPos + realVector / realDistance * resultDistance;
	}

	protected virtual void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
		if (!_initialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
	}

	public void SetRealBody(Transform realBody)
	{
		_realObjectTransform = realBody;
	}

	public void SetRealDiameter(float realDiameter)
	{
		_realObjectDiameter = realDiameter;
	}

	private bool IsObjectInSupernova()
	{
		float num = Locator.GetSunController().GetSupernovaRadius() - 2000f;
		if (num <= 0f)
		{
			return false;
		}
		float sqrMagnitude = (_realObjectTransform.position - Locator.GetSunController().transform.position).sqrMagnitude;
		return num * num >= sqrMagnitude;
	}

	private void OnEnterMapView()
	{
		ToggleRendering(on: false);
		base.enabled = false;
	}

	private void OnExitMapView()
	{
		base.enabled = true;
	}

	public virtual void UpdateScale(float scaleMultiplier, float viewDistance)
	{
		base.transform.localScale = Vector3.one * scaleMultiplier;
	}

	public virtual void ToggleRendering(bool on)
	{
		_renderingEnabled = on;
	}

	protected virtual void Initialize()
	{
	}

	public virtual void LateInitialize()
	{
		Initialize();
		_proxyAtan = Mathf.Atan(_realObjectDiameter / 42000f);
		_logSpaceLength = 0.08695652f;
		ToggleRendering(on: false);
		_currentScaleFactor = 1f;
		_initialized = true;
	}

	protected void PrintInitializeFailMessage(Exception e)
	{
		Debug.LogWarning(string.Concat("The distant proxy ", base.gameObject.name, " failed to initialize due to a(n) ", e.GetType(), ". It has destroyed itself and will not affect the game."));
	}
}
