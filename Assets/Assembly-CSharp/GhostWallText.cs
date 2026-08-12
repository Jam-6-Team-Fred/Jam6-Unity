using System;
using System.Xml;
using UnityEngine;

public class GhostWallText : NomaiText
{
	[Space]
	[SerializeField]
	private NomaiTextLine _textLine;

	[SerializeField]
	private bool _turnOffFlashlight;

	protected override void Awake()
	{
		base.Awake();
		_dictNomaiTextData = null;
		_listDBConditions = null;
	}

	public override bool CheckTurnOffFlashlight()
	{
		return _turnOffFlashlight;
	}

	public override void LateInitialize()
	{
		_initialized = true;
	}

	protected override void InitializeXmlAsset()
	{
		throw new NotSupportedException();
	}

	public override bool CheckAllowFocus(float focusDistance, Vector3 focusDirection)
	{
		if (focusDistance <= _interactRange)
		{
			return Vector3.Angle(focusDirection, -base.transform.forward) < 90f;
		}
		return false;
	}

	public override void SetNewXmlData(XmlNode rootNode)
	{
		throw new NotSupportedException();
	}

	public override void SetAsTranslated(int id)
	{
		throw new NotSupportedException();
	}

	public override int GetNumTextBlocksTranslated()
	{
		throw new NotSupportedException();
	}

	public override bool IsTranslated(int id)
	{
		throw new NotSupportedException();
	}

	public override int GetNumTextBlocks()
	{
		throw new NotSupportedException();
	}

	public override string GetTextNode(int id)
	{
		throw new NotSupportedException();
	}

	public NomaiTextLine GetTextLine()
	{
		return _textLine;
	}
}
