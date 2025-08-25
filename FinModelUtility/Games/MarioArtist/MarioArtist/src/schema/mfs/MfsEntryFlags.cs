namespace marioartist.schema.mfs;

public enum MfsEntryFlags : ushort {
  COPY_LIMIT = 1 << 9,
  ENCODE = 1 << 10,
  HIDDEN = 1 << 11,
  DISABLE_READ = 1 << 12,
  DISABLE_WRITE = 1 << 13,
}