using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Xmb
{
	public enum BinaryDataTreeHeaderSignature : byte
	{
		LittleEndian = 0x3E,
		BigEndian = 0xE3,
	};

	public enum BinaryDataTreeSectionID
	{
		NodeSectionIndex,
		NameValueSectionIndex, // 4b align
		NameDataSectionIndex, // 4b align
		ValueDataSectionIndex, // 16b align

		kNumberOf,
	};

	public struct BinaryDataTreeHeader
		: IO.IEndianStreamSerializable
	{
		private const byte kExpectedSize = 1+1+1+1+4+4+ (4 * (int)BinaryDataTreeSectionID.kNumberOf);
		private const byte kExpectedHeaderDwordCount = kExpectedSize / sizeof(uint);

		public const uint kSizeOf = kExpectedSize;

		public BinaryDataTreeHeaderSignature Signature;
		//private byte mHeaderDwordCount;
		public byte HeaderCrc8;
		public byte UserSectionCount;

		public uint DataCrc32;
		public uint DataSize;

		public uint BaseSectionSize0;
		public uint BaseSectionSize1;
		public uint BaseSectionSize2;
		public uint BaseSectionSize3;

		public uint this[BinaryDataTreeSectionID sectionId]
		{
			get
			{
				switch ((int)sectionId)
				{
					case 0: return this.BaseSectionSize0;
					case 1: return this.BaseSectionSize1;
					case 2: return this.BaseSectionSize2;
					case 3: return this.BaseSectionSize3;

					default:
						throw new KSoft.Debug.UnreachableException(sectionId.ToString());
				}
			}
			set
			{
				switch ((int)sectionId)
				{
					case 0:
						this.BaseSectionSize0 = value;
						break;
					case 1:
						this.BaseSectionSize1 = value;
						break;
					case 2:
						this.BaseSectionSize2 = value;
						break;
					case 3:
						this.BaseSectionSize3 = value;
						break;

					default:
						throw new KSoft.Debug.UnreachableException(sectionId.ToString());
				}
			}
		}

		public Shell.EndianFormat SignatureAsEndianFormat
		{
			get
			{
				return this.Signature == BinaryDataTreeHeaderSignature.LittleEndian
					? Shell.EndianFormat.Little
					: Shell.EndianFormat.Big;
			}
			set
			{
				this.Signature = value == Shell.EndianFormat.Little
					? BinaryDataTreeHeaderSignature.LittleEndian
					: BinaryDataTreeHeaderSignature.BigEndian;
			}
		}

		public ushort GetCrc16()
		{
			var computer = new Security.Cryptography.Crc16.BitComputer(PhxUtil.kCrc16Definition);
			computer.ComputeBegin();

			computer.Compute((byte) this.Signature);
			computer.Compute(kExpectedHeaderDwordCount);
			computer.Compute(byte.MinValue);
			computer.Compute(this.UserSectionCount);

			var byte_order = this.SignatureAsEndianFormat;

			computer.Compute(byte_order, this.DataCrc32);
			computer.Compute(byte_order, this.DataSize);

			computer.Compute(byte_order, this.BaseSectionSize0);
			computer.Compute(byte_order, this.BaseSectionSize1);
			computer.Compute(byte_order, this.BaseSectionSize2);
			computer.Compute(byte_order, this.BaseSectionSize3);

			return computer.ComputeFinish();
		}

		public void Serialize(IO.EndianStream s)
		{
			bool reading = s.IsReading;

			byte signature = (byte) this.Signature;

			if (!reading)
				this.HeaderCrc8 = (byte) this.GetCrc16();

			s.Stream(ref signature);
			s.StreamSignature(kExpectedHeaderDwordCount);
			s.Stream(ref this.HeaderCrc8);
			s.Stream(ref this.UserSectionCount);

			s.Stream(ref this.DataCrc32);
			s.Stream(ref this.DataSize);

			s.Stream(ref this.BaseSectionSize0);
			s.Stream(ref this.BaseSectionSize1);
			s.Stream(ref this.BaseSectionSize2);
			s.Stream(ref this.BaseSectionSize3);

			if (reading)
			{
				this.Signature = (BinaryDataTreeHeaderSignature)signature;
			}
		}

		public void Validate()
		{
			var actual_crc = (byte) this.GetCrc16();
			if (actual_crc != this.HeaderCrc8)
				throw new InvalidDataException(string.Format("Invalid CRC 0x{0}, expected 0x{1}",
					actual_crc.ToString("X2"),
					this.HeaderCrc8.ToString("X2")));
		}

		public static BinaryDataTreeHeaderSignature PeekSignature(BinaryReader reader)
		{
			Contract.Requires(reader != null);

			var peek = reader.PeekByte();

			switch (peek)
			{
				case (int)BinaryDataTreeHeaderSignature.LittleEndian:
				case (int)BinaryDataTreeHeaderSignature.BigEndian:
					return (BinaryDataTreeHeaderSignature)peek;

				default:
					throw new InvalidDataException(peek.ToString("X8"));
			}
		}

		public static Shell.EndianFormat PeekSignatureAsEndianFormat(BinaryReader reader)
		{
			Contract.Requires(reader != null);

			var signature = PeekSignature(reader);

			return signature == BinaryDataTreeHeaderSignature.LittleEndian
				? Shell.EndianFormat.Little
				: Shell.EndianFormat.Big;
		}
	};

	public struct BinaryDataTreeSectionHeader
		: IO.IEndianStreamSerializable
	{
		public const uint kSizeOf = sizeof(uint) * 3;

		public uint Id;
		public uint Size;
		public uint Offset;

		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.Id);
			s.Stream(ref this.Size);
			s.Stream(ref this.Offset);
		}
	};
}