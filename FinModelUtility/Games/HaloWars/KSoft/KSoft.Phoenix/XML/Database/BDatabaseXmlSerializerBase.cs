
namespace KSoft.Phoenix.XML
{
	public abstract partial class BDatabaseXmlSerializerBase
		: BXmlSerializerInterface
		, IO.ITagElementStringNameStreamable
	{
		IBListAutoIdXmlSerializer mDamageTypesSerializer
			, mImpactEffectsSerializer
			, mObjectsSerializer
			, mSquadsSerializer
			, mPowersSerializer
			, mTechsSerializer
			;

		protected BDatabaseXmlSerializerBase()
		{
		}

		#region IDisposable Members
		public override void Dispose()
		{
			this.AutoIdSerializersDispose();
		}
		#endregion

		protected virtual void AutoIdSerializersInitialize()
		{
			if (this.mDamageTypesSerializer == null)
				this.mDamageTypesSerializer = XmlUtil.CreateXmlSerializer(this.Database.DamageTypes, Phx.BDamageType.kBListXmlParams);
			if (this.mImpactEffectsSerializer == null)
				this.mImpactEffectsSerializer = XmlUtil.CreateXmlSerializer(this.Database.ImpactEffects, Phx.BProtoImpactEffect.kBListXmlParams);

			if (this.mObjectsSerializer == null)
				this.mObjectsSerializer = XmlUtil.CreateXmlSerializer(this.Database.Objects, Phx.BProtoObject.kBListXmlParams);
			if (this.mSquadsSerializer == null)
				this.mSquadsSerializer = XmlUtil.CreateXmlSerializer(this.Database.Squads, Phx.BProtoSquad.kBListXmlParams);
			if (this.mPowersSerializer == null)
				this.mPowersSerializer = XmlUtil.CreateXmlSerializer(this.Database.Powers, Phx.BProtoPower.kBListXmlParams);
			if (this.mTechsSerializer == null)
				this.mTechsSerializer = XmlUtil.CreateXmlSerializer(this.Database.Techs, Phx.BProtoTech.kBListXmlParams);
		}
		protected virtual void AutoIdSerializersDispose()
		{
			Util.DisposeAndNull(ref this.mDamageTypesSerializer);
			Util.DisposeAndNull(ref this.mImpactEffectsSerializer);

			Util.DisposeAndNull(ref this.mObjectsSerializer);
			Util.DisposeAndNull(ref this.mSquadsSerializer);
			Util.DisposeAndNull(ref this.mPowersSerializer);
			Util.DisposeAndNull(ref this.mTechsSerializer);
		}
	};
}