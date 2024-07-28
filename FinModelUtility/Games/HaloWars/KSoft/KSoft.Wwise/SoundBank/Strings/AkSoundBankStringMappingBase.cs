using System.Runtime.InteropServices;

namespace KSoft.Wwise.SoundBank
{
	using StringTypeStreamer = IO.EnumBinaryStreamer<AkSoundBankStringMappingBase.StringType>;

	partial class AkSoundBankObjectBase
	{
		static readonly Values.GroupTagData32 kStringMappingSignature =
					new Values.GroupTagData32("STID", "audiokinetic_string_mapping"); // BankStrMapChunkID

		static readonly AkSoundBankStringMapping kBankNamesMappingObject = new AkSoundBankStringMapping();

		static AkSoundBankObjectBase NewSTID(uint generatorVersion)
		{
			if (AkVersion.BankHasOldSTID(generatorVersion))
				return new AkSoundBankStringMapping2007();

			return kBankNamesMappingObject;
		}
	};

	abstract class AkSoundBankStringMappingBase
		: AkSoundBankObjectBase
	{
		[System.Reflection.Obfuscation(Exclude=true)]
		internal enum StringType : uint
		{
			None,
			Bank,

			OldEvents = 1,
			Old2, // states?
			Old3, // skip
			Old4,
			Old5, // switches?
			Old6, // skip
			Old7,
			Old8,
			Old9,
			Old10, // skip
			Old11,
		};

		[StructLayout(LayoutKind.Explicit)]
		protected struct AKBKHashHeader
			: IO.IEndianStreamSerializable
		{
			[FieldOffset(0)] public StringType Type;
			[FieldOffset(0)] public uint Hash; // 2007
			[FieldOffset(4)] public uint Size;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.Type, StringTypeStreamer.Instance);
				s.Stream(ref this.Size);
			}
			#endregion
		};

		protected static long EndOfStream(IO.EndianStream s, AKBKHashHeader header)
		{
			return s.BaseStream.Position + header.Size;
		}
	};
}