using UnityEngine;

public class EyeProxyQuantumMoon : QuantumObject
{
	[SerializeField]
	private GameObject _moonStateRoot;

	private const int CHECK_DEPTH = 20;

	protected override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
	{
		if (TimeLoop.GetSecondsRemaining() > 0f && Random.value > 0.3f)
		{
			_moonStateRoot.SetActive(value: false);
			return true;
		}
		_moonStateRoot.SetActive(value: true);
		for (int i = 0; i < 20; i++)
		{
			base.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
			if (skipInstantVisibilityCheck || !CheckVisibilityInstantly())
			{
				return true;
			}
		}
		return true;
	}

	protected override void Update()
	{
		base.Update();
		base.transform.Rotate(Vector3.up, 1f * Time.deltaTime, Space.Self);
	}
}
