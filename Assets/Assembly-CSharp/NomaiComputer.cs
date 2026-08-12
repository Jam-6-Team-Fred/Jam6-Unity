using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class NomaiComputer : NomaiText
{
	private struct Command
	{
		public enum Type
		{
			None = 0,
			Display = 1,
			Clear = 2,
			Wait = 3
		}

		public Type type;

		public int id;

		public float time;

		public Command(Type t)
		{
			type = t;
			id = -1;
			time = -1f;
		}

		public Command(Type t, int i)
		{
			type = t;
			id = i;
			time = -1f;
		}

		public Command(Type t, float f)
		{
			type = t;
			id = -1;
			time = f;
		}
	}

	private CapsuleCollider _capsuleCollider;

	[SerializeField]
	private bool _startActivated;

	[SerializeField]
	private float _dockedHeight;

	[SerializeField]
	private float _minHeight = 0.5f;

	[SerializeField]
	private float _maxHeight = 2f;

	[Space(10f)]
	[SerializeField]
	private OWAudioSource _ambientAudioSource;

	[SerializeField]
	private OWAudioSource _oneShotAudioSource;

	[SerializeField]
	private float _fadeLength = 0.5f;

	private AudioClip[] _activateClips;

	private AudioClip[] _deactivateClips;

	private NomaiComputerRing _movingRing;

	private List<NomaiComputerRing> _inactiveRingList;

	private List<NomaiComputerRing> _activeRingList;

	private bool _dynamicRingSpacing;

	private int _numRings;

	private Command _currentCommand;

	private Queue<Command> _commandQueue;

	private int _numEntries;

	private float _waitTimer;

	protected override void Awake()
	{
		base.Awake();
		_capsuleCollider = GetComponent<CapsuleCollider>();
		NomaiComputerRing[] componentsInChildren = GetComponentsInChildren<NomaiComputerRing>();
		_movingRing = null;
		_inactiveRingList = new List<NomaiComputerRing>(componentsInChildren.Length);
		_activeRingList = new List<NomaiComputerRing>(componentsInChildren.Length);
		_dynamicRingSpacing = true;
		_numRings = componentsInChildren.Length;
		_waitTimer = 0f;
		_currentCommand = new Command(Command.Type.None);
		_commandQueue = new Queue<Command>(8);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Initialize();
			componentsInChildren[i].Deactivate(_dockedHeight);
			_inactiveRingList.Add(componentsInChildren[i]);
		}
		if (_sector == null)
		{
			Debug.LogWarning("NomaiComputer does not have a sector for its Rings!  Make sure it's parented to one, or set it manually.", this);
		}
	}

	protected void Start()
	{
		_numEntries = GetNumTextBlocks();
		if (_startActivated)
		{
			DisplayAllEntries();
		}
		_capsuleCollider.enabled = _startActivated;
		if (_ambientAudioSource != null)
		{
			_ambientAudioSource.SetLocalVolume(0f);
		}
		else
		{
			Debug.LogError("Nomai computer audio missing!");
			Debug.Break();
		}
		_activateClips = Locator.GetAudioManager().GetAudioClipArray(AudioType.NomaiComputerRingActivate);
		_deactivateClips = Locator.GetAudioManager().GetAudioClipArray(AudioType.NomaiComputerRingDeactivate);
		base.enabled = false;
	}

	protected override void InitializeXmlAsset()
	{
		if (_dictNomaiTextData == null)
		{
			_dictNomaiTextData = new Dictionary<int, NomaiTextData>(ComparerLibrary.intEqComparer);
		}
		if (_nomaiTextAsset == null)
		{
			_dictNomaiTextData.Add(0, default(NomaiTextData));
		}
		else
		{
			LoadXml();
		}
	}

	public override void SetNewXmlData(XmlNode rootNode)
	{
		base.SetNewXmlData(rootNode);
		_numEntries = GetNumTextBlocks();
	}

	public override void SetAsTranslated(int id)
	{
		base.SetAsTranslated(id);
		for (int i = 0; i < _activeRingList.Count; i++)
		{
			if (_activeRingList[i].GetEntryID() == id)
			{
				_activeRingList[i].SetAsTranslated();
				break;
			}
		}
	}

	private void Update()
	{
		if (_activeRingList.Count > 0 && !_ambientAudioSource.isPlaying)
		{
			_ambientAudioSource.FadeIn(_fadeLength, fadeFromNothing: true);
		}
		else if (_activeRingList.Count == 0 && _ambientAudioSource.isPlaying && !_ambientAudioSource.IsFadingOut())
		{
			_ambientAudioSource.FadeOut(_fadeLength);
		}
		if (_currentCommand.type == Command.Type.None && _commandQueue.Count > 0)
		{
			if (_commandQueue.Peek().type == Command.Type.Display && _inactiveRingList.Count == 0 && (_movingRing == null || _movingRing.IsActivated()))
			{
				_currentCommand = new Command(Command.Type.Clear);
			}
			else
			{
				_currentCommand = _commandQueue.Dequeue();
				if (_currentCommand.type == Command.Type.Wait)
				{
					_waitTimer = _currentCommand.time;
				}
			}
		}
		switch (_currentCommand.type)
		{
		case Command.Type.Display:
			if (_movingRing == null)
			{
				_movingRing = _inactiveRingList[0];
				_inactiveRingList.RemoveAt(0);
				_movingRing.Activate(_currentCommand.id);
				int num2 = _currentCommand.id;
				if (_activateClips.Length != 0 && num2 > -1)
				{
					while (num2 > _activateClips.Length)
					{
						num2 -= _activateClips.Length;
					}
					num2--;
					_oneShotAudioSource.PlayOneShot(_activateClips[num2], Locator.GetAudioManager().GetAudioEntry(AudioType.NomaiComputerRingActivate).volume);
				}
				RecalculateRingHeightsDynamically();
			}
			else if (_movingRing.IsInPosition())
			{
				_activeRingList.Add(_movingRing);
				_movingRing = null;
				_currentCommand = new Command(Command.Type.None);
			}
			break;
		case Command.Type.Clear:
			if (_movingRing == null)
			{
				for (int i = 0; i < _activeRingList.Count; i++)
				{
					if (_activeRingList[i].GetEntryID() != _currentCommand.id && _currentCommand.id >= 0)
					{
						continue;
					}
					_movingRing = _activeRingList[i];
					_activeRingList.RemoveAt(i);
					_movingRing.Deactivate(_dockedHeight);
					int num = _currentCommand.id;
					if (_deactivateClips.Length != 0 && num > -1)
					{
						while (num > _deactivateClips.Length)
						{
							num -= _deactivateClips.Length;
						}
						num--;
						_oneShotAudioSource.PlayOneShot(_deactivateClips[num], Locator.GetAudioManager().GetAudioEntry(AudioType.NomaiComputerRingDeactivate).volume);
					}
					RecalculateRingHeightsDynamically();
					break;
				}
				if (_movingRing == null)
				{
					_currentCommand = new Command(Command.Type.None);
				}
			}
			else if (_movingRing.IsInPosition())
			{
				_inactiveRingList.Add(_movingRing);
				_movingRing = null;
				_currentCommand = new Command(Command.Type.None);
			}
			break;
		case Command.Type.Wait:
			_waitTimer -= Time.deltaTime;
			if (_waitTimer <= 0f)
			{
				_currentCommand = new Command(Command.Type.None);
			}
			break;
		case Command.Type.None:
			if (_activeRingList.Count == 0)
			{
				_dynamicRingSpacing = true;
			}
			break;
		}
	}

	private void FixedUpdate()
	{
		bool flag = _activeRingList.Count > 0 || _movingRing != null;
		if (_capsuleCollider.enabled != flag)
		{
			_capsuleCollider.enabled = flag;
		}
		if (flag)
		{
			float num = ((_movingRing != null) ? _movingRing.GetCurrentHeight() : 0f);
			for (int i = 0; i < _activeRingList.Count; i++)
			{
				num = Mathf.Max(num, _activeRingList[i].GetCurrentHeight());
			}
			num -= _dockedHeight;
			_capsuleCollider.height = num + _capsuleCollider.radius * 2f;
			_capsuleCollider.center = new Vector3(0f, num * 0.5f + _dockedHeight, 0f);
		}
	}

	private void RecalculateRingHeightsDynamically()
	{
		float num = 1.7f;
		float num2 = 0.5f;
		bool flag = _movingRing != null && _movingRing.IsActivated();
		int num3 = _activeRingList.Count + (flag ? 1 : 0);
		float num4 = num2 * (float)(num3 - 1);
		float num5 = Mathf.Max(1f, num - num4 * 0.5f);
		if (flag)
		{
			_movingRing.SetHoverHeight(num5);
			num5 += num2;
		}
		for (int num6 = _activeRingList.Count - 1; num6 >= 0; num6--)
		{
			_activeRingList[num6].SetHoverHeight(num5);
			num5 += num2;
		}
	}

	private void RecalculateRingHeights()
	{
		if (_dynamicRingSpacing)
		{
			int num = _activeRingList.Count + 1;
			if (_movingRing != null && _movingRing.IsActivated())
			{
				float num2 = ((num < 3) ? ((float)num / (float)(num + 1)) : 1f);
				float hoverHeight = Mathf.Lerp(_minHeight, _maxHeight, 1f - num2);
				_movingRing.SetHoverHeight(hoverHeight);
			}
			for (int i = 0; i < _activeRingList.Count; i++)
			{
				float num3 = ((num < 3) ? ((float)(i + 1) / (float)(num + 1)) : ((float)i / (float)(num - 1)));
				float hoverHeight2 = Mathf.Lerp(_minHeight, _maxHeight, 1f - num3);
				_activeRingList[i].SetHoverHeight(hoverHeight2);
			}
		}
		else
		{
			for (int j = 0; j < _activeRingList.Count; j++)
			{
				float num4 = (float)j / (float)(_numRings - 1);
				float hoverHeight3 = Mathf.Lerp(_minHeight, _maxHeight, 1f - num4);
				_activeRingList[j].SetHoverHeight(hoverHeight3);
			}
			if (_movingRing != null && _movingRing.IsActivated())
			{
				float num5 = (float)_activeRingList.Count / (float)(_numRings - 1);
				float hoverHeight4 = Mathf.Lerp(_minHeight, _maxHeight, 1f - num5);
				_movingRing.SetHoverHeight(hoverHeight4);
			}
		}
	}

	public void DisplayAllEntries()
	{
		for (int i = 0; i < _numEntries; i++)
		{
			_commandQueue.Enqueue(new Command(Command.Type.Display, i + 1));
		}
		_dynamicRingSpacing = false;
	}

	public void ClearAllEntries()
	{
		_commandQueue.Clear();
		for (int i = 0; i < _activeRingList.Count; i++)
		{
			_commandQueue.Enqueue(new Command(Command.Type.Clear, _activeRingList[i].GetEntryID()));
		}
		if (_movingRing != null && _movingRing.IsActivated())
		{
			_commandQueue.Enqueue(new Command(Command.Type.Clear, _movingRing.GetEntryID()));
		}
	}

	public void DisplayEntry(int index)
	{
		_commandQueue.Enqueue(new Command(Command.Type.Display, index));
	}

	public void ClearEntry(int index = -1)
	{
		_commandQueue.Enqueue(new Command(Command.Type.Clear, index));
	}

	public void Wait(float waitTime)
	{
		_commandQueue.Enqueue(new Command(Command.Type.Wait, waitTime));
	}

	public NomaiComputerRing GetClosestRing(Vector3 worldPos, out float verticalDist)
	{
		Vector3 vector = base.transform.InverseTransformPoint(worldPos);
		float num = float.PositiveInfinity;
		NomaiComputerRing result = null;
		for (int i = 0; i < _activeRingList.Count; i++)
		{
			float num2 = Mathf.Abs(vector.y - _activeRingList[i].GetCurrentHeight());
			if (num2 < num)
			{
				num = num2;
				result = _activeRingList[i];
			}
		}
		verticalDist = num;
		return result;
	}

	public NomaiComputerRing GetClosestRing(Vector3 worldPos)
	{
		float verticalDist;
		return GetClosestRing(worldPos, out verticalDist);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.red;
		OWGizmos.DrawWireCircle(Vector3.up * _dockedHeight, Vector3.up, 1.5f);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(Vector3.up * _minHeight, Vector3.up * _maxHeight);
		Gizmos.DrawRay(Vector3.up * _minHeight, Vector3.forward * 1.25f);
		Gizmos.DrawRay(Vector3.up * _maxHeight, Vector3.forward * 1f);
		OWGizmos.DrawWireCircle(Vector3.up * _minHeight, Vector3.up, 1.25f);
		OWGizmos.DrawWireCircle(Vector3.up * _maxHeight, Vector3.up, 1f);
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			base.enabled = true;
			for (int i = 0; i < _inactiveRingList.Count; i++)
			{
				_inactiveRingList[i].enabled = true;
			}
			for (int j = 0; j < _activeRingList.Count; j++)
			{
				_activeRingList[j].enabled = true;
			}
			if (_movingRing != null)
			{
				_movingRing.enabled = true;
			}
			return;
		}
		base.enabled = false;
		for (int k = 0; k < _inactiveRingList.Count; k++)
		{
			_inactiveRingList[k].enabled = false;
		}
		for (int l = 0; l < _activeRingList.Count; l++)
		{
			_activeRingList[l].enabled = false;
		}
		if (_movingRing != null)
		{
			_movingRing.enabled = false;
		}
		_ambientAudioSource.FadeOut(_fadeLength);
		_capsuleCollider.enabled = false;
	}
}
