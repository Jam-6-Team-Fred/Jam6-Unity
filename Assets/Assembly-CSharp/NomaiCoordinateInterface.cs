using UnityEngine;

public class NomaiCoordinateInterface : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private OWAudioSource _loopingSource;

	[Space]
	[SerializeField]
	private Transform _basePivot;

	[SerializeField]
	private Transform _pillarRoot;

	[SerializeField]
	private int[] _coordinateX;

	[SerializeField]
	private int[] _coordinateY;

	[SerializeField]
	private int[] _coordinateZ;

	[SerializeField]
	private NomaiNodeController[] _nodeControllers;

	[SerializeField]
	private NomaiInterfaceOrb _orb;

	[SerializeField]
	private NomaiInterfaceOrb _upperOrb;

	[SerializeField]
	private NomaiInterfaceSlot[] _rotateSlots;

	[SerializeField]
	private TransformAnimator[] _gateAnimators;

	[SerializeField]
	private NomaiInterfaceSlot _lowerPillarSlot;

	[SerializeField]
	private NomaiInterfaceSlot _raisePillarSlot;

	[SerializeField]
	private float _loweredHeight;

	private int _activePanelIndex;

	private bool _rotatingToPanel;

	private float _degrees;

	private float _startDegrees;

	private float _targetDegrees;

	private float _startRotationTime;

	private bool _updateHeight;

	private float _targetHeight;

	private bool _pillarRaised = true;

	private bool _finalSlotActivated;

	private bool _powered;

	private void Awake()
	{
		for (int i = 0; i < _rotateSlots.Length; i++)
		{
			_rotateSlots[i].OnSlotActivated += OnRotateSlotActivated;
		}
		_lowerPillarSlot.OnSlotActivated += OnLowerPillarSlotActivated;
		_raisePillarSlot.OnSlotActivated += OnRaisePillarSlotActivated;
	}

	private void Start()
	{
		_loopingSource.SetLocalVolume(0f);
		_gateAnimators[0].TranslateInDirection(-_gateAnimators[0].transform.forward, 0.5f);
		_upperOrb.AddLock();
		LowerPillarImmediate();
	}

	private void LowerPillarImmediate()
	{
		Vector3 position = _pillarRoot.InverseTransformPoint(_orb.transform.position);
		_pillarRoot.localPosition = new Vector3(0f, _loweredHeight, 0f);
		_upperOrb.RemoveLock();
		_orb.AddLock();
		_orb.transform.position = _pillarRoot.TransformPoint(position);
		_pillarRaised = false;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _rotateSlots.Length; i++)
		{
			_rotateSlots[i].OnSlotActivated -= OnRotateSlotActivated;
		}
		_lowerPillarSlot.OnSlotActivated -= OnLowerPillarSlotActivated;
		_raisePillarSlot.OnSlotActivated -= OnRaisePillarSlotActivated;
	}

	public bool CheckEyeCoordinates()
	{
		bool flag = _nodeControllers[0].CheckCoordinate(_coordinateX);
		bool flag2 = _nodeControllers[1].CheckCoordinate(_coordinateY);
		bool flag3 = _nodeControllers[2].CheckCoordinate(_coordinateZ);
		MonoBehaviour.print("coordinate check: " + flag + ", " + flag2 + ", " + flag3);
		return flag && flag2 && flag3;
	}

	public void SetPillarRaised(bool raised, bool powered)
	{
		_powered = powered;
		SetPillarRaised(raised);
	}

	public void SetPillarRaised(bool raised)
	{
		if (LoadManager.GetCurrentScene() != OWScene.EyeOfTheUniverse && (!raised || (_powered && !CheckEyeCoordinates())) && raised != _pillarRaised)
		{
			_pillarRaised = raised;
			_upperOrb.RemoveAllLocks();
			_orb.RemoveAllLocks();
			_upperOrb.AddLock();
			_orb.AddLock();
			_updateHeight = true;
			_targetHeight = (raised ? 0f : _loweredHeight);
			_loopingSource.FadeIn(1f);
		}
	}

	private void FixedUpdate()
	{
		if (_rotatingToPanel)
		{
			float t = Mathf.InverseLerp(_startRotationTime, _startRotationTime + 1f, Time.time);
			t = Mathf.SmoothStep(0f, 1f, t);
			_degrees = Mathf.Lerp(_startDegrees, _targetDegrees, t);
			_basePivot.localEulerAngles = Vector3.up * _degrees;
			if (t >= 1f)
			{
				_rotatingToPanel = false;
				_orb.RemoveLock();
			}
		}
		if (!_updateHeight)
		{
			return;
		}
		float num = Mathf.MoveTowards(_pillarRoot.localPosition.y, _targetHeight, Time.deltaTime);
		if (Mathf.Abs(num - _targetHeight) < 0.01f)
		{
			num = _targetHeight;
			_updateHeight = false;
			_loopingSource.FadeOut(0.2f);
			if (_pillarRaised)
			{
				_orb.RemoveLock();
			}
			else
			{
				_upperOrb.RemoveLock();
			}
		}
		_pillarRoot.localPosition = new Vector3(0f, num, 0f);
	}

	private void OnRaisePillarSlotActivated(NomaiInterfaceSlot slot)
	{
		if (_finalSlotActivated)
		{
			SetPillarRaised(raised: true);
			_finalSlotActivated = false;
		}
	}

	private void OnLowerPillarSlotActivated(NomaiInterfaceSlot slot)
	{
		_finalSlotActivated = true;
		SetPillarRaised(raised: false);
	}

	private void OnRotateSlotActivated(NomaiInterfaceSlot slot)
	{
		int activePanelIndex = _activePanelIndex;
		if (slot == _rotateSlots[0])
		{
			int panelIndex = ((_activePanelIndex == 0) ? 1 : 0);
			RotateToPanel(panelIndex);
		}
		else if (slot == _rotateSlots[1])
		{
			int panelIndex2 = ((_activePanelIndex != 1) ? 1 : 2);
			RotateToPanel(panelIndex2);
		}
		if (activePanelIndex != _activePanelIndex)
		{
			_oneShotSource.PlayOneShot(AudioType.NomaiPillarRotate);
			if (_activePanelIndex == 0)
			{
				OpenGate(0);
				CloseGate(1);
				CloseGate(2);
				CloseGate(3);
			}
			else if (_activePanelIndex == 1)
			{
				CloseGate(0);
				OpenGate(1);
				OpenGate(2);
				CloseGate(3);
			}
			else if (_activePanelIndex == 2)
			{
				CloseGate(0);
				CloseGate(1);
				CloseGate(2);
				OpenGate(3);
			}
		}
	}

	private void OpenGate(int index)
	{
		_gateAnimators[index].TranslateInDirection(-_gateAnimators[index].transform.forward, 0.5f);
	}

	private void CloseGate(int index)
	{
		_gateAnimators[index].TranslateToOriginalLocalPosition(0.5f);
	}

	private void RotateToPanel(int panelIndex)
	{
		if (panelIndex >= 0 && panelIndex < 3 && panelIndex != _activePanelIndex)
		{
			_targetDegrees = panelIndex * 120;
			_startDegrees = _degrees;
			_startRotationTime = Time.time;
			_orb.AddLock();
			_activePanelIndex = panelIndex;
			_rotatingToPanel = true;
		}
	}
}
