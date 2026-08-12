using System.Collections.Generic;
using UnityEngine;

namespace Tessellation
{
	public static class PatchPool
	{
		private const int _kInitialPoolSize = 1024;

		private static List<Patch> _pool;

		private static void Init()
		{
			_pool = new List<Patch>(1024);
			for (int i = 0; i < 1024; i++)
			{
				_pool.Add(new Patch());
			}
		}

		public static Patch GetPatch(Patch.RenderMode renderMode = Patch.RenderMode.Spherical, float cullPadding = 0f)
		{
			if (_pool == null)
			{
				Init();
			}
			if (_pool.Count == 0)
			{
				Debug.LogWarning("Out of patches!  Allocating more...  (" + _pool.Capacity + "->" + (_pool.Capacity + 1024) + ")");
				_pool.Capacity += 1024;
				for (int i = 0; i < 1024; i++)
				{
					_pool.Add(new Patch());
				}
			}
			Patch patch = _pool[_pool.Count - 1];
			_pool.RemoveAt(_pool.Count - 1);
			patch.Init(renderMode, cullPadding);
			return patch;
		}

		public static void ReturnPatch(Patch p)
		{
			if (_pool == null)
			{
				Init();
			}
			_pool.Add(p);
		}

		public static int GetPoolSize()
		{
			return _pool.Capacity;
		}

		public static int GetPatchCount()
		{
			return _pool.Count;
		}
	}
}
