public class NomaiConversationStoneSocket : OWItemSocket
{
	protected override void Awake()
	{
		base.Awake();
		_acceptableType = ItemType.ConversationStone;
	}
}
