using UnityEngine;

public class NomaiRecorderEffects : SectoredMonoBehaviour
{
	[SerializeField]
	private OWRenderer[] _ringRenderers = new OWRenderer[0];

	[SerializeField]
	private int[] _materialIndexes = new int[0];

	[SerializeField]
	private float[] _scrollSpeeds = new float[0];

	[SerializeField]
	private OWAudioSource _audioSource;

	private int _propID_DetailMainTex_ST;

	private bool[] _hasDetailST;

	private Vector4[] _baseST;

	protected override void Awake()
	{
		base.Awake();
		_propID_DetailMainTex_ST = Shader.PropertyToID("_DetailMainTex_ST");
		_hasDetailST = new bool[_ringRenderers.Length];
		_baseST = new Vector4[_ringRenderers.Length];
		for (int i = 0; i < _ringRenderers.Length; i++)
		{
			_hasDetailST[i] = _ringRenderers[i].sharedMaterials[_materialIndexes[i]].HasProperty(_propID_DetailMainTex_ST);
			if (_hasDetailST[i])
			{
				_baseST[i] = _ringRenderers[i].sharedMaterials[_materialIndexes[i]].GetVector(_propID_DetailMainTex_ST);
			}
		}
		base.enabled = false;
	}

	private void Start()
	{
		_audioSource.SetLocalVolume(0f);
	}

	private void Update()
	{
		for (int i = 0; i < _ringRenderers.Length; i++)
		{
			if (_hasDetailST[i])
			{
				_ringRenderers[i].SetMaterialProperty(_propID_DetailMainTex_ST, new Vector4(_baseST[i].x, _baseST[i].y, _baseST[i].z + Time.time * _scrollSpeeds[i], _baseST[i].w));
			}
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag = false;
		if (_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			base.enabled = true;
			flag = _sector.ContainsAnyOccupants(DynamicOccupant.Player);
		}
		else
		{
			base.enabled = false;
		}
		if (flag)
		{
			_audioSource.FadeIn(0.5f);
		}
		else
		{
			_audioSource.FadeOut(0.5f);
		}
	}
}
