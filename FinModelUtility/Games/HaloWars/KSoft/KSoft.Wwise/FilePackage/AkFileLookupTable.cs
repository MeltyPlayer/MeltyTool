using System;
using System.Collections.Generic;

namespace KSoft.Wwise.FilePackage
{
	sealed class AkFileLookupTable
		: IO.IEndianStreamSerializable
		, IEnumerable<AkFileLookupTableEntry>
	{
		AkFileLookupTableEntry[] mEntries;

		internal uint TotalSize;

		public int Count { get { return this.mEntries.Length; } }

		public AkFileLookupTableEntry this[int index]	{ get { return this.mEntries[index]; } }

		public AkFileLookupTableEntry Find(ulong id)
		{
			return Array.Find(this.mEntries, e => e.FileId64 == id);
		}

		public uint CalculateTotalSize()
		{
			return sizeof(uint) + (uint)(this.mEntries.Length * AkFileLookupTableEntry.kSizeOf);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamArrayInt32(ref this.mEntries);
		}
		#endregion

		#region IEnumerable<FileEntry> Members
		public IEnumerator<AkFileLookupTableEntry> GetEnumerator()
		{
			return (this.mEntries as IEnumerable<AkFileLookupTableEntry>).GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.mEntries.GetEnumerator();
		}
		#endregion
	};
}