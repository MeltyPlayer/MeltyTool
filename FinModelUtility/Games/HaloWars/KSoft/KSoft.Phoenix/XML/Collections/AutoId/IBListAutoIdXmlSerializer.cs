using System;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.XML
{
	[Contracts.ContractClass(typeof(IBListAutoIdXmlSerializerContract))]
	public interface IBListAutoIdXmlSerializer
		: IDisposable
		, IO.ITagElementStringNameStreamable
	{
		BListXmlParams Params { get; }

		void StreamPreload<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		void StreamUpdate<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
	};

	[Contracts.ContractClassFor(typeof(IBListAutoIdXmlSerializer))]
	abstract class IBListAutoIdXmlSerializerContract : IBListAutoIdXmlSerializer
	{
		#region IBListAutoIdXmlSerializer Members
		public abstract BListXmlParams Params { get; }

		public void StreamXmlPreload<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(this.Params.RequiresDataNamePreloading);

			throw new NotImplementedException();
		}

		public void StreamXmlUpdate<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(this.Params.SupportsUpdating);

			throw new NotImplementedException();
		}
		#endregion

		#region IDisposable Members
		public abstract void Dispose();
		#endregion

		#region ITagElementStreamable<string> Members
		public abstract void StreamPreload<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		public abstract void StreamUpdate<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;

		public abstract void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		#endregion
	};
}