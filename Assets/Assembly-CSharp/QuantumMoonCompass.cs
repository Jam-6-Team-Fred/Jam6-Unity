using UnityEngine;

public class QuantumMoonCompass : MonoBehaviour
{
	[SerializeField]
	private Transform _compassTransform;

	[SerializeField]
	private Transform[] _stateSymbols;

	[SerializeField]
	private bool _loop = true;

	private QuantumMoon _quantumMoon;

	private float _degrees;

	private void Awake()
	{
		if (_compassTransform == null)
		{
			_compassTransform = base.transform;
		}
	}

	private void Start()
	{
		AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.QuantumMoon);
		if (astroObject != null)
		{
			_quantumMoon = astroObject.GetComponent<QuantumMoon>();
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnPostCollapse(QuantumObject obj, bool collapsed)
	{
	}

	private void Update()
	{
		int stateIndex = _quantumMoon.GetStateIndex();
		if (stateIndex < 0 || stateIndex >= _stateSymbols.Length)
		{
			return;
		}
		float num = GetSymbolDegrees(stateIndex);
		if (!_loop && num < 0f)
		{
			num += 360f;
		}
		else if (_loop)
		{
			if (num - _degrees > 180f)
			{
				num -= 360f;
			}
			else if (num - _degrees < -180f)
			{
				num += 360f;
			}
		}
		_degrees = Mathf.MoveTowards(_degrees, num, Time.deltaTime * 90f);
		_compassTransform.localEulerAngles = new Vector3(0f, _degrees, 0f);
	}

	private float GetSymbolDegrees(int index)
	{
		Vector3 to = Vector3.ProjectOnPlane(_stateSymbols[index].transform.position - base.transform.position, base.transform.up);
		return OWMath.Angle(Vector3.ProjectOnPlane(_stateSymbols[0].transform.position - base.transform.position, base.transform.up), to, base.transform.up);
	}
}
