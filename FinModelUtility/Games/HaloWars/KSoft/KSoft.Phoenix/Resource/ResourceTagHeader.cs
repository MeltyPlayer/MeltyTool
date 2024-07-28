
namespace KSoft.Phoenix.Resource
{
	public enum ResourceTagPlatformId : byte
	{
		Any,
		Pc,
		Xbox,
		_64bit,
	};

	public sealed class ResourceTagHeader
		: IO.IEndianStreamSerializable
	{
		public const ulong kChunkId = 0x00000000714BFE00;

		const ushort kSignature = 0x714C;
		const byte kMajorVersion = 0x11;
		const byte kMinorVersion = 0x0;

		public Shell.ProcessorSize CreatorPointerSize { get; private set; }

		public ushort HeaderSize;
		public ushort DataSize;
		public uint HeaderAdler32;

		public ulong TagTimeStamp; // FILETIME
		public Values.KGuid TagGuid;
		Values.PtrHandle TagMachineNameOffset;
		Values.PtrHandle TagUserNameOffset;

		Values.PtrHandle SourceFileName;
		public byte[] SourceDigest = new byte[Security.Cryptography.PhxHash.kSha1SizeOf];
		public ulong SourceFileSize;
		public ulong SourceFileTimeStamp;

		Values.PtrHandle CreatorToolCommandLine;
		public byte CreatorToolVersion;

		private byte mPlatformId;
		public ResourceTagPlatformId PlatformId
		{
			get { return (ResourceTagPlatformId) this.mPlatformId; }
			set { this.mPlatformId = (byte)value; }
		}

		public ResourceTagHeader(Shell.ProcessorSize pointerSize = Shell.ProcessorSize.x32)
		{
			if (pointerSize == Shell.ProcessorSize.x32)
			{
				this.TagMachineNameOffset = Values.PtrHandle.InvalidHandle32;
				this.TagUserNameOffset = Values.PtrHandle.InvalidHandle32;
				this.CreatorToolCommandLine = Values.PtrHandle.InvalidHandle32;
			}
			else
			{
				this.TagMachineNameOffset = Values.PtrHandle.InvalidHandle64;
				this.TagUserNameOffset = Values.PtrHandle.InvalidHandle64;
				this.CreatorToolCommandLine = Values.PtrHandle.InvalidHandle64;
			}
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			using (s.BeginEndianSwitch(Shell.EndianFormat.Little))
			{
				s.VirtualAddressTranslationInitialize(this.CreatorPointerSize);
				s.VirtualAddressTranslationPushPosition();
				this.SerializeBody(s);
				s.VirtualAddressTranslationPop();
			}
		}

		private void SerializeBody(IO.EndianStream s)
		{
			s.StreamSignature(kSignature);
			s.StreamVersion(kMajorVersion);
			s.StreamVersion(kMinorVersion);

			s.Stream(ref this.HeaderSize);
			s.Stream(ref this.DataSize);
			s.Stream(ref this.HeaderAdler32);

			s.Stream(ref this.TagTimeStamp);
			s.Stream(ref this.TagGuid);

			s.StreamVirtualAddress(ref this.TagMachineNameOffset);
			s.StreamVirtualAddress(ref this.TagUserNameOffset);

			s.StreamVirtualAddress(ref this.SourceFileName);
			s.Stream(this.SourceDigest);
			s.Stream(ref this.SourceFileSize);
			s.Stream(ref this.SourceFileTimeStamp);

			s.StreamVirtualAddress(ref this.CreatorToolCommandLine);
			s.Stream(ref this.CreatorToolVersion);

			s.Stream(ref this.mPlatformId);
			s.Pad(sizeof(byte) + sizeof(uint));
		}

		public bool StreamTagMachineName(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref this.TagMachineNameOffset, ref value);
		}

		public bool StreamTagUserName(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref this.TagUserNameOffset, ref value);
		}

		public bool StreamSourceFileNamee(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref this.SourceFileName, ref value);
		}

		public bool StreamCreatorToolCommandLine(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref this.CreatorToolCommandLine, ref value);
		}
		#endregion
	};
}