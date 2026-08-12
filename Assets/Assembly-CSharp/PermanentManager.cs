using OWML.ModLoader;
using UnityEngine;

public class PermanentManager : MonoBehaviour
{
	private static PermanentManager s_theManager;

	private void Awake()
	{
		if (null != s_theManager)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		IPermanentManagerWorker[] components = GetComponents<IPermanentManagerWorker>();
		base.transform.SetParent(null);
		Object.DontDestroyOnLoad(base.gameObject);
		s_theManager = this;
		for (int i = 0; i < components.Length; i++)
		{
			components[i].InitializeOnAwake();
		}
		ModLoader.LoadMods();
	}

	public static PermanentManager Get()
	{
		return s_theManager;
	}
}
