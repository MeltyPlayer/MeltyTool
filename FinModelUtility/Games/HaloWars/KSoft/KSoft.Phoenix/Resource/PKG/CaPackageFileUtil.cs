using System;
using System.IO;

namespace KSoft.Phoenix.Resource.PKG
{
	public abstract class CaPackageFileUtil
		: IDisposable
	{
		public CaPackageFileDefinition PkgDefinition { get; private set; }
		internal CaPackageFile mPkgFile;
		protected string mSourceFile; // filename of the source file which the util stems from
		public TextWriter ProgressOutput { get; set; }
		public TextWriter VerboseOutput { get; set; }
		public TextWriter DebugOutput { get; set; }

		protected CaPackageFileUtil()
		{
			this.PkgDefinition = new CaPackageFileDefinition();

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
			this.mPkgFile = null;
		}
		#endregion
	};
}