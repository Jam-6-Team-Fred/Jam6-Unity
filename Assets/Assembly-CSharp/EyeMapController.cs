using UnityEngine;

public class EyeMapController : MonoBehaviour
{
	private void Update()
	{
		if (OWInput.IsInputMode(InputMode.Character | InputMode.ShipCockpit) && OWInput.IsNewlyPressed(InputLibrary.map))
		{
			NotificationManager.SharedInstance.PostNotification(new NotificationData(UITextLibrary.GetString(UITextType.NotificationUnableToOpenMap)));
		}
	}
}
