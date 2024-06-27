using System.Collections.Generic;
using System.Linq;

using f3dzex2.displaylist.opcodes;
using f3dzex2.io;

using schema.binary;

namespace f3dzex2.displaylist;

public interface IDisplayListReader {
  IDisplayList ReadDisplayList(
      IReadOnlyN64Memory n64Memory,
      IOpcodeParser opcodeParser,
      uint segmentedAddress);

  IReadOnlyList<IDisplayList> ReadPossibleDisplayLists(
      IReadOnlyN64Memory n64Memory,
      IOpcodeParser opcodeParser,
      uint segmentedAddress);

  IDisplayList ReadDisplayList(IReadOnlyN64Memory n64Memory,
                               IOpcodeParser opcodeParser,
                               SchemaBinaryReader br);
}

public class DisplayListReader : IDisplayListReader {
  public IDisplayList ReadDisplayList(IReadOnlyN64Memory n64Memory,
                                      IOpcodeParser opcodeParser,
                                      uint segmentedAddress)
    => this.ReadPossibleDisplayLists(n64Memory,
                                     opcodeParser,
                                     segmentedAddress)
           .Single();

  public IReadOnlyList<IDisplayList> ReadPossibleDisplayLists(
      IReadOnlyN64Memory n64Memory,
      IOpcodeParser opcodeParser,
      uint segmentedAddress) {
      var options = new LinkedList<IDisplayList>();
      if (Constants.STRICT) {
        foreach (var impl in n64Memory.OpenPossibilitiesAtSegmentedAddress(
                     segmentedAddress)) {
          using var er = impl;
          options.AddLast(this.ReadDisplayList(n64Memory, opcodeParser, er));
        }
      } else {
        if (n64Memory.TryToOpenPossibilitiesAtSegmentedAddress(
                segmentedAddress,
                out var possibilities)) {
          foreach (var impl in possibilities) {
            using var er = impl;
            options.AddLast(this.ReadDisplayList(n64Memory, opcodeParser, er));
          }
        }
      }
      return options.ToArray();
    }

  public IDisplayList ReadDisplayList(IReadOnlyN64Memory n64Memory,
                                      IOpcodeParser opcodeParser,
                                      SchemaBinaryReader br) {
      var opcodeCommands = new LinkedList<IOpcodeCommand>();
      while (true) {
        var opcodeCommand = opcodeParser.Parse(n64Memory, this, br);
        opcodeCommands.AddLast(opcodeCommand);

        if (opcodeCommand is DlOpcodeCommand {PushCurrentDlToStack: false}) {
          break;
        }
        if (opcodeCommand is EndDlOpcodeCommand) {
          break;
        }
      }
      return new DisplayList {
          OpcodeCommands = opcodeCommands.ToArray(), Type = opcodeParser.Type,
      };
    }

  private class DisplayList : IDisplayList {
    public required IReadOnlyList<IOpcodeCommand> OpcodeCommands {
      get;
      init;
    }

    public required DisplayListType Type { get; init; }
  }
}