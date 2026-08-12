using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class UpdateManagerBase : MonoBehaviour
{
	public class MonoBehaviourGroup<T> where T : MonoBehaviour
	{
		private int _count;

		private T[] _array;

		private bool[] _pendingAddition;

		private bool[] _pendingRemoval;

		public int Count => _count;

		public T this[int index] => _array[index];

		public bool IsPendingAdditionOrRemoval(int index)
		{
			if (!_pendingAddition[index])
			{
				return _pendingRemoval[index];
			}
			return true;
		}

		public MonoBehaviourGroup()
		{
			_count = 0;
			_array = new T[128];
			_pendingAddition = new bool[128];
			_pendingRemoval = new bool[128];
		}

		public MonoBehaviourGroup(int maxCount)
		{
			_count = 0;
			_array = new T[maxCount];
			_pendingAddition = new bool[maxCount];
			_pendingRemoval = new bool[maxCount];
		}

		public void ProcessAdditionsAndRemovals()
		{
			for (int num = _count - 1; num >= 0; num--)
			{
				if (_pendingRemoval[num])
				{
					_array[num] = _array[_count - 1];
					_array[_count - 1] = null;
					_count--;
					_pendingRemoval[num] = false;
				}
				_pendingAddition[num] = false;
			}
		}

		public void RemoveDestroyedElements()
		{
			for (int num = _count - 1; num >= 0; num--)
			{
				if ((Object)_array[num] == (Object)null)
				{
					int num2 = _count - 1;
					_array[num] = _array[num2];
					_pendingAddition[num] = _pendingAddition[num2];
					_pendingRemoval[num] = _pendingRemoval[num2];
					_array[num2] = null;
					_pendingAddition[num2] = false;
					_pendingRemoval[num2] = false;
					_count--;
				}
			}
		}

		public void Add(T element)
		{
			if (_count >= _array.Length)
			{
				Debug.LogError("No more space in MonoBehaviourGroup of type " + element.GetType());
				return;
			}
			_array[_count] = element;
			_pendingAddition[_count] = true;
			_pendingRemoval[_count] = false;
			_count++;
		}

		public void RemoveAt(int index)
		{
			_pendingRemoval[index] = true;
		}

		public void Remove(T element)
		{
			for (int i = 0; i < _count; i++)
			{
				if ((Object)_array[i] == (Object)element)
				{
					RemoveAt(i);
					break;
				}
			}
		}

		public int IndexOf(T element)
		{
			for (int i = 0; i < _count; i++)
			{
				if ((Object)_array[i] == (Object)element)
				{
					return i;
				}
			}
			return -1;
		}
	}

	protected const int kDefaultMaxMonoBehaviours = 128;

	protected const int kMaxNumRigidbodies = 1024;

	protected static GameObject s_managerGameObject;

	protected static void InstantiateManager<T>() where T : UpdateManagerBase
	{
		if (s_managerGameObject == null)
		{
			s_managerGameObject = new GameObject("UpdateManagers");
			Object.DontDestroyOnLoad(s_managerGameObject);
			s_managerGameObject.hideFlags = HideFlags.NotEditable;
		}
		if ((Object)s_managerGameObject.GetComponent<T>() == (Object)null)
		{
			T val = s_managerGameObject.AddComponent<T>();
			SceneManager.sceneUnloaded += val.OnSceneUnloaded;
			if (val is PreUpdateManager.IPreFixedUpdateListener)
			{
				PreUpdateManager.Register(val as PreUpdateManager.IPreFixedUpdateListener);
			}
			if (val is PreUpdateManager.IPreUpdateListener)
			{
				PreUpdateManager.Register(val as PreUpdateManager.IPreUpdateListener);
			}
			if (val is PreUpdateManager.IPreLateUpdateListener)
			{
				PreUpdateManager.Register(val as PreUpdateManager.IPreLateUpdateListener);
			}
		}
	}

	protected virtual void OnSceneUnloaded(Scene scene)
	{
	}
}
