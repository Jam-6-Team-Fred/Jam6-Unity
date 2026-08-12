using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ShockLayerController : MonoBehaviour
{
	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_Color;

	private static int s_propID_WorldToLocalShockMatrix;

	private static int s_propID_Dir;

	private static int s_propID_Length;

	private static int s_propID_Flare;

	private static int s_propID_TrailFade;

	private static int s_propID_GradientLerp;

	private static int s_propID_MainTex_ST;

	private MeshRenderer _meshRenderer;

	private OWRigidbody _owRigidbody;

	[SerializeField]
	private ShockLayerRuleset _rulesetOverride;

	[SerializeField]
	private RulesetDetector _rulesetDetector;

	[SerializeField]
	private FluidDetector _fluidDetector;

	private ShockLayerRuleset _ruleset;

	private void Awake()
	{
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_Color = Shader.PropertyToID("_Color");
			s_propID_WorldToLocalShockMatrix = Shader.PropertyToID("_WorldToShockLocalMatrix");
			s_propID_Dir = Shader.PropertyToID("_Dir");
			s_propID_Length = Shader.PropertyToID("_Length");
			s_propID_Flare = Shader.PropertyToID("_Flare");
			s_propID_TrailFade = Shader.PropertyToID("_TrailFade");
			s_propID_GradientLerp = Shader.PropertyToID("_GradientLerp");
			s_propID_MainTex_ST = Shader.PropertyToID("_MainTex_ST");
		}
		_meshRenderer = GetComponent<MeshRenderer>();
		_owRigidbody = this.GetAttachedOWRigidbody();
		if (_rulesetOverride != null)
		{
			_ruleset = _rulesetOverride;
			return;
		}
		if (_rulesetDetector != null)
		{
			_rulesetDetector.OnChangeRuleset += OnChangeRuleset;
		}
		_meshRenderer.enabled = false;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_rulesetDetector != null)
		{
			_rulesetDetector.OnChangeRuleset -= OnChangeRuleset;
		}
	}

	private void OnChangeRuleset()
	{
		if (_rulesetDetector.GetUseShockLayer())
		{
			_ruleset = _rulesetDetector.GetCurrentShockLayerRuleset();
			base.enabled = true;
		}
		else
		{
			_ruleset = null;
			_meshRenderer.enabled = false;
			base.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if (_ruleset == null)
		{
			_meshRenderer.enabled = false;
			base.enabled = false;
			return;
		}
		Vector3 vector = _ruleset.GetRadialCenter().position - _owRigidbody.GetPosition();
		float magnitude = vector.magnitude;
		float num = 1f - Mathf.InverseLerp(_ruleset.GetInnerRadius(), _ruleset.GetOuterRadius(), magnitude);
		if (_ruleset.GetShockLayerType() == ShockLayerRuleset.ShockType.Atmospheric)
		{
			Vector3 relativeFluidVelocity = _fluidDetector.GetRelativeFluidVelocity();
			float magnitude2 = relativeFluidVelocity.magnitude;
			float num2 = Mathf.InverseLerp(_ruleset.GetMinShockSpeed(), _ruleset.GetMaxShockSpeed(), magnitude2);
			num2 *= num;
			if (num2 <= 0f)
			{
				if (_meshRenderer.enabled)
				{
					_meshRenderer.enabled = false;
				}
				return;
			}
			if (!_meshRenderer.enabled)
			{
				_meshRenderer.enabled = true;
			}
			Vector3 vector2 = ((!(magnitude2 > 0.001f)) ? base.transform.forward : (relativeFluidVelocity / magnitude2));
			Matrix4x4 matrix4x = Matrix4x4.TRS(q: Quaternion.LookRotation(vector2, (!(Mathf.Abs(Vector3.Dot(vector2, base.transform.up)) < 0.999f)) ? base.transform.forward : base.transform.up), pos: base.transform.position, s: Vector3.one);
			Color color = _ruleset.GetColor();
			color.a *= num2;
			Vector4 vector3 = _meshRenderer.sharedMaterial.GetVector(s_propID_MainTex_ST);
			vector3.w = 0f - Time.timeSinceLevelLoad;
			s_matPropBlock.SetColor(s_propID_Color, color);
			s_matPropBlock.SetMatrix(s_propID_WorldToLocalShockMatrix, matrix4x.inverse);
			s_matPropBlock.SetVector(s_propID_Dir, vector2);
			s_matPropBlock.SetFloat(s_propID_Length, magnitude2);
			s_matPropBlock.SetFloat(s_propID_Flare, magnitude2 * 0.5f);
			s_matPropBlock.SetFloat(s_propID_TrailFade, num2);
			s_matPropBlock.SetFloat(s_propID_GradientLerp, num2);
			s_matPropBlock.SetVector(s_propID_MainTex_ST, vector3);
			_meshRenderer.SetPropertyBlock(s_matPropBlock);
		}
		else
		{
			if (_ruleset.GetShockLayerType() != ShockLayerRuleset.ShockType.Radial)
			{
				return;
			}
			if (num <= 0f)
			{
				if (_meshRenderer.enabled)
				{
					_meshRenderer.enabled = false;
				}
				return;
			}
			if (!_meshRenderer.enabled)
			{
				_meshRenderer.enabled = true;
			}
			Vector3 vector4 = -vector / magnitude;
			Matrix4x4 matrix4x2 = Matrix4x4.TRS(q: Quaternion.LookRotation(vector4, (!(Mathf.Abs(Vector3.Dot(vector4, base.transform.up)) < 0.999f)) ? base.transform.forward : base.transform.up), pos: base.transform.position, s: Vector3.one);
			Color color2 = _ruleset.GetColor();
			color2.a *= num;
			Vector4 vector5 = _meshRenderer.sharedMaterial.GetVector(s_propID_MainTex_ST);
			vector5.w = 0f - Time.timeSinceLevelLoad;
			s_matPropBlock.SetColor(s_propID_Color, color2);
			s_matPropBlock.SetMatrix(s_propID_WorldToLocalShockMatrix, matrix4x2.inverse);
			s_matPropBlock.SetVector(s_propID_Dir, vector4);
			s_matPropBlock.SetFloat(s_propID_Length, _ruleset.GetTrailLength() * num);
			s_matPropBlock.SetFloat(s_propID_Flare, _ruleset.GetTrailFlare() * num);
			s_matPropBlock.SetFloat(s_propID_TrailFade, 0f);
			s_matPropBlock.SetFloat(s_propID_GradientLerp, num);
			s_matPropBlock.SetVector(s_propID_MainTex_ST, vector5);
			_meshRenderer.SetPropertyBlock(s_matPropBlock);
		}
	}
}
