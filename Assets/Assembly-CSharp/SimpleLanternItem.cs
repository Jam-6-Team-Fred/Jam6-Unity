using UnityEngine;

public class SimpleLanternItem : OWItem, ILightSource
{
	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private OWAudioSource _oneShotAudio;

	[SerializeField]
	private CustomCollisionChecker _collisionChecker;

	[SerializeField]
	private bool _startsLit = true;

	[SerializeField]
	private TransformAnimator _animator;

	[SerializeField]
	private ElectricityEffect _extinguishElectricityEffect;

	private bool _lit = true;

	private float _origLightSourceShapeRadius;

	private float _animDuration = 0.6f;

	private float _animOffsetY = 0.2f;

	private RingWorldFlickerController _flickerController;

	private LightSourceVolume _lightSourceVol;

	private SphereShape _lightSourceShape;

	public OWEvent OnLanternExtinguished = new OWEvent(4);

	protected override void Awake()
	{
		_type = ItemType.Lantern;
		_flickerController = GetComponent<RingWorldFlickerController>();
		if (!_startsLit)
		{
			_lightController.FadeTo(0f, 0f);
			OnLanternExtinguished.Invoke();
			_lit = false;
		}
		if (_collisionChecker != null)
		{
			_collisionChecker.OnEnterCustomCollider += new OWEvent.OWCallback(OnEnterCustomCollider);
		}
		_lightSourceVol = GetComponentInChildren<LightSourceVolume>();
		base.Awake();
	}

	private void Start()
	{
		if (_lightSourceVol != null)
		{
			_lightSourceVol.SetVolumeActivation(_lit);
			_lightSourceVol.LinkLightSource(this);
			_lightSourceShape = (SphereShape)_lightSourceVol.GetShape();
			_origLightSourceShapeRadius = _lightSourceShape.radius;
		}
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		if (_collisionChecker != null)
		{
			_collisionChecker.OnEnterCustomCollider -= new OWEvent.OWCallback(OnEnterCustomCollider);
		}
		base.OnDestroy();
	}

	public bool IsLit()
	{
		return _lit;
	}

	public override string GetDisplayName()
	{
		return UITextLibrary.GetString(UITextType.ItemSimpleLanternPrompt);
	}

	public override void SocketItem(Transform socketTransform, Sector sector)
	{
		base.SocketItem(socketTransform, sector);
		if (_lightSourceVol != null)
		{
			_lightSourceShape.radius = _origLightSourceShapeRadius;
		}
	}

	public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
	{
		base.DropItem(position, normal, parent, sector, customDropTarget);
		if (_lightSourceVol != null)
		{
			_lightSourceShape.radius = _origLightSourceShapeRadius;
		}
	}

	public override void PickUpItem(Transform holdTranform)
	{
		if (_lit)
		{
			Locator.GetFlashlight().TurnOff();
		}
		if (_lightSourceVol != null)
		{
			_lightSourceShape.radius = _origLightSourceShapeRadius / holdTranform.localScale.x;
		}
		base.PickUpItem(holdTranform);
	}

	public override void PlaySocketAnimation()
	{
		if (!(_animator == null))
		{
			_animator.transform.localPosition = Vector3.up * _animOffsetY;
			_animator.TranslateToOriginalLocalPosition(_animDuration);
		}
	}

	public override void PlayUnsocketAnimation()
	{
		if (!(_animator == null))
		{
			_animator.TranslateToLocalPosition(Vector3.up * _animOffsetY, _animDuration);
		}
	}

	public override void OnCompleteUnsocket()
	{
		if (!(_animator == null))
		{
			_animator.ResetToOriginalPositionRotation();
		}
	}

	protected override void UpdateCollisionLOD()
	{
		base.UpdateCollisionLOD();
		if (_lightSourceShape != null)
		{
			_lightSourceShape.enabled = _sector == null || _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		}
	}

	private void OnEnterCustomCollider()
	{
		if (!_lit)
		{
			return;
		}
		if (_sector == null || Locator.GetPlayerSectorDetector().IsWithinSector(_sector.GetName()))
		{
			_flickerController.Flicker(0f, 1f, 0.1f, 0.04f, 0.08f);
			_oneShotAudio.PlayOneShot(AudioType.Lantern_ShortOut);
			if (_extinguishElectricityEffect != null)
			{
				_extinguishElectricityEffect.Play();
			}
		}
		else
		{
			_lightController.FadeTo(0f, 0f);
		}
		_lit = false;
		if (_lightSourceVol != null)
		{
			_lightSourceVol.SetVolumeActivation(_lit);
			_lightSourceShape.SetActivation(_lit);
		}
		OnLanternExtinguished.Invoke();
	}

	public override bool IsAnimationPlaying()
	{
		if (_animator == null)
		{
			return false;
		}
		return _animator.IsAnimating();
	}

	public LightSourceType GetLightSourceType()
	{
		return LightSourceType.SIMPLE_LANTERN;
	}

	public bool CheckIlluminationAtPoint(Vector3 point, float buffer = 0f, float maxDistance = float.PositiveInfinity)
	{
		return _lightController.CheckIlluminationAtPoint(point, buffer, maxDistance);
	}

	public OWLight2[] GetLights()
	{
		return _lightController.GetLights();
	}
}
