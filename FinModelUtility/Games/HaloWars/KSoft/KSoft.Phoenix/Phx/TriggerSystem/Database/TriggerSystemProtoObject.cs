#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	public abstract class TriggerSystemProtoObject
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		const string kXmlAttrDbId = "DBID";
		const string kXmlAttrVersion = "Version";
		#endregion

		int mDbId = TypeExtensions.kNone;
		public int DbId { get { return this.mDbId; } }

		int mVersion = TypeExtensions.kNone;
		public int Version { get { return this.mVersion; } }

		public Collections.BListExplicitIndex<BTriggerParam> Params { get; private set; }

		protected TriggerSystemProtoObject()
		{
			this.Params = new Collections.BListExplicitIndex<BTriggerParam>(BTriggerParam.kBListExplicitIndexParams);
		}
		protected TriggerSystemProtoObject(BTriggerSystem root, TriggerScriptObjectWithArgs instance)
		{
			this.Name = instance.Name;

			this.mDbId = instance.DbId;
			this.mVersion = instance.Version;
			this.Params = BTriggerParam.BuildDefinition(root, instance.Args);
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute(kXmlAttrDbId, ref this.mDbId);
			s.StreamAttribute(kXmlAttrVersion, ref this.mVersion);

			XML.XmlUtil.Serialize(s, this.Params, BTriggerParam.kBListExplicitIndexXmlParams);
		}

		static bool ContainsUserClassTypeVar(BTriggerSystem ts, TriggerScriptObjectWithArgs obj)
		{
			foreach (var arg in obj.Args)
			{
				if (arg.IsInvalid)
					continue;
				if (arg.GetVarType(ts) == BTriggerVarType.UserClassType)
					return true;
			}
			return false;
		}
		public virtual int CompareTo(BTriggerSystem ts, TriggerScriptObjectWithArgs obj)
		{
			if (this.Name != obj.Name)
				Debug.Trace.Engine.TraceInformation(
					"TriggerProtoDbObject: '{0}' - Encountered different names for {1}, '{2}' != '{3}'",
					ts, this.DbId.ToString(), this.Name, obj.Name);

			if (ContainsUserClassTypeVar(ts, obj))
			{
				Debug.Trace.Engine.TraceInformation(
					"TriggerProtoDbObject: {0} - Encountered {1}/{2} which has a UserClassType Var, skipping comparison",
					ts,
					this.DbId.ToString(),
					this.Name);
				return 0;
			}

			Contract.Assert(this.Version == obj.Version);
			Contract.Assert(this.Params.Count == obj.Args.Count);

			int diff = 0;
			for (int x = 0; x < this.Params.Count; x++)
			{
				int sig = this.Params[x].SigID;
				int obj_sig = obj.Args[x].SigID;
				sig = sig < 0 ? 0 : sig;
				obj_sig = obj_sig < 0 ? 0 : obj_sig;

				diff += sig - obj_sig;
			}

			return diff;
		}
	};
}