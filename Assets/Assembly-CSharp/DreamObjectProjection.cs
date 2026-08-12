using System.Collections.Generic;
using UnityEngine;

public class DreamObjectProjection : MonoBehaviour, IItemDropTarget
{
	[SerializeField]
	private bool _setActive;

	protected OWLightController _lightController;

	protected DitheringAnimator _dithering;

	protected OWCollider[] _colliders;

	protected Shape[] _shapes;

	protected DreamCandle[] _candles;

	protected DreamObjectCandleProjector[] _candlesProjectors;

	protected bool _visible;

	private float _startPulseTime;

	private List<OWItem> _droppedItems;

	protected virtual void Awake()
	{
		base.enabled = false;
		if (!_setActive)
		{
			_lightController = GetComponent<OWLightController>();
			if (_lightController == null)
			{
				_candles = GetComponentsInChildren<DreamCandle>();
			}
			_dithering = GetComponent<DitheringAnimator>();
			_colliders = GetComponentsInChildren<OWCollider>();
			_shapes = GetComponentsInChildren<Shape>();
		}
		_candlesProjectors = GetComponents<DreamObjectCandleProjector>();
	}

	public Transform GetItemDropTargetTransform(GameObject raycastTarget)
	{
		return base.transform;
	}

	public void AddDroppedItem(GameObject dropTarget, OWItem item)
	{
		if (_droppedItems == null)
		{
			_droppedItems = new List<OWItem>();
		}
		_droppedItems.Add(item);
		item.onPickedUp += new OWEvent<OWItem>.OWCallback(OnPickedUpDroppedItem);
	}

	public bool IsVisible()
	{
		return _visible;
	}

	public virtual void SetVisibleImmediate(bool visible, bool forceUpdate = false)
	{
		if (_visible != visible || forceUpdate)
		{
			_visible = visible;
			UpdateVisibility(immediate: true);
		}
	}

	public virtual void SetVisible(bool visible)
	{
		if (_visible != visible)
		{
			_visible = visible;
			UpdateVisibility();
		}
	}

	public virtual void PulseOnAndOff()
	{
		base.enabled = true;
		_startPulseTime = Time.time;
	}

	protected virtual void UpdateVisibility(bool immediate = false)
	{
		for (int i = 0; i < _candlesProjectors.Length; i++)
		{
			_candlesProjectors[i].OnCandlesProjectionChange(_visible);
		}
		if (_setActive)
		{
			base.gameObject.SetActive(_visible);
			return;
		}
		if (immediate)
		{
			if (_lightController != null)
			{
				_lightController.SetIntensity(_visible ? 1f : 0f);
			}
			if (_candles != null)
			{
				for (int j = 0; j < _candles.Length; j++)
				{
					_candles[j].SetLit(_visible, playAudio: false);
				}
			}
			if (_dithering != null)
			{
				_dithering.SetVisibleImmediate(_visible);
			}
		}
		else
		{
			if (_lightController != null)
			{
				_lightController.FadeTo(_visible ? 1f : 0f, _visible ? 1f : 0.5f);
			}
			if (_candles != null)
			{
				for (int k = 0; k < _candles.Length; k++)
				{
					_candles[k].SetLit(_visible, playAudio: false);
				}
			}
			if (_dithering != null)
			{
				_dithering.SetVisible(_visible, 3f);
			}
		}
		if (_colliders != null)
		{
			for (int l = 0; l < _colliders.Length; l++)
			{
				_colliders[l].SetActivation(_visible);
			}
		}
		if (_shapes != null)
		{
			for (int m = 0; m < _shapes.Length; m++)
			{
				if (_shapes[m].collisionMode != Shape.CollisionMode.Detector)
				{
					_shapes[m].SetActivation(_visible);
				}
			}
		}
		if (_droppedItems == null)
		{
			return;
		}
		for (int num = _droppedItems.Count - 1; num >= 0; num--)
		{
			if (_droppedItems[num].GetItemType() == ItemType.DreamLantern)
			{
				if (!_visible)
				{
					Locator.GetDreamWorldController().ExitDreamWorld(DreamWakeType.LanternBlownOut);
					_droppedItems[num].onPickedUp -= new OWEvent<OWItem>.OWCallback(OnPickedUpDroppedItem);
					_droppedItems.RemoveAt(num);
				}
			}
			else
			{
				_droppedItems[num].gameObject.SetActive(_visible);
			}
		}
	}

	protected virtual void Update()
	{
		float num = Mathf.InverseLerp(_startPulseTime, _startPulseTime + 1.2f, Time.time);
		float num2 = 0f - Mathf.Pow(2f * num - 1f, 2f) + 1f;
		float num3 = 0.5f;
		if (_lightController != null)
		{
			_lightController.SetIntensity(num2 * num3);
		}
		if (_candles != null)
		{
			for (int i = 0; i < _candles.Length; i++)
			{
				_candles[i].SetPulseIntensity(num2 * num3);
			}
		}
		if (num >= 1f)
		{
			base.enabled = false;
		}
	}

	private void OnPickedUpDroppedItem(OWItem item)
	{
		_droppedItems.Remove(item);
		item.onPickedUp -= new OWEvent<OWItem>.OWCallback(OnPickedUpDroppedItem);
	}
}
