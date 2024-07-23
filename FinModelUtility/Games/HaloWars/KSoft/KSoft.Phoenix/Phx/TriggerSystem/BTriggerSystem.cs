using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.TriggerScript)]
	public sealed class BTriggerSystem
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public const string kXmlRootName = "TriggerSystem";

		const string kXmlAttrType = "Type";
		const string kXmlAttrNextTriggerVar = "NextTriggerVarID";
		const string kXmlAttrNextTrigger = "NextTriggerID";
		const string kXmlAttrNextCondition = "NextConditionID";
		const string kXmlAttrNextEffect = "NextEffectID";
		const string kXmlAttrExternal = "External";
		#endregion

		#region File Util
		public static string GetFileExt(BTriggerScriptType type)
		{
			switch (type)
			{
				case BTriggerScriptType.TriggerScript: return ".triggerscript";
				case BTriggerScriptType.Ability: return ".ability";
				case BTriggerScriptType.Power: return ".power";

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		public static string GetFileExtSearchPattern(BTriggerScriptType type)
		{
			switch (type)
			{
				case BTriggerScriptType.TriggerScript: return "*.triggerscript";
				case BTriggerScriptType.Ability: return "*.ability";
				case BTriggerScriptType.Power: return "*.power";

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		#endregion

		public BTriggerSystem Owner { get; private set; }

		string mName;
		public string Name { get { return mName; } }
		public override string ToString() { return mName; }

		BTriggerScriptType mType;
		int mNextTriggerVarID = TypeExtensions.kNone;
		int mNextTriggerID = TypeExtensions.kNone;
		int mNextConditionID = TypeExtensions.kNone;
		int mNextEffectID = TypeExtensions.kNone;
		bool mExternal;

		public Collections.BListAutoId<BTriggerGroup> Groups { get; private set; } = new Collections.BListAutoId<BTriggerGroup>();

		public Collections.BListAutoId<BTriggerVar> Vars { get; private set; } = new Collections.BListAutoId<BTriggerVar>();
		public Collections.BListAutoId<BTrigger> Triggers { get; private set; } = new Collections.BListAutoId<BTrigger>();

		public BTriggerEditorData EditorData { get; private set; }

		#region Database interfaces
		Dictionary<int, BTriggerGroup> mDbiGroups;
		Dictionary<int, BTriggerVar> mDbiVars;
		Dictionary<int, BTrigger> mDbiTriggers;

		static void BuildDictionary<T>(out Dictionary<int, T> dic, Collections.BListAutoId<T> list)
			where T : TriggerScriptIdObject, new()
		{
			dic = new Dictionary<int, T>(list.Count);

			foreach (var item in list)
				dic.Add(item.ID, item);
		}

		public BTriggerVar GetVar(int var_id)
		{
			BTriggerVar var;
			mDbiVars.TryGetValue(var_id, out var);

			return var;
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute(DatabaseNamedObject.kXmlAttrNameN, ref mName);
			s.StreamAttributeEnum(kXmlAttrType, ref mType);
			s.StreamAttribute(kXmlAttrNextTriggerVar, ref mNextTriggerVarID);
			s.StreamAttribute(kXmlAttrNextTrigger, ref mNextTriggerID);
			s.StreamAttribute(kXmlAttrNextCondition, ref mNextConditionID);
			s.StreamAttribute(kXmlAttrNextEffect, ref mNextEffectID);
			s.StreamAttribute(kXmlAttrExternal, ref mExternal);

			using (s.EnterUserDataBookmark(this))
			{
				XML.XmlUtil.Serialize(s, Groups, BTriggerGroup.kBListXmlParams);
				if (s.IsReading) BuildDictionary(out mDbiGroups, Groups);

				XML.XmlUtil.Serialize(s, Vars, BTriggerVar.kBListXmlParams);
				if (s.IsReading) BuildDictionary(out mDbiVars, Vars);
				XML.XmlUtil.Serialize(s, Triggers, BTrigger.kBListXmlParams);
				if (s.IsReading) BuildDictionary(out mDbiTriggers, Triggers);
			}

			if(s.IsReading)
				(xs as XML.BTriggerScriptSerializer).TriggerDb.UpdateFromGameData(this);
		}
		#endregion
	};
}
