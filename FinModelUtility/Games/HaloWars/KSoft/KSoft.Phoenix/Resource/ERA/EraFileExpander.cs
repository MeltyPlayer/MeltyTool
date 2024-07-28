using System;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource
{
	public enum EraFileExpanderOptions
	{
		/// <summary>Only the ERA's file listing (.xml) is generated</summary>
		OnlyDumpListing,
		/// <summary>Files that already exist in the output directory will be skipped</summary>
		DontOverwriteExistingFiles,
		/// <summary>Don't perform XMB to XML translations</summary>
		DontTranslateXmbFiles,
		/// <summary>Decompresses Scaleform data</summary>
		DecompressUIFiles,
		/// <summary>Translates GFX files to SWF</summary>
		TranslateGfxFiles,
		Decrypt,
		DontLoadEntireEraIntoMemory,
		DontRemoveXmlOrXmbFiles,
		IgnoreNonDataFiles,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public sealed class EraFileExpander
		: EraFileUtil
	{
		public const string kNameExtension = ".era.bin";

		System.IO.Stream mEraBaseStream;
		IO.EndianStream mEraStream;

		/// <see cref="EraFileExpanderOptions"/>
		public Collections.BitVector32 ExpanderOptions;

		public EraFileExpander(string eraPath)
		{
			this.mSourceFile = eraPath;
		}

		public override void Dispose()
		{
			base.Dispose();

			Util.DisposeAndNull(ref this.mEraStream);
			Util.DisposeAndNull(ref this.mEraBaseStream);
		}

		bool ReadEraFromStream()
		{
			bool result = true;

			result = EraFileHeader.VerifyIsEraAndDecrypted(this.mEraStream.Reader);
			if (!result)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tFailed: File is either not decrypted, corrupt, or not even an ERA");
			}
			else
			{
				this.mEraStream.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);

				this.mEraFile = new EraFile();
				this.mEraFile.FileName = this.mSourceFile;
				this.mEraFile.Serialize(this.mEraStream);
				this.mEraFile.ReadPostprocess(this.mEraStream);
			}

			return result;
		}

		bool ReadEraFromFile()
		{
			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Opening and reading ERA file {0}...",
				                              this.mSourceFile);

			if (this.ExpanderOptions.Test(EraFileExpanderOptions.DontLoadEntireEraIntoMemory))
				this.mEraBaseStream = System.IO.File.OpenRead(this.mSourceFile);
			else
			{
				byte[] era_bytes = System.IO.File.ReadAllBytes(this.mSourceFile);
				if (this.ExpanderOptions.Test(EraFileExpanderOptions.Decrypt))
				{
					if (this.ProgressOutput != null)
						this.ProgressOutput.WriteLine("Decrypting...");

					this.DecryptFileBytes(era_bytes);
				}

				this.mEraBaseStream = new System.IO.MemoryStream(era_bytes, writable: false);
			}

			this.mEraStream = new IO.EndianStream(this.mEraBaseStream, Shell.EndianFormat.Big, this, permissions: FA.Read);
			this.mEraStream.StreamMode = FA.Read;

			return this.ReadEraFromStream();
		}

		void DecryptFileBytes(byte[] eraBytes)
		{
			using (var era_in_ms = new System.IO.MemoryStream(eraBytes, writable: false))
			using (var era_out_ms = new System.IO.MemoryStream(eraBytes, writable: true))
			using (var era_reader = new IO.EndianReader(era_in_ms, Shell.EndianFormat.Big))
			using (var era_writer = new IO.EndianWriter(era_out_ms, Shell.EndianFormat.Big))
			{
				// "Halo Wars Alpha 093106 Feb 21 2009" was released pre-decrypted, so try and detect if the file is already decrypted first
				if (!EraFileHeader.VerifyIsEraAndDecrypted(era_reader))
				{
					CryptStream(era_reader, era_writer,
						Security.Cryptography.CryptographyTransformType.Decrypt);
				}
			}
		}

		public bool Read()
		{
			bool result = true;

			try { result &= this.ReadEraFromFile(); }
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while trying to read the ERA: {0}", ex);
				result = false;
			}

			return result;
		}

		void SaveListing(string workPath, string listingName)
		{
			string listing_filename = System.IO.Path.Combine(workPath, listingName);

			using (var xml = IO.XmlElementStream.CreateForWrite("EraArchive", this))
			{
				xml.InitializeAtRootElement();
				xml.StreamMode = FA.Write;

				this.mEraFile.WriteDefinition(xml);

				xml.Document.Save(listing_filename + EraFileBuilder.kNameExtension);
			}
		}
		public bool ExpandTo(string workPath, string listingName)
		{
			if (this.mEraFile == null)
				return false;

			if (!System.IO.Directory.Exists(workPath))
				System.IO.Directory.CreateDirectory(workPath);

			bool result = true;

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Outputting listing...");

			try {
				this.SaveListing(workPath, listingName); }
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while outputting listing: {0}", ex);
				result = false;
			}

			if (result && !this.ExpanderOptions.Test(EraFileExpanderOptions.OnlyDumpListing))
			{
				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Expanding archive to {0}...", workPath);

				try {
					this.mEraFile.ExpandTo(this.mEraStream, workPath); }
				catch (Exception ex)
				{
					if (this.VerboseOutput != null)
						this.VerboseOutput.WriteLine("\tEncountered an error while expanding archive: {0}", ex);
					result = false;
				}

				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Done");
			}

			this.mEraStream.Close();

			return result;
		}
	};
}