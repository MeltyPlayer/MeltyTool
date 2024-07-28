using System;
using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Phoenix.Phx
{
	public enum ProtoDataObjectSourceKind
	{
		None = PhxUtil.kObjectKindNone,

		Database,
		GameData,
		HPData,

		Scenario,
		TacticData,
		TriggerScript,
		Visual,
	};

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class ProtoDataTypeObjectSourceKindAttribute
		: Attribute
	{
		public ProtoDataObjectSourceKind SourceKind { get; private set; }

		public ProtoDataTypeObjectSourceKindAttribute(ProtoDataObjectSourceKind kind)
		{
			this.SourceKind = kind;
		}
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		[Contracts.Pure]
		public static bool RequiresFileReference(this Phx.ProtoDataObjectSourceKind kind)
		{
			switch (kind)
			{
				case Phx.ProtoDataObjectSourceKind.TacticData:
				case Phx.ProtoDataObjectSourceKind.Visual:
				case Phx.ProtoDataObjectSourceKind.TriggerScript:
				case Phx.ProtoDataObjectSourceKind.Scenario:
					return true;

				default:
					return false;
			}
		}
	};
}