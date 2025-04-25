namespace nitro.schema.narc;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/nickworonekin/narchive/blob/master/src/Narchive/Formats/NarcArchiveFileEntry.cs
/// </summary>
public class NarcArchiveFileEntry : NarcArchiveEntry {
  /// <summary>
  /// Gets the relative path of the entry.
  /// </summary>
  public override string FullName => Directory != null
      ? System.IO.Path.Combine(Directory.FullName, Name)
      : Name;

  /// <summary>
  /// Gets or sets the <seealso cref="DirectoryEntry"/> this file belongs to.
  /// </summary>
  public NarcArchiveDirectoryEntry Directory { get; set; }

  /// <summary>
  /// Gets or sets the offset of the file data.
  /// </summary>
  internal int Offset { get; set; }

  /// <summary>
  /// Gets or sets the length of the file data.
  /// </summary>
  internal int Length { get; set; }
}