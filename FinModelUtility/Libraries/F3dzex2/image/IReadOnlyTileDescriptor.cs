using f3dzex2.displaylist.opcodes;

namespace f3dzex2.image;

/// <summary>
///   http://ultra64.ca/files/documentation/online-manuals/man/pro-man/pro13/index13.4.html
/// </summary>
public interface IReadOnlyTileDescriptor {
  TileDescriptorState State { get; }
}