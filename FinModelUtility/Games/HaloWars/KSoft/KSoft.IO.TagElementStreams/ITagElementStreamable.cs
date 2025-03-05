using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	[Contracts.ContractClass(typeof(ITagElementStreamableContract<>))]
	public interface ITagElementStreamable<TName>
	{
		void Serialize<TDoc, TCursor>(TagElementStream<TDoc, TCursor, TName> s)
			where TDoc : class
			where TCursor : class;
	};
	[Contracts.ContractClassFor(typeof(ITagElementStreamable<>))]
	abstract class ITagElementStreamableContract<TName> : ITagElementStreamable<TName>
	{
		public void Serialize<TDoc, TCursor>(TagElementStream<TDoc, TCursor, TName> s)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
		}
	};


	public interface ITagElementStringNameStreamable : ITagElementStreamable<string>;
}