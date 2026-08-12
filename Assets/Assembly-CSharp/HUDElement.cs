using UnityEngine;

public abstract class HUDElement : MonoBehaviour
{
	private Renderer[] _renderers;

	protected bool _isHelmetHUDOn;

	protected bool _isVisible;

	protected virtual void Awake()
	{
		base.enabled = false;
		_renderers = GetComponentsInChildren<Renderer>();
		GlobalMessenger.AddListener("HelmetHUDActivated", OnHelmetHUDActivated);
		GlobalMessenger.AddListener("RemoveHelmet", OnRemoveHelmet);
		GlobalMessenger.AddListener("ChangeGUIMode", OnChangeGUIMode);
	}

	protected virtual void OnDestroy()
	{
		GlobalMessenger.RemoveListener("HelmetHUDActivated", OnHelmetHUDActivated);
		GlobalMessenger.RemoveListener("RemoveHelmet", OnRemoveHelmet);
		GlobalMessenger.RemoveListener("ChangeGUIMode", OnChangeGUIMode);
	}

	protected virtual void ShowHUD()
	{
		EnableRenderers();
	}

	protected virtual void HideHUD()
	{
		DisableRenderers();
	}

	protected virtual bool AllowVisibility()
	{
		return _isHelmetHUDOn;
	}

	protected void UpdateVisibility()
	{
		if (AllowVisibility())
		{
			ShowHUD();
		}
		else
		{
			HideHUD();
		}
	}

	protected void EnableRenderers()
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].enabled = true;
		}
		_isVisible = true;
	}

	protected void DisableRenderers()
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].enabled = false;
		}
		_isVisible = false;
	}

	private void OnHelmetHUDActivated()
	{
		_isHelmetHUDOn = true;
		UpdateVisibility();
	}

	private void OnRemoveHelmet()
	{
		_isHelmetHUDOn = false;
		HideHUD();
		DisableRenderers();
	}

	private void OnChangeGUIMode()
	{
		if (GUIMode.IsHiddenMode())
		{
			DisableRenderers();
		}
		else
		{
			UpdateVisibility();
		}
	}
}
