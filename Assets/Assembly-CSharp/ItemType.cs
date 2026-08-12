using System;

[Flags]
public enum ItemType
{
	Invalid = 0,
	Scroll = 1,
	WarpCore = 2,
	SharedStone = 4,
	ConversationStone = 8,
	Lantern = 0x10,
	SlideReel = 0x20,
	DreamLantern = 0x40,
	VisionTorch = 0x80
}
