using System;
using System.IO;

namespace KSoft.Phoenix.Resource.PKG
{
	public enum CaPackageFileExpanderOptions
	{
		/// <summary>Only the PKG's file listing (.xml) is generated</summary>
		OnlyDumpListing,
		/// <summary>Files that already exist in the output directory will be skipped</summary>
		DontOverwriteExistingFiles,
		DontLoadEntirePkgIntoMemory,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public sealed class CaPackageFileExpander
		: CaPackageFileUtil
	{
		Stream mPkgBaseStream;
		IO.EndianStream mPkgStream;

		/// <see cref="CaPackageFileExpanderOptions"/>
		public Collections.BitVector32 ExpanderOptions;

		public CaPackageFileExpander(string pkgPath)
		{
			this.mSourceFile = pkgPath;
		}

		public override void Dispose()
		{
			base.Dispose();

			Util.DisposeAndNull(ref this.mPkgBaseStream);
			Util.DisposeAndNull(ref this.mPkgStream);
		}

		#region Reading
		public bool Read()
		{
			bool result = true;

			try { result &= this.ReadPkgFromFile(); }
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while trying to read the PKG: {0}", ex);
				result = false;
			}

			return result;
		}

		bool ReadPkgFromFile()
		{
			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Opening and reading PKG file {0}...",
				                              this.mSourceFile);

			if (this.ExpanderOptions.Test(CaPackageFileExpanderOptions.DontLoadEntirePkgIntoMemory))
				this.mPkgBaseStream = File.OpenRead(this.mSourceFile);
			else
			{
				byte[] ecf_bytes = File.ReadAllBytes(this.mSourceFile);

				this.mPkgBaseStream = new MemoryStream(ecf_bytes, writable: false);
			}

			this.mPkgStream = new IO.EndianStream(this.mPkgBaseStream, Shell.EndianFormat.Little, this, permissions: FileAccess.Read);
			this.mPkgStream.StreamMode = FileAccess.Read;

			return this.ReadPkgFromStream();
		}

		bool ReadPkgFromStream()
		{
			bool result = true;

			result = CaPackageFile.VerifyIsPkg(this.mPkgStream.Reader);
			if (!result)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tFailed: File is either not even an PKG file, or corrupt");
			}
			else
			{
				this.mPkgFile = new CaPackageFile();
				this.mPkgFile.Serialize(this.mPkgStream);
			}

			return result;
		}
		#endregion

		#region Expanding
		public bool ExpandTo(string workPath, string listingName)
		{
			if (this.mPkgFile == null)
				return false;

			if (!Directory.Exists(workPath))
				Directory.CreateDirectory(workPath);

			bool result = true;

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Outputting listing...");

			try
			{
				this.PopulatePkgDefinitionFromPkgFile(workPath);
				this.SaveListing(workPath, listingName);
			}
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while outputting listing: {0}", ex);
				result = false;
			}

			if (result && !this.ExpanderOptions.Test(CaPackageFileExpanderOptions.OnlyDumpListing))
			{
				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Expanding PKG to {0}...", workPath);

				try
				{
					this.ExpandEntriesToFiles(workPath);
				}
				catch (Exception ex)
				{
					if (this.VerboseOutput != null)
						this.VerboseOutput.WriteLine("\tEncountered an error while expanding PKG: {0}", ex);
					result = false;
				}

				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Done");
			}

			this.mPkgStream.Close();

			return result;
		}

		void SaveListing(string workPath, string listingName)
		{
			string listing_filename = Path.Combine(workPath, listingName);

			using (var xml = IO.XmlElementStream.CreateForWrite("PkgFile", this))
			{
				xml.InitializeAtRootElement();
				xml.StreamMode = FileAccess.Write;

				this.PkgDefinition.Serialize(xml);

				xml.Document.Save(listing_filename + CaPackageFileDefinition.kFileExtension);
			}
		}

		void PopulatePkgDefinitionFromPkgFile(string workPath)
		{
			foreach (var entry in this.mPkgFile.FileEntries)
			{
				this.PkgDefinition.FileNames.Add(entry.Name);
			}
		}

		void ExpandEntriesToFiles(string workPath)
		{
			foreach (var entry in this.mPkgFile.FileEntries)
			{
				try
				{
					this.ExpandEntryToFile(workPath, entry);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format(
						"ExpandEntriesToFiles failed on {0} in {1}",
						entry.Name,
						this.mPkgStream.StreamName
					), e);
				}
			}
		}

		void ExpandEntryToFile(string workPath, CaPackageEntry entry)
		{
			string file_path = Path.Combine(workPath, entry.Name);

			if (!this.ExpanderOptions.Test(CaPackageFileExpanderOptions.DontOverwriteExistingFiles))
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
				var entry_bytes = this.mPkgFile.ReadEntryBytes(this.mPkgStream, entry);
				fs.Write(entry_bytes, 0, entry_bytes.Length);
			}
		}
		#endregion
	};
}

