using System;

[Serializable]
public class ShipLogFactSave
{
	public string id;

	public int revealOrder;

	public bool read;

	public bool newlyRevealed;

	public ShipLogFactSave(string id)
	{
		this.id = id;
		revealOrder = -1;
		read = false;
		newlyRevealed = false;
	}
}
