using System.Collections.Generic;

using f3dzex2.displaylist.opcodes;


namespace f3dzex2.displaylist;

public static class DisplayListExtensions {
  public static IEnumerable<IOpcodeCommand> Flatten(
      this IDisplayList displayList) {
    foreach (var opcodeCommand in displayList.OpcodeCommands) {
      yield return opcodeCommand;

      if (opcodeCommand is not DlOpcodeCommand dlOpcodeCommand) {
        continue;
      }

      foreach (var possibleBranch in dlOpcodeCommand.PossibleBranches) {
        foreach (var branchOpcodeCommand in possibleBranch.Flatten()) {
          yield return branchOpcodeCommand;
        }
      }
    }
  }

  public static IEnumerable<string> FlattenLabels(
      this IDisplayList displayList,
      int depth = 0) {
    foreach (var opcodeCommand in displayList.OpcodeCommands) {
      yield return $"{depth} -- {opcodeCommand}";

      if (opcodeCommand is not DlOpcodeCommand dlOpcodeCommand) {
        continue;
      }

      foreach (var possibleBranch in dlOpcodeCommand.PossibleBranches) {
        foreach (var branchOpcodeCommand in possibleBranch.FlattenLabels(depth + 1)) {
          yield return branchOpcodeCommand;
        }
      }
    }
  }
}