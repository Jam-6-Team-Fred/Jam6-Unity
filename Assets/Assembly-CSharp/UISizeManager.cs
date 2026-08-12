using System.Collections.Generic;
using UnityEngine;

public class UISizeManager : MonoBehaviour
{
	private List<IUiSizeSetter> _listUiSizeSetters;

	private bool _waiting;

	private bool _lateInit;

	private void Awake()
	{
		_listUiSizeSetters = new List<IUiSizeSetter>();
	}

	private void Update()
	{
		if (!_lateInit && PlayerData.IsLoaded() && !PlayerData.IsBusy())
		{
			LateInitialize();
		}
	}

	private void LateInitialize()
	{
		for (int i = 0; i < _listUiSizeSetters.Count; i++)
		{
			if (_listUiSizeSetters[i].readyForResize)
			{
				_listUiSizeSetters[i].DoResizeAction(PlayerData.GetTextSize());
			}
			else
			{
				_listUiSizeSetters[i].OnReadyForResize += OnSizeSetterReadyForResize;
			}
		}
		_lateInit = true;
	}

	private void OnSizeSetterReadyForResize(IUiSizeSetter sizeSetter)
	{
		sizeSetter.OnReadyForResize -= OnSizeSetterReadyForResize;
		sizeSetter.DoResizeAction(PlayerData.GetTextSize());
	}

	public void RegisterUiSizeSetter(IUiSizeSetter uiSizeSetter)
	{
		_listUiSizeSetters.Add(uiSizeSetter);
		if (_lateInit)
		{
			if (uiSizeSetter.readyForResize)
			{
				uiSizeSetter.DoResizeAction(PlayerData.GetTextSize());
			}
			else
			{
				uiSizeSetter.OnReadyForResize += OnSizeSetterReadyForResize;
			}
		}
	}

	public void UnregisterUiSizeSetter(IUiSizeSetter uiSizeSetter)
	{
		_listUiSizeSetters.Remove(uiSizeSetter);
	}

	public void OnUiSizeSettingChanged()
	{
		if (_lateInit)
		{
			_lateInit = false;
		}
	}
}
