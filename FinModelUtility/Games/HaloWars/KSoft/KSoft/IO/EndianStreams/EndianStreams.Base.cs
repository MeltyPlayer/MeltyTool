using System.IO;

namespace KSoft.IO
{
	partial class EndianReader
	{
		/// <summary>Align the stream's position by a certain page boundry</summary>
		/// <param name="alignmentBit">log2 size of the alignment (ie, 1&lt;&lt;bit)</param>
		/// <returns>True if any alignment had to be performed, false if otherwise</returns>
		public bool AlignToBoundry(int alignmentBit)
		{
			int align_size = IntegerMath.PaddingRequired(alignmentBit, (uint) this.BaseStream.Position);

			if (align_size > 0)
			{
				this.BaseStream.Seek(align_size, SeekOrigin.Current);
				return true;
			}

			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// If we're not the owner, don't let BinaryReader dispose it
				if (!this.BaseStreamOwner)
					kSetBaseStream(this, null);
			}

			this.Owner = null;
			this.StreamName = null;
			this.mStringEncoding = null;
			this.mVAT = null;

			base.Dispose(disposing);
		}
	};

	partial class EndianWriter
	{
		/// <summary>Align the stream's position by a certain page boundry</summary>
		/// <param name="alignmentBit">log2 size of the alignment (ie, 1&lt;&lt;bit)</param>
		/// <returns>True if any alignment had to be performed, false if otherwise</returns>
		public bool AlignToBoundry(int alignmentBit)
		{
			int align_size = IntegerMath.PaddingRequired(alignmentBit, (uint) this.BaseStream.Position);

			if (align_size > 0)
			{
				while (align_size-- > 0)
					this.BaseStream.WriteByte(byte.MinValue);
				return true;
			}

			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!this.BaseStreamOwner)
					this.OutStream = Stream.Null;
			}

			this.Owner = null;
			this.StreamName = null;
			this.mStringEncoding = null;
			this.mVAT = null;

			base.Dispose(disposing);
		}
	};
}