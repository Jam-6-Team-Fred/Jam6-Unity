using UnityEngine;

public class CullDistanceTracker : MonoBehaviour
{
	public delegate void ChangeLevelHandler(int level);

	[SerializeField]
	private float[] _levelDistances = new float[0];

	private int _currentLevel;

	private Transform _transform;

	public event ChangeLevelHandler OnChangeLevel;

	private void Awake()
	{
		float num = 2f;
		InvokeRepeating("CheckCameraDistance", 0f, 1f / num);
		_transform = base.transform;
	}

	private void CheckCameraDistance()
	{
		float num = Vector3.Distance(Locator.GetActiveCamera().transform.position, _transform.position);
		int currentLevel = _currentLevel;
		if (_levelDistances.Length >= _currentLevel + 1 && num > _levelDistances[_currentLevel])
		{
			_currentLevel++;
		}
		else if (_currentLevel - 1 >= 0 && num <= _levelDistances[_currentLevel - 1])
		{
			_currentLevel--;
		}
		if (currentLevel != _currentLevel && this.OnChangeLevel != null)
		{
			this.OnChangeLevel(_currentLevel);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			for (int i = 0; i < _levelDistances.Length; i++)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(base.transform.position, _levelDistances[i]);
			}
		}
	}
}
