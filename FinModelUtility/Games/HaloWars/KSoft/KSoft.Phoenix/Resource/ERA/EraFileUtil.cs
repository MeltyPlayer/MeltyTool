using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource
{
	using CryptographyTransformType = Security.Cryptography.CryptographyTransformType;

	public enum EraFileUtilOptions
	{
		DumpDebugInfo,
		SkipVerification,
		/// <summary>Built for 64-bit builds</summary>
		x64,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public abstract class EraFileUtil
		: IDisposable
	{
		// The Cryptography pipeline assumes encrypted ERA files have the ".era" extension, while decrypted
		// variants have an ".bin" extension. The Crypt() API wants a naked filename (not dir or extension data).

		/// <summary>DO NOT USE UNLESS YOU ARE KSoft.Phoenix</summary>
		public const string kExtensionEncrypted = EraFile.kExtensionEncrypted;
		/// <summary>DO NOT USE UNLESS YOU ARE KSoft.Phoenix</summary>
		public const string kExtensionDecrypted = EraFile.kExtensionDecrypted;

		/*protected*/ internal EraFile mEraFile;
		protected string mSourceFile; // filename of the source file which the util stems from (.era, .xml)
		public TextWriter ProgressOutput { get; set; }
		public TextWriter VerboseOutput { get; set; }
		public TextWriter DebugOutput { get; set; }

		/// <see cref="EraFileUtilOptions"/>
		public Collections.BitVector32 Options = new Collections.BitVector32();

		protected EraFileUtil()
		{
			if (System.Diagnostics.Debugger.IsAttached)
				this.ProgressOutput = Console.Out;
			if (System.Diagnostics.Debugger.IsAttached)
				this.VerboseOutput = Console.Out;
		}

		#region IDisposable Members
		public virtual void Dispose()
		{
			this.ProgressOutput = null;
			this.VerboseOutput = null;
			this.DebugOutput = null;
			Util.DisposeAndNull(ref this.mEraFile);
		}
		#endregion

		/// <summary>Performs a Cryptography operation on a given ERA file</summary>
		/// <param name="path">Path to the input ERA file</param>
		/// <param name="eraName">The naked ERA file, void of directory or extension data</param>
		/// <param name="outputPath">Path to </param>
		/// <param name="transformType">Type of Cryptography operation to perform</param>
		/// <param name="verboseOutput">(Optional) the object to write verbose operation output to</param>
		/// <returns>The output file's full path</returns>
		/// <exception cref="FileNotFoundException">Input ERA file does not exist</exception>
		public static string Crypt(string path, string eraName, string outputPath, CryptographyTransformType transformType,
			TextWriter verboseOutput = null)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
				outputPath = path;

			string input_file = Path.Combine(path, eraName) + EraFile.kExtensionEncrypted;
			string output_file = Path.Combine(outputPath, eraName) + EraFile.kExtensionEncrypted;

			// If we're encrypting, the input file will be a .bin, else the output file will be a .bin
			switch(transformType)
			{
			case CryptographyTransformType.Decrypt:
				output_file += EraFile.kExtensionDecrypted;
				break;

			case CryptographyTransformType.Encrypt:
				input_file += EraFile.kExtensionDecrypted;
				break;

			default:
				throw new KSoft.Debug.UnreachableException(transformType.ToString());
			}

			if (!File.Exists(input_file))
				throw new FileNotFoundException("ERA file for cryptography operation does not exist", input_file);

			if (verboseOutput != null)
			{
				verboseOutput.WriteLine("Input:  {0}", input_file);
				verboseOutput.WriteLine("Output: {0}", output_file);
			}

			using (var er_fs = File.OpenRead(input_file))
			using (var er = new IO.EndianReader(er_fs, Shell.EndianFormat.Big))
			using (var ew_fs = new FileStream(output_file, FileMode.Create, FileAccess.Write))
			using (var ew = new IO.EndianWriter(ew_fs, Shell.EndianFormat.Big))
			{
				CryptStream(er, ew, transformType);
			}

			return output_file;
		}

		public static void CryptStream(IO.EndianReader input, IO.EndianWriter output, CryptographyTransformType transformType)
		{
			Contract.Requires(input != null);
			Contract.Requires(output != null);
			// This should be OK because PhxTEA is buffered
			//Contract.Requires(input.BaseStream != output.BaseStream);

			var tea = new Security.Cryptography.PhxTEA(input, output);
			tea.InitializeKey(Security.Cryptography.PhxTEA.kKeyEra);

			switch (transformType)
			{
			case CryptographyTransformType.Decrypt:
				tea.Decrypt();
				break;

			case CryptographyTransformType.Encrypt:
				tea.Encrypt();
				break;
			}
		}
	};
}