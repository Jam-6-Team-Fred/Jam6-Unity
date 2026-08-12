using UnityEngine;

public class FragmentProxy : MonoBehaviour
{
	private FragmentIntegrity _fragmentIntegrity;

	private Vector3 _localCenter;

	public Vector3 worldCenter => base.transform.TransformPoint(_localCenter);

	private void Awake()
	{
		_fragmentIntegrity = GetComponent<FragmentIntegrity>();
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		bool flag = false;
		Vector3 vector = base.transform.position;
		Vector3 vector2 = base.transform.position;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!componentsInChildren[i].isTrigger)
			{
				if (!flag)
				{
					vector = componentsInChildren[i].bounds.min;
					vector2 = componentsInChildren[i].bounds.max;
					flag = true;
				}
				else
				{
					vector = Vector3.Min(componentsInChildren[i].bounds.min, vector);
					vector2 = Vector3.Max(componentsInChildren[i].bounds.max, vector2);
				}
			}
		}
		Vector3 position = (vector + vector2) * 0.5f;
		_localCenter = base.transform.InverseTransformPoint(position);
	}

	public bool IsIntact()
	{
		if (_fragmentIntegrity != null)
		{
			return _fragmentIntegrity.GetIntegrity() > 0f;
		}
		return true;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawSphere(_localCenter, 10f);
	}
}
