using System;
using System.ComponentModel;

namespace KSoft.ObjectModel
{
	public abstract class BasicViewModel
		: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(
			[System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
		{
			var handler = this.PropertyChanged;
			if (handler != null)
				handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetFieldVal<T>(ref T field, T value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
			where T : struct, IEquatable<T>
		{
			return TypeExtensions.SetFieldVal(this,
			                                  this.PropertyChanged,
				ref field, value, overrideChecks, propertyName);
		}

		protected bool SetFieldEnum<TEnum>(ref TEnum field, TEnum value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return TypeExtensions.SetFieldEnum(this,
			                                   this.PropertyChanged,
				ref field, value, overrideChecks, propertyName);
		}

		protected bool SetFieldObj<T>(ref T field, T value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
			where T : class, IEquatable<T>
		{
			return TypeExtensions.SetFieldObj(this,
			                                  this.PropertyChanged,
				ref field, value, overrideChecks, propertyName);
		}

		protected bool SetField<T>(ref T field, T value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
		{
			return TypeExtensions.SetField(this,
			                               this.PropertyChanged,
				ref field, value, overrideChecks, propertyName);
		}
	};
}