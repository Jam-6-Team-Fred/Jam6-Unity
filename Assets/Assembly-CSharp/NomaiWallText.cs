using System.Collections.Generic;
using UnityEngine;

public class NomaiWallText : NomaiText
{
	private enum AnimationState
	{
		STANDBY = 0,
		HIDE = 1,
		SHOW = 2
	}

	public static bool s_revealAllAtStart;

	private bool _showTextOnStart = true;

	private bool _wasShownImmediatedly;

	private bool _useLegacyRevealAnimation;

	private OWTreeNode<NomaiTextLine> _rootTreeNode;

	private Dictionary<int, OWTreeNode<NomaiTextLine>> _idToNodeDict;

	private Dictionary<NomaiTextLine, OWTreeNode<NomaiTextLine>> _lineToNodeDict;

	private bool _textLinesInitialized;

	private NomaiTextLine[] _textLines;

	private AnimationState _animationState;

	private float _hideAnimDuration;

	private OWCollider _collider;

	protected override void Awake()
	{
		base.Awake();
		_textLines = GetComponentsInChildren<NomaiTextLine>();
		_animationState = AnimationState.STANDBY;
		_collider = base.gameObject.GetAddComponent<OWCollider>();
		base.enabled = false;
	}

	public override void LateInitialize()
	{
		base.LateInitialize();
		if (_textLines.Length < _dictNomaiTextData.Count)
		{
			Debug.LogError("CRITICAL ERROR!  NomaiWallText \"" + base.name + "\" has fewer arcs than text lines in NomaiTextAsset.  This will result in inaccessible writing.", this);
			Debug.Break();
		}
		_idToNodeDict = new Dictionary<int, OWTreeNode<NomaiTextLine>>();
		_lineToNodeDict = new Dictionary<NomaiTextLine, OWTreeNode<NomaiTextLine>>();
		NomaiTextLine[] textLines = _textLines;
		foreach (NomaiTextLine nomaiTextLine in textLines)
		{
			int entryID = nomaiTextLine.GetEntryID();
			if (_idToNodeDict.ContainsKey(entryID))
			{
				Debug.LogError("Duplicate NomaiTextLine ID found. Aborting initialization.", this);
				return;
			}
			OWTreeNode<NomaiTextLine> value = new OWTreeNode<NomaiTextLine>(nomaiTextLine);
			_idToNodeDict.Add(entryID, value);
			_lineToNodeDict.Add(nomaiTextLine, value);
		}
		bool flag = false;
		OWTreeNode<NomaiTextLine> oWTreeNode = null;
		textLines = _textLines;
		for (int i = 0; i < textLines.Length; i++)
		{
			int entryID2 = textLines[i].GetEntryID();
			if (_dictNomaiTextData.ContainsKey(entryID2))
			{
				int parentID = _dictNomaiTextData[entryID2].ParentID;
				if (_idToNodeDict.ContainsKey(parentID) && _idToNodeDict.ContainsKey(entryID2))
				{
					flag = true;
					oWTreeNode = _idToNodeDict[entryID2];
					oWTreeNode.SetParent(_idToNodeDict[parentID]);
				}
			}
		}
		if (flag)
		{
			if (oWTreeNode.IsRootNode())
			{
				_rootTreeNode = oWTreeNode;
			}
			else
			{
				_rootTreeNode = oWTreeNode.GetRootNode();
			}
		}
		else if (_idToNodeDict.ContainsKey(1))
		{
			_rootTreeNode = _idToNodeDict[1];
		}
		_useLegacyRevealAnimation = !flag;
		InitializeNomaiTextLines();
		if (_showTextOnStart)
		{
			ShowImmediate();
		}
		else if (!_wasShownImmediatedly)
		{
			_collider.SetActivation(active: false);
		}
	}

	public void HideTextOnStart()
	{
		_showTextOnStart = false;
		if (_textLinesInitialized)
		{
			Debug.LogWarning("You're too late to set text reveal on start (it's already been initialized)", this);
			Debug.Break();
		}
	}

	public void InitializeAsWhiteboardText()
	{
		_showTextOnStart = false;
		_minimumReadableDistance = 1.5f;
		if (_textLinesInitialized)
		{
			Debug.LogWarning("You're too late to set text reveal on start (it's already been initialized)", this);
			Debug.Break();
		}
	}

	public override bool CheckAllowFocus(float focusDistance, Vector3 focusDirection)
	{
		if (focusDistance <= _interactRange)
		{
			return Vector3.Angle(focusDirection, -base.transform.forward) < 90f;
		}
		return false;
	}

	public bool IsAnyTextAnimating()
	{
		if (_animationState != 0)
		{
			return true;
		}
		for (int i = 0; i < _textLines.Length; i++)
		{
			if (_textLines[i].IsRevealing())
			{
				return true;
			}
		}
		return false;
	}

	public void Show()
	{
		VerifyInitialized();
		_animationState = AnimationState.SHOW;
		_collider.SetActivation(active: true);
		UpdateAnimation();
		Locator.GetPlayerAudioController().PlayNomaiTextReveal(this);
		base.enabled = true;
	}

	public void Hide(float animDuration = 1f)
	{
		VerifyInitialized();
		_collider.SetActivation(active: false);
		_animationState = AnimationState.HIDE;
		_hideAnimDuration = animDuration;
		Locator.GetPlayerAudioController().PlayNomaiTextReveal(this);
		base.enabled = true;
	}

	public void ShowImmediate()
	{
		for (int i = 0; i < _textLines.Length; i++)
		{
			_textLines[i].SetActive(active: true, playAnimation: false);
		}
		_collider.SetActivation(active: true);
		_wasShownImmediatedly = true;
	}

	public void HideImmediate()
	{
		for (int i = 0; i < _textLines.Length; i++)
		{
			_textLines[i].SetActive(active: false, playAnimation: false);
		}
		_collider.SetActivation(active: false);
	}

	public NomaiTextLine GetClosestTextLineBySegment(Vector3 worldPos)
	{
		NomaiTextLine result = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _textLines.Length; i++)
		{
			if (!(_textLines[i] == null))
			{
				float sqrMagnitude = (_textLines[i].ClosestPointOnLine(worldPos) - worldPos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = _textLines[i];
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public NomaiTextLine GetClosestTextLineByCenter(Vector3 worldPos)
	{
		NomaiTextLine result = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _textLines.Length; i++)
		{
			if (!(_textLines[i] == null) && !_textLines[i].IsHidden())
			{
				Vector3 worldCenter = _textLines[i].GetWorldCenter();
				float worldRadius = _textLines[i].GetWorldRadius();
				float sqrMagnitude = (worldCenter - worldPos).sqrMagnitude;
				if (sqrMagnitude < worldRadius * worldRadius && sqrMagnitude < num)
				{
					result = _textLines[i];
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public override bool IsAnimationPlaying()
	{
		return _animationState != AnimationState.STANDBY;
	}

	public override void SetAsTranslated(int id)
	{
		base.SetAsTranslated(id);
		bool flag = false;
		if (!_idToNodeDict.ContainsKey(id))
		{
			return;
		}
		if (_useLegacyRevealAnimation)
		{
			if (_idToNodeDict.ContainsKey(id))
			{
				bool flag2 = false;
				if (!_idToNodeDict[id].Value.IsTranslated())
				{
					if (id == 1)
					{
						_idToNodeDict[id].Value.SetTranslatedState();
						flag2 = true;
					}
					else if (IsTranslated(id - 1))
					{
						_idToNodeDict[id].Value.SetTranslatedState();
						flag2 = true;
					}
				}
				if (flag2 && _idToNodeDict.ContainsKey(id + 1))
				{
					_idToNodeDict[id + 1].Value.SetUnreadState();
					_idToNodeDict[id + 1].Value.SetActive(active: true, playAnimation: true);
				}
			}
		}
		else
		{
			NomaiTextLine value = _idToNodeDict[id].Value;
			if (_lineToNodeDict.ContainsKey(value))
			{
				OWTreeNode<NomaiTextLine> oWTreeNode = _lineToNodeDict[value];
				bool flag3 = false;
				if (oWTreeNode.Parent == null)
				{
					flag3 = true;
				}
				else if (oWTreeNode.Parent.Value.IsTranslated())
				{
					flag3 = true;
				}
				if (flag3 && !_idToNodeDict[id].Value.IsTranslated())
				{
					_idToNodeDict[id].Value.SetTranslatedState();
					if (!s_revealAllAtStart)
					{
						for (int i = 0; i < oWTreeNode.Children.Count; i++)
						{
							oWTreeNode.Children[i].Value.SetUnreadState();
							flag = true;
						}
					}
				}
			}
		}
		if (flag)
		{
			Locator.GetPlayerAudioController().PlayNomaiTextReveal(this);
		}
	}

	private void InitializeNomaiTextLines()
	{
		if (_textLinesInitialized)
		{
			return;
		}
		bool flag = false;
		NomaiTextLine[] textLines = _textLines;
		foreach (NomaiTextLine nomaiTextLine in textLines)
		{
			int entryID = nomaiTextLine.GetEntryID();
			if (_dictNomaiTextData.ContainsKey(entryID))
			{
				nomaiTextLine.SetLocations(_dictNomaiTextData[entryID].SpeakerLocation, _location);
			}
			if (s_revealAllAtStart)
			{
				nomaiTextLine.SetUnreadState(updateImmediate: true);
				flag = true;
			}
			else if (_useLegacyRevealAnimation && entryID == 1)
			{
				nomaiTextLine.SetUnreadState(updateImmediate: true);
				flag = true;
			}
			else if (nomaiTextLine == _rootTreeNode.Value)
			{
				nomaiTextLine.SetUnreadState(updateImmediate: true);
				flag = true;
			}
			nomaiTextLine.UpdateColorImmediate();
		}
		if (!flag)
		{
			Debug.LogWarning("Unable to determine first revealed NomaiTextLine. Revealing all lines.", this);
			textLines = _textLines;
			for (int i = 0; i < textLines.Length; i++)
			{
				textLines[i].SetUnreadState(updateImmediate: true);
			}
		}
		_textLinesInitialized = true;
	}

	private void Update()
	{
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		if (_animationState == AnimationState.SHOW)
		{
			if (!((!_useLegacyRevealAnimation) ? ShowAnimate(_rootTreeNode) : ShowAnimationByID()))
			{
				base.enabled = false;
				_animationState = AnimationState.STANDBY;
			}
		}
		else if (_animationState == AnimationState.HIDE && !((!_useLegacyRevealAnimation) ? HideAnimateSimultaneous() : HideAnimationByID()))
		{
			base.enabled = false;
			_animationState = AnimationState.STANDBY;
		}
	}

	private bool ShowAnimate(OWTreeNode<NomaiTextLine> node)
	{
		if (!node.Value.IsActive())
		{
			node.Value.SetActive(active: true, playAnimation: true);
			return true;
		}
		if (node.Value.IsAnimating())
		{
			return true;
		}
		bool result = false;
		for (int i = 0; i < node.Children.Count; i++)
		{
			if (ShowAnimate(node.Children[i]))
			{
				result = true;
			}
		}
		return result;
	}

	private bool HideAnimateSimultaneous()
	{
		bool result = false;
		for (int i = 0; i < _textLines.Length; i++)
		{
			if (_textLines[i].IsActive())
			{
				_textLines[i].SetActive(active: false, playAnimation: true, _hideAnimDuration);
			}
			if (_textLines[i].IsAnimating())
			{
				result = true;
			}
		}
		return result;
	}

	private bool HideAnimate(OWTreeNode<NomaiTextLine> node)
	{
		if (node.Value.IsActive())
		{
			bool flag = false;
			if (node.Children.Count > 0)
			{
				for (int i = 0; i < node.Children.Count; i++)
				{
					if (HideAnimate(node.Children[i]))
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				node.Value.SetActive(active: false, playAnimation: true);
			}
			return true;
		}
		return node.Value.IsAnimating();
	}

	private bool ShowAnimationByID()
	{
		if (_idToNodeDict.Count == 0)
		{
			return false;
		}
		for (int i = 1; i <= _idToNodeDict.Count; i++)
		{
			if (_idToNodeDict.ContainsKey(i))
			{
				if (!_idToNodeDict[i].Value.IsActive())
				{
					if (i == 1 || !_idToNodeDict[i].Value.IsHidden())
					{
						_idToNodeDict[i].Value.SetActive(active: true, playAnimation: true);
						return true;
					}
				}
				else if (!_idToNodeDict[i].Value.IsAnimationComplete())
				{
					return true;
				}
				if (i == _idToNodeDict.Count)
				{
					return false;
				}
				continue;
			}
			Debug.LogWarning("ID numbers not sequential. Showing all lines.");
			for (int j = 0; j < _idToNodeDict.Count; j++)
			{
				_idToNodeDict[i].Value.SetActive(active: true, playAnimation: true);
			}
			return false;
		}
		Debug.LogWarning("That's strange... Possibly number of Arcs do not match the number of text blocks in the Nomai Text.", this);
		return false;
	}

	private bool HideAnimationByID()
	{
		if (_idToNodeDict.Count == 0)
		{
			return false;
		}
		int num = _idToNodeDict.Count;
		while (num >= 1)
		{
			if (_idToNodeDict.ContainsKey(num))
			{
				if (_idToNodeDict[num].Value.IsActive())
				{
					_idToNodeDict[num].Value.SetActive(active: false, playAnimation: true);
					return true;
				}
				if (!_idToNodeDict[num].Value.IsAnimationComplete())
				{
					return true;
				}
				if (num == 1)
				{
					return false;
				}
				num--;
				continue;
			}
			Debug.LogWarning("ID numbers not sequential. Hiding all lines.");
			for (int i = 0; i < _idToNodeDict.Count; i++)
			{
				_idToNodeDict[num].Value.SetActive(active: false, playAnimation: true);
			}
			return false;
		}
		Debug.LogWarning("That's strange...", this);
		return false;
	}
}
