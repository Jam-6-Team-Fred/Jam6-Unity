using UnityEngine;

[RequireComponent(typeof(OWRenderer))]
public class ElectricityEffect : MonoBehaviour
{
	private OWRenderer _electricityRenderer;

	[SerializeField]
	private Vector2 _startOffset = new Vector2(-1f, -1f);

	[SerializeField]
	private Vector2 _endOffset = new Vector2(1f, 1f);

	[SerializeField]
	private float _duration = 1f;

	private int _propID_MainTex_ST;

	private Vector4 _baseTexST;

	private float _offsetTimer;

	private void Awake()
	{
		_electricityRenderer = GetComponent<OWRenderer>();
		_propID_MainTex_ST = Shader.PropertyToID("_MainTex_ST");
		_baseTexST = _electricityRenderer.sharedMaterial.GetVector(_propID_MainTex_ST);
		_electricityRenderer.SetActivation(active: false);
		base.enabled = false;
	}

	private void Update()
	{
		_offsetTimer += Time.deltaTime;
		float num = _offsetTimer / _duration;
		Vector2 vector = Vector2.Lerp(_startOffset, _endOffset, num);
		_electricityRenderer.SetMaterialProperty(_propID_MainTex_ST, new Vector4(_baseTexST.x, _baseTexST.y, vector.x, vector.y));
		if (num >= 1f)
		{
			_electricityRenderer.SetActivation(active: false);
			base.enabled = false;
		}
	}

	public void Play(bool forceRestart = false)
	{
		if (!base.enabled || forceRestart)
		{
			_offsetTimer = 0f;
			_electricityRenderer.SetActivation(active: true);
			base.enabled = true;
		}
	}
}
