using System;
using System.Text;
using UnityEngine;

public class SlideShipLogEntryModule : SlideFunctionModule
{
	[SerializeField]
	public string _entryKey;

	public override int DataSize()
	{
		return _entryKey.Length * 2 + 4;
	}

	public SlideShipLogEntryModule()
	{
		_entryKey = "";
	}

	public SlideShipLogEntryModule(byte[] data, int offset)
	{
		int count = BitConverter.ToInt32(data, offset);
		_entryKey = Encoding.UTF8.GetString(data, offset + 4, count);
	}

	public override void EnterSlide(Slide slide, bool forward)
	{
	}

	public override void Update(Slide slide)
	{
	}

	public override void ExitSlide(Slide slide, bool forward)
	{
	}

	public override Color EditorLabelColor()
	{
		return new Color(0.8f, 0.4f, 0f);
	}

	public override byte[] ToBytes()
	{
		int length = _entryKey.Length;
		byte[] array = new byte[4 + length];
		Buffer.BlockCopy(BitConverter.GetBytes(length), 0, array, 0, 4);
		Buffer.BlockCopy(Encoding.UTF8.GetBytes(_entryKey), 0, array, 4, length);
		return array;
	}
}
