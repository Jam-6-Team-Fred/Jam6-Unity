using System.Text;
using System.Xml;
using UnityEngine;

[RequireComponent(typeof(NomaiComputer))]
public class NomaiWarpComputerLogger : MonoBehaviour
{
	[SerializeField]
	private NomaiWarpReceiver _warpReceiver;

	private NomaiComputer _computer;

	private StringBuilder _strBuilder;

	private XmlNode _rootNode;

	private XmlNode _textNode;

	private void Awake()
	{
		_computer = this.GetRequiredComponent<NomaiComputer>();
		_strBuilder = new StringBuilder();
		XmlDocument xmlDoc = new XmlDocument();
		_rootNode = xmlDoc.CreateNode("NomaiObject");
		XmlNode xmlNode = xmlDoc.CreateNode("TextBlock");
		XmlNode xmlNode2 = xmlDoc.CreateNode("ID");
		xmlNode2.SetValue(1.ToString());
		_textNode = xmlDoc.CreateNode("Text");
		xmlNode.AppendChild(xmlNode2);
		xmlNode.AppendChild(_textNode);
		_rootNode.AppendChild(xmlNode);
		XmlNode xmlNode3 = xmlDoc.CreateNode("TextBlock");
		XmlNode xmlNode4 = xmlDoc.CreateNode("ID");
		xmlNode4.SetValue(2.ToString());
		XmlNode xmlNode5 = xmlDoc.CreateNode("Text");
		xmlNode5.SetValue(UITextLibrary.GetString(UITextType.NomaiReturnWarpMessage));
		xmlNode3.AppendChild(xmlNode4);
		xmlNode3.AppendChild(xmlNode5);
		_rootNode.AppendChild(xmlNode3);
		if (_warpReceiver != null)
		{
			_warpReceiver.OnReceiveWarpedBody += OnReceiveWarpedBody;
		}
	}

	private void OnDestroy()
	{
		if (_warpReceiver != null)
		{
			_warpReceiver.OnReceiveWarpedBody -= OnReceiveWarpedBody;
		}
	}

	public void OnReceiveWarpedBody(OWRigidbody body, NomaiWarpPlatform startPlatform, NomaiWarpPlatform targetPlatform)
	{
		if (_strBuilder == null)
		{
			Debug.LogError("NomaiWarpComputerLogger not Initialized!");
			return;
		}
		_computer.ClearAllEntries();
		_strBuilder.Length = 0;
		float num = Mathf.Round(TimeLoop.GetSecondsElapsed() * 1000f) / 1000f;
		float num2 = Mathf.Round(Random.Range(1f, 9f));
		string value = num + "0" + num2;
		string value2 = num + "0" + (num2 - 1f);
		_strBuilder.Append(UITextLibrary.GetString(UITextType.NomaiDepartureMessage));
		_strBuilder.Append(value);
		_strBuilder.Append("\n");
		_strBuilder.Append(UITextLibrary.GetString(UITextType.NomaiArrivalMessage));
		_strBuilder.Append(value2);
		_textNode.SetValue(_strBuilder.ToString());
		_computer.SetNewXmlData(_rootNode);
		_computer.DisplayAllEntries();
	}
}
