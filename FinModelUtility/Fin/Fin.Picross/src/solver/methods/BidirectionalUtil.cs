namespace fin.picross.solver.methods;

public static class BidirectionalUtil {
  public static int GetAbsoluteStartI(
      int iStart,
      int length,
      int increment)
    => increment == 1 ? iStart : iStart - (length - 1);
}