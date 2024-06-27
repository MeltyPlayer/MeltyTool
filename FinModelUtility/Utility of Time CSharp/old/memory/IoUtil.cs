namespace UoT {
  public static class IoUtil {
    // TODO: Rename params.
    /// <summary>
    ///   Gets multiple bits from the right-hand side of a value.
    /// </summary>
    public static uint ShiftR(uint v, int s, int w)
      => (uint)((v >> s) & ((1 << w) - 1));
  }
}
