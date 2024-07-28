
namespace KSoft.Wwise.SoundBank
{
	using SourceTypeStreamer = IO.EnumBinaryStreamer<AkBankSourceData.SourceType>;

	sealed class AkBankSourceData
		: IO.IEndianStreamSerializable
	{
		enum AkPluginType
		{
			None,
			Codec,
			Source,
			Effect,
			MotionDevice,
			MotionSource,
		};
		public enum SourceType : uint
		{
			Data,
			Streaming,
			PrefetchStreaming,

			NotInitialized = uint.MaxValue
		};

		const uint AkPluginTypeMask = 0xF;

		public uint PluginID;
		public SourceType StreamType;
		public uint AudioFormat_SampleRate;
		// uChannelMask : 18
		// uBitsPerSample : 6
		// uBlockAlign : 5
		// uTypeID : 2
		// uInterleaveID : 1
		public uint AudioFormat_Bits;
		public AkMediaInformation MediaInfo;

		public CAkParameterNodeBase ParameterNode = new CAkParameterNodeBase();

		public bool Prefetch { get { return this.StreamType == SourceType.PrefetchStreaming; } }

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.PluginID);
			s.Stream(ref this.StreamType, SourceTypeStreamer.Instance);
			s.Stream(ref this.AudioFormat_SampleRate);
			s.Stream(ref this.AudioFormat_Bits);
			s.Stream(ref this.MediaInfo.SourceID);
			s.Stream(ref this.MediaInfo.FileID);
			if (this.StreamType != SourceType.Streaming)
			{
				s.Stream(ref this.MediaInfo.FileOffset);
				s.Stream(ref this.MediaInfo.MediaSize);
			}
			s.Stream(ref this.MediaInfo.IsLanguageSpecific);

			var ptype = (AkPluginType)(this.PluginID & AkPluginTypeMask);
			if (ptype == AkPluginType.Source || ptype == AkPluginType.MotionSource)
				s.Pad32(); // size
#if false
			s.Stream(ParameterNode);
#endif
			// There's more...
		}
		#endregion
	};
}