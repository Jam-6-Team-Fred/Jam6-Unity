using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class DialogueText
{
	private struct TextBlock
	{
		public string condition;

		public List<string> listPageText;

		public TextBlock(List<string> textPages, string conditionValue)
		{
			listPageText = textPages;
			condition = conditionValue;
		}
	}

	private List<TextBlock> _listTextBlocks;

	private bool _randomize;

	private DialogueConditionManager _condMgr;

	public DialogueText(IEnumerable<XElement> dialogueNodeList, bool randomize)
	{
		_condMgr = DialogueConditionManager.SharedInstance;
		_randomize = randomize;
		_listTextBlocks = new List<TextBlock>();
		foreach (XElement dialogueNode in dialogueNodeList)
		{
			List<string> list = new List<string>();
			foreach (XElement item2 in dialogueNode.Elements("Page"))
			{
				string text = OWUtilities.CleanupXmlText(item2.Value, replaceBlackslashN: false);
				list.Add(text.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "")
					.Replace("]]>", ""));
			}
			string conditionValue = string.Empty;
			XElement xElement = dialogueNode.Element("RequiredCondition");
			if (xElement != null)
			{
				conditionValue = xElement.Value;
			}
			TextBlock item = new TextBlock(list, conditionValue);
			_listTextBlocks.Add(item);
		}
	}

	public List<string> GetDisplayStringList()
	{
		if (_randomize)
		{
			List<TextBlock> list = new List<TextBlock>();
			for (int i = 0; i < _listTextBlocks.Count; i++)
			{
				TextBlock textBlock = _listTextBlocks[i];
				if (BlockSatisfiesConditions(textBlock))
				{
					list.Add(textBlock);
				}
			}
			if (list.Count > 0)
			{
				return list[Random.Range(0, list.Count)].listPageText;
			}
		}
		else
		{
			for (int num = _listTextBlocks.Count - 1; num >= 0; num--)
			{
				TextBlock block = _listTextBlocks[num];
				if (BlockSatisfiesConditions(block))
				{
					return block.listPageText;
				}
			}
		}
		Debug.LogError("No conditions satisfied! Did you not include a default/condition-less text option?");
		return null;
	}

	private bool BlockSatisfiesConditions(TextBlock block)
	{
		bool result = true;
		if (block.condition != string.Empty)
		{
			if (_condMgr.ConditionExists(block.condition))
			{
				if (!_condMgr.GetConditionState(block.condition))
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
		}
		return result;
	}
}
