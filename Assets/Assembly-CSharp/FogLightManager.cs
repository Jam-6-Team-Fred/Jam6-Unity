using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FogLightManager : MonoBehaviour
{
	private class FogLightDataImagePair
	{
		public FogLight.LightData lightdata;

		public int imageIndex = -1;

		public Image image;
	}

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private Sprite _fogLightSprite;

	[SerializeField]
	private GameObject _templateImageObject;

	[SerializeField]
	private bool _useDebugSprite;

	[SerializeField]
	private Sprite _debugSprite;

	[SerializeField]
	private bool _onGUIMode;

	private List<FogLightDataImagePair> _lightDataList;

	private List<Image> _fogLightImagePool;

	private const int c_initPoolSize = 16;

	private void Awake()
	{
		_lightDataList = new List<FogLightDataImagePair>();
		InitializeImagePool();
		Canvas.willRenderCanvases += WillRenderCanvases;
	}

	private void OnDestroy()
	{
		Canvas.willRenderCanvases -= WillRenderCanvases;
	}

	private void InitializeImagePool()
	{
		_fogLightImagePool = new List<Image>();
		_templateImageObject.SetActive(value: false);
		for (int i = 0; i < 16; i++)
		{
			GameObject obj = Object.Instantiate(_templateImageObject);
			obj.transform.SetParent(_canvas.transform);
			obj.SetActive(value: false);
			Image requiredComponent = obj.GetRequiredComponent<Image>();
			requiredComponent.enabled = false;
			if (_useDebugSprite && _debugSprite != null)
			{
				requiredComponent.sprite = _debugSprite;
			}
			else
			{
				requiredComponent.sprite = _fogLightSprite;
			}
			_fogLightImagePool.Add(requiredComponent);
		}
	}

	private void IncreaseSizeOfImagePool()
	{
		int count = _fogLightImagePool.Count;
		int num = count * 2;
		for (int i = count; i < num; i++)
		{
			GameObject obj = Object.Instantiate(_templateImageObject);
			obj.transform.SetParent(_canvas.transform);
			obj.SetActive(value: false);
			Image requiredComponent = obj.GetRequiredComponent<Image>();
			requiredComponent.enabled = false;
			if (_useDebugSprite && _debugSprite != null)
			{
				requiredComponent.sprite = _debugSprite;
			}
			else
			{
				requiredComponent.sprite = _fogLightSprite;
			}
			_fogLightImagePool.Add(requiredComponent);
		}
	}

	public void RegisterLightData(FogLight.LightData lightData)
	{
		FogLightDataImagePair fogLightDataImagePair = new FogLightDataImagePair();
		fogLightDataImagePair.lightdata = lightData;
		_lightDataList.Add(fogLightDataImagePair);
	}

	private void WillRenderCanvases()
	{
		for (int i = 0; i < _lightDataList.Count; i++)
		{
			bool flag = IsLightVisible(_lightDataList[i].lightdata);
			bool flag2 = _lightDataList[i].image != null;
			if (flag != flag2)
			{
				if (flag)
				{
					AssignImage(_lightDataList[i]);
					continue;
				}
				_lightDataList[i].image.enabled = false;
				_lightDataList[i].image.gameObject.SetActive(value: false);
				_lightDataList[i].image = null;
			}
		}
		for (int j = 0; j < _lightDataList.Count; j++)
		{
			DrawLightData(_lightDataList[j]);
		}
	}

	private bool IsLightVisible(FogLight.LightData lightData)
	{
		if (lightData.alpha > 0f && lightData.screenPos.z > 0f)
		{
			return true;
		}
		return false;
	}

	private void AssignImage(FogLightDataImagePair dataPair)
	{
		for (int i = 0; i < _fogLightImagePool.Count; i++)
		{
			if (!_fogLightImagePool[i].gameObject.activeSelf)
			{
				_fogLightImagePool[i].gameObject.SetActive(value: true);
				dataPair.image = _fogLightImagePool[i];
				return;
			}
		}
		IncreaseSizeOfImagePool();
		AssignImage(dataPair);
	}

	private void DrawLightData(FogLightDataImagePair dataPair)
	{
		if (_onGUIMode)
		{
			if (dataPair.image != null && dataPair.image.enabled)
			{
				dataPair.image.enabled = false;
			}
		}
		else if (IsLightVisible(dataPair.lightdata))
		{
			Color color = dataPair.image.color;
			Color color2 = dataPair.lightdata.color;
			color2.a = dataPair.lightdata.alpha * dataPair.lightdata.maxAlpha;
			if (color != color2)
			{
				dataPair.image.color = color2;
			}
			dataPair.image.rectTransform.anchoredPosition = dataPair.lightdata.screenPos;
			dataPair.image.rectTransform.localScale = new Vector3(dataPair.lightdata.scale, dataPair.lightdata.scale, 1f);
			if (!dataPair.image.enabled)
			{
				dataPair.image.enabled = true;
			}
		}
	}

	public bool IsOnGUIMode()
	{
		return _onGUIMode;
	}
}
