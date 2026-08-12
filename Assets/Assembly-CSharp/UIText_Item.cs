using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIText_Item : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public bool active;

	public Color orangeColor;

	public Transform myPanel;

	private void Start()
	{
		Debug.Log("started");
		if (base.transform.childCount > 0)
		{
			myPanel = base.transform.Find("Panel");
			Debug.Log("My Panel name = " + myPanel.name);
			Debug.Log("Made it to the bottom of the start");
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Debug.Log("This is when we're turning the text color white");
		GetComponent<Text>().color = Color.white;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Debug.Log("This is when we're turning the text color orange");
		GetComponent<Text>().color = orangeColor;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("Clicked");
		if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			active = true;
			GetComponent<Animator>().SetTrigger("Fetch");
		}
		else
		{
			active = false;
			GetComponent<Animator>().SetTrigger("Idle");
		}
		foreach (Transform listOfAllMenuItem in UISample_Controller.Instance.listOfAllMenuItems)
		{
			if (listOfAllMenuItem.GetComponent<UIText_Item>().active && !listOfAllMenuItem.Equals(base.transform))
			{
				listOfAllMenuItem.GetComponent<UIText_Item>().active = false;
				listOfAllMenuItem.GetComponent<Animator>().SetTrigger("Idle");
			}
		}
	}
}
