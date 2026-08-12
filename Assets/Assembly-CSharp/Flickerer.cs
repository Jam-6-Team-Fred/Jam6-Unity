using UnityEngine;
using UnityEngine.UI;

public class Flickerer : MonoBehaviour
{
	[SerializeField]
	private Color _flickerColor;

	[SerializeField]
	private Text[] _listTextItems;

	[SerializeField]
	private Image[] _listImageItems;

	[SerializeField]
	private MeshRenderer[] _listRenderers;

	[SerializeField]
	private float _flickerTime;

	[SerializeField]
	private bool _flickerOnInstantDamage;

	private PlayerResources _playerResources;

	private Color[] _originalTextColors;

	private float _timeSinceFlickerStart;

	private bool _isFlickering;

	private void Start()
	{
		_originalTextColors = new Color[_listTextItems.Length];
		for (int i = 0; i < _listTextItems.Length; i++)
		{
			_originalTextColors[i] = _listTextItems[i].color;
		}
		if (_flickerOnInstantDamage)
		{
			_playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
			_playerResources.OnInstantDamage += OnInstantDamage;
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if ((bool)_playerResources)
		{
			_playerResources.OnInstantDamage -= OnInstantDamage;
		}
	}

	private void LateUpdate()
	{
		if (_isFlickering)
		{
			if (_timeSinceFlickerStart > _flickerTime)
			{
				EndFlicker();
				return;
			}
			for (int i = 0; i < _listImageItems.Length; i++)
			{
				FlickerElement(_listImageItems[i]);
			}
			for (int j = 0; j < _listTextItems.Length; j++)
			{
				FlickerElement(_listTextItems[j]);
			}
			for (int k = 0; k < _listRenderers.Length; k++)
			{
				FlickerElement(_listRenderers[k]);
			}
			_timeSinceFlickerStart += Time.deltaTime;
		}
		else
		{
			base.enabled = false;
		}
	}

	public void StartFlicker()
	{
		_isFlickering = true;
		_timeSinceFlickerStart = 0f;
		base.enabled = true;
	}

	public void EndFlicker()
	{
		_isFlickering = false;
		for (int i = 0; i < _listImageItems.Length; i++)
		{
			_listImageItems[i].enabled = true;
		}
		for (int j = 0; j < _listTextItems.Length; j++)
		{
			_listTextItems[j].enabled = true;
			_listTextItems[j].color = _originalTextColors[j];
		}
		for (int k = 0; k < _listRenderers.Length; k++)
		{
			_listRenderers[k].enabled = true;
		}
		base.enabled = false;
	}

	private void FlickerElement(Image image)
	{
		bool flag = Random.value < 0.5f;
		if (image.enabled)
		{
			if (flag)
			{
				image.enabled = false;
			}
		}
		else if (flag)
		{
			image.enabled = true;
		}
	}

	private void FlickerElement(Text text)
	{
		bool flag = Random.value < 0.5f;
		if (text.enabled)
		{
			if (flag)
			{
				text.enabled = false;
			}
		}
		else if (flag)
		{
			text.enabled = true;
			text.color = _flickerColor;
		}
	}

	private void FlickerElement(MeshRenderer mr)
	{
		bool flag = Random.value < 0.5f;
		if (mr.enabled)
		{
			if (flag)
			{
				mr.enabled = false;
			}
		}
		else if (flag)
		{
			mr.enabled = true;
		}
	}

	private void OnInstantDamage(float instantDamage, InstantDamageType damageType)
	{
		StartFlicker();
	}
}
