using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class TractorBeamParticleController : MonoBehaviour
{
	[SerializeField]
	private float _lifetimeScalar = 1f;

	[SerializeField]
	private float _sizeScalar = 1f;

	private ParticleSystemRenderer _renderer;

	private MaterialPropertyBlock _matPropBlock;

	private int _propID_CameraFarFade;

	private float _fadeTarget;

	public void OnValidate()
	{
		TractorBeamFluid componentInParent = GetComponentInParent<TractorBeamFluid>();
		if (!(componentInParent != null))
		{
			return;
		}
		ParticleSystem component = GetComponent<ParticleSystem>();
		component.Stop();
		ParticleSystem.MainModule main = component.main;
		float num = componentInParent.GetHeight() * _lifetimeScalar;
		if (main.startLifetime.constant != num)
		{
			main.startLifetime = num;
		}
		ParticleSystem.ShapeModule shape = component.shape;
		float num2 = componentInParent.GetRadius() * _sizeScalar;
		if (shape.enabled)
		{
			if (shape.radius != num2)
			{
				shape.radius = num2;
			}
		}
		else if (main.startSize.constant != num2)
		{
			main.startSize = num2;
		}
		Vector3 vector;
		Quaternion quaternion;
		if (componentInParent.GetVerticalSpeed() > 0f)
		{
			vector = base.transform.parent.TransformPoint(new Vector3(0f, componentInParent.GetHeight(), 0f));
			quaternion = Quaternion.identity;
		}
		else if (componentInParent.GetVerticalSpeed() == 0f)
		{
			vector = base.transform.parent.TransformPoint(new Vector3(0f, componentInParent.GetHeight() * 0.5f, 0f));
			quaternion = Quaternion.identity;
		}
		else
		{
			vector = base.transform.parent.TransformPoint(Vector3.zero);
			quaternion = Quaternion.AngleAxis(180f, Vector3.right);
		}
		if (Vector3.Distance(base.transform.position, vector) > 0.001f)
		{
			base.transform.position = vector;
		}
		if (Quaternion.Angle(base.transform.localRotation, quaternion) > 0.001f)
		{
			base.transform.localRotation = quaternion;
		}
		component.Play();
	}

	private void Awake()
	{
		_renderer = GetComponent<ParticleSystemRenderer>();
		_matPropBlock = new MaterialPropertyBlock();
		_propID_CameraFarFade = Shader.PropertyToID("_CameraFarFade");
		base.enabled = false;
	}

	public void OnPlayerEnterBeam()
	{
		_fadeTarget = 0.96f;
		base.enabled = true;
	}

	public void OnPlayerExitBeam()
	{
		_fadeTarget = 0f;
		base.enabled = true;
	}

	private void Update()
	{
		float @float = _matPropBlock.GetFloat(_propID_CameraFarFade);
		@float = Mathf.MoveTowards(@float, _fadeTarget, Time.deltaTime);
		_matPropBlock.SetFloat(_propID_CameraFarFade, @float);
		_renderer.SetPropertyBlock(_matPropBlock);
		if (@float == _fadeTarget)
		{
			if (_fadeTarget <= 0f)
			{
				_renderer.SetPropertyBlock(null);
			}
			base.enabled = false;
		}
	}
}
