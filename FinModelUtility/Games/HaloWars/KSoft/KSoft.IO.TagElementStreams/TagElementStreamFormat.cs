namespace KSoft.IO
{
	/// <remarks>If the <see cref="TagElementStreamFormat.Binary"/> flag is not set, assume 'Text'</remarks>
	public enum TagElementStreamFormat
	{
		Undefined,

		Xml,
		/// <summary>Currently unsupported</summary>
		Json,
		/// <summary>Currently unsupported</summary>
		Yaml,

		kCustomStart,
		kCustomEnd = 1<<6,
		kCustomMax = kCustomEnd - kCustomStart,

		Binary = 1<<7,

		kTypeFlags = Binary,

		/// <summary>Currently unsupported</summary>
		Bson = Binary | Json,
	};
}
