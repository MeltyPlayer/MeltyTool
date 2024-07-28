using System.ComponentModel;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	public abstract class DatabaseNamedObject
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		internal const string kXmlAttrName = "name";
		internal const string kXmlAttrNameN = "Name";
		#endregion

		#region UserInterfaceTextData
		[Browsable(false)]
		public DatabaseObjectUserInterfaceTextData UserInterfaceTextData { get; private set; }

		protected DatabaseObjectUserInterfaceTextData CreateDatabaseObjectUserInterfaceTextData()
		{
			Contract.Requires(this.UserInterfaceTextData == null);

			this.UserInterfaceTextData = new DatabaseObjectUserInterfaceTextData();
			return this.UserInterfaceTextData;
		}
		#endregion

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			if (this.UserInterfaceTextData != null)
				this.UserInterfaceTextData.Serialize(s);
		}
		#endregion
	};
}