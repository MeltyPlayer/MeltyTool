using fin.util.types;

namespace marioartist.schema.mfs;

[UnionCandidate]
public interface IMfsEntry {
  MfsEntryFlags Flags { get; set; }
  ushort ParentDirectoryIndex { get; set; }

  string CompanyCode { get; set; }
  string GameCode { get; set; }

  string Name { get; set; }

  byte Renewal { get; set; }
  MfsDate Date { get; }
}