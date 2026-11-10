// ReSharper disable InconsistentNaming

using ModelPluginWrappers.noesis;

namespace ModelPluginWrappers.noesis;

public sealed class IncNoesis {
  public static INoeBitStream NoeBitStream(byte[]? data = null)
    => new NoeBitStreamReader(data ?? []);
}