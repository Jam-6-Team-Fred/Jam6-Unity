using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
	private const string EMPTY_KEYS = "The keys for this dictionary are missing.";

	public SerializableDictionary()
	{
	}

	public SerializableDictionary(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public SerializableDictionary(IEqualityComparer<TKey> comparer)
		: base(comparer)
	{
	}

	public override void OnDeserialization(object sender)
	{
		try
		{
			base.OnDeserialization(sender);
		}
		catch (SerializationException ex)
		{
			if (ex.Message == "The keys for this dictionary are missing.")
			{
				Debug.LogError("[" + ex.Message + "]");
				return;
			}
			throw;
		}
	}
}
