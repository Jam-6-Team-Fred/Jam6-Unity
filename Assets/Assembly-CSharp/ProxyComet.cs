using System;
using UnityEngine;

public class ProxyComet : ProxyBody
{
	[SerializeField]
	private MeshRenderer[] _renderers;

	private TempCometCollisionFix _cometCollision;

	protected override void Awake()
	{
		base.Awake();
		GlobalMessenger.AddListener("CometDestroyed", OnCometDestroyed);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger.RemoveListener("CometDestroyed", OnCometDestroyed);
		if (_cometCollision != null)
		{
			_cometCollision.onCometDestroyed -= new OWEvent.OWCallback(OnCometDestroyed);
		}
	}

	public override void ToggleRendering(bool on)
	{
		base.ToggleRendering(on);
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].enabled = on;
		}
	}

	protected override void Initialize()
	{
		try
		{
			base.Initialize();
			AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.Comet);
			_realObjectTransform = astroObject.transform;
			_cometCollision = _realObjectTransform.gameObject.GetComponent<TempCometCollisionFix>();
			if (_cometCollision != null)
			{
				_cometCollision.onCometDestroyed += new OWEvent.OWCallback(OnCometDestroyed);
			}
		}
		catch (NullReferenceException e)
		{
			PrintInitializeFailMessage(e);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnCometDestroyed()
	{
		ToggleRendering(on: false);
		base.gameObject.SetActive(value: false);
	}
}
