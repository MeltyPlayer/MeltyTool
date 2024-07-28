using System;
using System.Collections.Generic;
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
			Collections.BTypeValues<T> list, BTypeValuesXmlParams<T> @params)
			where T : IEqualityComparer<T>, IO.ITagElementStringNameStreamable, new()
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			using (var xs = new BTypeValuesXmlSerializer<T>(@params, list))
			{
				xs.Serialize(s);
			}
		}
	};

	internal class BTypeValuesXmlSerializer<T>
		: BTypeValuesXmlSerializerBase<T>
		where T : IEqualityComparer<T>, IO.ITagElementStringNameStreamable, new()
	{
		public BTypeValuesXmlSerializer(BTypeValuesXmlParams<T> @params, Collections.BTypeValues<T> list) : base(@params, list)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			int index = this.ReadExplicitIndex(s, xs);

			this.ListExplicitIndex.InitializeItem(index);
			T data = new T();
			data.Serialize(s);
			this.ListExplicitIndex[index] = data;
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data)
		{
			data.Serialize(s);
		}
		#endregion
	};
}