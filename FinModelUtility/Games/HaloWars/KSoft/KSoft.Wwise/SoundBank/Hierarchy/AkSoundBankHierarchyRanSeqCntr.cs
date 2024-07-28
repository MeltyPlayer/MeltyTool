
namespace KSoft.Wwise.SoundBank
{
	sealed class AkSoundBankHierarchyRanSeqCntr
		: AkSoundBankHierarchyObjectBase
	{
		public CAkParameterNodeBase ParameterNode = new CAkParameterNodeBase();
		public AkPlaylistItem[] Playlist;

	void SerializeReverseHack2008(IO.EndianStream s)
		{
			const long k_seek_amount = -(sizeof(uint) + AkPlaylistItem.kSizeOf);
			int item_count = 1;
			long terminator = s.BaseStream.Position + 0x74;
			bool read_playlist = false;

			s.Seek(s.VirtualBufferStart + s.VirtualBufferLength);
			do{
				s.Seek(k_seek_amount, System.IO.SeekOrigin.Current);
				if (s.Reader.ReadInt32() == item_count)
				{
					read_playlist = true;
					break;
				}

				item_count++;
			}while(s.BaseStream.Position > terminator);

			if (read_playlist)
			{
				Playlist = new AkPlaylistItem[item_count];
				s.StreamArray(Playlist);
			}
		}
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			uint gen_ver = (s.Owner as AkSoundBank).GeneratorVersion;

			if (gen_ver == AkVersion.k2008.BankGenerator)
				SerializeReverseHack2008(s);
			else
			{
				s.Stream(ParameterNode);
				// 0x18
				s.Pad16(); // LoopCount
				s.Pad32(); // float TransitionTime
				s.Pad32(); // float TransitionTimeModMin
				s.Pad32(); // float TransitionTimeModMax
				s.Pad16(); // AvoidRepeatCount
				s.Pad8(); // TransitionMode
				s.Pad8(); // RandomMode
				s.Pad8(); // Mode
				s.Pad8(); // IsUsingWeight
				s.Pad8(); // ResetPlayListAtEachPlay
				s.Pad8(); // IsRestartBackward
				s.Pad8(); // IsContinuous
				s.Pad8(); // IsGlobal
			}
		}
	};
}