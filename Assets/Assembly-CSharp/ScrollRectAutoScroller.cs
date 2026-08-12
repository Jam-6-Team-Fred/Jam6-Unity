using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectAutoScroller : MonoBehaviour
{
	private RectTransform scrollRectTransform;

	private RectTransform contentPanel;

	private RectTransform selectedRectTransform;

	private GameObject lastSelected;

	private Vector2 targetPos;

	private void Start()
	{
		scrollRectTransform = GetComponent<RectTransform>();
		if (contentPanel == null)
		{
			contentPanel = GetComponent<ScrollRect>().content;
		}
		targetPos = contentPanel.anchoredPosition;
	}

	private void Update()
	{
		Autoscroll();
	}

	public void Autoscroll(bool forceAutoscroll = false)
	{
		if (contentPanel == null)
		{
			contentPanel = GetComponent<ScrollRect>().content;
		}
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject == null || (currentSelectedGameObject == lastSelected && !forceAutoscroll))
		{
			return;
		}
		if (currentSelectedGameObject.transform.parent != contentPanel.transform)
		{
			if (!currentSelectedGameObject.transform.IsChildOf(contentPanel.transform))
			{
				return;
			}
			Transform parent = currentSelectedGameObject.transform;
			while (parent.parent != contentPanel.transform)
			{
				parent = parent.parent;
			}
			selectedRectTransform = (RectTransform)parent.transform;
		}
		else
		{
			selectedRectTransform = (RectTransform)currentSelectedGameObject.transform;
		}
		targetPos = contentPanel.anchoredPosition;
		float num = 0f - selectedRectTransform.localPosition.y + (selectedRectTransform.pivot.y - 1f) * selectedRectTransform.sizeDelta.y;
		float num2 = 0f - selectedRectTransform.localPosition.y + selectedRectTransform.pivot.y * selectedRectTransform.sizeDelta.y;
		float num3 = contentPanel.localPosition.y + (scrollRectTransform.pivot.y - 1f) * scrollRectTransform.rect.height;
		float num4 = contentPanel.localPosition.y + scrollRectTransform.pivot.y * scrollRectTransform.rect.height;
		if (num2 > num4 || OWMath.ApproxEquals(num2, num4))
		{
			targetPos.y = num2 - scrollRectTransform.pivot.y * scrollRectTransform.rect.height;
		}
		else if (num < num3 || OWMath.ApproxEquals(num, num3))
		{
			targetPos.y = num - (scrollRectTransform.pivot.y - 1f) * scrollRectTransform.rect.height;
		}
		contentPanel.anchoredPosition = targetPos;
		if (lastSelected != currentSelectedGameObject)
		{
			lastSelected = currentSelectedGameObject;
		}
	}
}
