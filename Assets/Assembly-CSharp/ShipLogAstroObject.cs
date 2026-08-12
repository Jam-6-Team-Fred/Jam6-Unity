using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipLogAstroObject : MonoBehaviour
{
	[SerializeField]
	private string _id;

	[SerializeField]
	private GameObject _imageObj;

	[SerializeField]
	private GameObject _outlineObj;

	[SerializeField]
	private GameObject _unviewedObj;

	[SerializeField]
	private bool _invisibleWhenHidden;

	private RectTransform _rectTransform;

	private Image _image;

	private Material _greyscaleMaterial;

	private List<ShipLogEntry> _entries;

	private ShipLogEntry.State _state;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		_state = ShipLogEntry.State.None;
		if (_imageObj != null)
		{
			_image = _imageObj.GetComponent<Image>();
			if (_image != null)
			{
				_greyscaleMaterial = _image.material;
			}
		}
	}

	public string GetID()
	{
		return _id;
	}

	public bool IsVisible()
	{
		if (_invisibleWhenHidden)
		{
			return _state != ShipLogEntry.State.Hidden;
		}
		return true;
	}

	public ShipLogEntry.State GetState()
	{
		return _state;
	}

	public string GetName()
	{
		string text = AstroObject.AstroObjectNameToString(AstroObject.StringIDToAstroObjectName(_id));
		if (text.Length <= 0)
		{
			return "Location Unknown";
		}
		return text;
	}

	public Vector2 GetAnchoredPosition()
	{
		return _rectTransform.anchoredPosition;
	}

	public void OnEnterComputer()
	{
		_entries = Locator.GetShipLogManager().GetEntriesByAstroBody(_id);
	}

	public List<ShipLogEntry> GetEntries()
	{
		return _entries;
	}

	public void UpdateState()
	{
		bool active = false;
		_state = ShipLogEntry.State.Hidden;
		for (int i = 0; i < _entries.Count; i++)
		{
			if (_entries[i].HasUnreadFacts())
			{
				active = true;
			}
			if (_entries[i].GetState() == ShipLogEntry.State.Explored)
			{
				_state = ShipLogEntry.State.Explored;
			}
			else if (_state != ShipLogEntry.State.Explored && _entries[i].GetState() == ShipLogEntry.State.Rumored)
			{
				_state = ShipLogEntry.State.Rumored;
			}
		}
		_unviewedObj.SetActive(active);
		if (_state == ShipLogEntry.State.Hidden)
		{
			_imageObj.SetActive(!_invisibleWhenHidden);
			if (_outlineObj != null)
			{
				_outlineObj.SetActive(!_invisibleWhenHidden);
			}
			if (_image != null)
			{
				_image.color = Color.black;
			}
		}
		else if (_state == ShipLogEntry.State.Rumored)
		{
			_imageObj.SetActive(value: true);
			if (_outlineObj != null)
			{
				_outlineObj.SetActive(value: false);
			}
			if (_image != null)
			{
				SetMaterialGreyscale(isGreyscale: true);
				_image.color = Color.white;
			}
		}
		else if (_state == ShipLogEntry.State.Explored)
		{
			_imageObj.SetActive(value: true);
			if (_outlineObj != null)
			{
				_outlineObj.SetActive(value: false);
			}
			if (_image != null)
			{
				SetMaterialGreyscale(isGreyscale: false);
				_image.color = Color.white;
			}
		}
	}

	private void SetMaterialGreyscale(bool isGreyscale)
	{
		_image.material = (isGreyscale ? _greyscaleMaterial : null);
	}
}
