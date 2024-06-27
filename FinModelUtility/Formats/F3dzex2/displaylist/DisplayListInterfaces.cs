using System.Collections.Generic;

using f3dzex2.displaylist.opcodes;

namespace f3dzex2.displaylist;

public enum DisplayListType {
  FAST3D,
  F3DZEX2,
}

public interface IDisplayList {
  IReadOnlyList<IOpcodeCommand> OpcodeCommands { get; }
  DisplayListType Type { get; }
}