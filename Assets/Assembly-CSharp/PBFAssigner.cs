using UnityEngine;

public class PBFAssigner : MonoBehaviour
{
	public Transform realFragmentsRoot;

	public bool assign;

	private void OnValidate()
	{
		if (assign)
		{
			DetachableFragment[] componentsInChildren = realFragmentsRoot.GetComponentsInChildren<DetachableFragment>();
			float realObjectDiameter = GetComponentInParent<ProxyPlanet>().realObjectDiameter;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				ProxyBrittleHollowFragment proxyBrittleHollowFragment = base.transform.GetChild(i).gameObject.AddComponent<ProxyBrittleHollowFragment>();
				proxyBrittleHollowFragment.SetRealBody(componentsInChildren[i].transform);
				proxyBrittleHollowFragment.CollectRenderers();
				proxyBrittleHollowFragment.SetRealDiameter(realObjectDiameter);
			}
			assign = false;
		}
	}
}
