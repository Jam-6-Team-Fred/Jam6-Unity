using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class SelectableAudioPlayer : MonoBehaviour, IEventSystemHandler, ISelectHandler
{
	private bool _silenceNextSelect;

	public void SilenceNextSelectEvent()
	{
		_silenceNextSelect = true;
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (_silenceNextSelect)
		{
			_silenceNextSelect = false;
		}
		else
		{
			Locator.GetMenuAudioController().PlayButtonFocus();
		}
	}
}
