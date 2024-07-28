using System.ComponentModel;

namespace KSoft.Collections
{
	public abstract class BListAutoIdObject
		: ObjectModel.BasicViewModel
		, IListAutoIdObject
	{
		private string mName = Phoenix.Phx.BDatabaseBase.kInvalidString;
		[Browsable(false)]
		public string Name
		{
			get { return this.mName; }
			protected set
			{
				if (this.SetFieldObj(ref this.mName, value))
				{
					this.OnPropertyChanged(nameof(IListAutoIdObject.Data));
				}
			}
		}

		#region IListAutoIdObject Members
		private int mAutoId;
		public int AutoId
		{
			get { return this.mAutoId; }
			set { this.SetFieldVal(ref this.mAutoId, value); }
		}

		string IListAutoIdObject.Data
		{
			get { return this.mName; }
			set { this.Name = value; }
		}

		public abstract void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		#endregion

		public override string ToString() { return this.mName; }
	};
}
