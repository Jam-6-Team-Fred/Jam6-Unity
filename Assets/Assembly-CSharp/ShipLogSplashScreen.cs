using System;
using UnityEngine;

public class ShipLogSplashScreen : MonoBehaviour
{
	[SerializeField]
	private Color _pulseColor;

	[SerializeField]
	private ShipLight[] _lights;

	[SerializeField]
	private Renderer _splashScreen;

	[SerializeField]
	private Renderer _nomaiCable;

	private Color _origColor;

	private bool _newUpdates;

	private bool _damaged;

	private void Awake()
	{
		_origColor = _splashScreen.sharedMaterial.color;
		GlobalMessenger.AddListener("ShipLogUpdated", OnShipLogUpdated);
	}

	private void Start()
	{
		base.enabled = PlayerData.GetNewlyRevealedFactIDs().Count > 0;
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("ShipLogUpdated", OnShipLogUpdated);
	}

	public void SetDamaged(bool damaged)
	{
		_damaged = damaged;
		_splashScreen.material.SetFloat("_Damaged", _damaged ? 1f : 0f);
		CheckForEnabled();
	}

	public void OnEnterComputer()
	{
		base.enabled = false;
	}

	public void OnExitComputer()
	{
		base.enabled = false;
	}

	private void OnShipLogUpdated()
	{
		_newUpdates = true;
		CheckForEnabled();
	}

	private void CheckForEnabled()
	{
		base.enabled = _newUpdates && !_damaged && PlayerData.GetShowShipLogNotifications();
	}

	private void OnEnable()
	{
		_nomaiCable.material.SetTextureOffset("_EmissionMap", new Vector2(0f, -0.5f));
	}

	private void OnDisable()
	{
		_newUpdates = false;
		_splashScreen.material.color = _origColor;
		_nomaiCable.material.SetTextureOffset("_EmissionMap", Vector2.zero);
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].SetExtraIntensityScale(1f);
		}
	}

	private void Update()
	{
		float num = (Mathf.Sin(Time.time * (float)Math.PI + 0.8f) + 1f) / 2f;
		_splashScreen.material.color = Color.Lerp(_origColor, _pulseColor, num);
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].SetExtraIntensityScale(num);
		}
	}
}
