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
		public static void Serialize<T, TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BListExplicitIndex<T> list, BListExplicitIndexXmlParams<T> @params)
			where T : IO.ITagElementStringNameStreamable, new()
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			using(var xs = new BListExplicitIndexXmlSerializer<T>(@params, list))
			{
				xs.Serialize(s);
			}
		}
	};

	internal class BListExplicitIndexXmlSerializer<T>
		: BListExplicitIndexXmlSerializerBase<T>
		where T : IO.ITagElementStringNameStreamable, new()
	{
		Collections.BListExplicitIndex<T> mList;

		public override Collections.BListExplicitIndexBase<T> ListExplicitIndex { get { return this.mList; } }

		public BListExplicitIndexXmlSerializer(BListExplicitIndexXmlParams<T> @params, Collections.BListExplicitIndex<T> list) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);

			this.mList = list;
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			int index = this.ReadExplicitIndex(s, xs);
			Contract.Assert(index.IsNotNone());

			this.mList.InitializeItem(index);
			T data = new T();
			data.Serialize(s);
			this.mList[index] = data;
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data)
		{
			data.Serialize(s);
		}
		#endregion
	};
}