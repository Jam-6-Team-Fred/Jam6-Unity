using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ListSelectorElement : OptionsSelectorElement, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IMoveHandler
{
	public delegate void ListSelectorEvent();

	public delegate void ListSelectionValueChangedEvent(int optionIndex);

	[Space(10f)]
	[SerializeField]
	protected LayoutElement _wholeControlLayoutElement;

	[SerializeField]
	protected HorizontalLayoutGroup _wholeElementHorzLayoutGroup;

	[Space(10f)]
	[SerializeField]
	protected LayoutElement _listSectionLayoutElement;

	[SerializeField]
	protected ToggleElement _templateToggle;

	[SerializeField]
	protected VerticalLayoutGroup _toggleListLayoutGroup;

	protected List<ToggleElement> _toggleElementList;

	protected bool _refreshToggleList;

	protected bool _ignoreToggleEvents;

	protected int _lastToggledElementIndex = -1;

	protected bool _selectSelfNextFrame;

	protected FontAndLanguageController _fontController;

	public event ListSelectorEvent OnEnterListSelection;

	public event ListSelectorEvent OnExitListSelection;

	public event ListSelectionValueChangedEvent OnListSelectionValueChanged;

	protected override void Update()
	{
		if (_selectSelfNextFrame)
		{
			_selectable.Select();
			_selectSelfNextFrame = false;
		}
		else if (_refreshToggleList)
		{
			RefreshToggleList();
		}
	}

	public void Initialize(int index, string[] displayedOptions, FontAndLanguageController fontController)
	{
		_fontController = fontController;
		Initialize(index, displayedOptions);
	}

	public override void Initialize(int index, string[] displayedOptions)
	{
		if (_selectable == null)
		{
			_selectable = this.GetRequiredComponent<Selectable>();
		}
		_value = -1;
		if (index >= 0)
		{
			_value = index;
		}
		_optionsList = displayedOptions;
		if (_toggleElementList == null)
		{
			_toggleElementList = new List<ToggleElement>();
		}
		else
		{
			for (int i = 0; i < _toggleElementList.Count; i++)
			{
				_toggleElementList[i].gameObject.SetActive(value: false);
			}
		}
		HorizontalOrVerticalLayoutGroup component = base.transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>();
		if (component != null && !component.childControlHeight)
		{
			Debug.LogWarning("ListSelectorelement requires control of its own height!");
		}
		_directionality = Direction.VERTICAL;
		float spacing = _toggleListLayoutGroup.spacing;
		float preferredHeight = _templateToggle.GetRequiredComponent<LayoutElement>().preferredHeight;
		if (preferredHeight == 0f)
		{
			Debug.LogWarning("ListSelectorElement's Toggle template must have preferred height defined!");
		}
		float num = 0f;
		if (_wholeElementHorzLayoutGroup != null)
		{
			num += (float)_wholeElementHorzLayoutGroup.padding.top;
			num += (float)_wholeElementHorzLayoutGroup.padding.bottom;
		}
		float num2 = preferredHeight * (float)displayedOptions.Length + spacing * (float)(displayedOptions.Length - 1);
		float preferredHeight2 = num2 + num;
		_wholeControlLayoutElement.preferredHeight = preferredHeight2;
		_listSectionLayoutElement.preferredHeight = num2;
		_templateToggle.gameObject.SetActive(value: true);
		Selectable selectable = null;
		Selectable selectable2 = null;
		Selectable selectable3 = null;
		for (int j = 0; j < _optionsList.Length; j++)
		{
			if (j < _toggleElementList.Count)
			{
				_toggleElementList[j].SetDisplayText(_optionsList[j]);
				_toggleElementList[j].gameObject.SetActive(value: true);
				selectable2 = _toggleElementList[j].gameObject.GetRequiredComponent<Selectable>();
			}
			else
			{
				GameObject obj = Object.Instantiate(_templateToggle.gameObject, _toggleListLayoutGroup.transform);
				ToggleElement requiredComponent = obj.GetRequiredComponent<ToggleElement>();
				requiredComponent.SetDisplayText(_optionsList[j]);
				requiredComponent.OnToggleSubmit += OnToggleSubmit;
				requiredComponent.OnToggleCancel += OnToggleCancel;
				if (_fontController != null)
				{
					Text component2 = requiredComponent.GetComponent<Text>();
					if (component2 != null)
					{
						_fontController.AddTextElement(component2, rescale: true, useDefaultLineSpacing: true, isDynamicFont: true);
					}
				}
				_toggleElementList.Add(requiredComponent);
				selectable2 = obj.GetRequiredComponent<Selectable>();
			}
			_toggleElementList[j].Initialize((j == _value) ? 1 : 0);
			if (j == 0)
			{
				selectable = selectable2;
			}
			Navigation navigation = selectable2.navigation;
			navigation.selectOnUp = selectable3;
			if (j == _toggleElementList.Count - 1)
			{
				navigation.selectOnDown = selectable;
				Navigation navigation2 = selectable.navigation;
				navigation2.selectOnUp = selectable2;
				selectable.navigation = navigation2;
			}
			else
			{
				navigation.selectOnDown = null;
			}
			selectable2.navigation = navigation;
			if (selectable3 != null)
			{
				Navigation navigation3 = selectable3.navigation;
				navigation3.selectOnDown = selectable2;
				selectable3.navigation = navigation3;
			}
			selectable3 = selectable2;
		}
		_templateToggle.gameObject.SetActive(value: false);
	}

	protected void OnToggleCancel(BaseEventData eventData, ToggleElement selectable)
	{
		_selectable.Select();
		if (this.OnExitListSelection != null)
		{
			this.OnExitListSelection();
		}
	}

	protected void OnToggleSubmit(BaseEventData eventData, ToggleElement selectable)
	{
		OnToggle(selectable);
		if (this.OnExitListSelection != null)
		{
			this.OnExitListSelection();
		}
	}

	protected void OnToggle(ToggleElement selectable)
	{
		if (_ignoreToggleEvents)
		{
			return;
		}
		_refreshToggleList = true;
		for (int i = 0; i < _toggleElementList.Count; i++)
		{
			if (_toggleElementList[i] == selectable)
			{
				_lastToggledElementIndex = i;
				_ignoreToggleEvents = true;
				break;
			}
		}
	}

	protected void RefreshToggleList()
	{
		int num = -1;
		if (_value == _lastToggledElementIndex)
		{
			if (_toggleElementList[_lastToggledElementIndex].GetValue() == 1)
			{
				num = -1;
			}
		}
		else
		{
			num = _lastToggledElementIndex;
			if (_value >= 0)
			{
				for (int i = 0; i < _toggleElementList.Count; i++)
				{
					if (i != _lastToggledElementIndex && _toggleElementList[i].GetValue() == 1)
					{
						_toggleElementList[i].Toggle();
					}
				}
			}
		}
		if (num != _value)
		{
			_value = num;
			if (this.OnListSelectionValueChanged != null)
			{
				this.OnListSelectionValueChanged(_value);
			}
			OnOptionValueChanged();
		}
		_ignoreToggleEvents = false;
		_refreshToggleList = false;
		_selectSelfNextFrame = true;
	}

	protected override void SetSelectedOption()
	{
	}

	protected override void OptionsMove(Vector2 moveVector)
	{
	}

	public void OnSubmit(BaseEventData eventData)
	{
		eventData.Use();
		_toggleElementList[0].GetRequiredComponent<Selectable>().Select();
		if (this.OnEnterListSelection != null)
		{
			this.OnEnterListSelection();
		}
	}
}
