using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Wwise.FilePackage
{
	public sealed class AkFilePackageExtractor
	{
		public string PackageFileName { get; private set; }
		public AkFilePackage Package { get; private set; }

		public IReadOnlyDictionary<uint, string> EventToSoundNameMap { get; private set; }

		public AkFilePackageExtractor(string packageFileName, AkFilePackage package,
			IReadOnlyDictionary<uint, string> eventToSoundNameMap)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(packageFileName));
			Contract.Requires<ArgumentNullException>(package != null);
			Contract.Requires<ArgumentNullException>(eventToSoundNameMap != null);

			this.PackageFileName = packageFileName;
			this.Package = package;
			this.EventToSoundNameMap = eventToSoundNameMap;
		}

		internal Dictionary<SoundBank.HircType, Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase>> mObjects =
			new Dictionary<SoundBank.HircType, Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase>>();

		internal Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase> mIdToObject =
			new Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase>();

		internal HashSet<uint> mDupObjects = new HashSet<uint>();

		internal Dictionary<uint, SoundBank.MediaReference> mUntouched =
			new Dictionary<uint, SoundBank.MediaReference>();

		Dictionary<uint, SoundBank.AkSoundBank> mIdToBank =
			new Dictionary<uint, SoundBank.AkSoundBank>();

		bool mPreparedForExtraction;
		public void PrepareForExtraction()
		{
			if (this.mPreparedForExtraction)
				return;

			foreach (var bank in this.Package.SoundBanks)
			{
				this.mIdToBank.Add(bank.Id, bank);
				bank.CopyObjectsTo(this);
			}

			this.BuildSoundNames();

			foreach (var bank in this.Package.SoundBanks)
				bank.PrepareForExtraction();

			this.mPreparedForExtraction = true;
		}

		public void BuildSoundNames()
		{
			Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase> events;
			if (!this.mObjects.TryGetValue(SoundBank.HircType.Event, out events))
			{
				Debug.Trace.FilePackage.TraceInformation("{0} - No events?",
				                                         this.PackageFileName);
				return;
			}

			foreach (var kv in events)
			{
				var e = kv.Value as SoundBank.AkSoundBankHierarchyEvent;

				if (!this.EventToSoundNameMap.TryGetValue(e.ID, out e.Name))
					continue;

				foreach (var action_id in e.ActionList)
				{
					var action = this.mIdToObject[action_id] as SoundBank.AkSoundBankHierarchyAction;
					if (action.Type != SoundBank.AkActionType.Play)
						continue;

					var target = this.mIdToObject[action.TargetID];
					if (target is SoundBank.AkSoundBankHierarchySound)
					{
						(target as SoundBank.AkSoundBankHierarchySound).Name = e.Name
							.Replace("play_", "");
					}
					else if (target is SoundBank.AkSoundBankHierarchyRanSeqCntr)
					{
						var ran_seq = target as SoundBank.AkSoundBankHierarchyRanSeqCntr;
						if (ran_seq.Playlist != null)
						{
							foreach (var item in ran_seq.Playlist)
							{
								SoundBank.AkSoundBankHierarchyObjectBase item_obj;
								if (this.mIdToObject.TryGetValue(item.ID, out item_obj) &&
									item_obj is SoundBank.AkSoundBankHierarchySound)
								{
									(item_obj as SoundBank.AkSoundBankHierarchySound).Name = e.Name
										.Replace("play_", "") + "_" + item.ID.ToString("X8");
								}
								else
								{
									Debug.Trace.FilePackage.TraceInformation("{0} - {1}: couldn't name item {2} {3}",
									                                         this.PackageFileName, e.Name, item.ID.ToString("X8"), item.GetType().Name);
								}
							}
						}
						else
						{
							Debug.Trace.FilePackage.TraceInformation("{0} - {1}: couldn't name playlist {2} {3}",
							                                         this.PackageFileName, e.Name, target.ID.ToString("X8"), SoundBank.HircType.RanSeqCntr.ToString());
						}
					}
					else
					{
						Debug.Trace.FilePackage.TraceInformation("{0} - {1}: couldn't name {2} {3}",
						                                         this.PackageFileName, e.Name, target.ID.ToString("X8"), target.ToString());
					}
				}
			}
		}

		public void ExtractSounds(string path, System.IO.StreamWriter towav, IO.EndianReader pckReader,
			bool overwriteExisting = false)
		{
			Dictionary<uint, SoundBank.AkSoundBankHierarchyObjectBase> sounds;
			if (!this.mObjects.TryGetValue(SoundBank.HircType.Sound, out sounds))
			{
				Debug.Trace.FilePackage.TraceInformation("{0} - No sounds to extract?",
				                                         this.PackageFileName);
				return;
			}

			foreach (var kv in sounds)
			{
				var snd = kv.Value as SoundBank.AkSoundBankHierarchySound;
				if (snd.Name == null && this.mDupObjects.Contains(kv.Key))
					continue;

				uint src_id = snd.Source.MediaInfo.SourceID;
				this.mUntouched.Remove(src_id);

				if (snd.Source.MediaInfo.FileID == 0)
				{
					towav.WriteLine("REM NoData {0}", snd.Name);
					continue;
				}

				string dir = null;
				string filename = (snd.Name ?? ("unknown_" + kv.Key.ToString("X8"))) + ".xma";

				uint bank_id = snd.BankId;
				string bank_name;
				if (!this.Package.IdToName.TryGetValue(bank_id, out bank_name))
					bank_name = bank_id.ToString("X8");

				SoundBank.AkSoundBankData bank_data = null;
				bool streamed = snd.Source.StreamType != SoundBank.AkBankSourceData.SourceType.Data;
				if (!streamed)
					bank_data = this.mIdToBank[bank_id].mData;

				dir = System.IO.Path.Combine(path, bank_name);
				System.IO.Directory.CreateDirectory(dir);

				string full_path = System.IO.Path.Combine(dir, filename);
				towav.WriteLine("towav.exe {0}", full_path);

				if (System.IO.File.Exists(full_path) && !overwriteExisting)
					continue;

				using (var fs = System.IO.File.Create(full_path))
				{
					if (streamed)
					{
						var file = this.Package.FindStreamedFileById(snd.Source.MediaInfo.FileID);
						pckReader.Seek(file.FileOffset);
						byte[] data = pckReader.ReadBytes((int)file.FileSize32);

						fs.Write(data, 0, data.Length);
					}
					else
					{
						fs.Write(bank_data.Buffer, (int)snd.Source.MediaInfo.FileOffset, (int)snd.Source.MediaInfo.MediaSize);
					}
				}
			}
			//////////////////////////////////////////////////////////////////////////
			foreach (var kv in this.mUntouched)
			{
				var mr = kv.Value;

				string name = "unknown2_" + mr.Media.ID.ToString("X8");
				if (mr.Media.Size == 0)
				{
					towav.WriteLine("REM NoData2 {0}", name);
					continue;
				}

				string filename = name + ".xma";

				uint bank_id = mr.BankId;
				string bank_name;
				if (!this.Package.IdToName.TryGetValue(bank_id, out bank_name))
					bank_name = bank_id.ToString("X8");

				SoundBank.AkSoundBankData bank_data = this.mIdToBank[bank_id].mData;

				string dir = System.IO.Path.Combine(path, bank_name);
				System.IO.Directory.CreateDirectory(dir);

				string full_path = System.IO.Path.Combine(dir, filename);
				towav.WriteLine("towav.exe {0}", full_path);

				if (System.IO.File.Exists(full_path))
					continue;

				using (var fs = System.IO.File.Create(full_path))
				{
					fs.Write(bank_data.Buffer, (int)mr.Media.Offset, (int)mr.Media.Size);
				}
			}
		}
	};
}