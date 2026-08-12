using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiSelectionListElement : MenuOption, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IMoveHandler
{
	public struct ListEntry
	{
		public int itemIndex;

		public bool itemBoolVal;

		public string itemLabel;
	}

	public delegate void ListSelectorEvent();

	public delegate void ListUpdateEvent(ListEntry[] entries);

	[Space(10f)]
	[SerializeField]
	protected UIStyleApplier _optionsBoxStyleApplier;

	[Space(10f)]
	[SerializeField]
	protected LayoutElement _wholeControlLayoutElement;

	[SerializeField]
	protected HorizontalLayoutGroup _wholeElementHorzLayoutGroup;

	[SerializeField]
	protected LayoutElement _listSectionLayoutElement;

	[SerializeField]
	protected ToggleElement _templateToggle;

	[SerializeField]
	protected VerticalLayoutGroup _toggleListLayoutGroup;

	private List<ToggleElement> _toggleElementList;

	private ListEntry[] _listEntries;

	public event ListSelectorEvent OnEnterListSelection;

	public event ListSelectorEvent OnExitListSelection;

	public event ListUpdateEvent OnListUpdated;

	public void Initialize(ListEntry[] listEntries)
	{
		base.Initialize();
		_listEntries = listEntries;
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
			Debug.LogWarning("MultiSelectionListElement requires control of its own height!");
		}
		CalculateAndSetUIElementHeights(_listEntries.Length);
		SetupIndividualToggles();
	}

	private void SetupIndividualToggles()
	{
		_templateToggle.gameObject.SetActive(value: true);
		Selectable selectable = null;
		Selectable selectable2 = null;
		Selectable selectable3 = null;
		for (int i = 0; i < _listEntries.Length; i++)
		{
			if (i < _toggleElementList.Count)
			{
				_toggleElementList[i].SetDisplayText(_listEntries[i].itemLabel);
				_toggleElementList[i].gameObject.SetActive(value: true);
				selectable2 = _toggleElementList[i].gameObject.GetRequiredComponent<Selectable>();
			}
			else
			{
				GameObject obj = Object.Instantiate(_templateToggle.gameObject, _toggleListLayoutGroup.transform);
				ToggleElement requiredComponent = obj.GetRequiredComponent<ToggleElement>();
				requiredComponent.SetDisplayText(_listEntries[i].itemLabel);
				requiredComponent.OnToggleSubmit += OnToggleSubmit;
				requiredComponent.OnToggleCancel += OnToggleCancel;
				_toggleElementList.Add(requiredComponent);
				selectable2 = obj.GetRequiredComponent<Selectable>();
			}
			_toggleElementList[i].Initialize(_listEntries[i].itemBoolVal ? 1 : 0);
			if (i == 0)
			{
				selectable = selectable2;
			}
			Navigation navigation = selectable2.navigation;
			navigation.selectOnUp = selectable3;
			if (i == _toggleElementList.Count - 1)
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

	public override void Initialize()
	{
		Debug.LogError("MultiSelectionListElement should be initialzied with the ListEntry[] param");
		base.Initialize();
	}

	private void CalculateAndSetUIElementHeights(int numEntries)
	{
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
		float num2 = preferredHeight * (float)numEntries + spacing * (float)(numEntries - 1);
		float preferredHeight2 = num2 + num;
		_wholeControlLayoutElement.preferredHeight = preferredHeight2;
		_listSectionLayoutElement.preferredHeight = num2;
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

	protected void OnToggleCancel(BaseEventData eventData, ToggleElement selectable)
	{
		_selectable.Select();
		if (this.OnExitListSelection != null)
		{
			this.OnExitListSelection();
		}
	}

	protected void OnToggleSubmit(BaseEventData eventData, ToggleElement updatedToggle)
	{
		UpdateToggleStates(updatedToggle);
		if (this.OnExitListSelection != null)
		{
			this.OnExitListSelection();
		}
		_selectable.Select();
	}

	private void UpdateToggleStates(ToggleElement updatedToggle)
	{
		int num = -1;
		for (int i = 0; i < _toggleElementList.Count; i++)
		{
			if (_toggleElementList[i] == updatedToggle)
			{
				num = i;
			}
		}
		if (num != -1)
		{
			_listEntries[num].itemBoolVal = !_listEntries[num].itemBoolVal;
			if (this.OnListUpdated != null)
			{
				this.OnListUpdated(_listEntries);
			}
		}
	}

	protected void OnToggle(ToggleElement updatedToggle)
	{
		UpdateToggleStates(updatedToggle);
		_selectable.Select();
	}

	public virtual void OnMove(AxisEventData eventData)
	{
	}

	public virtual void OnPointerExit(PointerEventData pointerEventData)
	{
	}
}
