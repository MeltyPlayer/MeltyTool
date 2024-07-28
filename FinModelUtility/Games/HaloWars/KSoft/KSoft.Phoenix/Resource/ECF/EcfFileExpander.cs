using System;
using System.IO;

namespace KSoft.Phoenix.Resource.ECF
{
	public enum EcfFileExpanderOptions
	{
		/// <summary>Only the ECF's file listing (.xml) is generated</summary>
		OnlyDumpListing,
		/// <summary>Files that already exist in the output directory will be skipped</summary>
		DontOverwriteExistingFiles,
		DontSaveChunksToFiles,
		DontLoadEntireEcfIntoMemory,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public sealed class EcfFileExpander
		: EcfFileUtil
	{
		Stream mEcfBaseStream;
		IO.EndianStream mEcfStream;

		/// <see cref="EcfFileExpanderOptions"/>
		public Collections.BitVector32 ExpanderOptions;

		public EcfFileExpander(string ecfPath)
		{
			this.mSourceFile = ecfPath;
		}

		public override void Dispose()
		{
			base.Dispose();

			Util.DisposeAndNull(ref this.mEcfBaseStream);
			Util.DisposeAndNull(ref this.mEcfStream);
		}

		#region Reading
		public bool Read()
		{
			bool result = true;

			try { result &= this.ReadEcfFromFile(); }
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while trying to read the ECF: {0}", ex);
				result = false;
			}

			return result;
		}

		bool ReadEcfFromFile()
		{
			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Opening and reading ECF file {0}...",
				                              this.mSourceFile);

			if (this.ExpanderOptions.Test(EcfFileExpanderOptions.DontLoadEntireEcfIntoMemory))
				this.mEcfBaseStream = File.OpenRead(this.mSourceFile);
			else
			{
				byte[] ecf_bytes = File.ReadAllBytes(this.mSourceFile);

				this.mEcfBaseStream = new MemoryStream(ecf_bytes, writable: false);
			}

			this.mEcfStream = new IO.EndianStream(this.mEcfBaseStream, Shell.EndianFormat.Big, this, permissions: FileAccess.Read);
			this.mEcfStream.StreamMode = FileAccess.Read;

			return this.ReadEcfFromStream();
		}

		bool ReadEcfFromStream()
		{
			bool result = true;

			result = EcfHeader.VerifyIsEcf(this.mEcfStream.Reader);
			if (!result)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tFailed: File is either not even an ECF-based file, or corrupt");
			}
			else
			{
				this.mEcfFile = new EcfFile();
				this.mEcfFile.Serialize(this.mEcfStream);
			}

			return result;
		}
		#endregion

		#region Expanding
		bool WriteChunksToFile { get { return this.ExpanderOptions.Test(EcfFileExpanderOptions.DontSaveChunksToFiles) == false; } }

		public bool ExpandTo(string workPath, string listingName)
		{
			if (this.mEcfFile == null)
				return false;

			if (!Directory.Exists(workPath))
				Directory.CreateDirectory(workPath);

			bool result = true;

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Outputting listing...");

			try
			{
				this.PopulateEcfDefinitionFromEcfFile(workPath);
				this.SaveListing(workPath, listingName);
			}
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while outputting listing: {0}", ex);
				result = false;
			}

			if (result && !this.ExpanderOptions.Test(EcfFileExpanderOptions.OnlyDumpListing) && this.WriteChunksToFile)
			{
				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Expanding ECF to {0}...", workPath);

				try
				{
					this.ExpandChunksToFiles();
				}
				catch (Exception ex)
				{
					if (this.VerboseOutput != null)
						this.VerboseOutput.WriteLine("\tEncountered an error while expanding ECF: {0}", ex);
					result = false;
				}

				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Done");
			}

			this.mEcfStream.Close();

			return result;
		}

		void SaveListing(string workPath, string listingName)
		{
			string listing_filename = Path.Combine(workPath, listingName);

			using (var xml = IO.XmlElementStream.CreateForWrite("EcfFile", this))
			{
				xml.InitializeAtRootElement();
				xml.StreamMode = FileAccess.Write;

				this.EcfDefinition.Serialize(xml);

				xml.Document.Save(listing_filename + EcfFileDefinition.kFileExtension);
			}
		}

		void PopulateEcfDefinitionFromEcfFile(string workPath)
		{
			this.EcfDefinition.WorkingDirectory = workPath;
			this.EcfDefinition.Initialize(this.mSourceFile);

			this.mEcfFile.CopyHeaderDataTo(this.EcfDefinition);

			int raw_chunk_index = 0;
			foreach (var rawChunk in this.mEcfFile)
			{
				var chunk = this.EcfDefinition.Add(rawChunk, raw_chunk_index++);

				if (this.WriteChunksToFile)
					chunk.SetFilePathFromParentNameAndId();
			}

			if (!this.WriteChunksToFile)
				this.ReadEcfChunksToDefinitionBytes();
		}

		void ReadEcfChunksToDefinitionBytes()
		{
			foreach (var chunk in this.EcfDefinition.Chunks)
			{
				var raw_chunk = this.mEcfFile.GetChunk(chunk.RawChunkIndex);

				try
				{
					var chunk_bytes = raw_chunk.GetBuffer(this.mEcfStream);
					chunk.SetFileBytes(chunk_bytes);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format(
						"ReadEcfChunksToDefinitionBytes failed on chunk {0} in {1}",
						chunk.Id.ToString("X8"),
						this.mEcfStream.StreamName
					), e);
				}
			}
		}

		void ExpandChunksToFiles()
		{
			foreach (var chunk in this.EcfDefinition.Chunks)
			{
				var raw_chunk = this.mEcfFile.GetChunk(chunk.RawChunkIndex);

				try
				{
					this.ExpandChunkToFile(chunk, raw_chunk);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format(
						"ExpandChunksToFiles failed on chunk {0} in {1}",
						chunk.Id.ToString("X8"),
						this.mEcfStream.StreamName
					), e);
				}
			}
		}

		void ExpandChunkToFile(EcfFileChunkDefinition chunk, EcfChunk rawChunk)
		{
			string file_path = this.EcfDefinition.GetChunkAbsolutePath(chunk);

			if (!this.ExpanderOptions.Test(EcfFileExpanderOptions.DontOverwriteExistingFiles))
			{
				if (File.Exists(file_path))
				{
					if (this.VerboseOutput != null)
						this.VerboseOutput.WriteLine("\tSkipping chunk, output file already exists: {0}", file_path);

					return;
				}
			}

			using (var fs = File.OpenWrite(file_path))
			{
				var chunk_bytes = rawChunk.GetBuffer(this.mEcfStream);
				fs.Write(chunk_bytes, 0, chunk_bytes.Length);
			}
		}
		#endregion
	};
}