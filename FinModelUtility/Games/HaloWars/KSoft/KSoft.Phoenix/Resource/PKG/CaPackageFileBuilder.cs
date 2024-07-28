using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource.PKG
{
	public enum CaPackageFileBuilderOptions
	{
		AlwaysUseXmlOverXmb,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public sealed class CaPackageFileBuilder
		: CaPackageFileUtil
	{
		/// <see cref="CaPackageFileBuilderOptions"/>
		public Collections.BitVector32 BuilderOptions;

		public CaPackageFileBuilder(string listingPath)
		{
			if (Path.GetExtension(listingPath) != CaPackageFileDefinition.kFileExtension)
				listingPath += CaPackageFileDefinition.kFileExtension;

			this.mSourceFile = listingPath;
		}

		#region Reading
		bool ReadInternal()
		{
			bool result = true;

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Trying to read source listing {0}...", this.mSourceFile);

			if (!File.Exists(this.mSourceFile))
				result = false;
			else
			{
				this.mPkgFile = new CaPackageFile();

				using (var xml = new IO.XmlElementStream(this.mSourceFile, FileAccess.Read, this))
				{
					xml.InitializeAtRootElement();
					this.PkgDefinition.Serialize(xml);
				}
			}

			if (result == false)
			{
				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("\tFailed!");
			}

			return result;
		}
		public bool Read() // read the listing definition
		{
			bool result = true;

			try { result &= this.ReadInternal(); }
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while trying to read listing: {0}", ex);
				result = false;
			}

			return result;
		}

	#endregion
  };
}

