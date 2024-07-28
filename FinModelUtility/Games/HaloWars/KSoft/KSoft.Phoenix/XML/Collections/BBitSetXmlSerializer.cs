using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		[ThreadStatic]
		private static BBitSetXmlSerializer gBitSetXmlSerializer;

		public static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BBitSet bits, BBitSetXmlParams @params)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(bits != null);
			Contract.Requires(@params != null);
			Contract.Requires(@params.UseElementName || @params.ElementItselfMeansTrue,
				"Collection only supports element name filtering");

			if (gBitSetXmlSerializer == null)
				gBitSetXmlSerializer = new BBitSetXmlSerializer();
			var xs = gBitSetXmlSerializer;

			using (xs.Reset(@params, bits))
			{
				xs.Serialize(s);
			}
		}
	};
	internal sealed class BBitSetXmlSerializer
		: IDisposable
		, IO.ITagElementStringNameStreamable
	{
		public BBitSetXmlParams Params { get; private set; }
		public Collections.BBitSet Bits { get; private set; }

		internal BBitSetXmlSerializer()
		{
		}

		internal BBitSetXmlSerializer Reset(BBitSetXmlParams @params, Collections.BBitSet bits)
		{
			this.Params = @params;
			this.Bits = bits;

			return this;
		}

		#region ITagElementStringNameStreamable Members
		Collections.IProtoEnum GetProtoEnum(Phx.BDatabaseBase db)
		{
			if (this.Bits.Params.kGetProtoEnum != null)
				return this.Bits.Params.kGetProtoEnum();

			return this.Bits.Params.kGetProtoEnumFromDB(db);
		}

		void ReadNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();
			Collections.IProtoEnum penum = this.Bits.InitializeFromEnum(xs.Database);

			if (this.Params.ElementItselfMeansTrue)
			{
				var getDefault = this.Bits.Params.kGetMemberDefaultValue;
				foreach (var e in s.Elements)
				{
					var element_name = s.GetElementName(e);
					int id = penum.TryGetMemberId(element_name);
					if (id.IsNone())
						continue;

					bool flag = true;
					s.StreamElementOpt(element_name, ref flag);

					if (getDefault != null && flag != getDefault(id))
					{
						// do nothing, allow the Set call below
					}
					else if (!flag)
						continue;

					this.Bits.Set(id, flag);
				}
			}
			else
			{
				foreach (var n in s.ElementsByName(this.Params.ElementName))
				{
					using (s.EnterCursorBookmark(n))
					{
						string name = null;
						this.Params.StreamDataName(s, ref name);

						int id = penum.GetMemberId(name);
						this.Bits.Set(id);
					}
				}
			}

			this.Bits.OptimizeStorage();
		}
		void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (this.Bits.EnabledCount == 0)
				return;

			var xs = s.GetSerializerInterface();
			Collections.IProtoEnum penum = this.GetProtoEnum(xs.Database);

			if (this.Bits.Params.kGetMemberDefaultValue != null)
			{
				Contract.Assert(this.Params.ElementItselfMeansTrue);
				this.WriteNodesNotEqualToDefaultValues(s, penum);
				return;
			}

			foreach (var bitIndex in this.Bits.RawBits.SetBitIndices)
			{
				string name = penum.GetMemberName(bitIndex);

				if (this.Params.ElementItselfMeansTrue)
				{
					using (s.EnterCursorBookmark(name))
					{
						// do nothing
					}
				}
				else
				{
					using (s.EnterCursorBookmark(this.Params.ElementName))
					{
						this.Params.StreamDataName(s, ref name);
					}
				}
			}
		}
		void WriteNodesNotEqualToDefaultValues<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, Collections.IProtoEnum penum)
			where TDoc : class
			where TCursor : class
		{
			var getDefault = this.Bits.Params.kGetMemberDefaultValue;
			for (int x = 0; x < penum.MemberCount; x++)
			{
				bool bitDefault = getDefault(x);
				if (bitDefault == this.Bits[x])
					continue;

				string name = penum.GetMemberName(x);
				using (s.EnterCursorBookmark(name))
				{
					bool writtenValue = !bitDefault;
					s.WriteCursor(writtenValue);
				}
			}
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			// #NOTE we don't check the book mark for null here because the root element is optional
			using (s.EnterCursorBookmarkOpt(this.Params.GetOptionalRootName()))
			{
					 if (s.IsReading)
						 this.ReadNodes(s);
				else if (s.IsWriting)
					this.WriteNodes(s);
			}
		}
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			this.Params = null;
			this.Bits = null;
		}
		#endregion
	};
}