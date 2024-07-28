using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Exprs = System.Linq.Expressions;

namespace KSoft.ObjectModel
{
	public class PropertyChangedEventArgsCollection : IEnumerable<System.ComponentModel.PropertyChangedEventArgs>
	{
		List<System.ComponentModel.PropertyChangedEventArgs> mEventArgs;

		public PropertyChangedEventArgsCollection()
		{
			this.mEventArgs = new List<System.ComponentModel.PropertyChangedEventArgs>();
		}
		PropertyChangedEventArgsCollection(IEnumerable<System.ComponentModel.PropertyChangedEventArgs> eventArgs)
		{
			this.mEventArgs = new List<System.ComponentModel.PropertyChangedEventArgs>(eventArgs);
		}

		public PropertyChangedEventArgsCollection CreateArgs<T, TProp>(
			out System.ComponentModel.PropertyChangedEventArgs eventArgs,
			Exprs.Expression<Func<T, TProp>> propertyExpr)
		{
			eventArgs = Util.CreatePropertyChangedEventArgs(propertyExpr);
			this.mEventArgs.Add(eventArgs);

			return this;
		}

		public PropertyChangedEventArgsCollection Branch()
		{
			Contract.Ensures(Contract.Result<PropertyChangedEventArgsCollection>() != this);

			return new PropertyChangedEventArgsCollection(this.mEventArgs);
		}

		public void NotifyPropertiesChanged(object sender, System.ComponentModel.PropertyChangedEventHandler handler)
		{
			if (handler != null)
				foreach (var args in this.mEventArgs)
					handler(sender, args);
		}

		#region IEnumerable<PropertyChangedEventArgs> Members
		public IEnumerator<System.ComponentModel.PropertyChangedEventArgs> GetEnumerator()
		{
			return this.mEventArgs.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.mEventArgs.GetEnumerator();
		}
		#endregion
	};
}