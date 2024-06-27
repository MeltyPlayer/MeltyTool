using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes.f3d;
using f3dzex2.io;

namespace sm64.Scripts {
  public class F3dParser {
    public IDisplayList Parse(IReadOnlyN64Memory n64Memory, uint address) {
      var dlReader = new DisplayListReader();
      var opcodeParser = new F3dOpcodeParser();
      return dlReader.ReadDisplayList(n64Memory, opcodeParser, address);
    }
  }
}
