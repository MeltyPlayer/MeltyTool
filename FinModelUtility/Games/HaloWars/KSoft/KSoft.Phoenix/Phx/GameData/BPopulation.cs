using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public struct BPopulation
		: IO.ITagElementStringNameStreamable
		, IComparable<BPopulation>
		, IEqualityComparer<BPopulation>
	{
		sealed class _EqualityComparer : IEqualityComparer<BPopulation>
		{
			#region IEqualityComparer<BPopulation> Members
			public bool Equals(BPopulation x, BPopulation y)
			{
				return x.Max == y.Max && x.Count == y.Count;
			}

			public int GetHashCode(BPopulation obj)
			{
				return obj.Max.GetHashCode() ^ obj.Count.GetHashCode();
			}
			#endregion
		};
		private static _EqualityComparer gEqualityComparer;
		public static IEqualityComparer<BPopulation> EqualityComparer { get {
			if (gEqualityComparer == null)
				gEqualityComparer = new _EqualityComparer();

			return gEqualityComparer;
		} }

		#region Xml constants
		public static readonly Collections.BTypeValuesParams<BPopulation> kBListParams = new
			Collections.BTypeValuesParams<BPopulation>(db => db.GameData.Populations)
			{
				kTypeGetInvalid = () => kInvalid
			};
		public static readonly XML.BTypeValuesXmlParams<BPopulation> kBListXmlParams = new
			XML.BTypeValuesXmlParams<BPopulation>("Pop", "Type");

		public static readonly Collections.BTypeValuesParams<float> kBListParamsSingle = new
			Collections.BTypeValuesParams<float>(db => db.GameData.Populations)
			{
				kTypeGetInvalid = PhxUtil.kGetInvalidSingle
			};
		public static readonly XML.BTypeValuesXmlParams<float> kBListXmlParamsSingle = new
			XML.BTypeValuesXmlParams<float>("Pop", "Type");
		public static readonly XML.BTypeValuesXmlParams<float> kBListXmlParamsSingle_LowerCase = new
			XML.BTypeValuesXmlParams<float>("Pop", "Type".ToLowerInvariant());
		public static readonly XML.BTypeValuesXmlParams<float> kBListXmlParamsSingle_CapAddition = new
			XML.BTypeValuesXmlParams<float>("PopCapAddition", "Type");
		#endregion

		private static BPopulation kInvalid { get { return new BPopulation(PhxUtil.kInvalidSingle, PhxUtil.kInvalidSingle); } }

		float mMax;
		public float Max { get { return this.mMax; } }

		float mCount;
		public float Count { get { return this.mCount; } }

		BPopulation(float max, float count) {
			this.mMax = max;
			this.mCount = count; }

		#region IComparable<T> Members
		int IComparable<BPopulation>.CompareTo(BPopulation other)
		{
			if (this.Max == other.Max)
				return this.Count.CompareTo(other.Count);
			else
				return this.Max.CompareTo(other.Max);
		}
		#endregion

		#region IEqualityComparer<BPopulation> Members
		public bool Equals(BPopulation x, BPopulation y)
		{
			return EqualityComparer.Equals(x, y);
		}

		public int GetHashCode(BPopulation obj)
		{
			return EqualityComparer.GetHashCode(obj);
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("Max", ref this.mMax);
			s.StreamCursor(ref this.mCount);
		}
		#endregion
	};
}