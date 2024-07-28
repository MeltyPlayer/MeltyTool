using System.Collections.Generic;

namespace KSoft.Wwise.FilePackage
{
	public sealed class AkFilePackage
		: IO.IEndianStreamSerializable
	{
		internal AkFilePackageSettings Settings { get; private set; }

		AkFilePackageHeader mHeader;
		AkLanguageMap mLangMap;
		AkFileLookupTable mSoundBanksTable;
		AkFileLookupTable mStreamedFilesTable;
		AkFileLookupTable mExternalFilesTable;

		SoundBank.AkSoundBank[] mSoundBanks;
		Dictionary<uint, string> mIdToName;
		List<KeyValuePair<uint, string>> mIdToNameDups;

		bool HasExternalFiles { get {
			return this.mExternalFilesTable != null;
		} }

		public AkFilePackage(AkFilePackageSettings settings)
		{
			this.Settings = settings;

			this.mLangMap = new AkLanguageMap(settings.UseAsciiStrings);

			this.mSoundBanksTable = new AkFileLookupTable();
			this.mStreamedFilesTable = new AkFileLookupTable();

			if (AkVersion.HasExternalFiles(settings.SdkVersion))
				this.mExternalFilesTable = new AkFileLookupTable();
		}

		public IReadOnlyDictionary<uint, string> IdToName { get { return this.mIdToName; } }
		public IEnumerable<SoundBank.AkSoundBank> SoundBanks { get { return this.mSoundBanks; } }

		internal void MapIdToName(uint id, string name)
		{
			string existing_name;
			if (this.mIdToName.TryGetValue(id, out existing_name))
			{
				if (existing_name != name)
					this.mIdToNameDups.Add(new KeyValuePair<uint, string>(id, name));
			}
			else
				this.mIdToName.Add(id, name);
		}

		internal AkFileLookupTableEntry FindStreamedFileById(ulong streamedFileId)
		{
			return this.mStreamedFilesTable.Find(streamedFileId);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Owner = this;

			if(s.IsWriting)
				this.mHeader.InitializeSize(this.Settings.SdkVersion, this.mLangMap.TotalMapSize);

			s.Stream(ref this.mHeader);
			s.Stream(ref this.mLangMap.TotalMapSize);
			s.Stream(ref this.mSoundBanksTable.TotalSize);
			s.Stream(ref this.mStreamedFilesTable.TotalSize);
			if (this.HasExternalFiles) s.Stream(ref this.mExternalFilesTable.TotalSize);
			s.Stream(this.mLangMap);
			s.Stream(this.mSoundBanksTable);
			s.Stream(this.mStreamedFilesTable);
			if (this.HasExternalFiles) s.Stream(this.mExternalFilesTable);
		}

		public void SerializeSoundBanks(IO.EndianStream s)
		{
			if (s.IsReading)
			{
				this.mSoundBanks = new SoundBank.AkSoundBank[this.mSoundBanksTable.Count];
				this.mIdToName = new Dictionary<uint, string>(this.mSoundBanks.Length);
				this.mIdToNameDups = new List<KeyValuePair<uint, string>>();
				for (int x = 0; x < this.mSoundBanksTable.Count; x++)
				{
					var entry = this.mSoundBanksTable[x];
					this.mSoundBanks[x] = new SoundBank.AkSoundBank((long)entry.FileSize64, entry.FileOffset, this);
				}
			}

			for (int x = 0; x < this.mSoundBanks.Length; x++)
				s.Stream(this.mSoundBanks[x]);

			this.mSoundBanks.ToString();
		}
		#endregion
	};
}