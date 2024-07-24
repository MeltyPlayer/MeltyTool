using Diag = System.Diagnostics;

namespace KSoft.Wwise.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	internal static class Trace
	{
		static Diag.TraceSource kWwiseSource
			, kFilePackageSource
			;

		static Trace()
		{
			kWwiseSource = new			Diag.TraceSource("KSoft.Wwise",				Diag.SourceLevels.All);
			kFilePackageSource = new	Diag.TraceSource("KSoft.Wwise.FilePackage",	Diag.SourceLevels.All);
		}

		/// <summary>Tracer for the <see cref="KSoft.Wwise"/> namespace</summary>
		public static Diag.TraceSource Wwise		{ get { return kWwiseSource; } }
		/// <summary>Tracer for the <see cref="KSoft.Wwise.FilePackage"/> namespace</summary>
		public static Diag.TraceSource FilePackage	{ get { return kFilePackageSource; } }
	};
}
