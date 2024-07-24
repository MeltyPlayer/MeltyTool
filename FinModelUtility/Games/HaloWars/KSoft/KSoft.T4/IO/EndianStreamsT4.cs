using System.Collections.Generic;

namespace KSoft.T4
{
	public static class EndianStreamsT4
	{
		public static IEnumerable<string> ClassNames { get {
			yield return "EndianReader";
			yield return "EndianWriter";
		} }
	};
}
