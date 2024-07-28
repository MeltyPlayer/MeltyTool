using System.Collections.Generic;
using System.Threading.Tasks;

namespace KSoft.Phoenix.Phx
{
	public sealed class TriggerDatabase
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public const string kXmlRootName = "TriggerDatabase";
		#endregion

		public Collections.BListAutoId<BTriggerProtoCondition> Conditions { get; private set; } = new Collections.BListAutoId<BTriggerProtoCondition>();
		public Collections.BListAutoId<BTriggerProtoEffect> Effects { get; private set; } = new Collections.BListAutoId<BTriggerProtoEffect>();
		public Dictionary<uint, TriggerSystemProtoObject> LookupTable { get; private set; } = new Dictionary<uint, TriggerSystemProtoObject>();
		System.Collections.BitArray mUsedIds = new System.Collections.BitArray(1088);

		#region ITagElementStreamable<string> Members
		static int SortById(TriggerSystemProtoObject x, TriggerSystemProtoObject y)
		{
			if(x.DbId != y.DbId)
				return x.DbId - y.DbId;

			return x.Version - y.Version;
		}
		int WriteUnknowns<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			int count = 0;
			for (int x = 1; x < mUsedIds.Length; x++)
			{
				if (!mUsedIds[x])
				{
					s.WriteElement("Unknown", x);
					count++;
				}
			}
			return count;
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsWriting)
			{
				var task_sort_cond = Task.Factory.StartNew(() => Conditions.Sort(SortById));
				var task_sort_effe = Task.Factory.StartNew(() => Effects.Sort(SortById));

				var task_unknowns = Task<int>.Factory.StartNew(() =>
				{
					using (s.EnterCursorBookmark("Unknowns"))
						return WriteUnknowns(s);
				});
				s.WriteAttribute("UnknownCount", task_unknowns.Result);
				s.WriteAttribute("ConditionsCount", Conditions.Count);
				s.WriteAttribute("EffectsCount", Effects.Count);

				Task.WaitAll(task_sort_cond, task_sort_effe);
			}

			XML.XmlUtil.Serialize(s, Conditions, BTriggerProtoCondition.kBListXmlParams);
			XML.XmlUtil.Serialize(s, Effects, BTriggerProtoEffect.kBListXmlParams);

			if (s.IsReading)
			{
				foreach (var c in Conditions) LookupTableAdd(c);
				foreach (var e in Effects) LookupTableAdd(e);
			}
		}
		#endregion

		static uint GenerateHandle(TriggerSystemProtoObject dbo)
		{
			return ((uint)dbo.DbId << 8) | (uint)dbo.Version;
		}
		static uint GenerateHandle(TriggerScriptObject dbo)
		{
			return ((uint)dbo.DbId << 8) | (uint)dbo.Version;
		}

	void LookupTableAdd(TriggerSystemProtoObject dbo)
		{
			mUsedIds[dbo.DbId] = true;
			LookupTable.Add(GenerateHandle(dbo), dbo);
		}
		bool LookupTableContains<T>(T obj, out TriggerSystemProtoObject dbo)
			where T : TriggerScriptObject
		{
			return LookupTable.TryGetValue(GenerateHandle(obj), out dbo);
		}

		static void TraceUpdate(BTriggerSystem ts, TriggerSystemProtoObject dbo)
		{
			Debug.Trace.Engine.TraceInformation(
				"TriggerProtoDbObject: {0} - Updated {1}/{2}",
				ts, dbo.DbId.ToString(), dbo.Name);
		}
		void TryUpdate(BTriggerSystem ts, BTriggerCondition cond)
		{
			TriggerSystemProtoObject dbo;
			if (!LookupTableContains(cond, out dbo))
			{
				var dbo_cond = new BTriggerProtoCondition(ts, cond);

				Conditions.DynamicAdd(dbo_cond, dbo_cond.Name);
				LookupTableAdd(dbo_cond);
			}
			else
			{
				int diff = dbo.CompareTo(ts, cond);
				if (diff < 0)
				{
					var dbo_cond = new BTriggerProtoCondition(ts, cond);
					LookupTable[GenerateHandle(cond)] = dbo_cond;
					TraceUpdate(ts, dbo_cond);
				}
			}
		}
		void TryUpdate(BTriggerSystem ts, BTriggerEffect effe)
		{
			TriggerSystemProtoObject dbo;
			if (!LookupTableContains(effe, out dbo))
			{
				var dbo_effe = new BTriggerProtoEffect(ts, effe);

				Effects.DynamicAdd(dbo_effe, dbo_effe.Name);
				LookupTableAdd(dbo_effe);
			}
			else
			{
				int diff = dbo.CompareTo(ts, effe);
				if (diff < 0)
				{
					var dbo_effe = new BTriggerProtoEffect(ts, effe);
					LookupTable[GenerateHandle(effe)] = dbo_effe;
					TraceUpdate(ts, dbo_effe);
				}
			}
		}
		internal void UpdateFromGameData(BTriggerSystem ts)
		{
			lock (LookupTable)
			{
				foreach (var t in ts.Triggers)
				{
					foreach (var c in t.Conditions) TryUpdate(ts, c);
					foreach (var e in t.EffectsOnTrue) TryUpdate(ts, e);
					foreach (var e in t.EffectsOnFalse) TryUpdate(ts, e);
				}
			}
		}
		public void Save(string path, BDatabaseBase db)
		{
			using (var s = KSoft.IO.XmlElementStream.CreateForWrite(kXmlRootName))
			{
				s.InitializeAtRootElement();
				s.StreamMode = System.IO.FileAccess.Write;
				s.SetSerializerInterface(XML.BXmlSerializerInterface.GetNullInterface(db));
				Serialize(s);

				s.Document.Save(path);
			}
		}
	};
}
