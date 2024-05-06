namespace grezzo.schema.cmb {
  public enum Version {
    OCARINA_OF_TIME_3D = 6,
    MAJORAS_MASK_3D = 10,
    EVER_OASIS = 12,
    LUIGIS_MANSION_3D = 15
  }

  public static class VersionExtensions {
    public static bool SupportsQtrs(this Version version)
      => version > Version.OCARINA_OF_TIME_3D;

    public static bool SupportsMinAndMaxInSepd(this Version version)
      => CmbHeader.Version > Version.EVER_OASIS;

    public static bool SupportsInSepd(this Version version)
      => CmbHeader.Version > Version.OCARINA_OF_TIME_3D;

    public static bool SupportsStencilBuffer(this Version version)
      => CmbHeader.Version > Version.OCARINA_OF_TIME_3D;
  }
}