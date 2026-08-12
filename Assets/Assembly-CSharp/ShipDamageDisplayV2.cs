using UnityEngine;

public class ShipDamageDisplayV2 : MonoBehaviour
{
	private MeshRenderer _meshRenderer;

	private int _propID_RegionMask;

	private bool _shipDestroyed;

	[SerializeField]
	private ShipHull[] _shipHulls = new ShipHull[8];

	[SerializeField]
	private ShipComponent[] _shipComponents = new ShipComponent[16];

	private void Awake()
	{
		_meshRenderer = GetComponent<MeshRenderer>();
		_propID_RegionMask = Shader.PropertyToID("_RegionMask");
		_shipDestroyed = false;
		for (int i = 0; i < 8; i++)
		{
			if (_shipHulls[i] != null)
			{
				_shipHulls[i].OnDamaged += OnHullUpdate;
				_shipHulls[i].OnRepaired += OnHullUpdate;
			}
		}
		for (int j = 0; j < 16; j++)
		{
			if (_shipComponents[j] != null)
			{
				_shipComponents[j].OnDamaged += OnComponentUpdate;
				_shipComponents[j].OnRepaired += OnComponentUpdate;
			}
		}
		GlobalMessenger.AddListener("ShipSystemFailure", OnShipSystemFailure);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < 8; i++)
		{
			if (_shipHulls[i] != null)
			{
				_shipHulls[i].OnDamaged -= OnHullUpdate;
				_shipHulls[i].OnRepaired -= OnHullUpdate;
			}
		}
		for (int j = 0; j < 16; j++)
		{
			if (_shipComponents[j] != null)
			{
				_shipComponents[j].OnDamaged -= OnComponentUpdate;
				_shipComponents[j].OnRepaired -= OnComponentUpdate;
			}
		}
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipSystemFailure);
	}

	private void OnHullUpdate(ShipHull shipHull)
	{
		UpdateDisplay();
	}

	private void OnComponentUpdate(ShipComponent shipComponent)
	{
		UpdateDisplay();
	}

	private void OnShipSystemFailure()
	{
		_shipDestroyed = true;
		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		int num = 0;
		if (!_shipDestroyed)
		{
			for (int i = 0; i < 8; i++)
			{
				if (_shipHulls[i] != null && _shipHulls[i].isDamaged)
				{
					num |= 1 << i;
				}
			}
			for (int j = 0; j < 16; j++)
			{
				if (_shipComponents[j] != null && _shipComponents[j].isDamaged)
				{
					num |= 1 << j + 8;
				}
			}
		}
		_meshRenderer.material.SetInt(_propID_RegionMask, num);
	}
}
