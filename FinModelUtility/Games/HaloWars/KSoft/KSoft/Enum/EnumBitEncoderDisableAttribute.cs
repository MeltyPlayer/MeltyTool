using System;

namespace KSoft
{
	/// <summary>
	/// Apply to enumerations which are invalid with the <see cref="EnumBitEncoderBase">EnumBitEncoder</see> classes
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum, AllowMultiple=false)]
	public sealed class EnumBitEncoderDisableAttribute : Attribute	{};
}
