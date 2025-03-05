using f3dzex2.io;

using schema.binary;

namespace f3dzex2.displaylist.opcodes;

public interface IOpcodeCommand;

public interface IOpcodeParser {
  IOpcodeCommand Parse(IReadOnlyN64Memory n64Memory,
                       IDisplayListReader dlr,
                       SchemaBinaryReader br);

  DisplayListType Type { get; }
}