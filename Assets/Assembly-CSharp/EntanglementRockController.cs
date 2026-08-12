using UnityEngine;

public class EntanglementRockController : MonoBehaviour
{
	[SerializeField]
	private QuantumSocket _lakebedSocket;

	[SerializeField]
	private QuantumSocket[] _socketsToDeactivate;

	private SocketedQuantumObject _quantumObject;

	private void Awake()
	{
		_quantumObject = GetComponent<SocketedQuantumObject>();
		_quantumObject.OnPreCollapse += OnPreCollapse;
	}

	private void OnDestroy()
	{
		_quantumObject.OnPreCollapse -= OnPreCollapse;
		_quantumObject.OnPostCollapse -= OnPostCollapse;
	}

	private void OnPreCollapse(QuantumObject quantumObject)
	{
		if (_quantumObject.IsPlayerEntangled() && _quantumObject.GetCurrentSocket() == _lakebedSocket)
		{
			_quantumObject.OnPostCollapse += OnPostCollapse;
			for (int i = 0; i < _socketsToDeactivate.Length; i++)
			{
				_socketsToDeactivate[i].SetActive(active: false);
			}
		}
	}

	private void OnPostCollapse(QuantumObject quantumObject, bool changedState)
	{
		_quantumObject.OnPostCollapse -= OnPostCollapse;
		for (int i = 0; i < _socketsToDeactivate.Length; i++)
		{
			_socketsToDeactivate[i].SetActive(active: true);
		}
		if (changedState)
		{
			_quantumObject.OnPreCollapse -= OnPreCollapse;
		}
	}
}
