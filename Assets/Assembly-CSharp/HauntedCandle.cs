using System.Collections.Generic;
using UnityEngine;

public class HauntedCandle : MonoBehaviour
{
	[SerializeField]
	private bool _startLit = true;

	[Space]
	[SerializeField]
	private GameObject _objectsVisibleInLight;

	[SerializeField]
	private GameObject _lightsRoot;

	[SerializeField]
	private List<NomaiLamp> _lights;

	[Space]
	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private OWLightController _candleLightController;

	[SerializeField]
	private ParticleSystem _candleParticles;

	[Space]
	[SerializeField]
	private GameObject _doorsRoot;

	[SerializeField]
	private List<LightCodeDoor> _doorsToClose;

	[Space]
	[SerializeField]
	public HauntedRoom p_room;

	public OWEvent OnOldDreamCandleLit = new OWEvent(16);

	private float _lastFadeTime;

	private bool _lit;

	private void OnValidate()
	{
		if (_lightsRoot != null)
		{
			_lights.Clear();
			NomaiLamp[] componentsInChildren = _lightsRoot.GetComponentsInChildren<NomaiLamp>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_lights.Add(componentsInChildren[i]);
			}
			_lightsRoot = null;
		}
		if (_doorsRoot != null)
		{
			_doorsToClose.Clear();
			LightCodeDoor[] componentsInChildren2 = _doorsRoot.GetComponentsInChildren<LightCodeDoor>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				_doorsToClose.Add(componentsInChildren2[j]);
			}
			_doorsRoot = null;
		}
	}

	public bool IsLit()
	{
		return _lit;
	}

	private void Awake()
	{
		_interactReceiver.OnPressInteract += OnPressInteract;
	}

	private void Start()
	{
		base.enabled = false;
		_lit = _startLit;
		if (_objectsVisibleInLight != null)
		{
			_objectsVisibleInLight.SetActive(_lit);
		}
		_candleLightController.FadeTo(_lit ? 1f : 0f, 0f);
		if (_lit)
		{
			_candleParticles.Play();
		}
		UpdatePrompt();
	}

	private void OnDestroy()
	{
		_interactReceiver.OnPressInteract -= OnPressInteract;
	}

	private void UpdatePrompt()
	{
		_interactReceiver.SetPromptText(_lit ? UITextType.RoastingExtinguishPrompt : UITextType.LightCampfirePrompt);
	}

	private void OnPressInteract()
	{
		Lit(!_lit);
		UpdatePrompt();
	}

	public void Lit(bool lit)
	{
		base.enabled = true;
		_lastFadeTime = Time.time;
		_lit = lit;
		if (_lit)
		{
			_candleLightController.FadeTo(1f, 1f);
			_candleParticles.Play();
			_interactReceiver.EnableInteraction();
			return;
		}
		for (int i = 0; i < _doorsToClose.Count; i++)
		{
			_doorsToClose[i].CloseDoor(locked: false);
		}
		_candleLightController.FadeTo(0f, 1f);
		_candleParticles.Stop();
		_interactReceiver.DisableInteraction();
		Locator.GetPlayerAudioController().PlayMarshmallowBlowOut();
	}

	private void Update()
	{
		if (Time.time > _lastFadeTime + 1f)
		{
			_lastFadeTime = Time.time;
			if (_objectsVisibleInLight != null)
			{
				_objectsVisibleInLight.SetActive(_lit);
			}
			OnOldDreamCandleLit.Invoke();
			for (int i = 0; i < _lights.Count; i++)
			{
				_lights[i].FadeTo(_lit ? 1f : 0f, 0.2f);
			}
			base.enabled = false;
		}
	}
}
