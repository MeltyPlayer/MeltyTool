using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KSoft.Collections
{
	public interface IBList
	{
		BListParams Params { get; }

		int Count { get; }

		void Clear();

		object GetObject(int id);
		object UnderlyingObjectsCollection { get; }

		bool IsEmpty { get; }

		void Sort();
	};

	/// <summary>Our base interface for lists of comparable objects</summary>
	/// <typeparam name="T">Comparable object's type</typeparam>
	public abstract class BListBase<T>
		: IBList
		, IEqualityComparer<BListBase<T>>
		, IEnumerable<T>
	{
		#region kValueEqualityComparer
		private static IEqualityComparer<T> gValueEqualityComparer;
		protected static IEqualityComparer<T> kValueEqualityComparer { get {
			if (gValueEqualityComparer == null)
				gValueEqualityComparer = EqualityComparer<T>.Default;

			return gValueEqualityComparer;
		} }
		#endregion

		#region kEqualityComparer
		protected sealed class _EqualityComparer
			: IEqualityComparer<BListBase<T>>
		{
			#region IEqualityComparer<BListBase<T>> Members
			public bool Equals(BListBase<T> x, BListBase<T> y)
			{
				bool equals = x.Count == y.Count;
				if (equals)
				{
					var comparer = kValueEqualityComparer;
					for (int i = 0; i < x.Count && equals; i++)
						equals &= comparer.Equals(x[i], y[i]);
				}

				return equals;
			}

			public int GetHashCode(BListBase<T> obj)
			{
				int hash = 0;
				var comparer = kValueEqualityComparer;
				foreach (var o in obj)
					hash ^= comparer.GetHashCode(o);

				return hash;
			}
			#endregion
		};
		private static _EqualityComparer gEqualityComparer;
		protected static _EqualityComparer kEqualityComparer { get {
			if (gEqualityComparer == null)
				gEqualityComparer = new _EqualityComparer();

			return gEqualityComparer;
		} }
		#endregion

		protected ObservableCollection<T> mList;
		protected List<T> RawList { get {
			var list = ObjectModel.Util.GetUnderlyingItemsAsList(this.mList);
			return list;
		} }

		/// <summary>Parameters that dictate the functionality of this list</summary>
		public BListParams Params { get; private set; }

		protected BListBase(int capacity = BCollectionParams.kDefaultCapacity)
		{
			this.mList = new ObservableCollection<T>(/*capacity*/);
			this.Capacity = capacity;
		}
		protected BListBase(BListParams @params)
			: this(@params != null ? @params.InitialCapacity : BCollectionParams.kDefaultCapacity)
		{
			this.Params = @params;
		}

		#region List interface
		public int Count { get { return this.mList.Count; } }

		internal int Capacity
		{
			get { return this.RawList.Capacity; }
			set { this.RawList.Capacity = value; }
		}

		public virtual T this[int index]
		{
			get { return this.mList[index]; }
			set { this.mList[index] = value; }
		}

		internal void AddItem(T item)
		{
			this.mList.Add(item);
		}

		public virtual void Clear()
		{
			if (this.mList != null)
				this.mList.Clear();
		}

		#region IEnumerable<T> Members
		public List<T>.Enumerator GetEnumerator()
		{
			return this.RawList.GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return this.mList.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.mList.GetEnumerator();
		}
		#endregion
		#endregion

		public virtual object GetObject(int id)
		{
			return this[id];
		}

		object IBList.UnderlyingObjectsCollection { get { return this.mList; } }

		public bool IsEmpty { get { return this.Count == 0; } }
		internal void OptimizeStorage()
		{
			//if (Count == 0)
			//	mList = null;
			this.RawList.TrimExcess();
		}

		#region IEqualityComparer<BListBase<T>> Members
		public bool Equals(BListBase<T> x, BListBase<T> y)
		{
			return kEqualityComparer.Equals(x, y);
		}

		public int GetHashCode(BListBase<T> obj)
		{
			return kEqualityComparer.GetHashCode(obj);
		}
		#endregion

		public void Sort()
		{
			this.mList.Sort();
		}
		public void Sort(IComparer<T> comparer)
		{
			this.mList.Sort(comparer);
		}
		public void Sort(int index, int count, IComparer<T> comparer)
		{
			this.RawList.Sort(index, count, comparer);
		}
		public void Sort(Comparison<T> comparison)
		{
			this.mList.Sort(comparison);
		}
	};
}