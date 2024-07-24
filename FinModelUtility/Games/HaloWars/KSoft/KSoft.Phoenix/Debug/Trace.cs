using Diag = System.Diagnostics;

namespace KSoft.Phoenix.Debug
{
	/// <summary>Utility class for tracing assembly logic (or lack thereof)</summary>
	internal static class Trace
	{
		static Diag.TraceSource kPhoenixSource
			, kEngineSource
			, kResourceSource
			, kSecuritySource
			, kTriggerSystemSource
			, kXmlSource
			;

		static Trace()
		{
			kPhoenixSource = new		Diag.TraceSource("KSoft.Phoenix",			Diag.SourceLevels.All);
			kEngineSource = new			Diag.TraceSource("KSoft.Phoenix.Engine",	Diag.SourceLevels.All);
			kResourceSource = new		Diag.TraceSource("KSoft.Phoenix.Resource",	Diag.SourceLevels.All);
			kSecuritySource = new		Diag.TraceSource("KSoft.Security",			Diag.SourceLevels.All);
			kTriggerSystemSource = new	Diag.TraceSource("KSoft.Phoenix.Triggers",	Diag.SourceLevels.All);
			kXmlSource = new			Diag.TraceSource("KSoft.Phoenix.XML",		Diag.SourceLevels.All);
		}

		/// <summary>Tracer for the <see cref="KSoft.Phoenix"/> namespace</summary>
		public static Diag.TraceSource Phoenix		{ get { return kPhoenixSource; } }
		/// <summary>Tracer for the <see cref="KSoft.Phoenix.Engine"/> namespace</summary>
		public static Diag.TraceSource Engine		{ get { return kEngineSource; } }
		/// <summary>Tracer for the <see cref="KSoft.Phoenix.Resource"/> namespace</summary>
		public static Diag.TraceSource Resource		{ get { return kResourceSource; } }
		/// <summary>Tracer for the <see cref="KSoft.Security"/> namespace</summary>
		public static Diag.TraceSource Security		{ get { return kSecuritySource; } }
		/// <summary>Tracer for the Trigger System related code</summary>
		public static Diag.TraceSource TriggerSystem{ get { return kTriggerSystemSource; } }
		/// <summary>Tracer for the <see cref="KSoft.Phoenix.XML"/> namespace</summary>
		public static Diag.TraceSource XML			{ get { return kXmlSource; } }
	};
}
