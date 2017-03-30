using System;

namespace Frost_Sharp.Boards {
	[Flags] public enum Types {
		Board = 0x1,
		Outbox = 0x2,
		Sent = 0x4,
		Folder = 0x8,
	}
}
