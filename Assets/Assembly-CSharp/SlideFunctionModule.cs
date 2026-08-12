using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SlideFunctionModule
{
	public int ModuleIndex()
	{
		return GetByteCodeFromType(GetType());
	}

	public abstract int DataSize();

	public virtual void EnterSlide(Slide slide, bool forward)
	{
	}

	public virtual void Update(Slide slide)
	{
	}

	public virtual void ExitSlide(Slide slide, bool forward)
	{
	}

	public virtual Color EditorLabelColor()
	{
		return Color.white;
	}

	public abstract byte[] ToBytes();

	public void Write(ref int modulesList, ref byte[] data, ref ushort[] lengths)
	{
		byte[] array = ToBytes();
		int num = ModuleIndex();
		bool flag = (modulesList & num) > 0;
		int num2 = 1;
		int num3 = 0;
		int num4 = 0;
		while (num2 < num)
		{
			if ((modulesList & num2) > 0)
			{
				num3 += ExtractFromByteCode(num2, data, num3).DataSize();
				num4++;
			}
			num2 <<= 1;
		}
		int num5 = 0;
		if (!flag)
		{
			byte[] array2 = new byte[num3];
			byte[] array3 = new byte[data.Length - num3];
			if (array2.Length != 0)
			{
				Buffer.BlockCopy(data, 0, array2, 0, array2.Length);
			}
			if (array3.Length != 0)
			{
				Buffer.BlockCopy(data, num3, array3, 0, array3.Length);
			}
			data = new byte[data.Length + array.Length];
			if (array2.Length != 0)
			{
				Buffer.BlockCopy(array2, 0, data, 0, array2.Length);
			}
			Buffer.BlockCopy(array, 0, data, array2.Length, array.Length);
			if (array3.Length != 0)
			{
				Buffer.BlockCopy(array3, 0, data, array2.Length + array.Length, array3.Length);
			}
			if (lengths == null)
			{
				lengths = new ushort[1];
			}
			List<ushort> list = new List<ushort>(lengths);
			list.Insert(num4, (ushort)array.Length);
			lengths = list.ToArray();
		}
		else
		{
			int num6 = data.Length;
			num5 = data.Length - num3;
			int num7 = num3 + array.Length + num5;
			byte[] array4 = new byte[0];
			if (num6 != num7)
			{
				if (array4.Length != 0)
				{
					Buffer.BlockCopy(data, data.Length - num5, array4, 0, array4.Length);
				}
				Array.Resize(ref data, num7);
			}
			Buffer.BlockCopy(array, 0, data, num3, array.Length);
			if (array4.Length != 0 && num7 != num6)
			{
				Buffer.BlockCopy(array4, 0, data, num3 + array.Length, array4.Length);
			}
			lengths[num4] = (ushort)array.Length;
		}
		modulesList |= ModuleIndex();
	}

	public void Remove(ref int modulesList, ref byte[] data, ref ushort[] lengths)
	{
		int num = ModuleIndex();
		if ((modulesList & num) <= 0)
		{
			return;
		}
		int num2 = 1;
		int num3 = 0;
		int num4 = 0;
		while (num2 < num)
		{
			if ((modulesList & num2) > 0)
			{
				num3 += ExtractFromByteCode(num2, data, num3).DataSize();
				num4++;
			}
			num2 <<= 1;
		}
		byte[] array = ToBytes();
		if (array.Length != 0)
		{
			num3 += array.Length;
			byte[] array2 = new byte[num3 - array.Length];
			byte[] array3 = new byte[data.Length - num3];
			if (array2.Length != 0)
			{
				Buffer.BlockCopy(data, 0, array2, 0, array2.Length);
			}
			if (array3.Length != 0)
			{
				Buffer.BlockCopy(data, num3, array3, 0, array3.Length);
			}
			data = new byte[data.Length - array.Length];
			if (array2.Length != 0)
			{
				Buffer.BlockCopy(array2, 0, data, 0, array2.Length);
			}
			if (array3.Length != 0)
			{
				Buffer.BlockCopy(array3, 0, data, array2.Length, array3.Length);
			}
		}
		List<ushort> list = new List<ushort>(lengths);
		list.RemoveAt(num4);
		lengths = list.ToArray();
		modulesList &= ~ModuleIndex();
	}

	public static int MustReplaceByteCode(int code)
	{
		return code;
	}

	public static Type GetTypeFromByteCode(int byteCode)
	{
		switch (byteCode)
		{
		case 1:
			return typeof(SlideBlackFrameModule);
		case 2:
			return typeof(SlideBeatAudioModule);
		case 4:
			return typeof(SlideAmbientLightModule);
		case 8:
			return typeof(SlideSectionMarkerModule);
		case 16:
			return typeof(SlideRotationModule);
		case 32:
			return typeof(SlidePlayTimeModule);
		case 64:
			return typeof(SlideBackdropAudioModule);
		case 65536:
			return typeof(SlideShipLogEntryModule);
		default:
			return null;
		}
	}

	public static int GetByteCodeFromType(Type type)
	{
		if (type == typeof(SlideBlackFrameModule))
		{
			return 1;
		}
		if (type == typeof(SlideBeatAudioModule))
		{
			return 2;
		}
		if (type == typeof(SlideAmbientLightModule))
		{
			return 4;
		}
		if (type == typeof(SlideSectionMarkerModule))
		{
			return 8;
		}
		if (type == typeof(SlideRotationModule))
		{
			return 16;
		}
		if (type == typeof(SlidePlayTimeModule))
		{
			return 32;
		}
		if (type == typeof(SlideBackdropAudioModule))
		{
			return 64;
		}
		if (type == typeof(SlideShipLogEntryModule))
		{
			return 65536;
		}
		return 0;
	}

	public static SlideFunctionModule ExtractFromByteCode(int byteCode, byte[] data, int offset, bool readData = true)
	{
		if (readData)
		{
			return GetTypeFromByteCode(byteCode).GetConstructor(new Type[2]
			{
				typeof(byte[]),
				typeof(int)
			}).Invoke(new object[2] { data, offset }) as SlideFunctionModule;
		}
		return GetTypeFromByteCode(byteCode).GetConstructor(new Type[0]).Invoke(new object[0]) as SlideFunctionModule;
	}
}
