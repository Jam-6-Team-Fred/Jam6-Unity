using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SlideCollectionContainer))]
public class SlideReelItem : OWItem
{
	private const int FIRSTSLIDE_RT_SIZE = 128;

	[SerializeField]
	private TransformAnimator _animator;

	[SerializeField]
	private GameObject _destroyedReelPrefab;

	private Vector3 _animLocalDir = Vector3.up;

	private float _animDuration = 0.6f;

	private float _animOffset = 0.3f;

	private RenderTexture _firstSlideStandIn;

	private Material _reelMeshSlidesMaterial;

	private Material _standInBlitMaterial;

	private SlideCollectionContainer _slideCollectionContainer;

	private Dictionary<int, float> _sectionRotations = new Dictionary<int, float>(0);

	private bool _rotationsAssembled;

	public RenderTexture firstSlideStandIn => _firstSlideStandIn;

	public int slideIndex
	{
		get
		{
			return _slideCollectionContainer.slideIndex;
		}
		set
		{
			_slideCollectionContainer.slideIndex = value;
		}
	}

	public SlideCollectionContainer slidesContainer => _slideCollectionContainer;

	protected override void Awake()
	{
		_type = ItemType.SlideReel;
		base.Awake();
		_slideCollectionContainer = base.gameObject.GetRequiredComponent<SlideCollectionContainer>();
	}

	private void Start()
	{
		_slideCollectionContainer.Initialize();
		AssembleRotationTable();
		base.enabled = false;
	}

	public void Burn()
	{
		if (_destroyedReelPrefab != null)
		{
			Achievements.Earn(Achievements.Type.CELCIUS);
			Object.Instantiate(_destroyedReelPrefab, base.transform.position, base.transform.rotation, base.transform.parent);
			Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		if (_slideCollectionContainer.streamingTexturesAvailable && _slideCollectionContainer.IsSlideStreamingTextureAvailable(slideIndex) && _standInBlitMaterial != null && _firstSlideStandIn != null)
		{
			_standInBlitMaterial.SetFloat("_BlurLevel", Mathf.Sin(6f * Time.time) / 5f + 0.75f);
			Graphics.Blit(_reelMeshSlidesMaterial.mainTexture, _firstSlideStandIn, _standInBlitMaterial);
		}
	}

	private void OnEnable()
	{
		_slideCollectionContainer.enabled = true;
	}

	private void OnDisable()
	{
		_slideCollectionContainer.enabled = false;
	}

	private void CreateFirstSlideStandIn()
	{
		if (_animator == null)
		{
			return;
		}
		MeshRenderer[] componentsInChildren = _animator.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			foreach (Material material in materials)
			{
				if (material.name.Contains("Slideshow"))
				{
					_reelMeshSlidesMaterial = material;
					break;
				}
			}
			if (_reelMeshSlidesMaterial != null)
			{
				break;
			}
		}
		_firstSlideStandIn = new RenderTexture(128, 128, 24);
		_standInBlitMaterial = new Material(Shader.Find("Hidden/SlideStandIn"));
		Graphics.Blit(_reelMeshSlidesMaterial.mainTexture, _firstSlideStandIn, _standInBlitMaterial);
	}

	private void AssembleRotationTable()
	{
		if (_rotationsAssembled)
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < _slideCollectionContainer.slideCount; i++)
		{
			Slide slideAt = _slideCollectionContainer.GetSlideAt(i);
			if (slideAt.HasModule(typeof(SlideRotationModule)))
			{
				_sectionRotations.Add(slideAt.GetStreamingIndex(), num);
				num += -45f;
			}
		}
		_rotationsAssembled = true;
	}

	public void RotateToSection(int streamingIndex)
	{
		if (_sectionRotations.ContainsKey(streamingIndex))
		{
			float angle = _sectionRotations[streamingIndex];
			RotateToAngle(angle);
		}
	}

	public void RotateToPrevSection(int streamingIndex)
	{
		int num = -1;
		int num2 = -1;
		foreach (KeyValuePair<int, float> sectionRotation in _sectionRotations)
		{
			if (sectionRotation.Key < streamingIndex && sectionRotation.Key > num)
			{
				num = sectionRotation.Key;
			}
			if (sectionRotation.Key > num2)
			{
				num2 = sectionRotation.Key;
			}
		}
		num = ((num >= 0) ? num : num2);
		float angle = _sectionRotations[num];
		RotateToAngle(angle);
	}

	public void Rotate(float angle, float duration = 0.2f)
	{
		_animator.RotateAroundLocalAxis(angle, Vector3.up, duration);
	}

	public void RotateToAngle(float angle)
	{
		_animator.RotateToLocalEulerAngles(new Vector3(0f, angle, 0f), 0.2f);
	}

	public void SetSocketLocalDir(Vector3 localDir)
	{
		_animLocalDir = localDir.normalized;
	}

	public override string GetDisplayName()
	{
		return UITextLibrary.GetString(UITextType.ItemSlideReelPrompt);
	}

	public override void PlaySocketAnimation()
	{
		_animator.transform.localPosition = _animLocalDir * _animOffset;
		_animator.TranslateToOriginalLocalPosition(_animDuration);
	}

	public override void PlayUnsocketAnimation()
	{
		_animator.TranslateToLocalPosition(_animLocalDir * _animOffset, _animDuration);
	}

	public override void OnCompleteUnsocket()
	{
		_animator.ResetToOriginalPositionRotation();
	}

	public void Removed()
	{
		if (_slideCollectionContainer.streamingTexturesAvailable)
		{
			_slideCollectionContainer.UnloadStreamingTextures();
		}
		base.enabled = false;
		_slideCollectionContainer.enabled = false;
		_slideCollectionContainer.SetChangeSlidesAllowed(allowed: true);
	}

	public override bool IsAnimationPlaying()
	{
		return _animator.IsAnimating();
	}
}
