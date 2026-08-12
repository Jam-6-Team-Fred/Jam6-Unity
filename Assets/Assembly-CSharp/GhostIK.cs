using UnityEngine;

public class GhostIK : SectoredMonoBehaviour
{
	[SerializeField]
	private OWIK _leftFootIK;

	[SerializeField]
	private OWIK _rightFootIK;

	[SerializeField]
	private bool _adjustHipsY;

	[SerializeField]
	private float _hipsLerp;

	[SerializeField]
	private float _maxFootVerticalDistance;

	[SerializeField]
	private float _footYOffset;

	[SerializeField]
	private Transform _leftToe;

	[SerializeField]
	private Transform _rightToe;

	private Animator _animator;

	private bool _wasLeftIK;

	private bool _wasRightIK;

	private int _leftFootLiftParamKey;

	private int _rightFootLiftParamKey;

	private float _baseLeftFootElev;

	private float _baseRightFootElev;

	private Vector3 _leftFootPlantPoint;

	private Vector3 _rightFootPlantPoint;

	private SurfaceType _lastLeftFootSurfaceType;

	private SurfaceType _lastRightFootSurfaceType;

	private bool _initialized;

	public OWEvent<SurfaceType> OnLeftFootHitGround = new OWEvent<SurfaceType>(1);

	public OWEvent<SurfaceType> OnRightFootHitGround = new OWEvent<SurfaceType>(1);

	public SurfaceType lastLeftFootSurfaceType => _lastLeftFootSurfaceType;

	public SurfaceType lastRightFoorSurfaceType => _lastRightFootSurfaceType;

	protected override void Awake()
	{
		base.Awake();
		_animator = GetComponent<Animator>();
		_leftFootLiftParamKey = Animator.StringToHash("LeftFootLift");
		_rightFootLiftParamKey = Animator.StringToHash("RightFootLift");
	}

	private void Start()
	{
		if (!_initialized)
		{
			Init();
		}
		base.enabled = false;
	}

	private void LateUpdate()
	{
		if (!((Locator.GetPlayerCamera().transform.position - base.transform.position).sqrMagnitude > 2500f))
		{
			SolveFoot(left: true, out var _);
			SolveFoot(left: false, out var _);
		}
	}

	private void Init()
	{
		_baseLeftFootElev = base.transform.InverseTransformPoint(_leftToe.position).y;
		_baseRightFootElev = base.transform.InverseTransformPoint(_rightToe.position).y;
		_initialized = true;
	}

	private void AdjustHipHeight(Vector3 finalLeftGoal, Vector3 finalRightGoal)
	{
		if (_adjustHipsY)
		{
			base.transform.localPosition = Vector3.zero;
			Transform boneTransform = _animator.GetBoneTransform(HumanBodyBones.Hips);
			Vector3 vector = base.transform.InverseTransformPoint(finalLeftGoal);
			Vector3 vector2 = base.transform.InverseTransformPoint(finalRightGoal);
			Vector3 vector3 = base.transform.InverseTransformPoint(boneTransform.position);
			Vector3 obj = ((vector.y < vector2.y) ? vector : vector2);
			Vector3 position = base.transform.position;
			float num = Mathf.Clamp(Mathf.Abs(obj.y - vector3.y) - _maxFootVerticalDistance, 0f, float.PositiveInfinity);
			position += base.transform.TransformVector(new Vector3(0f, 0f - num, 0f));
			if (Vector3.Distance(base.transform.position, position) <= 0.5f)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, position, _hipsLerp);
			}
		}
	}

	private void SolveFoot(bool left, out Vector3 finalGoal)
	{
		Vector3 vector = (finalGoal = (left ? _leftToe.position : _rightToe.position));
		bool flag = _animator.GetFloat(left ? _leftFootLiftParamKey : _rightFootLiftParamKey) <= 0f;
		bool flag2 = (left ? _wasLeftIK : _wasRightIK);
		float num = (left ? _baseLeftFootElev : _baseRightFootElev);
		float a = base.transform.InverseTransformPoint(vector).y - num;
		if (Physics.Raycast(vector + base.transform.up * 1.5f, -base.transform.up * 3.5f, out var hitInfo, 2f, OWLayerMask.physicalMask))
		{
			finalGoal = hitInfo.point;
			finalGoal += base.transform.TransformVector(new Vector3(0f, _footYOffset + Mathf.Max(a, 0f), 0f));
			if (Locator.GetSurfaceManager() != null)
			{
				SurfaceType hitSurfaceType = Locator.GetSurfaceManager().GetHitSurfaceType(hitInfo);
				if (hitSurfaceType != 0)
				{
					if (left)
					{
						_lastLeftFootSurfaceType = hitSurfaceType;
					}
					else
					{
						_lastRightFootSurfaceType = hitSurfaceType;
					}
				}
			}
			if (flag && !flag2)
			{
				if (left)
				{
					OnLeftFootHitGround.Invoke(_lastLeftFootSurfaceType);
					_leftFootPlantPoint = finalGoal;
				}
				else
				{
					OnRightFootHitGround.Invoke(_lastRightFootSurfaceType);
					_rightFootPlantPoint = finalGoal;
				}
			}
		}
		if (flag && !flag2)
		{
			finalGoal = (left ? _leftFootPlantPoint : _rightFootPlantPoint);
		}
		if (left)
		{
			_wasLeftIK = flag;
		}
		else
		{
			_wasRightIK = flag;
		}
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			if (!_initialized)
			{
				Init();
			}
			base.enabled = true;
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			base.enabled = false;
		}
	}
}
