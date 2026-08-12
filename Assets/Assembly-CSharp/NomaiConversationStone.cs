using UnityEngine;

[SelectionBase]
public class NomaiConversationStone : OWItem
{
	[Space]
	[SerializeField]
	private NomaiWord _word;

	[SerializeField]
	private OWRenderer _decalRenderer;

	private bool _revealed;

	private float _fade;

	private Color _baseColor;

	protected override void Awake()
	{
		base.Awake();
		_type = ItemType.ConversationStone;
		_revealed = false;
		_fade = 0f;
		_baseColor = _decalRenderer.GetOriginalColor();
		SetColliderActivation(active: false);
		_decalRenderer.SetActivation(active: false);
		_decalRenderer.SetColor(new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0f));
		base.enabled = false;
	}

	private void Update()
	{
		if (!_revealed)
		{
			base.enabled = false;
			return;
		}
		_fade += Time.deltaTime;
		_decalRenderer.SetColor(new Color(_baseColor.r, _baseColor.g, _baseColor.b, _baseColor.a * Mathf.Clamp01(_fade)));
		if (_fade >= 1f)
		{
			base.enabled = false;
		}
	}

	public override string GetDisplayName()
	{
		switch (_word)
		{
		case NomaiWord.Explain:
			return UITextLibrary.GetString(UITextType.SolanumStonePrompt_Explain);
		case NomaiWord.Eye:
			return UITextLibrary.GetString(UITextType.SolanumStonePrompt_Eye);
		case NomaiWord.Identify:
			return UITextLibrary.GetString(UITextType.SolanumStonePrompt_Identify);
		case NomaiWord.Me:
			return UITextLibrary.GetString(UITextType.SolanumStonePrompt_Me);
		case NomaiWord.QuantumMoon:
			return UITextLibrary.GetString(UITextType.SolanumStonePrompt_QM);
		case NomaiWord.You:
			return UITextLibrary.GetString(UITextType.SolanumStonePrompt_You);
		default:
			return "ERROR";
		}
	}

	public NomaiWord GetWord()
	{
		return _word;
	}

	public void Reveal()
	{
		_revealed = true;
		SetColliderActivation(active: true);
		_decalRenderer.SetActivation(active: true);
		base.enabled = true;
	}
}
