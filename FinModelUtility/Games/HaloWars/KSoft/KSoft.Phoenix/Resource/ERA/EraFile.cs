using System;
using System.Collections.Generic;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource
{
	public sealed class EraFile
		: IO.IEndianStreamSerializable
		, IDisposable
	{
		public const string kExtensionEncrypted = ".era";
		public const string kExtensionDecrypted = ".bin";

		const int kAlignmentBit = IntegerMath.kFourKiloAlignmentBit;
		const string kFileNamesTableName = "_file_names.bin";
		static readonly Memory.Strings.StringMemoryPoolSettings kFileNamesTablePoolConfig = new Memory.Strings.
			StringMemoryPoolSettings(Memory.Strings.StringStorage.CStringAscii, false);

		public string FileName { get; set; }

		private EraFileHeader mHeader = new EraFileHeader();
		private List<EraFileEntryChunk> mFiles = new List<EraFileEntryChunk>();
		private Dictionary<string, string> mLocalFiles = new Dictionary<string, string>();
		private Dictionary<string, EraFileEntryChunk> mFileNameToChunk = new Dictionary<string, EraFileEntryChunk>();

		public DateTime BuildModeDefaultTimestamp { get; set; }
			= DateTime.Now;

		public Security.Cryptography.TigerHashBase TigerHasher { get; private set; } = Security.Cryptography.PhxHash.CreateHaloWarsTigerHash();

		private int FileChunksFirstIndex { get {
			// First comes the filenames table in mFiles, then all the files defined in the listing
			return 1;
		} }

	/// <summary>Number of files destined for the ERA, excluding the internal filenames table</summary>
	private int FileChunksCount { get {
			// Exclude the first chunk from the count, as it is the filenames table
			return this.mFiles.Count - this.FileChunksFirstIndex;
		} }

		#region IDisposable Members
		public void Dispose()
		{
			if (this.TigerHasher != null)
			{
				this.TigerHasher.Dispose();
				this.TigerHasher = null;
			}
		}
		#endregion

		public int CalculateHeaderAndFileChunksSize()
		{
			return
				EraFileHeader.CalculateHeaderSize() +
				EraFileEntryChunk.CalculateFileChunksSize(this.mFiles.Count);
		}

		private void ValidateAdler32(EraFileEntryChunk fileEntry, IO.EndianStream blockStream)
		{
			var actual_adler = fileEntry.ComputeAdler32(blockStream);

			if (actual_adler != fileEntry.Adler32)
			{
				string chunk_name = !string.IsNullOrEmpty(fileEntry.FileName)
					? fileEntry.FileName
					: "FileNames";//fileEntry.EntryId.ToString("X16");

				throw new InvalidDataException(string.Format(
					"Invalid chunk adler32 for '{0}' offset={1} size={2} " +
					"expected {3} but got {4}",
					chunk_name, fileEntry.DataOffset, fileEntry.DataSize.ToString("X8"),
					fileEntry.Adler32.ToString("X8"),
					actual_adler.ToString("X8")
					));
			}
		}

		private void ValidateHashes(EraFileEntryChunk fileEntry, IO.EndianStream blockStream)
		{
			fileEntry.ComputeHash(blockStream, this.TigerHasher);

			if (!fileEntry.CompressedDataTiger128.EqualsArray(this.TigerHasher.Hash))
			{
				string chunk_name = !string.IsNullOrEmpty(fileEntry.FileName)
					? fileEntry.FileName
					: "FileNames";//fileEntry.EntryId.ToString("X16");

				throw new InvalidDataException(string.Format(
					"Invalid chunk hash for '{0}' offset={1} size={2} " +
					"expected {3} but got {4}",
					chunk_name, fileEntry.DataOffset, fileEntry.DataSize.ToString("X8"),
					Text.Util.ByteArrayToString(fileEntry.CompressedDataTiger128),
					Text.Util.ByteArrayToString(this.TigerHasher.Hash, 0, EraFileEntryChunk.kCompresssedDataTigerHashSize)
					));
			}

			if (fileEntry.CompressionType == ECF.EcfCompressionType.Stored)
			{
				ulong tiger64;
				this.TigerHasher.TryGetAsTiger64(out tiger64);

				if (fileEntry.DecompressedDataTiger64 != tiger64)
				{
					string chunk_name = !string.IsNullOrEmpty(fileEntry.FileName)
						? fileEntry.FileName
						: "FileNames";//fileEntry.EntryId.ToString("X16");

					throw new InvalidDataException(string.Format(
						"Chunk id mismatch for '{0}' offset={1} size={2} " +
						"expected {3} but got {4}",
						chunk_name, fileEntry.DataOffset, fileEntry.DataSize.ToString("X8"),
						fileEntry.DecompressedDataTiger64.ToString("X16"),
						Text.Util.ByteArrayToString(this.TigerHasher.Hash, 0, sizeof(ulong))
						));
				}
			}
		}

		private int FileIndexToListingIndex(int fileIndex)
		{
			return fileIndex - 1;
		}

		private void BuildFileNameMaps(TextWriter verboseOutput)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles.Count; )
			{
				var file = this.mFiles[x];

				EraFileEntryChunk existingFile;
				if (this.mFileNameToChunk.TryGetValue(file.FileName, out existingFile))
				{
					if (verboseOutput != null)
					{
						verboseOutput.WriteLine("Removing duplicate {0} entry at #{1}",
							file.FileName,
							this.FileIndexToListingIndex(x));
					}

					this.mFiles.RemoveAt(x);
					continue;
				}

				this.mFileNameToChunk.Add(file.FileName, file);
				x++;
			}
		}

		private void RemoveXmbFilesWhereXmlExists(TextWriter verboseOutput)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles.Count; x++)
			{
				var file = this.mFiles[x];
				if (!ResourceUtils.IsXmbFile(file.FileName))
					continue;

				string xml_name = file.FileName;
				ResourceUtils.RemoveXmbExtension(ref xml_name);
				EraFileEntryChunk xml_file;
				if (!this.mFileNameToChunk.TryGetValue(xml_name, out xml_file))
					continue;

				if (verboseOutput != null)
					verboseOutput.WriteLine("\tRemoving XMB file #{0} '{1}' from listing since its XML already exists {2}",
					                        this.FileIndexToListingIndex(x),
						file.FileName,
						xml_file.FileName);

				this.mFiles.RemoveAt(x);
				x--;
			}
		}

		private void RemoveXmlFilesWhereXmbExists(TextWriter verboseOutput)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles.Count; x++)
			{
				var file = this.mFiles[x];
				if (!ResourceUtils.IsXmlBasedFile(file.FileName))
					continue;

				string xmb_name = file.FileName;
				xmb_name += Xmb.XmbFile.kFileExt;
				EraFileEntryChunk xmb_file;
				if (!this.mFileNameToChunk.TryGetValue(xmb_name, out xmb_file))
					continue;

				if (verboseOutput != null)
					verboseOutput.WriteLine("\tRemoving XML file #{0} '{1}' from listing since its XMB already exists {2}",
					                        this.FileIndexToListingIndex(x),
						file.FileName,
						xmb_file.FileName);

				this.mFiles.RemoveAt(x);
				x--;
			}
		}

		public void TryToReferenceXmlOverXmbFies(string workPath, TextWriter verboseOutput)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles.Count; x++)
			{
				var file = this.mFiles[x];
				if (!ResourceUtils.IsXmbFile(file.FileName))
					continue;

				string xml_name = file.FileName;
				ResourceUtils.RemoveXmbExtension(ref xml_name);

				// if the user already references the XML file too, just skip doing anything
				EraFileEntryChunk xml_file;
				if (this.mFileNameToChunk.TryGetValue(xml_name, out xml_file))
					continue;

				// does the XML file exist?
				string xml_path = Path.Combine(workPath, xml_name);
				if (!File.Exists(xml_path))
					continue;

				if (verboseOutput != null)
					verboseOutput.WriteLine("\tReplacing XMB ref with {0}",
						xml_name);

				// right now, all we should need to do to update things is remove the XMB mapping and replace it with the XML we found
				bool removed = this.mFileNameToChunk.Remove(file.FileName);
				file.FileName = xml_name;
				if (removed)
					this.mFileNameToChunk.Add(xml_name, file);
			}
		}

		#region Xml definition Streaming
		static EraFileEntryChunk GenerateFileNamesTableEntryChunk()
		{
			var chunk = new EraFileEntryChunk();
			chunk.CompressionType = ECF.EcfCompressionType.DeflateStream;

			return chunk;
		}
		private void ReadChunks(IO.XmlElementStream s)
		{
			foreach (var n in s.ElementsByName(ECF.EcfChunk.kXmlElementStreamName))
			{
				var f = new EraFileEntryChunk();
				using (s.EnterCursorBookmark(n))
				{
					f.Read(s, false);
				}

				this.mFiles.Add(f);
			}
		}
		private void ReadLocalFiles(IO.XmlElementStream s)
		{
			foreach (var n in s.ElementsByName("file"))
			{
				using (s.EnterCursorBookmark(n))
				{
					string file_name = null;
					s.ReadAttribute("name", ref file_name);

					string file_data = "";
					s.ReadCursor(ref file_data);

					if (!string.IsNullOrEmpty(file_name))
						this.mLocalFiles[file_name] = file_data;
				}
			}
		}

		private void WriteChunks(IO.XmlElementStream s)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles.Count; x++)
			{
				this.mFiles[x].Write(s, false);
			}
		}
		private void WriteLocalFiles(IO.XmlElementStream s)
		{
			foreach (var kvp in this.mLocalFiles)
			{
				string file_name = kvp.Key;
				string file_data = kvp.Value;

				using (s.EnterCursorBookmark("file"))
				{
					s.WriteAttribute("name", file_name);
					s.WriteCursor(file_data);
				}
			}
		}

		public bool ReadDefinition(IO.XmlElementStream s)
		{
			this.mFiles.Clear();

			// first entry should always be the null terminated filenames table
			this.mFiles.Add(GenerateFileNamesTableEntryChunk());

			using (s.EnterCursorBookmark("Files"))
				this.ReadChunks(s);

			using (var bm = s.EnterCursorBookmarkOpt("LocalFiles")) if (bm.IsNotNull)
				this.ReadLocalFiles(s);

			this.AddVersionFile();

			// there should be at least one file destined for the ERA, excluding the filenames table
			return this.FileChunksCount != 0;
		}
		public void WriteDefinition(IO.XmlElementStream s)
		{
			using (s.EnterCursorBookmark("Files"))
				this.WriteChunks(s);

			using (var bm = s.EnterCursorBookmarkOpt("LocalFiles", this.mLocalFiles, Predicates.HasItems)) if (bm.IsNotNull)
				this.WriteLocalFiles(s);
		}
		#endregion

		#region Expand
		public void ExpandTo(IO.EndianStream blockStream, string workPath)
		{
			Contract.Requires(blockStream.IsReading);

			var eraExpander = (EraFileExpander)blockStream.Owner;

			if (eraExpander.ProgressOutput != null)
			{
				eraExpander.ProgressOutput.WriteLine("\tUnpacking files...");
			}

			for (int x = this.FileChunksFirstIndex; x < this.mFiles.Count; x++)
			{
				var file = this.mFiles[x];

				if (eraExpander.ProgressOutput != null)
				{
					eraExpander.ProgressOutput.Write("\r\t\t{0} ", file.EntryId.ToString("X16"));
				}

				this.TryUnpack(blockStream, workPath, eraExpander, file);
			}

			if (eraExpander.ProgressOutput != null)
			{
				eraExpander.ProgressOutput.Write("\r\t\t{0} \r", new string(' ', 16));
				eraExpander.ProgressOutput.WriteLine("\t\tDone");
			}

			this.mDirsThatExistForUnpacking = null;
		}

		private bool TryUnpack(IO.EndianStream blockStream, string workPath, EraFileExpander expander, EraFileEntryChunk file)
		{
			if (IsIgnoredLocalFile(file.FileName))
				return false;

			string full_path = Path.Combine(workPath, file.FileName);

			if (ResourceUtils.IsLocalScenarioFile(file.FileName))
			{
				return false;
			}
			else if (!this.ShouldUnpack(expander, full_path))
			{
				return false;
			}

			this.CreatePathForUnpacking(full_path);

			this.UnpackToDisk(blockStream, full_path, expander, file);
			return true;
		}

		private void UnpackToDisk(IO.EndianStream blockStream, string fullPath, EraFileExpander expander, EraFileEntryChunk file)
		{
			byte[] buffer = file.GetBuffer(blockStream);

			using (var fs = File.Create(fullPath))
			{
				fs.Write(buffer, 0, buffer.Length);
			}

			File.SetCreationTimeUtc(fullPath, file.FileDateTime);
			File.SetLastWriteTimeUtc(fullPath, file.FileDateTime);

			if (ResourceUtils.IsXmbFile(fullPath))
			{
				if (!expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
				{
					var va_size = Shell.ProcessorSize.x32;
					var builtFor64Bit = expander.Options.Test(EraFileUtilOptions.x64);
					if (builtFor64Bit)
					{
						va_size = Shell.ProcessorSize.x64;
					}

					this.TransformXmbToXml(buffer, fullPath, blockStream.ByteOrder, va_size);
				}
			}
			else if (ResourceUtils.IsScaleformFile(fullPath))
			{
				if (expander.ExpanderOptions.Test(EraFileExpanderOptions.DecompressUIFiles))
				{
					bool success = false;

					try
					{
						success = this.DecompressUIFileToDisk(buffer, fullPath);
					}
					catch (Exception ex)
					{
						Debug.Trace.Resource.TraceEvent(System.Diagnostics.TraceEventType.Error, TypeExtensions.kNone,
							"Exception during {0} of '{1}': {2}",
							EraFileExpanderOptions.DecompressUIFiles, fullPath, ex);
						success = false;
					}

					if (!success && expander.VerboseOutput != null)
					{
						expander.VerboseOutput.WriteLine("Option {0} failed on '{1}'",
							EraFileExpanderOptions.DecompressUIFiles, fullPath);
					}
				}
				if (expander.ExpanderOptions.Test(EraFileExpanderOptions.TranslateGfxFiles))
				{
					var result = TransformGfxToSwfFileResult.Failed;

					try
					{
						result = this.TransformGfxToSwfFile(buffer, fullPath);
					}
					catch (Exception ex)
					{
						Debug.Trace.Resource.TraceEvent(System.Diagnostics.TraceEventType.Error, TypeExtensions.kNone,
							"Exception during {0} of '{1}': {2}",
							EraFileExpanderOptions.TranslateGfxFiles, fullPath, ex);
					}

					if (expander.VerboseOutput != null)
					{
						if (result == TransformGfxToSwfFileResult.Failed)
						{
							expander.VerboseOutput.WriteLine("Option {0} failed on '{1}'",
								EraFileExpanderOptions.TranslateGfxFiles, fullPath);
						}
						else if (result == TransformGfxToSwfFileResult.InputIsAlreadySwf)
						{
							expander.VerboseOutput.WriteLine("Option {0} skipped on '{1}', it is already an SWF-based file",
								EraFileExpanderOptions.TranslateGfxFiles, fullPath);
						}
					}
				}
			}
		}

		private void TransformXmbToXml(byte[] eraFileEntryBuffer, string fullPath, Shell.EndianFormat byteOrder, Shell.ProcessorSize vaSize)
		{
			byte[] xmb_buffer;

			using (var xmb = new ECF.EcfFileXmb())
			using (var ms = new MemoryStream(eraFileEntryBuffer))
			using (var es = new IO.EndianStream(ms, byteOrder, permissions: FileAccess.Read))
			{
				es.StreamMode = FileAccess.Read;
				xmb.Serialize(es);

				xmb_buffer = xmb.FileData;
			}

			string xmb_path = fullPath;
			ResourceUtils.RemoveXmbExtension(ref xmb_path);

			var context = new Xmb.XmbFileContext()
			{
				PointerSize = vaSize,
			};

			using (var ms = new MemoryStream(xmb_buffer, false))
			using (var s = new IO.EndianReader(ms, byteOrder))
			{
				s.UserData = context;

				using (var xmbf = new Xmb.XmbFile())
				{
					xmbf.Read(s);
					xmbf.ToXml(xmb_path);
				}
			}
		}

		private bool DecompressUIFileToDisk(byte[] eraFileEntryBuffer, string fullPath)
		{
			bool success = false;

			using (var ms = new MemoryStream(eraFileEntryBuffer, false))
			using (var s = new IO.EndianReader(ms, Shell.EndianFormat.Little))
			{
				uint buffer_signature;
				if (ResourceUtils.IsScaleformBuffer(s, out buffer_signature))
				{
					int decompressed_size = s.ReadInt32();
					int compressed_size = (int)(ms.Length - ms.Position);

					byte[] decompressed_data = ResourceUtils.DecompressScaleform(eraFileEntryBuffer, decompressed_size);
					using (var fs = File.Create(fullPath + ".bin"))
					{
						fs.Write(decompressed_data, 0, decompressed_data.Length);
					}

					success = true;
				}
			}

			return success;
		}

		enum TransformGfxToSwfFileResult
		{
			Success,
			Failed,
			InputIsAlreadySwf,
		}
		private TransformGfxToSwfFileResult TransformGfxToSwfFile(byte[] eraFileEntryBuffer, string fullPath)
		{
			var result = TransformGfxToSwfFileResult.Failed;

			using (var ms = new MemoryStream(eraFileEntryBuffer, false))
			using (var s = new IO.EndianReader(ms, Shell.EndianFormat.Little))
			{
				uint buffer_signature;
				if (ResourceUtils.IsScaleformBuffer(s, out buffer_signature))
				{
					if (ResourceUtils.IsSwfHeader(buffer_signature))
						result = TransformGfxToSwfFileResult.InputIsAlreadySwf;
					else
					{
						this.TransformGfxToSwfFileInternal(eraFileEntryBuffer, fullPath, buffer_signature);
						result = TransformGfxToSwfFileResult.Success;
					}
				}
			}

			return result;
		}
		private void TransformGfxToSwfFileInternal(byte[] eraFileEntryBuffer, string fullPath, uint bufferSignature)
		{
			uint swf_signature = ResourceUtils.GfxHeaderToSwf(bufferSignature);
			using (var fs = File.Create(fullPath + ".swf"))
			using (var out_s = new IO.EndianWriter(fs, Shell.EndianFormat.Little))
			{
				out_s.Write(swf_signature);
				out_s.Write(eraFileEntryBuffer, sizeof(uint), eraFileEntryBuffer.Length - sizeof(uint));
			}
		}

		private HashSet<string> mDirsThatExistForUnpacking;
		private void CreatePathForUnpacking(string full_path)
		{
			if (this.mDirsThatExistForUnpacking == null)
				this.mDirsThatExistForUnpacking = new HashSet<string>();

			string folder = Path.GetDirectoryName(full_path);
			// don't bother checking the file system if we've already encountered this folder
			if (this.mDirsThatExistForUnpacking.Add(folder))
			{
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}
			}
		}

		private bool ShouldUnpack(EraFileExpander expander, string path)
		{
			if (expander.ExpanderOptions.Test(EraFileExpanderOptions.DontOverwriteExistingFiles))
			{
				// it's an XMB file and the user didn't say NOT to translate them
				if (ResourceUtils.IsXmbFile(path) && !expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
				{
					ResourceUtils.RemoveXmbExtension(ref path);
				}

				if (File.Exists(path))
				{
					return false;
				}
			}

			if (expander.ExpanderOptions.Test(EraFileExpanderOptions.IgnoreNonDataFiles))
			{
				if (!ResourceUtils.IsDataBasedFile(path))
					return false;
			}

			return true;
		}
		#endregion

		#region Build
		private bool BuildFileNamesTable(IO.EndianStream blockStream)
		{
			Contract.Requires(blockStream.IsWriting);

			using (var ms = new MemoryStream(this.mFiles.Count * 128))
			using (var s = new IO.EndianWriter(ms, blockStream.ByteOrder))
			{
				var smp = new Memory.Strings.StringMemoryPool(kFileNamesTablePoolConfig);
				for (int x = this.FileChunksFirstIndex; x < this.mFiles.Count; x++)
				{
					var file = this.mFiles[x];

					file.FileNameOffset = smp.Add(file.FileName).u32;
				}
				smp.WriteStrings(s);

				var filenames_chunk = this.mFiles[0];
				this.PackFileNames(blockStream, ms, filenames_chunk);

				return true;
			}
		}

		public bool Build(IO.EndianStream blockStream, string workPath)
		{
			Contract.Requires(blockStream.IsWriting);

			var builder = blockStream.Owner as EraFileBuilder;

			Contract.Assert(blockStream.BaseStream.Position == this.CalculateHeaderAndFileChunksSize());

			this.BuildFileNameMaps(builder != null ? builder.VerboseOutput : null);
			bool success = this.BuildFileNamesTable(blockStream);
			for (int x = this.FileChunksFirstIndex; x < this.mFiles.Count && success; x++)
			{
				var file = this.mFiles[x];
				if (builder != null && builder.ProgressOutput != null)
				{
					builder.ProgressOutput.Write("\r\t\t{0} ", file.EntryId.ToString("X16"));
				}

				success &= this.TryPack(blockStream, workPath, file);

				if (!success &&
					builder != null && builder.VerboseOutput != null)
				{
					builder.VerboseOutput.WriteLine("Couldn't pack file into {0}: {1}",
					                                this.FileName, file.FileName);
				}
			}

			if (builder != null && builder.ProgressOutput != null &&
				success)
			{
				builder.ProgressOutput.Write("\r\t\t{0} \r", new string(' ', 16));
			}

			if (success)
			{
				blockStream.AlignToBoundry(kAlignmentBit);
			}

#if false
			if (success)
			{
				if (builder != null && !builder.Options.Test(EraFileUtilOptions.SkipVerification))
				{
					var filenames_chunk = mFiles[0];

					ValidateAdler32(filenames_chunk, blockStream);
					ValidateHashes(filenames_chunk, blockStream);

					ValidateFileHashes(blockStream);
				}
			}
#endif

			return success;
		}

		private void PackFileData(IO.EndianStream blockStream, Stream source, EraFileEntryChunk file)
		{
			file.BuildBuffer(blockStream, source, this.TigerHasher);

#if false
			ValidateAdler32(file, blockStream);
			ValidateHashes(file, blockStream);
#endif
		}

		private void PackFileNames(IO.EndianStream blockStream, MemoryStream source, EraFileEntryChunk file)
		{
			file.FileDateTime = this.BuildModeDefaultTimestamp;
			this.PackFileData(blockStream, source, file);
		}

		private bool TryPack(IO.EndianStream blockStream, string workPath,
			EraFileEntryChunk file)
		{
			if (this.mLocalFiles.ContainsKey(file.FileName))
				return this.TryPackLocalFile(blockStream, file);

			return this.TryPackFileFromDisk(blockStream, workPath, file);
		}

		private bool TryPackLocalFile(IO.EndianStream blockStream,
			EraFileEntryChunk file)
		{
			string file_data;
			if (!this.mLocalFiles.TryGetValue(file.FileName, out file_data))
			{
				Debug.Trace.Resource.TraceInformation("Couldn't pack local-file into {0}, local-file does not exist: {1}",
				                                      this.FileName, file.FileName);
				return false;
			}

			byte[] file_bytes = System.Text.Encoding.ASCII.GetBytes(file_data);
			using (var ms = new MemoryStream(file_bytes, false))
			{
				this.PackFileData(blockStream, ms, file);
			}

			return true;
		}

		private bool TryPackFileFromDisk(IO.EndianStream blockStream, string workPath,
			EraFileEntryChunk file)
		{
			string path = Path.Combine(workPath, file.FileName);
			if (!File.Exists(path))
			{
				Debug.Trace.Resource.TraceInformation("Couldn't pack file into {0}, file does not exist: {1}",
				                                      this.FileName, file.FileName);
				return false;
			}

			try
			{
				file.FileDateTime = GetMostRecentTimeStamp(path);

				byte[] file_bytes = File.ReadAllBytes(path);
				using (var ms = new MemoryStream(file_bytes, false))
				{
					this.PackFileData(blockStream, ms, file);
				}
			} catch (Exception ex)
			{
				Debug.Trace.Resource.TraceData(System.Diagnostics.TraceEventType.Error, TypeExtensions.kNone,
					string.Format("Couldn't pack file into {0}, encountered exception dealing with {1}", this.FileName, file.FileName),
					ex);
				return false;
			}

			return true;
		}
		#endregion

		#region IEndianStreamSerializable Members
		public void ReadPostprocess(IO.EndianStream s)
		{
			if (this.mFiles.Count == 0)
			{
				return;
			}

			var expander = s.Owner as EraFileExpander;
			var progressOutput = expander != null ? expander.ProgressOutput : null;
			var verboseOutput = expander != null ? expander.VerboseOutput : null;

			this.ReadFileNamesChunk(s);
			this.ValidateFileHashes(s);

			this.BuildFileNameMaps(verboseOutput);

			if (expander != null && !expander.ExpanderOptions.Test(EraFileExpanderOptions.DontRemoveXmlOrXmbFiles))
			{
				if (expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
				{
					if (progressOutput != null)
						progressOutput.WriteLine("Removing any XML files if their XMB counterpart exists...");

					this.RemoveXmlFilesWhereXmbExists(verboseOutput);
				}
				else
				{
					if (progressOutput != null)
						progressOutput.WriteLine("Removing any XMB files if their XML counterpart exists...");

					this.RemoveXmbFilesWhereXmlExists(verboseOutput);
				}
			}

			this.BuildLocalFiles(s);
		}

		void ReadFileNamesChunk(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;

			var filenames_chunk = this.mFiles[0];

			if (eraUtil != null &&
				!eraUtil.Options.Test(EraFileUtilOptions.SkipVerification))
			{
				this.ValidateAdler32(filenames_chunk, s);
				this.ValidateHashes(filenames_chunk, s);
			}

			filenames_chunk.FileName = kFileNamesTableName;

			byte[] filenames_buffer = filenames_chunk.GetBuffer(s);
			using (var ms = new MemoryStream(filenames_buffer, false))
			using (var er = new IO.EndianReader(ms, s.ByteOrder))
			{
				for (int x = this.FileChunksFirstIndex; x < this.mFiles.Count; x++)
				{
					var file = this.mFiles[x];

					if (file.FileNameOffset != er.BaseStream.Position)
					{
						throw new InvalidDataException(string.Format(
							"#{0} {1} has bad filename offset {2} != {3}",
							this.FileIndexToListingIndex(x),
							file.EntryId.ToString("X16"),
							file.FileNameOffset.ToString("X8"),
							er.BaseStream.Position.ToString("X8")
							));
					}

					file.FileName = er.ReadString(Memory.Strings.StringStorage.CStringAscii);
				}
			}
		}

		void ValidateFileHashes(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;

			if (eraUtil != null &&
				eraUtil.Options.Test(EraFileUtilOptions.SkipVerification))
			{
				return;
			}

			if (eraUtil != null && eraUtil.ProgressOutput != null)
			{
				eraUtil.ProgressOutput.WriteLine("\tVerifying file hashes...");
			}

			for (int x = this.FileChunksFirstIndex; x < this.mFiles.Count; x++)
			{
				var file = this.mFiles[x];

				if (eraUtil != null && eraUtil.ProgressOutput != null)
				{
					eraUtil.ProgressOutput.Write("\r\t\t{0} ", file.EntryId.ToString("X16"));
				}

				this.ValidateAdler32(file, s);
				this.ValidateHashes(file, s);
			}

			if (eraUtil != null && eraUtil.ProgressOutput != null)
			{
				eraUtil.ProgressOutput.Write("\r\t\t{0} \r", new string(' ', 16));
				eraUtil.ProgressOutput.WriteLine("\t\tDone");
			}
		}

		void BuildLocalFiles(IO.EndianStream s)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles.Count; x++)
			{
				var file = this.mFiles[x];
				if (!ResourceUtils.IsLocalScenarioFile(file.FileName))
					continue;

				byte[] file_bytes = file.GetBuffer(s);
				using (var ms = new MemoryStream(file_bytes, false))
				using (var sr = new StreamReader(ms))
				{
					string file_data = sr.ReadToEnd();

					this.mLocalFiles[file.FileName] = file_data;
				}
			}
		}

	public void Serialize(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;

			if (s.IsWriting)
			{
				this.mHeader.UpdateFileCount(this.mFiles.Count);
			}

			this.mHeader.Serialize(s);

			if (eraUtil != null && eraUtil.DebugOutput != null)
			{
				eraUtil.DebugOutput.WriteLine("Header position end: {0}",
					s.BaseStream.Position);
				eraUtil.DebugOutput.WriteLine();
			}

			this.SerializeFileEntryChunks(s);

			if (eraUtil != null && eraUtil.DebugOutput != null)
			{
				eraUtil.DebugOutput.WriteLine();
			}
		}

		public void SerializeFileEntryChunks(IO.EndianStream s)
		{
			if (s.IsReading)
			{
				this.mFiles.Capacity = this.mHeader.FileCount;

				for (int x = 0; x < this.mFiles.Capacity; x++)
				{
					var file = new EraFileEntryChunk();
					file.Serialize(s);

					this.mFiles.Add(file);
				}
			}
			else if (s.IsWriting)
			{
				foreach (var f in this.mFiles)
				{
					f.Serialize(s);
				}
			}
		}
		#endregion

		#region Local file utils
		private static bool IsIgnoredLocalFile(string fileName)
		{
			if (0==string.Compare(fileName, "version.txt", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}

		private void AddVersionFile()
		{
			var file = new EraFileEntryChunk();
			file.CompressionType = ECF.EcfCompressionType.Stored;
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			file.FileName = "version.txt";
			file.FileDateTime = this.BuildModeDefaultTimestamp;
			string version = string.Format("{0}\n{1}\n{2}",
				assembly.FullName,
				assembly.GetName().Version,
				System.Reflection.Assembly.GetEntryAssembly().FullName);
			this.mLocalFiles[file.FileName] = version;
			this.mFiles.Add(file);
		}
		#endregion

		internal static DateTime GetMostRecentTimeStamp(string path)
		{
			var creation_time = File.GetCreationTimeUtc(path);
			var write_time = File.GetLastWriteTimeUtc(path);
			return write_time > creation_time
				? write_time
				: creation_time;
		}
	};
}
